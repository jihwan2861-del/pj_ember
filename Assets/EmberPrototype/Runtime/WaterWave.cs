using UnityEngine;

namespace EmberPrototype
{
    [AddComponentMenu("Ember Prototype/Water Wave")]
    [RequireComponent(typeof(Rigidbody2D), typeof(PolygonCollider2D), typeof(WaterContact))]
    public sealed class WaterWave : MonoBehaviour
    {
        public enum TravelDirection { LeftToRight, RightToLeft }
        [Header("파도 이동")]
        [SerializeField, InspectorName("진행 방향")] private TravelDirection direction;
        [SerializeField, Min(0.1f), InspectorName("파도 기준 높이")] private float height = 2.65f;
        [SerializeField, Min(0.1f), InspectorName("파도 기준 너비")] private float width = 1.2f;
        [SerializeField, Min(0.1f), InspectorName("이동 속도")] private float speed = 4f;
        [SerializeField, Min(0.1f), InspectorName("이동 거리")] private float travelDistance = 18f;
        [SerializeField, InspectorName("시작하면 자동 발사")] private bool playOnEnable = true;
        [SerializeField, InspectorName("반복 발사")] private bool repeat = true;
        [SerializeField, Min(0f), InspectorName("반복 대기 시간")] private float repeatDelay = 1f;
        [SerializeField, InspectorName("닿으면 사망")] private bool deadly = true;
        [Header("그림 설정 (판정과 별개)")]
        [SerializeField, InspectorName("파도 그림 연결")]
        [Tooltip("Wave Visual의 Sprite Renderer를 넣으세요. 비어 있으면 그림이 하나일 때 자동 연결합니다.")]
        private SpriteRenderer visual;
        [SerializeField, InspectorName("그림 크기 배율")]
        [Tooltip("X는 너비, Y는 높이입니다. 1은 기본 크기, 2는 두 배입니다. 충돌 판정은 바뀌지 않습니다.")]
        private Vector2 visualSizeMultiplier = Vector2.one;
        [SerializeField, InspectorName("그림 위치 조절")]
        [Tooltip("X: 오른쪽 + / 왼쪽 -. Y: 위 + / 아래 -. 왼쪽 공격에서는 X가 자동 반전됩니다.")]
        private Vector2 visualOffset;
        [SerializeField, InspectorName("그림 바닥 자동 맞춤")]
        [Tooltip("켜 두면 스프라이트 피벗이 중앙이어도 그림의 아래 중앙을 파도 기준점에 맞춥니다.")]
        private bool alignVisualBottom = true;
        private Rigidbody2D body;
        private PolygonCollider2D waveCollider;
        private RoomHazard hazard;
        private WaterContact waterContact;
        private Vector2 origin;
        private Vector2 initialOrigin;
        private float travelled, waiting;
        private bool repeatPending;
        private bool waitingForRespawn;
        private Vector2[][] authoredPaths;
        private Vector2 authoredColliderOffset;
        private float authoredWidth, authoredHeight;
        public bool IsMoving { get; private set; }
        public float WorldHeight => height * Mathf.Abs(transform.lossyScale.y);

        public WaterSpiritAttack DescribeAttack(Vector2 start, TravelDirection travelDirection) =>
            new WaterSpiritAttack(start, travelDirection, WorldHeight,
                width * Mathf.Abs(transform.lossyScale.x), speed, travelDistance, deadly);

        public void Launch(WaterSpiritAttack attack)
        {
            // Freeze template settings in world units; warning and collision use this same snapshot.
            transform.localScale = Vector3.one;
            transform.rotation = Quaternion.identity;
            height = attack.Height; width = attack.Width; speed = attack.Speed;
            travelDistance = attack.Distance; deadly = attack.Deadly;
            Launch(attack.Start, attack.Direction);
        }

        public void Launch(Vector2 start, TravelDirection travelDirection)
        {
            direction = travelDirection;
            playOnEnable = false;
            repeat = false;
            PlayAt(start);
        }

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
            waterContact = GetComponent<WaterContact>();
            if (hazard == null) hazard = gameObject.AddComponent<RoomHazard>();
            origin = body.position;
            initialOrigin = origin;
            body.bodyType = RigidbodyType2D.Kinematic; body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            CaptureAuthoredCollider();
            RefreshShape();
        }
        private void OnEnable() { if (Application.isPlaying) { if (playOnEnable) Play(); else Stop(); } }
        private void OnDisable() { if (Application.isPlaying) Stop(); }
        private void OnValidate()
        {
            visualSizeMultiplier.x = Mathf.Max(0.01f, visualSizeMultiplier.x);
            visualSizeMultiplier.y = Mathf.Max(0.01f, visualSizeMultiplier.y);
            // Never rewrite Edit Collider data while authoring or re-opening a prefab.
            RefreshVisual();
        }
        private void LateUpdate()
        {
            // Animation may swap sprites with different pivots/bounds each frame.
            if (visual != null) RefreshVisual();
        }

