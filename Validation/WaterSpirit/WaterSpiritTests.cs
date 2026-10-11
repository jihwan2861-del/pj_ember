using System;
using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;

namespace WaterSpiritValidation
{
    public sealed class WaterSpiritTests
    {
        private static Type SpiritType => Type.GetType("EmberPrototype.WaterSpirit, Assembly-CSharp", true);
        private static Component Spirit => (Component)UnityEngine.Object.FindFirstObjectByType(SpiritType);
        private static void Call(string method, params object[] args) => SpiritType.GetMethod(method).Invoke(Spirit, args);
        private static IEnumerator WaitGameTime(float seconds)
        {
            float end = Time.time + seconds;
            while (Time.time < end) yield return null;
        }
        private static Component Wave => (Component)SpiritType.GetProperty("ActiveWave").GetValue(Spirit);

        private static void Build(bool reverse)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Type builder = Type.GetType("EmberPrototype.Editor.WaterSpiritPrefabBuilder, Assembly-CSharp-Editor", true);
            builder.GetMethod("Build").Invoke(null, null);
            GameObject spirit = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/EmberPrototype/Prefabs/WaterSpirit.prefab"));
            // This fixture exercises the legacy non-AI execution path.
            Component ai = spirit.GetComponent(Type.GetType("EmberPrototype.WaterSpiritAI, Assembly-CSharp", true));
            if (ai != null) ((Behaviour)ai).enabled = false;
            SerializedObject data = new SerializedObject(spirit.GetComponent(SpiritType));
            data.FindProperty("autoAttack").boolValue = false;
            data.FindProperty("direction").enumValueIndex = reverse ? 1 : 0;
            data.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), "Assets/WaterSpiritQA.unity");
        }

        [UnityTest]
        public IEnumerator RightAttackReleasesOneWaveOnSwingAndReturnsToIdle()
        {
            Build(false); yield return new EnterPlayMode(); yield return null;
            Call("Attack"); Call("Attack");
            Assert.That(Wave, Is.Null);
            yield return WaitGameTime(0.32f);
            Assert.That(Wave, Is.Not.Null);
            Assert.That(Wave.GetComponent<Rigidbody2D>().position.x, Is.GreaterThan(0.8f));
            Assert.That(UnityEngine.Object.FindObjectsByType(Wave.GetType(), FindObjectsSortMode.None).Length, Is.EqualTo(1));
            yield return WaitGameTime(0.25f);
            Assert.That(Spirit.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Idle"), Is.True);
            // Shorten travel distance and verify that the launched wave never repeats.
            SerializedObject waveData = new SerializedObject(Wave);
            waveData.FindProperty("travelDistance").floatValue = 1f;
            waveData.ApplyModifiedPropertiesWithoutUndo();
            yield return WaitGameTime(1.2f);
            Assert.That((bool)Wave.GetType().GetProperty("IsMoving").GetValue(Wave), Is.False);
            Assert.That(Wave.GetComponent<PolygonCollider2D>().enabled, Is.False);
            yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator LeftAttackAndMoveUseTheAuthoredFrames()
        {
            Build(true); yield return new EnterPlayMode(); yield return null;
            Call("SetMoveVelocity", new Vector2(-1f, 0f));
            yield return WaitGameTime(0.15f);
            Assert.That(Spirit.transform.position.x, Is.LessThan(0f));
            Assert.That(Spirit.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Move"), Is.True);
            Assert.That(Spirit.GetComponent<SpriteRenderer>().sprite.name, Does.Match("Sprite-0001_[5-8]$"));
            Call("Attack");
            float attackX = Spirit.transform.position.x;
            yield return WaitGameTime(0.32f);
            Assert.That(Spirit.transform.position.x, Is.EqualTo(attackX).Within(0.01f));
            Assert.That(Spirit.GetComponent<SpriteRenderer>().flipX, Is.False);
            Assert.That(Wave.GetComponent<Rigidbody2D>().position.x, Is.LessThan(attackX - 0.8f));
            Assert.That(Wave.GetComponentInChildren<SpriteRenderer>().flipX, Is.True);
            yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator DeferredRespawnCancelsAttackUntilResumed()
        {
            Build(false); yield return new EnterPlayMode(); yield return null;
            Call("Attack");
            Type resetType = Type.GetType("EmberPrototype.MapReset, Assembly-CSharp", true);
            resetType.GetMethod("ResetScene").Invoke(null, new object[] { Spirit.gameObject.scene, true });
            Call("Attack");
            yield return WaitGameTime(0.4f);
            Assert.That(Wave, Is.Null);
            Assert.That((bool)SpiritType.GetProperty("IsAttacking").GetValue(Spirit), Is.False);
            resetType.GetMethod("ResumeWaves").Invoke(null, new object[] { Spirit.gameObject.scene }); Call("Attack");
            yield return WaitGameTime(0.32f);
            Assert.That(Wave, Is.Not.Null);
            Component firedWave = Wave;
            Spirit.gameObject.SetActive(false);
            Assert.That(firedWave.GetComponent<PolygonCollider2D>().enabled, Is.False);
            yield return new ExitPlayMode();
        }

        [UnityTearDown]
        public IEnumerator Cleanup() { if (Application.isPlaying) yield return new ExitPlayMode(); }
    }
}
