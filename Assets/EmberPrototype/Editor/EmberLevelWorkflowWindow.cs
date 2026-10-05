using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace EmberPrototype.Editor
{
    public sealed class EmberLevelWorkflowWindow : EditorWindow
    {
        [SerializeField] private string roomName = "Room_01";
        [SerializeField] private Vector2 roomCenter;
        [SerializeField] private Vector2 roomSize = new Vector2(17.77778f, 10f);
        [SerializeField] private float tileSize = 0.5f;
        [SerializeField] private CameraRoom selectedRoom;
        [SerializeField] private Camera sceneCamera;
        private Vector2 scroll;
        private string[] validationMessages = Array.Empty<string>();
        private string status;

        [MenuItem("Tools/Ember/Level Workflow")]
        public static void OpenWindow()
        {
            GetWindow<EmberLevelWorkflowWindow>("Ember 방 만들기");
        }

        private void OnEnable()
        {
            Selection.selectionChanged += OnSelectionChanged;
            OnSelectionChanged();
        }

        private void OnDisable() => Selection.selectionChanged -= OnSelectionChanged;

        private void OnSelectionChanged()
        {
            CameraRoom room = FindSelectedRoom();
            if (room != null) selectedRoom = room;
            if (sceneCamera == null) sceneCamera = FindSceneCamera(SceneManager.GetActiveScene());
            validationMessages = Array.Empty<string>();
            Repaint();
        }

        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.LabelField("Ember 레벨 제작", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("새 방을 만든 뒤 Terrain에 땅을, Spikes에 위험 타일을 그리세요. 기존 씬을 자동으로 재배치하거나 저장하지 않습니다.", MessageType.Info);
            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("1. 빈 방 만들기", EditorStyles.boldLabel);
                roomName = EditorGUILayout.TextField("방 이름", roomName);
                roomCenter = EditorGUILayout.Vector2Field("방 중심 (월드)", roomCenter);
                roomSize = EditorGUILayout.Vector2Field("카메라 경계 크기 (월드)", roomSize);
                tileSize = EditorGUILayout.FloatField("타일 한 칸 (월드)", tileSize);
                sceneCamera = (Camera)EditorGUILayout.ObjectField("사용할 카메라", sceneCamera, typeof(Camera), true);
                if (GUILayout.Button("카메라 한 화면 크기를 생성값에 사용"))
                {
                    if (sceneCamera != null && sceneCamera.orthographic)
                        roomSize = GetCameraViewSize(sceneCamera);
                    else status = "정사영 카메라를 지정해주세요.";
                }
                bool validInput = IsFinite(roomCenter) && IsPositiveFinite(roomSize) && IsPositiveFinite(tileSize);
                using (new EditorGUI.DisabledScope(!validInput || PrefabStageUtility.GetCurrentPrefabStage() != null))
                {
                    if (GUILayout.Button("새 방 생성 (Undo 가능)"))
                    {
                        GameObject root = CreateRoom(roomName, roomCenter, roomSize, tileSize);
                        selectedRoom = root.GetComponentInChildren<CameraRoom>();
                        Selection.activeGameObject = root;
                        SceneView.lastActiveSceneView?.Frame(selectedRoom.WorldBounds, false);
                        status = root.name + " 생성 완료. 씬 저장은 직접 해주세요.";
                    }
                }
                if (!validInput) EditorGUILayout.HelpBox("중심은 유한한 값, 크기와 타일 한 칸은 0보다 큰 값을 사용해주세요.", MessageType.Warning);
                if (PrefabStageUtility.GetCurrentPrefabStage() != null)
                    EditorGUILayout.HelpBox("프리팹 모드를 닫고 씬에서 새 방을 만드세요.", MessageType.Info);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("2. 타일 그리기", EditorStyles.boldLabel);
                if (GUILayout.Button("Tile Palette 열기"))
                    status = EditorApplication.ExecuteMenuItem("Window/2D/Tile Palette") ? null : "Window > 2D > Tile Palette에서 열어주세요.";
                EditorGUILayout.HelpBox("Hierarchy에서 Terrain / Spikes / Decorations를 선택하고 Tile Palette의 Active Target을 확인하세요. Scene 창의 Gizmos를 켜면 방 경계와 Spawn을 볼 수 있습니다.", MessageType.None);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("3. 선택한 방 확인", EditorStyles.boldLabel);
                selectedRoom = (CameraRoom)EditorGUILayout.ObjectField("카메라 경계", selectedRoom, typeof(CameraRoom), true);
                using (new EditorGUI.DisabledScope(selectedRoom == null))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("경계 선택")) Selection.activeGameObject = selectedRoom.gameObject;
                        if (GUILayout.Button("방 화면으로 보기")) SceneView.lastActiveSceneView?.Frame(selectedRoom.WorldBounds, false);
                    }
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("카메라 한 화면 크기로 맞추기"))
                            status = FitRoomToCamera(selectedRoom, sceneCamera) ? "방 중심을 유지하고 경계 크기를 맞췄습니다." : "같은 씬의 정사영 카메라를 지정해주세요.";
                        if (GUILayout.Button("타일맵 범위로 맞추기"))
                            status = FitRoomToTilemaps(selectedRoom) ? "방의 그려진 타일맵 범위에 맞췄습니다." : "방 아래에 그려진 타일맵이 없습니다. 경계 Inspector에서 타일맵을 지정할 수 있습니다.";
                    }
                    if (GUILayout.Button("카메라를 이 방에 연결"))
                        status = ConnectCameraToRoom(selectedRoom, sceneCamera) ? "카메라 시작 방을 연결했습니다." : "같은 씬의 카메라를 지정해주세요.";
                    if (GUILayout.Button("방 구성 검사")) validationMessages = ValidateRoom(selectedRoom);
                }
                if (GUILayout.Button("사용할 카메라 선택") && sceneCamera != null) Selection.activeGameObject = sceneCamera.gameObject;
            }
            if (!string.IsNullOrEmpty(status)) EditorGUILayout.HelpBox(status, MessageType.Info);
            foreach (string message in validationMessages)
                EditorGUILayout.HelpBox(message, message.StartsWith("구성 검사 통과", StringComparison.Ordinal) ? MessageType.Info : MessageType.Warning);
            EditorGUILayout.EndScrollView();
        }

        public static GameObject CreateRoom(string roomName, Vector2 center, Vector2 size, float tileSize = 0.5f)
        {
            if (!IsFinite(center) || !IsPositiveFinite(size) || !IsPositiveFinite(tileSize))
                throw new ArgumentException("Room center must be finite, and size / tileSize must be positive and finite.");
            if (EditorApplication.isPlayingOrWillChangePlaymode || PrefabStageUtility.GetCurrentPrefabStage() != null)
                throw new InvalidOperationException("Create a room in an editable scene outside Play Mode and Prefab Mode.");
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded) throw new InvalidOperationException("No editable scene is loaded.");

            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Create Ember Room");
            try
            {
                string name = string.IsNullOrWhiteSpace(roomName) ? "Room_01" : roomName.Trim();
                GameObject root = CreateObject(GameObjectUtility.GetUniqueNameForSibling(null, name), null, scene);
                Undo.RecordObject(root.transform, "Position Ember Room");
                root.transform.position = new Vector3(center.x, center.y, 0f);
                GameObject boundsObject = CreateObject("CameraBounds", root.transform, scene);
                CameraRoom room = Undo.AddComponent<CameraRoom>(boundsObject);
                Undo.RecordObjects(new UnityEngine.Object[] { room, room.transform }, "Set Room Bounds");
                room.SetWorldBounds(new Bounds(center, new Vector3(size.x, size.y, 0f)));

                GameObject gridObject = CreateObject("Grid", root.transform, scene);
                Undo.RecordObject(gridObject.transform, "Set Tile Size");
                gridObject.transform.localScale = new Vector3(tileSize, tileSize, 1f);
                Grid grid = Undo.AddComponent<Grid>(gridObject);
                Undo.RecordObject(grid, "Configure Room Grid");
                grid.cellSize = new Vector3(1f, 1f, 0f);
                grid.cellGap = Vector3.zero;
                grid.cellLayout = GridLayout.CellLayout.Rectangle;

                GameObject terrain = CreateTilemap("Terrain", gridObject.transform, scene, 0);
                Rigidbody2D body = Undo.AddComponent<Rigidbody2D>(terrain);
                Undo.RecordObject(body, "Configure Terrain Body");
                body.bodyType = RigidbodyType2D.Static;
                CompositeCollider2D composite = Undo.AddComponent<CompositeCollider2D>(terrain);
                Undo.RecordObject(composite, "Configure Terrain Composite");
                composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
                composite.generationType = CompositeCollider2D.GenerationType.Synchronous;
                composite.vertexDistance = 0.0005f;
                composite.offsetDistance = 0.00005f;
                TilemapCollider2D terrainCollider = Undo.AddComponent<TilemapCollider2D>(terrain);
                Undo.RecordObject(terrainCollider, "Merge Terrain Tiles");
                terrainCollider.compositeOperation = Collider2D.CompositeOperation.Merge;
                terrainCollider.extrusionFactor = 0f;

                GameObject spikes = CreateTilemap("Spikes", gridObject.transform, scene, 1);
                TilemapCollider2D spikesCollider = Undo.AddComponent<TilemapCollider2D>(spikes);
                Undo.RecordObject(spikesCollider, "Configure Spike Trigger");
                spikesCollider.isTrigger = true;
                spikesCollider.compositeOperation = Collider2D.CompositeOperation.None;
                Undo.AddComponent<RoomHazard>(spikes);
                CreateTilemap("Decorations", gridObject.transform, scene, -1);
                CreateObject("Mechanics", root.transform, scene);
                CreateObject("Paths", root.transform, scene);
                GameObject spawn = CreateObject("Spawn", root.transform, scene);
                Undo.RecordObject(spawn.transform, "Position Room Spawn");
                spawn.transform.localPosition = new Vector3(-size.x * 0.5f + Mathf.Min(tileSize * 2f, size.x * 0.25f),
                    -size.y * 0.5f + Mathf.Min(tileSize * 2f, size.y * 0.25f), 0f);
                Undo.AddComponent<RoomSpawnPoint>(spawn);
                EditorSceneManager.MarkSceneDirty(scene);
                Undo.CollapseUndoOperations(group);
                return root;
            }
            catch
            {
                Undo.RevertAllDownToGroup(group);
                throw;
            }
        }

        public static bool FitRoomToCamera(CameraRoom room, Camera camera)
        {
            if (!CanEdit(room) || camera == null || camera.gameObject.scene != room.gameObject.scene || !camera.orthographic)
                return false;
            Vector2 size = GetCameraViewSize(camera);
            if (!IsPositiveFinite(size)) return false;
            ApplyWorldBounds(room, new Bounds(room.WorldBounds.center, new Vector3(size.x, size.y, 0f)), "Fit Room To Camera");
            return true;
        }

        public static bool FitRoomToTilemaps(CameraRoom room) => FitRoomToTilemaps(room, FindRoomTilemaps(room));

        public static bool FitRoomToTilemaps(CameraRoom room, Tilemap[] tilemaps)
        {
            if (!CanEdit(room) || tilemaps == null) return false;
            bool found = false;
            Bounds total = default;
            foreach (Tilemap map in tilemaps)
            {
                if (map == null || map.gameObject.scene != room.gameObject.scene || map.GetUsedTilesCount() == 0) continue;
                // Fit painted grid cells, independent of sprite bounds and renderer refresh timing.
                foreach (Vector3Int cell in map.cellBounds.allPositionsWithin)
                {
                    if (!map.HasTile(cell)) continue;
                    for (int x = 0; x < 2; x++)
                    for (int y = 0; y < 2; y++)
                    {
                        Vector3 point = map.CellToWorld(cell + new Vector3Int(x, y, 0));
                        point.z = 0f;
                        if (!found) { total = new Bounds(point, Vector3.zero); found = true; }
                        else total.Encapsulate(point);
                    }
                }
            }
            if (!found || total.size.x <= 0f || total.size.y <= 0f) return false;
            ApplyWorldBounds(room, total, "Fit Room To Tilemaps");
            return true;
        }

        public static bool ConnectCameraToRoom(CameraRoom room, Camera camera)
        {
            if (!CanEdit(room) || camera == null || camera.gameObject.scene != room.gameObject.scene) return false;
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Connect Ember Room Camera");
            Undo.RecordObject(camera, "Configure Room Camera");
            camera.orthographic = true;
            CelesteRoomCamera controller = camera.GetComponent<CelesteRoomCamera>();
            if (controller == null) controller = Undo.AddComponent<CelesteRoomCamera>(camera.gameObject);
            Undo.RecordObject(controller, "Connect Room Camera");
            controller.RefreshRooms();
            controller.SetStartingRoom(room, false);
            var controllerProperties = new SerializedObject(controller);
            if (controllerProperties.FindProperty("target").objectReferenceValue == null)
            {
                foreach (GameObject root in room.gameObject.scene.GetRootGameObjects())
                {
                    FlamePlayerController player = root.GetComponentInChildren<FlamePlayerController>();
                    if (player == null) continue;
                    controller.SetTarget(player.transform, false);
                    break;
                }
            }
            RecordPrefabChanges(camera);
            RecordPrefabChanges(controller);
            EditorSceneManager.MarkSceneDirty(room.gameObject.scene);
            Undo.CollapseUndoOperations(group);
            return true;
        }

        public static string[] ValidateRoom(CameraRoom room)
        {
            var messages = new List<string>();
            if (room == null) return new[] { "검사할 CameraRoom을 선택해주세요." };
            Transform root = GetRoomRoot(room);
            Tilemap[] maps = FindRoomTilemaps(room);
            if (maps.Length == 0) messages.Add("타일맵이 없습니다. 새 방의 Grid 아래 Terrain / Spikes / Decorations를 확인하세요.");
            foreach (Tilemap map in maps)
            {
                TilemapCollider2D collider = map.GetComponent<TilemapCollider2D>();
                CompositeCollider2D composite = map.GetComponent<CompositeCollider2D>();
                Rigidbody2D body = map.GetComponent<Rigidbody2D>();
                if (collider != null && collider.compositeOperation == Collider2D.CompositeOperation.Merge &&
                    (composite == null || !composite.enabled || body == null || body.bodyType != RigidbodyType2D.Static))
                    messages.Add(map.name + ": Merge에는 같은 오브젝트의 활성 Composite와 Static Rigidbody2D가 필요합니다.");
                if (map.name == "Terrain" && (collider == null || !collider.enabled || collider.isTrigger ||
                    collider.compositeOperation != Collider2D.CompositeOperation.Merge || composite == null || !composite.enabled ||
                    composite.geometryType != CompositeCollider2D.GeometryType.Polygons ||
                    composite.generationType != CompositeCollider2D.GenerationType.Synchronous ||
                    body == null || body.bodyType != RigidbodyType2D.Static || !body.simulated))
                    messages.Add("Terrain: 바닥 충돌용 TilemapCollider와 Composite를 확인하세요.");
                if (map.name == "Spikes" && (collider == null || !collider.enabled || !collider.isTrigger ||
                    collider.compositeOperation != Collider2D.CompositeOperation.None || map.GetComponent<RoomHazard>() == null))
                    messages.Add("Spikes: Trigger TilemapCollider와 RoomHazard를 확인하세요.");
                if (map.name == "Decorations" && map.GetComponent<Collider2D>() != null)
                    messages.Add("Decorations: 장식 타일맵에는 콜라이더를 두지 않는 편이 좋습니다.");
            }
            RoomSpawnPoint[] spawns = root.GetComponentsInChildren<RoomSpawnPoint>(true);
            if (spawns.Length == 0) messages.Add("Spawn이 없습니다. 방에 RoomSpawnPoint를 배치하세요.");
            foreach (RoomSpawnPoint spawn in spawns)
                if (!room.Contains(spawn.transform.position)) messages.Add(spawn.name + ": Spawn이 카메라 경계 밖에 있습니다.");
            Camera camera = FindSceneCamera(room.gameObject.scene);
            if (camera == null) messages.Add("씬에 카메라가 없습니다.");
            else
            {
                if (!camera.orthographic) messages.Add("2D 방 카메라는 Orthographic으로 설정하세요.");
                if (camera.GetComponent<CelesteRoomCamera>() == null) messages.Add("카메라에 CelesteRoomCamera가 없습니다. 연결 버튼을 사용하세요.");
                Vector2 view = GetCameraViewSize(camera);
                if (camera.orthographic && (room.WorldBounds.size.x < view.x || room.WorldBounds.size.y < view.y))
                    messages.Add("경계가 카메라 한 화면보다 작습니다. 해당 축의 카메라는 방 중앙에 고정됩니다.");
            }
            if (SceneView.lastActiveSceneView != null && !SceneView.lastActiveSceneView.drawGizmos)
                messages.Add("Scene 창의 Gizmos가 꺼져 있습니다. 경계와 Spawn 표시를 보려면 켜주세요.");
            if (messages.Count == 0) messages.Add("구성 검사 통과. 타일을 그린 뒤 평지 이동, 착지, 위험 타일, 방 전환을 플레이 모드에서 확인하세요.");
            return messages.ToArray();
        }

        internal static void ApplyWorldBounds(CameraRoom room, Bounds bounds, string undoName)
        {
            Undo.RecordObjects(new UnityEngine.Object[] { room, room.transform }, undoName);
            room.SetWorldBounds(bounds);
            RecordPrefabChanges(room);
            RecordPrefabChanges(room.transform);
            EditorSceneManager.MarkSceneDirty(room.gameObject.scene);
            SceneView.RepaintAll();
        }

        internal static Camera FindSceneCamera(Scene scene)
        {
            Camera fallback = null;
            if (!scene.IsValid() || !scene.isLoaded) return null;
            foreach (GameObject root in scene.GetRootGameObjects())
            foreach (Camera camera in root.GetComponentsInChildren<Camera>(true))
            {
                if (camera.CompareTag("MainCamera")) return camera;
                if (fallback == null) fallback = camera;
            }
            return fallback;
        }

        private static GameObject CreateObject(string name, Transform parent, Scene scene)
        {
            GameObject obj = new GameObject(name);
            SceneManager.MoveGameObjectToScene(obj, scene);
            Undo.RegisterCreatedObjectUndo(obj, "Create Ember Room Object");
            if (parent != null) Undo.SetTransformParent(obj.transform, parent, "Parent Ember Room Object");
            Undo.RecordObject(obj.transform, "Reset Room Object Transform");
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;
            return obj;
        }

        private static GameObject CreateTilemap(string name, Transform parent, Scene scene, int sortingOrder)
        {
            GameObject obj = CreateObject(name, parent, scene);
            Undo.AddComponent<Tilemap>(obj);
            TilemapRenderer renderer = Undo.AddComponent<TilemapRenderer>(obj);
            Undo.RecordObject(renderer, "Set Tilemap Sorting");
            renderer.sortingOrder = sortingOrder;
            return obj;
        }

        private static Transform GetRoomRoot(CameraRoom room) => room.transform.parent != null ? room.transform.parent : room.transform;

        private static Tilemap[] FindRoomTilemaps(CameraRoom room)
        {
            if (room == null) return Array.Empty<Tilemap>();
            Tilemap[] maps = GetRoomRoot(room).GetComponentsInChildren<Tilemap>(true);
            if (maps.Length > 0) return maps;
            var sceneMaps = new List<Tilemap>();
            int rooms = 0;
            foreach (GameObject root in room.gameObject.scene.GetRootGameObjects())
            {
                rooms += root.GetComponentsInChildren<CameraRoom>(true).Length;
                sceneMaps.AddRange(root.GetComponentsInChildren<Tilemap>(true));
            }
            // The original test scene has its sole CameraRoom and Grid as siblings.
            return rooms == 1 ? sceneMaps.ToArray() : Array.Empty<Tilemap>();
        }

        private static CameraRoom FindSelectedRoom()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null || EditorUtility.IsPersistent(selected)) return null;
            CameraRoom room = selected.GetComponentInParent<CameraRoom>();
            if (room != null) return room;
            for (Transform cursor = selected.transform; cursor != null; cursor = cursor.parent)
            {
                CameraRoom[] rooms = cursor.GetComponentsInChildren<CameraRoom>(true);
                if (rooms.Length == 1) return rooms[0];
                if (rooms.Length > 1) break;
            }
            return null;
        }

        private static bool CanEdit(CameraRoom room) => room != null && !EditorUtility.IsPersistent(room) &&
            room.gameObject.scene.IsValid() && !EditorApplication.isPlayingOrWillChangePlaymode;
        private static Vector2 GetCameraViewSize(Camera camera) => new Vector2(camera.orthographicSize * 2f * camera.aspect, camera.orthographicSize * 2f);
        private static bool IsFinite(Vector2 value) => !float.IsNaN(value.x) && !float.IsInfinity(value.x) && !float.IsNaN(value.y) && !float.IsInfinity(value.y);
        private static bool IsPositiveFinite(Vector2 value) => IsFinite(value) && value.x > 0f && value.y > 0f;
        private static bool IsPositiveFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value) && value > 0f;
        private static void RecordPrefabChanges(UnityEngine.Object obj)
        {
            EditorUtility.SetDirty(obj);
            if (PrefabUtility.IsPartOfPrefabInstance(obj)) PrefabUtility.RecordPrefabInstancePropertyModifications(obj);
        }
    }
}
