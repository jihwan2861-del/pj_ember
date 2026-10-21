using System.Collections;
using UnityEngine;

namespace EmberPrototype
{
    public enum StoryCuePresentation
    {
        Narration = 0,
        Dialogue = 1
    }

    [AddComponentMenu("Ember/Story Narration Cue")]
    [DisallowMultipleComponent]
    public sealed class StoryNarrationCue : MonoBehaviour
    {
        [SerializeField] private StorybookNarration narration;
        [SerializeField, TextArea(1, 4)] private string[] lines = { "아무거나" };
        [SerializeField] private bool lockPlayerControls;
        [SerializeField] private bool playOnce = true;
        [Header("Dialogue")]
        [SerializeField] private StoryCuePresentation presentation;
        [SerializeField] private string speakerName;
        [SerializeField] private Sprite speakerPortrait;
        [Tooltip("비우면 모든 줄에 기본 화자와 초상화를 사용합니다. 줄별로 설정할 때 Lines와 순서를 맞춥니다.")]
        [SerializeField] private string[] lineSpeakers;
        [Tooltip("줄별 초상화입니다. null 항목은 해당 줄에서 초상화를 숨깁니다.")]
        [SerializeField] private Sprite[] linePortraits;
        [Header("Rescue")]
        [Tooltip("대화가 실제로 시작될 때 비활성화해 구조된 상태를 보여줄 오브젝트입니다.")]
        [SerializeField] private GameObject[] rescueVisuals;
        private bool hasPlayed;
        private Coroutine pending;

        public void BeginNarration()
        {
            if (!isActiveAndEnabled || (playOnce && hasPlayed) || pending != null) return;
            if (narration == null)
            {
                foreach (var candidate in FindObjectsByType<StorybookNarration>())
                {
                    if (candidate.gameObject.scene != gameObject.scene) continue;
                    narration = candidate;
                    break;
                }
            }

            if (narration != null) pending = StartCoroutine(BeginWhenReady());
        }

        private IEnumerator BeginWhenReady()
        {
            // Let the triggering event return, then queue behind any narration already in progress.
            yield return null;
            while (isActiveAndEnabled && narration != null && narration.IsPlaying)
                yield return null;

            if (isActiveAndEnabled && narration != null)
            {
                bool began = presentation == StoryCuePresentation.Dialogue
                    ? narration.BeginDialogueWithLines(lines, lockPlayerControls, speakerName, speakerPortrait, lineSpeakers, linePortraits)
                    : narration.BeginNarrationWithLines(lines, lockPlayerControls);
                if (began)
                {
                    hasPlayed = true;
                    if (rescueVisuals != null)
                    {
                        foreach (GameObject visual in rescueVisuals)
                            if (visual != null) visual.SetActive(false);
                    }
                }
            }
            pending = null;
        }

        public void ResetCue() => hasPlayed = false;

        private void OnDisable()
        {
            if (pending != null) StopCoroutine(pending);
            pending = null;
        }
    }
}
