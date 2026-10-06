using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;

namespace WaveValidation
{
    public sealed class WaterWaveTests
    {
        private static Type WaveType => Type.GetType("EmberPrototype.WaterWave, Assembly-CSharp", true);
        private static Component Wave => (Component)UnityEngine.Object.FindFirstObjectByType(WaveType);
        private static void Build(bool reverse)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Type builder = Type.GetType("EmberPrototype.Editor.WaterWavePrefabBuilder, Assembly-CSharp-Editor", true);
            builder.GetMethod("Build").Invoke(null, null);
            GameObject wave = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/EmberPrototype/Prefabs/WaterWave.prefab"));
            wave.transform.position = new Vector2(0f, 1f);
            SerializedObject data = new SerializedObject(wave.GetComponent(WaveType));
            data.FindProperty("direction").enumValueIndex = reverse ? 1 : 0;
            data.FindProperty("travelDistance").floatValue = 2f;
            data.FindProperty("repeat").boolValue = false;
            data.FindProperty("deadly").boolValue = false;
            data.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), "Assets/WaveQA.unity");
        }
        [UnityTest]
        public IEnumerator NativePhysicsMovesRightAndStopsAtDistance()
        {
            Build(false); yield return new EnterPlayMode();
            float deadline = Time.realtimeSinceStartup + 3f;
            while ((bool)WaveType.GetProperty("IsMoving").GetValue(Wave) && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(Wave.GetComponent<Rigidbody2D>().position.x, Is.EqualTo(2f).Within(0.01f));
            Assert.That(Wave.GetComponent<Rigidbody2D>().position.y, Is.EqualTo(1f).Within(0.01f));
            Assert.That(Wave.GetComponent<PolygonCollider2D>().enabled, Is.False);
            WaveType.GetMethod("PlayAt").Invoke(Wave, new object[] { new Vector2(-4f, 3f) });
            yield return new WaitForFixedUpdate(); yield return null;
            Assert.That(Wave.GetComponent<Rigidbody2D>().position.x, Is.InRange(-4f, -3.5f));
            Assert.That(Wave.GetComponent<Rigidbody2D>().position.y, Is.EqualTo(3f).Within(0.01f));
            WaveType.GetMethod("Stop").Invoke(Wave, null);
            Assert.That(Wave.GetComponent<PolygonCollider2D>().enabled, Is.False);
            yield return new ExitPlayMode();
        }
        [UnityTest]
        public IEnumerator NativePhysicsMovesLeftAndMirrorsShape()
        {
            Build(true); yield return new EnterPlayMode(); yield return null;
            Assert.That(Wave.GetComponentInChildren<SpriteRenderer>().flipX, Is.True);
            Assert.That(Wave.GetComponent<PolygonCollider2D>().bounds.size.y, Is.EqualTo(2.65f).Within(0.01f));
            Assert.That(Wave.GetComponentInChildren<SpriteRenderer>().bounds.size.y, Is.EqualTo(2.65f).Within(0.01f));
            float deadline = Time.realtimeSinceStartup + 3f;
            while ((bool)WaveType.GetProperty("IsMoving").GetValue(Wave) && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(Wave.GetComponent<Rigidbody2D>().position.x, Is.EqualTo(-2f).Within(0.01f));
            yield return new ExitPlayMode();
        }
        [UnityTearDown]
        public IEnumerator Cleanup() { if (Application.isPlaying) yield return new ExitPlayMode(); }

        [UnityTest]
        public IEnumerator DeadlyWaveUsesExistingRoomRespawn()
        {
            Build(false);
            SerializedObject data = new SerializedObject(Wave); data.FindProperty("deadly").boolValue = true; data.ApplyModifiedPropertiesWithoutUndo();
            GameObject floor = new GameObject("QA floor"); floor.transform.position = new Vector2(0f, 0.8f);
            floor.AddComponent<BoxCollider2D>().size = new Vector2(10f, 0.2f);
            GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/prefab/player.prefab"));
            player.name = "QA contact player"; player.transform.position = new Vector2(2f, 1.25f);
            Type roomType = Type.GetType("EmberPrototype.PrototypeRoom, Assembly-CSharp", true);
            Component room = player.GetComponent(roomType); if (room == null) room = player.AddComponent(roomType);
            Transform start = new GameObject("QA start").transform; start.position = player.transform.position;
            Transform respawn = new GameObject("QA respawn").transform; respawn.position = new Vector2(-3f, 1.25f);
            roomType.GetMethod("Configure").Invoke(room, new object[] { start, null, -5f });
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), "Assets/WaveQA.unity");
            yield return new EnterPlayMode(); yield return null;
            Component runtimeRoom = GameObject.Find("QA contact player").GetComponent(roomType);
            roomType.GetMethod("SetSpawnPoint").Invoke(runtimeRoom, new object[] { GameObject.Find("QA respawn").transform });
            Rigidbody2D playerBody = runtimeRoom.GetComponent<Rigidbody2D>();
            float deadline = Time.realtimeSinceStartup + 3f;
            while (playerBody.position.x > 0f && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(playerBody.position.x, Is.EqualTo(-3f).Within(0.05f));
            yield return new ExitPlayMode();
        }
    }
}
