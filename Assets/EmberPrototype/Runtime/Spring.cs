using UnityEngine;
using UnityEngine.Events;

namespace EmberPrototype
{
    [AddComponentMenu("Ember Prototype/Spring")]
    [RequireComponent(typeof(BoxCollider2D))]
    [DisallowMultipleComponent]
    public sealed class Spring : MonoBehaviour
    {
        [Tooltip("플레이어를 위로 튕기는 속도입니다. (월드 단위/초)")]
        [SerializeField, Min(0.1f)] private float bounceSpeed = 10f;
        [Tooltip("스프링 윗면 판정의 여유입니다. 옆면이나 아래쪽 접촉은 튕기지 않습니다.")]
        [SerializeField, Min(0f)] private float topTolerance = 0.08f;
        [SerializeField] private UnityEvent onBounce = new UnityEvent();

        private BoxCollider2D trigger;

        private void Awake() => ConfigureTrigger();
        private void Reset() => ConfigureTrigger();

        private void ConfigureTrigger()
        {
            trigger = GetComponent<BoxCollider2D>();
            if (trigger != null) trigger.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other) => TryBounce(other);
        private void OnTriggerStay2D(Collider2D other) => TryBounce(other);

        private void TryBounce(Collider2D other)
        {
            if (!isActiveAndEnabled || other == null || trigger == null || !trigger.enabled) return;
            FlamePlayerController player = other.GetComponentInParent<FlamePlayerController>();
            if (player == null || other.attachedRigidbody == null) return;
            // Account for one physics step of descent so a fast fall cannot miss the top.
            float previousFeet = other.bounds.min.y
                - Mathf.Min(0f, other.attachedRigidbody.linearVelocity.y) * Time.fixedDeltaTime;
            if (other.bounds.center.y < trigger.bounds.center.y
                || previousFeet < trigger.bounds.center.y - topTolerance) return;
            if (player.TrySpringBounce(bounceSpeed)) onBounce.Invoke();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.7f, 0.15f);
            Vector3 start = transform.position;
            Vector3 end = start + Vector3.up * 0.8f;
            Gizmos.DrawLine(start, end);
            Gizmos.DrawLine(end, end + new Vector3(-0.15f, -0.2f));
            Gizmos.DrawLine(end, end + new Vector3(0.15f, -0.2f));
        }
    }
}
