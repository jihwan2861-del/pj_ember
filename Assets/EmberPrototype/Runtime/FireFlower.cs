using System.Collections.Generic;
using UnityEngine;

namespace EmberPrototype
{
    [DisallowMultipleComponent]
    public sealed class FireFlower : MonoBehaviour
    {
        private static readonly List<FireFlower> activeFlowers = new List<FireFlower>();
        private static Sprite fallbackWilted, fallbackBlooming;
        [SerializeField, Min(0.1f)] private float reactionRadius = 3f;
        [SerializeField, Min(0.05f)] private float warningTime = 1f;
        [SerializeField, Min(0.1f)] private float projectileSpeed = 10f;
        [SerializeField, Min(0.1f)] private float projectileRange = 14f;
        [SerializeField, Min(0.01f)] private float projectileRadius = 0.14f;
        [SerializeField] private bool oneShot = true;
        [SerializeField, Min(0.05f)] private float cooldown = 1.5f;
        [SerializeField] private Transform muzzle;
        [SerializeField] private SpriteRenderer visual;
        [SerializeField] private Sprite wiltedSprite;
        [SerializeField] private Sprite bloomingSprite;
        private readonly List<GameObject> dots = new List<GameObject>();
        private Vector2 shotOrigin;
        private Vector2 shotDirection;
        private float remaining;
        private float cooldownRemaining;
        private bool pending;
        private bool activated;
        public Vector2 LockedTarget { get; private set; }
        public bool IsPreparingShot => pending;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRegistry() => activeFlowers.Clear();

        private void Awake()
        {
            if (visual == null) visual = GetComponent<SpriteRenderer>();
            if (visual == null) visual = gameObject.AddComponent<SpriteRenderer>();
            if (fallbackWilted == null) fallbackWilted = CreateFlowerSprite(false);
            if (fallbackBlooming == null) fallbackBlooming = CreateFlowerSprite(true);
            if (wiltedSprite == null) wiltedSprite = fallbackWilted;
            if (bloomingSprite == null) bloomingSprite = fallbackBlooming;
            visual.sprite = wiltedSprite;
        }
        private void OnEnable() { if (!activeFlowers.Contains(this)) activeFlowers.Add(this); }
        private void OnDisable()
        {
            activeFlowers.Remove(this);
            pending = false;
            HideWarning();
        }
        public static void ReactToBurst(FlamePlayerController player)
        {
            for (int i = activeFlowers.Count - 1; i >= 0; i--)
            {
                FireFlower flower = activeFlowers[i];
                if (flower != null && flower.gameObject.scene == player.gameObject.scene)
                    flower.TryActivate(player.transform.position);
            }
        }
        public bool TryActivate(Vector2 playerPosition)
        {
            if (!isActiveAndEnabled || pending || cooldownRemaining > 0f || (oneShot && activated)
                || (playerPosition - (Vector2)transform.position).sqrMagnitude > reactionRadius * reactionRadius)
                return false;
            shotOrigin = muzzle != null ? (Vector2)muzzle.position : (Vector2)transform.position;
            Vector2 delta = playerPosition - shotOrigin;
            if (delta.sqrMagnitude < 0.0001f) return false;
            LockedTarget = playerPosition;
            shotDirection = delta.normalized;
            activated = pending = true;
            remaining = warningTime;
            visual.sprite = bloomingSprite;
            ShowWarning(Vector2.Distance(shotOrigin, LockedTarget));
            return true;
        }
        private void Update()
        {
            cooldownRemaining = Mathf.Max(0f, cooldownRemaining - Time.deltaTime);
            if (!pending) return;
            remaining -= Time.deltaTime;
            if (remaining > 0f) return;
            pending = false;
            cooldownRemaining = cooldown;
            HideWarning();
            FireFlowerProjectile.Spawn(shotOrigin, shotDirection, projectileSpeed, projectileRange,
                projectileRadius, transform);
        }
        public void ResetForRespawn()
        {
            pending = activated = false;
            cooldownRemaining = remaining = 0f;
            LockedTarget = Vector2.zero;
            HideWarning();
            if (visual != null) visual.sprite = wiltedSprite;
        }
        private void ShowWarning(float distance)
        {
            int count = Mathf.Clamp(Mathf.CeilToInt(distance / 0.22f), 1, 64);
            for (int i = 0; i < count; i++)
            {
                if (i >= dots.Count)
                {
                    var dot = new GameObject("Locked aim dot");
                    dot.transform.SetParent(transform, false);
                    dot.AddComponent<PrototypeSprite>().Color = new Color(1f, 0.6f, 0.12f);
                    dot.GetComponent<SpriteRenderer>().sortingOrder = 30;
                    dots.Add(dot);
                }
                dots[i].transform.position = Vector2.Lerp(shotOrigin, LockedTarget, (i + 1f) / count);
                dots[i].transform.localScale = Vector3.one * (i == count - 1 ? 0.14f : 0.055f);
                dots[i].SetActive(true);
            }
        }
        private void HideWarning() { foreach (GameObject dot in dots) if (dot != null) dot.SetActive(false); }
        private static Sprite CreateFlowerSprite(bool bloom)
        {
            var texture = new Texture2D(16, 16) { filterMode = FilterMode.Point, hideFlags = HideFlags.HideAndDontSave };
            Color stem = new Color(0.45f, 0.36f, 0.16f);
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                {
                    Color color = Color.clear;
                    if ((x == 7 || x == 8) && y < 9) color = stem;
                    if (bloom)
                    {
                        int dx = x - 8, dy = y - 10;
                        if ((Mathf.Abs(dx) <= 2 && Mathf.Abs(dy) <= 5)
                            || (Mathf.Abs(dx) <= 5 && Mathf.Abs(dy) <= 2))
                            color = new Color(1f, 0.29f, 0.04f);
                        if (dx * dx + dy * dy <= 5) color = new Color(1f, 0.9f, 0.3f);
                    }
                    else if (y >= 6 && y <= 10 && x >= 5 && x <= 11)
                        color = new Color(0.46f, 0.34f, 0.24f);
                    texture.SetPixel(x, y, color);
                }
            texture.Apply();
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.2f), 16f);
            sprite.hideFlags = HideFlags.HideAndDontSave;
            return sprite;
        }
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.55f, 0.1f, 0.7f);
            Gizmos.DrawWireSphere(transform.position, reactionRadius);
        }
    }
}
