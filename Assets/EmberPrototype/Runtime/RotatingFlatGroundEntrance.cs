using UnityEngine;

namespace EmberPrototype
{
    [AddComponentMenu("Ember/Rotating Flat Ground Entrance")]
    [DefaultExecutionOrder(-50)]
    public sealed class RotatingFlatGroundEntrance : MonoBehaviour
    {
        [SerializeField] private AngleRotator progressSource;
        [SerializeField] private Transform orbitCenter;
        [SerializeField] private Transform flatGround;
        [Tooltip("평지가 수평으로 연결될 최종 위치와 방향입니다.")]
        [SerializeField] private Transform joinPose;
        [Tooltip("연결하면 바퀴 수 대신 오염된 태양 등장 시점에 평지도 진입합니다.")]
        [SerializeField] private OpeningSunTransition synchronizeWithSun;
        [Tooltip("원형 회전 없이 시작하는 오프닝에서 평지를 처음부터 연결 위치에 둡니다.")]
        [SerializeField] private bool beginAtJoinPose;
        [SerializeField, Min(0)] private int completedTurns = 2;
        [Tooltip("바퀴 수를 채운 뒤 평지가 화면 밖에서 돌아 들어오는 추가 회전량입니다.")]
        [SerializeField, Range(1f, 180f)] private float entranceDegrees = 90f;

        public float EntranceStartAngle => synchronizeWithSun != null
            ? synchronizeWithSun.CorruptionStartAngle : Mathf.Max(0, completedTurns) * 360f;
        public float ArrivalAngle => EntranceStartAngle + entranceDegrees;

        private void Start()
        {
            if (progressSource != null) progressSource.SetAngleLimit(ArrivalAngle);
            PreviewProgress(beginAtJoinPose || progressSource == null ? ArrivalAngle : progressSource.ProgressAngle);
        }

        private void LateUpdate()
        {
            PreviewProgress(beginAtJoinPose || progressSource == null ? ArrivalAngle : progressSource.ProgressAngle);
        }

        public void PreviewProgress(float angle)
        {
            if (flatGround == null || orbitCenter == null || joinPose == null) return;
            float start = EntranceStartAngle;
            bool visible = angle >= start;
            if (flatGround.gameObject.activeSelf != visible) flatGround.gameObject.SetActive(visible);
            // Rotate the entire slab (including its collision) around the world's center.
            // At ArrivalAngle the slab reaches the exact horizontal join pose.
            float sign = progressSource != null ? progressSource.RotationSign : 1f;
            Quaternion offset = Quaternion.Euler(0f, 0f, (Mathf.Clamp(angle, start, ArrivalAngle) - ArrivalAngle) * sign);
            flatGround.SetPositionAndRotation(orbitCenter.position + offset * (joinPose.position - orbitCenter.position),
                offset * joinPose.rotation);
        }
    }
}
