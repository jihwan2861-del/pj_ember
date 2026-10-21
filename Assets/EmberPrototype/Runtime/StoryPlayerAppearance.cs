using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine;

namespace EmberPrototype
{
    [AddComponentMenu("Ember/Story Player Appearance")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    public sealed class StoryPlayerAppearance : MonoBehaviour
    {
        [SerializeField] private AnimationClip fireModeClip;
        [SerializeField] private EmberOpeningSequence sequence;
        [SerializeField] private bool startAsFire = true;

        private Animator animator;
        private RuntimeAnimatorController emberController;
        [Tooltip("그림만 회전합니다. 기본 90도는 둥근 부분이 아래, 꼬리가 위를 향합니다.")]
        [SerializeField] private float fireRotationDegrees = 90f;
        [Tooltip("충돌체는 그대로 두고 불꽃 그림만 위로 띄우는 높이입니다.")]
        [SerializeField] private float fireVisualHeight = 0.2f;
        [SerializeField, Range(0f, 60f)] private float movementTiltDegrees = 30f;
        [SerializeField, Min(0.01f)] private float tiltSmoothTime = 0.12f;
        private FlamePlayerController player;
        private Rigidbody2D body;
        private CircularOpeningWorld circularWorld;
        private float currentTilt;
        private float tiltVelocity;
        private SpriteRenderer emberRenderer;
        private SpriteRenderer fireRenderer;
        private PlayableGraph fireGraph;
        private bool previousForceRenderingOff;
        private bool initialized;
        private EmberOpeningSequence.OpeningStep previousStep;
        private bool observedStep;
        public bool IsFireForm { get; private set; }

        private void OnEnable()
        {
            if (!initialized)
            {
                animator = GetComponent<Animator>();
                player = GetComponent<FlamePlayerController>();
                body = GetComponent<Rigidbody2D>();
                foreach (var world in FindObjectsByType<CircularOpeningWorld>())
                    if (world.gameObject.scene == gameObject.scene) { circularWorld = world; break; }
                emberController = animator.runtimeAnimatorController;
                if (emberController == null || fireModeClip == null)
                {
                    Debug.LogWarning("Fire Mode Clip과 기존 플레이어 Animator Controller가 필요합니다.", this);
                    return;
                }
                emberRenderer = GetComponent<SpriteRenderer>();
                if (emberRenderer == null) return;
                previousForceRenderingOff = emberRenderer.forceRenderingOff;
                var visual = new GameObject("StoryFireVisual", typeof(SpriteRenderer), typeof(Animator));
                visual.transform.SetParent(transform, false);
                visual.transform.localPosition = Vector3.up * fireVisualHeight;
                visual.transform.localRotation = Quaternion.Euler(0f, 0f, fireRotationDegrees);
                fireRenderer = visual.GetComponent<SpriteRenderer>();
                fireRenderer.sharedMaterial = emberRenderer.sharedMaterial;
                fireGraph = PlayableGraph.Create("Story Fire Form");
                fireGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
                var output = AnimationPlayableOutput.Create(fireGraph, "Fire", visual.GetComponent<Animator>());
                output.SetSourcePlayable(AnimationClipPlayable.Create(fireGraph, fireModeClip));
                fireGraph.Play();
                IsFireForm = startAsFire;
                initialized = true;
            }
            ApplyForm();
            observedStep = false;
        }

        private void Update()
        {
            if (!initialized || sequence == null) return;
            var step = sequence.CurrentStep;
            if (observedStep && step == previousStep) return;
            observedStep = true;
            previousStep = step;
            if (step >= EmberOpeningSequence.OpeningStep.EmberBirth) ShowEmber();
            else if (sequence.IsRunning) ShowFire();
        }

        public void ShowFire()
        {
            if (!initialized) return;
            if (!IsFireForm) currentTilt = tiltVelocity = 0f;
            IsFireForm = true;
            ApplyForm();
        }

        public void ShowEmber()
        {
            if (!initialized) return;
            IsFireForm = false;
            ApplyForm();
        }

        private void ApplyForm()
        {
            if (emberRenderer == null || fireRenderer == null) return;
            emberRenderer.forceRenderingOff = IsFireForm || previousForceRenderingOff;
            fireRenderer.gameObject.SetActive(IsFireForm);
        }

        private void LateUpdate()
        {
            if (!initialized || !IsFireForm || fireRenderer == null) return;
            fireRenderer.transform.localPosition = Vector3.up * fireVisualHeight;
            float direction = 0f;
            bool blocked = GameSessionControls.BlockGameplayInput
                || (player != null && (player.ControlsLocked || player.IsDying));
            if (!blocked)
            {
                if (player != null && player.IsCircularOpening)
                {
                    if (circularWorld != null && circularWorld.IsWalking && !circularWorld.IsPaused)
                        direction = EmberPlayerInput.MovementDirection().x;
                }
                else if (body != null && Mathf.Abs(body.linearVelocity.x) > 0.1f)
                    direction = Mathf.Sign(body.linearVelocity.x);
            }
            currentTilt = Mathf.SmoothDampAngle(currentTilt, direction * movementTiltDegrees,
                ref tiltVelocity, tiltSmoothTime);
            fireRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, fireRotationDegrees + currentTilt);
            fireRenderer.sortingLayerID = emberRenderer.sortingLayerID;
            fireRenderer.sortingOrder = emberRenderer.sortingOrder;
            fireRenderer.color = emberRenderer.color;
            fireRenderer.enabled = emberRenderer.enabled;
        }

        private void OnDisable()
        {
            if (emberRenderer != null) emberRenderer.forceRenderingOff = previousForceRenderingOff;
            if (fireRenderer != null) fireRenderer.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (fireGraph.IsValid()) fireGraph.Destroy();
            if (fireRenderer != null) Destroy(fireRenderer.gameObject);
        }
    }
}