        public void ResetWave(bool deferStart = false)
        {
            Stop();
            origin = initialOrigin;
            travelled = waiting = 0f;
            if (body == null) return;
            body.position = origin;
            body.linearVelocity = Vector2.zero;
            waitingForRespawn = deferStart && playOnEnable && isActiveAndEnabled;
            if (!deferStart && playOnEnable && isActiveAndEnabled) Play();
        }

        public void ResumeAfterRespawn()
        {
            if (!waitingForRespawn) return;
            waitingForRespawn = false;
            if (isActiveAndEnabled) Play();
        }

        public void PlayAt(Vector2 start)
        {
            origin = start; Play();
        }
        public void Play()
        {
            waitingForRespawn = false;
            if (body == null) return;
            RefreshShape(); body.position = origin;
            travelled = waiting = 0f; repeatPending = false; IsMoving = true;
            waveCollider.enabled = true; hazard.enabled = deadly;
            if (visual != null) visual.enabled = true;
            waterContact.SetContactActive(true);
        }
        public void Stop()
        {
            if (waterContact != null) waterContact.SetContactActive(false);
            waitingForRespawn = false;
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
            Vector2 destination = origin + Vector2.right * (sign * travelled);
            waterContact.ExtinguishSweep(destination - body.position);
            body.MovePosition(destination);
        }
        public void RefreshShape()
        {
            if (authoredPaths != null && waveCollider != null)
            {
                float sign = direction == TravelDirection.LeftToRight ? 1f : -1f;
                float scaleX = width / authoredWidth, scaleY = height / authoredHeight;
                waveCollider.isTrigger = true;
                waveCollider.pathCount = authoredPaths.Length;
                for (int path = 0; path < authoredPaths.Length; path++)
                {
                    Vector2[] points = new Vector2[authoredPaths[path].Length];
                    for (int i = 0; i < points.Length; i++)
                        points[i] = new Vector2(authoredPaths[path][i].x * sign * scaleX,
                            authoredPaths[path][i].y * scaleY);
                    waveCollider.SetPath(path, points);
                }
                waveCollider.offset = new Vector2(authoredColliderOffset.x * sign * scaleX,
                    authoredColliderOffset.y * scaleY);
            }
            RefreshVisual();
        }

        private void CaptureAuthoredCollider()
        {
            // Treat the saved, hand-edited shape as the right-facing source. Never
            // mirror an already mirrored runtime shape on the next pooled launch.
            if (waveCollider.pathCount == 0 || waveCollider.GetPath(0).Length < 3)
            {
                Vector2[] fallback = Outline;
                for (int i = 0; i < fallback.Length; i++)
                    fallback[i] = new Vector2(fallback[i].x * width, fallback[i].y * height);
                waveCollider.pathCount = 1;
                waveCollider.SetPath(0, fallback);
            }
            authoredPaths = new Vector2[waveCollider.pathCount][];
            for (int i = 0; i < authoredPaths.Length; i++) authoredPaths[i] = waveCollider.GetPath(i);
            authoredColliderOffset = waveCollider.offset;
            authoredWidth = Mathf.Max(0.01f, width);
            authoredHeight = Mathf.Max(0.01f, height);
        }

        private void RefreshVisual()
        {
            if (visual == null)
            {
                SpriteRenderer[] candidates = GetComponentsInChildren<SpriteRenderer>(true);
                if (candidates.Length == 1) visual = candidates[0];
            }
            if (visual == null) return;
            float sign = direction == TravelDirection.LeftToRight ? 1f : -1f;
            visual.flipX = sign < 0f;
            // Scaling a renderer on the physics root would also scale the hitbox.
            // Existing Wave Visual child is therefore the supported authoring setup.
            if (visual.transform == transform) return;
            Bounds bounds = visual.localBounds;
            Vector3 scale = new Vector3(width * visualSizeMultiplier.x / Mathf.Max(0.001f, bounds.size.x),
                height * visualSizeMultiplier.y / Mathf.Max(0.001f, bounds.size.y), 1f);
            Vector3 position = new Vector3(visualOffset.x * sign, visualOffset.y, 0f);
            if (alignVisualBottom)
            {
                position.x -= bounds.center.x * scale.x;
                position.y -= bounds.min.y * scale.y;
            }
            visual.transform.localScale = scale;
            visual.transform.localPosition = position;
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


