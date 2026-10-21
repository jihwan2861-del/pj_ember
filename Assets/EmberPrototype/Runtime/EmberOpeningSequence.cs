using System;
using UnityEngine;
using UnityEngine.Events;

namespace EmberPrototype
{
    /// <summary>Storyboard order only. Scene layout, camera motion and effects are connected through events.</summary>
    [AddComponentMenu("Ember/Opening Sequence")]
    [DisallowMultipleComponent]
    public sealed class EmberOpeningSequence : MonoBehaviour
    {
        public enum OpeningStep
        {
            NotStarted = 0,
            WarmSun = 1,
            PollutedSun = 2,
            FlowerPurification = 3,
            RoundToFlat = 4,
            FirstWaterJump = 5,
            ConsecutiveJumps = 6,
            PastReflection = 7,
            EmberBirth = 8,
            MountainReveal = 9,
            Completed = 10
        }

        [Serializable]
        private sealed class StepEvents
        {
            [Tooltip("이 단계에 들어올 때 실행합니다.")]
            public UnityEvent onEnter = new UnityEvent();
            [Tooltip("완료하거나 ResetSequence로 나갈 때 실행합니다. 안내·연출 정리에 사용합니다.")]
            public UnityEvent onExit = new UnityEvent();
        }

        [SerializeField] private bool beginOnStart = true;
        [Header("1. 따뜻했던 태양")]
        [SerializeField] private StepEvents warmSun = new StepEvents();
        [Header("2. 오염된 태양")]
        [SerializeField] private StepEvents pollutedSun = new StepEvents();
        [Header("3. C — 불꽃꽃으로 오염 제거")]
        [SerializeField] private StepEvents flowerPurification = new StepEvents();
        [Header("4. 둥근 지형에서 평지로")]
        [SerializeField] private StepEvents roundToFlat = new StepEvents();
        [Header("5. Z — 첫 물 건너기")]
        [SerializeField] private StepEvents firstWaterJump = new StepEvents();
        [Header("6. 연속 점프")]
        [SerializeField] private StepEvents consecutiveJumps = new StepEvents();
        [Header("7. 물에 비친 과거")]
        [SerializeField] private StepEvents pastReflection = new StepEvents();
        [Header("8. 앰버의 탄생")]
        [SerializeField] private StepEvents emberBirth = new StepEvents();
        [Header("9. 산을 향해")]
        [SerializeField] private StepEvents mountainReveal = new StepEvents();
        [Header("Sequence complete")]
        [SerializeField] private UnityEvent onCompleted = new UnityEvent();

        private bool transitioning;
        public OpeningStep CurrentStep { get; private set; } = OpeningStep.NotStarted;
        public bool IsRunning => CurrentStep >= OpeningStep.WarmSun && CurrentStep <= OpeningStep.MountainReveal;

        private void Start()
        {
            if (beginOnStart) BeginSequence();
        }

        public void BeginSequence()
        {
            if (!isActiveAndEnabled || transitioning || CurrentStep != OpeningStep.NotStarted) return;
            TransitionTo(OpeningStep.WarmSun);
        }

        // Named methods prevent a late or duplicate signal from completing a different step.
        public void CompleteWarmSun() => CompleteStep(OpeningStep.WarmSun);
        public void CompletePollutedSun() => CompleteStep(OpeningStep.PollutedSun);
        public void CompleteFlowerPurification() => CompleteStep(OpeningStep.FlowerPurification);
        public void CompleteRoundToFlat() => CompleteStep(OpeningStep.RoundToFlat);
        public void CompleteFirstWaterJump() => CompleteStep(OpeningStep.FirstWaterJump);
        public void CompleteConsecutiveJumps() => CompleteStep(OpeningStep.ConsecutiveJumps);
        public void CompletePastReflection() => CompleteStep(OpeningStep.PastReflection);
        public void CompleteEmberBirth() => CompleteStep(OpeningStep.EmberBirth);
        public void CompleteMountainReveal() => CompleteStep(OpeningStep.MountainReveal);

        public void CompleteStep(OpeningStep expectedStep)
        {
            if (!isActiveAndEnabled || transitioning || !IsRunning || CurrentStep != expectedStep) return;
            TransitionTo((OpeningStep)((int)CurrentStep + 1));
        }

        /// <summary>Stops progress and runs the current exit event; call BeginSequence to replay.</summary>
        public void ResetSequence()
        {
            if (transitioning) return;
            TransitionTo(OpeningStep.NotStarted);
        }

        private void TransitionTo(OpeningStep next)
        {
            transitioning = true;
            try
            {
                EventsFor(CurrentStep)?.onExit.Invoke();
                CurrentStep = next;
                EventsFor(CurrentStep)?.onEnter.Invoke();
                if (CurrentStep == OpeningStep.Completed) onCompleted.Invoke();
            }
            finally { transitioning = false; }
        }

        private StepEvents EventsFor(OpeningStep step)
        {
            switch (step)
            {
                case OpeningStep.WarmSun: return warmSun;
                case OpeningStep.PollutedSun: return pollutedSun;
                case OpeningStep.FlowerPurification: return flowerPurification;
                case OpeningStep.RoundToFlat: return roundToFlat;
                case OpeningStep.FirstWaterJump: return firstWaterJump;
                case OpeningStep.ConsecutiveJumps: return consecutiveJumps;
                case OpeningStep.PastReflection: return pastReflection;
                case OpeningStep.EmberBirth: return emberBirth;
                case OpeningStep.MountainReveal: return mountainReveal;
                default: return null;
            }
        }
    }
}
