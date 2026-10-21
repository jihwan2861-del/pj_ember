using System.Collections;
using UnityEngine;

namespace EmberPrototype
{
    [AddComponentMenu("Ember/Story Flower Energy Sequence")]
    [DisallowMultipleComponent]
    public sealed class StoryFlowerEnergySequence : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private FireFlower flower;
        [SerializeField] private FlamePlayerController player;
        [SerializeField] private Transform energyOrigin;
        [SerializeField] private StoryTutorialSceneTransition transition;
        [Header("Timing — Game Seconds")]
        [SerializeField, Min(0.05f)] private float travelSeconds = 0.85f;
        [SerializeField, Min(0f)] private float arrivalGlowSeconds = 0.25f;
        [SerializeField, Min(0f)] private float arcHeight = 0.65f;
        [Header("Look")]
        [SerializeField] private Color energyColor = new Color(1f, 0.68f, 0.12f, 1f);
        [SerializeField] private string sortingLayer = "EmberWater";
        [SerializeField] private int sortingOrder = 30;
        [Header("Particle Accents")]
        [SerializeField] private ParticleSystem energyParticlesPrefab;
        [SerializeField] private ParticleSystem arrivalParticlesPrefab;
        private ParticleSystem energyParticles;

        private bool started;
        private bool ownsPlayerLock;
        private GameObject effectRoot;
        private LineRenderer trail;
        private LineRenderer arrivalRing;
        private Material effectMaterial;

        public void BeginEnergyReception()
        {
            if (!isActiveAndEnabled || started) return;
            if (player == null)
            {
                foreach (var candidate in FindObjectsByType<FlamePlayerController>())
                {
                    if (candidate.gameObject.scene != gameObject.scene) continue;
                    player = candidate;
                    break;
                }
            }
            if (player == null || player.IsDying || player.ControlsLocked) return;

            started = true;
            player.SetControlsLocked(true);
            ownsPlayerLock = true;
            Rigidbody2D body = player.GetComponent<Rigidbody2D>();
            if (body != null) body.linearVelocity = Vector2.zero;
            StartCoroutine(PlayAndTransition());
        }

        private IEnumerator PlayAndTransition()
        {
            CreateEffect();
            Vector3 start = energyOrigin != null ? energyOrigin.position : transform.position;
            float duration = Mathf.Max(0.05f, travelSeconds);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (player == null || player.IsDying) { Cleanup(); yield break; }
                if (!GameSessionControls.IsPaused && !SceneRestartWipe.IsPlaying && !SceneFadeTransition.IsPlaying)
                {
                    elapsed += Time.deltaTime;
                    DrawTrail(start, PlayerCenter, Mathf.Clamp01(elapsed / duration));
                }
                yield return null;
            }

            DrawTrail(start, PlayerCenter, 1f);
            if (energyParticles != null) energyParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            if (arrivalParticlesPrefab != null)
            {
                ParticleSystem arrival = Instantiate(arrivalParticlesPrefab, PlayerCenter, Quaternion.identity, effectRoot.transform);
                SortParticles(arrival);
                arrival.Play(true);
            }
            float glowDuration = Mathf.Max(0f, arrivalGlowSeconds);
            elapsed = 0f;
            while (elapsed < glowDuration)
            {
                if (player == null || player.IsDying) { Cleanup(); yield break; }
                if (!GameSessionControls.IsPaused && !SceneRestartWipe.IsPlaying && !SceneFadeTransition.IsPlaying)
                {
                    elapsed += Time.deltaTime;
                    DrawArrivalRing(PlayerCenter, Mathf.Clamp01(elapsed / glowDuration));
                }
                yield return null;
            }

            if (transition == null)
            {
                Debug.LogError("StoryFlowerEnergySequence에 Story Tutorial Scene Transition을 연결해주세요.", this);
                Cleanup();
                yield break;
            }

