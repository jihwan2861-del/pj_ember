using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace EmberPrototype
{
    [AddComponentMenu("Ember Prototype/Trial Altar")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class TrialAltar : MonoBehaviour
    {
        [Header("Ceremony")]
        [SerializeField] private bool unlocked = true;
        [Tooltip("When enabled, the player can press C to replay this ceremony after completing it. Completion events fire again each time.")]
        [SerializeField] private bool allowRepeatInteraction;
        [SerializeField] private Transform cameraFocus;
        [SerializeField] private CelesteRoomCamera roomCamera;
        [SerializeField, Range(0.1f, 1f)] private float cameraZoomMultiplier = 0.82f;
        [Tooltip("Screen width of the camera focus during the ceremony (0 = left, 1 = right). 0.5 keeps it centered.")]
        [SerializeField, Range(0f, 1f)] private float cameraFocusScreenX = 0.5f;
        [Tooltip("Screen height of the camera focus during the ceremony (0 = bottom, 1 = top). Leave Camera Focus empty to frame the player.")]
        [SerializeField, Range(0f, 1f)] private float cameraFocusScreenY = 0.28f;
        [SerializeField, Min(0f)] private float cameraZoomSeconds = 0.5f;
        [SerializeField, Min(0f)] private float cameraReturnSeconds = 0.45f;
        [SerializeField, Min(0f)] private float laurelIgnitionSeconds = 1.4f;

        [Header("Presentation")]
        [Tooltip("Assign the lit overlay sprites in the order they should ignite.")]
        [SerializeField] private SpriteRenderer[] laurelLeafLights;
        [SerializeField] private Color unlitLeafColor = new Color(1f, 1f, 1f, 0f);
        [SerializeField] private Color litLeafColor = Color.white;
        [SerializeField] private GameObject interactionPrompt;
        [Tooltip("An optional story panel that stays open until the player presses a movement key to leave the ceremony.")]
        [SerializeField] private GameObject storyPanel;
        [SerializeField, Min(0f)] private float panelInputDelay = 0.25f;

        [Header("Events")]
        [SerializeField] private UnityEvent onCeremonyStarted = new UnityEvent();
        [SerializeField] private UnityEvent onLaurelIgnited = new UnityEvent();
        [SerializeField] private UnityEvent onCeremonyCompleted = new UnityEvent();

        private FlamePlayerController playerInZone;
        private FlamePlayerController activePlayer;
        private CelesteRoomCamera activeCamera;
        private bool isRunning;
        private bool isCompleted;

        public bool IsCompleted => isCompleted;

        private void Awake()
        {
            GetComponent<BoxCollider2D>().isTrigger = true;
            if (interactionPrompt != null) interactionPrompt.SetActive(false);
            if (storyPanel != null) storyPanel.SetActive(false);
            SetAllLeaves(unlitLeafColor);
        }

        private void Reset()
        {
            GetComponent<BoxCollider2D>().isTrigger = true;
        }

        private void OnValidate()
        {
            cameraZoomMultiplier = Mathf.Clamp(cameraZoomMultiplier, 0.1f, 1f);
            cameraFocusScreenX = Mathf.Clamp01(cameraFocusScreenX);
            cameraFocusScreenY = Mathf.Clamp01(cameraFocusScreenY);
            cameraZoomSeconds = Mathf.Max(0f, cameraZoomSeconds);
            cameraReturnSeconds = Mathf.Max(0f, cameraReturnSeconds);
            laurelIgnitionSeconds = Mathf.Max(0f, laurelIgnitionSeconds);
            panelInputDelay = Mathf.Max(0f, panelInputDelay);
        }

        private void Update()
        {
            UpdatePrompt();
        }

        private void OnDisable()
        {
            CancelCeremony();
            if (playerInZone != null) playerInZone.ClearNearbyTrialAltar(this);
            playerInZone = null;
            if (interactionPrompt != null) interactionPrompt.SetActive(false);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            RegisterPlayer(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (playerInZone == null) RegisterPlayer(other);
        }

        private void RegisterPlayer(Collider2D other)
        {
            FlamePlayerController player = other.GetComponentInParent<FlamePlayerController>();
            if (player == null) return;

            playerInZone = player;
            player.SetNearbyTrialAltar(this);
            UpdatePrompt();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            FlamePlayerController player = other.GetComponentInParent<FlamePlayerController>();
            if (player == null || player != playerInZone) return;

            if (isRunning && player == activePlayer) CancelCeremony();
            player.ClearNearbyTrialAltar(this);
            playerInZone = null;
            UpdatePrompt();
        }

        public bool TryBeginCeremony(FlamePlayerController player)
        {
            if (!isActiveAndEnabled || !unlocked || isRunning || (isCompleted && !allowRepeatInteraction)
                || player == null || player != playerInZone || !player.CanStartTrialCeremony)
                return false;

            StartCoroutine(RunCeremony(player));
            return true;
        }

        public void SetUnlocked(bool value)
        {
            unlocked = value;
            UpdatePrompt();
        }

        public void ResetCeremony()
        {
            CancelCeremony();
            isCompleted = false;
            SetAllLeaves(unlitLeafColor);
            UpdatePrompt();
        }

        public void CancelCeremonyFor(FlamePlayerController player)
        {
            if (activePlayer == player) CancelCeremony();
            if (playerInZone != player) return;

            playerInZone = null;
            player.ClearNearbyTrialAltar(this);
            UpdatePrompt();
        }

        private IEnumerator RunCeremony(FlamePlayerController player)
        {
            isRunning = true;
            activePlayer = player;
            SetAllLeaves(unlitLeafColor);
            UpdatePrompt();
            player.SetControlsLocked(true);

            activeCamera = roomCamera != null
                ? roomCamera
                : Camera.main != null ? Camera.main.GetComponent<CelesteRoomCamera>() : null;
            if (activeCamera != null)
                activeCamera.BeginCinematic(cameraFocus != null ? cameraFocus : player.transform,
                    cameraZoomMultiplier, cameraZoomSeconds,
                    cameraFocusScreenY, cameraFocusScreenX);

            onCeremonyStarted.Invoke();
            yield return IgniteLaurel();
            onLaurelIgnited.Invoke();

            if (storyPanel != null)
            {
                storyPanel.SetActive(true);
                if (panelInputDelay > 0f) yield return new WaitForSeconds(panelInputDelay);
            }

            // Stay on the completed ceremony until the player intentionally moves again.
            while (!WasMovementPressedThisFrame()) yield return null;
            if (storyPanel != null) storyPanel.SetActive(false);
            if (activeCamera != null) activeCamera.EndCinematic(cameraReturnSeconds);
            player.SetControlsLocked(false);
            activePlayer = null;
            activeCamera = null;
            isRunning = false;
            isCompleted = true;
            onCeremonyCompleted.Invoke();
        }

        private static bool WasMovementPressedThisFrame()
        {
            Keyboard keyboard = Keyboard.current;
            return keyboard != null && (keyboard.leftArrowKey.wasPressedThisFrame
                || keyboard.rightArrowKey.wasPressedThisFrame
                || keyboard.upArrowKey.wasPressedThisFrame
                || keyboard.downArrowKey.wasPressedThisFrame
                || keyboard.zKey.wasPressedThisFrame);
        }

        private IEnumerator IgniteLaurel()
        {
            int count = laurelLeafLights != null ? laurelLeafLights.Length : 0;
            if (laurelIgnitionSeconds <= 0f)
            {
                SetAllLeaves(litLeafColor);
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < laurelIgnitionSeconds)
            {
                float leafProgress = elapsed / laurelIgnitionSeconds * count;
                for (int i = 0; i < count; i++)
                {
                    if (laurelLeafLights[i] != null)
                        laurelLeafLights[i].color = Color.Lerp(
                            unlitLeafColor, litLeafColor, Mathf.Clamp01(leafProgress - i));
                }
                elapsed += Time.deltaTime;
                yield return null;
            }
            SetAllLeaves(litLeafColor);
        }

        private void SetAllLeaves(Color color)
        {
            if (laurelLeafLights == null) return;
            foreach (SpriteRenderer leaf in laurelLeafLights)
            {
                if (leaf != null) leaf.color = color;
            }
        }

        private void UpdatePrompt()
        {
            if (interactionPrompt == null) return;
            bool show = unlocked && !isRunning && (!isCompleted || allowRepeatInteraction)
                && playerInZone != null && playerInZone.CanStartTrialCeremony;
            if (interactionPrompt.activeSelf != show) interactionPrompt.SetActive(show);
        }

        private void CancelCeremony()
        {
            if (!isRunning) return;
            StopAllCoroutines();
            if (storyPanel != null) storyPanel.SetActive(false);
            if (activeCamera != null) activeCamera.EndCinematic(0f);
            if (activePlayer != null) activePlayer.SetControlsLocked(false);
            activePlayer = null;
            activeCamera = null;
            isRunning = false;
            SetAllLeaves(isCompleted ? litLeafColor : unlitLeafColor);
            UpdatePrompt();
        }
    }
}
