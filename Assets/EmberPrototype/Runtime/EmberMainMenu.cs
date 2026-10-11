using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EmberPrototype
{
    /// <summary>Editable title-screen UI. Does not create gameplay objects or change gameplay input.</summary>
    public sealed class EmberMainMenu : MonoBehaviour
    {
        public const string VolumePreference = "Ember.Settings.MasterVolume";
        public const string FullscreenPreference = "Ember.Settings.Fullscreen";
        public const string WidthPreference = "Ember.Settings.Width";
        public const string HeightPreference = "Ember.Settings.Height";

        [Header("Game start")]
        [SerializeField, Tooltip("Enabled scene name in Build Profiles. No .unity extension.")]
        private string gameSceneName = "game 1";
        [SerializeField, Min(0f)] private float fadeOutSeconds = 0.45f;
        [SerializeField] private Button startButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private CanvasGroup mainMenuGroup;
        [SerializeField] private CanvasGroup fadeOverlay;
        [SerializeField] private Text menuStatus;
        [SerializeField] private InputSystemUIInputModule inputModule;

        [Header("Settings panel")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Text volumeValue;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Button resolutionButton;
        [SerializeField] private Text resolutionValue;
        [SerializeField] private Button applyButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Text settingsStatus;
        [SerializeField] private Vector2Int[] resolutionOptions =
        {
            new Vector2Int(1280, 720), new Vector2Int(1600, 900),
            new Vector2Int(1920, 1080), new Vector2Int(2560, 1440)
        };

        private int resolutionIndex;
        private bool loading;
        private InputAction cancelAction;
        public bool IsLoading => loading;
        public bool IsSettingsOpen => settingsPanel != null && settingsPanel.activeSelf;

        private void Awake()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            AudioListener.volume = Mathf.Clamp01(PlayerPrefs.GetFloat(VolumePreference, 1f));
            if (!Application.isEditor && PlayerPrefs.HasKey(WidthPreference))
            {
                ApplyDisplay(PlayerPrefs.GetInt(WidthPreference, Screen.width),
                    PlayerPrefs.GetInt(HeightPreference, Screen.height),
                    PlayerPrefs.GetInt(FullscreenPreference, Screen.fullScreen ? 1 : 0) == 1);
            }
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (fadeOverlay != null)
            {
                fadeOverlay.alpha = 0f;
                fadeOverlay.blocksRaycasts = false;
                fadeOverlay.interactable = false;
            }
        }

        private void OnEnable()
        {
            if (startButton != null) startButton.onClick.AddListener(StartGame);
            if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
            if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
            if (backButton != null) backButton.onClick.AddListener(CloseSettings);
            if (applyButton != null) applyButton.onClick.AddListener(ApplySettings);
            if (resolutionButton != null) resolutionButton.onClick.AddListener(CycleResolution);
            if (volumeSlider != null) volumeSlider.onValueChanged.AddListener(SetMasterVolume);
            if (inputModule != null && inputModule.cancel != null)
            {
                cancelAction = inputModule.cancel.action;
                if (cancelAction != null) cancelAction.performed += OnCancel;
            }
        }

        private void Start() => Select(startButton);

        private void OnDisable()
        {
            if (startButton != null) startButton.onClick.RemoveListener(StartGame);
            if (settingsButton != null) settingsButton.onClick.RemoveListener(OpenSettings);
            if (quitButton != null) quitButton.onClick.RemoveListener(QuitGame);
            if (backButton != null) backButton.onClick.RemoveListener(CloseSettings);
            if (applyButton != null) applyButton.onClick.RemoveListener(ApplySettings);
            if (resolutionButton != null) resolutionButton.onClick.RemoveListener(CycleResolution);
            if (volumeSlider != null) volumeSlider.onValueChanged.RemoveListener(SetMasterVolume);
            if (cancelAction != null) cancelAction.performed -= OnCancel;
            cancelAction = null;
        }

        public void OpenSettings()
        {
            if (loading || settingsPanel == null) return;
            if (volumeSlider != null) volumeSlider.SetValueWithoutNotify(AudioListener.volume);
            SetVolumeLabel(AudioListener.volume);
            if (fullscreenToggle != null)
                fullscreenToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt(FullscreenPreference,
                    Screen.fullScreen ? 1 : 0) == 1);
            resolutionIndex = FindClosestResolution(PlayerPrefs.GetInt(WidthPreference, Screen.width),
                PlayerPrefs.GetInt(HeightPreference, Screen.height));
            UpdateResolutionLabel();
            if (settingsStatus != null) settingsStatus.text = "";
            SetMenuInteractive(false);
            settingsPanel.SetActive(true);
            if (volumeSlider != null) Select(volumeSlider);
        }

        public void CloseSettings()
        {
            if (loading || !IsSettingsOpen) return;
            PlayerPrefs.Save();
            settingsPanel.SetActive(false);
            SetMenuInteractive(true);
            Select(settingsButton);
        }

        public void SetMasterVolume(float volume)
        {
            volume = Mathf.Clamp01(volume);
            AudioListener.volume = volume;
            PlayerPrefs.SetFloat(VolumePreference, volume);
            SetVolumeLabel(volume);
        }

        public void CycleResolution()
        {
            if (resolutionOptions == null || resolutionOptions.Length == 0) return;
            resolutionIndex = (resolutionIndex + 1) % resolutionOptions.Length;
            UpdateResolutionLabel();
        }

        public void ApplySettings()
        {
            if (!IsSettingsOpen) return;
            bool fullscreen = fullscreenToggle != null && fullscreenToggle.isOn;
            PlayerPrefs.SetInt(FullscreenPreference, fullscreen ? 1 : 0);
            if (resolutionOptions != null && resolutionOptions.Length > 0)
            {
                Vector2Int size = resolutionOptions[Mathf.Clamp(resolutionIndex, 0, resolutionOptions.Length - 1)];
                PlayerPrefs.SetInt(WidthPreference, size.x);
                PlayerPrefs.SetInt(HeightPreference, size.y);
                if (!Application.isEditor) ApplyDisplay(size.x, size.y, fullscreen);
            }
            PlayerPrefs.Save();
            if (settingsStatus != null) settingsStatus.text = Application.isEditor
                ? "저장됨 · 화면 설정은 빌드에서 적용됩니다."
                : "설정이 적용되었습니다.";
        }

        public void StartGame()
        {
            if (loading || IsSettingsOpen) return;
            if (string.IsNullOrWhiteSpace(gameSceneName) || !Application.CanStreamedLevelBeLoaded(gameSceneName))
            {
                if (menuStatus != null) menuStatus.text = "시작 씬을 Build Profiles에 추가해 주세요: " + gameSceneName;
                Debug.LogWarning("Ember main menu cannot load the configured scene: " + gameSceneName, this);
                return;
            }
            StartCoroutine(LoadGame());
        }

        private IEnumerator LoadGame()
        {
            loading = true;
            SetMenuInteractive(false);
            PlayerPrefs.Save();
            if (menuStatus != null) menuStatus.text = "불씨를 깨우는 중…";
            if (fadeOverlay != null)
            {
                fadeOverlay.blocksRaycasts = true;
                for (float elapsed = 0f; elapsed < fadeOutSeconds; elapsed += Time.unscaledDeltaTime)
                {
                    fadeOverlay.alpha = Mathf.Clamp01(elapsed / fadeOutSeconds);
                    yield return null;
                }
                fadeOverlay.alpha = 1f;
            }
            AsyncOperation operation = null;
            try { operation = SceneManager.LoadSceneAsync(gameSceneName, LoadSceneMode.Single); }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                if (menuStatus != null) menuStatus.text = "씬을 불러오지 못했습니다.";
            }
            if (operation == null)
            {
                loading = false;
                SetMenuInteractive(true);
                if (fadeOverlay != null) { fadeOverlay.alpha = 0f; fadeOverlay.blocksRaycasts = false; }
                Select(startButton);
                yield break;
            }
            while (!operation.isDone) yield return null;
        }

        public void QuitGame()
        {
            if (loading || IsSettingsOpen) return;
            PlayerPrefs.Save();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnCancel(InputAction.CallbackContext context)
        {
            if (IsSettingsOpen) CloseSettings();
        }

        private static void ApplyDisplay(int width, int height, bool fullscreen)
        {
            Screen.SetResolution(Mathf.Max(640, width), Mathf.Max(360, height),
                fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
        }

        private int FindClosestResolution(int width, int height)
        {
            if (resolutionOptions == null || resolutionOptions.Length == 0) return 0;
            int best = 0;
            float distance = float.MaxValue;
            for (int i = 0; i < resolutionOptions.Length; i++)
            {
                float candidate = ((Vector2)resolutionOptions[i] - new Vector2(width, height)).sqrMagnitude;
                if (candidate < distance) { best = i; distance = candidate; }
            }
            return best;
        }

        private void UpdateResolutionLabel()
        {
            if (resolutionValue == null || resolutionOptions == null || resolutionOptions.Length == 0) return;
            Vector2Int size = resolutionOptions[Mathf.Clamp(resolutionIndex, 0, resolutionOptions.Length - 1)];
            resolutionValue.text = size.x + " × " + size.y + "   ›";
        }

        private void SetVolumeLabel(float volume)
        {
            if (volumeValue != null) volumeValue.text = Mathf.RoundToInt(volume * 100f) + "%";
        }

        private void SetMenuInteractive(bool interactive)
        {
            if (mainMenuGroup == null) return;
            mainMenuGroup.interactable = interactive;
            mainMenuGroup.blocksRaycasts = interactive;
        }

        private static void Select(Selectable selectable)
        {
            if (selectable != null && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(selectable.gameObject);
        }
    }
}
