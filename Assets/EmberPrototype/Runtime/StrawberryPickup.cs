using UnityEngine;
using UnityEngine.Events;

namespace EmberPrototype
{
    [AddComponentMenu("Ember Prototype/Chicken Pickup")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CircleCollider2D))]
    public sealed class StrawberryPickup : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float confirmDelay = 0.3f;
        [SerializeField, Min(0.1f)] private float followSpeed = 8f;
        [SerializeField] private Vector2 followOffset = new Vector2(-0.65f, 0.6f);
        [SerializeField, Min(0f)] private float bobHeight = 0.08f;
        [SerializeField] private UnityEvent onCollected = new UnityEvent();
        private CircleCollider2D pickupCollider;
        private SpriteRenderer visual;
        private Vector3 origin, visualOrigin;
        private FlamePlayerController player;
        private StrawberryCollection collection;
        private float groundedTime;
        private float facing = 1f;
        private float lastPlayerX;
        public bool IsFollowing => collection != null;
        public bool IsCollected { get; private set; }
        public UnityEvent OnCollected => onCollected;

        private void Awake()
        {
            origin = transform.position;
            pickupCollider = GetComponent<CircleCollider2D>();
            pickupCollider.isTrigger = true;
            visual = GetComponentInChildren<SpriteRenderer>();
            if (visual != null) visualOrigin = visual.transform.localPosition;
        }
        private void OnTriggerEnter2D(Collider2D other) => TryPickup(other.GetComponentInParent<FlamePlayerController>());
        private void OnTriggerStay2D(Collider2D other) => TryPickup(other.GetComponentInParent<FlamePlayerController>());
        public bool TryPickup(FlamePlayerController candidate)
        {
            if (IsCollected || IsFollowing || candidate == null || !candidate.isActiveAndEnabled
                || candidate.IsDying || candidate.ControlsLocked) return false;
            player = candidate;
            collection = player.GetComponent<StrawberryCollection>();
            if (collection == null) collection = player.gameObject.AddComponent<StrawberryCollection>();
            collection.Attach(this);
            lastPlayerX = player.transform.position.x;
            groundedTime = 0f;
            pickupCollider.enabled = false;
            return true;
        }
        private void Update()
        {
            if (!IsFollowing) return;
            if (player == null || !player.isActiveAndEnabled || player.IsDying)
            {
                ResetForRespawn();
                return;
            }
            groundedTime = player.CanConfirmCollectible ? groundedTime + Time.deltaTime : 0f;
            if (groundedTime < confirmDelay) return;
            IsCollected = true;
            StrawberryCollection owner = collection;
            collection = null;
            player = null;
            if (visual != null) visual.enabled = false;
            owner.Confirm(this);
            onCollected.Invoke();
        }
        private void LateUpdate()
        {
            if (IsCollected) return;
            if (IsFollowing && player != null)
            {
                float x = player.transform.position.x;
                if (Mathf.Abs(x - lastPlayerX) > 0.001f) facing = Mathf.Sign(x - lastPlayerX);
                lastPlayerX = x;
                Transform leader = collection.LeaderFor(this);
                Vector3 offset = new Vector3(followOffset.x * facing, leader == player.transform ? followOffset.y : 0f, 0f);
                transform.position = Vector3.Lerp(transform.position, leader.position + offset, 1f - Mathf.Exp(-followSpeed * Time.deltaTime));
            }
            if (visual != null && visual.transform != transform)
                visual.transform.localPosition = visualOrigin + Vector3.up * (Mathf.Sin(Time.time * 3f) * bobHeight);
        }
        public void ResetForRespawn()
        {
            if (IsCollected) return;
            if (collection != null) collection.Detach(this);
            collection = null;
            player = null;
            groundedTime = 0f;
            facing = 1f;
            transform.position = origin;
            if (pickupCollider != null) pickupCollider.enabled = true;
            if (visual != null) visual.enabled = true;
        }
        private void OnDisable()
        {
            if (IsFollowing) ResetForRespawn();
        }
    }
}
