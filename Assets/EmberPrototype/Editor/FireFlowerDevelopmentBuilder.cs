using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EmberPrototype.Editor
{
    public static class FireFlowerDevelopmentBuilder
    {
        [MenuItem("Tools/Ember/Develop Fire Flower In Current Scene")]
        public static void Build()
        {
            const string sheet = "Assets/EmberPrototype/Art/FireFlower/FireFlowerSheet.png";
            AssetDatabase.ImportAsset(sheet);
            var importer = (TextureImporter)AssetImporter.GetAtPath(sheet);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.maxTextureSize = 2048;
            importer.GetSourceTextureWidthAndHeight(out int w, out int h);
            importer.spritePixelsPerUnit = w / 4f;
            var metadata = new SpriteMetaData[8];
            for (int i = 0; i < 8; i++)
                metadata[i] = new SpriteMetaData { name = "Frame" + i,
                    rect = new Rect(i % 4 * w / 4f, (i < 4 ? h / 2f : 0), w / 4f, h / 2f),
                    alignment = 9, pivot = i < 4 ? new Vector2(.5f, .02f) : new Vector2(.78f, .45f) };
            importer.spritesheet = metadata;
            importer.SaveAndReimport();
            var sprites = AssetDatabase.LoadAllAssetsAtPath(sheet).OfType<Sprite>().OrderBy(s => s.name).ToArray();
            var bullet = new GameObject("FireFlowerProjectile");
            bullet.AddComponent<SpriteRenderer>().sprite = sprites[4];
            bullet.transform.localScale = Vector3.one * .6f;
            var projectile = bullet.AddComponent<FireFlowerProjectile>();
            var ps = new SerializedObject(projectile);
            SetFrames(ps, "animationFrames", sprites.Skip(4).ToArray());
            ps.ApplyModifiedPropertiesWithoutUndo();
            var bulletAsset = PrefabUtility.SaveAsPrefabAsset(bullet, "Assets/EmberPrototype/Prefabs/FireFlowerProjectile.prefab");
            Object.DestroyImmediate(bullet);
            var flower = new GameObject("FireFlower");
            var renderer = flower.AddComponent<SpriteRenderer>();
            renderer.sprite = sprites[0]; renderer.sortingOrder = 10;
            var component = flower.AddComponent<FireFlower>();
            var muzzle = new GameObject("Muzzle").transform;
            muzzle.SetParent(flower.transform, false); muzzle.localPosition = new Vector3(0, .52f, 0);
            var fs = new SerializedObject(component);
            fs.FindProperty("visual").objectReferenceValue = renderer;
            fs.FindProperty("wiltedSprite").objectReferenceValue = sprites[0];
            fs.FindProperty("bloomingSprite").objectReferenceValue = sprites[2];
            SetFrames(fs, "bloomFrames", new[] { sprites[1], sprites[2], sprites[3], sprites[2] });
            fs.FindProperty("muzzle").objectReferenceValue = muzzle;
            fs.FindProperty("projectilePrefab").objectReferenceValue = bulletAsset.GetComponent<FireFlowerProjectile>();
            fs.FindProperty("warningTime").floatValue = 0;
            fs.FindProperty("firingDirection").vector2Value = Vector2.right;
            fs.FindProperty("oneShot").boolValue = false;
            fs.ApplyModifiedPropertiesWithoutUndo();
            var asset = PrefabUtility.SaveAsPrefabAsset(flower, "Assets/EmberPrototype/Prefabs/FireFlower.prefab");
            Object.DestroyImmediate(flower);
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var existing = Object.FindObjectsByType<FireFlower>(FindObjectsSortMode.None).FirstOrDefault(f => f.gameObject.scene == scene);
            if (existing == null)
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(asset, scene);
                Undo.RegisterCreatedObjectUndo(instance, "Place Fire Flower");
                var parent = GameObject.Find("Mechanics");
                if (parent != null) instance.transform.SetParent(parent.transform, false);
                var spawn = GameObject.Find("Spawn");
                instance.transform.position = spawn != null ? spawn.transform.position + Vector3.left * 1.5f : Vector3.zero;
                Selection.activeGameObject = instance;
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            var parts = AssetDatabase.LoadAllAssetsAtPath("Assets/EmberPrototype/Art/FireFlower/FireFlowerParts.png")
                .OfType<Sprite>().OrderBy(s => s.name).ToArray();
            if (parts.Length == 4) UpgradeToSplitFlower(parts);
        }
        public static void UpgradeToSplitFlower(Sprite[] parts)
        {
            if (parts == null || parts.Length != 4) throw new System.ArgumentException("Four flower part sprites required.");
            const string path = "Assets/EmberPrototype/Prefabs/FireFlower.prefab";
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                ConfigureParts(root.GetComponent<FireFlower>(), parts);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.path == "Assets/Scenes/New Scene.unity")
            {
                foreach (var flower in Object.FindObjectsByType<FireFlower>(FindObjectsSortMode.None))
                {
                    if (flower.gameObject.scene != scene) continue;
                    Undo.RegisterFullObjectHierarchyUndo(flower.gameObject, "Split flower head and body");
                    ConfigureParts(flower, parts);
                }
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();
        }
        private static void ConfigureParts(FireFlower flower, Sprite[] parts)
        {
            var root = flower.transform;
            var contact = root.GetComponent<BoxCollider2D>();
            if (contact == null) contact = root.gameObject.AddComponent<BoxCollider2D>();
            contact.isTrigger = true;
            contact.offset = new Vector2(0, .55f);
            contact.size = new Vector2(.95f, 1.15f);
            var body = root.Find("Body");
            if (body == null) { body = new GameObject("Body").transform; body.SetParent(root, false); }
            var bodyRenderer = body.GetComponent<SpriteRenderer>();
            if (bodyRenderer == null) bodyRenderer = body.gameObject.AddComponent<SpriteRenderer>();
            bodyRenderer.sprite = parts[0]; bodyRenderer.sortingOrder = 10;
            var so = new SerializedObject(flower);
            var head = root.Find("Head");
            if (head == null)
            {
                head = new GameObject("Head").transform;
                head.SetParent(root, false);
                head.localPosition = new Vector3(0, .65f, 0);
                var direction = so.FindProperty("firingDirection").vector2Value;
                head.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            }
            var headRenderer = head.GetComponent<SpriteRenderer>();
            if (headRenderer == null) headRenderer = head.gameObject.AddComponent<SpriteRenderer>();
            headRenderer.sprite = parts[1]; headRenderer.sortingOrder = 11;
            var muzzle = head.Find("Muzzle");
            if (muzzle == null) muzzle = root.Find("Muzzle");
            if (muzzle == null) muzzle = new GameObject("Muzzle").transform;
            muzzle.SetParent(head, false); muzzle.localPosition = Vector3.zero;
            muzzle.localRotation = Quaternion.identity;
            so.FindProperty("head").objectReferenceValue = head;
            so.FindProperty("visual").objectReferenceValue = headRenderer;
            so.FindProperty("muzzle").objectReferenceValue = muzzle;
            so.FindProperty("wiltedSprite").objectReferenceValue = parts[1];
            so.FindProperty("bloomingSprite").objectReferenceValue = parts[3];
            SetFrames(so, "bloomFrames", new[] { parts[2], parts[3], parts[3] });
            so.ApplyModifiedPropertiesWithoutUndo();
            var oldRenderer = root.GetComponent<SpriteRenderer>();
            if (oldRenderer != null) oldRenderer.enabled = false;
            EditorUtility.SetDirty(flower);
        }
        private static void SetFrames(SerializedObject obj, string name, Sprite[] frames)
        {
            var property = obj.FindProperty(name); property.arraySize = frames.Length;
            for (int i = 0; i < frames.Length; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
        }
    }
}
