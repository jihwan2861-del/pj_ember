using UnityEngine;
using UnityEngine.UI;

namespace EmberPrototype
{
    // Keep the full line laid out while revealing its glyphs, so centering and wrapping stay still.
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UnityEngine.UI.Text))]
    public sealed class NarrationTextReveal : BaseMeshEffect
    {
        private int visibleCharacters = int.MaxValue;
        public int TotalCharacters { get; private set; }

        public void BeginReveal()
        {
            TotalCharacters = 0;
            SetVisibleCharacters(0);
        }

        public void SetVisibleCharacters(int count)
        {
            visibleCharacters = Mathf.Max(0, count);
            if (graphic != null) graphic.SetVerticesDirty();
        }

        public void ShowAll() => SetVisibleCharacters(int.MaxValue);

        public override void ModifyMesh(VertexHelper vertices)
        {
            if (!IsActive()) return;
            TotalCharacters = vertices.currentVertCount / 4;
            if (visibleCharacters >= TotalCharacters) return;
            UIVertex vertex = default;
            for (int i = visibleCharacters * 4; i < vertices.currentVertCount; i++)
            {
                vertices.PopulateUIVertex(ref vertex, i);
                vertex.color.a = 0;
                vertices.SetUIVertex(vertex, i);
            }
        }
    }
}
