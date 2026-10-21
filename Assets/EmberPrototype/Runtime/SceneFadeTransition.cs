using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EmberPrototype
{
    public sealed class SceneFadeTransition : MonoBehaviour
    {
        private static SceneFadeTransition instance;
        private CanvasGroup overlay;
        private FlamePlayerController lockedPlayer;
        public static bool IsPlaying => instance != null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => instance = null;

        public static bool TryBegin(string scene, FlamePlayerController player, float fadeOut, float fadeIn)
        {
            if (IsPlaying || SceneRestartWipe.IsPlaying || GameSessionControls.IsPaused) return false;
            if (string.IsNullOrWhiteSpace(scene) || !Application.CanStreamedLevelBeLoaded(scene))
            {
                Debug.LogError("이동할 씬을 Build Profiles의 Scene List에 등록해주세요: " + scene);
                return false;
            }
            CreateOverlay(player);
            instance.StartCoroutine(instance.Run(scene, Mathf.Max(0f, fadeOut), Mathf.Max(0f, fadeIn)));
            return true;
        }

        public static bool TryFade(FlamePlayerController player, float fadeOut, float fadeIn, System.Action onBlack)
        {
            if (IsPlaying || SceneRestartWipe.IsPlaying || GameSessionControls.IsPaused) return false;
            CreateOverlay(player);
            instance.StartCoroutine(instance.RunInScene(Mathf.Max(0f, fadeOut), Mathf.Max(0f, fadeIn), onBlack));
            return true;
        }

        private static void CreateOverlay(FlamePlayerController player)
        {
            var root = new GameObject("Scene Fade Transition", typeof(RectTransform), typeof(Canvas), typeof(CanvasGroup));
            DontDestroyOnLoad(root);
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32767;
            instance = root.AddComponent<SceneFadeTransition>();
            instance.overlay = root.GetComponent<CanvasGroup>();
            instance.overlay.alpha = 0f;
            instance.overlay.blocksRaycasts = true;
            var shade = new GameObject("Black Overlay", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            shade.transform.SetParent(root.transform, false);
            var rect = shade.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            shade.GetComponent<UnityEngine.UI.Image>().color = Color.black;
            if (player != null && !player.ControlsLocked)
            {
                player.SetControlsLocked(true);
                instance.lockedPlayer = player;
            }
        }

        private IEnumerator RunInScene(float fadeOut, float fadeIn, System.Action onBlack)
        {
            yield return Fade(0f, 1f, fadeOut);
            yield return null;
            try { onBlack?.Invoke(); }
            catch (System.Exception exception) { Debug.LogException(exception); }
            yield return null;
            yield return Fade(1f, 0f, fadeIn);
            Destroy(gameObject);
        }

        private IEnumerator Run(string scene, float fadeOut, float fadeIn)
        {
            yield return Fade(0f, 1f, fadeOut);
            yield return null;
            AsyncOperation load = null;
            try { load = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single); }
            catch (System.Exception exception) { Debug.LogException(exception); }
            if (load != null) yield return load;
            yield return null;
            yield return Fade(1f, 0f, fadeIn);
            Destroy(gameObject);
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            overlay.alpha = from;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                overlay.alpha = Mathf.SmoothStep(from, to, duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f);
                yield return null;
            }
            overlay.alpha = to;
        }

        private void OnDestroy()
        {
            if (lockedPlayer != null) lockedPlayer.SetControlsLocked(false);
            if (instance == this) instance = null;
        }
    }
}
