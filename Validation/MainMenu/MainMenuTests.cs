using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace MainMenuValidation
{
    public sealed class MainMenuTests
    {
        private MonoBehaviour menu;
        private const string VolumePreference = "Ember.Settings.MasterVolume";
        private const string FullscreenPreference = "Ember.Settings.Fullscreen";
        private const string WidthPreference = "Ember.Settings.Width";
        private const string HeightPreference = "Ember.Settings.Height";
        private Keyboard keyboard;
        private Mouse mouse;
        private InputSettings.BackgroundBehavior previousBackground;
        private InputSettings.EditorInputBehaviorInPlayMode previousEditorInput;

        [UnitySetUp]
        public IEnumerator LoadMenu()
        {
            Time.timeScale = 1f;
            previousBackground = InputSystem.settings.backgroundBehavior;
            previousEditorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            foreach (string key in new[] { VolumePreference, FullscreenPreference,
                         WidthPreference, HeightPreference }) PlayerPrefs.DeleteKey(key);
            keyboard = InputSystem.AddDevice<Keyboard>();
            mouse = InputSystem.AddDevice<Mouse>();
            yield return SceneManager.LoadSceneAsync("EmberMainMenu");
            yield return null;
            yield return null;
            menu = GameObject.Find("Main Menu Controller").GetComponent<MonoBehaviour>();
            Assert.NotNull(menu);
            Assert.IsTrue(Object.FindFirstObjectByType<InputSystemUIInputModule>().move.action.enabled);
        }

        [TearDown]
        public void Cleanup()
        {
            Time.timeScale = 1f;
            InputSystem.settings.backgroundBehavior = previousBackground;
            InputSystem.settings.editorInputBehaviorInPlayMode = previousEditorInput;
            if (keyboard != null && keyboard.added) InputSystem.RemoveDevice(keyboard);
            if (mouse != null && mouse.added) InputSystem.RemoveDevice(mouse);
            foreach (string key in new[] { VolumePreference, FullscreenPreference,
                         WidthPreference, HeightPreference }) PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }

        [UnityTest]
        public IEnumerator KeyboardNavigationSubmitCancelAndWrap()
        {
            Assert.AreEqual("Game Start", EventSystem.current.currentSelectedGameObject.name);
            yield return Tap(Key.UpArrow);
            Assert.AreEqual("Quit", EventSystem.current.currentSelectedGameObject.name);
            yield return Tap(Key.DownArrow);
            Assert.AreEqual("Game Start", EventSystem.current.currentSelectedGameObject.name);
            yield return Tap(Key.DownArrow);
            Assert.AreEqual("Settings", EventSystem.current.currentSelectedGameObject.name);
            yield return Tap(Key.Z);
            Assert.IsTrue(MenuFlag("IsSettingsOpen"));
            Assert.AreEqual("Master Volume", EventSystem.current.currentSelectedGameObject.name);
            yield return Tap(Key.Escape);
            Assert.IsFalse(MenuFlag("IsSettingsOpen"));
            Assert.AreEqual("Settings", EventSystem.current.currentSelectedGameObject.name);
        }

        [UnityTest]
        public IEnumerator MouseCanOpenSettingsAndReturn()
        {
            yield return Click(GameObject.Find("Settings").GetComponent<RectTransform>());
            Assert.IsTrue(MenuFlag("IsSettingsOpen"));
            yield return Click(GameObject.Find("Back").GetComponent<RectTransform>());
            Assert.IsFalse(MenuFlag("IsSettingsOpen"));
        }

        [UnityTest]
        public IEnumerator VolumeAndDisplaySettingsPersist()
        {
            GameObject.Find("Settings").GetComponent<Button>().onClick.Invoke();
            Slider slider = GameObject.Find("Master Volume").GetComponent<Slider>();
            slider.value = 0.32f;
            Assert.That(AudioListener.volume, Is.EqualTo(0.32f).Within(0.001f));
            Assert.AreEqual("32%", GameObject.Find("Volume Percentage").GetComponent<Text>().text);
            Toggle toggle = GameObject.Find("Fullscreen").GetComponent<Toggle>();
            toggle.isOn = true;
            string oldResolution = GameObject.Find("Resolution").GetComponentInChildren<Text>().text;
            GameObject.Find("Resolution").GetComponent<Button>().onClick.Invoke();
            Assert.AreNotEqual(oldResolution, GameObject.Find("Resolution").GetComponentInChildren<Text>().text);
            GameObject.Find("Apply").GetComponent<Button>().onClick.Invoke();
            Assert.That(PlayerPrefs.GetFloat(VolumePreference), Is.EqualTo(0.32f).Within(0.001f));
            Assert.AreEqual(1, PlayerPrefs.GetInt(FullscreenPreference));
            Assert.Greater(PlayerPrefs.GetInt(WidthPreference), 0);
            GameObject.Find("Back").GetComponent<Button>().onClick.Invoke();
            GameObject.Find("Settings").GetComponent<Button>().onClick.Invoke();
            Assert.That(GameObject.Find("Master Volume").GetComponent<Slider>().value, Is.EqualTo(0.32f).Within(0.001f));
            Assert.IsTrue(GameObject.Find("Fullscreen").GetComponent<Toggle>().isOn);
            yield return null;
        }

        [UnityTest]
        public IEnumerator StartFadesAndLoadsTargetEvenWithPausedTime()
        {
            SetField(menu, "gameSceneName", "MenuValidationTarget");
            Time.timeScale = 0f;
            GameObject.Find("Game Start").GetComponent<Button>().onClick.Invoke();
            Assert.IsTrue(MenuFlag("IsLoading"));
            Assert.IsFalse(GameObject.Find("Menu Buttons").GetComponent<CanvasGroup>().interactable);
            InvokeMenu("StartGame"); // Repeat submissions must not create a second load.
            float deadline = Time.realtimeSinceStartup + 5f;
            while (SceneManager.GetActiveScene().name != "MenuValidationTarget" && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.AreEqual("MenuValidationTarget", SceneManager.GetActiveScene().name);
        }

        [UnityTest]
        public IEnumerator MissingTargetKeepsMenuUsable()
        {
            SetField(menu, "gameSceneName", "ThisSceneDoesNotExist");
            LogAssert.Expect(LogType.Warning, new Regex("Ember main menu cannot load"));
            GameObject.Find("Game Start").GetComponent<Button>().onClick.Invoke();
            Assert.IsFalse(MenuFlag("IsLoading"));
            Assert.IsTrue(GameObject.Find("Menu Buttons").GetComponent<CanvasGroup>().interactable);
            Assert.That(GameObject.Find("Menu Status").GetComponent<Text>().text, Does.Contain("Build Profiles"));
            yield return null;
        }

        [UnityTest]
        public IEnumerator ReenablingDoesNotDuplicateButtonListeners()
        {
            menu.gameObject.SetActive(false);
            menu.gameObject.SetActive(true);
            PlayerPrefs.SetInt(WidthPreference, 1280);
            PlayerPrefs.SetInt(HeightPreference, 720);
            InvokeMenu("OpenSettings");
            Text resolution = GameObject.Find("Resolution").GetComponentInChildren<Text>();
            GameObject.Find("Resolution").GetComponent<Button>().onClick.Invoke();
            Assert.AreEqual("1600 × 900   ›", resolution.text);
            yield return null;
        }

        [TestCase(1920f, 1080f, 1f, 1f)]
        [TestCase(1280f, 960f, 0.75f, 1f)]
        [TestCase(2560f, 1080f, 1f, 0.75f)]
        public void BackgroundCoverDoesNotStretchOrLeaveBars(float width, float height, float uWidth, float uHeight)
        {
            System.Type coverType = System.AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType("EmberPrototype.MainMenuBackgroundCover"))
                .First(type => type != null);
            Rect crop = (Rect)coverType.GetMethod("CalculateUvRect").Invoke(null,
                new object[] { new Vector2(width, height), new Vector2(1920f, 1080f) });
            Assert.That(crop.width, Is.EqualTo(uWidth).Within(0.001f));
            Assert.That(crop.height, Is.EqualTo(uHeight).Within(0.001f));
            Assert.That(crop.center.x, Is.EqualTo(0.5f).Within(0.001f));
            Assert.That(crop.center.y, Is.EqualTo(0.5f).Within(0.001f));
        }

        private IEnumerator Tap(Key key)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
            yield return null;
            yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;
            yield return null;
        }

        private IEnumerator Click(RectTransform target)
        {
            Vector2 point = RectTransformUtility.WorldToScreenPoint(null, target.TransformPoint(target.rect.center));
            InputSystem.QueueStateEvent(mouse, new MouseState { position = point });
            yield return null;
            InputSystem.QueueStateEvent(mouse, new MouseState { position = point }.WithButton(MouseButton.Left));
            yield return null;
            InputSystem.QueueStateEvent(mouse, new MouseState { position = point });
            yield return null;
            yield return null;
        }

        private static void SetField(object target, string field, object value)
            => target.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value);

        private bool MenuFlag(string property) => (bool)menu.GetType().GetProperty(property).GetValue(menu);
        private void InvokeMenu(string method) => menu.GetType().GetMethod(method).Invoke(menu, null);
    }
}
