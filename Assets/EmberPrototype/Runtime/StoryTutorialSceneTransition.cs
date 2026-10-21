using System.Collections;
using UnityEngine;

namespace EmberPrototype
{
    [DisallowMultipleComponent]
    public sealed class StoryTutorialSceneTransition : MonoBehaviour
    {
        [SerializeField] private string tutorialScene = "TutorialScene";
        [SerializeField] private FlamePlayerController player;
        [SerializeField] private WaterSurface2D waterSurface;
        [SerializeField] private RiverMemorySequence memorySequence;
        [SerializeField, Min(0f)] private float fadeOutSeconds = 0.65f;
        [SerializeField, Min(0f)] private float fadeInSeconds = 0.4f;
        private Coroutine pending;

        public void BeginTransition()
        {
            if (pending == null && isActiveAndEnabled) pending = StartCoroutine(WaitAndLoad());
        }

        private IEnumerator WaitAndLoad()
        {
            // Closing narration early must not skip the reflection's final state.
            yield return null;
            while ((memorySequence != null ? !memorySequence.IsCompleted
                    : waterSurface != null && waterSurface.TreeWitherProgress < 1f)
                || (player != null && player.IsDying) || GameSessionControls.IsPaused
                || SceneFadeTransition.IsPlaying || SceneRestartWipe.IsPlaying)
                yield return null;

            FlowerRideTutorial.EndSessionSlowMotion();
            if (player != null) player.CancelRingShotAim();
            Time.timeScale = GameSessionControls.PlaybackSpeed;
            SceneFadeTransition.TryBegin(tutorialScene, player, fadeOutSeconds, fadeInSeconds);
            pending = null;
        }

        private void OnDisable()
        {
            if (pending != null) StopCoroutine(pending);
            pending = null;
        }
    }
}
