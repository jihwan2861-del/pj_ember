using UnityEngine;
using UnityEngine.Events;

namespace EmberPrototype
{
    [ExecuteAlways]
    [DefaultExecutionOrder(1100)] // Sample the final ParallaxLayer bounds after its LateUpdate.
    [AddComponentMenu("Ember/Water Surface 2D")]
    public sealed class WaterSurface2D : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer waterRenderer;
        [SerializeField, Min(0.01f)] private float waterDepth = 5f;
        [Header("Past Reflection")]
        [SerializeField, Tooltip("수면에 비칠 밝은 과거 풍경. 비어 있으면 현재 화면 반사만 사용합니다.")]
        private Texture2D pastLandscape;
        [SerializeField, Tooltip("과거 하늘로 지울 현재 반사 물체입니다. 오염된 태양을 연결하면 과거 태양과 겹치지 않습니다.")]
        private SpriteRenderer replacedReflectionSource;
        [SerializeField, Tooltip("시듦 진행에 맞춰 현재 태양 반사를 다시 드러냅니다. 과거 태양도 Withering Reflection Layers에 등록하세요.")]
        private bool restorePresentSunWithWithering;
        [SerializeField, Tooltip("현재 지형 반사를 보존하고 흰 종이 영역만 과거 하늘로 바꿉니다.")]
        private bool preservePastGeography;
        [SerializeField, Tooltip("수면에만 합성할 과거 그림의 SpriteRenderer입니다. 최대 8개, Single/Full Rect PNG를 사용하고 카메라에서 제외한 레이어에 배치하세요.")]
        private SpriteRenderer[] pastReflectionLayers = new SpriteRenderer[0];
        [SerializeField, Tooltip("강이 처음 보일 때부터 밝은 과거 반사를 유지합니다. 위치에 따른 등장보다 우선합니다.")]
        private bool startWithPastReflection;
        [SerializeField, Tooltip("현재 풍경 SpriteRenderer를 연결하면 실제 그림의 위치·크기에 맞춰 과거 반사를 정렬합니다.")]
        private SpriteRenderer landscapeReference;
        [SerializeField] private Transform player;
        [SerializeField] private bool revealByPlayerPosition = true;
        [SerializeField] private float memoryRevealStartX = 68f;
        [SerializeField] private float memoryRevealEndX = 78f;
        [SerializeField, Min(0.01f), Tooltip("반사가 0에서 1까지 전환되는 데 필요한 게임 시간입니다.")]
        private float fadeSeconds = 2f;
        [SerializeField, Tooltip("과거 그림의 좌표 원점. River 위치와 수면 높이를 기준으로 한 로컬 오프셋입니다.")]
        private Vector2 pastWorldOrigin = Vector2.zero;
        [SerializeField, Tooltip("과거 그림을 펼칠 월드 좌표 너비와 높이입니다.")]
        private Vector2 pastWorldSize = new Vector2(32f, 8f);
        [SerializeField, Range(0f, 1f), Tooltip("편집 모드에서만 사용하는 과거 반사 미리보기입니다.")]
        private float previewPastBlend;
        [SerializeField, Tooltip("과거 반사가 95% 드러나면 한 번 호출됩니다. ResetPastReflection으로 초기화합니다.")]
        private UnityEvent onPastRevealed = new UnityEvent();
        [Header("Reflected Withering")]
        [SerializeField, Tooltip("나무·발판의 푸른 풀 등 이 Renderer들만 현재의 마른 반사로 전환합니다. Past Reflection Layers에도 등록해야 합니다.")]
        private SpriteRenderer[] witheringReflectionLayers = new SpriteRenderer[0];
        [SerializeField] private bool witherTreesByPlayerPosition;
        [SerializeField] private float treeWitherStartX = 80f;
        [SerializeField, Min(0.01f), Tooltip("푸른 나무 반사가 시드는 데 걸리는 게임 시간입니다. 배속을 적용합니다.")]
        private float treeWitherSeconds = 2f;
        [SerializeField, Range(0f, 1f), Tooltip("편집 모드에서 0=푸른 나무, 1=현재의 마른 나무 반사입니다.")]
        private float previewTreeWithering;
        [SerializeField] private UnityEvent onTreeWitheringStarted = new UnityEvent();
        [Header("Sun Before Water")]
        [SerializeField, Tooltip("태양 반사 전환 → 대기 → 나무·물빛 변화 순서로 분리합니다.")]
        private bool separateSunBeforeWithering;
        [SerializeField, Tooltip("태양만 먼저 전환할 과거 반사 Renderer입니다.")]
        private SpriteRenderer pastSunReflectionSource;
        [SerializeField, Min(0.01f)] private float sunTransitionSeconds = 2f;
        [SerializeField, Min(0f), Tooltip("태양 전환 완료 후 물 변화 전까지의 게임 시간입니다.")]
        private float holdAfterSunSeconds = 3f;
        [SerializeField, Range(0f, 1f)] private float previewSunTransition;
        [SerializeField] private UnityEvent onSunTransitionStarted = new UnityEvent();
        [Header("Reflected Sky")]
        [SerializeField, Tooltip("나무·풀이 시드는 진행에 맞춰 물에 비친 하늘과 물빛을 어둡게 합니다.")]
        private bool darkenSkyWithWithering;
        [SerializeField, Range(0.01f, 1f), Tooltip("연출이 끝났을 때 반사 하늘의 밝기 배율입니다. 낮을수록 어둡습니다.")]
        private float witheredSkyBrightness = 0.24f;
        [SerializeField, Range(0f, 1f), Tooltip("연출이 끝났을 때 반사 하늘의 채도입니다.")]
        private float witheredSkySaturation = 0.3f;
        [Header("External Memory Sequence")]
        [SerializeField] private bool useExternalMemorySequence;
        public bool UsesExternalMemorySequence => useExternalMemorySequence;
        private MaterialPropertyBlock properties;
        private float pastBlend;
        private float manualTarget;
        private bool useManualTarget;
        private bool pastRevealed;
        private bool treeWitheringStarted;
        private float treeWitherProgress;
        private bool reflectionSequenceStarted;
        private float reflectionSequenceElapsed;
        private float sunTransitionProgress;
        public float TreeWitherProgress => treeWitherProgress;
        public float SunTransitionProgress => separateSunBeforeWithering ? sunTransitionProgress : treeWitherProgress;
        private const int MaxPastLayers = 8;
        private static readonly int[] PastLayerTextures =
        {
            Shader.PropertyToID("_PastLayerTex0"), Shader.PropertyToID("_PastLayerTex1"),
            Shader.PropertyToID("_PastLayerTex2"), Shader.PropertyToID("_PastLayerTex3"),
            Shader.PropertyToID("_PastLayerTex4"), Shader.PropertyToID("_PastLayerTex5"),
            Shader.PropertyToID("_PastLayerTex6"), Shader.PropertyToID("_PastLayerTex7")
        };
        private static readonly int[] PastLayerRects =
        {
            Shader.PropertyToID("_PastLayerRect0"), Shader.PropertyToID("_PastLayerRect1"),
            Shader.PropertyToID("_PastLayerRect2"), Shader.PropertyToID("_PastLayerRect3"),
            Shader.PropertyToID("_PastLayerRect4"), Shader.PropertyToID("_PastLayerRect5"),
            Shader.PropertyToID("_PastLayerRect6"), Shader.PropertyToID("_PastLayerRect7")
        };
        private static readonly int[] PastLayerFlips =
        {
            Shader.PropertyToID("_PastLayerFlip0"), Shader.PropertyToID("_PastLayerFlip1"),
            Shader.PropertyToID("_PastLayerFlip2"), Shader.PropertyToID("_PastLayerFlip3"),
            Shader.PropertyToID("_PastLayerFlip4"), Shader.PropertyToID("_PastLayerFlip5"),
            Shader.PropertyToID("_PastLayerFlip6"), Shader.PropertyToID("_PastLayerFlip7")
        };
        private static readonly int[] PastLayerColors =
        {
            Shader.PropertyToID("_PastLayerColor0"), Shader.PropertyToID("_PastLayerColor1"),
            Shader.PropertyToID("_PastLayerColor2"), Shader.PropertyToID("_PastLayerColor3"),
            Shader.PropertyToID("_PastLayerColor4"), Shader.PropertyToID("_PastLayerColor5"),
            Shader.PropertyToID("_PastLayerColor6"), Shader.PropertyToID("_PastLayerColor7")
        };

