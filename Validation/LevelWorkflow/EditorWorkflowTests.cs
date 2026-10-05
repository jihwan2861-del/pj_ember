using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace EmberLevelWorkflow
{
    [TestFixture]
    public sealed class EditorWorkflowTests
    {
        private Scene scene;
        private Tile tile;

        [OneTimeSetUp]
        public void RequireIsolatedBatchProject()
        {
            string taskTempRoot = Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp"))
                + Path.DirectorySeparatorChar;
            if (!Application.isBatchMode || !Path.GetFullPath(Application.dataPath).StartsWith(taskTempRoot, StringComparison.OrdinalIgnoreCase))
                Assert.Ignore("Run with Validation/LevelWorkflow/Run-IsolatedValidation.ps1 in the temporary QA project.");
        }

        [SetUp]
        public void SetUp()
        {
            scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SceneManager.SetActiveScene(scene);
            Undo.ClearAll();
        }

        [TearDown]
        public void TearDown()
        {
            if (tile != null) UnityEngine.Object.DestroyImmediate(tile);
            Undo.ClearAll();
            if (scene.IsValid()) EditorSceneManager.CloseScene(scene, true);
        }

        [Test]
        public void RoomTemplateSeparatesTerrainSpikesAndDecorationAndPreservesTileScale()
        {
            GameObject root = CreateRoom(new Vector2(4f, 2f));
            Assert.That(root.scene, Is.EqualTo(scene));
            Assert.That(root.transform.localScale, Is.EqualTo(Vector3.one));
            Assert.That(root.transform.Find("Mechanics"), Is.Not.Null);
            Assert.That(root.transform.Find("Paths"), Is.Not.Null);
            Assert.That(root.GetComponentInChildren(FindType("CameraRoom")), Is.Not.Null);
            Assert.That(root.GetComponentInChildren(FindType("RoomSpawnPoint")), Is.Not.Null);
            Tilemap terrain = FindTilemap(root, "Terrain");
            Tilemap spikes = FindTilemap(root, "Spikes");
            Tilemap decoration = FindTilemap(root, "Decorations");
            Vector3 cell = terrain.GetCellCenterWorld(Vector3Int.right) - terrain.GetCellCenterWorld(Vector3Int.zero);
            Assert.That(cell.x, Is.EqualTo(.5f).Within(.0001f));
            Assert.That(terrain.GetComponent<Rigidbody2D>().bodyType, Is.EqualTo(RigidbodyType2D.Static));
            Assert.That(terrain.GetComponent<TilemapCollider2D>().compositeOperation, Is.EqualTo(Collider2D.CompositeOperation.Merge));
            Assert.That(terrain.GetComponent<CompositeCollider2D>().geometryType, Is.EqualTo(CompositeCollider2D.GeometryType.Polygons));
            Assert.That(terrain.GetComponent<CompositeCollider2D>().generationType, Is.EqualTo(CompositeCollider2D.GenerationType.Synchronous));
            Assert.That(spikes.GetComponent<TilemapCollider2D>().isTrigger, Is.True);
            Assert.That(spikes.GetComponent(FindType("RoomHazard")), Is.Not.Null);
            Assert.That(spikes.GetComponent<BoxCollider2D>(), Is.Null, "A tilemap hazard must not create an invisible box at its origin.");
            Assert.That(decoration.GetComponents<Collider2D>(), Is.Empty);
            Assert.That(scene.path, Is.Empty, "The tool must not silently save or replace an authored scene.");
        }

        [Test]
        public void PaintedTerrainAutomaticallyMergesAndSpikesUseOnlyTheirPaintedCells()
        {
            GameObject root = CreateRoom(Vector2.zero);
            Tilemap terrain = FindTilemap(root, "Terrain");
            Tilemap spikes = FindTilemap(root, "Spikes");
            tile = ScriptableObject.CreateInstance<Tile>();
            tile.colliderType = Tile.ColliderType.Grid;
            terrain.SetTile(new Vector3Int(0, -2, 0), tile);
            terrain.SetTile(new Vector3Int(1, -2, 0), tile);
            terrain.SetTile(new Vector3Int(2, -2, 0), tile);
            terrain.GetComponent<TilemapCollider2D>().ProcessTilemapChanges();
            CompositeCollider2D composite = terrain.GetComponent<CompositeCollider2D>();
            Assert.That(composite.pathCount, Is.GreaterThan(0));
            Assert.That(terrain.GetComponent<TilemapCollider2D>().composite, Is.EqualTo(composite));
            spikes.SetTile(new Vector3Int(8, 0, 0), tile);
            spikes.GetComponent<TilemapCollider2D>().ProcessTilemapChanges();
            Physics2D.SyncTransforms();
            TilemapCollider2D hazardCollider = spikes.GetComponent<TilemapCollider2D>();
            Assert.That(hazardCollider.OverlapPoint(spikes.GetCellCenterWorld(new Vector3Int(8, 0, 0))), Is.True);
            Assert.That(hazardCollider.OverlapPoint(spikes.GetCellCenterWorld(Vector3Int.zero)), Is.False);
            Assert.That(spikes.GetComponents<Collider2D>().Length, Is.EqualTo(1));
        }

        [Test]
        public void RoomCreationCanBeUndoneAndRedoneAsOneAction()
        {
            GameObject root = CreateRoom(Vector2.zero);
            Undo.PerformUndo();
            Assert.That(root == null, Is.True);
            Assert.That(scene.GetRootGameObjects(), Is.Empty);
            Undo.PerformRedo();
            GameObject[] roots = scene.GetRootGameObjects();
            Assert.That(roots.Length, Is.EqualTo(1));
            Tilemap restoredTerrain = FindTilemap(roots[0], "Terrain");
            Assert.That(restoredTerrain.GetComponent<CompositeCollider2D>(), Is.Not.Null);
            Assert.That(restoredTerrain.GetComponent<Rigidbody2D>().bodyType, Is.EqualTo(RigidbodyType2D.Static));
            Assert.That(restoredTerrain.GetComponent<TilemapCollider2D>().compositeOperation, Is.EqualTo(Collider2D.CompositeOperation.Merge));
            Assert.That((restoredTerrain.GetCellCenterWorld(Vector3Int.right) - restoredTerrain.GetCellCenterWorld(Vector3Int.zero)).x,
                Is.EqualTo(.5f).Within(.0001f));
            Assert.That(FindTilemap(roots[0], "Spikes").GetComponent<BoxCollider2D>(), Is.Null);
        }

        [Test]
        public void FittingCameraBoundsUsesWorldViewportAndPreservesRoomCenter()
        {
            GameObject root = CreateRoom(new Vector2(5f, 3f));
            Component room = root.GetComponentInChildren(FindType("CameraRoom"));
            Bounds before = (Bounds)room.GetType().GetProperty("WorldBounds").GetValue(room);
            Camera camera = new GameObject("Validation Camera").AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 3f;
            camera.aspect = 16f / 9f;
            Assert.That((bool)InvokeWorkflow("FitRoomToCamera", room, camera), Is.True);
            Bounds fitted = (Bounds)room.GetType().GetProperty("WorldBounds").GetValue(room);
            Assert.That(fitted.center, Is.EqualTo(before.center));
            Assert.That(fitted.size.x, Is.EqualTo(6f * camera.aspect).Within(.0001f));
            Assert.That(fitted.size.y, Is.EqualTo(6f).Within(.0001f));
            Vector2 clamped = (Vector2)room.GetType().GetMethod("ClampCameraCenter").Invoke(room, new object[] { new Vector2(100f, 100f), new Vector2(3f * camera.aspect, 3f) });
            Assert.That(clamped.x, Is.EqualTo(fitted.center.x).Within(.0001f));
            Assert.That(clamped.y, Is.EqualTo(fitted.center.y).Within(.0001f));
        }

        [Test]
        public void BoxHazardsStillCreateOneTriggerColliderWhenMissing()
        {
            GameObject hazardObject = new GameObject("Box Hazard");
            Component hazard = hazardObject.AddComponent(FindType("RoomHazard"));
            MethodInfo awake = hazard.GetType().GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);
            awake.Invoke(hazard, null);
            awake.Invoke(hazard, null);
            Assert.That(hazardObject.GetComponents<Collider2D>().Length, Is.EqualTo(1));
            Assert.That(hazardObject.GetComponent<BoxCollider2D>().isTrigger, Is.True);
        }

        [Test]
        public void FittingPaintedTilemapsAccountsForGridScaleAndCanBeUndone()
        {
            GameObject root = CreateRoom(new Vector2(10f, 4f));
            Component room = root.GetComponentInChildren(FindType("CameraRoom"));
            Bounds before = (Bounds)room.GetType().GetProperty("WorldBounds").GetValue(room);
            Tilemap terrain = FindTilemap(root, "Terrain");
            tile = ScriptableObject.CreateInstance<Tile>();
            terrain.SetTile(new Vector3Int(2, 4, 0), tile);
            terrain.SetTile(new Vector3Int(4, 5, 0), tile);
            // A separate editor button click starts an Undo action after room creation.
            Undo.ClearAll();
            Assert.That((bool)InvokeWorkflow("FitRoomToTilemaps", room, new[] { terrain }), Is.True);
            Bounds fitted = (Bounds)room.GetType().GetProperty("WorldBounds").GetValue(room);
            Assert.That(fitted.center.x, Is.EqualTo(11.75f).Within(.0001f));
            Assert.That(fitted.center.y, Is.EqualTo(6.5f).Within(.0001f));
            Assert.That(fitted.size.x, Is.EqualTo(1.5f).Within(.0001f));
            Assert.That(fitted.size.y, Is.EqualTo(1f).Within(.0001f));
            Undo.FlushUndoRecordObjects();
            Undo.PerformUndo();
            Bounds restored = (Bounds)room.GetType().GetProperty("WorldBounds").GetValue(room);
            Assert.That(restored, Is.EqualTo(before));
        }

        [Test]
        public void ConnectingCameraPreservesExistingTargetAndTransformAndSupportsUndo()
        {
            GameObject root = CreateRoom(Vector2.zero);
            Component room = root.GetComponentInChildren(FindType("CameraRoom"));
            Camera camera = new GameObject("Validation Camera").AddComponent<Camera>();
            camera.orthographic = false;
            camera.transform.position = new Vector3(9f, 7f, -10f);
            Vector3 beforePosition = camera.transform.position;
            Component controller = camera.gameObject.AddComponent(FindType("CelesteRoomCamera"));
            Transform originalTarget = new GameObject("Custom Camera Target").transform;
            controller.GetType().GetMethod("SetTarget").Invoke(controller, new object[] { originalTarget, false });
            Undo.ClearAll();
            Assert.That((bool)InvokeWorkflow("ConnectCameraToRoom", room, camera), Is.True);
            SerializedObject settings = new SerializedObject(controller);
            Assert.That(settings.FindProperty("target").objectReferenceValue, Is.EqualTo(originalTarget));
            Assert.That(settings.FindProperty("startingRoom").objectReferenceValue, Is.EqualTo(room));
            Assert.That(camera.orthographic, Is.True);
            Assert.That(camera.transform.position, Is.EqualTo(beforePosition));
            Undo.FlushUndoRecordObjects();
            Undo.PerformUndo();
            settings.Update();
            Assert.That(settings.FindProperty("startingRoom").objectReferenceValue, Is.Null);
            Assert.That(settings.FindProperty("target").objectReferenceValue, Is.EqualTo(originalTarget));
            Assert.That(camera.orthographic, Is.False);
        }

        private static GameObject CreateRoom(Vector2 center)
        {
            return (GameObject)InvokeWorkflow("CreateRoom", "Validation Room", center, new Vector2(20f, 12f), .5f);
        }

        private static Tilemap FindTilemap(GameObject root, string name)
        {
            foreach (Tilemap map in root.GetComponentsInChildren<Tilemap>())
                if (map.name == name) return map;
            Assert.Fail("Missing tilemap: " + name);
            return null;
        }

        private static object InvokeWorkflow(string name, params object[] arguments)
        {
            Type[] parameterTypes = Array.ConvertAll(arguments, argument => argument.GetType());
            MethodInfo method = FindType("EmberLevelWorkflowWindow").GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null, parameterTypes, null);
            Assert.That(method, Is.Not.Null, name);
            return method.Invoke(null, arguments);
        }

        private static Type FindType(string name)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetName().Name != "Assembly-CSharp" && assembly.GetName().Name != "Assembly-CSharp-Editor") continue;
                foreach (Type type in assembly.GetTypes()) if (type.Name == name) return type;
            }
            throw new InvalidOperationException("Production type not compiled: " + name);
        }
    }
}
