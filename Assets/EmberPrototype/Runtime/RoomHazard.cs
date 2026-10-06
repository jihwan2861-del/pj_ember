using UnityEngine;

namespace EmberPrototype
{
    [AddComponentMenu("Ember Prototype/Death Hazard")]
    [DisallowMultipleComponent]
    public sealed class RoomHazard : MonoBehaviour
    {
        private Collider2D hazardCollider;

        private void Awake()
        {
            ConfigureCollider(true);
        }

        private void Reset()
        {
            ConfigureCollider(true);
        }

        private void OnValidate()
        {
            ConfigureCollider(false);
        }

        private void OnTriggerEnter2D(Collider2D other) => TryKill(other);

        private void OnCollisionEnter2D(Collision2D collision) => TryKill(collision.collider);

        private void TryKill(Collider2D other)
        {
            FlamePlayerController player = other.GetComponentInParent<FlamePlayerController>();
            if (player == null) return;

            player.KillAndRespawn();
        }

        private void ConfigureCollider(bool createIfMissing)
        {
            if (hazardCollider == null) hazardCollider = GetComponent<Collider2D>();
            if (hazardCollider == null && createIfMissing) hazardCollider = gameObject.AddComponent<BoxCollider2D>();
            if (hazardCollider != null) hazardCollider.isTrigger = true;
        }
    }
}
