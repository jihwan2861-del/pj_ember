using UnityEngine;

namespace EmberPrototype
{
    [DefaultExecutionOrder(500)]
    [DisallowMultipleComponent]
    public sealed class TutorialSceneEntry : MonoBehaviour
    {
        [SerializeField] private FlamePlayerController player;
        [SerializeField] private Transform startPoint;
        [SerializeField] private FlammableEntryTutorial firstTutorial;
        [SerializeField] private CelesteRoomCamera roomCamera;
        [SerializeField] private CameraRoom firstRoom;
        [SerializeField] private EmberBirthSequence birthSequence;

        private void Start()
        {
            if (player == null || startPoint == null || firstTutorial == null)
            {
                Debug.LogError("TutorialSceneEntry의 Player, Start Point, First Tutorial을 연결해주세요.", this);
                return;
            }

            player.SetCircularOpeningMode(false);
            player.ResetAt(startPoint.position);
            player.SetRespawnPoint(startPoint);
            if (roomCamera != null)
            {
                roomCamera.SetNormalOrthographicSize(5.0625f, true);
                roomCamera.RefreshRooms();
                roomCamera.SetTarget(player.transform, false);
                roomCamera.SetStartingRoom(firstRoom, true);
                roomCamera.SnapAfterRespawn(startPoint.position);
            }
            if (birthSequence != null && birthSequence.TryBegin(BeginAbilities)) return;
            BeginAbilities();
        }

        private void BeginAbilities()
        {
            if (!isActiveAndEnabled || player == null || firstTutorial == null) return;
            var appearance = player.GetComponent<StoryPlayerAppearance>();
            if (appearance != null) appearance.ShowEmber();
            // Scenes without a birth sequence retain the original fade-aware entry.
            firstTutorial.BeginTutorial();
        }
    }
}
