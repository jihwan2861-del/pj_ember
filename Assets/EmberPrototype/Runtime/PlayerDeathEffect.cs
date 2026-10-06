using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace EmberPrototype
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Ember Prototype/Player Death Effect")]
    public sealed class PlayerDeathEffect : MonoBehaviour
    {
        [Tooltip("사망 시 재생할 파티클입니다. 비워두면 자식 Death Particles를 찾습니다.")]
        [SerializeField] private ParticleSystem deathParticles;
        [Tooltip("사망 파티클을 보여준 뒤 화면 덮기를 시작하기까지의 시간입니다. (초)")]
        [SerializeField, Min(0.01f)] private float respawnDelay = 0.6f;
        [Tooltip("검은 와이프가 화면을 덮는 시간입니다. (초)")]
        [SerializeField, Min(0f)] private float wipeOutDuration = 0.22f;
        [Tooltip("리스폰 뒤 검은 와이프가 화면을 여는 시간입니다. (초)")]
        [SerializeField, Min(0f)] private float wipeInDuration = 0.25f;
        [Tooltip("화면을 완전히 덮은 상태로 유지하는 시간입니다. (초)")]
        [SerializeField, Min(0f)] private float blackHoldDuration = 0.04f;
        [Tooltip("와이프 경계의 계단 개수입니다.")]
        [SerializeField, Range(2, 16)] private int wipeSteps = 6;
        [Tooltip("사망 시 화면이 흔들리는 시간입니다. (초)")]
        [SerializeField, Min(0f)] private float shakeDuration = 0.12f;
        [Tooltip("사망 시 화면 흔들림의 크기입니다. (월드 단위)")]
        [SerializeField, Min(0f)] private float shakeMagnitude = 0.12f;

        private Coroutine sequence;
        private SpriteRenderer[] sprites;
        private bool[] spriteVisibility;
        private Light2D[] lights;
        private bool[] lightVisibility;
        private ParticleSystem[] ambient;
        private bool[] ambientPlaying;
        private PlayerFlameFeedback feedback;
        private bool feedbackEnabled;
        private DeathScreenWipe screenWipe;
        private bool resettingUnderCover;
        public bool IsPlaying { get; private set; }

        private void Awake()
        {
            ResolveParticleReference();
            sprites = GetComponentsInChildren<SpriteRenderer>(true);
            spriteVisibility = new bool[sprites.Length];
            lights = GetComponentsInChildren<Light2D>(true);
            lightVisibility = new bool[lights.Length];
            ambient = GetComponentsInChildren<ParticleSystem>(true);
            ambientPlaying = new bool[ambient.Length];
            feedback = GetComponent<PlayerFlameFeedback>();
        }

        private void Reset() => ResolveParticleReference();

        private void ResolveParticleReference()
        {
            if (deathParticles != null) return;
            Transform child = transform.Find("Death Particles");
            if (child != null) deathParticles = child.GetComponent<ParticleSystem>();
        }

        public void Play(Action respawn)
        {
            if (IsPlaying) return;
            IsPlaying = true;
            ResolveParticleReference();
            feedbackEnabled = feedback != null && feedback.enabled;
            if (feedback != null) feedback.enabled = false;
            for (int i = 0; i < sprites.Length; i++)
            {
                spriteVisibility[i] = sprites[i].enabled;
                sprites[i].enabled = false;
            }
            for (int i = 0; i < lights.Length; i++)
            {
                lightVisibility[i] = lights[i].enabled;
                lights[i].enabled = false;
            }
            for (int i = 0; i < ambient.Length; i++)
            {
                ambientPlaying[i] = ambient[i].isPlaying;
                if (!IsDeathParticle(ambient[i]))
                    ambient[i].Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            if (deathParticles != null)
            {
                deathParticles.gameObject.SetActive(true);
                deathParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                deathParticles.Play(true);
            }
            Camera.main?.GetComponent<CelesteRoomCamera>()?.Shake(shakeDuration, shakeMagnitude);
            sequence = StartCoroutine(RespawnAfterEffect(respawn));
        }

        private bool IsDeathParticle(ParticleSystem particles) => deathParticles != null
            && (particles == deathParticles || particles.transform.IsChildOf(deathParticles.transform));

        private IEnumerator RespawnAfterEffect(Action respawn)
        {
            // Aiming slow motion must never extend the retry delay.
            yield return new WaitForSecondsRealtime(respawnDelay);
            EnsureScreenWipe();
            screenWipe.gameObject.SetActive(true);
            yield return AnimateWipe(0f, 1f, wipeOutDuration);
            // ResetAt normally cancels death; this reset belongs to the ongoing transition.
            resettingUnderCover = true;
            RestorePresentation();
            try { respawn?.Invoke(); }
            finally { resettingUnderCover = false; }
            var player = GetComponent<FlamePlayerController>();
            player.SetControlsLocked(true);
            Camera.main?.GetComponent<CelesteRoomCamera>()?.SnapAfterRespawn(
                GetComponent<Rigidbody2D>().position);
            // Allow physics interpolation and the camera to settle behind full coverage.
            yield return null;
            if (blackHoldDuration > 0f) yield return new WaitForSecondsRealtime(blackHoldDuration);
            Camera.main?.GetComponent<CelesteRoomCamera>()?.SnapAfterRespawn(
                GetComponent<Rigidbody2D>().position);
            yield return AnimateWipe(1f, 0f, wipeInDuration);
            screenWipe.gameObject.SetActive(false);
            sequence = null;
            IsPlaying = false;
            player.SetControlsLocked(false);
        }

        private void EnsureScreenWipe()
        {
            if (screenWipe != null) return;
            var overlay = new GameObject("Death Screen Wipe", typeof(RectTransform), typeof(Canvas));
            overlay.transform.SetParent(transform, false);
            var canvas = overlay.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32767;
            var graphic = new GameObject("Staircase Mask", typeof(RectTransform), typeof(CanvasRenderer));
            graphic.transform.SetParent(overlay.transform, false);
            var rect = graphic.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            screenWipe = graphic.AddComponent<DeathScreenWipe>();
            screenWipe.color = Color.black;
            screenWipe.raycastTarget = false;
        }

        private IEnumerator AnimateWipe(float from, float to, float duration)
        {
            screenWipe.SetCoverage(from, wipeSteps);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                screenWipe.SetCoverage(Mathf.Lerp(from, to, t * t * (3f - 2f * t)), wipeSteps);
                yield return null;
            }
            screenWipe.SetCoverage(to, wipeSteps);
        }

        public void Cancel()
        {
            if (resettingUnderCover) return;
            if (sequence != null) StopCoroutine(sequence);
            sequence = null;
            if (screenWipe != null) screenWipe.gameObject.SetActive(false);
            if (IsPlaying) RestorePresentation();
            IsPlaying = false;
        }

        private void RestorePresentation()
        {
            if (deathParticles != null)
                deathParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            for (int i = 0; i < sprites.Length; i++)
                if (sprites[i] != null) sprites[i].enabled = spriteVisibility[i];
            for (int i = 0; i < lights.Length; i++)
                if (lights[i] != null) lights[i].enabled = lightVisibility[i];
            if (feedback != null) feedback.enabled = feedbackEnabled;
            for (int i = 0; i < ambient.Length; i++)
                if (ambient[i] != null && ambientPlaying[i] && !IsDeathParticle(ambient[i]))
                    ambient[i].Play(false);
        }

        private void OnDisable() => Cancel();
    }
}
