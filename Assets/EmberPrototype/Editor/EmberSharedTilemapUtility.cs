using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

namespace EmberPrototype.Editor
{
    /// <summary>Creates shared geometry and merges aligned room maps without saving the user's scene.</summary>
    public static class EmberSharedTilemapUtility
    {
        private const float Tolerance = 0.0001f;
        private static readonly string[] Roles = { "Terrain", "Spikes", "Decorations" };
        private static readonly string[] SkinFields = { "fillMaterial", "outlineMaterial", "grassSprite", "textureWorldSize",
            "cornerRadius", "outlineWidth", "grassPatchWidth", "grassHeight", "openBottomInfill", "openBottomY" };

        private sealed class RoomPlan
        {
            internal CameraRoom room;
            internal Transform root;
            internal Grid grid;
            internal Tilemap[] maps;
            internal DoodleTerrainSkin skin;
            internal bool skinEnabled;
            internal Vector3 shift;
        }

        private struct CellData
        {
            internal TileBase tile;
            internal Color color;
            internal Matrix4x4 matrix;
            internal TileFlags flags;
            internal Tile.ColliderType collider;

            internal bool Matches(CellData other) => tile == other.tile && color.Equals(other.color)
                && matrix.Equals(other.matrix) && flags == other.flags && collider == other.collider;
        }

        public static LevelTilemaps[] FindAll(Scene scene)
        {
            var result = new List<LevelTilemaps>();
            if (scene.IsValid() && scene.isLoaded)
                foreach (GameObject root in scene.GetRootGameObjects())
                    result.AddRange(root.GetComponentsInChildren<LevelTilemaps>(true));
            return result.ToArray();
        }

        public static LevelTilemaps Create(Scene scene, float tileSize)
        {
            RequireEditableScene(scene);
            if (!Finite(tileSize) || tileSize <= 0f) throw new ArgumentException("타일 크기는 0보다 큰 유한한 값이어야 합니다.");
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Create Shared Ember Tilemaps");
            try
            {
                LevelTilemaps result = CreateCore(scene, tileSize);
                EditorSceneManager.MarkSceneDirty(scene);
                Undo.CollapseUndoOperations(group);
                return result;
            }
            catch { Undo.RevertAllDownToGroup(group); throw; }
        }

        private static LevelTilemaps CreateCore(Scene scene, float tileSize)
        {
            GameObject root = NewObject(GameObjectUtility.GetUniqueNameForSibling(null, "WorldGeometry"), null, scene);
            LevelTilemaps level = Undo.AddComponent<LevelTilemaps>(root);
            GameObject gridObject = NewObject("Grid", root.transform, scene);
            Undo.RecordObject(gridObject.transform, "Set Shared Tile Size");
            gridObject.transform.localScale = new Vector3(tileSize, tileSize, 1f);
            Grid grid = Undo.AddComponent<Grid>(gridObject);
            Undo.RecordObject(grid, "Configure Shared Grid");
            grid.cellSize = new Vector3(1f, 1f, 0f);
            grid.cellGap = Vector3.zero;
            grid.cellLayout = GridLayout.CellLayout.Rectangle;
            Tilemap terrain = NewMap("Terrain", grid.transform, scene, 0);
            Rigidbody2D body = Undo.AddComponent<Rigidbody2D>(terrain.gameObject);
            body.bodyType = RigidbodyType2D.Static;
            CompositeCollider2D composite = Undo.AddComponent<CompositeCollider2D>(terrain.gameObject);
            composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
            composite.generationType = CompositeCollider2D.GenerationType.Synchronous;
            composite.vertexDistance = 0.0005f;
            composite.offsetDistance = 0.00005f;
            TilemapCollider2D terrainCollider = Undo.AddComponent<TilemapCollider2D>(terrain.gameObject);
            terrainCollider.compositeOperation = Collider2D.CompositeOperation.Merge;
            terrainCollider.extrusionFactor = 0f;
            Tilemap spikes = NewMap("Spikes", grid.transform, scene, 1);
            TilemapCollider2D spikesCollider = Undo.AddComponent<TilemapCollider2D>(spikes.gameObject);
            spikesCollider.isTrigger = true;
            spikesCollider.compositeOperation = Collider2D.CompositeOperation.None;
            Undo.AddComponent<RoomHazard>(spikes.gameObject);
            Tilemap decorations = NewMap("Decorations", grid.transform, scene, -1);
            Undo.RecordObject(level, "Connect Shared Tilemaps");
            level.Configure(grid, terrain, spikes, decorations);
            return level;
        }

