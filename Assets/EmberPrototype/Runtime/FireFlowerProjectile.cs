using UnityEngine;

namespace EmberPrototype
{
    public sealed class FireFlowerProjectile : MonoBehaviour
    {
        private readonly RaycastHit2D[] hits = new RaycastHit2D[64];
        private Vector2 direction;
        private float speed, radius, remainingRange;
        private Transform owner;
        private bool stopped;
        [SerializeField] private Sprite[] animationFrames;
        [SerializeField, Min(1f)] private float framesPerSecond = 12f;
        private float animationTime;
        private void Update()
        {
            if (animationFrames == null || animationFrames.Length == 0) return;
            animationTime += Time.deltaTime;
            GetComponent<SpriteRenderer>().sprite = animationFrames[(int)(animationTime * framesPerSecond) % animationFrames.Length];
        }
        public static FireFlowerProjectile Spawn(Vector2 origin, Vector2 direction, float speed,
            float range, float radius, Transform owner, FireFlowerProjectile prefab = null)
        {
            var root = prefab != null ? Instantiate(prefab.gameObject) : new GameObject("Flower purification flame");
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root, owner.gameObject.scene);
            root.transform.position = origin;
            if (prefab == null)
            {
                root.transform.localScale = Vector3.one * (radius * 2f);
                root.AddComponent<PrototypeSprite>().Color = new Color(1f, 0.78f, 0.15f);
            }
            root.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            root.GetComponent<SpriteRenderer>().sortingOrder = 40;
            var shot = root.GetComponent<FireFlowerProjectile>();
            if (shot == null) shot = root.AddComponent<FireFlowerProjectile>();
            shot.direction = direction.normalized;
            shot.speed = Mathf.Max(0.1f, speed);
            shot.radius = Mathf.Max(0.01f, radius);
            shot.remainingRange = Mathf.Max(0.1f, range);
            shot.owner = owner;
            return shot;
        }
        private void FixedUpdate()
        {
            if (stopped) return;
            float distance = Mathf.Min(speed * Time.fixedDeltaTime, remainingRange);
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(Physics2D.AllLayers);
            filter.useTriggers = true;
            int count = Physics2D.CircleCast(transform.position, radius, direction, filter, hits, distance);
            int nearest = -1;
            float nearestDistance = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                Collider2D hit = hits[i].collider;
                if (hit == null || (owner != null && hit.transform.IsChildOf(owner))) continue;
                if (hit.GetComponentInParent<FlamePlayerController>() != null) continue;
                bool relevant = !hit.isTrigger || hit.GetComponentInParent<PollutionObstacle>() != null
                    || hit.GetComponentInParent<PurifiableSpikeTilemap>() != null;
                if (relevant && hits[i].distance < nearestDistance)
                { nearest = i; nearestDistance = hits[i].distance; }
            }
            if (nearest >= 0)
            {
                stopped = true;
                Collider2D hit = hits[nearest].collider;
                PollutionObstacle pollution = hit.GetComponentInParent<PollutionObstacle>();
                if (pollution != null) pollution.TryPurifyFromFlower();
                else hit.GetComponentInParent<PurifiableSpikeTilemap>()?.TryPurifyAt(hits[nearest].point, direction);
                Retire();
                return;
            }
            transform.position += (Vector3)(direction * distance);
            remainingRange -= distance;
            if (remainingRange <= 0f) Retire();
        }
        private void Retire()
        {
            stopped = true;
            gameObject.SetActive(false);
            if (Application.isPlaying) Destroy(gameObject);
            else DestroyImmediate(gameObject);
        }
    }
}
