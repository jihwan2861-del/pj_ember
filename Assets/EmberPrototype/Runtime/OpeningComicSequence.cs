using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace EmberPrototype
{
    [DefaultExecutionOrder(-1000)]
    [AddComponentMenu("Ember/Opening Comic Sequence")]
    [DisallowMultipleComponent]
    public sealed class OpeningComicSequence : MonoBehaviour
    {
        [SerializeField] private CanvasGroup comicPanel;
        [SerializeField] private FlamePlayerController player;
        [SerializeField] private CircularOpeningWorld circularOpening;
        [SerializeField] private StorySectionCamera storyCamera;
        [SerializeField] private OpeningSunTransition openingSun;
        [Header("Opening Darkness")]
        [SerializeField] private Light2D globalLight;
        [SerializeField] private bool turnOffGlobalLight = true;
        [SerializeField] private bool createPlayerLight = true;
        [SerializeField, Min(0f)] private float playerLightIntensity = 1.25f;
        [SerializeField] private Color playerLightColor = new Color(1f, 0.48f, 0.16f, 1f);
        [SerializeField, Min(0f)] private float playerLightInnerRadius = 1.2f;
        [SerializeField, Min(0.1f)] private float playerLightOuterRadius = 6.5f;
        [Header("Timing — Real Seconds")]
        [SerializeField, Min(0f)] private float holdSeconds = 3.5f;
        [SerializeField, Min(0f)] private float fadeOutSeconds = 0.85f;

        private bool ownsPlayerLock;
        private bool sequenceFinished;
        private GameObject playerLightObject;
        private Light2D playerLight;

        private void Awake()
        {
            if (turnOffGlobalLight && globalLight != null)
                globalLight.enabled = false;

            CreatePlayerLight();
        }

        private void LateUpdate()
        {
            if (playerLight == null || player == null) return;
            Vector3 position = player.transform.position;
            position.z = 0f;
            playerLight.transform.position = position;
        }

        private void CreatePlayerLight()
        {
            if (!Application.isPlaying || !createPlayerLight || player == null) return;

            playerLightObject = new GameObject("Story Opening - Ember Light");
            SceneManager.MoveGameObjectToScene(playerLightObject, gameObject.scene);
            playerLightObject.transform.position = player.transform.position;
            playerLight = playerLightObject.AddComponent<Light2D>();
            playerLight.lightType = Light2D.LightType.Point;
            playerLight.color = playerLightColor;
            playerLight.intensity = playerLightIntensity;
            playerLight.pointLightOuterRadius = Mathf.Max(0.1f, playerLightOuterRadius);
            playerLight.pointLightInnerRadius = Mathf.Clamp(playerLightInnerRadius, 0f, playerLight.pointLightOuterRadius);

            SortingLayer[] sortingLayers = SortingLayer.layers;
            int[] layerIds = new int[sortingLayers.Length];
            for (int i = 0; i < sortingLayers.Length; i++)
                layerIds[i] = sortingLayers[i].id;
            playerLight.targetSortingLayers = layerIds;
        }

        private IEnumerator Start()
        {
            if (comicPanel != null)
            {
                comicPanel.alpha = 1f;
                comicPanel.interactable = false;
                comicPanel.blocksRaycasts = false;
            }

            // Start runs after every scene object's Awake, so the player's Rigidbody2D
            // is ready when the opening takes its temporary input lock.
            if (player != null && !player.ControlsLocked)
            {
                player.SetControlsLocked(true);
                ownsPlayerLock = player.ControlsLocked;
            }
            while (SceneFadeTransition.IsPlaying || SceneRestartWipe.IsPlaying || GameSessionControls.IsPaused)
                yield return null;

            float elapsed = 0f;
            while (elapsed < Mathf.Max(0f, holdSeconds))
            {
                if (!GameSessionControls.IsPaused && !SceneFadeTransition.IsPlaying && !SceneRestartWipe.IsPlaying)
                    elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            elapsed = 0f;
            float fadeDuration = Mathf.Max(0f, fadeOutSeconds);
            while (elapsed < fadeDuration)
            {
                if (!GameSessionControls.IsPaused && !SceneFadeTransition.IsPlaying && !SceneRestartWipe.IsPlaying)
                {
                    elapsed += Time.unscaledDeltaTime;
                    if (comicPanel != null)
                        comicPanel.alpha = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / fadeDuration));
                }
                yield return null;
            }

            if (comicPanel != null) comicPanel.alpha = 0f;
            ReleasePlayerLock();
            while (player != null && (player.ControlsLocked || player.IsDying || !player.CanStartTrialCeremony))
                yield return null;

            if (circularOpening != null)
            {
                circularOpening.BeginWalking();
                if (!circularOpening.IsWalking)
                {
                    Debug.LogError("연결된 Circular Opening을 시작하지 못했습니다. Player와 AngleRotator 상태를 확인해주세요.", this);
                    yield break;
                }
            }

            if (openingSun != null) openingSun.PinCorruptedSunToCamera();
            if (storyCamera != null) storyCamera.BeginFollowing();
            sequenceFinished = true;
        }

        private void ReleasePlayerLock()
        {
            if (!ownsPlayerLock) return;
            ownsPlayerLock = false;
            if (player != null && !player.IsDying) player.SetControlsLocked(false);
        }

        private void OnDisable()
        {
            if (!sequenceFinished) ReleasePlayerLock();
        }

        private void OnDestroy()
        {
            if (playerLightObject != null) Destroy(playerLightObject);
        }
    }
}
