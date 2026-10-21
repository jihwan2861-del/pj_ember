using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace EmberPrototype
{
    [AddComponentMenu("Ember/Storybook Narration")]
    [DefaultExecutionOrder(-900)]
    [DisallowMultipleComponent]
    public sealed class StorybookNarration : MonoBehaviour
    {
        [Header("Presentation")]
        [SerializeField] private CanvasGroup panel;
        [SerializeField] private Text narrationText;
        [SerializeField, TextArea(2, 5)] private string[] lines = { "앰버는 산을 오르기 시작했습니다." };
        [Header("Playback")]
        [Tooltip("이 나레이션이 재생되는 동안만 플레이어를 멈춥니다. 자동 종료 시 해제됩니다.")]
        [SerializeField] private bool lockPlayerControls;
        [SerializeField] private bool typewriter = true;
        [SerializeField, Min(1f)] private float charactersPerSecond = 24f;
        [SerializeField] private bool autoAdvance = true;
        [Tooltip("글자가 전부 나타난 뒤 다음 문장 또는 자동 종료까지 기다리는 시간입니다. 배속과 무관한 실제 초입니다.")]
        [SerializeField, Min(0f)] private float lineHoldSeconds = 2f;
        [SerializeField] private Text continueHint;
        [Tooltip("비워두면 같은 씬의 플레이어를 찾습니다.")]
        [SerializeField] private FlamePlayerController player;
        [SerializeField] private UnityEvent onStarted = new UnityEvent();
        [SerializeField] private UnityEvent onFinished = new UnityEvent();

        private static StorybookNarration active;
        private static int consumedFrame = -1;
        private int lineIndex;
        private int shownFrame;
        private FlamePlayerController lockedPlayer;
        private NarrationTextReveal reveal;
        private Text revealTarget;
        private string[] activeLines;
        private string[] activeLineSpeakers;
        private Sprite[] activeLinePortraits;
        private bool activeLockPlayerControls;
        private bool activeIsDialogue;
        private bool activeAutoAdvance;
        private string activeDefaultSpeaker;
        private Sprite activeDefaultPortrait;
        private int totalCharacters;
        private float revealedCharacters;
        private float holdRemaining;

        private RectTransform dialogueRoot;
        private CanvasGroup dialogueGroup;
        private RectTransform portraitFrame;
        private Image portraitImage;
        private Text speakerLabel;
        private Text dialogueText;
        private Text dialogueHint;

        public bool IsPlaying { get; private set; }
        public bool IsTyping { get; private set; }
        public bool IsDialoguePlaying => IsPlaying && activeIsDialogue;
        // Also consume the closing Z, even after the panel and player lock are released.
        public static bool BlocksGameplayInput => consumedFrame == Time.frameCount
            || (active != null && active.IsPlaying && active.activeLockPlayerControls);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            active = null;
            consumedFrame = -1;
        }

        private void Awake()
        {
            SetVisible(false);
            if (continueHint == null)
                foreach (var label in GetComponentsInChildren<Text>(true))
                    if (label.name == "AdvanceHint" || label.name == "ContinueHint") { continueHint = label; break; }
        }

        public void BeginNarration() => BeginNarrationWithLines(lines, lockPlayerControls);

        /// <summary>Plays cue-specific lines through this panel without changing its default lines.</summary>
        public bool BeginNarrationWithLines(string[] cueLines, bool lockControls)
        {
            return BeginPlayback(cueLines, lockControls, false, autoAdvance, null, null, null, null);
        }

        /// <summary>Plays a manually advanced NPC conversation in the shared dialogue panel.</summary>
        public bool BeginDialogueWithLines(
            string[] cueLines,
            bool lockControls,
            string defaultSpeaker,
            Sprite defaultPortrait,
            string[] lineSpeakers,
            Sprite[] linePortraits)
        {
            return BeginPlayback(cueLines, lockControls, true, false, defaultSpeaker, defaultPortrait, lineSpeakers, linePortraits);
        }

        private bool BeginPlayback(
            string[] cueLines,
            bool lockControls,
            bool dialogue,
            bool shouldAutoAdvance,
            string defaultSpeaker,
            Sprite defaultPortrait,
            string[] lineSpeakers,
            Sprite[] linePortraits)
        {
            if (!isActiveAndEnabled || IsPlaying) return false;
            if (active != null && active != this && active.IsPlaying)
            {
                Debug.LogWarning("다른 나레이션이 재생 중입니다.", this);
                return false;
            }

            string[] selectedLines = cueLines != null && cueLines.Length > 0 ? cueLines : lines;
            if (panel == null || narrationText == null || selectedLines == null || selectedLines.Length == 0)
            {
                Debug.LogWarning("나레이션 Panel, Narration Text, Lines를 설정해주세요.", this);
                return false;
            }
            if (dialogue && !EnsureDialogueView())
            {
                Debug.LogWarning("NPC 대화창을 만들 Canvas를 찾을 수 없습니다.", this);
                return false;
            }

            active = this;
            IsPlaying = true;
            lineIndex = 0;
            shownFrame = Time.frameCount;
            activeLines = selectedLines;
            activeLineSpeakers = lineSpeakers;
            activeLinePortraits = linePortraits;
            activeLockPlayerControls = lockControls;
            activeIsDialogue = dialogue;
            activeAutoAdvance = shouldAutoAdvance;
            activeDefaultSpeaker = defaultSpeaker;
            activeDefaultPortrait = defaultPortrait;
            if (activeLockPlayerControls)
            {
                consumedFrame = Time.frameCount;
                if (player == null)
                    foreach (var candidate in FindObjectsByType<FlamePlayerController>())
                        if (candidate.gameObject.scene == gameObject.scene) { player = candidate; break; }
                // Do not release a lock that another system already owns.
                if (player != null && !player.ControlsLocked)
                {
                    player.SetControlsLocked(true);
                    if (player.ControlsLocked) lockedPlayer = player;
                }
            }
            SetVisible(true);
            ShowLine();
            onStarted.Invoke();
            return true;
        }

        private void Update()
        {
            if (!IsPlaying || GameSessionControls.IsPaused || SceneRestartWipe.IsPlaying || SceneFadeTransition.IsPlaying
                || Time.frameCount == shownFrame) return;
            if (Keyboard.current != null && Keyboard.current.zKey.wasPressedThisFrame)
            {
                NextLine();
                return;
            }
            if (IsTyping)
            {
                revealedCharacters += Mathf.Max(1f, charactersPerSecond) * Time.unscaledDeltaTime;
                if (reveal != null) reveal.SetVisibleCharacters(Mathf.FloorToInt(revealedCharacters));
                if (revealedCharacters >= totalCharacters) CompleteTyping();
                return;
            }
            if (activeAutoAdvance)
            {
                holdRemaining -= Time.unscaledDeltaTime;
                if (holdRemaining <= 0f) AdvanceLine();
            }
        }

        public void NextLine()
        {
            if (!IsPlaying || GameSessionControls.IsPaused || SceneRestartWipe.IsPlaying || SceneFadeTransition.IsPlaying
                || Time.frameCount == shownFrame) return;
            consumedFrame = Time.frameCount;
            shownFrame = Time.frameCount;
            if (IsTyping) CompleteTyping();
            else AdvanceLine();
        }

        private void AdvanceLine()
        {
            shownFrame = Time.frameCount;
            if (++lineIndex >= activeLines.Length) FinishNarration();
            else ShowLine();
        }

        public void FinishNarration()
        {
            if (!IsPlaying) return;
            CancelNarration();
            onFinished.Invoke();
        }

        public void CancelNarration()
        {
            if (IsPlaying && activeLockPlayerControls) consumedFrame = Time.frameCount;
            IsPlaying = false;
            IsTyping = false;
            revealedCharacters = holdRemaining = 0f;
            if (reveal != null) reveal.ShowAll();
            if (continueHint != null) continueHint.enabled = false;
            if (dialogueHint != null) dialogueHint.enabled = false;
            if (active == this) active = null;
            SetVisible(false);
            if (lockedPlayer != null) lockedPlayer.SetControlsLocked(false);
            lockedPlayer = null;
            activeLines = null;
            activeLineSpeakers = null;
            activeLinePortraits = null;
            activeLockPlayerControls = false;
            activeIsDialogue = false;
            activeAutoAdvance = false;
            activeDefaultSpeaker = null;
            activeDefaultPortrait = null;
        }

        private void OnDisable() => CancelNarration();

        private void ShowLine()
        {
            shownFrame = Time.frameCount;
            holdRemaining = Mathf.Max(0f, lineHoldSeconds);
            revealedCharacters = 0f;
            totalCharacters = 0;

            Text target = activeIsDialogue ? dialogueText : narrationText;
            if (target == null) return;
            if (activeIsDialogue) UpdateDialogueSpeaker();
            target.text = activeLines[lineIndex] ?? string.Empty;
            EnsureReveal(target);
            reveal.enabled = true;
            if (typewriter && target.text.Length > 0)
            {
                reveal.BeginReveal();
                // One layout pass per line: measure the actual generated glyphs, including rich text.
                Canvas.ForceUpdateCanvases();
                totalCharacters = reveal.TotalCharacters;
                IsTyping = totalCharacters > 0;
                if (!IsTyping) CompleteTyping();
            }
            else CompleteTyping();
            RefreshHint();
        }

        private void EnsureReveal(Text target)
        {
            if (revealTarget == target && reveal != null) return;
            if (reveal != null) reveal.ShowAll();
            revealTarget = target;
            reveal = target.GetComponent<NarrationTextReveal>();
            if (reveal == null) reveal = target.gameObject.AddComponent<NarrationTextReveal>();
        }

        private void CompleteTyping()
        {
            IsTyping = false;
            revealedCharacters = totalCharacters;
            holdRemaining = Mathf.Max(0f, lineHoldSeconds);
            if (reveal != null) reveal.ShowAll();
            RefreshHint();
        }

        private void RefreshHint()
        {
            bool ready = IsPlaying && !IsTyping;
            if (continueHint != null) continueHint.enabled = ready && !activeIsDialogue && !activeAutoAdvance;
            if (dialogueHint != null)
            {
                dialogueHint.text = lineIndex >= activeLines.Length - 1 ? "Z   닫기" : "Z   계속";
                dialogueHint.enabled = ready && activeIsDialogue;
            }
        }

        private void UpdateDialogueSpeaker()
        {
            string speaker = activeDefaultSpeaker;
            if (activeLineSpeakers != null && lineIndex < activeLineSpeakers.Length
                && !string.IsNullOrWhiteSpace(activeLineSpeakers[lineIndex]))
                speaker = activeLineSpeakers[lineIndex];
            speakerLabel.text = string.IsNullOrWhiteSpace(speaker) ? " " : speaker;

            Sprite portrait = activeDefaultPortrait;
            if (activeLinePortraits != null && lineIndex < activeLinePortraits.Length)
                portrait = activeLinePortraits[lineIndex];

            portraitImage.sprite = portrait;
            bool hasPortrait = portrait != null;
            portraitFrame.gameObject.SetActive(hasPortrait);
            float contentLeft = hasPortrait ? 0.205f : 0.07f;
            RectTransform speakerRect = speakerLabel.rectTransform;
            RectTransform lineRect = dialogueText.rectTransform;
            speakerRect.anchorMin = new Vector2(contentLeft, speakerRect.anchorMin.y);
            lineRect.anchorMin = new Vector2(contentLeft, lineRect.anchorMin.y);
        }

        private bool EnsureDialogueView()
        {
            if (dialogueRoot != null) return true;
            Canvas sourceCanvas = narrationText != null ? narrationText.canvas : null;
            if (sourceCanvas == null) sourceCanvas = GetComponentInParent<Canvas>();
            if (sourceCanvas == null) return false;
            Canvas canvas = sourceCanvas.rootCanvas;

            GameObject rootObject = new GameObject("NPC Dialogue (Runtime)", typeof(RectTransform), typeof(CanvasGroup));
            dialogueRoot = rootObject.GetComponent<RectTransform>();
            dialogueRoot.SetParent(canvas.transform, false);
            dialogueRoot.anchorMin = Vector2.zero;
            dialogueRoot.anchorMax = Vector2.one;
            dialogueRoot.offsetMin = Vector2.zero;
            dialogueRoot.offsetMax = Vector2.zero;
            dialogueRoot.SetAsLastSibling();
            dialogueGroup = rootObject.GetComponent<CanvasGroup>();
            dialogueGroup.alpha = 0f;
            dialogueGroup.interactable = false;
            dialogueGroup.blocksRaycasts = false;

            RectTransform frame = CreateRect("DialogueFrame", dialogueRoot, new Vector2(0.055f, 0.045f), new Vector2(0.945f, 0.285f));
            Image frameImage = frame.gameObject.AddComponent<Image>();
            frameImage.color = new Color(0.045f, 0.055f, 0.072f, 0.96f);
            frameImage.raycastTarget = false;
            Outline frameOutline = frame.gameObject.AddComponent<Outline>();
            frameOutline.effectColor = new Color(0.91f, 0.76f, 0.48f, 0.96f);
            frameOutline.effectDistance = new Vector2(2.5f, -2.5f);
            frameOutline.useGraphicAlpha = true;

            portraitFrame = CreateRect("PortraitFrame", frame, new Vector2(0.025f, 0.12f), new Vector2(0.175f, 0.88f));
            Image portraitBacking = portraitFrame.gameObject.AddComponent<Image>();
            portraitBacking.color = new Color(0.12f, 0.105f, 0.09f, 1f);
            portraitBacking.raycastTarget = false;
            Outline portraitOutline = portraitFrame.gameObject.AddComponent<Outline>();
            portraitOutline.effectColor = new Color(0.89f, 0.61f, 0.27f, 1f);
            portraitOutline.effectDistance = new Vector2(2f, -2f);
            portraitImage = CreateImage("Portrait", portraitFrame, new Vector2(0.025f, 0.025f), new Vector2(0.975f, 0.975f));
            portraitImage.preserveAspect = true;

            speakerLabel = CreateText("Speaker", frame, new Vector2(0.205f, 0.69f), new Vector2(0.95f, 0.91f), 38,
                new Color(1f, 0.73f, 0.31f, 1f), TextAnchor.MiddleLeft, FontStyle.Bold);
            dialogueText = CreateText("Line", frame, new Vector2(0.205f, 0.18f), new Vector2(0.95f, 0.68f), 43,
                new Color(0.96f, 0.94f, 0.87f, 1f), TextAnchor.UpperLeft, FontStyle.Normal);
            dialogueHint = CreateText("ContinueHint", frame, new Vector2(0.73f, 0.025f), new Vector2(0.96f, 0.20f), 26,
                new Color(1f, 0.78f, 0.42f, 1f), TextAnchor.MiddleRight, FontStyle.Bold);
            dialogueHint.enabled = false;
            return true;
        }

        private static RectTransform CreateRect(string name, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            var child = new GameObject(name, typeof(RectTransform));
            var rect = child.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        private static Image CreateImage(string name, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            RectTransform rect = CreateRect(name, parent, anchorMin, anchorMax);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = Color.white;
            image.raycastTarget = false;
            return image;
        }

        private Text CreateText(string name, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax,
            int fontSize, Color color, TextAnchor alignment, FontStyle style)
        {
            RectTransform rect = CreateRect(name, parent, anchorMin, anchorMax);
            Text text = rect.gameObject.AddComponent<Text>();
            text.font = narrationText.font;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.supportRichText = false;
            text.raycastTarget = false;
            return text;
        }

        private void SetVisible(bool visible)
        {
            if (panel != null)
            {
                panel.alpha = visible && !activeIsDialogue ? 1f : 0f;
                panel.interactable = false;
                panel.blocksRaycasts = false;
            }
            if (dialogueGroup != null)
            {
                dialogueGroup.alpha = visible && activeIsDialogue ? 1f : 0f;
                dialogueGroup.interactable = false;
                dialogueGroup.blocksRaycasts = false;
            }
        }
    }
}
