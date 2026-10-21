using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace EmberPrototype
{
    [DefaultExecutionOrder(-1000)]
    [DisallowMultipleComponent]
    public sealed class GameSessionControls : MonoBehaviour
    {
        private static GameSessionControls instance;
        private bool paused;
        private float previousTimeScale = 1f;
        private int inputConsumedFrame = -1;
        public static bool IsPaused => instance != null && instance.paused;
        public static float PlaybackSpeed { get; private set; } = 1f;
        public static bool BlockGameplayInput => StorybookNarration.BlocksGameplayInput
            || SceneFadeTransition.IsPlaying || SceneRestartWipe.IsPlaying || (instance != null
            && (instance.paused || instance.inputConsumedFrame == Time.frameCount));

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            instance = null;
            PlaybackSpeed = 1f;
        }
        private void OnEnable()
        {
            if (instance == null) instance = this;
        }
        private void Update()
        {
            if (instance != this || SceneRestartWipe.IsPlaying || SceneFadeTransition.IsPlaying || Keyboard.current == null) return;
            bool faster = Keyboard.current.rightBracketKey.wasPressedThisFrame;
            bool slower = Keyboard.current.leftBracketKey.wasPressedThisFrame;
            if (faster != slower) SetPlaybackSpeed(PlaybackSpeed * (faster ? 2f : 0.5f));
            if (Keyboard.current.rKey.wasPressedThisFrame) RestartScene();
            else if (Keyboard.current.escapeKey.wasPressedThisFrame) SetPaused(!paused);
        }
        public void SetPlaybackSpeed(float speed)
        {
            if (instance != this || SceneRestartWipe.IsPlaying || SceneFadeTransition.IsPlaying) return;
            FlowerRideTutorial.EndSessionSlowMotion();
            // End temporary aiming slow motion before changing its saved base speed.
            foreach (var player in FindObjectsByType<FlamePlayerController>())
                player.CancelRingShotAim();
            PlaybackSpeed = Mathf.Clamp(speed, 1f / 32f, 32f);
            previousTimeScale = PlaybackSpeed;
            Time.timeScale = paused ? 0f : PlaybackSpeed;
            Debug.Log("재생 속도: " + PlaybackSpeed.ToString("0.#####") + "배", this);
        }
        public void SetPaused(bool value)
        {
            if (instance != this || SceneRestartWipe.IsPlaying || SceneFadeTransition.IsPlaying || paused == value) return;
            FlowerRideTutorial.EndSessionSlowMotion();
            inputConsumedFrame = Time.frameCount;
            if (value)
            {
                // End aiming before owning timeScale, so its cleanup cannot undo the pause.
                foreach (var player in FindObjectsByType<FlamePlayerController>(FindObjectsSortMode.None))
                    player.CancelRingShotAim();
                previousTimeScale = Time.timeScale;
                paused = true;
                Time.timeScale = 0f;
            }
            else
            {
                paused = false;
                Time.timeScale = previousTimeScale;
            }
        }
        public void RestartScene()
        {
            if (instance != this || SceneRestartWipe.IsPlaying || SceneFadeTransition.IsPlaying) return;
            FlowerRideTutorial.EndSessionSlowMotion();
            Scene scene = gameObject.scene;
            inputConsumedFrame = Time.frameCount;
            SetPaused(false);
            foreach (var player in FindObjectsByType<FlamePlayerController>(FindObjectsSortMode.None))
                player.CancelRingShotAim();
            Time.timeScale = 1f;
            SceneRestartWipe.Begin(scene, GetComponent<PlayerDeathEffect>());
        }
        private void OnDisable()
        {
            if (instance != this) return;
            if (paused) Time.timeScale = previousTimeScale;
            paused = false;
            instance = null;
        }
        private void OnGUI()
        {
            if (instance != this || !paused) return;
            Color previous = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.65f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = previous;
            float x = (Screen.width - 300f) * 0.5f, y = (Screen.height - 180f) * 0.5f;
            GUI.Box(new Rect(x, y, 300f, 180f), "PAUSED");
            GUI.Label(new Rect(x + 45f, y + 35f, 240f, 30f), "Esc: Resume    R: Restart");
            if (GUI.Button(new Rect(x + 40f, y + 75f, 220f, 35f), "Resume")) SetPaused(false);
            if (GUI.Button(new Rect(x + 40f, y + 120f, 220f, 35f), "Restart")) RestartScene();
        }
    }
}
