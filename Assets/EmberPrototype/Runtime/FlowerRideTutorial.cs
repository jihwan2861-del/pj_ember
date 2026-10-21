using UnityEngine;
using UnityEngine.Events;

namespace EmberPrototype
{
    [DefaultExecutionOrder(200)]
    [DisallowMultipleComponent]
    [AddComponentMenu("Ember/Flower Ride Tutorial")]
    public sealed class FlowerRideTutorial : MonoBehaviour
    {
        public enum TutorialPhase
        {
            Idle,
            Armed,
            ActivateFlower,
            BoardFlame,
            RideAndExit,
            LandAndPurify,
            Completed,
            ReopenPassage
        }

        [Header("Exercise")]
        [SerializeField] private FlamePlayerController player;
        [SerializeField] private FireFlower flower;
        [SerializeField] private BoxCollider2D landingArea;
        [SerializeField] private PollutionObstacle pollution;
        [SerializeField] private GameObject exerciseRoot;
        [SerializeField] private bool hideExerciseUntilBegin = true;
        [SerializeField] private Transform startPoint;
        [SerializeField, Min(0f)] private float startDistance = 5f;

        [Header("Existing Cue UI")]
        [SerializeField] private Canvas cueCanvas;
        [SerializeField] private UnityEngine.UI.Text instruction;
        [SerializeField] private UnityEngine.UI.Text keyLabel;
        [SerializeField] private GameObject keycapRoot;
        [SerializeField, Min(0f)] private float completedCueSeconds = 1.2f;

        [Header("First Boarding Slow Motion")]
        [SerializeField, Range(0.01f, 1f)] private float slowMotionScale = 0.2f;
        [SerializeField, Range(0f, 3f)] private float slowMotionRealSeconds = 2f;

        [Header("Events")]
        [SerializeField] private UnityEvent onBegan = new UnityEvent();
        [SerializeField] private UnityEvent onCompleted = new UnityEvent();

        private static FlowerRideTutorial slowOwner;
        private FireFlowerProjectile trackedProjectile;
        private int lastSeenShotCount;
        private int rideCountAtAttempt;
        private int exitCountAtAttempt;
        private bool directBeginRequested;
        private bool observedDeath;
        private bool completedOnce;
        private bool ownsSlowMotion;
        private float capturedPlaybackSpeed;
        private float appliedTimeScale;
        private float slowSecondsRemaining;
        private float completedSecondsRemaining;

        public TutorialPhase CurrentPhase { get; private set; }
        public bool HasBegun { get; private set; }
        public bool IsCompleted => completedOnce;
        public bool IsPassageClear => pollution != null && pollution.IsPurified;
        public bool IsSlowMotionActive => ownsSlowMotion;

        private static bool ProgressBlocked => GameSessionControls.BlockGameplayInput
            || GameSessionControls.IsPaused || SceneRestartWipe.IsPlaying || SceneFadeTransition.IsPlaying;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => slowOwner = null;

        private void Awake()
        {
            HideCue();
            if (hideExerciseUntilBegin && exerciseRoot != null) exerciseRoot.SetActive(false);
        }

        // Session controls call this before saving a pause speed or changing playback speed.
        public static void EndSessionSlowMotion()
        {
            if (slowOwner != null) slowOwner.EndSlowMotion();
        }

        public void ArmTutorial()
        {
            if (!isActiveAndEnabled || HasBegun || completedOnce) return;
            directBeginRequested = false;
            CurrentPhase = TutorialPhase.Armed;
            HideCue();
        }

        // May also be connected to an existing PlayerEventTrigger.
        public void BeginTutorial()
        {
            if (!isActiveAndEnabled || HasBegun || completedOnce) return;
            directBeginRequested = true;
            CurrentPhase = TutorialPhase.Armed;
            if (player == null || flower == null || landingArea == null || pollution == null
                || ProgressBlocked || player.IsDying || player.ControlsLocked) return;

            directBeginRequested = false;
            HasBegun = true;
            if (exerciseRoot != null) exerciseRoot.SetActive(true);
            PrepareAttempt(false);
            onBegan.Invoke();
            if (isActiveAndEnabled && !ProgressBlocked && !player.ControlsLocked) ShowCue();
        }

