using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace EmberPrototype.Editor
{
    public static class BurrTileSetBuilder
    {
        [MenuItem("Tools/Ember/Create Burr Tile Set")]
        public static void Build()
        {
            const string folder = "Assets/tile/PollutionBurrs";
            var tiles = new Tile[3];
            for (int i = 0; i < 3; i++)
            {
                string name = "Burr_" + (char)('A' + i);
                string image = folder + "/" + name + ".png";
                AssetDatabase.ImportAsset(image);
                var importer = (TextureImporter)AssetImporter.GetAtPath(image);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.GetSourceTextureWidthAndHeight(out int w, out int h);
                importer.spritePixelsPerUnit = w;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
                string path = folder + "/" + name + ".asset";
                tiles[i] = AssetDatabase.LoadAssetAtPath<Tile>(path);
                if (tiles[i] == null) { tiles[i] = ScriptableObject.CreateInstance<Tile>(); AssetDatabase.CreateAsset(tiles[i], path); }
                tiles[i].sprite = AssetDatabase.LoadAssetAtPath<Sprite>(image);
                tiles[i].colliderType = Tile.ColliderType.Sprite;
                EditorUtility.SetDirty(tiles[i]);
            }
            const string palettePath = folder + "/Burr Pollution.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(palettePath) == null)
                GridPaletteUtility.CreateNewPalette(folder, "Burr Pollution", GridLayout.CellLayout.Rectangle,
                    GridPalette.CellSizing.Automatic, new Vector3(1, 1, 0), GridLayout.CellSwizzle.XYZ,
                    TransparencySortMode.Default, Vector3.forward);
            var palette = PrefabUtility.LoadPrefabContents(palettePath);
            try
            {
                var map = palette.GetComponentInChildren<Tilemap>();
                for (int i = 0; i < 3; i++) map.SetTile(new Vector3Int(i, 0, 0), tiles[i]);
                PrefabUtility.SaveAsPrefabAsset(palette, palettePath);
            }
            finally { PrefabUtility.UnloadPrefabContents(palette); }
            const string mapPath = "Assets/EmberPrototype/Prefabs/PollutionTilemap.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(mapPath) == null)
            {
                var grid = new GameObject("PollutionGrid", typeof(Grid));
                try { CreateMap(grid.transform); PrefabUtility.SaveAsPrefabAsset(grid, mapPath); }
                finally { Object.DestroyImmediate(grid); }
            }
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (!EditorApplication.isPlaying && scene.path == "Assets/Scenes/New Scene.unity")
            {
                Grid grid = null;
                foreach (var candidate in Object.FindObjectsByType<Grid>(FindObjectsSortMode.None))
                    if (candidate.gameObject.scene == scene) { grid = candidate; break; }
                if (grid != null && grid.transform.Find("PollutionTilemap") == null)
                {
                    var map = CreateMap(grid.transform);
                    Undo.RegisterCreatedObjectUndo(map.gameObject, "Create pollution tilemap");
                    Selection.activeGameObject = map.gameObject;
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                }
            }
            AssetDatabase.SaveAssets();
        }
        private static Tilemap CreateMap(Transform parent)
        {
            var root = new GameObject("PollutionTilemap", typeof(Tilemap), typeof(TilemapRenderer), typeof(TilemapCollider2D));
            root.transform.SetParent(parent, false);
            root.GetComponent<TilemapRenderer>().sortingOrder = 12;
            root.GetComponent<TilemapCollider2D>().isTrigger = true;
            root.AddComponent<RoomHazard>();
            root.AddComponent<PurifiableSpikeTilemap>();
            return root.GetComponent<Tilemap>();
        }
    }
}
