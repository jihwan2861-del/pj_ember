using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using EmberPrototype;

namespace EmberPrototype.Editor
{
    /// <summary>Creates an editable death effect once, preserving subsequent artist edits.</summary>
    public static class DeathParticlePrefabSetup
    {
        private const string PrefabPath = "Assets/prefab/player.prefab";
        private const string EffectName = "Death Particles";

        [InitializeOnLoadMethod]
        private static void QueueSetup()
        {
            EditorApplication.delayCall += TrySetup;
        }

        private static void TrySetup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling
                || IsPlayerPrefabOpen())
            {
                EditorApplication.update -= TrySetup;
                EditorApplication.update += TrySetup;
                return;
            }

            EditorApplication.update -= TrySetup;
            SetupIfNeeded();
        }

        private static bool IsPlayerPrefabOpen()
        {
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            return stage != null && stage.assetPath == PrefabPath;
        }

        [MenuItem("Tools/Ember/Setup Death Particles")]
        public static void SetupIfNeeded()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling
                || IsPlayerPrefabOpen() || AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) == null)
                return;

            GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                // Never overwrite an effect the user has already authored.
                Transform existing = root.transform.Find(EffectName);
                if (existing != null)
                {
                    if (WireEffect(root, existing.GetComponent<ParticleSystem>()))
                        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                    return;
                }

                var effectObject = new GameObject(EffectName);
                effectObject.layer = root.layer;
                effectObject.transform.SetParent(root.transform, false);
                var particles = effectObject.AddComponent<ParticleSystem>();
                particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

                var main = particles.main;
                main.duration = 0.45f;
                main.loop = false;
                main.playOnAwake = false;
                main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.45f);
                main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 4f);
                main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.18f);
                main.startColor = Color.white;
                main.gravityModifier = 0f;
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                main.maxParticles = 16;

                var emission = particles.emission;
                emission.rateOverTime = 0f;
                emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 10) });

                var shape = particles.shape;
                shape.enabled = true;
                shape.shapeType = ParticleSystemShapeType.Circle;
                shape.radius = 0.04f;
                shape.radiusThickness = 0f;

                var size = particles.sizeOverLifetime;
                size.enabled = true;
                size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

                var color = particles.colorOverLifetime;
                color.enabled = true;
                var gradient = new Gradient();
                gradient.SetKeys(
                    new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                    new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) });
                color.color = gradient;

                var animation = particles.textureSheetAnimation;
                animation.enabled = true;
                animation.mode = ParticleSystemAnimationMode.Sprites;
                animation.frameOverTime = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0f, 1f, 1f));
                animation.cycleCount = 1;

                var renderer = particles.GetComponent<ParticleSystemRenderer>();
                renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Material/M_FlameParticle.mat");
                var sprite = root.GetComponent<SpriteRenderer>();
                renderer.sortingLayerID = sprite != null ? sprite.sortingLayerID : 0;
                renderer.sortingOrder = (sprite != null ? sprite.sortingOrder : 0) + 5;

                WireEffect(root, particles);
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                Debug.Log("Death Particles 준비 완료: Texture Sheet Animation의 Sprites 목록에 불씨 프레임을 추가하세요.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static bool WireEffect(GameObject root, ParticleSystem particles)
        {
            PlayerDeathEffect effect = root.GetComponent<PlayerDeathEffect>();
            bool added = effect == null;
            if (added) effect = root.AddComponent<PlayerDeathEffect>();
            var serialized = new SerializedObject(effect);
            var reference = serialized.FindProperty("deathParticles");
            if (reference.objectReferenceValue != null || particles == null) return added;
            reference.objectReferenceValue = particles;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return true;
        }
    }
}
