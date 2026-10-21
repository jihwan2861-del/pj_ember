using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace EmberPrototype.Editor
{
    public static class WaterSpiritPrefabBuilder
    {
        public const string PrefabPath = "Assets/EmberPrototype/Prefabs/WaterSpirit.prefab";
        private const string AnimationFolder = "Assets/EmberPrototype/Animations/WaterSpirit";

        [InitializeOnLoadMethod]
        private static void QueueSetup()
        {
            EditorApplication.delayCall += CreateIfMissing;
        }

        private static void CreateIfMissing()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) == null &&
                AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprite-0001.png") != null) Build();
        }

        [MenuItem("Ember Prototype/Create Water Spirit Prefab")]
        public static void Build()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null)
            {
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
                return;
            }
            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprite-0001.png").OfType<Sprite>()
                .OrderBy(sprite => int.Parse(sprite.name.Substring(sprite.name.LastIndexOf('_') + 1))).ToArray();
            if (sprites.Length != 13) throw new InvalidOperationException("Water Spirit needs the 13 sliced Sprite-0001 frames.");
            EnsureFolder("Assets/EmberPrototype/Animations");
            EnsureFolder(AnimationFolder);
            WaterWavePrefabBuilder.Build();
            AnimationClip idle = CreateClip("Idle", sprites.Take(5).ToArray(), true);
            AnimationClip move = CreateClip("Move", sprites.Skip(5).Take(4).ToArray(), true);
            AnimationClip attack = CreateClip("Attack", sprites.Skip(9).Take(4).ToArray(), false);
            string controllerPath = AnimationFolder + "/WaterSpirit.controller";
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
                AnimatorStateMachine states = controller.layers[0].stateMachine;
                AnimatorState idleState = states.AddState("Idle"); idleState.motion = idle;
                states.AddState("Move").motion = move;
                states.AddState("Attack").motion = attack;
                states.defaultState = idleState;
            }
            GameObject root = new GameObject("WaterSpirit");
            try
            {
                SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
                renderer.sprite = sprites[0]; renderer.sortingOrder = 15;
                renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat");
                Animator animator = root.AddComponent<Animator>();
                animator.runtimeAnimatorController = controller;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                WaterSpirit spirit = root.AddComponent<WaterSpirit>();
                GameObject spawn = new GameObject("Wave Spawn");
                spawn.transform.SetParent(root.transform, false);
                spawn.transform.localPosition = new Vector3(0.8f, -0.5f, 0f);
                SerializedObject data = new SerializedObject(spirit);
                data.FindProperty("wavePrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(WaterWavePrefabBuilder.PrefabPath).GetComponent<WaterWave>();
                data.FindProperty("waveSpawn").objectReferenceValue = spawn.transform;
                data.ApplyModifiedPropertiesWithoutUndo();
                Selection.activeObject = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                AssetDatabase.SaveAssets();
                Debug.Log("WaterSpirit: created Idle / Move / Attack clips and wave-firing prefab.");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }

        private static AnimationClip CreateClip(string name, Sprite[] frames, bool loop)
        {
            string path = AnimationFolder + "/" + name + ".anim";
            AnimationClip existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (existing != null) return existing;
            AnimationClip clip = new AnimationClip { name = name, frameRate = 8f };
            ObjectReferenceKeyframe[] keys = new ObjectReferenceKeyframe[frames.Length + 1];
            for (int i = 0; i < frames.Length; i++) keys[i] = new ObjectReferenceKeyframe { time = i / clip.frameRate, value = frames[i] };
            keys[frames.Length] = new ObjectReferenceKeyframe { time = frames.Length / clip.frameRate, value = frames[frames.Length - 1] };
            AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), keys);
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            AssetDatabase.CreateAsset(clip, path);
            return clip;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int split = path.LastIndexOf('/');
            AssetDatabase.CreateFolder(path.Substring(0, split), path.Substring(split + 1));
        }
    }
}
