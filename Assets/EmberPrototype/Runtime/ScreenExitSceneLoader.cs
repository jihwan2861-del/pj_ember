using UnityEngine;

namespace EmberPrototype
{
    [AddComponentMenu("Ember/Screen Exit Fade")]
    [DefaultExecutionOrder(-100)]
    public sealed class ScreenExitSceneLoader : MonoBehaviour
    {
        [SerializeField] private Camera viewCamera;
        [SerializeField] private FlamePlayerController player;
        [SerializeField] private Transform nextStoryStart;
        [SerializeField] private Transform nextCameraPose;
        [SerializeField] private Transform[] screenBackdrops;
        [SerializeField] private UnityEngine.Events.UnityEvent onContinued = new UnityEngine.Events.UnityEvent();
        [SerializeField, Min(0f)] private float screenMargin = 0.03f;
        [SerializeField, Min(0f)] private float fadeOutSeconds = 0.65f;
        [SerializeField, Min(0f)] private float fadeInSeconds = 0.4f;
        private bool wasOnScreen;
        private bool triggered;
        private bool reportedMissingDestination;
        private CelesteRoomCamera openingRoomCamera;

        private void Awake() => ReserveOpeningCamera();

        private void OnEnable()
        {
            if (!triggered) ReserveOpeningCamera();
        }

        private void ReserveOpeningCamera()
        {
            // A following camera prevents the player from ever leaving its screen.
            // Reserve the authored opening view before CelesteRoomCamera.Start snaps.
            if (viewCamera != null)
                openingRoomCamera = viewCamera.GetComponent<CelesteRoomCamera>();
            if (openingRoomCamera != null) openingRoomCamera.SetMotionSuspended(this, true);
        }

        private void OnDisable() => ReleaseOpeningCamera();

        private void ReleaseOpeningCamera()
        {
            if (openingRoomCamera != null) openingRoomCamera.SetMotionSuspended(this, false);
        }

        private void LateUpdate()
        {
            if (triggered || viewCamera == null || player == null || player.IsDying
                || !player.gameObject.activeInHierarchy || SceneFadeTransition.IsPlaying
                || GameSessionControls.IsPaused || SceneRestartWipe.IsPlaying) return;
            Bounds bounds = new Bounds(player.transform.position, Vector3.zero);
            bool found = false;
            foreach (var visual in player.GetComponentsInChildren<SpriteRenderer>())
            {
                if (!visual.enabled || visual.forceRenderingOff) continue;
                if (!found) { bounds = visual.bounds; found = true; }
                else bounds.Encapsulate(visual.bounds);
            }
            float minX = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float minY = float.PositiveInfinity;
            float maxY = float.NegativeInfinity;
            for (int i = 0; i < 8; i++)
            {
                Vector3 corner = bounds.center + Vector3.Scale(bounds.extents,
                    new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                Vector3 viewport = viewCamera.WorldToViewportPoint(corner);
                if (viewport.z <= 0f) return;
                minX = Mathf.Min(minX, viewport.x); maxX = Mathf.Max(maxX, viewport.x);
                minY = Mathf.Min(minY, viewport.y); maxY = Mathf.Max(maxY, viewport.y);
            }
            if (maxX >= 0f && minX <= 1f && maxY >= 0f && minY <= 1f) wasOnScreen = true;
            if (!wasOnScreen || player.IsCircularOpening || minX <= 1f + screenMargin
                || maxY < 0f || minY > 1f) return;
            if (nextStoryStart == null || nextCameraPose == null)
            {
                if (!reportedMissingDestination)
                    Debug.LogError("Next Story Start와 Next Camera Pose를 연결해주세요.", this);
                reportedMissingDestination = true;
                return;
            }
            triggered = SceneFadeTransition.TryFade(player, fadeOutSeconds, fadeInSeconds, ContinueInScene);
        }

        private void ContinueInScene()
        {
            if (player == null || viewCamera == null || nextStoryStart == null || nextCameraPose == null) return;
            Vector3 cameraDelta = nextCameraPose.position - viewCamera.transform.position;
            var body = player.GetComponent<Rigidbody2D>();
            if (body != null)
            {
                body.position = nextStoryStart.position;
                body.linearVelocity = Vector2.zero;
                body.angularVelocity = 0f;
            }
            else player.transform.position = nextStoryStart.position;
            viewCamera.transform.SetPositionAndRotation(nextCameraPose.position, nextCameraPose.rotation);
            if (screenBackdrops != null)
                foreach (var backdrop in screenBackdrops)
                    if (backdrop != null && !backdrop.IsChildOf(viewCamera.transform)) backdrop.position += cameraDelta;
            player.SetRespawnPoint(nextStoryStart);
            var room = player.GetComponent<PrototypeRoom>();
            if (room != null) room.SetSpawnPoint(nextStoryStart);
            Physics2D.SyncTransforms();
            try { onContinued.Invoke(); }
            finally
            {
                // The callback may activate rooms that were absent from the startup cache.
                if (openingRoomCamera != null) openingRoomCamera.RefreshRooms();
                ReleaseOpeningCamera();
            }
        }
    }
}
