using UnityEngine;

namespace EmberPrototype
{
    [AddComponentMenu("Ember/Story Section Camera")]
    [DefaultExecutionOrder(-10)]
    public sealed class StorySectionCamera : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private GameObject sectionRoot;
        [SerializeField] private Transform backdrop;
        [SerializeField] private float horizontalOffset = 1.5f;
        [SerializeField] private float minX = 44.5f;
        [SerializeField] private float maxX = 52.5f;
        [SerializeField, Min(0.01f)] private float smoothTime = 0.3f;
        [Header("Water Framing")]
        [SerializeField, Tooltip("물가로 걸어갈 때 수면 아래의 반사까지 보이도록 카메라 높이를 전환합니다.")]
        private bool reframeForWater = true;
        [SerializeField] private float waterFramingStartX = 48f;
        [SerializeField] private float waterFramingEndX = 58f;
        [SerializeField] private float waterCameraY = 0.5f;
        [SerializeField, Min(0.01f)] private float waterOrthographicSize = 9f;
        [Header("Past Reflection Framing")]
        [SerializeField] private float pastFramingStartX = 74f;
        [SerializeField] private float pastFramingEndX = 80f;
        [SerializeField] private float pastCameraY = -2.3f;
        [SerializeField] private float pastCameraX = 66.5f;
        [SerializeField, Min(0.01f)] private float pastOrthographicSize = 8.5f;
        [Header("Gameplay Tutorial Framing")]
        [SerializeField, Min(0.01f)] private float gameplayOrthographicSize = 5.0625f;
        [SerializeField, Min(0.01f), Tooltip("오른쪽 CameraRoom에 걸어 들어갈 때 셀레스트 카메라가 줌과 위치를 이어받는 시간입니다.")]
        private float gameplayCameraBlendSeconds = 0.8f;
        private bool following;
        private bool gameplayFraming;
        private float velocity;
        private float verticalVelocity;
        private float orthographicVelocity;
        private float initialCameraY;
        private float initialOrthographicSize;
        private Camera viewCamera;
        private CelesteRoomCamera roomCamera;

        public void BeginFollowing()
        {
            gameplayFraming = false;
            if (roomCamera != null) roomCamera.SetMotionSuspended(this, false);
            if (sectionRoot != null) sectionRoot.SetActive(true);
            velocity = 0f;
            verticalVelocity = 0f;
            orthographicVelocity = 0f;
            initialCameraY = transform.position.y;
            viewCamera = GetComponent<Camera>();
            initialOrthographicSize = viewCamera != null ? viewCamera.orthographicSize : 5f;
            following = true;
            roomCamera = GetComponent<CelesteRoomCamera>();
            if (roomCamera != null) roomCamera.RefreshRooms();
            UpdateCameraOwnership();
        }

        public void BeginGameplayTutorial()
        {
            viewCamera = GetComponent<Camera>();
            if (viewCamera == null || player == null) return;
            roomCamera = GetComponent<CelesteRoomCamera>();
            if (roomCamera != null)
            {
                roomCamera.RefreshRooms();
                roomCamera.SetMotionSuspended(this, true);
            }
            velocity = verticalVelocity = orthographicVelocity = 0f;
            gameplayFraming = true;
            following = true;
        }

        public void StopFollowing()
        {
            following = false;
            gameplayFraming = false;
            if (roomCamera != null) roomCamera.SetMotionSuspended(this, false);
        }

        private void OnDisable()
        {
            following = false;
            gameplayFraming = false;
            if (roomCamera != null) roomCamera.SetMotionSuspended(this, false);
        }

        private bool UpdateCameraOwnership()
        {
            if (roomCamera == null) roomCamera = GetComponent<CelesteRoomCamera>();
            if (roomCamera == null) return true;
            bool useRoomCamera = roomCamera.isActiveAndEnabled && player != null
                && (roomCamera.IsCinematicActive || roomCamera.HasRoomAtPosition(player.position));
            roomCamera.SetMotionSuspended(this, !useRoomCamera);
            return !useRoomCamera;
        }

        private void LateUpdate()
        {
            if (!following || GameSessionControls.BlockGameplayInput) return;
            if (gameplayFraming)
            {
                StepGameplayFraming();
                return;
            }
            if (player == null) return;
            if (!UpdateCameraOwnership()) return;
            Vector3 before = transform.position;
            float target = Mathf.Clamp(player.position.x + horizontalOffset, minX, Mathf.Max(minX, maxX));
            float pastProgress = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(pastFramingStartX,
                Mathf.Max(pastFramingStartX + 0.01f, pastFramingEndX), player.position.x));
            if (reframeForWater) target = Mathf.Lerp(target, pastCameraX, pastProgress);
            Vector3 position = before;
            position.x = Mathf.SmoothDamp(before.x, target, ref velocity, smoothTime);
            if (reframeForWater)
            {
                float progress = Mathf.InverseLerp(waterFramingStartX,
                    Mathf.Max(waterFramingStartX + 0.01f, waterFramingEndX), player.position.x);
                float waterProgress = Mathf.SmoothStep(0f, 1f, progress);
                float targetY = Mathf.Lerp(initialCameraY, waterCameraY, waterProgress);
                targetY = Mathf.Lerp(targetY, pastCameraY, pastProgress);
                position.y = Mathf.SmoothDamp(before.y, targetY, ref verticalVelocity, smoothTime);
                if (viewCamera != null && viewCamera.orthographic)
                {
                    float targetSize = Mathf.Lerp(initialOrthographicSize, waterOrthographicSize, waterProgress);
                    targetSize = Mathf.Lerp(targetSize, pastOrthographicSize, pastProgress);
                    viewCamera.orthographicSize = Mathf.SmoothDamp(viewCamera.orthographicSize,
                        targetSize, ref orthographicVelocity, smoothTime);
                }
            }
            transform.position = position;
            MoveLegacyBackdrop(position - before);
        }

        private void StepGameplayFraming()
        {
            if (roomCamera == null || player == null)
            {
                StopFollowing();
                return;
            }
            // Keep the narration view until the player walks into the adjacent room.
            // Room bounds and the existing Celeste camera own the continuation.
            if (!roomCamera.HasRoomAtPosition(player.position)) return;
            roomCamera.SetNormalOrthographicSize(gameplayOrthographicSize, false);
            roomCamera.SetTarget(player, false);
            StopFollowing();
            roomCamera.EndCinematic(gameplayCameraBlendSeconds);
        }

        private void MoveLegacyBackdrop(Vector3 displacement)
        {
            if (backdrop != null && !backdrop.IsChildOf(transform))
            {
                ParallaxLayer parallax = backdrop.GetComponent<ParallaxLayer>();
                // A disabled ParallaxLayer intentionally leaves its backdrop fixed.
                // Only legacy backdrops without that component follow this camera.
                if (parallax == null)
                    backdrop.position += displacement;
            }
        }
    }
}