        public static LevelTilemaps MergeRooms(CameraRoom[] rooms, bool alignToGrid)
        {
            // All compatibility, cell conflicts and references are checked before any Undo mutation.
            if (rooms == null || rooms.Length == 0) throw new ArgumentException("합칠 방을 선택해주세요.");
            Scene scene = rooms[0] != null ? rooms[0].gameObject.scene : default;
            RequireEditableScene(scene);
            if (FindAll(scene).Length != 0)
                throw new InvalidOperationException("이미 공유 지형이 있습니다. 기존 공유 지형에 연결하거나 별도 씬에서 병합해주세요.");
            var plans = new List<RoomPlan>();
            var seenRooms = new HashSet<CameraRoom>();
            var seenRoots = new HashSet<Transform>();
            Vector3 origin = default;
            float tileSize = 0f;
            Tilemap[] templates = null;
            var cells = new[] { new Dictionary<Vector3Int, CellData>(), new Dictionary<Vector3Int, CellData>(),
                new Dictionary<Vector3Int, CellData>() };
            foreach (CameraRoom room in rooms)
            {
                if (room == null || room.gameObject.scene != scene || EditorUtility.IsPersistent(room)
                    || !seenRooms.Add(room)) throw new InvalidOperationException("같은 씬의 서로 다른 방을 선택해주세요.");
                if (room.SharedTilemaps != null) throw new InvalidOperationException(room.name + ": 이미 공유 지형에 연결되어 있습니다.");
                Transform root = room.transform.parent != null ? room.transform.parent : room.transform;
                if (!seenRoots.Add(root) || root.GetComponentsInChildren<CameraRoom>(true).Length != 1)
                    throw new InvalidOperationException(room.name + ": 카메라 방 하나를 가진 방 부모가 필요합니다.");
                if (PrefabUtility.IsPartOfPrefabInstance(root.gameObject))
                    throw new InvalidOperationException(root.name + ": 프리팹 인스턴스는 먼저 Unpack 후 합쳐주세요.");
                var plan = new RoomPlan { room = room, root = root, maps = new Tilemap[3] };
                foreach (Tilemap map in root.GetComponentsInChildren<Tilemap>(true))
                {
                    int role = Array.IndexOf(Roles, map.name);
                    if (role < 0) throw new InvalidOperationException(root.name + ": 추가 타일맵 " + map.name + "은 자동 병합하지 않습니다.");
                    if (plan.maps[role] != null) throw new InvalidOperationException(root.name + ": " + map.name + "이 여러 개입니다.");
                    plan.maps[role] = map;
                }
                for (int role = 0; role < 3; role++)
                    if (plan.maps[role] == null) throw new InvalidOperationException(root.name + ": " + Roles[role] + "이 없습니다.");
                DoodleTerrainSkin[] skins = root.GetComponentsInChildren<DoodleTerrainSkin>(true);
                if (skins.Length > 1) throw new InvalidOperationException(root.name + ": 스킨이 여러 개입니다.");
                plan.skin = skins.Length == 1 ? skins[0] : null;
                plan.skinEnabled = plan.skin != null && plan.skin.enabled;
                if (plan.skin != null && new SerializedObject(plan.skin).FindProperty("source").objectReferenceValue != plan.maps[0])
                    throw new InvalidOperationException(root.name + ": 스킨 Source가 Terrain과 다릅니다.");
                plan.grid = plan.maps[0].layoutGrid;
                if (plan.grid == null) throw new InvalidOperationException(root.name + ": Grid가 없습니다.");
                for (int role = 0; role < 3; role++) ValidateMap(plan.maps[role], role, plan.grid);
                Vector3 mapOrigin = plan.maps[0].CellToWorld(Vector3Int.zero);
                float size = (plan.maps[0].CellToWorld(Vector3Int.right) - mapOrigin).x;
                if (plans.Count == 0) { origin = mapOrigin; tileSize = size; templates = plan.maps; }
                if (!Near(size, tileSize)) throw new InvalidOperationException(root.name + ": 타일 한 칸 크기가 다릅니다.");
                if (plans.Count > 0) ValidateMatchingSkin(plans[0], plan);
                Vector3 relative = (mapOrigin - origin) / tileSize;
                Vector3 desiredOrigin = origin + new Vector3(Mathf.Round(relative.x) * tileSize, Mathf.Round(relative.y) * tileSize, 0f);
                desiredOrigin.z = origin.z;
                plan.shift = desiredOrigin - mapOrigin;
                if (!alignToGrid && plan.shift.sqrMagnitude > Tolerance * Tolerance)
                    throw new InvalidOperationException(root.name + ": 격자가 어긋나 있습니다. 격자 정렬 후 합치기를 사용해주세요.");
                if (plan.shift.sqrMagnitude < Tolerance * Tolerance) plan.shift = Vector3.zero;
                for (int role = 0; role < 3; role++)
                {
                    Tilemap map = plan.maps[role];
                    ValidateMatchingMap(templates[role], map, role);
                    Vector3 roleOrigin = map.CellToWorld(Vector3Int.zero);
                    if ((roleOrigin - mapOrigin).sqrMagnitude > Tolerance * Tolerance
                        || !Near((map.CellToWorld(Vector3Int.right) - roleOrigin).x, tileSize))
                        throw new InvalidOperationException(root.name + ": 각 타일맵의 원점이 다릅니다.");
                    Vector3Int offset = new Vector3Int(Mathf.RoundToInt(relative.x), Mathf.RoundToInt(relative.y), 0);
                    foreach (Vector3Int cell in map.cellBounds.allPositionsWithin)
                    {
                        TileBase tile = map.GetTile(cell);
                        if (tile == null) continue;
                        if (cell.z != 0) throw new InvalidOperationException(root.name + ": Z=0 밖의 타일은 지원하지 않습니다.");
                        Vector3Int destination = cell + offset;
                        CellData value = new CellData { tile = tile, color = map.GetColor(cell), matrix = map.GetTransformMatrix(cell),
                            flags = map.GetTileFlags(cell), collider = map.GetColliderType(cell) };
                        if (cells[role].TryGetValue(destination, out CellData previous) && !value.Matches(previous))
                            throw new InvalidOperationException(Roles[role] + " " + destination + ": 서로 다른 타일/색/변환/충돌이 겹칩니다. 병합하지 않았습니다.");
                        cells[role][destination] = value;
                    }
                }
                plans.Add(plan);
            }
            ValidateExternalReferences(scene, plans);

            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Merge Ember Room Tilemaps");
            try
            {
                // Let each old skin restore the renderer state it owns before copying that renderer.
                foreach (RoomPlan plan in plans)
                    if (plan.skin != null)
                    {
                        Undo.RecordObject(plan.skin, "Disable Previous Room Skin");
                        plan.skin.enabled = false;
                    }
                LevelTilemaps level = CreateCore(scene, tileSize);
                Undo.RecordObject(level.Grid.transform, "Preserve Shared Grid Origin");
                level.Grid.transform.position = origin;
                Tilemap[] targets = level.Maps;
                for (int role = 0; role < 3; role++)
                {
                    Tilemap target = targets[role];
                    Tilemap template = templates[role];
                    Undo.RegisterCompleteObjectUndo(target, "Copy Room Cells");
                    target.color = template.color;
                    target.tileAnchor = template.tileAnchor;
                    target.orientation = template.orientation;
                    target.orientationMatrix = template.orientationMatrix;
                    target.animationFrameRate = template.animationFrameRate;
                    target.gameObject.layer = template.gameObject.layer;
                    target.gameObject.tag = template.gameObject.tag;
                    target.gameObject.SetActive(template.gameObject.activeSelf);
                    var renderer = target.GetComponent<TilemapRenderer>();
                    Undo.RecordObject(renderer, "Preserve Tilemap Rendering");
                    EditorUtility.CopySerialized(template.GetComponent<TilemapRenderer>(), renderer);
                    renderer.forceRenderingOff = false;
                    CopyPhysics(template, target);
                    foreach (var pair in cells[role])
                    {
                        CellData data = pair.Value;
                        target.SetTile(pair.Key, data.tile);
                        target.SetTileFlags(pair.Key, TileFlags.None);
                        target.SetColor(pair.Key, data.color);
                        target.SetTransformMatrix(pair.Key, data.matrix);
                        target.SetColliderType(pair.Key, data.collider);
                        target.SetTileFlags(pair.Key, data.flags);
                    }
                    target.CompressBounds();
                    TilemapCollider2D targetCollider = target.GetComponent<TilemapCollider2D>();
                    if (targetCollider != null) targetCollider.ProcessTilemapChanges();
                }
                DoodleTerrainSkin firstSkin = plans[0].skin;
                if (firstSkin != null)
                {
                    DoodleTerrainSkin sharedSkin = Undo.AddComponent<DoodleTerrainSkin>(level.gameObject);
                    Undo.RecordObject(sharedSkin, "Preserve Shared Skin Style");
                    var data = new SerializedObject(sharedSkin);
                    var source = new SerializedObject(firstSkin);
                    foreach (string field in SkinFields) data.CopyFromSerializedProperty(source.FindProperty(field));
                    data.FindProperty("source").objectReferenceValue = level.Terrain;
                    data.ApplyModifiedProperties();
                    sharedSkin.enabled = plans[0].skinEnabled;
                }
                Transform roomsParent = FindRoomsParent(scene);
                foreach (RoomPlan plan in plans)
                {
                    Undo.RecordObject(plan.root, "Align Room To Shared Grid");
                    plan.root.position += plan.shift;
                    Undo.RecordObject(plan.room, "Connect Shared Room Geometry");
                    plan.room.SetSharedTilemaps(level);
                    if (plan.skin != null) Undo.DestroyObjectImmediate(plan.skin);
                    foreach (Tilemap map in plan.maps)
                    {
                        // Keep every custom decoration/helper child, including its world pose.
                        while (map.transform.childCount > 0)
                            Undo.SetTransformParent(map.transform.GetChild(0), plan.root, "Preserve Room Map Child");
                        Undo.DestroyObjectImmediate(map.gameObject);
                    }
                    if (plan.grid != null && plan.grid.transform.childCount == 0
                        && plan.grid.GetComponents<Component>().Length == 2)
                        Undo.DestroyObjectImmediate(plan.grid.gameObject);
                    Undo.SetTransformParent(plan.root, roomsParent, "Group Ember Rooms");
                }
                if (level.Skin != null) level.Skin.Rebuild();
                Physics2D.SyncTransforms();
                EditorSceneManager.MarkSceneDirty(scene);
                SceneView.RepaintAll();
                Undo.CollapseUndoOperations(group);
                return level;
            }
            catch { Undo.RevertAllDownToGroup(group); throw; }
        }

