using System.IO;
using UnityEditor;
using UnityEngine;

namespace EmberPrototype.Editor
{
    public static class StrawberryPickupBuilder
    {
        public const string PrefabPath = "Assets/EmberPrototype/Prefabs/ChickenPickup.prefab";
        private const string IconPath = "Assets/EmberPrototype/Art/ChickenPickup.png";
        [MenuItem("Ember Prototype/Create Chicken Pickup Prefab")]
        public static void Build()
        {
            if (!File.Exists(IconPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(IconPath));
                string[] rows = {
                    "................", "...dddddd.......", "..dggggggd......", ".dggyyggggd.....",
                    ".dgyygggggd.....", ".dggggggggd.....", "..dgggggggd.....", "...dgggggd......",
                    "....dddddbb.....", ".........bbw....", "..........bww...", "..........wwww..", 
                    "...........www..", "................", "................", "................"
                };
                Texture2D texture = new Texture2D(16,16,TextureFormat.RGBA32,false);
                for (int y=0;y<16;y++) for(int x=0;x<16;x++)
                {
                    char pixel = rows[y][x];
                    Color tint = pixel == 'd' ? new Color32(118,61,35,255) : pixel == 'g' ? new Color32(215,131,50,255)
                        : pixel == 'y' ? new Color32(255,203,109,255) : pixel == 'b' ? new Color32(189,165,124,255)
                        : pixel == 'w' ? new Color32(255,241,208,255) : Color.clear;
                    texture.SetPixel(x,15-y,tint);
                }
                texture.Apply(); File.WriteAllBytes(IconPath,texture.EncodeToPNG()); Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(IconPath);
                TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(IconPath);
                importer.textureType = TextureImporterType.Sprite; importer.spritePixelsPerUnit = 24f;
                importer.filterMode = FilterMode.Point; importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled = false; importer.alphaIsTransparency = true; importer.SaveAndReimport();
            }
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null) return;
            GameObject root = new GameObject("ChickenPickup");
            root.AddComponent<CircleCollider2D>().radius = 0.3f;
            root.GetComponent<CircleCollider2D>().isTrigger = true;
            root.AddComponent<StrawberryPickup>();
            GameObject image = new GameObject("Visual"); image.transform.SetParent(root.transform,false);
            SpriteRenderer renderer = image.AddComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(IconPath); renderer.sortingOrder = 10;
            PrefabUtility.SaveAsPrefabAsset(root,PrefabPath); Object.DestroyImmediate(root);
        }
    }
}
