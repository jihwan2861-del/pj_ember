using System;
using System.Collections;
using UnityEngine;

namespace EmberPrototype
{
    [DisallowMultipleComponent]
    public sealed class EmberBirthSequence : MonoBehaviour
    {
        public enum BirthPhase { Idle, WaitingForFade, Resonating, ReceivingLight, Transforming, Revealing, Returning, Completed, Skipped }

        [Header("Scene References")]
        [SerializeField] private FlamePlayerController player;
        [SerializeField] private StoryPlayerAppearance appearance;
        [SerializeField] private CelesteRoomCamera roomCamera;
        [SerializeField] private Transform cameraFocus;
        [SerializeField] private Transform flowerHead;
        [SerializeField] private SpriteRenderer flowerPetals;
        [SerializeField] private Transform lightOrigin;

        [Header("Timing — Game Seconds")]
        [SerializeField, Min(0.01f)] private float resonanceSeconds = 0.9f;
        [SerializeField, Min(0.01f)] private float lightTravelSeconds = 0.85f;
        [SerializeField, Min(0.01f)] private float transformationSeconds = 0.55f;
        [SerializeField, Min(0f)] private float revealSeconds = 0.9f;
        [SerializeField, Min(0.01f)] private float cameraReturnSeconds = 0.55f;
        [SerializeField, Range(0.3f, 1f)] private float cameraZoom = 0.7f;

        [Header("Light")]
        [SerializeField] private Color lightColor = new Color(1f, 0.68f, 0.1f, 1f);
        [SerializeField] private string sortingLayer = "EmberWater";
        [SerializeField] private int sortingOrder = 30;
        [Header("Particle Accent")]
        [SerializeField] private ParticleSystem birthParticlesPrefab;
        private ParticleSystem birthParticles;
        private bool birthParticlesPlayed;
        private Coroutine running;
        private Action completed;
        private bool ownsControlLock;
        private bool ownsCamera;
        private Vector3 headScale;
        private Quaternion headRotation;
        private Color petalsColor;
        private GameObject effects;
        private Material effectMaterial;
        private LineRenderer lightMote;
        private LineRenderer birthRing;
        private LineRenderer[] sparks;

        public BirthPhase CurrentPhase { get; private set; }
        public bool IsPlaying => running != null;

        public bool TryBegin(Action onCompleted)
        {
            if (!isActiveAndEnabled || CurrentPhase != BirthPhase.Idle || player == null || appearance == null
                || flowerHead == null || flowerPetals == null || lightOrigin == null || player.ControlsLocked)
                return false;
            completed = onCompleted;
            birthParticlesPlayed = false;
            headScale = flowerHead.localScale;
            headRotation = flowerHead.localRotation;
            petalsColor = flowerPetals.color;
            appearance.ShowFire();
            player.SetControlsLocked(true);
            ownsControlLock = true;
            CurrentPhase = BirthPhase.WaitingForFade;
            running = StartCoroutine(Play());
            return true;
        }

        private IEnumerator Play()
        {
            // New scenes prepare the fire form before the incoming fade reveals them.
            yield return null;
            while (SceneFadeTransition.IsPlaying || SceneRestartWipe.IsPlaying || GameSessionControls.IsPaused)
                yield return null;

            CreateEffects();
            if (roomCamera != null && cameraFocus != null)
            {
                roomCamera.BeginCinematic(cameraFocus, cameraZoom, 0.45f);
                ownsCamera = true;
            }

            CurrentPhase = BirthPhase.Resonating;
            for (float elapsed = 0f; elapsed < resonanceSeconds; elapsed += Time.deltaTime)
            {
                float t = Mathf.Clamp01(elapsed / resonanceSeconds);
                float pulse = Mathf.Sin(t * Mathf.PI * 3f) * 0.035f;
                flowerHead.localScale = headScale * (0.96f + 0.1f * t + pulse);
                flowerHead.localRotation = headRotation * Quaternion.Euler(0f, 0f, Mathf.Sin(t * Mathf.PI) * 7f);
                flowerPetals.color = Color.Lerp(petalsColor, new Color(1f, 0.86f, 0.5f), t * 0.4f);
                DrawCircle(lightMote, lightOrigin.position, 0.08f + 0.1f * t, 0.02f, 0.9f);
                yield return null;
            }

            CurrentPhase = BirthPhase.ReceivingLight;
            Vector3 from = lightOrigin.position;
            for (float elapsed = 0f; elapsed < lightTravelSeconds; elapsed += Time.deltaTime)
            {
                float t = Mathf.SmoothStep(0f, 1f, elapsed / lightTravelSeconds);
                Vector3 to = PlayerCenter;
                Vector3 position = Vector3.Lerp(from, to, t) + Vector3.up * Mathf.Sin(t * Mathf.PI) * 0.5f;
                DrawCircle(lightMote, position, 0.13f, 0.045f, 1f);
                for (int i = 0; i < sparks.Length; i++)
                {
                    float angle = t * Mathf.PI * 4f + i * Mathf.PI * 0.5f;
                    Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * (0.1f + 0.14f * (1f - t));
                    DrawCircle(sparks[i], position + offset, 0.025f, 0.022f, 0.8f);
                }
                yield return null;
            }

            CurrentPhase = BirthPhase.Transforming;
            bool transformed = false;
            for (float elapsed = 0f; elapsed < transformationSeconds; elapsed += Time.deltaTime)
            {
                float t = Mathf.Clamp01(elapsed / transformationSeconds);
                if (!transformed && t >= 0.35f)
                {
                    appearance.ShowEmber();
                    PlayBirthParticles();
                    transformed = true;
                }
                DrawCircle(lightMote, PlayerCenter, Mathf.Lerp(0.18f, 0.02f, t), 0.05f, 1f - t);
                DrawCircle(birthRing, PlayerCenter, Mathf.Lerp(0.12f, 1.2f, t), 0.04f, 1f - t);
                for (int i = 0; i < sparks.Length; i++)
                {
                    float angle = i * Mathf.PI * 0.5f + 0.4f;
                    Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * Mathf.Lerp(0.2f, 0.85f, t);
                    DrawCircle(sparks[i], PlayerCenter + offset, 0.035f, 0.025f, 1f - t);
                }
                yield return null;
            }
            appearance.ShowEmber();
            PlayBirthParticles();
            if (effects != null) effects.SetActive(false);
            CurrentPhase = BirthPhase.Revealing;
            yield return WaitGameSeconds(revealSeconds);
            CurrentPhase = BirthPhase.Returning;
            if (ownsCamera && roomCamera != null) roomCamera.EndCinematic(cameraReturnSeconds);
            yield return WaitGameSeconds(cameraReturnSeconds);
            Finish(false);
        }