        private void OnEnable()
        {
            pastBlend = Application.isPlaying ? (startWithPastReflection ? 1f : 0f) : previewPastBlend;
            manualTarget = startWithPastReflection ? 1f : 0f;
            useManualTarget = startWithPastReflection;
            pastRevealed = false;
            treeWitheringStarted = false;
            treeWitherProgress = Application.isPlaying ? 0f : previewTreeWithering;
            reflectionSequenceStarted = false;
            reflectionSequenceElapsed = 0f;
            sunTransitionProgress = Application.isPlaying ? 0f : previewSunTransition;
            if (useExternalMemorySequence) { SetMemoryReflection(0f); return; }
            ApplyProperties();
        }

        /// <summary>Switch to the past reflection using the configured fade duration.</summary>
        public void BeginPastReflection()
        {
            if (!Application.isPlaying)
            {
                SetPastBlend(1f);
                return;
            }
            useManualTarget = true;
            manualTarget = 1f;
        }

        /// <summary>Restore the initial reflection and allow the tree cue to play again.</summary>
        public void ResetPastReflection()
        {
            if (useExternalMemorySequence) { SetMemoryReflection(0f); return; }
            pastBlend = startWithPastReflection ? 1f : 0f;
            manualTarget = pastBlend;
            useManualTarget = startWithPastReflection;
            pastRevealed = false;
            treeWitheringStarted = false;
            treeWitherProgress = 0f;
            reflectionSequenceStarted = false;
            reflectionSequenceElapsed = 0f;
            sunTransitionProgress = 0f;
            if (!Application.isPlaying)
            {
                previewPastBlend = pastBlend;
                previewTreeWithering = 0f;
                previewSunTransition = 0f;
            }
            ApplyProperties();
        }

