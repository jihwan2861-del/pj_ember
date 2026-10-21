using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EmberPrototype.Editor
{
    public static class FireFlowerValidationSceneBuilder
    {
        [MenuItem("Tools/Ember/Create Fire Flower Test Scene")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            BuildBatch();
        }
        public static void BuildBatch()
        {
            if (!AssetDatabase.IsValidFolder("Assets/EmberPrototype"))
                AssetDatabase.CreateFolder("Assets", "EmberPrototype");
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cameraRoot = new GameObject("Main Camera");
            cameraRoot.tag = "MainCamera";
            cameraRoot.transform.position = new Vector3(-1f, 2f, -10f);
            var camera = cameraRoot.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 4.7f;
            camera.backgroundColor = new Color(0.12f, 0.16f, 0.24f);
            Block("Flower ledge", new Vector2(-4f, -0.5f), new Vector2(3f, 1f), new Color(0.3f, 0.3f, 0.35f));
            Block("Player ledge", new Vector2(-1f, -0.5f), new Vector2(1.4f, 1f), new Color(0.3f, 0.3f, 0.35f));
            Block("Blocked exit ledge", new Vector2(2f, -0.5f), new Vector2(2f, 1f), new Color(0.3f, 0.3f, 0.35f));
            var player = new GameObject("Fire Spirit");
            player.transform.position = new Vector2(-1f, 0.3f);
            player.AddComponent<PrototypeSprite>().Color = new Color(1f, 0.4f, 0.08f);
            player.GetComponent<SpriteRenderer>().transform.localScale = Vector3.one * 0.6f;
            var body = player.AddComponent<Rigidbody2D>();
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            var box = player.AddComponent<BoxCollider2D>();
            box.size = new Vector2(0.6f, 1f);
            player.AddComponent<FlamePlayerController>();
            var flower = new GameObject("Fire Flower");
            flower.transform.position = new Vector2(-4f, 0.3f);
            flower.AddComponent<FireFlower>();
            var flowerSettings = new SerializedObject(flower.GetComponent<FireFlower>());
            flowerSettings.FindProperty("reactionRadius").floatValue = 3.5f;
            flowerSettings.ApplyModifiedPropertiesWithoutUndo();
            var pollution = Block("Flower-fire pollution barrier", new Vector2(2f, 0.8f),
                new Vector2(0.8f, 1.6f), new Color(0.25f, 0.04f, 0.33f));
            pollution.AddComponent<RoomHazard>();
            var pollutionComponent = pollution.AddComponent<PollutionObstacle>();
            var sprout = new GameObject("Purified golden sprout");
            sprout.transform.SetParent(pollution.transform, false);
            sprout.transform.localPosition = new Vector3(0f, -0.35f, 0f);
            sprout.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
            sprout.AddComponent<PrototypeSprite>().Color = new Color(1f, 0.8f, 0.2f);
            sprout.SetActive(false);
            var pollutionSettings = new SerializedObject(pollutionComponent);
            pollutionSettings.FindProperty("purifiedVisual").objectReferenceValue = sprout;
            pollutionSettings.ApplyModifiedPropertiesWithoutUndo();
            var pit = Block("Fall hazard", new Vector2(-1f, -3f), new Vector2(20f, 0.5f), Color.black);
            pit.AddComponent<RoomHazard>();
            var hud = new GameObject("Controls");
            hud.AddComponent<PrototypeHud>();
            hud.AddComponent<GameSessionControls>();
            var text = new GameObject("Flower instructions").AddComponent<TextMesh>();
            text.text = "C near flower -> locked aim -> dodge -> purify\nArrow keys + Z/X/C    R: reset";
            text.fontSize = 32;
            text.characterSize = 0.06f;
            text.anchor = TextAnchor.MiddleCenter;
            text.transform.position = new Vector3(-1f, 3.5f, 0f);
            EditorSceneManager.SaveScene(scene, "Assets/EmberPrototype/FireFlowerValidation.unity");
            AssetDatabase.SaveAssets();
        }
        private static GameObject Block(string name, Vector2 position, Vector2 size, Color color)
        {
            var root = new GameObject(name);
            root.transform.position = position;
            root.transform.localScale = size;
            root.AddComponent<PrototypeSprite>().Color = color;
            root.AddComponent<BoxCollider2D>();
            return root;
        }
    }
}
