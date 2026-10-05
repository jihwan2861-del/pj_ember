using UnityEngine;
using UnityEngine.Events;

namespace EmberPrototype
{
    [AddComponentMenu("Ember Prototype/Path Moving Block")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PathMovingBlock : MonoBehaviour
    {
        public enum TravelMode { Once = 0, PingPong = 1, Loop = 2 }

        [SerializeField] private MovementPath path;
        [SerializeField, Min(0.01f)] private float moveSpeed = 2.5f;
        [SerializeField] private bool snapToPathStart = true;
        [SerializeField] private bool activateOnStart;
        [SerializeField] private UnityEvent onArrived = new UnityEvent();
        [SerializeField] private TravelMode travelMode = TravelMode.Once;
        [SerializeField, Min(0f)] private float endWaitTime;

        private Rigidbody2D body;
        private float travelledDistance;
        private bool isMoving;
        private bool hasArrived;
        private int direction = 1;
        private float waitRemaining;

        public bool IsMoving => isMoving;
        public bool HasArrived => hasArrived;
        public bool IsWaiting => isMoving && waitRemaining > 0f;
        public MovementPath Path => path;
        public TravelMode Mode => travelMode;
        public bool CanLoop => path != null && path.IsClosed;
        public float TravelledDistance => travelledDistance;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            ConfigureBody();
            PreparePath();
            if (activateOnStart) Activate();
        }

        private void OnValidate()
        {
            moveSpeed = Mathf.Max(0.01f, moveSpeed);
            endWaitTime = Mathf.Max(0f, endWaitTime);
        }

        private void FixedUpdate()
        {
            if (!isMoving)
            {
                if (body != null && hasArrived) body.linearVelocity = Vector2.zero;
                return;
            }
            if (path == null || !path.IsValid || (travelMode == TravelMode.Loop && !path.IsClosed))
            {
                isMoving = false;
                if (body != null) body.linearVelocity = Vector2.zero;
                return;
            }
            float remainingTime = Time.fixedDeltaTime;
            // Bounded traversal handles normal overshoot without allocations or extreme-setting loops.
            for (int transition = 0; transition < 32 && remainingTime > 0f && isMoving; transition++)
            {
                if (waitRemaining > 0f)
                {
                    float spent = Mathf.Min(remainingTime, waitRemaining);
                    waitRemaining -= spent;
                    remainingTime -= spent;
                    if (remainingTime <= 0f) break;
                }
                hasArrived = false;
                float end = direction > 0 ? path.TotalLength : 0f;
                float distanceToEnd = Mathf.Abs(end - travelledDistance);
                float timeToEnd = distanceToEnd / Mathf.Max(0.01f, moveSpeed);
                // Distance baking and float arithmetic may put an exact tick endpoint a few ULPs away.
                // Snap only within a one-millionth world unit so arrival/wait do not gain an extra tick.
                bool reachesEnd = distanceToEnd <= moveSpeed * remainingTime + .000001f;
                float travelTime = Mathf.Min(remainingTime, timeToEnd);
                travelledDistance = reachesEnd ? end : Mathf.MoveTowards(travelledDistance, end, moveSpeed * travelTime);
                remainingTime -= travelTime;
                if (!reachesEnd) break;
                body.MovePosition(path.GetPositionAtDistance(travelledDistance));
                hasArrived = true;
                if (travelMode == TravelMode.Once) isMoving = false;
                else
                {
                    waitRemaining = endWaitTime;
                    if (travelMode == TravelMode.PingPong) direction = -direction;
                    else travelledDistance = 0f; // Closed endpoint equals start; never jump across an open path.
                }
                onArrived.Invoke();
            }
            // An arrival listener calling ResetBlock has authority over the queued move.
            if (isMoving || hasArrived) body.MovePosition(path.GetPositionAtDistance(travelledDistance));
        }

        public void Activate()
        {
            if (hasArrived && travelMode == TravelMode.Once) return;
            if (!PreparePath())
            {
                Debug.LogWarning("PathMovingBlock needs a valid MovementPath (two points or a circle).", this);
                return;
            }
            if (travelMode == TravelMode.Loop && !path.IsClosed)
            {
                Debug.LogWarning("PathMovingBlock Loop requires a closed path or Circle. Use PingPong for an open path.", this);
                return;
            }
            isMoving = true;
        }

        private void OnDisable()
        {
            // Retain traversal state for re-enable, but never let the last kinematic velocity drift.
            if (body != null) body.linearVelocity = Vector2.zero;
        }

        public void ResetBlock()
        {
            if (body == null) body = GetComponent<Rigidbody2D>();
            isMoving = false;
            hasArrived = false;
            travelledDistance = 0f;
            direction = 1;
            waitRemaining = 0f;
            if (!PreparePath()) return;
            body.position = path.StartPosition;
            body.linearVelocity = Vector2.zero;
        }

        private bool PreparePath()
        {
            if (body == null) body = GetComponent<Rigidbody2D>();
            if (path == null || body == null) return false;
            path.Rebuild();
            if (!path.IsValid) return false;
            if (snapToPathStart && !isMoving && !hasArrived)
            {
                travelledDistance = 0f;
                direction = 1;
                waitRemaining = 0f;
                body.position = path.StartPosition;
            }
            return true;
        }

        private void ConfigureBody()
        {
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
    }
}
