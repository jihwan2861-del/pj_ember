using UnityEngine;

namespace EmberPrototype
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    [DisallowMultipleComponent]
    public sealed class BurstDashAbility : MonoBehaviour
    {
        [Header("Burst Dash (C, then Z)")]
        [Tooltip("Maximum speed reached by the dash.")]
        [SerializeField, Min(0f)] private float maximumSpeed = 16f;
        [Tooltip("Time in seconds to accelerate from rest to maximum speed. Set to 0 for an instant start.")]
        [SerializeField, Min(0f)] private float accelerationTime = 0.05f;
        [SerializeField, Min(0.01f)] private float duration = 0.15f;
        [SerializeField, Range(0f, 1f)] private float endSpeedMultiplier = 0.55f;
        [Tooltip("Caps upward speed retained after C+Z.")]
        [SerializeField, Range(0f, 1f)] private float upwardExitSpeedMultiplier = 0.22f;
        [Tooltip("Optional smaller collider used only during the dash.")]
        [SerializeField] private Collider2D dashCollider;
        [Tooltip("Legacy setting retained for serialized compatibility. Dash direction assistance is disabled.")]
        [SerializeField, HideInInspector, Min(0f)] private float obstacleAssistDistance = 0.45f;
        [Tooltip("Legacy setting retained for serialized compatibility. Dash direction assistance is disabled.")]
        [SerializeField, HideInInspector, Range(0f, 90f)] private float obstacleAssistAngle = 22f;
        [Tooltip("Legacy setting retained for serialized compatibility. Dash direction assistance is disabled.")]
        [SerializeField, HideInInspector, Range(0, 4)] private int obstacleAssistSteps = 3;

        private readonly Collider2D[] restoreOverlaps = new Collider2D[32];
        private Rigidbody2D body;
        private Collider2D playerCollider;
        private BoxCollider2D playerBoxCollider;
        private ContactFilter2D restoreFilter;
        private float remainingDuration;
        private float currentSpeed;
        private bool isActive;
        private bool playerColliderEnabledBeforeDash = true;
        private bool usingDashCollider;
        private bool usedSafePositionFallback;
        private Vector2 dashStartPosition;
        private Vector2 lastSafePosition;

        public Vector2 Direction { get; private set; }
        public float CurrentSpeed => currentSpeed;
        public bool IsActive => isActive;
        public bool NeedsSafeReset { get; private set; }
        public Collider2D CastCollider => usingDashCollider ? dashCollider : playerCollider;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            playerCollider = GetComponent<Collider2D>();
            playerBoxCollider = playerCollider as BoxCollider2D;
            if (dashCollider == null)
            {
                Transform dashColliderTransform = transform.Find("Dash Collider");
                if (dashColliderTransform != null) dashCollider = dashColliderTransform.GetComponent<Collider2D>();
            }
            if (dashCollider != null) dashCollider.enabled = false;
        }

        private void OnDisable()
        {
            RestoreCollider();
            isActive = false;
        }

        public bool TryBegin(Vector2 desiredDirection, int obstacleLayerMask)
        {
            if (isActive) return false;

            if (desiredDirection == Vector2.zero) desiredDirection = Vector2.up;
            NeedsSafeReset = false;
            usedSafePositionFallback = false;
            dashStartPosition = lastSafePosition = body.position;
            restoreFilter = new ContactFilter2D();
            restoreFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
            restoreFilter.useTriggers = false;
            SetDashColliderActive(true);
            Direction = desiredDirection.normalized;
            remainingDuration = duration;
            currentSpeed = 0f;
            isActive = true;
            return true;
        }

        public float AdvanceSpeed(float deltaTime)
        {
            if (!isActive) return 0f;
            if (usingDashCollider && IsBodySpaceClear(body.position)) lastSafePosition = body.position;
            currentSpeed = AccelerateToMaximumSpeed(currentSpeed, maximumSpeed, accelerationTime, deltaTime);
            return currentSpeed;
        }

        public bool AdvanceDuration(float deltaTime)
        {
            if (!isActive) return true;
            remainingDuration -= deltaTime;
            return remainingDuration <= 0f;
        }

        public bool IsBlocked(Collision2D collision)
        {
            if (!isActive) return false;
            for (int i = 0; i < collision.contactCount; i++)
            {
                if (Vector2.Dot(Direction, collision.GetContact(i).normal) < -0.2f)
                    return true;
            }
            return false;
        }

        public Vector2 End(bool blocked)
        {
            Vector2 exitVelocity = Vector2.zero;
            if (!blocked)
            {
                float exitSpeed = currentSpeed * endSpeedMultiplier;
                exitVelocity = Direction * exitSpeed;
                exitVelocity.y = Mathf.Min(exitVelocity.y, currentSpeed * upwardExitSpeedMultiplier);
            }

            RestoreCollider();
            if (usedSafePositionFallback || NeedsSafeReset) exitVelocity = Vector2.zero;
            isActive = false;
            remainingDuration = 0f;
            currentSpeed = 0f;
            Direction = Vector2.zero;
            return exitVelocity;
        }

        public void Cancel()
        {
            if (!isActive) NeedsSafeReset = false;
            End(true);
        }

        public void RestoreCollider()
        {
            if (dashCollider == null) return;
            if (usingDashCollider)
            {
                NeedsSafeReset = !TryRestoreBodyPosition();
                // The controller resets the player if all recorded positions are now obstructed.
                playerCollider.enabled = !NeedsSafeReset && playerColliderEnabledBeforeDash;
            }
            dashCollider.enabled = false;
            usingDashCollider = false;
        }

        private void SetDashColliderActive(bool active)
        {
            if (dashCollider == null) return;
            if (!active)
            {
                RestoreCollider();
                return;
            }

            playerColliderEnabledBeforeDash = playerCollider.enabled;
            // Only swap a known full-body shape, from a position that can be restored safely.
            // Other collider shapes keep their normal collider throughout the dash.
            usingDashCollider = playerCollider.enabled && playerBoxCollider != null
                && IsBodySpaceClear(body.position);
            if (usingDashCollider) playerCollider.enabled = false;
            dashCollider.enabled = usingDashCollider;
        }

        private bool TryRestoreBodyPosition()
        {
            if (IsBodySpaceClear(body.position)) return true;

            Vector2 safePosition;
            if (IsBodySpaceClear(lastSafePosition)) safePosition = lastSafePosition;
            else if (IsBodySpaceClear(dashStartPosition)) safePosition = dashStartPosition;
            else return false;

            body.position = safePosition;
            body.linearVelocity = Vector2.zero;
            usedSafePositionFallback = true;
            return true;
        }

        private bool IsBodySpaceClear(Vector2 position)
        {
            if (playerBoxCollider == null) return false;

            Vector2 scale = transform.lossyScale;
            Vector2 absoluteScale = new Vector2(Mathf.Abs(scale.x), Mathf.Abs(scale.y));
            Vector2 size = Vector2.Scale(playerBoxCollider.size + Vector2.one * (playerBoxCollider.edgeRadius * 2f), absoluteScale);
            // Allow 0.001 world units of contact tolerance, not the smaller dash footprint.
            size.x = Mathf.Max(0.0001f, size.x - 0.002f);
            size.y = Mathf.Max(0.0001f, size.y - 0.002f);
            Vector2 offset = Quaternion.Euler(0f, 0f, body.rotation) * Vector2.Scale(playerBoxCollider.offset, scale);
            int count = Physics2D.OverlapBox(position + offset, size, body.rotation, restoreFilter, restoreOverlaps);
            if (count == restoreOverlaps.Length) return false;

            for (int i = 0; i < count; i++)
            {
                Collider2D other = restoreOverlaps[i];
                if (other == null || other.attachedRigidbody == body || Physics2D.GetIgnoreCollision(playerCollider, other)) continue;
                return false;
            }

            return true;
        }

        private static float AccelerateToMaximumSpeed(
            float speed,
            float targetSpeed,
            float accelerationDuration,
            float deltaTime)
        {
            if (targetSpeed <= 0f || accelerationDuration <= 0f) return targetSpeed;
            float acceleration = targetSpeed / accelerationDuration;
            return Mathf.MoveTowards(speed, targetSpeed, acceleration * deltaTime);
        }
    }
}
