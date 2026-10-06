using System.IO;
using UnityEditor;
using UnityEngine;

namespace EmberPrototype.Editor
{
    public static class WaterWavePrefabBuilder
    {
        public const string PrefabPath = "Assets/EmberPrototype/Prefabs/WaterWave.prefab";
        [MenuItem("Ember Prototype/Create Water Wave Prefab")]
        public static void Build()
        {
            if (File.Exists(PrefabPath)) { Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath); return; }
            Directory.CreateDirectory("Assets/EmberPrototype/Prefabs"); AssetDatabase.Refresh();
            const string imagePath = "Assets/EmberPrototype/Prefabs/WaterWave.png";
            if (!File.Exists(imagePath))
            {
                Texture2D texture = new Texture2D(128, 256, TextureFormat.RGBA32, false);
                Vector2[] outline = WaterWave.Outline;
                for (int y = 0; y < 256; y++)
                    for (int x = 0; x < 128; x++)
                    {
                        Vector2 p = new Vector2((x + 0.5f) / 128f - 0.5f, (y + 0.5f) / 256f);
                        bool inside = false; float edgeDistance = 1f;
                        for (int i = 0, j = outline.Length - 1; i < outline.Length; j = i++)
                        {
                            Vector2 a = outline[i], b = outline[j];
                            if ((a.y > p.y) != (b.y > p.y) && p.x < (b.x - a.x) * (p.y - a.y) / (b.y - a.y) + a.x) inside = !inside;
                            Vector2 d = b - a;
                            if (i >= 3 && j >= 2) edgeDistance = Mathf.Min(edgeDistance, Vector2.Distance(p, a + d * Mathf.Clamp01(Vector2.Dot(p - a, d) / d.sqrMagnitude)));
                        }
                        Color color = Color.Lerp(new Color(0.03f, 0.25f, 0.6f), new Color(0.08f, 0.85f, 1f), p.y);
                        if (p.y > 0.62f && edgeDistance < 0.025f) color = new Color(0.85f, 0.98f, 1f);
                        texture.SetPixel(x, y, inside ? color : Color.clear);
                    }
                texture.Apply(); File.WriteAllBytes(imagePath, texture.EncodeToPNG()); Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(imagePath);
                TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(imagePath);
                importer.textureType = TextureImporterType.Sprite; importer.spritePixelsPerUnit = 128f;
                importer.spriteImportMode = SpriteImportMode.Single;
                TextureImporterSettings textureSettings = new TextureImporterSettings(); importer.ReadTextureSettings(textureSettings);
                textureSettings.spriteAlignment = (int)SpriteAlignment.Custom; textureSettings.spritePivot = new Vector2(0.5f, 0f);
                textureSettings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(textureSettings); importer.filterMode = FilterMode.Bilinear; importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }
            GameObject root = new GameObject("WaterWave");
            try
            {
                WaterWave wave = root.AddComponent<WaterWave>(); root.AddComponent<RoomHazard>();
                GameObject art = new GameObject("Wave Visual"); art.transform.SetParent(root.transform, false);
                SpriteRenderer renderer = art.AddComponent<SpriteRenderer>(); renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(imagePath);
                // Texture is 1x2 world units; use drawMode Sliced to normalize it to 1x1 before scaling.
                renderer.drawMode = SpriteDrawMode.Sliced; renderer.size = Vector2.one; renderer.sortingOrder = 15;
                renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat");
                SerializedObject data = new SerializedObject(wave); data.FindProperty("visual").objectReferenceValue = renderer;
                data.ApplyModifiedPropertiesWithoutUndo();
                wave.RefreshShape();
                Selection.activeObject = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath); AssetDatabase.SaveAssets();
            }
            finally { Object.DestroyImmediate(root); }
        }
    }
}
