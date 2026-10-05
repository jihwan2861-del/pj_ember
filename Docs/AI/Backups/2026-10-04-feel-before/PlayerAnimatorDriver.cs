using UnityEngine;

namespace EmberPrototype
{
    /// <summary>Feeds grounded movement speed or the jump threshold into the player's 1D Blend Tree.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerAnimatorDriver : MonoBehaviour
    {
        private static readonly int SpeedParameter = Animator.StringToHash("Speed");
        private const float JumpBlendThreshold = 2f;

        [SerializeField, Min(0.01f)] private float runSpeed = 5f;
        [SerializeField, Min(0f)] private float speedDampTime = 0.08f;

        private Animator animator;
        private Rigidbody2D body;
        private SpriteRenderer spriteRenderer;
        private FlamePlayerController playerController;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            body = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            playerController = GetComponent<FlamePlayerController>();
        }

        private void Update()
        {
            if (animator == null || body == null) return;

            float horizontalVelocity = body.linearVelocity.x;
            if (spriteRenderer != null && Mathf.Abs(horizontalVelocity) > 0.05f)
            {
                // Keep the source-facing direction on rightward movement; mirror it when moving left.
                spriteRenderer.flipX = horizontalVelocity < 0f;
            }

            float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(horizontalVelocity) / Mathf.Max(runSpeed, 0.01f));
            bool isAirborne = playerController != null && playerController.IsAirborneForAnimation;
            float blendValue = isAirborne ? JumpBlendThreshold : normalizedSpeed;
            animator.SetFloat(SpeedParameter, blendValue, speedDampTime, Time.deltaTime);
        }
    }
}