        private static void ValidateMap(Tilemap map, int role, Grid grid)
        {
            Vector3 origin = map.CellToWorld(Vector3Int.zero);
            Vector3 x = map.CellToWorld(Vector3Int.right) - origin;
            Vector3 y = map.CellToWorld(Vector3Int.up) - origin;
            if (map.layoutGrid != grid || grid.cellLayout != GridLayout.CellLayout.Rectangle
                || grid.cellSwizzle != GridLayout.CellSwizzle.XYZ || grid.cellGap != Vector3.zero
                || !Near(grid.cellSize.x, 1f) || !Near(grid.cellSize.y, 1f)
                || !Finite(origin.x) || !Finite(origin.y) || !Finite(origin.z)
                || x.x <= 0f || !Near(x.y, 0f) || !Near(x.z, 0f) || !Near(y.x, 0f)
                || !Near(y.z, 0f) || !Near(y.y, x.x))
                throw new InvalidOperationException(map.name + ": 회전/비균일/간격 있는 Grid는 자동 병합하지 않습니다.");
            foreach (Component component in map.GetComponents<Component>())
                if (component == null || !(component is Transform || component is Tilemap || component is TilemapRenderer
                    || component is TilemapCollider2D || (role == 0 && (component is Rigidbody2D || component is CompositeCollider2D))
                    || (role == 1 && component is RoomHazard)))
                    throw new InvalidOperationException(map.name + ": 추가 컴포넌트가 있어 자동 병합하지 않습니다.");
            if (map.GetComponent<TilemapRenderer>() == null) throw new InvalidOperationException(map.name + ": 렌더러가 없습니다.");
            TilemapCollider2D collider = map.GetComponent<TilemapCollider2D>();
            if (role == 0)
            {
                Rigidbody2D body = map.GetComponent<Rigidbody2D>();
                CompositeCollider2D composite = map.GetComponent<CompositeCollider2D>();
                if (collider == null || collider.isTrigger || collider.compositeOperation != Collider2D.CompositeOperation.Merge
                    || body == null || body.bodyType != RigidbodyType2D.Static || composite == null
                    || composite.geometryType != CompositeCollider2D.GeometryType.Polygons)
                    throw new InvalidOperationException("Terrain의 Static Rigidbody/Polygon Composite/Merge 충돌을 확인해주세요.");
            }
            if (role == 1 && (collider == null || !collider.isTrigger || map.GetComponent<RoomHazard>() == null
                || collider.compositeOperation != Collider2D.CompositeOperation.None))
                throw new InvalidOperationException("Spikes의 Trigger/RoomHazard를 확인해주세요.");
            if (role == 2 && collider != null) throw new InvalidOperationException("Decorations에 충돌이 있어 자동 병합하지 않습니다.");
        }

