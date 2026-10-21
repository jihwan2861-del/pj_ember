using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace EmberPrototype.Editor
{
    public sealed class EmberLevelWorkflowWindow : EditorWindow
    {
        private const string SkinExamplePath = "Assets/Ember/Prefabs/LevelDesign/SkinScene/SkinRoom_Example.prefab";
        private const string DoodlePalettePath = "Assets/Ember/LevelDesign/SkinScene/Palettes/Ember Doodle.prefab";
        private const string GrayboxPalettePath = "Assets/Ember/LevelDesign/Graybox/Ember Graybox.prefab";
        [SerializeField] private string roomName = "Room_01";
        [SerializeField] private Vector2 roomCenter;
        [SerializeField] private Vector2 roomSize = new Vector2(18f, 10.125f);
        [SerializeField] private float tileSize = 0.5f;
        [SerializeField] private bool createWithSkin = true;
        [SerializeField] private bool useSharedTilemaps = true;
        [SerializeField] private LevelTilemaps sharedTilemaps;
        [SerializeField] private bool alignRoomsOnMerge;
        [SerializeField] private DoodleTerrainSkin skinStyle;
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
            if (skinStyle == null) skinStyle = GetDefaultSkinStyle();
            Selection.selectionChanged += OnSelectionChanged;
            OnSelectionChanged();
        }

        private void OnDisable() => Selection.selectionChanged -= OnSelectionChanged;

        private void OnSelectionChanged()
        {
            CameraRoom room = FindSelectedRoom();
            if (room != null)
            {
                selectedRoom = room;
                if (room.SharedTilemaps != null) sharedTilemaps = room.SharedTilemaps;
            }
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
                useSharedTilemaps = EditorGUILayout.Toggle("공용 Grid에 방 추가", useSharedTilemaps);
                if (useSharedTilemaps)
                {
                    sharedTilemaps = (LevelTilemaps)EditorGUILayout.ObjectField("공용 지형 (선택)", sharedTilemaps, typeof(LevelTilemaps), true);
                    EditorGUILayout.HelpBox("Terrain 하나에서 모든 방을 이어 그립니다. 방에는 CameraBounds와 Spawn만 생기며, 방 경계를 옮겨도 공용 지형은 이동하지 않습니다.", MessageType.None);
                }
                createWithSkin = EditorGUILayout.Toggle("낙서 스킨룸으로 생성", createWithSkin);
                skinStyle = (DoodleTerrainSkin)EditorGUILayout.ObjectField("새 스킨의 스타일", skinStyle, typeof(DoodleTerrainSkin), false);
                roomName = EditorGUILayout.TextField("방 이름", roomName);
                roomCenter = EditorGUILayout.Vector2Field("방 중심 (월드)", roomCenter);
                roomSize = EditorGUILayout.Vector2Field("카메라 경계 크기 (월드)", roomSize);
                tileSize = EditorGUILayout.FloatField("타일 한 칸 (월드)", tileSize);
                if (IsPositiveFinite(roomSize) && IsPositiveFinite(tileSize))
                    EditorGUILayout.LabelField("화면 타일 수", $"{roomSize.x / tileSize:0.##} × {roomSize.y / tileSize:0.##}칸");
                if (GUILayout.Button("스킨룸 기본 규격 (36 × 20.25칸)"))
                {
                    roomSize = new Vector2(18f, 10.125f);
                    tileSize = 0.5f;
                }
                sceneCamera = (Camera)EditorGUILayout.ObjectField("사용할 카메라", sceneCamera, typeof(Camera), true);
                if (GUILayout.Button("카메라 한 화면 크기를 생성값에 사용"))
                {
                    if (sceneCamera != null && sceneCamera.orthographic)
                        roomSize = GetCameraViewSize(sceneCamera);
                    else status = "정사영 카메라를 지정해주세요.";
                }
                bool validInput = IsFinite(roomCenter) && IsPositiveFinite(roomSize) && IsPositiveFinite(tileSize);
                using (new EditorGUI.DisabledScope(!validInput || (createWithSkin && !HasSkinStyle(skinStyle))
                    || PrefabStageUtility.GetCurrentPrefabStage() != null))
                {
                    if (GUILayout.Button("새 방 생성 (Undo 가능)"))
                    {
                        try
                        {
                            GameObject root = useSharedTilemaps
                                ? CreateSharedRoom(roomName, roomCenter, roomSize, tileSize, sharedTilemaps, createWithSkin ? skinStyle : null)
                                : createWithSkin ? CreateSkinRoom(roomName, roomCenter, roomSize, tileSize, skinStyle)
                                : CreateRoom(roomName, roomCenter, roomSize, tileSize);
                            selectedRoom = root.GetComponentInChildren<CameraRoom>();
                            sharedTilemaps = selectedRoom.SharedTilemaps;
                            Selection.activeGameObject = root;
                            SceneView.lastActiveSceneView?.Frame(selectedRoom.WorldBounds, false);
                            status = root.name + " 생성 완료. 씬 저장은 직접 해주세요.";
                        }
                        catch (Exception exception) { status = exception.Message; }
                    }
                }
                if (!validInput) EditorGUILayout.HelpBox("중심은 유한한 값, 크기와 타일 한 칸은 0보다 큰 값을 사용해주세요.", MessageType.Warning);
                if (createWithSkin && !HasSkinStyle(skinStyle))
                    EditorGUILayout.HelpBox("Fill Material이 연결된 스킨 스타일 프리팹을 지정해주세요.", MessageType.Warning);
                if (PrefabStageUtility.GetCurrentPrefabStage() != null)
                    EditorGUILayout.HelpBox("프리팹 모드를 닫고 씬에서 새 방을 만드세요.", MessageType.Info);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("2. 타일 그리기", EditorStyles.boldLabel);
                if (GUILayout.Button("Tile Palette 열기"))
                    status = EditorApplication.ExecuteMenuItem("Window/2D/Tile Palette") ? null : "Window > 2D > Tile Palette에서 열어주세요.";
                using (new EditorGUI.DisabledScope(!CanEdit(selectedRoom)))
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Terrain 선택 + 팔레트")) status = SelectPaintingTarget(selectedRoom, "Terrain");
                    if (GUILayout.Button("Spikes 선택 + 팔레트")) status = SelectPaintingTarget(selectedRoom, "Spikes");
                }
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
                        using (new EditorGUI.DisabledScope(selectedRoom != null && selectedRoom.SharedTilemaps != null))
                            if (GUILayout.Button("타일맵 범위로 맞추기"))
                                status = FitRoomToTilemaps(selectedRoom) ? "방의 그려진 타일맵 범위에 맞췄습니다." : "방 아래에 그려진 타일맵이 없습니다.";
                    }
                    if (GUILayout.Button("카메라를 이 방에 연결"))
                        status = ConnectCameraToRoom(selectedRoom, sceneCamera) ? "카메라 시작 방을 연결했습니다." : "같은 씬의 카메라를 지정해주세요.";
                    if (GUILayout.Button("이 방의 Spawn 자동 저장 연결"))
                        status = ConnectRoomSpawn(selectedRoom) ? "방 진입 시 Spawn을 저장합니다." : "방 아래 이름이 Spawn인 RoomSpawnPoint를 하나 배치해주세요.";
                    if (GUILayout.Button("방 구성 검사")) validationMessages = ValidateRoom(selectedRoom);
                }
                DrawRoomSkin();
                DrawSharedLayout();
                if (GUILayout.Button("사용할 카메라 선택") && sceneCamera != null) Selection.activeGameObject = sceneCamera.gameObject;
            }
            if (!string.IsNullOrEmpty(status)) EditorGUILayout.HelpBox(status, MessageType.Info);
            foreach (string message in validationMessages)
                EditorGUILayout.HelpBox(message, message.StartsWith("구성 검사 통과", StringComparison.Ordinal) ? MessageType.Info : MessageType.Warning);
            EditorGUILayout.EndScrollView();
        }

        private void DrawRoomSkin()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("4. 선택한 방의 스킨", EditorStyles.boldLabel);
            bool editable = CanEdit(selectedRoom) && PrefabStageUtility.GetCurrentPrefabStage() == null;
            DoodleTerrainSkin skin = FindRoomSkin(selectedRoom);
            using (new EditorGUI.DisabledScope(!editable))
            {
                if (GUILayout.Button("선택 방에 스킨 연결 / 복구"))
                {
                    try
                    {
                        skin = ApplyDoodleSkin(selectedRoom, skinStyle);
                        status = "Terrain에 스킨을 연결했습니다. 타일을 수정하면 돌·해칭·풀·외곽선이 따라갑니다.";
                    }
                    catch (Exception exception) { status = exception.Message; }
                }
                using (new EditorGUI.DisabledScope(skin == null))
                {
                    bool visible = skin != null && skin.enabled;
                    bool nextVisible = EditorGUILayout.Toggle("스킨 표시", visible);
                    if (skin != null && visible != nextVisible)
                    {
                        Undo.RecordObject(skin, "Toggle Room Skin");
                        skin.enabled = nextVisible;
                        RecordPrefabChanges(skin);
                        EditorSceneManager.MarkSceneDirty(skin.gameObject.scene);
                        SceneView.RepaintAll();
                    }
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("스킨 설정 선택")) Selection.activeGameObject = skin.gameObject;
                        if (GUILayout.Button("스킨 다시 만들기")) skin.Rebuild();
                    }
                }
            }
            EditorGUILayout.HelpBox("Terrain은 충돌용 원본입니다. 스킨 표시를 끄면 그레이박스로 편집할 수 있습니다. 스킨 연결은 기존 방의 스타일 설정을 유지합니다.", MessageType.None);
        }

        private void DrawSharedLayout()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("5. 공용 Grid로 정리", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("카메라 방과 Spawn은 방별로 유지하고 Terrain / Spikes / Decorations는 하나로 합칩니다. 기존 씬의 Center·Finish 체크포인트는 삭제하지 않습니다. 격자 맞춤을 켜면 방의 지형과 기믹이 함께 가장 가까운 칸으로 이동합니다.", MessageType.None);
            alignRoomsOnMerge = EditorGUILayout.Toggle("어긋난 방을 격자에 맞춤", alignRoomsOnMerge);
            if (GUILayout.Button("현재 씬의 방 타일맵 통합 (Undo 가능)"))
            {
                try
                {
                    Scene scene = SceneManager.GetActiveScene();
                    CameraRoom[] rooms = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<CameraRoom>(true)).ToArray();
                    sharedTilemaps = EmberSharedTilemapUtility.MergeRooms(rooms, alignRoomsOnMerge);
                    foreach (CameraRoom room in rooms) ConnectRoomSpawn(room);
                    useSharedTilemaps = true;
                    status = "공용 Grid로 통합했습니다. Terrain 하나에서 이어 그리세요. 씬 저장은 직접 해주세요.";
                }
                catch (Exception exception) { status = exception.Message; }
            }
            if (selectedRoom != null && selectedRoom.SharedTilemaps != null)
            {
                EditorGUILayout.ObjectField("이 방의 공용 지형", selectedRoom.SharedTilemaps, typeof(LevelTilemaps), true);
                EditorGUILayout.HelpBox("공용 맵 전체로 카메라 경계를 늘리지 않도록 타일맵 범위 맞추기는 꺼집니다. 방 경계는 카메라 크기 또는 Scene 핸들로 정하세요.", MessageType.None);
            }
        }

        private static DoodleTerrainSkin GetDefaultSkinStyle()
        {
            GameObject example = AssetDatabase.LoadAssetAtPath<GameObject>(SkinExamplePath);
            return example != null ? example.GetComponent<DoodleTerrainSkin>() : null;
        }

        private static bool HasSkinStyle(DoodleTerrainSkin style) => style != null
            && new SerializedObject(style).FindProperty("fillMaterial").objectReferenceValue != null;

        private static DoodleTerrainSkin FindRoomSkin(CameraRoom room)
        {
            if (room == null) return null;
            if (room.SharedTilemaps != null) return room.SharedTilemaps.Skin;
            DoodleTerrainSkin[] skins = GetRoomRoot(room).GetComponentsInChildren<DoodleTerrainSkin>(true);
            return skins.Length == 1 ? skins[0] : null;
        }

        private static Tilemap FindRoomTerrain(CameraRoom room)
        {
            if (room == null) return null;
            if (room.SharedTilemaps != null)
                return room.SharedTilemaps.gameObject.scene == room.gameObject.scene ? room.SharedTilemaps.Terrain : null;
            Transform root = GetRoomRoot(room);
            // Refuse a parent collection containing several rooms rather than modifying an arbitrary Terrain.
            if (root.GetComponentsInChildren<CameraRoom>(true).Length != 1) return null;
            Tilemap found = null;
            foreach (Tilemap map in root.GetComponentsInChildren<Tilemap>(true))
            {
                if (map.name != "Terrain") continue;
                if (found != null) return null;
                found = map;
            }
            if (found != null) return found;
            // A renamed source is usable only when it is a solid terrain map, never the hazard/art map.
            DoodleTerrainSkin skin = FindRoomSkin(room);
            if (skin == null) return null;
            var source = new SerializedObject(skin).FindProperty("source").objectReferenceValue as Tilemap;
            TilemapCollider2D collider = source != null ? source.GetComponent<TilemapCollider2D>() : null;
            return source != null && source.transform.IsChildOf(root) && source.gameObject.scene == room.gameObject.scene
                && collider != null && !collider.isTrigger && source.GetComponent<RoomHazard>() == null ? source : null;
        }

        public static DoodleTerrainSkin ApplyDoodleSkin(CameraRoom room, DoodleTerrainSkin style = null)
        {
            if (!CanEdit(room) || PrefabStageUtility.GetCurrentPrefabStage() != null)
                throw new InvalidOperationException("Play와 프리팹 모드를 닫고 씬의 CameraRoom을 선택해주세요.");
            Tilemap terrain = FindRoomTerrain(room);
            if (terrain == null) throw new InvalidOperationException("선택 방에서 하나의 Terrain을 찾을 수 없습니다. 방 아래 Grid/Terrain 연결을 확인해주세요.");
            DoodleTerrainSkin skin = FindRoomSkin(room);
            Transform skinOwner = room.SharedTilemaps != null ? room.SharedTilemaps.transform : GetRoomRoot(room);
            if (skinOwner.GetComponentsInChildren<DoodleTerrainSkin>(true).Length > 1)
                throw new InvalidOperationException("방에 스킨이 여러 개 있습니다. Terrain 하나당 사용할 스킨을 먼저 정리해주세요.");
            if (style == null) style = GetDefaultSkinStyle();
            if ((skin == null || !HasSkinStyle(skin)) && !HasSkinStyle(style))
                throw new InvalidOperationException("Fill Material이 연결된 스킨 스타일 프리팹을 지정해주세요.");

            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Connect Ember Room Skin");
            try
            {
                bool added = skin == null;
                if (added) skin = Undo.AddComponent<DoodleTerrainSkin>(skinOwner.gameObject);
                Undo.RecordObject(skin, "Configure Room Skin");
                var data = new SerializedObject(skin);
                if (added)
                {
                    var preset = new SerializedObject(style);
                    foreach (string propertyName in new[] { "fillMaterial", "outlineMaterial", "grassSprite", "textureWorldSize",
                        "cornerRadius", "outlineWidth", "grassPatchWidth", "grassHeight", "openBottomInfill", "openBottomY" })
                        data.CopyFromSerializedProperty(preset.FindProperty(propertyName));
                    CameraRoom presetRoom = style.GetComponentInChildren<CameraRoom>(true);
                    float offset = presetRoom != null
                        ? preset.FindProperty("openBottomY").floatValue - presetRoom.WorldBounds.min.y : 0f;
                    data.FindProperty("openBottomY").floatValue = room.WorldBounds.min.y + offset;
                }
                else if (data.FindProperty("fillMaterial").objectReferenceValue == null && HasSkinStyle(style))
                    data.FindProperty("fillMaterial").objectReferenceValue = new SerializedObject(style).FindProperty("fillMaterial").objectReferenceValue;
                data.FindProperty("source").objectReferenceValue = terrain;
                data.ApplyModifiedProperties();
                skin.enabled = true;
                RecordPrefabChanges(skin);
                TilemapRenderer terrainRenderer = terrain.GetComponent<TilemapRenderer>();
                if (terrainRenderer != null)
                    foreach (Tilemap map in FindRoomTilemaps(room))
                    {
                        TilemapRenderer renderer = map.GetComponent<TilemapRenderer>();
                        if (map.name != "Spikes" || renderer == null || renderer.sortingLayerID != terrainRenderer.sortingLayerID
                            || renderer.sortingOrder >= terrainRenderer.sortingOrder + 5) continue;
                        Undo.RecordObject(renderer, "Sort Spikes Above Skin");
                        renderer.sortingOrder = terrainRenderer.sortingOrder + 5;
                        RecordPrefabChanges(renderer);
                    }
                skin.Rebuild();
                EditorSceneManager.MarkSceneDirty(room.gameObject.scene);
                Undo.CollapseUndoOperations(group);
                return skin;
            }
            catch { Undo.RevertAllDownToGroup(group); throw; }
        }

        public static string SelectPaintingTarget(CameraRoom room, string tilemapName)
        {
            if (!CanEdit(room) || PrefabStageUtility.GetCurrentPrefabStage() != null) return "씬에서 편집할 방을 선택해주세요.";
            if (room.SharedTilemaps == null && GetRoomRoot(room).GetComponentsInChildren<CameraRoom>(true).Length != 1)
                return "여러 방이 묶인 부모 대신 편집할 방의 CameraBounds를 선택해주세요.";
            Tilemap map = tilemapName == "Terrain" ? FindRoomTerrain(room) : null;
            if (tilemapName != "Terrain")
            {
                foreach (Tilemap candidate in FindRoomTilemaps(room))
                    if (candidate.name == tilemapName)
                    {
                        if (map != null) return tilemapName + "이 여러 개입니다. Hierarchy에서 직접 선택해주세요.";
                        map = candidate;
                    }
            }
            if (map == null) return "선택 방에 편집할 " + tilemapName + "이 없습니다.";
            EditorApplication.ExecuteMenuItem("Window/2D/Tile Palette");
            string path = FindRoomSkin(room) != null ? DoodlePalettePath : GrayboxPalettePath;
            GameObject palette = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Selection.activeGameObject = map.gameObject;
            bool foundPalette = palette != null && GridPaintingState.palettes.Contains(palette);
            if (foundPalette) GridPaintingState.palette = palette;
            GridPaintingState.scenePaintTarget = map.gameObject;
            return map.name + "을 Active Target으로 선택했습니다. "
                + (foundPalette ? palette.name + " 팔레트를 사용합니다." : "Tile Palette에서 사용할 팔레트를 선택해주세요.");
        }

        public static GameObject CreateRoom(string roomName, Vector2 center, Vector2 size, float tileSize = 0.5f)
            => CreateRoomInternal(roomName, center, size, tileSize, null);

        public static GameObject CreateSkinRoom(string roomName, Vector2 center, Vector2 size, float tileSize = 0.5f,
            DoodleTerrainSkin style = null)
        {
            if (style == null) style = GetDefaultSkinStyle();
            if (!HasSkinStyle(style)) throw new InvalidOperationException("스킨 스타일의 Fill Material을 확인해주세요.");
            return CreateRoomInternal(roomName, center, size, tileSize, style);
        }

        public static GameObject CreateSharedRoom(string roomName, Vector2 center, Vector2 size, float tileSize = 0.5f,
            LevelTilemaps layout = null, DoodleTerrainSkin style = null)
        {
            if (!IsFinite(center) || !IsPositiveFinite(size) || !IsPositiveFinite(tileSize))
                throw new ArgumentException("방 중심·크기·타일 크기를 확인해주세요.");
            if (EditorApplication.isPlayingOrWillChangePlaymode || PrefabStageUtility.GetCurrentPrefabStage() != null)
                throw new InvalidOperationException("Play와 프리팹 모드를 닫고 씬에서 방을 만드세요.");
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded) throw new InvalidOperationException("편집할 씬을 열어주세요.");
            if (style != null && !HasSkinStyle(style)) throw new InvalidOperationException("스킨의 Fill Material을 확인해주세요.");
            if (layout == null)
            {
                LevelTilemaps[] existing = EmberSharedTilemapUtility.FindAll(scene);
                if (existing.Length > 1) throw new InvalidOperationException("공용 지형이 여러 개입니다. 사용할 공용 지형을 지정해주세요.");
                if (existing.Length == 1) layout = existing[0];
            }
            if (layout != null)
            {
                if (layout.gameObject.scene != scene || layout.Grid == null || layout.Terrain == null || layout.Spikes == null || layout.Decorations == null)
                    throw new InvalidOperationException("현재 씬의 Grid/Terrain/Spikes/Decorations가 연결된 공용 지형을 지정해주세요.");
                Vector3 origin = layout.Grid.CellToWorld(Vector3Int.zero);
                Vector3 x = layout.Grid.CellToWorld(Vector3Int.right) - origin;
                Vector3 y = layout.Grid.CellToWorld(Vector3Int.up) - origin;
                if ((x - Vector3.right * tileSize).sqrMagnitude > .0000001f || (y - Vector3.up * tileSize).sqrMagnitude > .0000001f)
                    throw new InvalidOperationException("기존 공용 Grid와 타일 크기가 다릅니다. 기존 타일 크기를 사용해주세요.");
            }

            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Create Shared Ember Room");
            try
            {
                if (layout == null) layout = EmberSharedTilemapUtility.Create(scene, tileSize);
                GameObject collection = scene.GetRootGameObjects().FirstOrDefault(x => x.name == "Rooms");
                if (collection == null) collection = CreateObject("Rooms", null, scene);
                string name = string.IsNullOrWhiteSpace(roomName) ? "Room_01" : roomName.Trim();
                GameObject root = CreateObject(GameObjectUtility.GetUniqueNameForSibling(collection.transform, name), collection.transform, scene);
                Undo.RecordObject(root.transform, "Position Shared Room");
                root.transform.position = new Vector3(center.x, center.y, 0f);
                GameObject boundsObject = CreateObject("CameraBounds", root.transform, scene);
                CameraRoom room = Undo.AddComponent<CameraRoom>(boundsObject);
                Undo.RecordObjects(new UnityEngine.Object[] { room, room.transform }, "Configure Shared Room");
                room.SetWorldBounds(new Bounds(center, new Vector3(size.x, size.y, 0f)));
                room.SetSharedTilemaps(layout);
                GameObject spawn = CreateObject("Spawn", root.transform, scene);
                Undo.RecordObject(spawn.transform, "Position Shared Room Spawn");
                spawn.transform.position = new Vector3(center.x - size.x * .5f + Mathf.Min(tileSize * 2f, size.x * .25f),
                    center.y - size.y * .5f + Mathf.Min(tileSize * 2f, size.y * .25f), 0f);
                RoomSpawnPoint point = Undo.AddComponent<RoomSpawnPoint>(spawn);
                room.SetSpawnPoint(point);
                if (style != null)
                {
                    DoodleTerrainSkin existingSkin = layout.Skin;
                    bool wasVisible = existingSkin != null && existingSkin.enabled;
                    DoodleTerrainSkin appliedSkin = ApplyDoodleSkin(room, style);
                    if (existingSkin != null && appliedSkin.enabled != wasVisible)
                    {
                        Undo.RecordObject(appliedSkin, "Preserve Shared Skin Display");
                        appliedSkin.enabled = wasVisible;
                        RecordPrefabChanges(appliedSkin);
                    }
                }
                EditorSceneManager.MarkSceneDirty(scene);
                Undo.CollapseUndoOperations(group);
                return root;
            }
            catch { Undo.RevertAllDownToGroup(group); throw; }
        }

        public static bool ConnectRoomSpawn(CameraRoom room)
        {
            if (!CanEdit(room)) return false;
            RoomSpawnPoint[] spawns = GetRoomRoot(room).GetComponentsInChildren<RoomSpawnPoint>(true)
                .Where(x => x.name == "Spawn").ToArray();
            if (spawns.Length != 1 || spawns[0].gameObject.scene != room.gameObject.scene) return false;
            if (room.SpawnPoint == spawns[0]) return true;
            Undo.RecordObject(room, "Connect Room Entry Spawn");
            room.SetSpawnPoint(spawns[0]);
            RecordPrefabChanges(room);
            EditorSceneManager.MarkSceneDirty(room.gameObject.scene);
            return true;
        }

        private static GameObject CreateRoomInternal(string roomName, Vector2 center, Vector2 size, float tileSize, DoodleTerrainSkin style)
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
                RoomSpawnPoint spawnPoint = Undo.AddComponent<RoomSpawnPoint>(spawn);
                Undo.RecordObject(room, "Connect Room Entry Spawn");
                room.SetSpawnPoint(spawnPoint);
                if (style != null) ApplyDoodleSkin(room, style);
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

        public static bool FitRoomToTilemaps(CameraRoom room) => room != null && room.SharedTilemaps == null
            && FitRoomToTilemaps(room, FindRoomTilemaps(room));

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
                if (map.name == "Terrain")
                {
                    int noCollision = 0;
                    foreach (Vector3Int cell in map.cellBounds.allPositionsWithin)
                        if (map.HasTile(cell) && map.GetColliderType(cell) == Tile.ColliderType.None) noCollision++;
                    if (noCollision > 0) messages.Add("Terrain: 충돌 없는 타일이 " + noCollision + "칸 있습니다. 지형에는 Collider Type=Grid인 타일을 사용하세요.");
                }
                if (map.name == "Spikes" && (collider == null || !collider.enabled || !collider.isTrigger ||
                    collider.compositeOperation != Collider2D.CompositeOperation.None || map.GetComponent<RoomHazard>() == null))
                    messages.Add("Spikes: Trigger TilemapCollider와 RoomHazard를 확인하세요.");
                if (map.name == "Decorations" && map.GetComponent<Collider2D>() != null)
                    messages.Add("Decorations: 장식 타일맵에는 콜라이더를 두지 않는 편이 좋습니다.");
            }
            DoodleTerrainSkin[] skins = room.SharedTilemaps != null
                ? room.SharedTilemaps.GetComponentsInChildren<DoodleTerrainSkin>(true) : root.GetComponentsInChildren<DoodleTerrainSkin>(true);
            if (skins.Length > 1) messages.Add("스킨이 여러 개 있습니다. Terrain 하나에 스킨 하나를 연결해주세요.");
            foreach (DoodleTerrainSkin skin in skins)
            {
                var data = new SerializedObject(skin);
                var source = data.FindProperty("source").objectReferenceValue as Tilemap;
                if (source == null || source != FindRoomTerrain(room))
                    messages.Add("스킨 Source가 선택 방의 Terrain에 연결되지 않았습니다. 스킨 연결 / 복구를 사용하세요.");
                if (data.FindProperty("fillMaterial").objectReferenceValue == null)
                    messages.Add("스킨 Fill Material이 없습니다. 스킨 스타일을 확인하고 연결 / 복구를 사용하세요.");
                if (source != null && source.GetComponent<TilemapRenderer>() == null)
                    messages.Add("스킨 Source에 TilemapRenderer가 없습니다. 원본 타일맵 렌더러를 복구해주세요.");
            }
            RoomSpawnPoint[] spawns = root.GetComponentsInChildren<RoomSpawnPoint>(true);
            if (spawns.Length == 0) messages.Add("Spawn이 없습니다. 방에 RoomSpawnPoint를 배치하세요.");
            if (room.SpawnPoint == null) messages.Add("방 진입 체크포인트가 연결되지 않았습니다. Spawn 자동 저장 연결을 사용하세요.");
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
            if (room.SharedTilemaps != null) return room.SharedTilemaps.Maps;
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
