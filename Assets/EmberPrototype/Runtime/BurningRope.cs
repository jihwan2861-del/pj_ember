using UnityEngine;

namespace EmberPrototype
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Ember Prototype/Burning Rope")]
    public sealed class BurningRope : MonoBehaviour
    {
        [Header("고정점과 끝")]
        [SerializeField] private FlammableTile endFire;
        [SerializeField, Min(0.1f)] private float length = 3f;
        [SerializeField] private float initialAngle;
        [SerializeField] private float initialAngularSpeed;
        [Header("스윙")]
        [SerializeField, Min(0f)] private float gravity = 18f;
        [SerializeField, Min(0f)] private float inputAcceleration = 14f;
        [SerializeField, Min(0f)] private float damping = 0.08f;
        [SerializeField, Min(1f)] private float maximumEndSpeed = 24f;
        [SerializeField, Range(0f, 1f)] private float entryTransfer = 1f;
        [SerializeField, Min(0f)] private float recoil = 0.65f;
        [Header("표현")]
        [SerializeField, Range(4, 32)] private int segments = 16;
        [SerializeField, Min(0f)] private float bendLimit = 0.14f;
        [SerializeField, Min(0.01f)] private float followTime = 0.09f;
        [SerializeField, Min(0.001f)] private float width = 0.045f;
        [SerializeField] private Material ropeMaterial;

        private float angle;
        private float angularSpeed;
        private bool initialized;
        private FlamePlayerController occupant;
        private LineRenderer line;
        private Material ownedMaterial;
        private Vector3[] points;
        private Vector2 lag;
        private Vector2 previousEnd;

        public FlammableTile EndFire => endFire;
        public float Angle => angle;
        public float Length => Mathf.Max(0.1f, length);
        public float MaximumEndSpeed => Mathf.Max(1f, maximumEndSpeed);
        public Vector2 Pivot => transform.position;
        public Vector2 Tangent => new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        public Vector2 EndPosition => Pivot + new Vector2(Mathf.Sin(angle), -Mathf.Cos(angle)) * Length;
        public Vector2 EndVelocity => Tangent * (angularSpeed * Length);
        public bool IsAvailable => isActiveAndEnabled && endFire != null && endFire.isActiveAndEnabled && endFire.IsBurning;

        public void Configure(FlammableTile tip, float ropeLength, float startAngle = 0f, float startAngularSpeed = 0f)
        {
            endFire = tip;
            length = Mathf.Max(0.1f, ropeLength);
            initialAngle = startAngle;
            initialAngularSpeed = startAngularSpeed;
            ResetMotion();
        }

        private void Awake() => Initialize();

        private bool Initialize()
        {
            if (initialized) return true;
            if (endFire == null) return false;
            ResetMotion();
            return true;
        }

        public void ResetMotion()
        {
            occupant = null;
            angle = initialAngle * Mathf.Deg2Rad;
            angularSpeed = Mathf.Clamp(initialAngularSpeed * Mathf.Deg2Rad,
                -MaximumEndSpeed / Length, MaximumEndSpeed / Length);
            initialized = endFire != null;
            lag = Vector2.zero;
            PlaceEnd();
            previousEnd = EndPosition;
        }

        private void FixedUpdate()
        {
            // Occupied ropes are advanced explicitly by the player before following their end.
            if (Initialize() && occupant == null) Advance(Time.fixedDeltaTime, 0f);
        }

        public void Advance(float deltaTime, float horizontalInput)
        {
            if (deltaTime <= 0f || !Initialize()) return;
            float acceleration = -gravity / Length * Mathf.Sin(angle)
                + Mathf.Clamp(horizontalInput, -1f, 1f) * inputAcceleration / Length * Mathf.Cos(angle);
            angularSpeed = (angularSpeed + acceleration * deltaTime) * Mathf.Exp(-damping * deltaTime);
            angularSpeed = Mathf.Clamp(angularSpeed, -MaximumEndSpeed / Length, MaximumEndSpeed / Length);
            angle = Mathf.Repeat(angle + angularSpeed * deltaTime + Mathf.PI, Mathf.PI * 2f) - Mathf.PI;
            PlaceEnd();
        }

        internal bool Attach(FlamePlayerController player, Vector2 incomingVelocity)
        {
            if (!IsAvailable || !Initialize() || (occupant != null && occupant != player)) return false;
            occupant = player;
            float incomingAngularSpeed = Vector2.Dot(incomingVelocity, Tangent) / Length;
            angularSpeed = Mathf.Clamp(Mathf.Lerp(angularSpeed, incomingAngularSpeed, entryTransfer),
                -MaximumEndSpeed / Length, MaximumEndSpeed / Length);
            return true;
        }

        internal void Detach(FlamePlayerController player, Vector2 additionalImpulse)
        {
            if (occupant != player) return;
            occupant = null;
            angularSpeed -= Vector2.Dot(additionalImpulse, Tangent) * recoil / Length;
            angularSpeed = Mathf.Clamp(angularSpeed, -MaximumEndSpeed / Length, MaximumEndSpeed / Length);
        }

        internal void StopAt(float safeAngle)
        {
            angle = safeAngle;
            angularSpeed = 0f;
            PlaceEnd();
        }

        private void PlaceEnd()
        {
            if (endFire != null)
            {
                Vector2 position = EndPosition;
                endFire.transform.position = new Vector3(position.x, position.y, transform.position.z);
            }
        }

        private void LateUpdate() => UpdateVisual(Time.deltaTime);

        public void UpdateVisual(float deltaTime)
        {
            if (!Initialize()) return;
            if (line == null)
            {
                GameObject visual = new GameObject("Rope Line");
                visual.transform.SetParent(transform, false);
                line = visual.AddComponent<LineRenderer>();
                line.useWorldSpace = true;
                line.numCornerVertices = 3;
                line.sortingOrder = 5;
                if (ropeMaterial != null) line.sharedMaterial = ropeMaterial;
                else
                {
                    Shader shader = Shader.Find("Sprites/Default");
                    if (shader != null) line.sharedMaterial = ownedMaterial = new Material(shader);
                }
            }
            int count = Mathf.Clamp(segments, 4, 32) + 1;
            if (points == null || points.Length != count) points = new Vector3[count];
            Vector2 end = EndPosition;
            lag = Vector2.ClampMagnitude(lag - (end - previousEnd), Mathf.Min(bendLimit, Length * 0.05f));
            lag *= Mathf.Exp(-Mathf.Max(0f, deltaTime) / Mathf.Max(0.01f, followTime));
            previousEnd = end;
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / (count - 1);
                Vector2 p = Vector2.Lerp(Pivot, end, t) + lag * Mathf.Sin(t * Mathf.PI);
                points[i] = new Vector3(p.x, p.y, transform.position.z);
            }
            line.positionCount = count;
            line.SetPositions(points);
            line.widthMultiplier = width;
            line.startColor = line.endColor = endFire.IsBurning
                ? new Color(1f, 0.45f, 0.08f) : new Color(0.35f, 0.28f, 0.21f);
        }

        private void OnDisable()
        {
            if (line != null) line.enabled = false;
        }

        private void OnEnable()
        {
            if (line != null) line.enabled = true;
        }

        private void OnDestroy()
        {
            if (ownedMaterial != null)
            {
                if (Application.isPlaying) Destroy(ownedMaterial);
                else DestroyImmediate(ownedMaterial);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Vector2 end = Application.isPlaying ? EndPosition
                : Pivot + new Vector2(Mathf.Sin(initialAngle * Mathf.Deg2Rad), -Mathf.Cos(initialAngle * Mathf.Deg2Rad)) * Length;
            Gizmos.color = new Color(1f, 0.5f, 0.1f);
            Gizmos.DrawLine(Pivot, end);
            Gizmos.DrawWireSphere(end, 0.2f);
        }
    }
}
