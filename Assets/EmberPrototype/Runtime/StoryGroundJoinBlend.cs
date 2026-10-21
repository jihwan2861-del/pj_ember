using UnityEngine;

namespace EmberPrototype
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    [DefaultExecutionOrder(120)]
    [AddComponentMenu("Ember/Story Ground Join Blend")]
    public sealed class StoryGroundJoinBlend : MonoBehaviour
    {
        [SerializeField] private CircleCollider2D circle;
        [SerializeField, Min(0.001f)] private float blendWidth = 1.4f;
        [SerializeField, Range(0f, 1f)] private float leftEdgeUV = 24f / 2172f;
        [SerializeField, Min(0.001f)] private float surfaceFeather = 0.08f;

        private static readonly int JoinCircle = Shader.PropertyToID("_JoinCircle");
        private static readonly int JoinLeftUV = Shader.PropertyToID("_JoinLeftUV");
        private static readonly int JoinWidthUV = Shader.PropertyToID("_JoinWidthUV");
        private static readonly int JoinSurfaceFeather = Shader.PropertyToID("_JoinSurfaceFeather");
        private static readonly int JoinEnabled = Shader.PropertyToID("_JoinEnabled");

        private SpriteRenderer spriteRenderer;
        private MaterialPropertyBlock properties;

        private void OnEnable()
        {
            ApplyProperties();
        }

        private void LateUpdate()
        {
            ApplyProperties();
        }

        private void ApplyProperties()
        {
            if (!GetRenderer()) return;

            // Read the current block so other components retain their properties.
            spriteRenderer.GetPropertyBlock(properties);
            Sprite sprite = spriteRenderer.sprite;
            float worldWidth = sprite != null
                ? sprite.bounds.size.x * Mathf.Abs(transform.lossyScale.x) : 0f;
            bool valid = circle != null && worldWidth > 0.0001f;
            if (valid)
            {
                Transform circleTransform = circle.transform;
                Vector3 center = circleTransform.TransformPoint(circle.offset);
                Vector3 circleScale = circleTransform.lossyScale;
                float radius = circle.radius * Mathf.Max(
                    Mathf.Abs(circleScale.x), Mathf.Abs(circleScale.y));
                valid = radius > 0.0001f;
                properties.SetVector(JoinCircle, new Vector4(center.x, center.y, radius, 0f));
                properties.SetFloat(JoinLeftUV, Mathf.Clamp01(leftEdgeUV));
                properties.SetFloat(JoinWidthUV, Mathf.Max(blendWidth, 0.001f) / worldWidth);
                properties.SetFloat(JoinSurfaceFeather, Mathf.Max(surfaceFeather, 0.001f));
            }

            properties.SetFloat(JoinEnabled, valid ? 1f : 0f);
            spriteRenderer.SetPropertyBlock(properties);
        }

        private bool GetRenderer()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) return false;
            if (properties == null) properties = new MaterialPropertyBlock();
            return true;
        }

        private void OnDisable()
        {
            if (!GetRenderer()) return;
            spriteRenderer.GetPropertyBlock(properties);
            properties.SetFloat(JoinEnabled, 0f);
            spriteRenderer.SetPropertyBlock(properties);
        }
    }
}
