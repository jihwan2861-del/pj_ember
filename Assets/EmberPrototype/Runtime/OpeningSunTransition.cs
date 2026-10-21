using UnityEngine;
using UnityEngine.Events;

namespace EmberPrototype
{
    /// <summary>Optionally moves opening suns with world progress, then pins the corrupted sun to the camera.</summary>
    [AddComponentMenu("Ember/Opening Sun Transition")]
    [DefaultExecutionOrder(1050)] // Follow the final camera pose before water samples the sun bounds.
    public sealed class OpeningSunTransition : MonoBehaviour
    {
        [SerializeField] private AngleRotator progressSource;
        [Tooltip("보이지 않는 원 궤도의 중심입니다. 지형 회전 대상 밖에 둡니다.")]
        [SerializeField] private Transform orbitCenter;
        [SerializeField, Min(0.01f)] private float orbitRadius = 11.5f;
        [Tooltip("끄면 오프닝 원형 구간 동안 정상/오염 태양을 숨기고 이동시키지 않습니다. 고정 연출은 PinCorruptedSunToCamera로 시작할 수 있습니다.")]
        [SerializeField] private bool showOrbitingSunsDuringOpening = true;
        [SerializeField] private Transform normalSun;
        [SerializeField] private Transform corruptedSun;
        [Header("Progress angles")]
        [SerializeField, Min(0.01f)] private float corruptionStartAngle = 180f;
        [SerializeField, Min(0.01f)] private float corruptionArrivalDegrees = 90f;
        [Header("Orbit angles: 0 right, 90 top, 180 left")]
        [SerializeField] private float normalStartDegrees = 60f;
        [SerializeField] private float normalSweepDegrees = 180f;
        [SerializeField] private float corruptedStartDegrees = 0f;
        [SerializeField] private float corruptedSweepDegrees = 90f;
        [Header("Corrupted sun resting position")]
        [SerializeField] private Camera skyCamera;
        [SerializeField] private Vector2 upperRightViewport = new Vector2(0.86f, 0.84f);
        [SerializeField] private UnityEvent onCorruptionStarted = new UnityEvent();
        private bool corruptionNotified;
        private bool pinnedToCamera;
        public float CorruptionStartAngle => Mathf.Max(0.01f, corruptionStartAngle);

        private void Awake()
        {
            if (!showOrbitingSunsDuringOpening) SetOrbitingSunsVisible(false);
        }

        private void LateUpdate()
        {
            if (pinnedToCamera)
            {
                ApplyPinnedPosition();
                return;
            }
            if (progressSource == null) return;
            if (!showOrbitingSunsDuringOpening)
            {
                SetOrbitingSunsVisible(false);
                NotifyCorruptionStarted(progressSource.ProgressAngle);
                return;
            }
            if (orbitCenter == null) return;
            ApplyAngle(progressSource.ProgressAngle);
        }

        /// <summary>Called under the opening fade; keep this viewport position afterward.</summary>
        public void PinCorruptedSunToCamera()
        {
            pinnedToCamera = true;
            if (skyCamera == null) skyCamera = Camera.main;
            if (normalSun != null) normalSun.gameObject.SetActive(false);
            if (corruptedSun != null) corruptedSun.gameObject.SetActive(true);
            ApplyPinnedPosition();
        }

        private void ApplyPinnedPosition()
        {
            if (skyCamera == null || corruptedSun == null) return;
            float distance = Vector3.Dot(corruptedSun.position - skyCamera.transform.position,
                skyCamera.transform.forward);
            if (distance <= 0f) distance = Mathf.Max(skyCamera.nearClipPlane + 1f, 10f);
            corruptedSun.position = skyCamera.ViewportToWorldPoint(
                new Vector3(upperRightViewport.x, upperRightViewport.y, distance));
        }

        // Can also be driven from an editor preview or a separate story timeline.
        public void ApplyAngle(float angle)
        {
            if (pinnedToCamera)
            {
                ApplyPinnedPosition();
                return;
            }
            if (!showOrbitingSunsDuringOpening)
            {
                SetOrbitingSunsVisible(false);
                NotifyCorruptionStarted(angle);
                return;
            }
            if (orbitCenter == null) return;
            float split = Mathf.Max(0.01f, corruptionStartAngle);
            if (normalSun != null)
            {
                bool visible = angle < split;
                if (normalSun.gameObject.activeSelf != visible) normalSun.gameObject.SetActive(visible);
                SetPosition(normalSun, normalStartDegrees + normalSweepDegrees * Mathf.Clamp01(angle / split));
            }
            if (corruptedSun != null)
            {
                bool visible = angle >= split;
                if (corruptedSun.gameObject.activeSelf != visible) corruptedSun.gameObject.SetActive(visible);
                SetPosition(corruptedSun, corruptedStartDegrees + corruptedSweepDegrees *
                    Mathf.Clamp01((angle - split) / Mathf.Max(0.01f, corruptionArrivalDegrees)));
                if (skyCamera != null)
                {
                    float arrival = Mathf.Clamp01((angle - split) / Mathf.Max(0.01f, corruptionArrivalDegrees));
                    float distance = Vector3.Dot(corruptedSun.position - skyCamera.transform.position, skyCamera.transform.forward);
                    Vector3 resting = skyCamera.ViewportToWorldPoint(new Vector3(upperRightViewport.x, upperRightViewport.y, distance));
                    float blend = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.65f, 1f, arrival));
                    corruptedSun.position = Vector3.Lerp(corruptedSun.position, resting, blend);
                }
            }
            NotifyCorruptionStarted(angle);
        }

        private void NotifyCorruptionStarted(float angle)
        {
            if (!Application.isPlaying || angle < CorruptionStartAngle || corruptionNotified) return;
            corruptionNotified = true;
            onCorruptionStarted.Invoke();
        }

        private void SetOrbitingSunsVisible(bool visible)
        {
            if (normalSun != null && normalSun.gameObject.activeSelf != visible)
                normalSun.gameObject.SetActive(visible);
            if (corruptedSun != null && corruptedSun.gameObject.activeSelf != visible)
                corruptedSun.gameObject.SetActive(visible);
        }

        private void SetPosition(Transform sun, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            sun.position = orbitCenter.TransformPoint(new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f) * orbitRadius);
        }

        private void OnDrawGizmosSelected()
        {
            if (orbitCenter == null) return;
            Gizmos.color = new Color(1f, 0.7f, 0.1f, 0.8f);
            Vector3 previous = orbitCenter.TransformPoint(Vector3.right * orbitRadius);
            for (int i = 1; i <= 64; i++)
            {
                float radians = i * Mathf.PI * 2f / 64f;
                Vector3 next = orbitCenter.TransformPoint(new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f) * orbitRadius);
                Gizmos.DrawLine(previous, next);
                previous = next;
            }
        }

        public void ResetTransition()
        {
            corruptionNotified = false;
            pinnedToCamera = false;
            if (showOrbitingSunsDuringOpening && orbitCenter != null) ApplyAngle(0f);
            else SetOrbitingSunsVisible(false);
        }
    }
}
