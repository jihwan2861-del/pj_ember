using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EmberPrototype.Editor
{
    /// <summary>Creates a new editable scene, without replacing an existing scene or touching open rooms.</summary>
    public static class EmberMainMenuSceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/EmberMainMenu.unity";
        public const string GameScenePath = "Assets/Scenes/game 1.unity";
        public const string ArtPath = "Assets/image/ProjectEmber_TitleBackground.png";
        public const string UiPath = "Assets/EmberPrototype/UI/MainMenu";
        private static readonly Color Cream = new Color(0.88f, 0.9f, 0.88f);
        private static readonly Color Gold = new Color(1f, 0.81f, 0.47f);
        private static Font font;
        private static Sprite divider;
        private static Sprite frame;

        [MenuItem("Ember Prototype/UI/Create Main Menu Scene")]
        public static void Build()
        {
            if (File.Exists(ScenePath))
            {
                Debug.Log("Main menu already exists. Edit the saved scene; it has not been regenerated: " + ScenePath);
                return;
            }
            AssetDatabase.Refresh();
            ImportTexture(ArtPath, false, Vector4.zero);
            ImportTexture(UiPath + "/KenneyDivider.png", true, Vector4.zero);
            ImportTexture(UiPath + "/KenneyFrame.png", true, Vector4.one * 24f);
            font = AssetDatabase.LoadAssetAtPath<Font>(UiPath + "/Fonts/NotoSerifCJKkr-Regular.otf");
            divider = AssetDatabase.LoadAssetAtPath<Sprite>(UiPath + "/KenneyDivider.png");
            frame = AssetDatabase.LoadAssetAtPath<Sprite>(UiPath + "/KenneyFrame.png");
            Texture2D background = AssetDatabase.LoadAssetAtPath<Texture2D>(ArtPath);
            if (font == null || divider == null || frame == null || background == null)
                throw new InvalidOperationException("Main menu art, Kenney decorations or Korean font is missing.");

            Directory.CreateDirectory("Assets/Scenes");
            Scene previous = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            try
            {
                GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
                cameraObject.tag = "MainCamera";
                Camera camera = cameraObject.GetComponent<Camera>();
                camera.orthographic = true;
                camera.backgroundColor = new Color(0.015f, 0.03f, 0.045f);
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.cullingMask = 0;
                cameraObject.transform.position = new Vector3(0f, 0f, -10f);

                GameObject events = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                InputSystemUIInputModule module = events.GetComponent<InputSystemUIInputModule>();
                ConfigureInput(module);

                RectTransform root = CreateRect("Main Menu Canvas", null);
                Canvas canvas = root.gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler scaler = root.gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0.5f;
                root.gameObject.AddComponent<GraphicRaycaster>();

                RectTransform artRect = CreateRect("Background - Generated Pixel Ruins", root);
                Stretch(artRect);
                RawImage art = artRect.gameObject.AddComponent<RawImage>();
                art.texture = background;
                art.raycastTarget = false;
                artRect.gameObject.AddComponent<MainMenuBackgroundCover>();
                CreateEmbers(root);

                RectTransform content = CreateRect("Title and Main Menu", root);
                content.anchorMin = content.anchorMax = new Vector2(0f, 0.5f);
                content.pivot = new Vector2(0f, 0.5f);
                content.anchoredPosition = new Vector2(130f, 0f);
                content.sizeDelta = new Vector2(800f, 800f);
                Label("English Subtitle", content, "P R O J E C T   E M B E R", 23, Gold,
                    10f, 122f, 720f, 42f);
                Text title = Label("Title - 프로젝트 앰버", content, "프로젝트 앰버", 92, Cream,
                    0f, 173f, 790f, 142f);
                Shadow shadow = title.gameObject.AddComponent<Shadow>();
                shadow.effectColor = new Color(0.035f, 0.05f, 0.07f, 0.7f);
                shadow.effectDistance = new Vector2(2f, -3f);
                CreateDivider(content, 12f, 321f, 630f);

                RectTransform menu = CreateRect("Menu Buttons", content);
                TopLeft(menu, 0f, 405f, 620f, 300f);
                CanvasGroup menuGroup = menu.gameObject.AddComponent<CanvasGroup>();
                Button start = MenuButton("Game Start", menu, "게임 시작", 0f, 0f, 550f, 76f, true);
                Button settings = MenuButton("Settings", menu, "설정", 0f, 91f, 550f, 76f, false);
                Button quit = MenuButton("Quit", menu, "나가기", 0f, 182f, 550f, 76f, false);
                Navigate(start, quit, settings);
                Navigate(settings, start, quit);
                Navigate(quit, settings, start);
                Text status = Label("Menu Status", content, "", 20, Gold, 92f, 705f, 660f, 72f);

                Text help = Label("Controls", root, "↑↓ 이동     Enter / Z 선택     Esc 뒤로", 20,
                    new Color(0.57f, 0.65f, 0.68f), 140f, 0f, 760f, 38f);
                RectTransform helpRect = help.rectTransform;
                helpRect.anchorMin = helpRect.anchorMax = new Vector2(0f, 0f);
                helpRect.pivot = Vector2.zero;
                helpRect.anchoredPosition = new Vector2(140f, 58f);
                events.GetComponent<EventSystem>().firstSelectedGameObject = start.gameObject;

                EmberMainMenu controller = new GameObject("Main Menu Controller").AddComponent<EmberMainMenu>();
                SerializedObject controllerData = new SerializedObject(controller);
                Set(controllerData, "startButton", start);
                Set(controllerData, "settingsButton", settings);
                Set(controllerData, "quitButton", quit);
                Set(controllerData, "mainMenuGroup", menuGroup);
                Set(controllerData, "inputModule", module);
                Set(controllerData, "menuStatus", status);
                CreateSettings(root, controllerData);

                RectTransform fade = CreateRect("Scene Transition Fade", root);
                Stretch(fade);
                Image fadeImage = fade.gameObject.AddComponent<Image>();
                fadeImage.color = Color.black;
                CanvasGroup fadeGroup = fade.gameObject.AddComponent<CanvasGroup>();
                fadeGroup.alpha = 0f;
                fadeGroup.blocksRaycasts = false;
                fadeGroup.interactable = false;
                Set(controllerData, "fadeOverlay", fadeGroup);
                controllerData.ApplyModifiedPropertiesWithoutUndo();

                if (!EditorSceneManager.SaveScene(scene, ScenePath))
                    throw new IOException("Could not save main menu scene.");
                RegisterBuildScenes();
                AssetDatabase.SaveAssets();
                Debug.Log("Created editable Ember main menu: " + ScenePath);
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
            }
        }

        [MenuItem("Ember Prototype/UI/Register Menu and Game in Build Profiles")]
        public static void RegisterBuildScenes()
        {
            if (!File.Exists(ScenePath) || !File.Exists(GameScenePath))
                throw new FileNotFoundException("Save the main menu and configured gameplay scene first.");
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
            foreach (EditorBuildSettingsScene existing in EditorBuildSettings.scenes)
            {
                if (existing.path == ScenePath) continue;
                scenes.Add(existing.path == GameScenePath
                    ? new EditorBuildSettingsScene(GameScenePath, true) : existing);
            }
            if (!scenes.Any(entry => entry.path == GameScenePath))
                scenes.Add(new EditorBuildSettingsScene(GameScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void CreateSettings(RectTransform root, SerializedObject controller)
        {
            RectTransform scrim = CreateRect("Settings Overlay", root);
            Stretch(scrim);
            Image scrimImage = scrim.gameObject.AddComponent<Image>();
            scrimImage.color = new Color(0f, 0.008f, 0.015f, 0.78f);
            RectTransform panel = CreateRect("Settings Panel - Kenney Frame", scrim);
            panel.sizeDelta = new Vector2(840f, 660f);
            Image plate = panel.gameObject.AddComponent<Image>();
            plate.color = new Color(0.025f, 0.045f, 0.06f, 1f);
            plate.raycastTarget = false;
            RectTransform borderRect = CreateRect("Kenney Border", panel);
            Stretch(borderRect);
            Image border = borderRect.gameObject.AddComponent<Image>();
            border.sprite = frame;
            border.type = Image.Type.Sliced;
            border.fillCenter = false;
            border.pixelsPerUnitMultiplier = 3f;
            border.color = new Color(0.74f, 0.62f, 0.39f, 0.65f);
            border.raycastTarget = false;

            Label("Heading", panel, "설정", 48, Cream, 72f, 53f, 696f, 90f, TextAnchor.MiddleCenter);
            CreateDivider(panel, 154f, 151f, 532f);
            Label("Volume Label", panel, "전체 음량", 28, Cream, 72f, 210f, 230f, 52f);
            Text volumeValue = Label("Volume Percentage", panel, "100%", 25, Gold,
                685f, 210f, 90f, 52f, TextAnchor.MiddleRight);
            Slider volume = CreateSlider(panel, 354f, 224f, 290f, 28f);

            Label("Display Label", panel, "화면 모드", 28, Cream, 72f, 302f, 230f, 52f);
            RectTransform toggleRect = CreateRect("Fullscreen", panel);
            TopLeft(toggleRect, 354f, 302f, 414f, 52f);
            Toggle fullscreen = toggleRect.gameObject.AddComponent<Toggle>();
            RectTransform boxRect = CreateRect("Checkbox", toggleRect);
            TopLeft(boxRect, 0f, 10f, 32f, 32f);
            Image box = boxRect.gameObject.AddComponent<Image>();
            box.sprite = frame;
            box.type = Image.Type.Sliced;
            box.fillCenter = false;
            box.pixelsPerUnitMultiplier = 3f;
            box.color = Gold;
            RectTransform checkRect = CreateRect("Checked", boxRect);
            checkRect.sizeDelta = new Vector2(12f, 12f);
            checkRect.localEulerAngles = new Vector3(0f, 0f, 45f);
            Image check = checkRect.gameObject.AddComponent<Image>();
            check.color = Gold;
            check.raycastTarget = false;
            fullscreen.targetGraphic = box;
            fullscreen.graphic = check;
            ColorBlock toggleColors = ColorBlock.defaultColorBlock;
            toggleColors.normalColor = Cream;
            toggleColors.highlightedColor = Gold;
            toggleColors.selectedColor = Gold;
            fullscreen.colors = toggleColors;
            Text toggleLabel = Label("Label", toggleRect, "전체화면", 28, Cream, 58f, 0f, 290f, 52f);
            toggleLabel.raycastTarget = true;

            Label("Resolution Label", panel, "해상도", 28, Cream, 72f, 390f, 230f, 52f);
            Button resolution = FramedButton("Resolution", panel, "1920 × 1080   ›", 354f, 385f, 414f, 62f, 25);
            Text resolutionValue = resolution.GetComponentInChildren<Text>();
            Text status = Label("Settings Status", panel, "", 20, Gold, 72f, 469f, 696f, 44f, TextAnchor.MiddleCenter);
            Button apply = FramedButton("Apply", panel, "적용", 188f, 543f, 204f, 66f, 28);
            Button back = FramedButton("Back", panel, "돌아가기", 448f, 543f, 204f, 66f, 28);

            Navigate(volume, back, fullscreen);
            Navigate(fullscreen, volume, resolution);
            Navigate(resolution, fullscreen, apply);
            Navigate(apply, resolution, volume, back, back);
            Navigate(back, resolution, volume, apply, apply);
            Set(controller, "settingsPanel", scrim.gameObject);
            Set(controller, "volumeSlider", volume);
            Set(controller, "volumeValue", volumeValue);
            Set(controller, "fullscreenToggle", fullscreen);
            Set(controller, "resolutionButton", resolution);
            Set(controller, "resolutionValue", resolutionValue);
            Set(controller, "applyButton", apply);
            Set(controller, "backButton", back);
            Set(controller, "settingsStatus", status);
            scrim.gameObject.SetActive(false);
        }

        private static Slider CreateSlider(Transform parent, float x, float y, float width, float height)
        {
            RectTransform rect = CreateRect("Master Volume", parent);
            TopLeft(rect, x, y, width, height);
            Slider slider = rect.gameObject.AddComponent<Slider>();
            RectTransform track = CreateRect("Track", rect);
            track.anchorMin = new Vector2(0f, 0.5f);
            track.anchorMax = new Vector2(1f, 0.5f);
            track.sizeDelta = new Vector2(0f, 4f);
            Image trackImage = track.gameObject.AddComponent<Image>();
            trackImage.color = new Color(0.2f, 0.27f, 0.3f);
            RectTransform fillArea = CreateRect("Fill Area", rect);
            fillArea.anchorMin = new Vector2(0f, 0.5f);
            fillArea.anchorMax = new Vector2(1f, 0.5f);
            fillArea.sizeDelta = new Vector2(-14f, 4f);
            RectTransform fill = CreateRect("Fill", fillArea);
            Stretch(fill);
            Image fillImage = fill.gameObject.AddComponent<Image>();
            fillImage.color = Gold;
            fillImage.raycastTarget = false;
            RectTransform handleArea = CreateRect("Handle Area", rect);
            Stretch(handleArea);
            handleArea.offsetMin = new Vector2(7f, 0f);
            handleArea.offsetMax = new Vector2(-7f, 0f);
            RectTransform handle = CreateRect("Handle", handleArea);
            handle.sizeDelta = new Vector2(14f, 0f);
            Image handleImage = handle.gameObject.AddComponent<Image>();
            handleImage.color = Gold;
            slider.fillRect = fill;
            slider.handleRect = handle;
            slider.targetGraphic = handleImage;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;
            return slider;
        }

        private static Button MenuButton(string name, Transform parent, string text,
            float x, float y, float width, float height, bool initiallySelected)
        {
            RectTransform rect = CreateRect(name, parent);
            TopLeft(rect, x, y, width, height);
            Image hitArea = rect.gameObject.AddComponent<Image>();
            hitArea.color = Color.clear;
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = hitArea;
            button.transition = Selectable.Transition.None;
            Text label = Label("Label", rect, text, 36, initiallySelected ? Gold : Cream,
                92f, 0f, width - 92f, height);
            RectTransform ornamentRect = CreateRect("Kenney Selection Ornament", rect);
            TopLeft(ornamentRect, 0f, 30f, 68f, 16f);
            Image ornamentImage = ornamentRect.gameObject.AddComponent<Image>();
            ornamentImage.sprite = divider;
            ornamentImage.color = Gold;
            ornamentImage.raycastTarget = false;
            CanvasGroup ornament = ornamentRect.gameObject.AddComponent<CanvasGroup>();
            ornament.alpha = initiallySelected ? 1f : 0f;
            ConfigureFeedback(button, label, ornament);
            return button;
        }

        private static Button FramedButton(string name, Transform parent, string text,
            float x, float y, float width, float height, int fontSize)
        {
            RectTransform rect = CreateRect(name, parent);
            TopLeft(rect, x, y, width, height);
            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = frame;
            image.type = Image.Type.Sliced;
            image.fillCenter = false;
            image.pixelsPerUnitMultiplier = 3f;
            image.color = new Color(0.71f, 0.62f, 0.45f, 0.5f);
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.None;
            Text label = Label("Label", rect, text, fontSize, Cream,
                8f, 0f, width - 16f, height, TextAnchor.MiddleCenter);
            ConfigureFeedback(button, label, null);
            return button;
        }

        private static void ConfigureFeedback(Button button, Text label, CanvasGroup ornament)
        {
            SerializedObject settings = new SerializedObject(button.gameObject.AddComponent<MainMenuButtonFeedback>());
            Set(settings, "label", label);
            Set(settings, "ornament", ornament);
            settings.FindProperty("normalColor").colorValue = Cream;
            settings.FindProperty("selectedColor").colorValue = Gold;
            settings.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateDivider(Transform parent, float x, float y, float width)
        {
            RectTransform left = CreateRect("Kenney Divider Left", parent);
            TopLeft(left, x, y, width * 0.5f - 12f, 24f);
            Image leftImage = left.gameObject.AddComponent<Image>();
            leftImage.sprite = divider;
            leftImage.color = new Color(Gold.r, Gold.g, Gold.b, 0.5f);
            leftImage.raycastTarget = false;
            RectTransform right = CreateRect("Kenney Divider Right", parent);
            TopLeft(right, x + width, y, width * 0.5f - 12f, 24f);
            right.localScale = new Vector3(-1f, 1f, 1f);
            Image rightImage = right.gameObject.AddComponent<Image>();
            rightImage.sprite = divider;
            rightImage.color = leftImage.color;
            rightImage.raycastTarget = false;
            RectTransform center = CreateRect("Ember Diamond", parent);
            TopLeft(center, x + width * 0.5f - 4f, y + 8f, 8f, 8f);
            center.pivot = new Vector2(0.5f, 0.5f);
            center.localEulerAngles = new Vector3(0f, 0f, 45f);
            Image point = center.gameObject.AddComponent<Image>();
            point.color = Gold;
            point.raycastTarget = false;
        }

        private static void CreateEmbers(RectTransform canvas)
        {
            RectTransform root = CreateRect("Ambient Embers", canvas);
            Stretch(root);
            RectTransform[] sparks = new RectTransform[12];
            for (int i = 0; i < sparks.Length; i++)
            {
                RectTransform spark = CreateRect("Ember " + (i + 1), root);
                spark.anchorMin = spark.anchorMax = new Vector2(0.915f, 0.32f);
                spark.anchoredPosition = new Vector2((i * 37 % 131) - 75f, (i * 23 % 89) - 18f);
                spark.sizeDelta = Vector2.one * (i % 3 == 0 ? 4f : 2f);
                Image image = spark.gameObject.AddComponent<Image>();
                image.color = new Color(1f, 0.6f + i % 3 * 0.1f, 0.23f, 0.65f);
                image.raycastTarget = false;
                sparks[i] = spark;
            }
            SerializedObject data = new SerializedObject(root.gameObject.AddComponent<MainMenuAmbientEmbers>());
            SerializedProperty references = data.FindProperty("embers");
            references.arraySize = sparks.Length;
            for (int i = 0; i < sparks.Length; i++) references.GetArrayElementAtIndex(i).objectReferenceValue = sparks[i];
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureInput(InputSystemUIInputModule module)
        {
            string path = UiPath + "/MainMenuInput.asset";
            InputActionAsset actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
            if (actions == null)
            {
                actions = new DefaultInputActions().asset;
                actions.name = "Ember Main Menu Input";
                actions.FindAction("UI/Submit", true).AddBinding("<Keyboard>/z");
                AssetDatabase.CreateAsset(actions, path);
            }
            module.actionsAsset = actions;
            module.point = ActionReference(actions, "UI/Point", path);
            module.leftClick = ActionReference(actions, "UI/Click", path);
            module.scrollWheel = ActionReference(actions, "UI/ScrollWheel", path);
            module.move = ActionReference(actions, "UI/Navigate", path);
            module.submit = ActionReference(actions, "UI/Submit", path);
            module.cancel = ActionReference(actions, "UI/Cancel", path);
            module.deselectOnBackgroundClick = false;
        }

        private static InputActionReference ActionReference(InputActionAsset asset, string name, string path)
        {
            InputAction action = asset.FindAction(name, true);
            InputActionReference existing = AssetDatabase.LoadAllAssetsAtPath(path).OfType<InputActionReference>()
                .FirstOrDefault(reference => reference.action != null && reference.action.id == action.id);
            if (existing != null) return existing;
            InputActionReference created = InputActionReference.Create(action);
            created.name = name.Replace('/', '_');
            AssetDatabase.AddObjectToAsset(created, asset);
            return created;
        }

        private static void ImportTexture(string path, bool sprite, Vector4 border)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new FileNotFoundException("Missing menu texture: " + path);
            importer.textureType = sprite ? TextureImporterType.Sprite : TextureImporterType.Default;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.spriteBorder = border;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = sprite;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
        }

        private static Text Label(string name, Transform parent, string value, int size, Color color,
            float x, float y, float width, float height, TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            RectTransform rect = CreateRect(name, parent);
            TopLeft(rect, x, y, width, height);
            Text text = rect.gameObject.AddComponent<Text>();
            text.font = font;
            text.text = value;
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            text.supportRichText = false;
            return text;
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            RectTransform rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            if (parent != null) rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            return rect;
        }

        private static void TopLeft(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        private static void Navigate(Selectable target, Selectable up, Selectable down,
            Selectable left = null, Selectable right = null)
        {
            target.navigation = new Navigation
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = up, selectOnDown = down,
                selectOnLeft = left, selectOnRight = right
            };
        }

        private static void Set(SerializedObject target, string property, UnityEngine.Object value)
        {
            SerializedProperty field = target.FindProperty(property);
            if (field == null) throw new MissingFieldException(property);
            field.objectReferenceValue = value;
        }
    }
}
