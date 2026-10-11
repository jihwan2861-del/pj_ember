using UnityEngine;

namespace EmberPrototype
{
    [RequireComponent(typeof(LineRenderer))]
    public sealed class WaterSpiritWarning : MonoBehaviour
    {
        [SerializeField] private Color warningColor = new Color(0.15f, 0.9f, 1f, 0.7f);
        private LineRenderer line;
        public bool IsVisible => line != null && line.enabled;
        public Bounds DisplayedBounds { get; private set; }

        private void Awake()
        {
            line = GetComponent<LineRenderer>();
            Hide();
        }

        public void Show(WaterSpiritAttack attack)
        {
            if (line == null) line = GetComponent<LineRenderer>();
            DisplayedBounds = attack.DangerBounds;
            Vector3 min = DisplayedBounds.min, max = DisplayedBounds.max;
            float z = transform.position.z;
            line.useWorldSpace = true;
            line.loop = true;
            line.positionCount = 4;
            line.SetPosition(0, new Vector3(min.x, min.y, z));
            line.SetPosition(1, new Vector3(max.x, min.y, z));
            line.SetPosition(2, new Vector3(max.x, max.y, z));
            line.SetPosition(3, new Vector3(min.x, max.y, z));
            line.startColor = line.endColor = warningColor;
            line.startWidth = line.endWidth = 0.05f;
            line.sortingOrder = 16;
            line.enabled = true;
        }

        public void Hide()
        {
            if (line == null) line = GetComponent<LineRenderer>();
            line.enabled = false;
        }

        private void OnDisable() => Hide();
    }
}