        private static void ValidateMatchingMap(Tilemap first, Tilemap map, int role)
        {
            var a = first.GetComponent<TilemapRenderer>();
            var b = map.GetComponent<TilemapRenderer>();
            if (first.color != map.color || first.tileAnchor != map.tileAnchor || first.orientation != map.orientation
                || first.orientationMatrix != map.orientationMatrix || first.animationFrameRate != map.animationFrameRate
                || first.gameObject.layer != map.gameObject.layer || first.gameObject.tag != map.gameObject.tag
                || first.gameObject.activeInHierarchy != map.gameObject.activeInHierarchy
                || a.sortingLayerID != b.sortingLayerID || a.sortingOrder != b.sortingOrder || a.enabled != b.enabled
                || a.mode != b.mode || a.sharedMaterial != b.sharedMaterial)
                throw new InvalidOperationException(Roles[role] + ": 방마다 표시/레이어/정렬/기준점 설정이 달라 자동 병합하지 않습니다.");
            var ca = first.GetComponent<TilemapCollider2D>();
            var cb = map.GetComponent<TilemapCollider2D>();
            if (ca != null && (ca.enabled != cb.enabled || ca.sharedMaterial != cb.sharedMaterial
                || ca.offset != cb.offset || ca.usedByEffector != cb.usedByEffector || ca.extrusionFactor != cb.extrusionFactor
                || ca.includeLayers != cb.includeLayers || ca.excludeLayers != cb.excludeLayers))
                throw new InvalidOperationException(Roles[role] + ": 방마다 물리 설정이 달라 자동 병합하지 않습니다.");
            if (role == 0)
            {
                var bodyA = first.GetComponent<Rigidbody2D>();
                var bodyB = map.GetComponent<Rigidbody2D>();
                if (bodyA.simulated != bodyB.simulated || bodyA.constraints != bodyB.constraints
                    || bodyA.collisionDetectionMode != bodyB.collisionDetectionMode || bodyA.interpolation != bodyB.interpolation
                    || bodyA.useAutoMass != bodyB.useAutoMass || bodyA.mass != bodyB.mass
                    || bodyA.gravityScale != bodyB.gravityScale || bodyA.linearDamping != bodyB.linearDamping
                    || bodyA.angularDamping != bodyB.angularDamping || bodyA.sleepMode != bodyB.sleepMode
                    || bodyA.includeLayers != bodyB.includeLayers || bodyA.excludeLayers != bodyB.excludeLayers)
                    throw new InvalidOperationException("Terrain: 방마다 Rigidbody2D 설정이 달라 자동 병합하지 않습니다.");
                var compositeA = first.GetComponent<CompositeCollider2D>();
                var compositeB = map.GetComponent<CompositeCollider2D>();
                if (compositeA.enabled != compositeB.enabled || compositeA.isTrigger != compositeB.isTrigger
                    || compositeA.generationType != compositeB.generationType || compositeA.geometryType != compositeB.geometryType
                    || compositeA.vertexDistance != compositeB.vertexDistance || compositeA.offsetDistance != compositeB.offsetDistance
                    || compositeA.edgeRadius != compositeB.edgeRadius || compositeA.offset != compositeB.offset
                    || compositeA.sharedMaterial != compositeB.sharedMaterial || compositeA.usedByEffector != compositeB.usedByEffector
                    || compositeA.includeLayers != compositeB.includeLayers || compositeA.excludeLayers != compositeB.excludeLayers)
                    throw new InvalidOperationException("Terrain: 방마다 CompositeCollider2D 설정이 달라 자동 병합하지 않습니다.");
            }
        }

