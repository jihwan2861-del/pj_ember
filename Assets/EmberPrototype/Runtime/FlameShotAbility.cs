using UnityEngine;

namespace EmberPrototype
{
    [RequireComponent(typeof(Rigidbody2D))]
    [DisallowMultipleComponent]
    public sealed class FlameShotAbility : MonoBehaviour
    {
        [Header("Projectile")]
        [Tooltip("Optional visual prefab. Leave empty to create a simple orange prototype shot automatically.")]
        [SerializeField] private EmberProjectile projectilePrefab;
        [SerializeField, Min(0.1f)] private float projectileSpeed = 16f;
        [SerializeField, Min(0.1f)] private float projectileRange = 8f;
        [SerializeField, Min(0.05f)] private float projectileRadius = 0.14f;

        [Header("Aim")]
        [Tooltip("Time scale while X is held to aim. Lower values give more time to choose a direction.")]
        [SerializeField, Range(0.05f, 1f)] private float aimTimeScale = 0.3f;
        [Tooltip("Maximum angle from the chosen arrow direction within which an unlit fire target can attract the shot.")]
        [SerializeField, Range(0f, 45f)] private float aimAssistAngle = 25f;

        private readonly Collider2D[] aimTargetHits = new Collider2D[64];
        private Rigidbody2D body;
        private bool isAiming;
        private bool slowMotionActive;
        private Vector2 aimDirection;
        private float timeScaleBeforeAim = 1f;
        private float fixedDeltaTimeBeforeAim;

        public bool IsAiming => isAiming;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        private void OnDisable()
        {
            CancelAim();
        }

        public bool BeginAim()
        {
            if (isAiming) return false;

            isAiming = true;
            aimDirection = Vector2.zero;
            timeScaleBeforeAim = Time.timeScale;
            fixedDeltaTimeBeforeAim = Time.fixedDeltaTime;

            float slowMotionScale = Mathf.Clamp(aimTimeScale, 0.05f, 1f);
            Time.timeScale = timeScaleBeforeAim * slowMotionScale;
            Time.fixedDeltaTime = Mathf.Max(0.0001f, fixedDeltaTimeBeforeAim * slowMotionScale);
            slowMotionActive = true;
            return true;
        }

        public void UpdateAim(Vector2 requestedDirection)
        {
            if (!isAiming || requestedDirection == Vector2.zero) return;
            aimDirection = FindAssistedDirection(requestedDirection.normalized);
        }

        public Vector2 EndAim()
        {
            Vector2 result = aimDirection;
            CancelAim();
            return result;
        }

        public void Fire(Vector2 direction)
        {
            if (direction == Vector2.zero) return;

            direction.Normalize();
            float radius = Mathf.Max(0.05f, projectileRadius);
            Vector2 origin = body.position + direction * (radius + 0.18f);
            EmberProjectile.Spawn(
                projectilePrefab,
                origin,
                direction,
                Mathf.Max(0.1f, projectileSpeed),
                Mathf.Max(0.1f, projectileRange),
                radius,
                transform);
        }

        public void CancelAim()
        {
            isAiming = false;
            aimDirection = Vector2.zero;
            if (!slowMotionActive) return;

            Time.timeScale = timeScaleBeforeAim;
            Time.fixedDeltaTime = fixedDeltaTimeBeforeAim;
            slowMotionActive = false;
        }

        private Vector2 FindAssistedDirection(Vector2 requestedDirection)
        {
            float assistAngle = Mathf.Clamp(aimAssistAngle, 0f, 45f);
            if (assistAngle <= 0f) return requestedDirection;

            float minimumAlignment = Mathf.Cos(assistAngle * Mathf.Deg2Rad);
            float searchRange = Mathf.Max(0.1f, projectileRange);
            int hitCount = Physics2D.OverlapCircleNonAlloc(
                body.position,
                searchRange,
                aimTargetHits,
                Physics2D.AllLayers);

            float bestAlignment = minimumAlignment;
            float bestDistanceSquared = float.PositiveInfinity;
            float searchRangeSquared = searchRange * searchRange;
            Vector2 assistedDirection = requestedDirection;

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hit = aimTargetHits[i];
                if (hit == null || hit.transform.IsChildOf(transform)) continue;

                FlammableTile fire = hit.GetComponentInParent<FlammableTile>();
                if (fire == null || fire.IsBurning) continue;

                Vector2 toFire = fire.AnchorPosition - body.position;
                float distanceSquared = toFire.sqrMagnitude;
                if (distanceSquared <= 0.0001f || distanceSquared > searchRangeSquared) continue;

                Vector2 targetDirection = toFire / Mathf.Sqrt(distanceSquared);
                float alignment = Vector2.Dot(requestedDirection, targetDirection);
                if (alignment < minimumAlignment) continue;
                if (alignment < bestAlignment
                    || (Mathf.Approximately(alignment, bestAlignment) && distanceSquared >= bestDistanceSquared))
                    continue;

                bestAlignment = alignment;
                bestDistanceSquared = distanceSquared;
                assistedDirection = targetDirection;
            }

            return assistedDirection;
        }
    }
}
