using UnityEngine;

namespace EmberPrototype
{
    [AddComponentMenu("Ember Prototype/Water Wave")]
    [RequireComponent(typeof(Rigidbody2D), typeof(PolygonCollider2D))]
    public sealed class WaterWave : MonoBehaviour
    {
        public enum TravelDirection { LeftToRight, RightToLeft }
        [SerializeField] private TravelDirection direction;
        [SerializeField, Min(0.1f)] private float height = 2.65f;
        [SerializeField, Min(0.1f)] private float width = 1.2f;
        [SerializeField, Min(0.1f)] private float speed = 4f;
        [SerializeField, Min(0.1f)] private float travelDistance = 18f;
        [SerializeField] private bool playOnEnable = true;
        [SerializeField] private bool repeat = true;
        [SerializeField, Min(0f)] private float repeatDelay = 1f;
        [SerializeField] private bool deadly = true;
        [SerializeField] private SpriteRenderer visual;
        private Rigidbody2D body;
        private PolygonCollider2D waveCollider;
        private RoomHazard hazard;
        private Vector2 origin;
        private float travelled, waiting;
        private bool repeatPending;
        public bool IsMoving { get; private set; }

        // Shared outline keeps the visible wave and its concave collision shape aligned.
        public static Vector2[] Outline => new[] {
            new Vector2(-0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.32f, 0.5f),
            new Vector2(0.04f, 0.73f), new Vector2(0.04f, 0.86f), new Vector2(0.2f, 0.88f),
            new Vector2(0.34f, 0.8f), new Vector2(0.5f, 0.84f), new Vector2(0.45f, 0.94f),
            new Vector2(0.18f, 1f), new Vector2(-0.12f, 0.99f), new Vector2(-0.36f, 0.91f),
            new Vector2(-0.5f, 0.74f)
        };

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>(); waveCollider = GetComponent<PolygonCollider2D>();
            hazard = GetComponent<RoomHazard>();
            if (hazard == null) hazard = gameObject.AddComponent<RoomHazard>();
            origin = body.position;
            body.bodyType = RigidbodyType2D.Kinematic; body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            RefreshShape();
        }
        private void OnEnable() { if (Application.isPlaying) { if (playOnEnable) Play(); else Stop(); } }
        private void OnDisable() { if (Application.isPlaying) Stop(); }
        private void OnValidate() => RefreshShape();

        public void PlayAt(Vector2 start)
        {
            origin = start; Play();
        }
        public void Play()
        {
            if (body == null) return;
            RefreshShape(); body.position = origin;
            travelled = waiting = 0f; repeatPending = false; IsMoving = true;
            waveCollider.enabled = true; hazard.enabled = deadly;
            if (visual != null) visual.enabled = true;
        }
        public void Stop()
        {
            IsMoving = repeatPending = false;
            if (waveCollider != null) waveCollider.enabled = false;
            if (visual != null) visual.enabled = false;
        }
        private void FixedUpdate()
        {
            if (!IsMoving)
            {
                if (repeatPending && (waiting -= Time.fixedDeltaTime) <= 0f) Play();
                return;
            }
            // Leave the final movement step collidable until native physics has processed it.
            if (travelled >= travelDistance)
            {
                Stop(); repeatPending = repeat; waiting = repeatDelay; return;
            }
            travelled = Mathf.Min(travelDistance, travelled + speed * Time.fixedDeltaTime);
            float sign = direction == TravelDirection.LeftToRight ? 1f : -1f;
            body.MovePosition(origin + Vector2.right * (sign * travelled));
        }
        public void RefreshShape()
        {
            PolygonCollider2D collider = GetComponent<PolygonCollider2D>();
            float sign = direction == TravelDirection.LeftToRight ? 1f : -1f;
            Vector2[] points = Outline;
            for (int i = 0; i < points.Length; i++) points[i] = new Vector2(points[i].x * width * sign, points[i].y * height);
            if (collider != null) { collider.isTrigger = true; collider.pathCount = 1; collider.SetPath(0, points); }
            if (visual != null)
            {
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localScale = new Vector3(width, height, 1f);
                visual.flipX = sign < 0f;
            }
        }
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Vector3 start = Application.isPlaying ? (Vector3)origin : transform.position;
            Vector3 end = start + Vector3.right * (direction == TravelDirection.LeftToRight ? travelDistance : -travelDistance);
            Gizmos.DrawLine(start, end);
            Gizmos.DrawWireCube(end + Vector3.up * height / 2f, new Vector3(width, height, 0f));
        }
    }
}