        private void Update()
        {
            UpdateSlowMotion();
            if (player == null || flower == null || pollution == null || landingArea == null)
            {
                EndSlowMotion();
                HideCue();
                return;
            }
            if (player.IsDying)
            {
                EndSlowMotion();
                if (!observedDeath && HasBegun && !completedOnce) PrepareAttempt(true);
                observedDeath = true;
                HideCue();
                return;
            }
            observedDeath = false;
            if (ProgressBlocked || player.ControlsLocked)
            {
                EndSlowMotion();
                HideCue();
                return;
            }

            if (CurrentPhase == TutorialPhase.Armed)
            {
                if (directBeginRequested || IsNearStart()) BeginTutorial();
                return;
            }
            if (!HasBegun) return;

            if (completedOnce)
            {
                // Learning persists through a death, but MapReset may close the passage again.
                if (!IsPassageClear)
                    CurrentPhase = TutorialPhase.ReopenPassage;
                else if (CurrentPhase == TutorialPhase.ReopenPassage)
                {
                    CurrentPhase = TutorialPhase.Completed;
                    completedSecondsRemaining = 0f;
                }
                if (CurrentPhase == TutorialPhase.Completed)
                    completedSecondsRemaining = Mathf.Max(0f, completedSecondsRemaining - Time.unscaledDeltaTime);
                ShowCue();
                return;
            }

            if (CurrentPhase == TutorialPhase.ActivateFlower || CurrentPhase == TutorialPhase.BoardFlame)
                ObserveNewShot();

            if (CurrentPhase == TutorialPhase.BoardFlame)
            {
                if (player.FlowerRideCount != rideCountAtAttempt && player.IsRidingFlower
                    && player.CurrentFlowerRide == trackedProjectile)
                {
                    EndSlowMotion();
                    CurrentPhase = TutorialPhase.RideAndExit;
                }
                else if (!IsTrackedProjectileAlive()) PrepareAttempt(true);
            }
            else if (CurrentPhase == TutorialPhase.RideAndExit)
            {
                if (player.FlowerRideExitCount != exitCountAtAttempt && !player.IsRidingFlower)
                    CurrentPhase = TutorialPhase.LandAndPurify;
                else if (!IsTrackedProjectileAlive() && !player.IsRidingFlower)
                    PrepareAttempt(true);
            }
            else if (CurrentPhase == TutorialPhase.LandAndPurify)
            {
                if (!player.IsRidingFlower && !player.IsInsideFire && !player.IsTravellingToFire
                    && IsPassageClear && IsAtLanding() && player.CanConfirmCollectible)
                {
                    CompleteTutorial();
                    return;
                }
                if (!IsTrackedProjectileAlive() && player.CanConfirmCollectible && !IsAtLanding())
                    PrepareAttempt(true);
                else if (!IsTrackedProjectileAlive() && !IsPassageClear)
                    PrepareAttempt(true);
            }
            ShowCue();
        }

        private void PrepareAttempt(bool resetFlower)
        {
            EndSlowMotion();
            trackedProjectile = null;
            if (resetFlower && flower != null && flower.isActiveAndEnabled) flower.ResetForRespawn();
            lastSeenShotCount = flower != null ? flower.ShotCount : 0;
            rideCountAtAttempt = player != null ? player.FlowerRideCount : 0;
            exitCountAtAttempt = player != null ? player.FlowerRideExitCount : 0;
            CurrentPhase = TutorialPhase.ActivateFlower;
        }

        private void ObserveNewShot()
        {
            if (flower.ShotCount == lastSeenShotCount) return;
            lastSeenShotCount = flower.ShotCount;
            EndSlowMotion();
            trackedProjectile = flower.LastProjectile;
            if (!IsTrackedProjectileAlive()) return;
            CurrentPhase = TutorialPhase.BoardFlame;
            if (trackedProjectile.CanBoard && !trackedProjectile.HasBeenRidden) BeginSlowMotion();
        }

        private bool IsTrackedProjectileAlive() => trackedProjectile != null && trackedProjectile.isActiveAndEnabled;

        private bool IsNearStart()
        {
            if (player == null) return false;
            Transform point = startPoint != null ? startPoint : flower != null ? flower.transform : null;
            return point != null && ((Vector2)(player.transform.position - point.position)).sqrMagnitude
                <= startDistance * startDistance;
        }

        private bool IsAtLanding()
        {
            if (landingArea == null || !landingArea.enabled || !landingArea.isTrigger
                || !landingArea.gameObject.activeInHierarchy) return false;
            Bounds bounds = landingArea.bounds;
            Vector3 position = player.transform.position;
            position.z = bounds.center.z;
            return bounds.Contains(position);
        }

        private void CompleteTutorial()
        {
            if (completedOnce) return;
            EndSlowMotion();
            completedOnce = true;
            CurrentPhase = TutorialPhase.Completed;
            completedSecondsRemaining = completedCueSeconds;
            ShowCue();
            onCompleted.Invoke();
        }

