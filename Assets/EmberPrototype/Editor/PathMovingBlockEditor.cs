using UnityEditor;
using UnityEngine;

namespace EmberPrototype.EditorTools
{
    [CustomEditor(typeof(PathMovingBlock)), CanEditMultipleObjects]
    public sealed class PathMovingBlockEditor : UnityEditor.Editor
    {
        private float previewFraction;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("path"), new GUIContent("이동 경로"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("moveSpeed"), new GUIContent("이동 속도 (월드 단위/초)"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("travelMode"), new GUIContent("이동 방식"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("endWaitTime"), new GUIContent("끝점 대기 시간 (초)"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("snapToPathStart"), new GUIContent("시작 시 경로 첫 점에 배치"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("activateOnStart"), new GUIContent("플레이 시작 시 자동 출발"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onArrived"), new GUIContent("끝점 도착 이벤트"));
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.HelpBox("Once: 끝에서 정지하며 ResetBlock 후 다시 출발합니다. PingPong: 양 끝에서 대기 후 왕복합니다. Loop: 닫힌 경로/원을 계속 순환하며 한 바퀴마다 대기할 수 있습니다.", MessageType.Info);
            foreach (Object item in targets)
            {
                PathMovingBlock block = (PathMovingBlock)item;
                if (block.Mode == PathMovingBlock.TravelMode.Loop && !block.CanLoop)
                    EditorGUILayout.HelpBox(block.name + ": 열린 경로의 Loop는 시작하지 않습니다. 경로를 닫거나 Circle/PingPong을 사용하세요.", MessageType.Warning);
                if (block.Path != null && block.transform.IsChildOf(block.Path.transform))
                    EditorGUILayout.HelpBox("블록을 경로의 자식으로 두면 자식 점 자동 수집에 포함될 수 있습니다. 경로와 블록은 별도 오브젝트로 배치하세요.", MessageType.Warning);
            }
            EditorGUILayout.HelpBox("점화/스위치 UnityEvent에는 기존 Activate를 연결합니다. 반복 모드의 IsMoving은 끝점 대기 중에도 true입니다. FlamePlayerController는 발판 위에서 함께 이동합니다. 발판 속도를 점프에 더하는 관성 전달은 적용하지 않습니다.", MessageType.None);
            using (new EditorGUI.DisabledScope(targets.Length != 1))
            {
                EditorGUI.BeginChangeCheck();
                previewFraction = EditorGUILayout.Slider("블록 미리보기 (고스트)", previewFraction, 0f, 1f);
                if (EditorGUI.EndChangeCheck()) SceneView.RepaintAll();
                if (GUILayout.Button("경로 선택") && ((PathMovingBlock)target).Path != null)
                    Selection.activeGameObject = ((PathMovingBlock)target).Path.gameObject;
            }
        }

        private void OnSceneGUI()
        {
            PathMovingBlock block = (PathMovingBlock)target;
            MovementPath path = block.Path;
            if (path == null) return;
            if (!Application.isPlaying) path.Rebuild();
            if (!path.IsValid) return;
            Vector2 position = GetPreviewPosition();
            Vector3 size = Vector3.one;
            Collider2D collider = block.GetComponent<Collider2D>();
            if (collider != null) size = collider.bounds.size;
            Handles.color = new Color(1f, .65f, .1f, .8f);
            Vector3 half = size * .5f;
            Vector3 center = new Vector3(position.x, position.y, block.transform.position.z);
            Handles.DrawAAPolyLine(center + new Vector3(-half.x, -half.y, 0f), center + new Vector3(-half.x, half.y, 0f),
                center + new Vector3(half.x, half.y, 0f), center + new Vector3(half.x, -half.y, 0f), center + new Vector3(-half.x, -half.y, 0f));
            Handles.Label(center, "  블록 고스트 (실제 위치 유지)");
        }

        private Vector2 GetPreviewPosition()
        {
            PathMovingBlock block = (PathMovingBlock)target;
            MovementPath path = block.Path;
            return path != null ? path.GetPositionAtDistance(path.TotalLength * previewFraction) : (Vector2)block.transform.position;
        }
    }
}
