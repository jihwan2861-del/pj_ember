using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace EmberMovementRegression
{
    public sealed partial class InputAndPhysicsTests
    {
        private const string TilemapArtifactFolder = "C:/Users/admin/Desktop/게임 그래픽 게임/project Ember/ValidationArtifacts/MovementFeel/TilemapSticking";

        [Test]
        public void TilemapNativeSceneImportsOriginalCellsAndCompositeGeometry()
        {
            using (var before = new Terrain("Before", 0f))
            {
                Assert.That(before.CellCount, Is.EqualTo(751));
                Assert.That(before.Collider.compositeOperation, Is.EqualTo(Collider2D.CompositeOperation.Merge));
                Assert.That(before.Collider.composite, Is.Null);
                Assert.That(before.Collider.attachedRigidbody, Is.Null);
                Assert.That(before.Map.GetColliderType(new Vector3Int(-17, -3, 0)), Is.EqualTo(Tile.ColliderType.Grid));
                SaveTerrainGeometry("before-geometry", before);
            }
            using (var after = new Terrain("After", 0f))
            {
                Assert.That(after.CellCount, Is.EqualTo(751));
                Assert.That(after.Composite, Is.Not.Null);
                Assert.That(after.Collider.composite, Is.EqualTo(after.Composite));
                Assert.That(after.Collider.attachedRigidbody.bodyType, Is.EqualTo(RigidbodyType2D.Static));
                Assert.That(after.Composite.geometryType, Is.EqualTo(CompositeCollider2D.GeometryType.Polygons));
                Assert.That(after.Composite.generationType, Is.EqualTo(CompositeCollider2D.GenerationType.Synchronous));
                Assert.That(after.Composite.pathCount, Is.GreaterThan(0));
                SaveTerrainGeometry("after-geometry", after);
            }
        }

        [Test]
        public void TilemapWalkOnActualLongFlatFloorBeforeAfterAndExtrusionComparison()
        {
            WalkResult before = Walk("Before", 0f, "walk-before", new Vector2(-1.2f, -2.5f), 100);
            SetKeys();
            WalkResult after = Walk("After", 0f, "walk-after", new Vector2(-1.2f, -2.5f), 100);
            SetKeys();
            WalkResult padded = Walk("After", 0.01f, "walk-after-extrusion01", new Vector2(-1.2f, -2.5f), 100);
            Assert.That(before.distance, Is.LessThan(2f), "The unchanged original terrain must reproduce the reported flat-floor snag.");
            Assert.That(before.stalledTicks, Is.GreaterThan(0));
            Assert.That(Array.Exists(before.frames, frame => frame.horizontalContact), Is.True);
            Assert.That(after.distance, Is.GreaterThan(5f), "Corrected terrain must allow crossing the actual flat floor's tile seams.");
            Assert.That(after.stalledTicks, Is.EqualTo(0));
            Assert.That(Array.Exists(after.frames, frame => frame.horizontalContact), Is.False);
            Assert.That(padded.distance, Is.GreaterThan(5f));
            Assert.That(padded.stalledTicks, Is.EqualTo(0));
            Assert.That(Array.Exists(padded.frames, frame => frame.horizontalContact), Is.False);
            TestContext.WriteLine("Actual flat-floor walk displacement before / after / padding: " + before.distance + " / " + after.distance + " / " + padded.distance);
        }

        [Test]
        public void TilemapLandingThenWalkingOnActualFloorBeforeAfter()
        {
            WalkResult before = Walk("Before", 0f, "landing-before", new Vector2(-1.2f, -1.8f), 60);
            SetKeys();
            WalkResult after = Walk("After", 0f, "landing-after", new Vector2(-1.2f, -1.8f), 60);
            Assert.That(before.stalledTicks, Is.GreaterThan(0));
            Assert.That(after.grounded, Is.True);
            Assert.That(after.distance, Is.GreaterThan(3f));
            Assert.That(after.stalledTicks, Is.EqualTo(0));
            TestContext.WriteLine("Landing/walking displacement before / after: " + before.distance + " / " + after.distance);
        }

        [Test]
        public void TilemapDashStopsAtActualRaisedWallAndRestoresBodyBeforeAfter()
        {
            DashAtWall("Before", "dash-wall-before");
            SetKeys();
            DashAtWall("After", "dash-wall-after");
        }

        private WalkResult Walk(string variant, float extrusion, string artifact, Vector2 spawn, int count)
        {
            using (var terrain = new Terrain(variant, extrusion))
            using (var f = new Fixture(spawn))
            {
                Frame(f);
                Step(f, 30);
                Assert.That(Get<bool>(f.Player, "wasGrounded"), Is.True, "Fixture must settle on the actual tilemap before walking.");
                Vector2 start = f.Body.position;
                Frame(f, Key.RightArrow);
                var frames = new List<WalkFrame>();
                int stalls = 0;
                var contacts = new ContactPoint2D[12];
                for (int i = 0; i < count; i++)
                {
                    Step(f, 1);
                    bool horizontalContact = false;
                    Vector2 sideNormal = Vector2.zero;
                    Vector2 sidePoint = Vector2.zero;
                    int contactCount = f.Body.GetContacts(contacts);
                    for (int j = 0; j < contactCount; j++)
                    {
                        if (Mathf.Abs(contacts[j].normal.x) <= 0.5f) continue;
                        horizontalContact = true;
                        sideNormal = contacts[j].normal;
                        sidePoint = contacts[j].point;
                    }
                    if (i > 3 && f.Body.linearVelocity.x < 0.5f) stalls++;
                    frames.Add(new WalkFrame { tick = i, x = f.Body.position.x, y = f.Body.position.y, vx = f.Body.linearVelocity.x, vy = f.Body.linearVelocity.y, horizontalContact = horizontalContact, sideNormal = sideNormal, sidePoint = sidePoint });
                }
                var result = new WalkResult
                {
                    variant = variant,
                    extrusion = extrusion,
                    startX = start.x,
                    startY = start.y,
                    endX = f.Body.position.x,
                    endY = f.Body.position.y,
                    distance = f.Body.position.x - start.x,
                    stalledTicks = stalls,
                    grounded = Get<bool>(f.Player, "wasGrounded"),
                    tilemapShapeCount = terrain.Collider.shapeCount,
                    compositePaths = terrain.Composite != null ? terrain.Composite.pathCount : 0,
                    colliderExtrusion = terrain.Collider.extrusionFactor,
                    terrainBoundsMin = terrain.Composite != null ? terrain.Composite.bounds.min : terrain.Collider.bounds.min,
                    terrainBoundsMax = terrain.Composite != null ? terrain.Composite.bounds.max : terrain.Collider.bounds.max,
                    frames = frames.ToArray()
                };
                Save(artifact, result);
                return result;
            }
        }

        private void DashAtWall(string variant, string artifact)
        {
            using (var terrain = new Terrain(variant, 0f))
            using (var f = new Fixture(new Vector2(-8.091217f, 0.10589218f)))
            {
                f.DashCollider.radius = 0.2026f;
                f.DashCollider.offset = new Vector2(0.022029638f, 0.055073977f);
                f.DashCollider.transform.localPosition = new Vector3(0f, -0.13f, 0f);
                Frame(f);
                Step(f, 35);
                Assert.That(Get<bool>(f.Player, "wasGrounded"), Is.True);
                Frame(f, Key.C, Key.Z, Key.RightArrow);
                Assert.That(f.State, Is.EqualTo("BurstDashing"));
                int ticks = 0;
                while (f.State == "BurstDashing" && ticks++ < 25) Step(f, 1);
                Assert.That(f.State, Is.EqualTo("Free"));
                Assert.That(f.BodyCollider.enabled, Is.True);
                Assert.That(f.DashCollider.enabled, Is.False);
                Assert.That(f.Body.position.x, Is.LessThan(-7.10f), "The authored raised wall begins at x=-6.96.");
                Save(artifact, new DashResult { variant = variant, ticks = ticks, x = f.Body.position.x, y = f.Body.position.y, vx = f.Body.linearVelocity.x, vy = f.Body.linearVelocity.y, bodyEnabled = f.BodyCollider.enabled, dashEnabled = f.DashCollider.enabled });
            }
        }

        private static void SaveTerrainGeometry(string name, Terrain terrain)
        {
            Save(name, new TerrainResult
            {
                cells = terrain.CellCount,
                tilemapShapes = terrain.Collider.shapeCount,
                missingComposite = terrain.Collider.composite == null,
                missingAttachedBody = terrain.Collider.attachedRigidbody == null,
                inspectorWarningCondition = terrain.Collider.compositeOperation != Collider2D.CompositeOperation.None && terrain.Collider.composite == null,
                compositePaths = terrain.Composite != null ? terrain.Composite.pathCount : 0,
                compositePoints = terrain.Composite != null ? terrain.Composite.pointCount : 0,
                compositePhysicsShapes = terrain.Composite != null ? terrain.Composite.shapeCount : 0,
                compositePathsOnOpen = terrain.CompositePathsOnOpen,
                compositePathsAfterProcess = terrain.CompositePathsAfterProcess,
                tilemapScale = terrain.Map.transform.lossyScale,
                gridPosition = terrain.Map.transform.parent.position
            });
        }

        private static void Save(string name, object value)
        {
            Directory.CreateDirectory(TilemapArtifactFolder);
            File.WriteAllText(Path.Combine(TilemapArtifactFolder, name + ".json"), JsonUtility.ToJson(value, true));
        }

        [Serializable] private sealed class TerrainResult { public int cells, tilemapShapes, compositePaths, compositePoints, compositePhysicsShapes, compositePathsOnOpen, compositePathsAfterProcess; public bool missingComposite, missingAttachedBody, inspectorWarningCondition; public Vector3 tilemapScale, gridPosition; }
        [Serializable] private sealed class WalkFrame { public int tick; public float x, y, vx, vy; public bool horizontalContact; public Vector2 sideNormal, sidePoint; }
        [Serializable] private sealed class WalkResult { public string variant; public float extrusion, colliderExtrusion, startX, startY, endX, endY, distance; public int stalledTicks, tilemapShapeCount, compositePaths; public bool grounded; public Vector3 terrainBoundsMin, terrainBoundsMax; public WalkFrame[] frames; }
        [Serializable] private sealed class DashResult { public string variant; public int ticks; public float x, y, vx, vy; public bool bodyEnabled, dashEnabled; }

        private sealed class Terrain : IDisposable
        {
            private readonly Scene scene;
            public readonly Tilemap Map;
            public readonly TilemapCollider2D Collider;
            public readonly CompositeCollider2D Composite;
            public readonly int CellCount;
            public readonly int CompositePathsOnOpen;
            public readonly int CompositePathsAfterProcess;
            public Terrain(string variant, float extrusion)
            {
                scene = EditorSceneManager.OpenScene("Assets/TileFixtures/Scene2Terrain" + variant + ".unity", OpenSceneMode.Additive);
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    Map = root.GetComponentInChildren<Tilemap>();
                    if (Map != null) break;
                }
                Assert.That(Map, Is.Not.Null);
                Collider = Map.GetComponent<TilemapCollider2D>();
                Composite = Map.GetComponent<CompositeCollider2D>();
                CompositePathsOnOpen = Composite != null ? Composite.pathCount : 0;
                foreach (Vector3Int position in Map.cellBounds.allPositionsWithin)
                    if (Map.HasTile(position)) CellCount++;
                if (extrusion != Collider.extrusionFactor)
                {
                    Collider.extrusionFactor = extrusion;
                    Map.RefreshAllTiles();
                }
                Collider.ProcessTilemapChanges();
                CompositePathsAfterProcess = Composite != null ? Composite.pathCount : 0;
                Physics2D.SyncTransforms();
            }
            public void Dispose() { EditorSceneManager.CloseScene(scene, true); }
        }
    }
}
