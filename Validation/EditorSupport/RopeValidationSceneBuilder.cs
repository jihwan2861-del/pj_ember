using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

namespace EmberPrototype.Editor
{
    public static class RopeValidationSceneBuilder
    {
        public const string ScenePath = "Assets/EmberPrototype/RopeValidation.unity";

        [MenuItem("Ember Prototype/Create Rope Validation Scene")]
        public static void Build()
        {
            BuildAt(ScenePath);
        }

        public static void BuildAt(string outputPath)
        {
            // Build only a new additive scene. Never replace a user's open room or existing scene file.
            if (File.Exists(outputPath))
            {
                Debug.Log("Rope validation scene already exists: " + outputPath);
                return;
            }
            if (!AssetDatabase.IsValidFolder("Assets/EmberPrototype")) AssetDatabase.CreateFolder("Assets", "EmberPrototype");
            Scene previous = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            try
            {
                GameObject cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                Camera camera = cameraObject.AddComponent<Camera>();
                camera.orthographic = true;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.orthographicSize = 5.8f;
                camera.backgroundColor = new Color(0.035f, 0.055f, 0.09f);
                cameraObject.transform.position = new Vector3(2.5f, 2f, -10f);
                cameraObject.AddComponent<UniversalAdditionalCameraData>();
                Light2D light = new GameObject("Global Light").AddComponent<Light2D>();
                light.lightType = Light2D.LightType.Global;
                light.intensity = 1f;

                Block("Start Platform", new Vector2(-6f, -0.1f), new Vector2(3f, 0.4f));
                Block("Landing Platform", new Vector2(11f, -0.1f), new Vector2(10f, 0.4f));
                FlammableTile start = Fire("Start Fire", new Vector2(-5.2f, 1f));
                Rope("Rope 1", new Vector2(-1f, 4f));
                Rope("Rope 2", new Vector2(4f, 4f));
                Transform spawn = new GameObject("Spawn").transform;
                spawn.position = new Vector2(-6.2f, 0.6f);
                Transform goal = new GameObject("Goal").transform;
                goal.position = new Vector2(10f, 0.7f);
                GameObject player = new GameObject("Player");
                player.transform.position = spawn.position;
                player.AddComponent<PrototypeSprite>().Color = new Color(1f, 0.88f, 0.24f);
                player.transform.localScale = new Vector3(0.4f, 0.6f, 1f);
                player.AddComponent<BoxCollider2D>();
                GameObject actionObject = new GameObject("Action Burst Particles");
                actionObject.transform.SetParent(player.transform, false);
                ParticleSystem action = actionObject.AddComponent<ParticleSystem>();
                var actionMain = action.main;
                actionMain.loop = false;
                actionMain.playOnAwake = false;
                actionMain.startLifetime = 0.25f;
                actionMain.startSize = 0.06f;
                actionMain.startSpeed = 1f;
                actionMain.startColor = new Color(1f, 0.5f, 0.08f);
                var actionEmission = action.emission;
                actionEmission.rateOverTime = 0f;
                action.GetComponent<ParticleSystemRenderer>().sharedMaterial = ParticleMaterial();
                action.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                Rigidbody2D body = player.AddComponent<Rigidbody2D>();
                body.gravityScale = 1.8f;
                body.freezeRotation = true;
                body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                FlamePlayerController movement = player.AddComponent<FlamePlayerController>();
                SerializedObject settings = new SerializedObject(movement);
                settings.FindProperty("moveSpeed").floatValue = 3.3f;
                settings.FindProperty("jumpSpeed").floatValue = 6.09f;
                settings.FindProperty("launchSpeed").floatValue = 10f;
                settings.FindProperty("fireTravelRange").floatValue = 6f;
                settings.FindProperty("fireTravelSpeed").floatValue = 19.5f;
                settings.FindProperty("fireTravelAccelerationTime").floatValue = 0.07f;
                settings.FindProperty("fireLaunchControlLockTime").floatValue = 0.12f;
                settings.ApplyModifiedPropertiesWithoutUndo();
                player.AddComponent<PrototypeRoom>().Configure(spawn, goal, -5f);
                GameObject hud = new GameObject("Rope Instructions");
                hud.AddComponent<RopeValidationHud>();
                // PrototypeSprite creates transient sprites in Awake; do not serialize those references.
                foreach (SpriteRenderer visual in Object.FindObjectsByType<SpriteRenderer>())
                    if (visual.gameObject.scene == scene) visual.sprite = null;
                if (!EditorSceneManager.SaveScene(scene, outputPath)) throw new IOException("Could not save rope validation scene.");
                AssetDatabase.SaveAssets();
                Debug.Log("Created standalone rope validation scene: " + outputPath);
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
            }
        }

        private static void Block(string name, Vector2 position, Vector2 size)
        {
            GameObject block = new GameObject(name);
            block.transform.position = position;
            block.transform.localScale = size;
            block.AddComponent<PrototypeSprite>().Color = new Color(0.16f, 0.23f, 0.29f);
            block.AddComponent<BoxCollider2D>();
        }

        private static FlammableTile Fire(string name, Vector2 position)
        {
            GameObject fire = new GameObject(name);
            fire.transform.position = position;
            fire.transform.localScale = Vector3.one * 0.35f;
            fire.AddComponent<PrototypeSprite>().Color = new Color(1f, 0.45f, 0.1f);
            fire.AddComponent<CircleCollider2D>().isTrigger = true;
            FlammableTile tile = fire.AddComponent<FlammableTile>();
            SerializedObject settings = new SerializedObject(tile);
            FireVisualEffect effect = FireVisualEffect.Create(fire.transform);
            effect.GetComponent<ParticleSystemRenderer>().sharedMaterial = ParticleMaterial();
            effect.gameObject.SetActive(false);
            settings.FindProperty("fireEffect").objectReferenceValue = effect.gameObject;
            settings.ApplyModifiedPropertiesWithoutUndo();
            tile.ConfigureStartBurning(true);
            return tile;
        }

        private static Material ParticleMaterial()
        {
            const string path = "Assets/EmberPrototype/RopeValidationParticles.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            material = new Material(Shader.Find("Sprites/Default"));
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void Rope(string name, Vector2 pivot)
        {
            GameObject root = new GameObject(name);
            root.transform.position = pivot;
            FlammableTile end = Fire(name + " End", pivot + Vector2.down * 3f);
            end.transform.SetParent(root.transform, true);
            root.AddComponent<BurningRope>().Configure(end, 3f);
        }
    }
}
