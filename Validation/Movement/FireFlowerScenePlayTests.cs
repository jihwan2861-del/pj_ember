using System;
using System.Collections;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

namespace EmberMovementRegression
{
    public sealed class FireFlowerScenePlayTests
    {
        private Keyboard keyboard;
        private InputSettings.BackgroundBehavior originalBackground;
        private InputSettings.EditorInputBehaviorInPlayMode originalEditorInput;
        private float originalCaptureDeltaTime;
        private static Type RuntimeType(string name) => Type.GetType("EmberPrototype." + name + ", Assembly-CSharp", true);
        [UnityTest]
        public IEnumerator FireFlowerCThenDodgePurifiesBarrierInRealPlayMode()
        {
            var initial = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (string.IsNullOrEmpty(initial.path)) EditorSceneManager.SaveScene(initial, "Assets/FlowerQAEmpty.unity");
            Type.GetType("EmberPrototype.Editor.FireFlowerValidationSceneBuilder, Assembly-CSharp-Editor", true)
                .GetMethod("BuildBatch").Invoke(null, null);
            yield return new EnterPlayMode();
            originalCaptureDeltaTime = Time.captureDeltaTime;
            Time.captureDeltaTime = 1f / 60f;
            originalBackground = InputSystem.settings.backgroundBehavior;
            originalEditorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            keyboard = InputSystem.AddDevice<Keyboard>();
            Component flower = GameObject.Find("Fire Flower").GetComponent(RuntimeType("FireFlower"));
            Component pollution = GameObject.Find("Flower-fire pollution barrier").GetComponent(RuntimeType("PollutionObstacle"));
            Rigidbody2D body = GameObject.Find("Fire Spirit").GetComponent<Rigidbody2D>();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.C));
            yield return null;
            yield return null;
            Assert.That((bool)flower.GetType().GetProperty("IsPreparingShot").GetValue(flower), Is.True);
            Vector2 target = (Vector2)flower.GetType().GetProperty("LockedTarget").GetValue(flower);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            body.bodyType = RigidbodyType2D.Kinematic;
            body.position = target + Vector2.up * 2f;
            body.linearVelocity = Vector2.zero;
            Physics2D.SyncTransforms();
            TestContext.WriteLine("Flower=" + flower.transform.position + " Locked target=" + target + " Dodged player=" + body.position);
            foreach (var hit in Physics2D.CircleCastAll(flower.transform.position, 0.14f,
                (target - (Vector2)flower.transform.position).normalized, 14f))
                TestContext.WriteLine("Shot path hit: " + hit.collider.name + " at " + hit.distance);
            float deadline = Time.time + 2.2f;
            float wallDeadline = Time.realtimeSinceStartup + 15f;
            while (Time.time < deadline && Time.realtimeSinceStartup < wallDeadline) yield return null;
            TestContext.WriteLine("After wait: time=" + Time.time + " flower preparing=" + flower.GetType().GetProperty("IsPreparingShot").GetValue(flower)
                + " player=" + body.position + " pollution collider=" + pollution.GetComponent<Collider2D>().enabled);
            foreach (var shot in UnityEngine.Object.FindObjectsByType(RuntimeType("FireFlowerProjectile"), FindObjectsSortMode.None))
                TestContext.WriteLine("Surviving shot=" + ((Component)shot).transform.position);
            Assert.That((Vector2)flower.GetType().GetProperty("LockedTarget").GetValue(flower), Is.EqualTo(target));
            Assert.That((bool)pollution.GetType().GetProperty("IsPurified").GetValue(pollution), Is.True);
            Assert.That(pollution.GetComponent<Collider2D>().enabled, Is.False);
        }
        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            if (keyboard != null) InputSystem.RemoveDevice(keyboard);
            if (Application.isPlaying)
            {
                InputSystem.settings.backgroundBehavior = originalBackground;
                InputSystem.settings.editorInputBehaviorInPlayMode = originalEditorInput;
                Time.timeScale = 1f;
                Time.captureDeltaTime = originalCaptureDeltaTime;
                yield return new ExitPlayMode();
            }
        }
    }
}