        private Vector3 PlayerCenter => player.transform.position + Vector3.up * 0.2f;

        private static IEnumerator WaitGameSeconds(float seconds)
        {
            for (float elapsed = 0f; elapsed < seconds; elapsed += Time.deltaTime) yield return null;
        }

        public void SkipBirth()
        {
            if (running == null) return;
            StopCoroutine(running);
            Finish(true);
        }

        private void Finish(bool skipped)
        {
            running = null;
            CurrentPhase = skipped ? BirthPhase.Skipped : BirthPhase.Completed;
            if (appearance != null) appearance.ShowEmber();
            RestorePresentation();
            if (ownsControlLock)
            {
                ownsControlLock = false;
                if (player != null && !player.IsDying) player.SetControlsLocked(false);
            }
            var callback = completed;
            completed = null;
            if (player != null && player.isActiveAndEnabled && gameObject.scene.isLoaded) callback?.Invoke();
        }

        private void ReleaseCamera(float seconds)
        {
            if (!ownsCamera) return;
            ownsCamera = false;
            if (roomCamera != null) roomCamera.EndCinematic(seconds);
        }

        private void RestorePresentation()
        {
            ReleaseCamera(0f);
            if (birthParticles != null) Destroy(birthParticles.gameObject);
            birthParticles = null;
            if (flowerHead != null)
            {
                flowerHead.localScale = headScale;
                flowerHead.localRotation = headRotation;
            }
            if (flowerPetals != null) flowerPetals.color = petalsColor;
            if (effects != null) Destroy(effects);
            if (effectMaterial != null) Destroy(effectMaterial);
            effects = null;
            effectMaterial = null;
        }

        private void PlayBirthParticles()
        {
            if (birthParticlesPrefab == null || birthParticlesPlayed) return;
            birthParticlesPlayed = true;
            // The sequence owns this object so Skip/disable cleans it up along with the light lines.
            birthParticles = Instantiate(birthParticlesPrefab, PlayerCenter, Quaternion.identity, transform);
            int order = sortingOrder + 1;
            foreach (var renderer in birthParticles.GetComponentsInChildren<ParticleSystemRenderer>(true))
            {
                renderer.sortingLayerName = sortingLayer;
                renderer.sortingOrder = order++;
            }
            birthParticles.Play(true);
        }

        private void CreateEffects()
        {
            effects = new GameObject("Birth Light");
            effects.transform.SetParent(transform, false);
            var shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (shader != null) effectMaterial = new Material(shader);
            lightMote = CreateLine("Flower Light");
            birthRing = CreateLine("Birth Ring");
            sparks = new LineRenderer[4];
            for (int i = 0; i < sparks.Length; i++) sparks[i] = CreateLine("Spark " + i);
        }

        private LineRenderer CreateLine(string label)
        {
            var obj = new GameObject(label);
            obj.transform.SetParent(effects.transform, false);
            var line = obj.AddComponent<LineRenderer>();
            line.sharedMaterial = effectMaterial;
            line.useWorldSpace = true;
            line.loop = true;
            line.positionCount = 40;
            line.numCornerVertices = line.numCapVertices = 2;
            line.sortingLayerName = sortingLayer;
            line.sortingOrder = sortingOrder;
            line.enabled = false;
            return line;
        }

        private void DrawCircle(LineRenderer line, Vector3 center, float radius, float width, float alpha)
        {
            line.enabled = alpha > 0.001f;
            line.widthMultiplier = width;
            Color color = lightColor;
            color.a *= alpha;
            line.startColor = line.endColor = color;
            for (int i = 0; i < line.positionCount; i++)
            {
                float angle = i * Mathf.PI * 2f / line.positionCount;
                float roughRadius = radius * (1f + 0.035f * Mathf.Sin(i * 2.7f));
                line.SetPosition(i, center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * roughRadius);
            }
        }

        private void OnDisable()
        {
            if (running == null) return;
            StopCoroutine(running);
            // Unload/cancellation cleans up without beginning a new exercise.
            // The explicit SkipBirth action resumes the tutorial instead.
            completed = null;
            Finish(true);
        }
    }
}