            transition.BeginTransition();
            float wait = 0f;
            while (!SceneFadeTransition.IsPlaying && wait < 1f)
            {
                if (!GameSessionControls.IsPaused && !SceneRestartWipe.IsPlaying)
                    wait += Time.unscaledDeltaTime;
                yield return null;
            }
            ReleasePlayerLock();
            CleanupVisuals();
        }

        private Vector3 PlayerCenter => player != null ? player.transform.position + Vector3.up * 0.2f : transform.position;

        private void CreateEffect()
        {
            effectRoot = new GameObject("Flower Energy to Amber");
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(effectRoot, gameObject.scene);
            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null) effectMaterial = new Material(shader);
            trail = CreateLine("Energy Trail", false);
            arrivalRing = CreateLine("Arrival Glow", true);
            if (energyParticlesPrefab != null)
            {
                energyParticles = Instantiate(energyParticlesPrefab, effectRoot.transform);
                energyParticles.transform.position = energyOrigin != null ? energyOrigin.position : transform.position;
                SortParticles(energyParticles);
                energyParticles.Play(true);
            }
        }

        private void SortParticles(ParticleSystem particles)
        {
            int order = sortingOrder + 1;
            foreach (var renderer in particles.GetComponentsInChildren<ParticleSystemRenderer>(true))
            {
                renderer.sortingLayerName = sortingLayer;
                renderer.sortingOrder = order++;
            }
        }

        private LineRenderer CreateLine(string name, bool loop)
        {
            var root = new GameObject(name);
            root.transform.SetParent(effectRoot.transform, false);
            var line = root.AddComponent<LineRenderer>();
            if (effectMaterial != null) line.sharedMaterial = effectMaterial;
            line.useWorldSpace = true;
            line.loop = loop;
            line.numCapVertices = 4;
            line.numCornerVertices = 4;
            line.sortingLayerName = sortingLayer;
            line.sortingOrder = sortingOrder;
            line.startColor = energyColor;
            line.endColor = energyColor;
            return line;
        }

        private void DrawTrail(Vector3 start, Vector3 end, float progress)
        {
            if (energyParticles != null)
            {
                Vector3 tangent = Bezier(start, end, Mathf.Min(1f, progress + 0.02f))
                    - Bezier(start, end, Mathf.Max(0f, progress - 0.02f));
                energyParticles.transform.SetPositionAndRotation(Bezier(start, end, progress),
                    Quaternion.Euler(0f, 0f, Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg));
            }
            const int points = 10;
            trail.positionCount = points;
            trail.widthMultiplier = Mathf.Lerp(0.1f, 0.045f, progress);
            trail.startColor = new Color(energyColor.r, energyColor.g, energyColor.b, 0.15f + 0.7f * progress);
            trail.endColor = energyColor;
            float tail = Mathf.Min(0.28f, progress);
            for (int i = 0; i < points; i++)
            {
                float t = Mathf.Lerp(Mathf.Max(0f, progress - tail), progress, i / (points - 1f));
                trail.SetPosition(i, Bezier(start, end, t));
            }
        }

        private void DrawArrivalRing(Vector3 center, float progress)
        {
            const int points = 40;
            arrivalRing.positionCount = points;
            arrivalRing.widthMultiplier = Mathf.Lerp(0.09f, 0.025f, progress);
            Color color = energyColor;
            color.a = 1f - progress;
            arrivalRing.startColor = arrivalRing.endColor = color;
            float radius = Mathf.Lerp(0.12f, 0.7f, progress);
            for (int i = 0; i < points; i++)
            {
                float angle = i * Mathf.PI * 2f / points;
                arrivalRing.SetPosition(i, center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
            }
        }

        private Vector3 Bezier(Vector3 start, Vector3 end, float t)
        {
            Vector3 control = Vector3.Lerp(start, end, 0.5f) + Vector3.up * arcHeight;
            float u = 1f - t;
            return u * u * start + 2f * u * t * control + t * t * end;
        }

        private void Cleanup()
        {
            ReleasePlayerLock();
            CleanupVisuals();
        }

        private void ReleasePlayerLock()
        {
            if (!ownsPlayerLock) return;
            ownsPlayerLock = false;
            if (player != null && !player.IsDying) player.SetControlsLocked(false);
        }

        private void CleanupVisuals()
        {
            if (effectRoot != null) Destroy(effectRoot);
            effectRoot = null;
            energyParticles = null;
            trail = arrivalRing = null;
            if (effectMaterial != null) Destroy(effectMaterial);
            effectMaterial = null;
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            Cleanup();
        }
    }
}
