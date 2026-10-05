using UnityEditor;
using UnityEngine;

namespace EmberPrototype.EditorTools
{
    [CustomEditor(typeof(MovementPath)), CanEditMultipleObjects]
    public sealed class MovementPathEditor : UnityEditor.Editor
    {
        private float previewFraction;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("shape"), new GUIContent("경로 모양 (Shape)"));
            MovementPath path = (MovementPath)target;
            bool circle = !serializedObject.FindProperty("shape").hasMultipleDifferentValues
                && serializedObject.FindProperty("shape").enumValueIndex == (int)MovementPath.PathShape.Circle;
            EditorGUILayout.HelpBox(circle
                ? "Circle: XY 평면의 정확한 원. 자식 점은 사용하지 않으며 반지름은 월드 단위입니다. 중심·시작점 핸들을 드래그하세요."
                : "Linear: 점을 직선으로 연결합니다. Smooth: 점을 통과하는 곡선이며 코너 밖으로 휘어질 수 있습니다. 닫힌 경로는 마지막 점을 첫 점에 연결합니다.", MessageType.Info);
            if (circle)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("circleCenter"), new GUIContent("중심 오프셋 (로컬)"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("circleRadius"), new GUIContent("반지름 (월드 단위)"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("circleStartAngle"), new GUIContent("시작 각도 (도)"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("circleClockwise"), new GUIContent("시계 방향"));
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("closed"), new GUIContent("닫힌 경로"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("autoCollectChildPoints"), new GUIContent("직계 자식 점 자동 수집"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("points"), new GUIContent("경유점 (순서대로)"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("smoothSamplesPerSegment"), new GUIContent("곡선 구간 샘플 수"));
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pathColor"), new GUIContent("경로 선 색상"));
            bool changed = serializedObject.ApplyModifiedProperties();
            if (changed) foreach (Object item in targets) ((MovementPath)item).Rebuild();

            using (new EditorGUI.DisabledScope(targets.Length != 1 || Application.isPlaying || circle))
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("끝에 점 추가")) { AddPoint(path); GUIUtility.ExitGUI(); }
                if (GUILayout.Button("마지막 점 삭제")) { RemoveLastPoint(path); GUIUtility.ExitGUI(); }
                EditorGUILayout.EndHorizontal();
                if (GUILayout.Button("자식 점 다시 수집"))
                {
                    Undo.RecordObject(path, "Collect Path Points");
                    path.CollectChildPoints();
                    path.Rebuild();
                    EditorUtility.SetDirty(path);
                }
            }
            using (new EditorGUI.DisabledScope(targets.Length != 1))
            {
                EditorGUILayout.Space();
                EditorGUI.BeginChangeCheck();
                previewFraction = EditorGUILayout.Slider("경로 미리보기 (고스트)", previewFraction, 0f, 1f);
                if (EditorGUI.EndChangeCheck()) SceneView.RepaintAll();
                EditorGUILayout.HelpBox("미리보기는 고스트 점만 표시하며 실제 블록 위치를 바꾸지 않습니다. 점 추가·삭제·이동은 Ctrl+Z로 되돌릴 수 있습니다.", MessageType.None);
            }
        }

        private void OnSceneGUI()
        {
            if (targets.Length != 1) return;
            MovementPath path = (MovementPath)target;
            if (!Application.isPlaying)
            {
                if (path.Shape == MovementPath.PathShape.Circle) DrawCircleHandles(path);
                else DrawPointHandles(path);
                path.Rebuild();
            }
            if (!path.IsValid) return;
            Vector2 preview = path.GetPositionAtDistance(path.TotalLength * previewFraction);
            Handles.color = new Color(1f, 0.65f, 0.1f, 0.8f);
            Handles.DrawWireDisc(preview, Vector3.forward, HandleUtility.GetHandleSize(preview) * .1f);
            Handles.Label(preview, "  경로 고스트 " + Mathf.RoundToInt(previewFraction * 100f) + "%");
        }

        private void DrawPointHandles(MovementPath path)
        {
            SerializedObject data = new SerializedObject(path);
            SerializedProperty points = data.FindProperty("points");
            for (int i = 0; i < points.arraySize; i++)
            {
                Transform point = points.GetArrayElementAtIndex(i).objectReferenceValue as Transform;
                if (point == null) continue;
                Handles.Label(point.position, "  점 " + (i + 1));
                EditorGUI.BeginChangeCheck();
                Vector3 moved = Handles.PositionHandle(point.position, Quaternion.identity);
                if (!EditorGUI.EndChangeCheck()) continue;
                Undo.RecordObject(point, "Move Path Point");
                moved.z = point.position.z; // 2D paths are edited in XY only.
                point.position = moved;
                PrefabUtility.RecordPrefabInstancePropertyModifications(point);
            }
        }

        private void DrawCircleHandles(MovementPath path)
        {
            Vector3 center = path.CircleCenterWorld;
            center.z = path.transform.position.z;
            Handles.color = new Color(.3f, .9f, 1f);
            Handles.Label(center, "  원 중심");
            EditorGUI.BeginChangeCheck();
            Vector3 movedCenter = Handles.PositionHandle(center, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                movedCenter.z = center.z;
                SerializedObject data = new SerializedObject(path);
                data.FindProperty("circleCenter").vector2Value = path.transform.InverseTransformPoint(movedCenter);
                data.ApplyModifiedProperties();
                path.Rebuild();
                center = path.CircleCenterWorld;
                center.z = path.transform.position.z;
            }
            float angle = path.CircleStartAngle * Mathf.Deg2Rad;
            Vector3 start = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * path.CircleRadius;
            Handles.Label(start, path.CircleClockwise ? "  시작점 ↻ (반지름·각도)" : "  시작점 ↺ (반지름·각도)");
            EditorGUI.BeginChangeCheck();
            Vector3 moved = Handles.PositionHandle(start, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Vector2 delta = (Vector2)(moved - center);
                SerializedObject data = new SerializedObject(path);
                data.FindProperty("circleRadius").floatValue = Mathf.Max(.01f, delta.magnitude);
                data.FindProperty("circleStartAngle").floatValue = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
                data.ApplyModifiedProperties();
                path.Rebuild();
            }
            Vector2 directionPoint = path.GetPositionAtDistance(path.TotalLength * .04f);
            Vector2 tangent = (directionPoint - path.StartPosition).normalized;
            Handles.ArrowHandleCap(0, directionPoint, Quaternion.LookRotation(tangent), HandleUtility.GetHandleSize(start) * .4f, EventType.Repaint);
        }

        private static void AddPoint(MovementPath path)
        {
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Add Path Point");
            SerializedObject data = new SerializedObject(path);
            SerializedProperty points = data.FindProperty("points");
            Vector3 position = path.transform.position;
            if (points.arraySize > 0 && points.GetArrayElementAtIndex(points.arraySize - 1).objectReferenceValue is Transform last)
                position = last.position + Vector3.right * 2f;
            GameObject point = new GameObject("Point " + (points.arraySize + 1));
            Undo.RegisterCreatedObjectUndo(point, "Add Path Point");
            Undo.SetTransformParent(point.transform, path.transform, "Parent Path Point");
            point.transform.position = position;
            points.arraySize++;
            points.GetArrayElementAtIndex(points.arraySize - 1).objectReferenceValue = point.transform;
            data.ApplyModifiedProperties();
            if (data.FindProperty("autoCollectChildPoints").boolValue) path.CollectChildPoints();
            path.Rebuild();
            Undo.CollapseUndoOperations(group);
            SceneView.RepaintAll();
        }

        private static void RemoveLastPoint(MovementPath path)
        {
            SerializedObject data = new SerializedObject(path);
            SerializedProperty points = data.FindProperty("points");
            if (points.arraySize == 0) return;
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Remove Path Point");
            Transform point = points.GetArrayElementAtIndex(points.arraySize - 1).objectReferenceValue as Transform;
            bool destroyChild = data.FindProperty("autoCollectChildPoints").boolValue && point != null && point.parent == path.transform;
            points.arraySize--;
            data.ApplyModifiedProperties();
            if (destroyChild) Undo.DestroyObjectImmediate(point.gameObject);
            Undo.RecordObject(path, "Update Path Points");
            if (data.FindProperty("autoCollectChildPoints").boolValue) path.CollectChildPoints();
            path.Rebuild();
            EditorUtility.SetDirty(path);
            Undo.CollapseUndoOperations(group);
            SceneView.RepaintAll();
        }
    }
}
