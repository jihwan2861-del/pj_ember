using System;
using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

namespace EmberMovementRegression
{
    public sealed class RopeScenePlayTests
    {
        private const string ScenePath = "Assets/EmberPrototype/RopeValidationVisual.unity";
        private Keyboard keyboard;
        private InputSettings.BackgroundBehavior originalBackground;
        private InputSettings.EditorInputBehaviorInPlayMode originalEditorInput;
        private float originalCaptureDeltaTime;
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static Type RuntimeType(string name) => Type.GetType("EmberPrototype." + name + ", Assembly-CSharp", true);
        private static string State(Component player) => player.GetType().GetField("state", Flags).GetValue(player).ToString();
        private static object Property(Component component, string name) => component.GetType().GetProperty(name, Flags).GetValue(component);

        private void Keys(params Key[] keys) => InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));

        [UnityTest]
        public IEnumerator RopeSceneRunsTorchToTwoRopesToLandingWithRealUpdates()
        {
            // The batch test runner starts with an untitled empty scene. Save that QA-only scene
            // so Unity permits the builder's non-destructive additive workflow.
            var initialScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (string.IsNullOrEmpty(initialScene.path)) EditorSceneManager.SaveScene(initialScene, "Assets/RopeQAEmpty.unity");
            Type builder = Type.GetType("EmberPrototype.Editor.RopeValidationSceneBuilder, Assembly-CSharp-Editor", true);
            builder.GetMethod("BuildAt").Invoke(null, new object[] { ScenePath });
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            yield return new EnterPlayMode();
            originalCaptureDeltaTime = Time.captureDeltaTime;
            Time.captureDeltaTime = 1f / 60f;
            originalBackground = InputSystem.settings.backgroundBehavior;
            originalEditorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            keyboard = InputSystem.AddDevice<Keyboard>();
            Component player = (Component)UnityEngine.Object.FindFirstObjectByType(RuntimeType("FlamePlayerController"));
            Rigidbody2D body = player.GetComponent<Rigidbody2D>();
            Component first = GameObject.Find("Rope 1").GetComponent(RuntimeType("BurningRope"));
            Component second = GameObject.Find("Rope 2").GetComponent(RuntimeType("BurningRope"));
            yield return null;
            Keys(Key.RightArrow, Key.X);
            yield return null;
            yield return null;
            TestContext.WriteLine("First X: state=" + State(player) + ", held=" + keyboard.xKey.isPressed + ", current=" + (Keyboard.current == keyboard) + ", mode=" + InputSystem.settings.updateMode);
            float deadline = Time.realtimeSinceStartup + 5f;
            while (State(player) == "Travelling" && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(State(player), Is.EqualTo("Anchored"));
            Keys(Key.RightArrow, Key.Z);
            yield return null;
            yield return null;
            Keys(Key.RightArrow);
            yield return null;
            yield return new WaitForFixedUpdate();
            Keys(Key.RightArrow, Key.X);
            yield return null;
            yield return null;
            deadline = Time.realtimeSinceStartup + 5f;
            while (State(player) == "Travelling" && Time.realtimeSinceStartup < deadline) yield return null;
            TestContext.WriteLine("Rope 1 entry: " + State(player) + ", position=" + body.position + ", end=" + Property(first, "EndPosition") + ", fixed=" + Time.fixedTime + ", constraints=" + body.constraints);
            Assert.That(State(player), Is.EqualTo("RopeAnchored"));
            Assert.That(Vector2.Distance(body.position, (Vector2)Property(first, "EndPosition")), Is.LessThan(0.01f));
            Keys(Key.RightArrow);
            deadline = Time.realtimeSinceStartup + 5f;
            while ((float)Property(first, "Angle") < 0.25f && Time.realtimeSinceStartup < deadline) yield return null;
            Capture("01-swing");
            Keys(Key.Z);
            yield return null;
            yield return null;
            Assert.That(State(player), Is.EqualTo("Free"));
            Assert.That(body.linearVelocity.x, Is.GreaterThan(3.3f));
            Vector2 before = (Vector2)Property(first, "EndPosition");
            Keys(Key.RightArrow);
            yield return null;
            yield return new WaitForFixedUpdate();
            Keys(Key.RightArrow, Key.X);
            yield return null;
            yield return null;
            deadline = Time.realtimeSinceStartup + 5f;
            while (State(player) == "Travelling" && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(State(player), Is.EqualTo("RopeAnchored"));
            Assert.That(Vector2.Distance(body.position, (Vector2)Property(second, "EndPosition")), Is.LessThan(0.01f));
            Assert.That(Vector2.Distance(before, (Vector2)Property(first, "EndPosition")), Is.GreaterThan(0.01f), "Departed rope must continue swinging.");
            // Kick early on the second rope for a shallow, reproducible landing arc.
            Keys(Key.Z);
            yield return null;
            yield return null;
            TestContext.WriteLine("Second kick: position=" + body.position + ", velocity=" + body.linearVelocity);
            Capture("02-chain");
            Keys();
            deadline = Time.realtimeSinceStartup + 6f;
            while (!(bool)player.GetType().GetField("wasGrounded", Flags).GetValue(player) && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.That((bool)player.GetType().GetField("wasGrounded", Flags).GetValue(player), Is.True);
            TestContext.WriteLine("Landing: " + body.position);
            Assert.That(body.position.x, Is.GreaterThan(6f));
            Capture("03-landing");
            Keys(Key.R);
            yield return null;
            yield return null;
            Assert.That(body.position.x, Is.LessThan(-6f));
            Assert.That(State(player), Is.EqualTo("Free"));
            TestContext.WriteLine("Real Play Mode: torch -> rope 1 -> rope 2 -> landing -> R restart; synthetic keyboard, normal Update/FixedUpdate and native physics.");
            InputSystem.settings.backgroundBehavior = originalBackground;
            InputSystem.settings.editorInputBehaviorInPlayMode = originalEditorInput;
            Time.captureDeltaTime = originalCaptureDeltaTime;
            yield return new ExitPlayMode();
        }

        private static void Capture(string name)
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) return;
            string folder = Path.GetFullPath(Path.Combine(Application.dataPath, "../RopeEvidence"));
            Directory.CreateDirectory(folder);
            Camera camera = Camera.main;
            RenderTexture render = new RenderTexture(1280, 720, 24);
            RenderTexture previous = RenderTexture.active;
            RenderTexture oldTarget = camera.targetTexture;
            Texture2D texture = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = render;
                if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null)
                    UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(camera,
                        new UnityEngine.Rendering.RenderPipeline.StandardRequest { destination = render });
                else camera.Render();
                RenderTexture.active = render;
                texture.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
                texture.Apply();
                File.WriteAllBytes(Path.Combine(folder, name + ".png"), texture.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = oldTarget;
                RenderTexture.active = previous;
                render.Release();
                UnityEngine.Object.Destroy(texture);
                UnityEngine.Object.Destroy(render);
            }
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (keyboard != null) InputSystem.RemoveDevice(keyboard);
            if (Application.isPlaying)
            {
                InputSystem.settings.backgroundBehavior = originalBackground;
                InputSystem.settings.editorInputBehaviorInPlayMode = originalEditorInput;
                Time.captureDeltaTime = originalCaptureDeltaTime;
                yield return new ExitPlayMode();
            }
        }
    }
}
