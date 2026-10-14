using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EmberPrototype.Editor
{
    public static class PollutionSpikeBuilder
    {
        [MenuItem("Tools/Ember/Create Purifiable Spike Obstacle")]
        public static void Build()
        {
            const string texturePath = "Assets/EmberPrototype/Art/FireFlower/PollutionSpikes.png";
            const string prefabPath = "Assets/EmberPrototype/Prefabs/PollutionSpikes.prefab";
            AssetDatabase.ImportAsset(texturePath);
            var importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);
            if (importer == null) throw new System.InvalidOperationException("Pollution spike art is missing.");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.GetSourceTextureWidthAndHeight(out int w, out int h);
            importer.spritePixelsPerUnit = w / 2f;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
            var root = new GameObject("PollutionSpikes");
            try
            {
                var renderer = root.AddComponent<SpriteRenderer>();
                renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);
                renderer.sortingOrder = 12;
                var collider = root.AddComponent<BoxCollider2D>();
                collider.isTrigger = true;
                collider.size = new Vector2(1.9f, .35f);
                collider.offset = new Vector2(0, -.07f);
                root.AddComponent<RoomHazard>();
                root.AddComponent<PollutionObstacle>();
                var prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                if (scene.path == "Assets/Scenes/New Scene.unity" && !scene.GetRootGameObjects()
                    .Where(g => g != root).SelectMany(g => g.GetComponentsInChildren<PollutionObstacle>(true)).Any())
                {
                    var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                    Undo.RegisterCreatedObjectUndo(instance, "Place purifiable spikes");
                    var flower = Object.FindObjectsByType<FireFlower>(FindObjectsSortMode.None)
                        .FirstOrDefault(f => f.gameObject.scene == scene);
                    var head = flower != null ? flower.transform.Find("Head") : null;
                    instance.transform.position = flower != null
                        ? (head != null ? head.position : flower.transform.position) + (Vector3)flower.FiringDirection * 3f
                        : Vector3.right * 3f;
                    var mechanics = GameObject.Find("Mechanics");
                    if (mechanics != null) instance.transform.SetParent(mechanics.transform, true);
                    Selection.activeGameObject = instance;
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                }
                AssetDatabase.SaveAssets();
            }
            finally { Object.DestroyImmediate(root); }
        }
    }
}
