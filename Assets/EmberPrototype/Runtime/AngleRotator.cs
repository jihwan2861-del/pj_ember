using System;
using UnityEngine;
using UnityEngine.Events;

namespace EmberPrototype
{
    [AddComponentMenu("Ember/Angle Rotator")]
    [DisallowMultipleComponent]
    public sealed class AngleRotator : MonoBehaviour
    {
        [Serializable]
        private sealed class AnglePoint
        {
            public string label;
            [Min(0f)] public float angle = 30f;
            public bool pauseOnArrival;
            public UnityEvent onReached = new UnityEvent();
            [NonSerialized] public bool reached;
        }

        [Tooltip("비우면 이 오브젝트를 회전합니다. 로컬 Z축 기준입니다.")]
        [SerializeField] private Transform rotationTarget;
        [SerializeField, Min(0.01f)] private float degreesPerSecond = 12f;
        [SerializeField] private bool limitAngle = true;
        [SerializeField, Min(0.01f)] private float endAngle = 120f;
        [SerializeField] private bool allowBacktracking = true;
        [SerializeField] private bool reverseRotation;
        [Tooltip("켜면 FixedUpdate에서 자동 회전합니다. 외부 입력으로 Drive할 때는 끕니다.")]
        [SerializeField] private bool automatic;
        [SerializeField, Range(-1f, 1f)] private float automaticDirection = 1f;
        [SerializeField] private AnglePoint[] anglePoints = Array.Empty<AnglePoint>();
        [SerializeField] private UnityEvent onFinished = new UnityEvent();

        private Transform target;
        private Quaternion initialRotation;
        private bool initialized;
        private bool driving;
        private int revision;
        public bool IsPaused { get; private set; }
        public bool IsFinished { get; private set; }
        public float ProgressAngle { get; private set; }
        public Transform RotationTarget => rotationTarget != null ? rotationTarget : transform;
        public float RotationSign => reverseRotation ? -1f : 1f;

        public void SetAngleLimit(float degrees)
        {
            endAngle = Mathf.Max(0.01f, degrees);
            limitAngle = true;
        }

        private void FixedUpdate()
        {
            if (automatic) Drive(automaticDirection, Time.fixedDeltaTime);
        }

        public void Drive(float direction, float deltaTime)
        {
            if (!isActiveAndEnabled || driving || IsPaused || IsFinished || deltaTime <= 0f) return;
            Initialize();
            if (limitAngle && ProgressAngle >= endAngle) { Finish(); return; }
            if (Mathf.Approximately(direction, 0f)) return;
            float next = Mathf.Max(0f, ProgressAngle + Mathf.Clamp(direction, -1f, 1f) * degreesPerSecond * deltaTime);
            if (limitAngle) next = Mathf.Min(next, endAngle);
            if (!allowBacktracking) next = Mathf.Max(ProgressAngle, next);
            foreach (var point in anglePoints)
                if (point != null && !point.reached && point.pauseOnArrival && point.angle >= ProgressAngle && point.angle <= next)
                    next = Mathf.Min(next, point.angle);
            float previous = ProgressAngle;
            ProgressAngle = next;
            target.localRotation = initialRotation * Quaternion.Euler(0f, 0f, ProgressAngle * (reverseRotation ? -1f : 1f));
            if (next < previous) return;
            driving = true;
            int currentRevision = revision;
            try
            {
                foreach (var point in anglePoints)
                    if (point != null && !point.reached && point.angle >= previous && point.angle <= next)
                    {
                        point.reached = true;
                        if (point.pauseOnArrival) IsPaused = true;
                        point.onReached.Invoke();
                        if (!isActiveAndEnabled || revision != currentRevision) return;
                    }
                if (!IsPaused && limitAngle && ProgressAngle >= endAngle) Finish();
            }
            finally { driving = false; }
        }

        private void Initialize()
        {
            if (initialized) return;
            target = RotationTarget;
            initialRotation = target.localRotation;
            Array.Sort(anglePoints, (a, b) => (a == null ? float.MaxValue : a.angle).CompareTo(b == null ? float.MaxValue : b.angle));
            initialized = true;
        }

        public void PauseRotation() => IsPaused = true;
        public void ResumeRotation() => IsPaused = false;
        public void SetAutomatic(bool value) => automatic = value;
        public void SetDirection(float value) => automaticDirection = Mathf.Clamp(value, -1f, 1f);

        private void Finish()
        {
            if (IsFinished) return;
            IsFinished = true;
            onFinished.Invoke();
        }

        public void ResetRotation()
        {
            revision++;
            ProgressAngle = 0f;
            IsPaused = IsFinished = false;
            if (initialized && target != null) target.localRotation = initialRotation;
            foreach (var point in anglePoints) if (point != null) point.reached = false;
        }
    }
}