        private static void ValidateMatchingSkin(RoomPlan first, RoomPlan other)
        {
            if ((first.skin == null) != (other.skin == null) || first.skinEnabled != other.skinEnabled)
                throw new InvalidOperationException(other.root.name + ": 방마다 스킨 연결/표시 상태가 다릅니다. 하나의 스타일로 맞춘 후 합쳐주세요.");
            if (first.skin == null) return;
            var a = new SerializedObject(first.skin);
            var b = new SerializedObject(other.skin);
            foreach (string field in SkinFields)
                if (field != "openBottomY" && !SerializedProperty.DataEquals(a.FindProperty(field), b.FindProperty(field)))
                    throw new InvalidOperationException(other.root.name + ": 스킨 " + field + " 설정이 다릅니다. 스타일을 맞춘 후 합쳐주세요.");
        }

        private static void ValidateExternalReferences(Scene scene, List<RoomPlan> plans)
        {
            var deleted = new HashSet<Object>();
            var ignored = new HashSet<Component>();
            foreach (RoomPlan plan in plans)
            {
                if (plan.skin != null) { ignored.Add(plan.skin); deleted.Add(plan.skin); }
                foreach (Tilemap map in plan.maps)
                {
                    deleted.Add(map.gameObject);
                    foreach (Component component in map.GetComponents<Component>()) { deleted.Add(component); ignored.Add(component); }
                }
                bool gridWillBeEmpty = plan.grid.GetComponents<Component>().Length == 2;
                foreach (Transform child in plan.grid.transform)
                {
                    bool isMap = false;
                    foreach (Tilemap map in plan.maps) if (map.transform == child) isMap = true;
                    if (!isMap) gridWillBeEmpty = false;
                }
                if (gridWillBeEmpty)
                {
                    deleted.Add(plan.grid.gameObject);
                    foreach (Component component in plan.grid.GetComponents<Component>()) { deleted.Add(component); ignored.Add(component); }
                }
            }
            foreach (GameObject sceneRoot in scene.GetRootGameObjects())
            foreach (MonoBehaviour behaviour in sceneRoot.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour == null || ignored.Contains(behaviour)) continue;
                var data = new SerializedObject(behaviour);
                SerializedProperty iterator = data.GetIterator();
                while (iterator.Next(true))
                    if (iterator.propertyType == SerializedPropertyType.ObjectReference && deleted.Contains(iterator.objectReferenceValue))
                        throw new InvalidOperationException(behaviour.name + "/" + iterator.propertyPath
                            + ": 기존 타일맵 참조가 있어 자동 병합하지 않습니다. 해당 기믹 연결을 먼저 분리해주세요.");
            }
        }

