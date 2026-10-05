using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace EmberLevelWorkflow
{
    [TestFixture]
    public sealed class PathWorkflowTests
    {
        private Scene scene, previousScene;
        private SimulationMode2D previousSimulation;
        private float previousTick;
        private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        [OneTimeSetUp]
        public void RequireIsolatedBatchProject()
        {
            string tempRoot = Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp"))
                + Path.DirectorySeparatorChar;
            string dataPath = Path.GetFullPath(Application.dataPath);
            if (!Application.isBatchMode || !dataPath.StartsWith(tempRoot, StringComparison.OrdinalIgnoreCase))
                Assert.Ignore("This fixture replaces scratch scenes and may only run in an isolated batch project under LocalApplicationData/Temp.");
        }

        [SetUp]
        public void SetUp()
        {
            previousScene = SceneManager.GetActiveScene();
            scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SceneManager.SetActiveScene(scene);
            previousSimulation = Physics2D.simulationMode;
            previousTick = Time.fixedDeltaTime;
            Physics2D.simulationMode = SimulationMode2D.Script;
            Time.fixedDeltaTime = .02f;
            Undo.ClearAll();
        }

        [TearDown]
        public void TearDown()
        {
            Undo.ClearAll();
            Physics2D.simulationMode = previousSimulation;
            Time.fixedDeltaTime = previousTick;
            if (previousScene.IsValid()) SceneManager.SetActiveScene(previousScene);
            if (scene.IsValid()) EditorSceneManager.CloseScene(scene, true);
        }

        [Test]
        public void LegacyLinearAndSmoothValuesAndOpenEndpointsArePreserved()
        {
            Component path = CreatePath(Vector2.zero, new Vector2(3f, 0f));
            Assert.That(Convert.ToInt32(Get(path, "Shape")), Is.EqualTo(0));
            Assert.That(Get<bool>(path, "IsClosed"), Is.False);
            Assert.That(Get<float>(path, "TotalLength"), Is.EqualTo(3f).Within(.0001f));
            AssertVector(Position(path, -1f), Vector2.zero);
            AssertVector(Position(path, 1.5f), new Vector2(1.5f, 0f));
            AssertVector(Position(path, 9f), new Vector2(3f, 0f));
            SetEnum(path, "shape", 1);
            Invoke(path, "Rebuild");
            Assert.That(Convert.ToInt32(Get(path, "Shape")), Is.EqualTo(1));
            AssertVector(Position(path, 0f), Vector2.zero);
            AssertVector(Position(path, Get<float>(path, "TotalLength")), new Vector2(3f, 0f));
        }

        [Test]
        public void CircleHasExactWorldRadiusDirectionAndClosedStartWithNoWaypoints()
        {
            Component path = CreatePath();
            path.transform.position = new Vector3(3f, 4f, 0f);
            path.transform.localScale = new Vector3(2f, 3f, 1f);
            SetEnum(path, "shape", 2);
            SetFloat(path, "circleRadius", 2f);
            SerializedObject data = new SerializedObject(path);
            data.FindProperty("circleCenter").vector2Value = new Vector2(1f, 1f);
            data.ApplyModifiedPropertiesWithoutUndo();
            Invoke(path, "Rebuild");
            Assert.That(Get<bool>(path, "IsValid"), Is.True);
            Assert.That(Get<bool>(path, "IsClosed"), Is.True);
            float length = Get<float>(path, "TotalLength");
            Assert.That(length, Is.EqualTo(4f * Mathf.PI).Within(.0001f));
            AssertVector(Position(path, 0f), new Vector2(7f, 7f));
            AssertVector(Position(path, length * .25f), new Vector2(5f, 9f));
            AssertVector(Position(path, length), Position(path, 0f));
            SetBool(path, "circleClockwise", true);
            Invoke(path, "Rebuild");
            AssertVector(Position(path, length * .25f), new Vector2(5f, 5f));
        }

        [Test]
        public void ClosedSmoothCurveIncludesReturnSegmentAndMatchingSeamTangents()
        {
            Component path = CreatePath(Vector2.zero, new Vector2(2f, 0f), new Vector2(2f, 2f), new Vector2(0f, 2f));
            SetEnum(path, "shape", 1);
            SetBool(path, "closed", true);
            SetInt(path, "smoothSamplesPerSegment", 32);
            Invoke(path, "Rebuild");
            float length = Get<float>(path, "TotalLength");
            Assert.That(length, Is.GreaterThan(8f));
            AssertVector(Position(path, length), Position(path, 0f));
            Vector2 incoming = (Position(path, length) - Position(path, length - .01f)).normalized;
            Vector2 outgoing = (Position(path, .01f) - Position(path, 0f)).normalized;
            Assert.That(Vector2.Dot(incoming, outgoing), Is.GreaterThan(.98f));
        }

        [Test]
        public void MissingWaypointInvalidatesPathWithoutException()
        {
            Component path = CreatePath(Vector2.zero, Vector2.right);
            SetBool(path, "autoCollectChildPoints", false);
            SerializedObject data = new SerializedObject(path);
            data.FindProperty("points").GetArrayElementAtIndex(1).objectReferenceValue = null;
            data.ApplyModifiedPropertiesWithoutUndo();
            Assert.DoesNotThrow(() => Invoke(path, "Rebuild"));
            Assert.That(Get<bool>(path, "IsValid"), Is.False);
        }

        [Test]
        public void DefaultOnceActivationArrivalAndResetRemainCompatible()
        {
            Component path = CreatePath(Vector2.zero, new Vector2(.1f, 0f));
            Component block = CreateBlock(path);
            Assert.That(Convert.ToInt32(Get(block, "Mode")), Is.EqualTo(0));
            Assert.That(Get<bool>(block, "IsMoving"), Is.False);
            int arrivals = 0;
            Event(block).AddListener(() => arrivals++);
            Invoke(block, "Activate");
            Tick(block, 10);
            Assert.That(arrivals, Is.EqualTo(1));
            Assert.That(Get<bool>(block, "HasArrived"), Is.True);
            Assert.That(Get<bool>(block, "IsMoving"), Is.False);
            AssertVector(block.GetComponent<Rigidbody2D>().position, new Vector2(.1f, 0f));
            Tick(block, 60);
            AssertVector(block.GetComponent<Rigidbody2D>().position, new Vector2(.1f, 0f));
            AssertVector(block.GetComponent<Rigidbody2D>().linearVelocity, Vector2.zero);
            Invoke(block, "Activate");
            Tick(block, 3);
            Assert.That(arrivals, Is.EqualTo(1));
            Invoke(block, "ResetBlock");
            AssertVector(block.GetComponent<Rigidbody2D>().position, Vector2.zero);
            Assert.That(Get<bool>(block, "HasArrived"), Is.False);
            Invoke(block, "Activate");
            Tick(block, 10);
            Assert.That(arrivals, Is.EqualTo(2));
        }

        [Test]
        public void AutomaticStartUsesExistingActivateOnStartField()
        {
            Component path = CreatePath(Vector2.zero, Vector2.right);
            Component block = CreateBlock(path, 0, 0f, true);
            Assert.That(Get<bool>(block, "IsMoving"), Is.True);
            Tick(block, 5);
            Assert.That(block.GetComponent<Rigidbody2D>().position.x, Is.GreaterThan(.05f));
        }

        [Test]
        public void ArrivalListenerResetOverridesQueuedEndpointMoveInNativePhysics()
        {
            Component path = CreatePath(Vector2.zero, new Vector2(.04f, 0f));
            Component block = CreateBlock(path);
            int arrivals = 0;
            Event(block).AddListener(() => { arrivals++; Invoke(block, "ResetBlock"); });
            Invoke(block, "Activate");
            Tick(block, 2);
            Assert.That(arrivals, Is.EqualTo(1));
            Assert.That(Get<bool>(block, "IsMoving"), Is.False);
            Assert.That(Get<bool>(block, "HasArrived"), Is.False);
            AssertVector(block.GetComponent<Rigidbody2D>().position, Vector2.zero);
            Tick(block, 20);
            AssertVector(block.GetComponent<Rigidbody2D>().position, Vector2.zero);
            AssertVector(block.GetComponent<Rigidbody2D>().linearVelocity, Vector2.zero);
        }

        [Test]
        public void PingPongWaitsAtEachEndpointThenReversesAndResetClearsWait()
        {
            Component path = CreatePath(Vector2.zero, new Vector2(.04f, 0f));
            Component block = CreateBlock(path, 1, .06f);
            int arrivals = 0;
            Event(block).AddListener(() => arrivals++);
            Invoke(block, "Activate");
            Tick(block, 2);
            Assert.That(arrivals, Is.EqualTo(1));
            Assert.That(Get<bool>(block, "IsWaiting"), Is.True);
            Assert.That(Get<bool>(block, "IsMoving"), Is.True);
            Tick(block, 2);
            AssertVector(block.GetComponent<Rigidbody2D>().position, new Vector2(.04f, 0f));
            Tick(block, 2);
            Assert.That(block.GetComponent<Rigidbody2D>().position.x, Is.LessThan(.035f));
            Tick(block, 2);
            Assert.That(arrivals, Is.EqualTo(2));
            Assert.That(Get<bool>(block, "IsWaiting"), Is.True);
            Invoke(block, "ResetBlock");
            Assert.That(Get<bool>(block, "IsWaiting"), Is.False);
            Assert.That(Get<bool>(block, "IsMoving"), Is.False);
            AssertVector(block.GetComponent<Rigidbody2D>().position, Vector2.zero);
        }

        [Test]
        public void CircleLoopWrapsContinuouslyAndPreservesFixedStepOvershoot()
        {
            Component path = CreatePath();
            SetEnum(path, "shape", 2);
            SetFloat(path, "circleRadius", 1f);
            Invoke(path, "Rebuild");
            Component block = CreateBlock(path, 2);
            SetFloat(block, "moveSpeed", Get<float>(path, "TotalLength"));
            int arrivals = 0;
            Event(block).AddListener(() => arrivals++);
            Invoke(block, "Activate");
            Rigidbody2D body = block.GetComponent<Rigidbody2D>();
            Vector2 previous = body.position;
            for (int i = 0; i < 65; i++)
            {
                Tick(block, 1);
                Assert.That(Vector2.Distance(previous, body.position), Is.LessThan(.14f), "No discontinuity at a closed wrap.");
                previous = body.position;
            }
            Assert.That(arrivals, Is.EqualTo(1));
            Assert.That(Get<bool>(block, "IsMoving"), Is.True);
            AssertVector(body.position, Position(path, Get<float>(path, "TotalLength") * .3f), .001f);
        }

        [Test]
        public void OpenLoopIsRejectedWithoutMovingOrWrapping()
        {
            Component path = CreatePath(Vector2.zero, Vector2.right);
            Component block = CreateBlock(path, 2);
            LogAssert.Expect(LogType.Warning, "PathMovingBlock Loop requires a closed path or Circle. Use PingPong for an open path.");
            Invoke(block, "Activate");
            Tick(block, 10);
            Assert.That(Get<bool>(block, "IsMoving"), Is.False);
            AssertVector(block.GetComponent<Rigidbody2D>().position, Vector2.zero);
        }

        [Test]
        public void EditorAddingAndRemovingChildWaypointsSupportsUndoAndRedo()
        {
            Component path = CreatePath(Vector2.zero, Vector2.right);
            Type editor = FindType("EmberPrototype.EditorTools.MovementPathEditor");
            MethodInfo add = editor.GetMethod("AddPoint", BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo remove = editor.GetMethod("RemoveLastPoint", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(add, Is.Not.Null);
            Assert.That(remove, Is.Not.Null);
            add.Invoke(null, new object[] { path });
            Undo.FlushUndoRecordObjects();
            Assert.That(path.transform.childCount, Is.EqualTo(3));
            Assert.That(Get<int>(path, "PointCount"), Is.EqualTo(3));
            Undo.PerformUndo();
            Invoke(path, "CollectChildPoints");
            Assert.That(path.transform.childCount, Is.EqualTo(2));
            Assert.That(Get<int>(path, "PointCount"), Is.EqualTo(2));
            Undo.PerformRedo();
            Invoke(path, "CollectChildPoints");
            Assert.That(path.transform.childCount, Is.EqualTo(3));
            remove.Invoke(null, new object[] { path });
            Undo.FlushUndoRecordObjects();
            Assert.That(path.transform.childCount, Is.EqualTo(2));
            Undo.PerformUndo();
            Invoke(path, "CollectChildPoints");
            Assert.That(path.transform.childCount, Is.EqualTo(3));
            Assert.That(Get<int>(path, "PointCount"), Is.EqualTo(3));
        }

        [Test]
        public void GhostEditorCreationLeavesRealBlockPositionUntouched()
        {
            Component path = CreatePath(Vector2.zero, new Vector2(3f, 2f));
            Component block = CreateBlock(path);
            Vector3 original = block.transform.position;
            UnityEditor.Editor editor = UnityEditor.Editor.CreateEditor(block);
            Assert.That(editor.GetType().FullName, Is.EqualTo("EmberPrototype.EditorTools.PathMovingBlockEditor"));
            editor.GetType().GetField("previewFraction", InstanceFlags).SetValue(editor, .8f);
            Vector2 preview = (Vector2)editor.GetType().GetMethod("GetPreviewPosition", InstanceFlags).Invoke(editor, null);
            AssertVector(preview, new Vector2(2.4f, 1.6f));
            Assert.That(block.transform.position, Is.EqualTo(original));
            AssertVector(block.GetComponent<Rigidbody2D>().position, original);
            UnityEngine.Object.DestroyImmediate(editor);
        }

        [Test]
        public void PingPongEndpointWaitRemainsStationaryForManyNativePhysicsSteps()
        {
            Component path = CreatePath(Vector2.zero, new Vector2(.04f, 0f));
            Component block = CreateBlock(path, 1, 2f);
            Invoke(block, "Activate");
            Tick(block, 2);
            Assert.That(Get<bool>(block, "IsWaiting"), Is.True);
            for (int i = 0; i < 60; i++)
            {
                Tick(block, 1);
                AssertVector(block.GetComponent<Rigidbody2D>().position, new Vector2(.04f, 0f));
                AssertVector(block.GetComponent<Rigidbody2D>().linearVelocity, Vector2.zero);
            }
        }

        [Test]
        public void CircleLoopHonorsEndWaitThenContinuesForwardWithoutTeleport()
        {
            Component path = CreatePath();
            SetEnum(path, "shape", 2);
            SetFloat(path, "circleRadius", 1f);
            Invoke(path, "Rebuild");
            Component block = CreateBlock(path, 2, .5f);
            SetFloat(block, "moveSpeed", Get<float>(path, "TotalLength"));
            Invoke(block, "Activate");
            Tick(block, 51);
            Assert.That(Get<bool>(block, "IsWaiting"), Is.True);
            for (int i = 0; i < 15; i++)
            {
                Tick(block, 1);
                AssertVector(block.GetComponent<Rigidbody2D>().position, Position(path, 0f));
            }
            Tick(block, 15);
            Assert.That(Get<bool>(block, "IsWaiting"), Is.False);
            Assert.That(block.GetComponent<Rigidbody2D>().position.y, Is.GreaterThan(.1f));
        }

        private static Component CreatePath(params Vector2[] positions)
        {
            GameObject root = new GameObject("Test Path");
            for (int i = 0; i < positions.Length; i++)
            {
                GameObject point = new GameObject("Point " + i);
                point.transform.SetParent(root.transform);
                point.transform.position = positions[i];
            }
            Component path = root.AddComponent(FindType("EmberPrototype.MovementPath"));
            Invoke(path, "Rebuild");
            return path;
        }

        private static Component CreateBlock(Component path, int mode = 0, float wait = 0f, bool automatic = false)
        {
            GameObject root = new GameObject("Test Block");
            Component block = root.AddComponent(FindType("EmberPrototype.PathMovingBlock"));
            SerializedObject data = new SerializedObject(block);
            data.FindProperty("path").objectReferenceValue = path;
            data.FindProperty("moveSpeed").floatValue = 1f;
            data.FindProperty("travelMode").enumValueIndex = mode;
            data.FindProperty("endWaitTime").floatValue = wait;
            data.FindProperty("activateOnStart").boolValue = automatic;
            data.ApplyModifiedPropertiesWithoutUndo();
            Invoke(block, "Awake");
            Physics2D.SyncTransforms();
            return block;
        }

        private static void Tick(Component block, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Invoke(block, "FixedUpdate");
                Physics2D.Simulate(.02f);
            }
        }

        private static UnityEvent Event(Component block) => (UnityEvent)block.GetType().GetField("onArrived", InstanceFlags).GetValue(block);
        private static Vector2 Position(Component path, float distance) => (Vector2)path.GetType().GetMethod("GetPositionAtDistance").Invoke(path, new object[] { distance });
        private static object Get(object target, string name) => target.GetType().GetProperty(name, InstanceFlags).GetValue(target);
        private static T Get<T>(object target, string name) => (T)Get(target, name);
        private static void Invoke(object target, string method) => target.GetType().GetMethod(method, InstanceFlags).Invoke(target, null);
        private static Type FindType(string name)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(name);
                if (type != null) return type;
            }
            throw new InvalidOperationException("Missing implementation type: " + name);
        }
        private static void SetFloat(Component target, string field, float value)
        { SerializedObject data = new SerializedObject(target); data.FindProperty(field).floatValue = value; data.ApplyModifiedPropertiesWithoutUndo(); }
        private static void SetInt(Component target, string field, int value)
        { SerializedObject data = new SerializedObject(target); data.FindProperty(field).intValue = value; data.ApplyModifiedPropertiesWithoutUndo(); }
        private static void SetEnum(Component target, string field, int value)
        { SerializedObject data = new SerializedObject(target); data.FindProperty(field).enumValueIndex = value; data.ApplyModifiedPropertiesWithoutUndo(); }
        private static void SetBool(Component target, string field, bool value)
        { SerializedObject data = new SerializedObject(target); data.FindProperty(field).boolValue = value; data.ApplyModifiedPropertiesWithoutUndo(); }
        private static void AssertVector(Vector2 actual, Vector2 expected, float tolerance = .0001f)
        { Assert.That(Vector2.Distance(actual, expected), Is.LessThan(tolerance)); }
    }
}
