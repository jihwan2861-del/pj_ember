using UnityEngine;
using UnityEngine.Events;

namespace EmberPrototype
{
    [DefaultExecutionOrder(200)]
    [DisallowMultipleComponent]
    [AddComponentMenu("Ember/Ignition Dash Tutorial")]
    public sealed class IgnitionDashTutorial : MonoBehaviour
    {
        public enum TutorialPhase { Idle, Armed, EnterRing, Dash, Land, Completed }

        [Header("Exercise")]
        [SerializeField] private FlamePlayerController player;
        [SerializeField] private GameObject exerciseRoot;
        [SerializeField] private bool hideExerciseUntilBegin = true;
        [SerializeField] private Transform startPoint;
        [SerializeField, Min(0f)] private float startDistance = 1.5f;
        [SerializeField] private BoxCollider2D launchArea;
        [SerializeField] private BoxCollider2D landingArea;
        [SerializeField] private Vector2 requiredDirection = Vector2.right;
        [SerializeField, Range(-1f, 1f)] private float directionDot = 0.7f;

        [Header("Existing Cue UI")]
        [SerializeField] private Canvas cueCanvas;
        [SerializeField] private UnityEngine.UI.Text instruction;
        [SerializeField] private UnityEngine.UI.Text keyLabel;
        [SerializeField] private GameObject keycapRoot;
        [SerializeField, Min(0f)] private float completedCueSeconds = 1.2f;

        [Header("Events")]
        [SerializeField] private UnityEvent onBegan = new UnityEvent();
        [SerializeField] private UnityEvent onCompleted = new UnityEvent();

        private int observedDashCount;
        private bool directBeginRequested;
        private float completedSecondsRemaining;

        public TutorialPhase CurrentPhase { get; private set; }
        public bool HasBegun { get; private set; }
        public bool IsCompleted { get; private set; }

        private static bool ProgressBlocked => GameSessionControls.BlockGameplayInput
            || GameSessionControls.IsPaused || SceneRestartWipe.IsPlaying || SceneFadeTransition.IsPlaying;

        private void Awake()
        {
            HideCue();
            if (hideExerciseUntilBegin && exerciseRoot != null) exerciseRoot.SetActive(false);
        }

        public void ArmTutorial()
        {
            if (!isActiveAndEnabled || HasBegun) return;
            CurrentPhase = TutorialPhase.Armed;
        }

        public void BeginTutorial()
        {
            if (!isActiveAndEnabled || HasBegun) return;
            directBeginRequested = true;
            CurrentPhase = TutorialPhase.Armed;
            if (player == null || launchArea == null || landingArea == null
                || ProgressBlocked || player.IsDying || player.ControlsLocked) return;
            directBeginRequested = false;
            HasBegun = true;
            if (exerciseRoot != null) exerciseRoot.SetActive(true);
            ResetAttempt();
            onBegan.Invoke();
            if (isActiveAndEnabled && !ProgressBlocked && !player.ControlsLocked) ShowCue();
        }

        private void Update()
        {
            if (player == null || launchArea == null || landingArea == null)
            {
                HideCue();
                return;
            }
            if (player.IsDying)
            {
                if (HasBegun && !IsCompleted) ResetAttempt();
                HideCue();
                return;
            }
            if (ProgressBlocked || player.ControlsLocked)
            {
                HideCue();
                return;
            }
            if (CurrentPhase == TutorialPhase.Armed)
            {
                if (directBeginRequested || (startPoint != null
                    && Vector2.Distance(player.transform.position, startPoint.position) <= startDistance))
                    BeginTutorial();
                return;
            }
            if (!HasBegun) return;
            if (IsCompleted)
            {
                completedSecondsRemaining = Mathf.Max(0f, completedSecondsRemaining - Time.unscaledDeltaTime);
                ShowCue();
                return;
            }

            if (player.IgnitionDashCount != observedDashCount)
            {
                observedDashCount = player.IgnitionDashCount;
                Vector2 direction = requiredDirection.sqrMagnitude > 0f ? requiredDirection.normalized : Vector2.right;
                if (Contains(launchArea, player.LastIgnitionDashPosition)
                    && Vector2.Dot(direction, player.LastIgnitionDashDirection) >= directionDot)
                    CurrentPhase = TutorialPhase.Land;
            }
            if (CurrentPhase == TutorialPhase.Land)
            {
                if (Contains(landingArea, player.transform.position) && player.CanConfirmCollectible)
                {
                    IsCompleted = true;
                    CurrentPhase = TutorialPhase.Completed;
                    completedSecondsRemaining = completedCueSeconds;
                    ShowCue();
                    onCompleted.Invoke();
                    return;
                }
                if (Contains(launchArea, player.transform.position) && player.CanConfirmCollectible)
                    ResetAttempt();
            }
            else
            {
                CurrentPhase = player.IsInIgnitionRing && Contains(launchArea, player.transform.position)
                    ? TutorialPhase.Dash : TutorialPhase.EnterRing;
            }
            ShowCue();
        }

        private void ResetAttempt()
        {
            observedDashCount = player != null ? player.IgnitionDashCount : 0;
            CurrentPhase = TutorialPhase.EnterRing;
        }

        public void ResetTutorial()
        {
            HasBegun = IsCompleted = directBeginRequested = false;
            completedSecondsRemaining = 0f;
            CurrentPhase = TutorialPhase.Idle;
            HideCue();
        }

        private static bool Contains(BoxCollider2D area, Vector2 point)
        {
            if (area == null || !area.enabled || !area.isTrigger || !area.gameObject.activeInHierarchy) return false;
            return area.bounds.Contains(new Vector3(point.x, point.y, area.bounds.center.z));
        }

        private void ShowCue()
        {
            bool completed = CurrentPhase == TutorialPhase.Completed;
            bool visible = completed ? completedSecondsRemaining > 0f : HasBegun;
            if (cueCanvas != null) cueCanvas.enabled = visible;
            if (keycapRoot != null) keycapRoot.SetActive(visible && !completed && CurrentPhase != TutorialPhase.Land);
            if (!visible) return;
            if (keyLabel != null) keyLabel.text = CurrentPhase == TutorialPhase.Dash ? "→+Z" : "C";
            if (instruction != null)
                instruction.text = completed ? "점화 대시 성공!" : CurrentPhase == TutorialPhase.Land
                    ? "오른쪽 바닥에 안전하게 착지하세요"
                    : CurrentPhase == TutorialPhase.Dash ? "링이 켜져 있는 동안 →+Z로 대시하세요"
                    : "틈 앞까지 걸어가 C, 이어서 →+Z로 건너세요";
        }

        private void HideCue()
        {
            if (cueCanvas != null) cueCanvas.enabled = false;
            if (keycapRoot != null) keycapRoot.SetActive(false);
        }

        private void OnDisable() => HideCue();
    }
}
