using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;

namespace WaterSpiritAIValidation
{
    public sealed class WaterSpiritAITests
    {
        private static Type T(string name) => Type.GetType("EmberPrototype." + name + ", Assembly-CSharp", true);
        private static Component Spirit => GameObject.Find("AI spirit").GetComponent(T("WaterSpirit"));
        private static Component AI => Spirit.GetComponent(T("WaterSpiritAI"));
        private static Component Mode => Spirit.GetComponent(T("SideChaseBehavior"));
        private static Camera Camera => GameObject.Find("QA camera").GetComponent<Camera>();
        private static GameObject Player => GameObject.Find("QA target");
        private static object P(object component, string property) => component.GetType().GetProperty(property).GetValue(component);
        private static object Invoke(object component, string method, params object[] args) => component.GetType().GetMethod(method).Invoke(component, args);
        private static string Phase => P(AI, "CurrentPhase").ToString();
        private static Component Wave => (Component)P(Spirit, "ActiveWave");

        private static void Set(Component component, string field, object value)
        {
            SerializedObject data = new SerializedObject(component);
            SerializedProperty p = data.FindProperty(field);
            if (value is bool b) p.boolValue = b;
            else if (value is float f) p.floatValue = f;
            else p.objectReferenceValue = (UnityEngine.Object)value;
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        private static IEnumerator WaitTime(float seconds)
        {
            float end = Time.time + seconds;
            float timeout = Time.realtimeSinceStartup + 8f;
            while (Time.time < end && Time.realtimeSinceStartup < timeout) yield return null;
            Assert.That(Time.time, Is.GreaterThanOrEqualTo(end));
        }

        private static IEnumerator WaitPhase(string phase)
        {
            float end = Time.realtimeSinceStartup + 8f;
            while (Phase != phase && Time.realtimeSinceStartup < end) yield return null;
            Assert.That(Phase, Is.EqualTo(phase));
        }

        private static Component NewTestMode() => Spirit.gameObject.AddComponent(Type.GetType("WaterSpiritTestBehavior, Assembly-CSharp", true));

        private static void Build(bool missingCamera = false, bool scaled = false, bool defaults = false)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Type builder = Type.GetType("EmberPrototype.Editor.WaterSpiritAISetup, Assembly-CSharp-Editor", true);
            builder.GetMethod("AttachToPrefab").Invoke(null, null);
            GameObject spirit = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/EmberPrototype/Prefabs/WaterSpirit.prefab"));
            spirit.name = "AI spirit"; spirit.transform.position = new Vector3(-7.8f, 0f, 0f);
            GameObject player = new GameObject("QA target"); player.transform.position = new Vector3(0f, 1f, 0f);
            player.AddComponent<BoxCollider2D>().size = new Vector2(0.6f, 0.8f);
            SpriteRenderer targetArt = player.AddComponent<SpriteRenderer>();
            targetArt.sprite = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprite-0001.png").OfType<Sprite>().First();
            targetArt.color = new Color(1f, 0.5f, 0.15f);
            targetArt.sortingOrder = 10;
            targetArt.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat");
            Camera camera = new GameObject("QA camera").AddComponent<Camera>();
            camera.transform.position = new Vector3(0f, 0f, -10f); camera.orthographic = true;
            camera.orthographicSize = 5f; camera.aspect = 16f / 9f;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(0.03f, 0.04f, 0.08f);
            Component ai = spirit.GetComponent(T("WaterSpiritAI"));
            Component mode = spirit.GetComponent(T("SideChaseBehavior"));
            Set(ai, "playerBody", player.GetComponent<BoxCollider2D>());
            Set(ai, "gameCamera", missingCamera ? null : camera);
            Set(spirit.GetComponent(T("WaterSpirit")), "attackInterval", 0.02f);
            if (!defaults)
            {
                Set(mode, "trackingTime", 0.15f); Set(mode, "warningTime", 0.2f);
                Set(mode, "recoveryTime", 0.1f); Set(mode, "moveSpeed", 25f);
            }
            // Use a private template in the test scene, preserving the authored wave prefab.
            GameObject template = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/EmberPrototype/Prefabs/WaterWave.prefab"));
            template.name = "QA wave template"; template.SetActive(false);
            Component wave = template.GetComponent(T("WaterWave"));
            Set(wave, "travelDistance", 8f); Set(wave, "speed", 16f); Set(wave, "deadly", false);
            if (scaled) template.transform.localScale = new Vector3(2f, 1.5f, 1f);
            Set(spirit.GetComponent(T("WaterSpirit")), "wavePrefab", wave);
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), "Assets/WaterSpiritAIQA.unity");
        }

        private static void Capture(string name)
        {
            string folder = Environment.GetEnvironmentVariable("EMBER_SPIRIT_AI_ARTIFACTS");
            if (string.IsNullOrEmpty(folder)) return;
            Directory.CreateDirectory(folder);
            RenderTexture target = new RenderTexture(960, 540, 24);
            RenderTexture previous = RenderTexture.active;
            Camera.targetTexture = target;
            Camera.Render(); RenderTexture.active = target;
            Texture2D image = new Texture2D(960, 540, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, 960, 540), 0, 0); image.Apply();
            File.WriteAllBytes(Path.Combine(folder, name + ".png"), image.EncodeToPNG());
            Camera.targetTexture = null; RenderTexture.active = previous;
            UnityEngine.Object.Destroy(image); target.Release(); UnityEngine.Object.Destroy(target);
        }

        [UnityTest]
        public IEnumerator WaveFiresWithoutWarningAndKeepsFollowing()
        {
            Build(); yield return new EnterPlayMode(); yield return WaitPhase("Firing");
            Component warning = Spirit.GetComponentInChildren(T("WaterSpiritWarning"));
            Assert.That((bool)P(warning, "IsVisible"), Is.False);
            float before = Spirit.transform.position.y;
            Player.transform.position += Vector3.up * 2f;
            yield return WaitTime(0.1f);
            Assert.That(Spirit.transform.position.y, Is.GreaterThan(before + 0.1f));
            yield return WaitPhase("Tracking");
            Assert.That(Phase, Is.Not.EqualTo("Recovery"));
            Capture("01-continuous-chase");
            yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator RushWarnsLocksDirectionAndImmediatelyResumes()
        {
            Build(); yield return new EnterPlayMode(); yield return WaitPhase("Warning");
            Component warning = Spirit.GetComponentInChildren(T("WaterSpiritWarning"));
            Assert.That((bool)P(warning, "IsVisible"), Is.True);
            object plan = P(AI, "PlannedAttack");
            Assert.That((Bounds)P(warning, "DisplayedBounds"), Is.EqualTo((Bounds)P(plan, "DangerBounds")));
            Capture("02-rush-warning");
            Player.transform.position = new Vector3(-5f, 3f, 0f);
            yield return WaitPhase("Rushing");
            Assert.That(P(AI, "PlannedAttack"), Is.EqualTo(plan));
            Assert.That((bool)P(warning, "IsVisible"), Is.False);
            Assert.That((bool)P(Spirit, "IsRushing"), Is.True);
            yield return WaitTime(0.05f);
            Assert.That(Spirit.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Move"), Is.True);
            Assert.That(Spirit.transform.Find("Surfing Wave"), Is.Null);
            Assert.That(UnityEngine.Object.FindObjectsByType(T("WaterWave"), FindObjectsSortMode.None).Length, Is.EqualTo(1));
            Transform glow = Spirit.transform.Find("Spirit Light");
            Assert.That(glow, Is.Not.Null);
            Component light = glow.GetComponent(Type.GetType("UnityEngine.Rendering.Universal.Light2D, Unity.RenderPipelines.Universal.2D.Runtime", true));
            Color glowColor = (Color)P(light, "color");
            Assert.That(glowColor.b, Is.GreaterThan(glowColor.r));
            Assert.That((float)P(light, "intensity"), Is.EqualTo(0.8f).Within(0.001f));
            Assert.That((float)P(light, "pointLightOuterRadius"), Is.EqualTo(2.5f).Within(0.001f));
            Assert.That(glow.localPosition, Is.EqualTo(Vector3.zero));
            GameObject trails = GameObject.Find("AI spirit Afterimages");
            Assert.That(trails, Is.Not.Null);
            SpriteRenderer[] snapshots = trails.GetComponentsInChildren<SpriteRenderer>();
            Assert.That(snapshots.Any(renderer => renderer.enabled), Is.True);
            foreach (SpriteRenderer snapshot in snapshots.Where(renderer => renderer.enabled))
            {
                Assert.That(snapshot.color.b, Is.GreaterThan(snapshot.color.r));
                Assert.That(snapshot.color.b, Is.GreaterThan(snapshot.color.g));
                Assert.That(snapshot.sprite, Is.Not.Null);
                Assert.That(snapshot.sortingOrder, Is.LessThan(Spirit.GetComponent<SpriteRenderer>().sortingOrder));
            }
            Capture("03-blue-rush-afterimages");
            yield return WaitPhase("Tracking");
            Assert.That((bool)P(Spirit, "IsRushing"), Is.False);
            Assert.That(Spirit.transform.Find("Rush Hitbox").gameObject.activeSelf, Is.False);
            yield return WaitTime(0.25f);
            Assert.That(snapshots.Any(renderer => renderer.enabled), Is.False);
            yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator RushStillExecutesWhenPlayerIsOutsideAndCameraCorrectsWarning()
        {
            Build(); yield return new EnterPlayMode(); yield return WaitPhase("Warning");
            object original = P(AI, "PlannedAttack");
            object direction = original.GetType().GetField("Direction").GetValue(original);
            Player.transform.position = new Vector3(40f, 40f, 0f);
            Camera.transform.position += Vector3.up * 8f;
            yield return null;
            Assert.That(Phase, Is.EqualTo("Warning"));
            Assert.That(((Bounds)P(P(AI, "PlannedAttack"), "DangerBounds")).Intersects(Player.GetComponent<BoxCollider2D>().bounds), Is.False);
            yield return WaitPhase("Rushing");
            Assert.That((bool)P(Spirit, "IsRushing"), Is.True);
            object committed = P(AI, "PlannedAttack");
            Assert.That(committed.GetType().GetField("Direction").GetValue(committed), Is.EqualTo(direction));
            yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator EveryWaveReleaseRequestsSmallShakeAndNoShakeBeforeRelease()
        {
            Build(defaults: true); yield return new EnterPlayMode();
            Component shaker = Camera.gameObject.AddComponent(T("CelesteRoomCamera"));
            ((Behaviour)shaker).enabled = false;
            var remaining = shaker.GetType().GetField("shakeRemaining", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var magnitude = shaker.GetType().GetField("shakeMagnitude", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            for (int shot = 0; shot < 2; shot++)
            {
                remaining.SetValue(shaker, 0f); magnitude.SetValue(shaker, 0f);
                yield return WaitPhase("Firing");
                Assert.That((float)remaining.GetValue(shaker), Is.Zero);
                float timeout = Time.realtimeSinceStartup + 4f;
                while (!(bool)P(Spirit, "HasReleasedWave") && Time.realtimeSinceStartup < timeout) yield return null;
                Assert.That((bool)P(Spirit, "HasReleasedWave"), Is.True);
                Assert.That((float)remaining.GetValue(shaker), Is.EqualTo(0.1f).Within(0.001f));
                Assert.That((float)magnitude.GetValue(shaker), Is.EqualTo(0.06f).Within(0.001f));
                Invoke(Spirit, "ResetForRespawn", false);
            }
            yield return new ExitPlayMode();
        }
        [UnityTest]
        public IEnumerator SmallVerticalCameraCorrectionMustNotCancelHorizontalRush()
        {
            Build(); yield return new EnterPlayMode(); yield return WaitPhase("Rushing");
            Set(Mode, "trackingTime", 2f);
            Bounds sprite = Spirit.GetComponent<SpriteRenderer>().bounds;
            Camera.transform.position = new Vector3(Camera.transform.position.x,
                sprite.max.y - Camera.orthographicSize + 0.15f - 0.03f, Camera.transform.position.z);
            float before = Spirit.transform.position.x;
            yield return WaitTime(0.06f);
            Assert.That(Phase, Is.EqualTo("Rushing"), "A vertical camera correction cancelled the horizontal rush.");
            Assert.That(Mathf.Abs(Spirit.transform.position.x - before), Is.GreaterThan(0.3f));
            Assert.That(Camera.WorldToViewportPoint(Spirit.GetComponent<SpriteRenderer>().bounds.max).y, Is.LessThanOrEqualTo(1f));
            yield return new ExitPlayMode();
        }
        [UnityTest]
        public IEnumerator BlueAmbientParticlesStayInWorldAndClearOnRoomJump()
        {
            Build(defaults: true); yield return new EnterPlayMode();
            ((Behaviour)Spirit).enabled = false;
            foreach (SpriteRenderer renderer in UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None)) renderer.enabled = false;
            Component ambient = Camera.gameObject.AddComponent(T("WaterAmbientParticles"));
            ParticleSystem particles = Camera.GetComponentInChildren<ParticleSystem>();
            Assert.That(particles, Is.Not.Null);
            yield return WaitTime(0.4f);
            Assert.That(particles.particleCount, Is.GreaterThan(0));
            Assert.That(particles.main.maxParticles, Is.EqualTo(64));
            Assert.That(particles.main.simulationSpace, Is.EqualTo(ParticleSystemSimulationSpace.World));
            Assert.That(particles.GetComponent<ParticleSystemRenderer>().sharedMaterial.shader.isSupported, Is.True);
            Color blue = particles.main.startColor.color;
            Assert.That(blue.b, Is.GreaterThan(blue.r));
            Capture("05-blue-ambient-particles");
            Camera.transform.position += Vector3.right * 30f;
            ambient.GetType().GetMethod("FollowCamera", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(ambient, null);
            Assert.That(particles.particleCount, Is.Zero);
            Assert.That(particles.transform.position.x, Is.EqualTo(Camera.transform.position.x));
            ((Behaviour)ambient).enabled = false;
            Assert.That(particles.isPlaying, Is.False);
            yield return new ExitPlayMode();
        }
        [UnityTest]
        public IEnumerator CameraMovementKeepsWholeSpiritInsideViewportAndResetStopsRush()
        {
            Build(); yield return new EnterPlayMode(); yield return WaitPhase("Rushing");
            Set(Mode, "trackingTime", 2f);
            Camera.transform.position += new Vector3(12f, 10f, 0f);
            yield return null; yield return null;
            foreach (SpriteRenderer renderer in Spirit.GetComponentsInChildren<SpriteRenderer>())
            {
                if (!renderer.enabled) continue;
                Vector3 min = Camera.WorldToViewportPoint(renderer.bounds.min);
                Vector3 max = Camera.WorldToViewportPoint(renderer.bounds.max);
                Assert.That(min.x, Is.GreaterThanOrEqualTo(-0.001f));
                Assert.That(min.y, Is.GreaterThanOrEqualTo(-0.001f));
                Assert.That(max.x, Is.LessThanOrEqualTo(1.001f));
                Assert.That(max.y, Is.LessThanOrEqualTo(1.001f));
            }
            Assert.That(Phase, Is.EqualTo("Tracking"));
            Capture("04-camera-boundary");
            Invoke(Spirit, "ResetForRespawn", true);
            Assert.That(Phase, Is.EqualTo("Suspended"));
            Assert.That((bool)P(Spirit, "IsRushing"), Is.False);
            Invoke(Spirit, "ResumeAfterRespawn");
            yield return null;
            Assert.That(Phase, Is.EqualTo("Tracking"));
            yield return new ExitPlayMode();
        }
    }
}