        private void BeginSlowMotion()
        {
            if (ownsSlowMotion || (slowOwner != null && slowOwner != this)
                || slowMotionRealSeconds <= 0f || ProgressBlocked || player == null || player.ControlsLocked) return;
            float playbackSpeed = GameSessionControls.PlaybackSpeed;
            if (playbackSpeed <= 0f || !Mathf.Approximately(Time.timeScale, playbackSpeed)) return;
            capturedPlaybackSpeed = playbackSpeed;
            appliedTimeScale = playbackSpeed * Mathf.Clamp(slowMotionScale, 0.01f, 1f);
            slowSecondsRemaining = Mathf.Clamp(slowMotionRealSeconds, 0f, 3f);
            ownsSlowMotion = true;
            slowOwner = this;
            Time.timeScale = appliedTimeScale;
        }

        private void UpdateSlowMotion()
        {
            if (!ownsSlowMotion) return;
            if (!Mathf.Approximately(GameSessionControls.PlaybackSpeed, capturedPlaybackSpeed)
                || !Mathf.Approximately(Time.timeScale, appliedTimeScale))
            {
                // An external system owns the new value; never write our saved speed over it.
                ownsSlowMotion = false;
                if (slowOwner == this) slowOwner = null;
                return;
            }
            slowSecondsRemaining -= Time.unscaledDeltaTime;
            if (slowSecondsRemaining <= 0f) EndSlowMotion();
        }

        private void EndSlowMotion()
        {
            if (!ownsSlowMotion) return;
            ownsSlowMotion = false;
            if (slowOwner == this) slowOwner = null;
            if (Mathf.Approximately(GameSessionControls.PlaybackSpeed, capturedPlaybackSpeed)
                && Mathf.Approximately(Time.timeScale, appliedTimeScale))
                Time.timeScale = capturedPlaybackSpeed;
        }

        public void ResetTutorial()
        {
            EndSlowMotion();
            trackedProjectile = null;
            HasBegun = completedOnce = directBeginRequested = observedDeath = false;
            completedSecondsRemaining = 0f;
            CurrentPhase = TutorialPhase.Idle;
            HideCue();
        }

        public void DismissCue()
        {
            if (!completedOnce) return;
            completedSecondsRemaining = 0f;
            HideCue();
        }

        private void ShowCue()
        {
            bool showKey = false;
            bool visible = true;
            string key = string.Empty;
            string message;
            switch (CurrentPhase)
            {
                case TutorialPhase.ActivateFlower:
                    visible = IsNearStart();
                    showKey = true;
                    key = "C";
                    message = "C로 태양꽃의 불꽃을 발사하세요";
                    break;
                case TutorialPhase.BoardFlame:
                    showKey = true;
                    key = "X";
                    message = "X로 날아가는 불꽃에 탑승하세요";
                    break;
                case TutorialPhase.RideAndExit:
                    showKey = true;
                    key = "Z";
                    float remaining = IsTrackedProjectileAlive() ? Mathf.Max(0f, trackedProjectile.RideSecondsRemaining) : 0f;
                    message = "방향키로 조향하고 Z로 빠져나오세요 (" + remaining.ToString("0.0") + "초)";
                    break;
                case TutorialPhase.LandAndPurify:
                    message = IsPassageClear ? "오른쪽 발판에 안전하게 착지하세요"
                        : "불꽃으로 오염을 지우고 오른쪽 발판에 착지하세요";
                    break;
                case TutorialPhase.Completed:
                    visible = completedSecondsRemaining > 0f;
                    message = "불꽃 탑승 성공!";
                    break;
                case TutorialPhase.ReopenPassage:
                    visible = IsNearStart();
                    showKey = true;
                    key = "C";
                    message = "C로 태양꽃을 깨워 길을 다시 여세요";
                    break;
                default:
                    HideCue();
                    return;
            }
            if (cueCanvas != null) cueCanvas.enabled = visible;
            if (keycapRoot != null) keycapRoot.SetActive(visible && showKey);
            if (!visible) return;
            if (keyLabel != null) keyLabel.text = key;
            if (instruction != null) instruction.text = message;
        }

        private void HideCue()
        {
            if (cueCanvas != null) cueCanvas.enabled = false;
            if (keycapRoot != null) keycapRoot.SetActive(false);
        }

        private void OnDisable()
        {
            EndSlowMotion();
            HideCue();
        }

        private void OnDestroy() => EndSlowMotion();
    }
}
