using UnityEngine;

namespace EmberPrototype
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(SpriteRenderer))]
    public sealed class EmberProjectile : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float lifetime = 2.5f;

        private readonly RaycastHit2D[] castHits = new RaycastHit2D[16];
        private Rigidbody2D body;
        private CircleCollider2D hitbox;
        private Transform owner;
        private Vector2 direction;
        private float speed;
        private float remainingDistance;
        private float remainingLifetime;
        private bool launched;

        public static EmberProjectile Spawn(
            EmberProjectile prefab,
            Vector2 position,
            Vector2 direction,
            float speed,
            float range,
            float radius,
            Transform owner)
        {
            EmberProjectile projectile;
            if (prefab != null)
            {
                projectile = Instantiate(prefab, position, Quaternion.identity);
            }
            else
            {
                GameObject shot = new GameObject("Ring Flame Shot");
                shot.transform.position = position;
                shot.transform.localScale = Vector3.one * (Mathf.Max(0.05f, radius) * 2f);
                PrototypeSprite visual = shot.AddComponent<PrototypeSprite>();
                visual.Color = new Color(1f, 0.48f, 0.08f);
                shot.GetComponent<SpriteRenderer>().sortingOrder = 10;
                shot.AddComponent<Rigidbody2D>();
                shot.AddComponent<CircleCollider2D>();
                projectile = shot.AddComponent<EmberProjectile>();
            }

            projectile.Launch(direction.normalized * speed, range, owner);
            return projectile;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            hitbox = GetComponent<CircleCollider2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            hitbox.isTrigger = true;
        }

        public void Launch(Vector2 velocity)
        {
            Launch(velocity, float.PositiveInfinity, null);
        }

        public void Launch(Vector2 velocity, float range, Transform shotOwner)
        {
            direction = velocity.normalized;
            speed = velocity.magnitude;
            remainingDistance = Mathf.Max(0f, range);
            remainingLifetime = lifetime;
            owner = shotOwner;
            launched = speed > 0f && remainingDistance > 0f;
            if (!launched) Destroy(gameObject);
        }

        private void FixedUpdate()
        {
            if (!launched) return;

            remainingLifetime -= Time.fixedDeltaTime;
            if (remainingLifetime <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            float stepDistance = Mathf.Min(speed * Time.fixedDeltaTime, remainingDistance);
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(Physics2D.AllLayers);
            filter.useTriggers = true;
            int hitCount = hitbox.Cast(direction, filter, castHits, stepDistance);
            float nearestDistance = float.PositiveInfinity;
            FlammableTile nearestFire = null;

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D other = castHits[i].collider;
                if (other == null || IsOwnerCollider(other)) continue;

                FlammableTile fire = other.GetComponentInParent<FlammableTile>();
                if (fire != null && fire.IsBurning && other.isTrigger) continue;
                if (fire == null && other.isTrigger) continue;
                if (castHits[i].distance >= nearestDistance) continue;

                nearestDistance = castHits[i].distance;
                nearestFire = fire;
            }

            if (nearestDistance < float.PositiveInfinity)
            {
                nearestFire?.TryIgnite();
                Destroy(gameObject);
                return;
            }

            body.MovePosition(body.position + direction * stepDistance);
            remainingDistance -= stepDistance;
            if (remainingDistance <= 0f) Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!launched || IsOwnerCollider(other)) return;

            FlammableTile fire = other.GetComponentInParent<FlammableTile>();
            if (fire != null && fire.IsBurning && other.isTrigger) return;
            if (fire == null && other.isTrigger) return;

            fire?.TryIgnite();
            Destroy(gameObject);
        }

        private bool IsOwnerCollider(Collider2D other)
        {
            return owner != null && other.transform.IsChildOf(owner);
        }
    }
}
