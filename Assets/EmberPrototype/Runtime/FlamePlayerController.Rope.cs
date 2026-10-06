using UnityEngine;

namespace EmberPrototype
{
    public sealed partial class FlamePlayerController
    {
        [Header("로프 연결")]
        [Tooltip("진입 전 속도가 로프 흡수 접근 속도에 반영되는 비율. 스윙 전달 비율과 별개입니다.")]
        [SerializeField, Min(0f)] private float ropeTravelSpeedFactor = 1.15f;
        [SerializeField, Min(1f)] private float ropeTravelSpeedLimit = 36f;
        [SerializeField, Min(0f)] private float ropeKickBoost = 4f;
        [SerializeField, Min(0f)] private float ropeAirAcceleration = 8f;
        [SerializeField, Min(1f)] private float ropeAirSpeedLimit = 30f;

        private BurningRope pendingRope;
        private BurningRope activeRope;
        private Vector2 ropeEntryVelocity;
        private bool travellingToRope;
        private bool ropeMomentum;
        private readonly RaycastHit2D[] ropeObstacleHits = new RaycastHit2D[16];

        private void PrepareRopeTravel(FlammableTile destination)
        {
            Vector2 incoming = activeRope != null ? activeRope.EndVelocity : body.linearVelocity;
            if (activeRope != null) activeRope.Detach(this, Vector2.zero);
            activeRope = null;
            pendingRope = destination != null ? destination.GetComponentInParent<BurningRope>() : null;
            if (pendingRope != null && pendingRope.EndFire != destination) pendingRope = null;
            travellingToRope = pendingRope != null;
            ropeEntryVelocity = pendingRope != null ? incoming : Vector2.zero;
            ropeMomentum = false;
        }

        private float RopeTravelSpeed()
        {
            // Always allow pursuit of a moving end; the endpoint speed is bounded by the rope.
            float minimum = Mathf.Max(fireTravelSpeed, pendingRope.MaximumEndSpeed + 2f);
            return Mathf.Clamp(Mathf.Max(minimum, ropeEntryVelocity.magnitude * ropeTravelSpeedFactor),
                minimum, Mathf.Max(minimum, ropeTravelSpeedLimit));
        }

        private void EnterRope()
        {
            BurningRope rope = pendingRope;
            if (rope == null || !rope.Attach(this, ropeEntryVelocity))
            {
                StopFireTravelAtObstacle();
                return;
            }
            activeRope = rope;
            pendingRope = null;
            travellingToRope = false;
            ropeEntryVelocity = Vector2.zero;
            state = FlameState.RopeAnchored;
            body.position = rope.EndPosition;
            body.linearVelocity = Vector2.zero;
            body.gravityScale = 0f;
            body.collisionDetectionMode = initialCollisionDetectionMode;
            bodyCollider.enabled = false;
            transform.localScale = Vector3.one * 0.45f;
            travelSourceFire = null;
            hasFireTravelWaypoint = false;
            fireTravelCurrentSpeed = 0f;
            jumpRemaining = coyoteRemaining = 0f;
            normalJump = jumpRiseActive = false;
            airJumpsRemaining = maxAirJumps;
            burstAvailable = true;
            anchoredAimDirection = Vector2.zero;
            SetAbsorbTargetPreview(null);
            flameFeedback.HideLaunchRing();
            afterimageEffect.StopTrail();
            Camera.main?.GetComponent<CelesteRoomCamera>()?.ShakeAbsorb();
        }

        private void StepAttachedRope()
        {
            if (activeRope == null || !activeRope.IsAvailable)
            {
                ReleaseRope(Vector2.zero, false);
                return;
            }
            float safeAngle = activeRope.Angle;
            Vector2 start = body.position;
            activeRope.Advance(Time.fixedDeltaTime, horizontalInput);
            Vector2 end = activeRope.EndPosition;
            if (!IsRopeStepClear(start, end))
            {
                activeRope.StopAt(safeAngle);
                end = activeRope.EndPosition;
            }
            body.position = end;
            body.linearVelocity = Vector2.zero;
            // Gravity and jump buffers must not advance while the flame is attached.
            body.gravityScale = 0f;
            jumpRemaining = coyoteRemaining = 0f;
        }

