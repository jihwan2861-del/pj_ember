using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace EmberPrototype
{
    [AddComponentMenu("Ember/Player Event Trigger")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class PlayerEventTrigger : MonoBehaviour
    {
        [Tooltip("켜면 씬이 유지되는 동안 한 번만 실행합니다. 사망하거나 오브젝트를 껐다 켜도 유지됩니다.")]
        [SerializeField] private bool triggerOnce = true;
        [Tooltip("켜면 영역 안에서 안전한 바닥에 착지한 뒤 실행합니다. 기존 진입 트리거는 끈 상태를 유지합니다.")]
        [SerializeField] private bool requireGrounded;
        [Tooltip("선택 사항: 이 충돌체를 밟고 있을 때만 착지로 인정합니다.")]
        [SerializeField] private Collider2D landingSurface;
        [Tooltip("플레이어가 영역에 들어오면 호출합니다.")]
        [SerializeField] private UnityEvent onPlayerEntered = new UnityEvent();

        private readonly HashSet<Collider2D> contacts = new HashSet<Collider2D>();
        private bool firedForCurrentVisit;
        public bool HasTriggered { get; private set; }

        private void Reset() => GetComponent<BoxCollider2D>().isTrigger = true;

        private void Awake() => GetComponent<BoxCollider2D>().isTrigger = true;

        private void OnTriggerEnter2D(Collider2D other)
        {
            FlamePlayerController player = other.GetComponentInParent<FlamePlayerController>();
            if (!isActiveAndEnabled || player == null)
                return;

            contacts.RemoveWhere(contact => contact == null || !contact.enabled || !contact.gameObject.activeInHierarchy);
            bool alreadyInside = contacts.Count > 0;
            contacts.Add(other);
            if (alreadyInside) return;
            firedForCurrentVisit = false;
            TryInvoke(player);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!requireGrounded || !isActiveAndEnabled) return;
            FlamePlayerController player = other.GetComponentInParent<FlamePlayerController>();
            if (player == null) return;
            contacts.RemoveWhere(contact => contact == null || !contact.enabled || !contact.gameObject.activeInHierarchy);
            contacts.Add(other);
            if (!firedForCurrentVisit) TryInvoke(player);
        }

        private void TryInvoke(FlamePlayerController player)
        {
            if (firedForCurrentVisit || (triggerOnce && HasTriggered)) return;
            if (requireGrounded)
            {
                if (!player.CanConfirmCollectible) return;
                Rigidbody2D body = player.GetComponent<Rigidbody2D>();
                if (landingSurface != null && (body == null || !body.IsTouching(landingSurface))) return;
            }

            firedForCurrentVisit = true;
            HasTriggered = true;
            onPlayerEntered.Invoke();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            contacts.Remove(other);
            if (contacts.Count == 0) firedForCurrentVisit = false;
        }

        private void OnDisable()
        {
            contacts.Clear();
            firedForCurrentVisit = false;
        }

        // UnityEvent에서 연결해 일회 실행 여부를 다시 열 수 있습니다.
        public void ResetTrigger()
        {
            HasTriggered = false;
            firedForCurrentVisit = false;
        }
    }
}
