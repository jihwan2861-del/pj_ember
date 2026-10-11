using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EmberPrototype
{
    // The overlay outlives the old scene, so the new scene is revealed without a flash.
    public sealed class SceneRestartWipe : MonoBehaviour
    {
        private static SceneRestartWipe instance;
        private DeathScreenWipe graphic;
        private bool running;
        public static bool IsPlaying => instance != null && instance.running;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => instance = null;

        internal static void Begin(Scene scene, PlayerDeathEffect settings)
        {
            if (IsPlaying) return;
            bool canLoad = scene.buildIndex >= 0;
#if UNITY_EDITOR
            canLoad |= !string.IsNullOrEmpty(scene.path);
#endif
            if (!canLoad)
            {
                Debug.LogError("Save this scene and include it in the build scene list to restart it.");
                return;
            }
            float close = settings != null ? settings.WipeOutDuration : 0.22f;
            float open = settings != null ? settings.WipeInDuration : 0.25f;
            float hold = settings != null ? settings.BlackHoldDuration : 0.04f;
            int steps = settings != null ? settings.WipeSteps : 6;
            foreach (var player in Object.FindObjectsByType<FlamePlayerController>(FindObjectsSortMode.None))
            {
                player.CancelRingShotAim();
                player.GetComponent<PlayerDeathEffect>()?.Cancel();
            }
            GameObject root = new GameObject("Restart Screen Wipe", typeof(RectTransform), typeof(Canvas));
            Object.DontDestroyOnLoad(root);
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32767;
            instance = root.AddComponent<SceneRestartWipe>();
            GameObject mask = new GameObject("Staircase Mask", typeof(RectTransform), typeof(CanvasRenderer));
            mask.transform.SetParent(root.transform, false);
            RectTransform rect = mask.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            instance.graphic = mask.AddComponent<DeathScreenWipe>();
            instance.graphic.color = Color.black;
            instance.graphic.raycastTarget = true;
            instance.running = true;
            instance.StartCoroutine(instance.Transition(scene.buildIndex, scene.path, close, open, hold, steps));
        }
        private IEnumerator Transition(int buildIndex, string path, float close, float open, float hold, int steps)
        {
            Time.timeScale = 0f;
            yield return Animate(0f, 1f, close, steps);
            // Render full coverage for a frame before replacing the scene.
            yield return null;
#if UNITY_EDITOR
            if (buildIndex < 0)
                UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(path, new LoadSceneParameters(LoadSceneMode.Single));
            else
#endif
                SceneManager.LoadScene(buildIndex);
            yield return null;
            float elapsed = 0f;
            while (elapsed < hold) { elapsed += Time.unscaledDeltaTime; yield return null; }
            yield return Animate(1f, 0f, open, steps);
            running = false;
            Time.timeScale = 1f;
            Destroy(gameObject);
        }
        private IEnumerator Animate(float from, float to, float duration, int steps)
        {
            graphic.SetCoverage(from, steps);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                graphic.SetCoverage(Mathf.Lerp(from, to, t * t * (3f - 2f * t)), steps);
                yield return null;
            }
            graphic.SetCoverage(to, steps);
        }
        private void OnDisable()
        {
            if (instance != this) return;
            if (running) Time.timeScale = 1f;
            running = false;
            instance = null;
        }
    }
}
