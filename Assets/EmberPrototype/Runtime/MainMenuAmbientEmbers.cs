using UnityEngine;
using UnityEngine.UI;

namespace EmberPrototype
{
    /// <summary>Small unscaled-time UI embers. Their anchored positions remain editable in the scene.</summary>
    public sealed class MainMenuAmbientEmbers : MonoBehaviour
    {
        [SerializeField] private RectTransform[] embers;
        [SerializeField, Min(1f)] private float riseDistance = 160f;
        [SerializeField, Min(1f)] private float cycleSeconds = 8f;
        private Vector2[] origins;
        private Image[] images;
        private Color[] colors;
        private float clock;

        private void Awake()
        {
            int count = embers == null ? 0 : embers.Length;
            origins = new Vector2[count];
            images = new Image[count];
            colors = new Color[count];
            for (int i = 0; i < count; i++)
            {
                if (embers[i] == null) continue;
                origins[i] = embers[i].anchoredPosition;
                images[i] = embers[i].GetComponent<Image>();
                if (images[i] != null) colors[i] = images[i].color;
            }
        }

        private void Update()
        {
            clock += Time.unscaledDeltaTime;
            for (int i = 0; i < origins.Length; i++)
            {
                if (embers[i] == null) continue;
                float phase = Mathf.Repeat(clock / cycleSeconds + i * 0.137f, 1f);
                embers[i].anchoredPosition = origins[i] + new Vector2(Mathf.Sin(phase * 6f + i) * 12f, phase * riseDistance);
                if (images[i] == null) continue;
                Color color = colors[i];
                color.a *= Mathf.Sin(phase * Mathf.PI);
                images[i].color = color;
            }
        }
    }
}