        /// <summary>Start the one-time reflection sequence; the tree cue waits for the sun beat when enabled.</summary>
        public void BeginTreeWithering()
        {
            if (useExternalMemorySequence) return;
            if (!Application.isPlaying || reflectionSequenceStarted) return;
            reflectionSequenceStarted = true;
            reflectionSequenceElapsed = 0f;
            if (separateSunBeforeWithering)
                onSunTransitionStarted.Invoke();
            else StartWaterWithering();
        }

        private void StartWaterWithering()
        {
            treeWitheringStarted = true;
            treeWitherProgress = 0f;
            onTreeWitheringStarted.Invoke();
        }

        /// <summary>Set an immediate preview, or hold a manual blend during play.</summary>
        public void SetPastBlend(float blend)
        {
            pastBlend = Mathf.Clamp01(blend);
            if (Application.isPlaying)
            {
                useManualTarget = true;
                manualTarget = pastBlend;
            }
            else previewPastBlend = pastBlend;
            ApplyProperties();
        }

        private void LateUpdate()
        {
            if (useExternalMemorySequence) { ApplyProperties(); return; }
            if (!Application.isPlaying)
            {
                pastBlend = previewPastBlend;
                treeWitherProgress = previewTreeWithering;
                sunTransitionProgress = previewSunTransition;
            }
            else if (!GameSessionControls.BlockGameplayInput)
            {
                float target = useManualTarget ? manualTarget : 0f;
                if (!useManualTarget && revealByPlayerPosition && player != null)
                    target = Mathf.InverseLerp(memoryRevealStartX,
                        Mathf.Max(memoryRevealStartX + 0.01f, memoryRevealEndX), player.position.x);
                pastBlend = Mathf.MoveTowards(pastBlend, target, Time.deltaTime / Mathf.Max(0.01f, fadeSeconds));
                if (!pastRevealed && HasPastVisual() && pastBlend >= 0.95f)
                {
                    pastRevealed = true;
                    onPastRevealed.Invoke();
                }
            }
            // Narration may lock movement, but its own visual cue must keep playing.
            if (Application.isPlaying && !GameSessionControls.IsPaused
                && !SceneRestartWipe.IsPlaying && !SceneFadeTransition.IsPlaying)
            {
                if (!reflectionSequenceStarted && witherTreesByPlayerPosition && player != null && player.position.x >= treeWitherStartX)
                    BeginTreeWithering();
                else if (reflectionSequenceStarted)
                {
                    reflectionSequenceElapsed += Time.deltaTime;
                    float waterStart = separateSunBeforeWithering
                        ? Mathf.Max(0.01f, sunTransitionSeconds) + Mathf.Max(0f, holdAfterSunSeconds) : 0f;
                    sunTransitionProgress = Mathf.Clamp01(reflectionSequenceElapsed / Mathf.Max(0.01f, sunTransitionSeconds));
                    if (!treeWitheringStarted && reflectionSequenceElapsed >= waterStart)
                        StartWaterWithering();
                    if (treeWitheringStarted)
                        treeWitherProgress = Mathf.Clamp01((reflectionSequenceElapsed - waterStart)
                            / Mathf.Max(0.01f, treeWitherSeconds));
                }
            }
            ApplyProperties();
        }

        public void SetMemoryReflection(float healthyAmount)
        {
            if (!useExternalMemorySequence) return;
            pastBlend = manualTarget = Mathf.Clamp01(healthyAmount);
            useManualTarget = true;
            treeWitherProgress = sunTransitionProgress = 1f - pastBlend;
            ApplyProperties();
        }

