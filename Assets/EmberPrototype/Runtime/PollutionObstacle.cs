using UnityEngine;
using UnityEngine.Events;

namespace EmberPrototype
{
    [DisallowMultipleComponent]
    public sealed class PollutionObstacle : MonoBehaviour
    {
        [Tooltip("Only FireFlowerProjectile can invoke this purification condition.")]
        [SerializeField] private bool acceptsFlowerFire = true;
        [SerializeField] private GameObject purifiedVisual;
        [SerializeField] private UnityEvent onPurified = new UnityEvent();
        private Renderer[] visuals;
        private Collider2D[] colliders;
        private bool[] originalVisualStates;
        private bool[] originalColliderStates;
        public bool IsPurified { get; private set; }

        private void Awake() => Initialize();
        private void Initialize()
        {
            if (visuals != null) return;
            visuals = GetComponentsInChildren<Renderer>(true);
            colliders = GetComponentsInChildren<Collider2D>(true);
            originalVisualStates = new bool[visuals.Length];
            originalColliderStates = new bool[colliders.Length];
            for (int i = 0; i < visuals.Length; i++) originalVisualStates[i] = visuals[i].enabled;
            for (int i = 0; i < colliders.Length; i++) originalColliderStates[i] = colliders[i].enabled;
            if (purifiedVisual != null) purifiedVisual.SetActive(false);
        }
        public bool TryPurifyFromFlower()
        {
            if (!isActiveAndEnabled || !acceptsFlowerFire || IsPurified) return false;
            Initialize();
            IsPurified = true;
            foreach (Renderer visual in visuals)
                if (visual != null && (purifiedVisual == null || !visual.transform.IsChildOf(purifiedVisual.transform)))
                    visual.enabled = false;
            foreach (Collider2D collider in colliders) if (collider != null) collider.enabled = false;
            if (purifiedVisual != null) purifiedVisual.SetActive(true);
            onPurified.Invoke();
            return true;
        }
        public void ResetForRespawn()
        {
            Initialize();
            IsPurified = false;
            for (int i = 0; i < visuals.Length; i++) if (visuals[i] != null) visuals[i].enabled = originalVisualStates[i];
            for (int i = 0; i < colliders.Length; i++) if (colliders[i] != null) colliders[i].enabled = originalColliderStates[i];
            if (purifiedVisual != null) purifiedVisual.SetActive(false);
        }
    }
}