        private static void CopyPhysics(Tilemap source, Tilemap target)
        {
            foreach (Type type in new[] { typeof(Rigidbody2D), typeof(CompositeCollider2D), typeof(TilemapCollider2D) })
            {
                Component from = source.GetComponent(type);
                Component to = target.GetComponent(type);
                if (from == null || to == null) continue;
                Undo.RecordObject(to, "Preserve Shared Physics Settings");
                EditorUtility.CopySerialized(from, to);
            }
        }

        private static Transform FindRoomsParent(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
                if (root.name == "Rooms" && root.transform.position == Vector3.zero && root.transform.rotation == Quaternion.identity
                    && root.transform.localScale == Vector3.one && root.GetComponents<Component>().Length == 1)
                    return root.transform;
            return NewObject(GameObjectUtility.GetUniqueNameForSibling(null, "Rooms"), null, scene).transform;
        }

        private static GameObject NewObject(string name, Transform parent, Scene scene)
        {
            GameObject result = new GameObject(name);
            SceneManager.MoveGameObjectToScene(result, scene);
            Undo.RegisterCreatedObjectUndo(result, "Create Ember Shared Geometry");
            if (parent != null) Undo.SetTransformParent(result.transform, parent, "Parent Shared Geometry");
            result.transform.localPosition = Vector3.zero;
            result.transform.localRotation = Quaternion.identity;
            result.transform.localScale = Vector3.one;
            return result;
        }

        private static Tilemap NewMap(string name, Transform parent, Scene scene, int order)
        {
            GameObject result = NewObject(name, parent, scene);
            Tilemap map = Undo.AddComponent<Tilemap>(result);
            TilemapRenderer renderer = Undo.AddComponent<TilemapRenderer>(result);
            renderer.sortingOrder = order;
            return map;
        }

        private static void RequireEditableScene(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded || EditorApplication.isPlayingOrWillChangePlaymode
                || PrefabStageUtility.GetCurrentPrefabStage() != null)
                throw new InvalidOperationException("Play/프리팹 모드를 닫고 편집할 씬을 선택해주세요.");
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        private static bool Near(float a, float b) => Mathf.Abs(a - b) <= Tolerance;
    }
}
