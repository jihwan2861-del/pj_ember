using UnityEngine;
using UnityEngine.Events;

namespace EmberPrototype
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Ember/Flammable Entry Tutorial")]
    public sealed class FlammableEntryTutorial : MonoBehaviour
    {
        public enum TutorialPhase
        {
            Idle,
            WaitingToBegin,
            Framing,
            EnterFlammable,
            ExitFlammable,
            Completed
        }

        [Header("Exercise")]
        [SerializeField] private FlamePlayerController player;
        [SerializeField] private FlammableTile targetFire;
        [SerializeField] private GameObject exerciseRoot;
        [SerializeField] private bool hideExerciseUntilBegin = true;
        [SerializeField] private BoxCollider2D landingArea;
        [SerializeField] private WaterSurface2D waterSurface;
        [SerializeField, Min(0f)] private float cameraBlendSeconds;

        [Header("Gameplay Sorting")]
        [SerializeField] private bool useGameplaySorting = true;
        [SerializeField] private string gameplaySortingLayer = "EmberWater";
        [SerializeField] private int gameplaySortingOrder = 10;

        [Header("Existing Cue UI")]
        [SerializeField] private Canvas cueCanvas;
        [SerializeField] private UnityEngine.UI.Text instruction;
        [SerializeField] private UnityEngine.UI.Text keyLabel;
        [SerializeField] private GameObject keycapRoot;
        [SerializeField, Min(0f)] private float cueActivationDistance = 5f;
        [SerializeField, Min(0f)] private float completedCueSeconds = 1.2f;

        [Header("Events")]
        [SerializeField] private UnityEvent onBegan = new UnityEvent();
        [SerializeField] private UnityEvent onCompleted = new UnityEvent();

        private bool waitForWithering;
        private bool ownsControlLock;
        private bool observedDeath;
        private float phaseRemaining;
        private int launchCountAtEntry;
        private SpriteRenderer sortingRenderer;
        private bool ownsGameplaySorting;
        private int previousSortingLayerId;
        private int previousSortingOrder;
        private int appliedSortingLayerId;
        private int appliedSortingOrder;

        public TutorialPhase CurrentPhase { get; private set; }
        public bool HasBegun { get; private set; }
        public bool IsCompleted => CurrentPhase == TutorialPhase.Completed;

        private static bool ProgressBlocked => GameSessionControls.BlockGameplayInput
            || GameSessionControls.IsPaused || SceneRestartWipe.IsPlaying || SceneFadeTransition.IsPlaying;

        private void Awake()
        {
            HideCue();
            if (hideExerciseUntilBegin && exerciseRoot != null) exerciseRoot.SetActive(false);
        }

        public void QueueBegin()
        {
            if (!isActiveAndEnabled || HasBegun || IsCompleted) return;
            waitForWithering = true;
            CurrentPhase = TutorialPhase.WaitingToBegin;
            HideCue();
        }

        public void BeginTutorial()
        {
            if (!isActiveAndEnabled || HasBegun || IsCompleted || player == null || targetFire == null) return;
            if (ProgressBlocked || player.IsDying)
            {
                // A direct request can wait out a pause or fade without requiring the reflection again.
                if (CurrentPhase != TutorialPhase.WaitingToBegin)
                {
                    waitForWithering = false;
                    CurrentPhase = TutorialPhase.WaitingToBegin;
                }
                return;
            }

            waitForWithering = false;
            HasBegun = true;
            CurrentPhase = cameraBlendSeconds > 0f ? TutorialPhase.Framing : TutorialPhase.EnterFlammable;
            phaseRemaining = cameraBlendSeconds;
            if (exerciseRoot != null) exerciseRoot.SetActive(true);
            ApplyGameplaySorting();
            if (cameraBlendSeconds > 0f && !player.ControlsLocked)
            {
                player.SetControlsLocked(true);
                ownsControlLock = player.ControlsLocked;
            }
            HideCue();
            onBegan.Invoke();
        }

        private void Update()
        {
            if (player == null) { HideCue(); return; }
            if (player.IsDying)
            {
                if (!observedDeath && HasBegun && !IsCompleted)
                {
                    // Death owns the player lock until respawn; do not release that lock here.
                    ownsControlLock = false;
                    CurrentPhase = TutorialPhase.EnterFlammable;
                    phaseRemaining = 0f;
                }
                observedDeath = true;
                HideCue();
                return;
            }
            observedDeath = false;
            if (ProgressBlocked || (player.ControlsLocked && !ownsControlLock))
            {
                HideCue();
                return;
            }

            if (CurrentPhase == TutorialPhase.WaitingToBegin)
            {
                if (!waitForWithering || (waterSurface != null && waterSurface.TreeWitherProgress >= 1f))
                    BeginTutorial();
                return;
            }
            if (!HasBegun) return;

            if (CurrentPhase == TutorialPhase.Framing)
            {
                phaseRemaining -= Time.deltaTime;
                if (phaseRemaining > 0f) return;
                ReleaseControlLock();
                CurrentPhase = TutorialPhase.EnterFlammable;
            }

            if (CurrentPhase == TutorialPhase.EnterFlammable)
            {
                if (targetFire != null && player.IsInsideFire && player.CurrentFire == targetFire)
                {
                    launchCountAtEntry = player.FireLaunchCount;
                    CurrentPhase = TutorialPhase.ExitFlammable;
                }
            }
            else if (CurrentPhase == TutorialPhase.ExitFlammable)
            {
                if (player.FireLaunchCount != launchCountAtEntry && !player.IsInsideFire
                    && !player.IsTravellingToFire && IsAtLanding() && player.CanConfirmCollectible)
                {
                    CurrentPhase = TutorialPhase.Completed;
                    phaseRemaining = completedCueSeconds;
                    ShowCue();
                    onCompleted.Invoke();
                    return;
                }
            }
            else if (IsCompleted)
            {
                phaseRemaining = Mathf.Max(0f, phaseRemaining - Time.deltaTime);
            }
            ShowCue();
        }

        private bool IsAtLanding()
        {
            if (landingArea == null || !landingArea.enabled || !landingArea.isTrigger
                || !landingArea.gameObject.activeInHierarchy) return false;
            Bounds bounds = landingArea.bounds;
            Vector3 position = player.transform.position;
            position.z = bounds.center.z;
            return bounds.Contains(position);
        }

        public void ResetTutorial()
        {
            ReleaseControlLock();
            CurrentPhase = TutorialPhase.Idle;
            HasBegun = false;
            waitForWithering = observedDeath = false;
            phaseRemaining = 0f;
            HideCue();
        }

        public void DismissCue()
        {
            if (!IsCompleted) return;
            phaseRemaining = 0f;
            HideCue();
        }

        private void ShowCue()
        {
            bool visible = CurrentPhase == TutorialPhase.EnterFlammable
                || CurrentPhase == TutorialPhase.ExitFlammable || (IsCompleted && phaseRemaining > 0f);
            if (CurrentPhase == TutorialPhase.EnterFlammable && targetFire != null && player != null)
                visible &= Vector2.Distance(player.transform.position, targetFire.AnchorPosition) <= cueActivationDistance;
            if (cueCanvas != null) cueCanvas.enabled = visible;
            if (keycapRoot != null) keycapRoot.SetActive(visible && !IsCompleted);
            if (!visible) return;

            if (CurrentPhase == TutorialPhase.EnterFlammable)
            {
                if (keyLabel != null) keyLabel.text = "X";
                if (instruction != null) instruction.text = "X로 가연물을 태우고 들어가세요";
            }
            else if (CurrentPhase == TutorialPhase.ExitFlammable)
            {
                if (keyLabel != null) keyLabel.text = "Z";
                if (instruction != null) instruction.text = "방향키와 Z로 빠져나오세요";
            }
            else
            {
                if (keyLabel != null) keyLabel.text = string.Empty;
                if (instruction != null) instruction.text = "불꽃 이동 성공!";
            }
        }

        private void HideCue()
        {
            if (cueCanvas != null) cueCanvas.enabled = false;
        }

        private void ReleaseControlLock()
        {
            if (!ownsControlLock) return;
            ownsControlLock = false;
            if (player != null && !player.IsDying) player.SetControlsLocked(false);
        }

        private void ApplyGameplaySorting()
        {
            if (!useGameplaySorting || ownsGameplaySorting || player == null) return;
            SpriteRenderer renderer = player.GetComponent<SpriteRenderer>();
            if (renderer == null) return;
            int layerId = SortingLayer.NameToID(gameplaySortingLayer);
            if (SortingLayer.IDToName(layerId) != gameplaySortingLayer)
            {
                Debug.LogWarning("Gameplay Sorting Layer를 확인해주세요: " + gameplaySortingLayer, this);
                return;
            }

            sortingRenderer = renderer;
            previousSortingLayerId = renderer.sortingLayerID;
            previousSortingOrder = renderer.sortingOrder;
            appliedSortingLayerId = layerId;
            appliedSortingOrder = gameplaySortingOrder;
            renderer.sortingLayerID = appliedSortingLayerId;
            renderer.sortingOrder = appliedSortingOrder;
            ownsGameplaySorting = true;
        }

        private void ReleaseGameplaySorting()
        {
            if (!ownsGameplaySorting) return;
            ownsGameplaySorting = false;
            if (sortingRenderer != null && sortingRenderer.sortingLayerID == appliedSortingLayerId
                && sortingRenderer.sortingOrder == appliedSortingOrder)
            {
                sortingRenderer.sortingLayerID = previousSortingLayerId;
                sortingRenderer.sortingOrder = previousSortingOrder;
            }
            sortingRenderer = null;
        }

        private void OnDisable()
        {
            ReleaseControlLock();
            ReleaseGameplaySorting();
            HideCue();
        }
    }
}