        private bool IsRopeStepClear(Vector2 start, Vector2 end)
        {
            Vector2 displacement = end - start;
            float distance = displacement.magnitude;
            if (distance <= 0.0001f) return true;
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(fireTravelObstacleMask.value == 0 ? Physics2D.AllLayers : fireTravelObstacleMask.value);
            filter.useTriggers = false;
            int count = Physics2D.BoxCast(start + fireTravelProbeOffset, fireTravelProbeSize, 0f,
                displacement / distance, filter, ropeObstacleHits, distance);
            for (int i = 0; i < count; i++)
            {
                RaycastHit2D hit = ropeObstacleHits[i];
                if (hit.collider == null || hit.collider.attachedRigidbody == body) continue;
                if (Vector2.Dot(displacement / distance, hit.normal) < -0.01f) return false;
            }
            return count < ropeObstacleHits.Length;
        }

        private void LaunchFromRope()
        {
            if (activeRope == null || !activeRope.IsAvailable) { ReleaseRope(Vector2.zero, false); return; }
            Vector2 velocity = activeRope.EndVelocity;
            Vector2 direction = velocity.sqrMagnitude > 0.0001f ? velocity.normalized : Vector2.zero;
            Vector2 additional = direction * ropeKickBoost;
            Vector2 exitPosition = body.position;
            if (!IsSafeFireLaunchPosition(body.position, exitPosition)) return;
            // No artificial offset is needed: the rope end is a trigger, not solid geometry.
            ReleaseRope(Vector2.ClampMagnitude(velocity + additional, ropeAirSpeedLimit), true);
            if (direction != Vector2.zero)
            {
                flameFeedback.PlayLaunch(direction);
                Camera.main?.GetComponent<CelesteRoomCamera>()?.ShakeFireLaunch();
                afterimageEffect.PlayTimedTrail(launchAfterimageDuration);
            }
        }

        private void ReleaseRope(Vector2 velocity, bool kick)
        {
            if (activeRope != null)
            {
                Vector2 additional = kick && activeRope.EndVelocity.sqrMagnitude > 0.0001f
                    ? activeRope.EndVelocity.normalized * ropeKickBoost : Vector2.zero;
                activeRope.Detach(this, additional);
            }
            activeRope = pendingRope = null;
            travellingToRope = false;
            ropeEntryVelocity = Vector2.zero;
            ropeMomentum = kick;
            state = FlameState.Free;
            targetFire = travelSourceFire = null;
            hasFireTravelWaypoint = false;
            appliedPlatformVelocity = Vector2.zero;
            jumpRemaining = coyoteRemaining = launchProtection = wallSpeedRetentionRemaining = 0f;
            normalJump = jumpRiseActive = cornerCorrectionUsed = false;
            wasGrounded = false;
            groundStateInitialized = true;
            transform.localScale = initialScale;
            bodyCollider.enabled = true;
            body.gravityScale = initialGravity;
            body.collisionDetectionMode = initialCollisionDetectionMode;
            body.linearVelocity = velocity;
            upwardVelocityBeforeCollision = Mathf.Max(0f, velocity.y);
            horizontalVelocityBeforeCollision = velocity.x;
            SetAbsorbTargetPreview(null);
            flameFeedback?.HideLaunchRing();
        }

        private void ClearRopeState(bool restoreAttachedBody)
        {
            if (restoreAttachedBody && state == FlameState.RopeAnchored && body != null)
                ReleaseRope(Vector2.zero, false);
            else if (activeRope != null) activeRope.Detach(this, Vector2.zero);
            activeRope = pendingRope = null;
            travellingToRope = false;
            ropeEntryVelocity = Vector2.zero;
            ropeMomentum = false;
        }
    }
}