        private void ApplyProperties()
        {
            if (waterRenderer == null) return;
            if (properties == null) properties = new MaterialPropertyBlock();
            waterRenderer.GetPropertyBlock(properties);
            properties.SetFloat("_SurfaceY", transform.position.y);
            properties.SetFloat("_WaterDepth", waterDepth);
            properties.SetFloat("_ReflectionDarkening", darkenSkyWithWithering ? treeWitherProgress : 0f);
            properties.SetFloat("_WitheredSkyBrightness", witheredSkyBrightness);
            properties.SetFloat("_WitheredSkySaturation", witheredSkySaturation);
            properties.SetVector("_PlayerReflectionPosition", player != null
                ? new Vector4(player.position.x, player.position.y, 0f, 1f) : Vector4.zero);
            if (pastLandscape != null) properties.SetTexture("_PastTex", pastLandscape);
            properties.SetFloat("_HasPastLandscape", pastLandscape != null ? 1f : 0f);
            properties.SetFloat("_PastSkyOnly", preservePastGeography ? 1f : 0f);
            if (replacedReflectionSource != null)
            {
                Bounds replacement = replacedReflectionSource.bounds;
                replacement.Expand(0.2f);
                properties.SetVector("_PastSkyReplaceRect", new Vector4(replacement.min.x, replacement.min.y,
                    replacement.size.x, replacement.size.y));
                properties.SetFloat("_ReplacePastSky", restorePresentSunWithWithering
                    ? 1f - SunTransitionProgress : 1f);
            }
            else properties.SetFloat("_ReplacePastSky", 0f);
            int layerCount = ApplyPastLayers();
            properties.SetFloat("_PastBlend", pastLandscape != null || layerCount > 0 ? pastBlend : 0f);
            Vector4 pastWorldRect;
            bool repeatPastX = false;
            if (landscapeReference != null && landscapeReference.sprite != null)
            {
                ParallaxLayer parallax = landscapeReference.GetComponent<ParallaxLayer>();
                repeatPastX = parallax != null && parallax.isActiveAndEnabled && parallax.RepeatsHorizontally;
                Bounds bounds = landscapeReference.bounds;
                pastWorldRect = new Vector4(bounds.min.x, bounds.min.y,
                    Mathf.Max(0.01f, bounds.size.x), Mathf.Max(0.01f, bounds.size.y));
            }
            else
            {
                pastWorldRect = new Vector4(
                    transform.position.x + pastWorldOrigin.x, transform.position.y + pastWorldOrigin.y,
                    Mathf.Max(0.01f, pastWorldSize.x), Mathf.Max(0.01f, pastWorldSize.y));
            }
            properties.SetVector("_PastWorldRect", pastWorldRect);
            properties.SetFloat("_PastRepeatX", repeatPastX ? 1f : 0f);
            waterRenderer.SetPropertyBlock(properties);
        }

        private bool HasPastVisual()
        {
            if (pastLandscape != null) return true;
            if (pastReflectionLayers == null) return false;
            foreach (SpriteRenderer layer in pastReflectionLayers)
                if (IsFullTextureLayer(layer)) return true;
            return false;
        }

        private int ApplyPastLayers()
        {
            int count = 0;
            if (pastReflectionLayers != null)
                foreach (SpriteRenderer layer in pastReflectionLayers)
                {
                    if (!IsFullTextureLayer(layer)) continue;
                    Bounds bounds = layer.bounds;
                    properties.SetTexture(PastLayerTextures[count], layer.sprite.texture);
                    properties.SetColor(PastLayerColors[count], layer.color);
                    properties.SetVector(PastLayerRects[count], new Vector4(bounds.min.x, bounds.min.y,
                        Mathf.Max(0.01f, bounds.size.x), Mathf.Max(0.01f, bounds.size.y)));
                    properties.SetVector(PastLayerFlips[count],
                        new Vector4(layer.flipX ? 1f : 0f, layer.flipY ? 1f : 0f,
                            separateSunBeforeWithering && layer == pastSunReflectionSource
                                ? sunTransitionProgress : (IsWitheringLayer(layer) ? treeWitherProgress : 0f), 1f));
                    if (++count >= MaxPastLayers) break;
                }
            for (int i = count; i < MaxPastLayers; i++)
                properties.SetVector(PastLayerFlips[i], Vector4.zero);
            properties.SetFloat("_PastLayerCount", count);
            return count;
        }

        private bool IsWitheringLayer(SpriteRenderer layer)
        {
            if (witheringReflectionLayers == null) return false;
            foreach (SpriteRenderer candidate in witheringReflectionLayers)
                if (candidate == layer) return true;
            return false;
        }

        private static bool IsFullTextureLayer(SpriteRenderer layer)
        {
            if (layer == null || layer.sprite == null || layer.sprite.packed) return false;
            Sprite sprite = layer.sprite;
            Texture2D texture = sprite.texture;
            Rect rect = sprite.rect;
            return texture != null && Mathf.Approximately(rect.x, 0f) && Mathf.Approximately(rect.y, 0f)
                && Mathf.Approximately(rect.width, texture.width) && Mathf.Approximately(rect.height, texture.height);
        }
    }
}
