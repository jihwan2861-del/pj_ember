using System;
using UnityEngine;

namespace EmberPrototype
{
    [AddComponentMenu("Ember Prototype/Weight Platform")]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public sealed class WeightPlatform : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float descendDistance = 2f;
        [SerializeField, Min(0f)] private float descendSpeed = 0.5f;
        [SerializeField, Min(0f)] private float returnSpeed = 0.5f;
        [SerializeField, Min(0.01f)] private float surfaceTolerance = 0.1f;
        [SerializeField] private LayerMask loadMask = Physics2D.AllLayers;
        private Rigidbody2D body;
        private BoxCollider2D surface;
        private Vector2 origin;
        private Collider2D[] hits = new Collider2D[16];
        public bool HasLoad { get; private set; }
        public Vector2 StepVelocity { get; private set; }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            surface = GetComponent<BoxCollider2D>();
            origin = body.position;
            if (GetComponent<PathMovingBlock>() != null)
            {
                Debug.LogError("WeightPlatform and PathMovingBlock must use separate objects.", this);
                enabled = false; return;
            }
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        private bool DetectLoad()
        {
            if (!surface.enabled || surface.isTrigger) return false;
            Bounds bounds = surface.bounds;
            Vector2 center = new Vector2(bounds.center.x, bounds.max.y + surfaceTolerance * 0.5f);
            Vector2 size = new Vector2(Mathf.Max(0.01f, bounds.size.x - 0.04f), surfaceTolerance * 2f);
            ContactFilter2D filter = new ContactFilter2D { useTriggers = false };
            filter.SetLayerMask(loadMask);
            int count;
            while ((count = Physics2D.OverlapBox(center, size, 0f, filter, hits)) == hits.Length)
                Array.Resize(ref hits, hits.Length * 2);
            for (int i = 0; i < count; i++)
            {
                Collider2D hit = hits[i];
                if (hit == null || hit.attachedRigidbody == body || hit.transform.IsChildOf(transform)) continue;
                if (hit.bounds.min.y >= bounds.max.y - surfaceTolerance) return true;
            }
            return false;
        }

        private void FixedUpdate()
        {
            HasLoad = DetectLoad();
            float target = origin.y - (HasLoad ? descendDistance : 0f);
            Vector2 destination = new Vector2(origin.x, Mathf.MoveTowards(body.position.y, target,
                (HasLoad ? descendSpeed : returnSpeed) * Time.fixedDeltaTime));
            StepVelocity = (destination - body.position) / Time.fixedDeltaTime;
            body.MovePosition(destination);
        }

        public void ResetForRespawn()
        {
            if (body == null) return;
            body.position = origin;
            body.linearVelocity = Vector2.zero;
            StepVelocity = Vector2.zero;
            HasLoad = false;
        }

        private void OnDisable()
        {
            StepVelocity = Vector2.zero;
            HasLoad = false;
            if (body != null) body.linearVelocity = Vector2.zero;
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 start = Application.isPlaying ? (Vector3)origin : transform.position;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(start, start + Vector3.down * descendDistance);
        }
    }
}
