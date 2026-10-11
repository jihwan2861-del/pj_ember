using System;
using System.IO;
using System.Linq;
using EmberPrototype;
using EmberPrototype.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MainMenuValidationBatch
{
    public static void Generate()
    {
        try
        {
            string output = Environment.GetEnvironmentVariable("EMBER_MENU_ARTIFACTS");
            if (string.IsNullOrEmpty(output)) throw new InvalidOperationException("Missing artifact directory.");
            Directory.CreateDirectory(output);
            Scene initial = SceneManager.GetActiveScene();
            if (initial.IsValid() && string.IsNullOrEmpty(initial.path))
                EditorSceneManager.SaveScene(initial, "Assets/Scenes/QA-Scratch.unity");
            EmberMainMenuSceneBuilder.Build();
            // Keep the production build list before adding an isolated test-only scene.
            File.Copy("ProjectSettings/EditorBuildSettings.asset", Path.Combine(output, "MainMenu.BuildSettings.asset"), true);
            Scene scene = EditorSceneManager.OpenScene(EmberMainMenuSceneBuilder.ScenePath, OpenSceneMode.Single);
            EmberMainMenu menu = UnityEngine.Object.FindFirstObjectByType<EmberMainMenu>();
            if (menu == null) throw new InvalidOperationException("Missing menu controller.");
            foreach (Transform root in scene.GetRootGameObjects().Select(go => go.transform))
            foreach (Component component in root.GetComponentsInChildren<Component>(true))
                if (component == null) throw new InvalidOperationException("Missing script in generated scene.");
            foreach (Text text in UnityEngine.Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            foreach (char character in text.text)
                if (!char.IsWhiteSpace(character) && !text.font.HasCharacter(character))
                    throw new InvalidOperationException("Missing font glyph: " + character);
            Render(scene, 1920, 1080, Path.Combine(output, "MainMenu-1920x1080.png"));
            Render(scene, 1280, 960, Path.Combine(output, "MainMenu-1280x960.png"));
            menu.OpenSettings();
            Render(scene, 1920, 1080, Path.Combine(output, "MainMenu-Settings.png"));
            EditorSceneManager.CloseScene(scene, true);
            Scene target = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("Validation target reached");
            EditorSceneManager.SaveScene(target, "Assets/Scenes/MenuValidationTarget.unity");
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(EmberMainMenuSceneBuilder.ScenePath, true),
                new EditorBuildSettingsScene("Assets/Scenes/MenuValidationTarget.unity", true)
            };
            // Do not use the real product's persisted display preferences in an isolated test run.
            PlayerSettings.companyName = "EmberValidation";
            PlayerSettings.productName = "MainMenuQA";
            AssetDatabase.SaveAssets();
            Debug.Log("MAIN_MENU_GENERATION_PASS: scene, references, Korean font glyphs, three preview renders.");
            EditorApplication.Exit(0);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorApplication.Exit(1);
        }
    }

    private static void Render(Scene scene, int width, int height, string path)
    {
        Canvas canvas = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Canvas>()).First();
        Camera camera = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Camera>()).First();
        RenderTexture target = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
        RenderTexture previous = RenderTexture.active;
        Texture2D capture = null;
        try
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 10f;
            camera.cullingMask = ~0;
            camera.targetTexture = target;
            camera.aspect = (float)width / height;
            canvas.GetComponent<CanvasScaler>().SendMessage("Update", SendMessageOptions.DontRequireReceiver);
            Canvas.ForceUpdateCanvases();
            canvas.GetComponentInChildren<MainMenuBackgroundCover>().Refresh();
            Canvas.ForceUpdateCanvases();
            camera.Render();
            RenderTexture.active = target;
            capture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            capture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            capture.Apply();
            File.WriteAllBytes(path, capture.EncodeToPNG());
        }
        finally
        {
            RenderTexture.active = previous;
            camera.targetTexture = null;
            camera.cullingMask = 0;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            if (capture != null) UnityEngine.Object.DestroyImmediate(capture);
            RenderTexture.ReleaseTemporary(target);
        }
    }
}
