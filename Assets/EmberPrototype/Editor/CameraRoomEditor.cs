using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace EmberPrototype.Editor
{
    [CustomEditor(typeof(CameraRoom)), CanEditMultipleObjects]
    public sealed class CameraRoomEditor : UnityEditor.Editor
    {
        private readonly BoxBoundsHandle boundsHandle = new BoxBoundsHandle();
        private Camera fitCamera;
        private Tilemap fitTilemap;
        private string status;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Scene 창에서 경계의 변을 드래그하세요. 크기는 월드 단위이며 Transform Scale과 Rotation은 경계에 적용되지 않습니다.", MessageType.Info);
            CameraRoom room = (CameraRoom)target;
            if (fitCamera == null) fitCamera = EmberLevelWorkflowWindow.FindSceneCamera(room.gameObject.scene);
            fitCamera = (Camera)EditorGUILayout.ObjectField("화면 크기 기준 카메라", fitCamera, typeof(Camera), true);
            fitTilemap = (Tilemap)EditorGUILayout.ObjectField("범위 기준 타일맵 (선택)", fitTilemap, typeof(Tilemap), true);
            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
            {
                if (GUILayout.Button("카메라 한 화면 크기로 맞추기"))
                {
                    int fitted = 0;
                    foreach (Object obj in targets)
                        if (EmberLevelWorkflowWindow.FitRoomToCamera((CameraRoom)obj, fitCamera)) fitted++;
                    status = fitted > 0 ? "중심을 유지하고 경계 크기를 맞췄습니다." : "같은 씬의 정사영 카메라를 지정해주세요.";
                }
                using (new EditorGUI.DisabledScope(room.SharedTilemaps != null && fitTilemap == null))
                if (GUILayout.Button("타일맵 범위로 맞추기"))
                {
                    int fitted = 0;
                    foreach (Object obj in targets)
                    {
                        bool success = fitTilemap == null
                            ? EmberLevelWorkflowWindow.FitRoomToTilemaps((CameraRoom)obj)
                            : EmberLevelWorkflowWindow.FitRoomToTilemaps((CameraRoom)obj, new[] { fitTilemap });
                        if (success) fitted++;
                    }
                    status = fitted > 0 ? "그려진 타일맵 범위에 맞췄습니다." : "그려진 타일맵을 지정해주세요. 여러 방이 있을 때는 각 방의 타일맵을 사용하세요.";
                }
            }
            if (room.SharedTilemaps != null)
                EditorGUILayout.HelpBox("이 방은 공용 타일맵을 사용합니다. 방 경계를 수정해도 공용 지형은 이동하지 않습니다. 방 크기는 카메라 크기 또는 Scene 경계 핸들로 정하세요.", MessageType.Info);
            if (!string.IsNullOrEmpty(status)) EditorGUILayout.HelpBox(status, MessageType.Info);
        }

        private void OnSceneGUI()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            // Unity invokes this callback for each selected target individually.
            CameraRoom room = target as CameraRoom;
            if (room == null) return;
            Bounds bounds = room.WorldBounds;
            // CameraRoom defines an axis-aligned world rectangle, independent of hierarchy scale.
            using (new Handles.DrawingScope(new Color(0.15f, 0.75f, 1f), Matrix4x4.identity))
            {
                boundsHandle.axes = PrimitiveBoundsHandle.Axes.X | PrimitiveBoundsHandle.Axes.Y;
                boundsHandle.center = bounds.center;
                boundsHandle.size = bounds.size;
                EditorGUI.BeginChangeCheck();
                boundsHandle.DrawHandle();
                if (EditorGUI.EndChangeCheck())
                {
                    Vector3 size = boundsHandle.size;
                    size.x = Mathf.Max(0.01f, size.x);
                    size.y = Mathf.Max(0.01f, size.y);
                    size.z = 0f;
                    EmberLevelWorkflowWindow.ApplyWorldBounds(room, new Bounds(boundsHandle.center, size), "Resize Camera Room");
                }
            }
        }
    }
}
