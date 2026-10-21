using UnityEngine;
using UnityEngine.Events;

namespace EmberPrototype
{
    [AddComponentMenu("Ember/Circular Opening World")]
    [DisallowMultipleComponent]
    public sealed class CircularOpeningWorld : MonoBehaviour
    {
        [SerializeField] private AngleRotator rotator;
        [SerializeField] private FlamePlayerController player;
        [SerializeField] private bool beginOnStart = true;
        [SerializeField] private UnityEvent onStarted = new UnityEvent();
        [SerializeField] private UnityEvent onFinished = new UnityEvent();

        private bool ownsPlayer;
        public bool IsWalking { get; private set; }
        public bool IsPaused => rotator != null && rotator.IsPaused;
        public float ProgressAngle => rotator != null ? rotator.ProgressAngle : 0f;

        private void Start()
        {
            if (beginOnStart) BeginWalking();
        }

        public void BeginWalking()
        {
            if (!isActiveAndEnabled || IsWalking) return;
            if (rotator == null || !rotator.isActiveAndEnabled || rotator.IsFinished || player == null
                || player.transform.IsChildOf(rotator.RotationTarget)
                || rotator.RotationTarget.IsChildOf(player.transform) || player.IsCircularOpening
                || player.ControlsLocked || !player.CanStartTrialCeremony)
            {
                Debug.LogWarning("Rotator와 자유 상태의 Player를 연결해주세요. 플레이어는 회전 대상 밖에 있어야 합니다.", this);
                return;
            }
            var body = player.GetComponent<Rigidbody2D>();
            if (body == null) return;
            rotator.SetAutomatic(false);
            body.linearVelocity = Vector2.zero;
            player.SetCircularOpeningMode(true);
            ownsPlayer = true;
            IsWalking = true;
            rotator.ResumeRotation();
            onStarted.Invoke();
        }

        private void FixedUpdate()
        {
            if (!IsWalking) return;
            if (rotator == null || !rotator.isActiveAndEnabled || player == null) { StopWalking(); return; }
            if (GameSessionControls.BlockGameplayInput || player.ControlsLocked || player.IsDying) return;
            rotator.Drive(EmberPlayerInput.MovementDirection().x, Time.fixedDeltaTime);
            if (IsWalking && rotator.IsFinished) FinishWalking();
        }

        public void PauseWalking() { if (IsWalking) rotator.PauseRotation(); }
        public void ResumeWalking() { if (IsWalking) rotator.ResumeRotation(); }

        public void FinishWalking()
        {
            if (!IsWalking) return;
            StopWalking();
            onFinished.Invoke();
        }

        public void StopWalking()
        {
            IsWalking = false;
            if (rotator != null) rotator.PauseRotation();
            if (!ownsPlayer) return;
            if (player != null) player.SetCircularOpeningMode(false);
            ownsPlayer = false;
        }

        public void ResetWorld()
        {
            StopWalking();
            if (rotator != null) rotator.ResetRotation();
        }

        private void OnDisable() => StopWalking();
    }
}