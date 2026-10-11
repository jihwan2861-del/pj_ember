using UnityEngine;
using UnityEngine.UI;

namespace EmberPrototype
{
    /// <summary>Cover instead of stretching or leaving bars at different window aspect ratios.</summary>
    [ExecuteAlways, RequireComponent(typeof(RawImage))]
    public sealed class MainMenuBackgroundCover : MonoBehaviour
    {
        private RawImage background;
        private void OnEnable() => Refresh();
        private void OnRectTransformDimensionsChange() => Refresh();

        public void Refresh()
        {
            if (background == null) background = GetComponent<RawImage>();
            if (background == null || background.texture == null) return;
            background.uvRect = CalculateUvRect(((RectTransform)transform).rect.size,
                new Vector2(background.texture.width, background.texture.height));
        }

        public static Rect CalculateUvRect(Vector2 viewport, Vector2 textureSize)
        {
            if (viewport.x <= 0f || viewport.y <= 0f || textureSize.x <= 0f || textureSize.y <= 0f)
                return new Rect(0f, 0f, 1f, 1f);
            float windowAspect = viewport.x / viewport.y;
            float imageAspect = textureSize.x / textureSize.y;
            if (imageAspect > windowAspect)
            {
                float width = windowAspect / imageAspect;
                return new Rect((1f - width) * 0.5f, 0f, width, 1f);
            }
            float height = imageAspect / windowAspect;
            return new Rect(0f, (1f - height) * 0.5f, 1f, height);
        }
    }
}
