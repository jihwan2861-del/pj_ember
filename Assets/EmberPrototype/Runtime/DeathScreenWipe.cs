using UnityEngine;
using UnityEngine.UI;

namespace EmberPrototype
{
    /// <summary>Screen-space staircase wipe without texture assets.</summary>
    public sealed class DeathScreenWipe : Graphic
    {
        private float coverage;
        private int steps = 6;

        public void SetCoverage(float value, int stepCount)
        {
            coverage = Mathf.Clamp01(value);
            steps = Mathf.Clamp(stepCount, 2, 16);
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            if (coverage <= 0f) return;
            Rect rect = rectTransform.rect;
            float width = rect.width;
            float height = rect.height / steps;
            float sweep = Mathf.Lerp(-width * 0.6f, width * 1.2f, coverage);
            for (int i = 0; i < steps; i++)
            {
                float top = rect.yMax - i * height;
                float bottom = top - height;
                float edge = sweep + width * 0.5f * (1f - (float)i / steps);
                float topX = rect.xMin + Mathf.Clamp(edge, 0f, width);
                float bottomX = rect.xMin + Mathf.Clamp(edge - width * 0.08f, 0f, width);
                int index = mesh.currentVertCount;
                mesh.AddVert(new Vector3(rect.xMin, bottom), color, Vector2.zero);
                mesh.AddVert(new Vector3(rect.xMin, top), color, Vector2.zero);
                mesh.AddVert(new Vector3(topX, top), color, Vector2.zero);
                mesh.AddVert(new Vector3(bottomX, bottom), color, Vector2.zero);
                mesh.AddTriangle(index, index + 1, index + 2);
                mesh.AddTriangle(index, index + 2, index + 3);
            }
        }
    }
}
