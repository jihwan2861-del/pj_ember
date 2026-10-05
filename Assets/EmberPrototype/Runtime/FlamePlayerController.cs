using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

namespace EmberPrototype
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerAbilityUnlocks))]
    [RequireComponent(typeof(BurstDashAbility))]
    [RequireComponent(typeof(FlameShotAbility))]
    [RequireComponent(typeof(PlayerAbilityInputRouter))]
    public sealed class FlamePlayerController : MonoBehaviour
    {
        private enum FlameState { Free, Bursting, BurstDashing, Travelling, Anchored }

        [Header("Platforming")]
        [Tooltip("좌우 입력으로 이동할 때 목표로 삼는 수평 속도입니다. (월드 단위/초)")]
        [SerializeField, Min(0f)] private float moveSpeed = 7f;
        [Tooltip("점프를 시작할 때 적용되는 기본 위쪽 속도입니다. 상승 중 중력 보정에 따라 최종 점프 높이는 달라질 수 있습니다.")]
        [SerializeField, Min(0f)] private float jumpSpeed = 12f;
        [Tooltip("땅을 떠난 뒤 공중에서 추가로 점프할 수 있는 횟수입니다. 0이면 공중 점프를 사용할 수 없습니다.")]
        [SerializeField, Min(0)] private int maxAirJumps = 1;
        [Tooltip("플레이어 발밑에서 바닥을 검사하는 최대 거리입니다. 접지 판정이 불안정하면 조금 늘려보세요.")]
        [SerializeField, Min(0f)] private float groundProbeDistance = 0.12f;
        [Tooltip("발판 끝에서 떨어진 직후에도 점프를 받아주는 유예 시간입니다. (초)")]
        [SerializeField, Min(0f)] private float coyoteTime = 0.12f;
        [Tooltip("점프 입력을 미리 저장해 착지 직후 점프하게 해주는 시간입니다. (초)")]
        [SerializeField, Min(0f)] private float jumpBuffer = 0.12f;
        [Tooltip("땅에서 좌우 입력 방향으로 가속하는 정도입니다. 값이 클수록 목표 속도에 빨리 도달합니다.")]
        [SerializeField, Min(0f)] private float groundAcceleration = 65f;
        [Tooltip("땅에서 입력을 놓았을 때 수평 속도가 줄어드는 정도입니다.")]
        [SerializeField, Min(0f)] private float groundDeceleration = 85f;
        [Tooltip("공중에서 좌우 입력 방향으로 가속하는 정도입니다.")]
        [SerializeField, Min(0f)] private float airAcceleration = 22f;
        [Tooltip("공중에서 입력을 놓았을 때 수평 속도가 줄어드는 정도입니다.")]
        [SerializeField, Min(0f)] private float airDeceleration = 14f;
        [Tooltip("불 발사나 대시로 일반 이동 속도를 넘었을 때, 같은 방향 입력을 유지하며 추진 속도를 소진하는 정도입니다.")]
        [SerializeField, Min(0f)] private float overspeedDeceleration = 8f;
        [Tooltip("이동 중 반대 방향을 눌러 방향을 바꿀 때 적용되는 가속도입니다.")]
        [SerializeField, Min(0f)] private float turnAcceleration = 95f;
        [Tooltip("낙하 중 허용되는 최대 아래쪽 속도입니다.")]
        [SerializeField, Min(0f)] private float maxFallSpeed = 22f;
        [Tooltip("착지가 임박한 낙하 중 Z 입력은 공중 점프 대신 착지 점프로 예약하는 최대 시간입니다. 0이면 즉시 공중 점프합니다.")]
        [SerializeField, Min(0f)] private float landingJumpBufferTime = 0.07f;

        [Header("Launch Control")]
        [Tooltip("불에서 발사한 직후 추진 방향을 유지하는 시간입니다. 반대 방향 제동은 이 시간에도 허용됩니다.")]
        [SerializeField, Min(0f)] private float fireLaunchControlLockTime = 0.25f;
        [Tooltip("대시가 끝난 뒤 추진 방향을 유지하는 시간입니다. 반대 방향 제동은 이 시간에도 허용됩니다.")]
        [SerializeField, Min(0f)] private float burstDashExitControlLockTime = 0.08f;

        [Header("Celeste-Style Jump Feel")]
        [Tooltip("점프 상승 초반에 적용하는 중력 배수입니다. 값이 클수록 더 빠르게 올라가며, 기본 점프 높이는 보정됩니다.")]
        [SerializeField, Range(1f, 2f)] private float riseGravityMultiplier = 1.4f;
        [Tooltip("점프 정점 부근에서 적용하는 중력 배수입니다. 낮출수록 정점에서 더 오래 체공합니다.")]
        [SerializeField, Range(0.1f, 1f)] private float apexGravityMultiplier = 0.5f;
        [Tooltip("낙하 중 적용하는 중력 배수입니다. 값이 클수록 더 빠르게 떨어집니다.")]
        [SerializeField, Min(1f)] private float fallGravityMultiplier = 1.3f;
        [Tooltip("점프 버튼을 누르는 동안 수직 속도의 절댓값이 이 값 이하이면 정점 구간용 낮은 중력을 적용합니다.")]
        [SerializeField, Min(0f)] private float apexVelocityThreshold = 2.2f;
        [Tooltip("점프 중 머리가 모서리에 걸릴 때 옆으로 이동해 충돌을 보정하는 최대 거리입니다.")]
        [SerializeField, Min(0f)] private float cornerCorrectionDistance = 0.24f;
        [Tooltip("모서리 보정 위치를 탐색하는 간격입니다. 작을수록 더 세밀하게 검사합니다.")]
        [SerializeField, Min(0.01f)] private float cornerCorrectionStep = 0.04f;
        [Tooltip("벽에 부딪힌 직후 벽을 벗어나면 수평 이동 속도를 잠시 유지해주는 시간입니다.")]
        [SerializeField, Min(0f)] private float wallSpeedRetentionTime = 0.06f;

        [Header("Respawn")]
        [Tooltip("리스폰 위치로 사용할 Transform입니다. 비워두면 씬 시작 시 플레이어 위치를 사용합니다.")]
        [SerializeField] private Transform respawnPoint;

        [Header("Fire Absorb (X)")]
        [Tooltip("불꽃 흡수 대상을 고를 때 입력 방향과 대상 방향이 얼마나 일치해야 하는지 정합니다. 1에 가까울수록 방향이 정확히 맞아야 합니다.")]
        [SerializeField, Range(-1f, 1f)] private float fireTargetDirectionDot = 0.35f;

        [Header("Fire Absorb Target Line")]
        [Tooltip("불꽃 안에서 X로 이동할 수 있는 목표를 안내하는 선을 표시합니다.")]
        [SerializeField] private bool showAbsorbTargetLine = true;
        [Tooltip("목표 안내선의 기본 두께입니다. (월드 단위)")]
        [SerializeField, Min(0.001f)] private float absorbTargetLineWidth = 0.045f;
        [Tooltip("목표 안내선의 기본 색과 투명도입니다.")]
        [SerializeField] private Color absorbTargetLineColor = new Color(1f, 0.68f, 0.16f, 0.9f);
        [Tooltip("목표 안내선에 사용할 머티리얼입니다. 비워두면 스크립트가 기본 효과용 머티리얼을 만듭니다.")]
        [SerializeField] private Material absorbTargetLineMaterial;
        [Tooltip("플레이어의 Sorting Order를 기준으로 안내선을 앞쪽에 그릴 순서 차이입니다.")]
        [SerializeField, Min(0)] private int absorbTargetLineSortingOrderOffset = 10;
        [Tooltip("곡선 안내선을 나누어 그릴 구간 수입니다. 값이 클수록 곡선이 부드러워집니다.")]
        [SerializeField, Range(4, 24)] private int absorbTargetLineSegments = 14;
        [Tooltip("불꽃 안내선이 좌우로 흔들리는 최대 폭입니다. 0이면 흔들리지 않습니다. (월드 단위)")]
        [SerializeField, Min(0f)] private float absorbTargetLineWobbleAmount = 0.035f;
        [Tooltip("불꽃 안내선의 불규칙한 흔들림 속도입니다.")]
        [SerializeField, Min(0.01f)] private float absorbTargetLineWobbleSpeed = 2.8f;
        [Tooltip("불꽃 안내선의 텍스처가 선을 따라 흐르는 속도입니다.")]
        [SerializeField, Min(0f)] private float absorbTargetLineTextureScrollSpeed = 0.6f;
        [Tooltip("안내선 바깥쪽 부드러운 광선의 두께 배수입니다. 기본 선 두께에 곱해집니다.")]
        [SerializeField, Range(2f, 5f)] private float absorbTargetLineGlowWidthMultiplier = 3.5f;

        [Header("Fire Absorb Target Glow")]
        [Tooltip("현재 선택된 불꽃을 강조하는 빛을 표시합니다.")]
        [SerializeField] private bool showAbsorbTargetGlow = true;
        [Tooltip("선택된 불꽃을 강조하는 빛의 색입니다.")]
        [SerializeField] private Color absorbTargetGlowColor = new Color(1f, 0.75f, 0.3f);
        [Tooltip("목표 강조 빛의 기본 밝기입니다.")]
        [SerializeField, Min(0f)] private float absorbTargetGlowIntensity = 1.1f;
        [Tooltip("빛이 맥동할 때 기본 밝기에서 변하는 폭입니다.")]
        [SerializeField, Min(0f)] private float absorbTargetGlowPulseAmount = 0.3f;
        [Tooltip("목표 강조 빛이 맥동하는 주기 속도입니다.")]
        [SerializeField, Min(0f)] private float absorbTargetGlowPulseSpeed = 2.5f;
        [Tooltip("목표 강조 빛의 바깥쪽 반경입니다. (월드 단위)")]
        [SerializeField, Min(0.1f)] private float absorbTargetGlowRadius = 1.4f;

        [Header("Ignition Burst (C)")]
        [Tooltip("C를 눌렀을 때 원형 폭발로 불을 붙이는 범위입니다. (월드 단위)")]
        [SerializeField, Min(0.1f)] private float ignitionBurstRadius = 1.5f;
        [Tooltip("C 폭발 직후 불의 고리 상태가 유지되는 시간입니다. 이 시간 안에 Z 대시나 X 조준을 시작할 수 있습니다. (초)")]
        [InspectorName("Ignition Burst Pause Time")]
        [SerializeField, Min(0f)] private float ignitionBurstChargeTime = 0.5f;
        [Tooltip("원형 폭발 시각 효과가 커지며 사라지는 시간입니다. (초)")]
        [SerializeField, Min(0.01f)] private float ignitionBurstVisualTime = 0.18f;

        [Header("Fire Network")]
        [Tooltip("선택한 불꽃을 향해 이동할 때 도달하는 최대 속도입니다.")]
        [SerializeField, Min(0f)] private float fireTravelSpeed = 20f;
        [Tooltip("불꽃 이동이 정지 상태에서 최대 속도까지 가속되는 시간입니다. 0이면 즉시 최대 속도로 이동합니다. (초)")]
        [SerializeField, Min(0f)] private float fireTravelAccelerationTime = 0.16f;
        [Tooltip("불꽃 흡수 대상으로 탐색할 최대 거리입니다. (월드 단위)")]
        [SerializeField, Min(0f)] private float fireTravelRange = 9f;
        [Tooltip("불꽃 이동 경로를 막는 장애물 레이어입니다. 아무 레이어도 선택하지 않으면 전체 레이어를 검사합니다.")]
        [SerializeField] private LayerMask fireTravelObstacleMask = ~0;
        [Tooltip("불꽃 이동 경로를 검사할 때 사용하는 플레이어 콜라이더 크기 배율입니다. 작게 설정하면 바닥이나 모서리에 덜 걸리지만 좁은 틈을 통과 가능하다고 판정할 수 있습니다.")]
        [SerializeField, Range(0.25f, 1f)] private float fireTravelCollisionScale = 0.75f;
        [Tooltip("직선 경로가 막혔을 때 모서리를 피해 우회할 최대 옆 방향 거리입니다. 0이면 우회 보정을 끕니다. (월드 단위)")]
        [SerializeField, Min(0f)] private float fireTravelCornerAssistDistance = 1f;
        [Tooltip("불꽃 이동 우회 경로를 탐색할 때 옆으로 이동해 검사하는 간격입니다. (월드 단위)")]
        [SerializeField, Min(0.05f)] private float fireTravelCornerAssistStep = 0.25f;
        [Tooltip("플레이어가 불꽃 안에 있을 때 이 거리 안의 불은 다음 목표로 선택될 우선순위가 높아집니다. (월드 단위)")]
        [SerializeField, Min(0.1f)] private float fireNetworkStepRange = 1.65f;
        [Tooltip("가까운 불꽃을 우선 목표로 삼기 위해 필요한 최소 방향 일치도입니다. 값이 높을수록 입력 방향과 더 비슷해야 합니다.")]
        [SerializeField, Range(-1f, 1f)] private float fireNetworkDirectionDot = 0.45f;
        [Tooltip("불꽃 안에서 방향키를 눌러 튀어나올 때 적용되는 속도입니다.")]
        [SerializeField, Min(0f)] private float launchSpeed = 15f;
        [Tooltip("불꽃에서 튀어나온 뒤 잔상 효과를 유지하는 시간입니다. (초)")]
        [SerializeField, Min(0f)] private float launchAfterimageDuration = 0.22f;
        [Tooltip("불꽃에서 나올 위치가 벽에 막히면 옆으로 이동해 안전한 위치를 찾는 최대 보정 거리입니다. (월드 단위)")]
        [SerializeField, Min(0f)] private float fireLaunchCornerAssistDistance = 0.24f;

        private Rigidbody2D body;
        private Collider2D bodyCollider;
        private FlameState state;
        private FlammableTile targetFire;
        private FlammableTile absorbTargetPreview;
        private LineRenderer absorbTargetLine;
        private LineRenderer absorbTargetLineGlow;
        private LineRenderer absorbTargetLineCore;
        private Light2D absorbTargetGlowLight;
        private Material runtimeAbsorbTargetLineMaterial;
        private Texture2D runtimeAbsorbTargetLineTexture;
        private Vector3[] absorbTargetLinePositions;
        private FlammableTile travelSourceFire;
        private float horizontalInput;
        private float coyoteRemaining;
        private float jumpRemaining;
        private float launchProtection;
        private float burstChargeRemaining;
        private float fireTravelCurrentSpeed;
        private Vector2 anchoredAimDirection;
        private bool jumpReleased;
        private bool jumpHeld;
        private bool normalJump;
        private bool jumpRiseActive;
        private bool cornerCorrectionUsed;
        private int airJumpsRemaining;
        private bool burstAvailable = true;
        private int facingDirection = 1;
        private Vector3 initialScale;
        private float initialGravity;
        private CollisionDetectionMode2D initialCollisionDetectionMode;
        private Vector2 initialSpawnPosition;
        private float upwardVelocityBeforeCollision;
        private float horizontalVelocityBeforeCollision;
        private float retainedWallSpeedX;
        private float wallSpeedRetentionRemaining;
        private GameObject activeBurstEffect;
        private PlayerFlameFeedback flameFeedback;
        private PlayerAfterimageEffect afterimageEffect;
        private BurstDashAbility burstDashAbility;
        private FlameShotAbility flameShotAbility;
        private PlayerAbilityUnlocks abilityUnlocks;
        private bool groundStateInitialized;
        private bool wasGrounded;
        private Vector2 appliedPlatformVelocity;
        private bool controlsLocked;
        private RigidbodyConstraints2D constraintsBeforeControlLock;
        private float gravityBeforeControlLock;
        private TrialAltar nearbyTrialAltar;
        private Vector2 fireTravelProbeSize;
        private Vector2 fireTravelProbeOffset;
        private Vector2 fireTravelWaypoint;
        private bool hasFireTravelWaypoint;
        private readonly RaycastHit2D[] fireTravelCastHits = new RaycastHit2D[8];
        private readonly RaycastHit2D[] wallClearanceCastHits = new RaycastHit2D[8];
        private readonly RaycastHit2D[] groundHits = new RaycastHit2D[16];
        private Collider2D[] fireTargetHits = new Collider2D[32];

        public bool ControlsLocked => controlsLocked;
        public float NormalMoveSpeed => moveSpeed;
        public bool CanStartTrialCeremony => isActiveAndEnabled && !controlsLocked && state == FlameState.Free;
        internal bool IsAirborneForAnimation =>
            (state == FlameState.Free || state == FlameState.Bursting || state == FlameState.BurstDashing)
            && groundStateInitialized
            && !wasGrounded;
        internal bool IsInsideFire => state == FlameState.Anchored;
        internal bool IsInIgnitionRing => state == FlameState.Bursting;
        internal bool CanUseFreeAbilities => !controlsLocked && state == FlameState.Free;
        internal bool IsRingShotAiming => flameShotAbility != null && flameShotAbility.IsAiming;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<Collider2D>();

            if (bodyCollider == null)
            {
                Debug.LogError("FlamePlayerController requires a Collider2D on the same GameObject.", this);
                enabled = false;
                return;
            }

            abilityUnlocks = GetComponent<PlayerAbilityUnlocks>();
            if (abilityUnlocks == null) abilityUnlocks = gameObject.AddComponent<PlayerAbilityUnlocks>();
            burstDashAbility = GetComponent<BurstDashAbility>();
            if (burstDashAbility == null) burstDashAbility = gameObject.AddComponent<BurstDashAbility>();
            flameShotAbility = GetComponent<FlameShotAbility>();
            if (flameShotAbility == null) flameShotAbility = gameObject.AddComponent<FlameShotAbility>();
            if (GetComponent<PlayerAbilityInputRouter>() == null)
                gameObject.AddComponent<PlayerAbilityInputRouter>();

            initialScale = transform.localScale;
            initialGravity = body.gravityScale;
            initialCollisionDetectionMode = body.collisionDetectionMode;
            initialSpawnPosition = transform.position;
            Bounds colliderBounds = bodyCollider.bounds;
            fireTravelProbeSize = colliderBounds.size;
            fireTravelProbeOffset = (Vector2)colliderBounds.center - body.position;
            airJumpsRemaining = maxAirJumps;
            flameFeedback = GetComponent<PlayerFlameFeedback>();
            if (flameFeedback == null) flameFeedback = gameObject.AddComponent<PlayerFlameFeedback>();
            afterimageEffect = GetComponent<PlayerAfterimageEffect>();
            if (afterimageEffect == null) afterimageEffect = gameObject.AddComponent<PlayerAfterimageEffect>();
            CreateAbsorbTargetLine();
        }

        private void OnDisable()
        {
            flameShotAbility?.CancelAim();
            burstDashAbility?.Cancel();
            if (nearbyTrialAltar != null) nearbyTrialAltar.CancelCeremonyFor(this);
            SetControlsLocked(false);
            StopAllCoroutines();
            ClearBurstEffect();
            SetAbsorbTargetPreview(null);
            if (absorbTargetGlowLight != null) absorbTargetGlowLight.enabled = false;
            flameFeedback?.HideLaunchRing();
            afterimageEffect?.StopTrail();
        }

        private void Update()
        {
            if (controlsLocked) return;

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                horizontalInput = jumpRemaining = 0f;
                jumpHeld = false;
                jumpReleased = true;
                return;
            }
            Vector2 heldDirection = ReadHeldDirection();
            // Walking is one-dimensional; aiming diagonally must not slow it down.
            horizontalInput = (keyboard.rightArrowKey.isPressed ? 1f : 0f)
                - (keyboard.leftArrowKey.isPressed ? 1f : 0f);
            jumpHeld = keyboard.zKey.isPressed;
            if (keyboard.zKey.wasPressedThisFrame)
            {
                jumpReleased = false;
                if (state == FlameState.Free || state == FlameState.BurstDashing)
                    jumpRemaining = jumpBuffer;
            }
            if (keyboard.zKey.wasReleasedThisFrame) jumpReleased = true;

            if (state == FlameState.Anchored)
            {
                body.linearVelocity = Vector2.zero;
                Vector2 anchoredDirection = heldDirection;
                if (anchoredDirection != Vector2.zero) anchoredAimDirection = anchoredDirection;
                if (anchoredDirection.x != 0f) facingDirection = anchoredDirection.x > 0f ? 1 : -1;
                UpdateAbsorbTargetPreview(anchoredAimDirection);
                return;
            }

            if (state != FlameState.Free) return;

            if (horizontalInput != 0f) facingDirection = horizontalInput > 0f ? 1 : -1;
            UpdateAbsorbTargetPreview(heldDirection);
        }

        private void FixedUpdate()
        {
            if (controlsLocked) { appliedPlatformVelocity = Vector2.zero; return; }
            if (state != FlameState.Free) appliedPlatformVelocity = Vector2.zero;

            // A jump pressed near the end of a dash survives until Free, but expires normally.
            if (state != FlameState.Free)
                jumpRemaining = Mathf.Max(0f, jumpRemaining - Time.fixedDeltaTime);

            if (state == FlameState.Bursting)
            {
                body.linearVelocity = Vector2.zero;
                if (!IsRingShotAiming) burstChargeRemaining -= Time.fixedDeltaTime;
                if (burstChargeRemaining <= 0f)
                {
                    state = FlameState.Free;
                    body.gravityScale = initialGravity;
                    flameFeedback.HideLaunchRing();
                }
                return;
            }

            if (state == FlameState.BurstDashing)
            {
                float dashSpeed = burstDashAbility.AdvanceSpeed(Time.fixedDeltaTime);
                float dashStepDistance = dashSpeed * Time.fixedDeltaTime;
                if (TryCastFireTravelStep(burstDashAbility.Direction, dashStepDistance))
                {
                    EndBurstDash(true);
                    return;
                }

                body.linearVelocity = burstDashAbility.Direction * dashSpeed;
                if (burstDashAbility.AdvanceDuration(Time.fixedDeltaTime)) EndBurstDash(false);
                return;
            }

            if (state == FlameState.Free)
            {
                // Solve walking and jump gravity relative to the platform, then add its motion once.
                Vector2 velocity = body.linearVelocity - appliedPlatformVelocity;
                appliedPlatformVelocity = Vector2.zero;
                RaycastHit2D groundHit = default;
                bool grounded = velocity.y <= 0.1f && TryFindGround(groundProbeDistance, out groundHit);
                PathMovingBlock platform = grounded && groundHit.rigidbody != null
                    ? groundHit.rigidbody.GetComponent<PathMovingBlock>() : null;
                if (!groundStateInitialized)
                {
                    groundStateInitialized = true;
                    wasGrounded = grounded;
                }
                else if (grounded && !wasGrounded)
                {
                    flameFeedback.PlayLand();
                }

                if (grounded)
                {
                    coyoteRemaining = coyoteTime;
                    airJumpsRemaining = maxAirJumps;
                    burstAvailable = true;
                    cornerCorrectionUsed = false;
                    wallSpeedRetentionRemaining = 0f;
                }
                else coyoteRemaining -= Time.fixedDeltaTime;

                bool jumpedThisStep = false;
                launchProtection -= Time.fixedDeltaTime;
                bool brakingLaunch = horizontalInput * velocity.x < 0f;
                if (launchProtection <= 0f || brakingLaunch)
                {
                    float targetSpeed = horizontalInput * moveSpeed;
                    float acceleration = SelectHorizontalAcceleration(velocity.x, targetSpeed, grounded);
                    velocity.x = Mathf.MoveTowards(velocity.x, targetSpeed, acceleration * Time.fixedDeltaTime);
                    ApplyWallSpeedRetention(ref velocity);
                }
                if (jumpRemaining > 0f && coyoteRemaining > 0f)
                {
                    velocity.y = GetJumpLaunchSpeed();
                    jumpRemaining = coyoteRemaining = 0f;
                    normalJump = true;
                    jumpRiseActive = true;
                    cornerCorrectionUsed = false;
                    jumpedThisStep = true;
                    flameFeedback.PlayJump(false);
                }
                else if (jumpRemaining > 0f && airJumpsRemaining > 0 && !ShouldWaitForLanding(velocity))
                {
                    velocity.y = GetJumpLaunchSpeed();
                    jumpRemaining = 0f;
                    airJumpsRemaining--;
                    normalJump = true;
                    jumpRiseActive = true;
                    cornerCorrectionUsed = false;
                    jumpedThisStep = true;
                    flameFeedback.PlayJump(true);
                }
                jumpRemaining -= Time.fixedDeltaTime;
                if (normalJump && (jumpReleased || !jumpHeld) && velocity.y > 0f)
                {
                    velocity.y = Mathf.Min(velocity.y * 0.5f, jumpSpeed * 0.5f);
                    normalJump = false;
                    jumpRiseActive = false;
                }
                if (velocity.y <= 0f)
                {
                    normalJump = false;
                    jumpRiseActive = false;
                }
                velocity.y = Mathf.Max(velocity.y, -maxFallSpeed);
                UpdateJumpGravity(velocity.y, grounded);
                upwardVelocityBeforeCollision = Mathf.Max(0f, velocity.y);
                horizontalVelocityBeforeCollision = velocity.x;
                if (grounded && !jumpedThisStep && platform != null && platform.isActiveAndEnabled)
                    appliedPlatformVelocity = platform.StepVelocity;
                body.linearVelocity = velocity + appliedPlatformVelocity;
                wasGrounded = grounded && !jumpedThisStep;
                return;
            }

            if (state != FlameState.Travelling || targetFire == null) return;
            Vector2 destination = CurrentFireTravelDestination();
            if (hasFireTravelWaypoint && Vector2.SqrMagnitude(destination - body.position) <= 0.03f)
            {
                hasFireTravelWaypoint = false;
                destination = targetFire.AnchorPosition;
            }

            Vector2 displacement = destination - body.position;
            fireTravelCurrentSpeed = AccelerateToMaximumSpeed(
                fireTravelCurrentSpeed,
                fireTravelSpeed,
                fireTravelAccelerationTime,
                Time.fixedDeltaTime);
            float stepDistance = Mathf.Min(displacement.magnitude, fireTravelCurrentSpeed * Time.fixedDeltaTime);
            if (stepDistance > 0.001f
                && TryCastFireTravelStep(displacement / displacement.magnitude, stepDistance))
            {
                StopFireTravelAtObstacle();
                return;
            }

            body.MovePosition(Vector2.MoveTowards(body.position, destination, stepDistance));
            if (!hasFireTravelWaypoint && Vector2.SqrMagnitude(targetFire.AnchorPosition - body.position) <= 0.03f)
            {
                EnterFire();
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (controlsLocked) return;
            HandleFlammableContact(collision.collider);

            if (state == FlameState.BurstDashing && burstDashAbility.IsBlocked(collision))
            {
                EndBurstDash(true);
                return;
            }

            if (state == FlameState.Travelling
                && IsFireTravelBlocked(collision))
            {
                StopFireTravelAtObstacle();
                return;
            }

            TryCornerCorrection(collision);
            RememberWallSpeed(collision);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (controlsLocked) return;
            if (state == FlameState.BurstDashing && burstDashAbility.IsBlocked(collision))
            {
                EndBurstDash(true);
                return;
            }

            if (state == FlameState.Travelling
                && IsFireTravelBlocked(collision))
            {
                StopFireTravelAtObstacle();
                return;
            }

            TryCornerCorrection(collision);
        }
        private void OnTriggerEnter2D(Collider2D other) => HandleFlammableContact(other);

        private void RememberWallSpeed(Collision2D collision)
        {
            if (state != FlameState.Free || wallSpeedRetentionTime <= 0f
                || Mathf.Abs(horizontalVelocityBeforeCollision) < 0.1f) return;

            float direction = Mathf.Sign(horizontalVelocityBeforeCollision);
            for (int i = 0; i < collision.contactCount; i++)
            {
                if (collision.GetContact(i).normal.x * direction > -0.5f) continue;
                retainedWallSpeedX = horizontalVelocityBeforeCollision;
                wallSpeedRetentionRemaining = wallSpeedRetentionTime;
                return;
            }
        }

        private void ApplyWallSpeedRetention(ref Vector2 velocity)
        {
            if (wallSpeedRetentionRemaining <= 0f) return;
            wallSpeedRetentionRemaining -= Time.fixedDeltaTime;
            if (Mathf.Abs(horizontalInput) < 0.1f) return;
            if (Mathf.Sign(horizontalInput) != Mathf.Sign(retainedWallSpeedX))
            {
                wallSpeedRetentionRemaining = 0f;
                return;
            }

            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(Physics2D.AllLayers);
            filter.useTriggers = false;
            int count = bodyCollider.Cast(Vector2.right * Mathf.Sign(retainedWallSpeedX),
                filter, wallClearanceCastHits, 0.05f);
            for (int i = 0; i < count; i++)
            {
                if (wallClearanceCastHits[i].normal.x * Mathf.Sign(retainedWallSpeedX) < -0.5f) return;
            }

            if (Mathf.Abs(velocity.x) < Mathf.Abs(retainedWallSpeedX))
                velocity.x = retainedWallSpeedX;
            wallSpeedRetentionRemaining = 0f;
        }

        private float SelectHorizontalAcceleration(float currentSpeed, float targetSpeed, bool grounded)
        {
            if (Mathf.Approximately(targetSpeed, 0f))
            {
                return grounded ? groundDeceleration : airDeceleration;
            }

            bool reversing = !Mathf.Approximately(currentSpeed, 0f)
                && Mathf.Sign(currentSpeed) != Mathf.Sign(targetSpeed);
            if (reversing) return turnAcceleration;
            if (!grounded && Mathf.Abs(currentSpeed) > Mathf.Abs(targetSpeed))
            {
                // Holding forward preserves a boost better than releasing the direction.
                return Mathf.Min(overspeedDeceleration, airDeceleration);
            }
            return grounded ? groundAcceleration : airAcceleration;
        }

        private void UpdateJumpGravity(float verticalSpeed, bool grounded)
        {
            if (grounded)
            {
                body.gravityScale = initialGravity;
                return;
            }

            if (jumpHeld && Mathf.Abs(verticalSpeed) <= apexVelocityThreshold)
            {
                body.gravityScale = initialGravity * apexGravityMultiplier;
            }
            else if (verticalSpeed < 0f)
            {
                body.gravityScale = initialGravity * fallGravityMultiplier;
            }
            else if (jumpRiseActive && verticalSpeed > apexVelocityThreshold)
            {
                body.gravityScale = initialGravity * riseGravityMultiplier;
            }
            else
            {
                body.gravityScale = initialGravity;
            }
        }

        private float GetJumpLaunchSpeed()
        {
            if (riseGravityMultiplier <= 1f || jumpSpeed <= apexVelocityThreshold)
                return jumpSpeed;

            // Increase the upward acceleration while preserving the old height at the apex threshold.
            float thresholdSpeedSquared = apexVelocityThreshold * apexVelocityThreshold;
            float baselineRiseSpeedSquared = jumpSpeed * jumpSpeed - thresholdSpeedSquared;
            return Mathf.Sqrt(thresholdSpeedSquared + baselineRiseSpeedSquared * riseGravityMultiplier);
        }

        private void TryCornerCorrection(Collision2D collision)
        {
            if (state != FlameState.Free || cornerCorrectionUsed || upwardVelocityBeforeCollision <= 0f) return;

            bool hitCeiling = false;
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y < -0.65f)
                {
                    hitCeiling = true;
                    break;
                }
            }
            if (!hitCeiling) return;

            float preferredDirection = horizontalInput != 0f ? Mathf.Sign(horizontalInput) : facingDirection;
            int stepCount = Mathf.CeilToInt(cornerCorrectionDistance / cornerCorrectionStep);
            for (int step = 1; step <= stepCount; step++)
            {
                float distance = Mathf.Min(step * cornerCorrectionStep, cornerCorrectionDistance);
                if (TryApplyCornerOffset(preferredDirection * distance)) return;
                if (TryApplyCornerOffset(-preferredDirection * distance)) return;
            }
        }

        private bool TryApplyCornerOffset(float horizontalOffset)
        {
            Bounds bounds = bodyCollider.bounds;
            Vector2 testCenter = (Vector2)bounds.center + new Vector2(horizontalOffset, 0.03f);
            Vector2 testSize = new Vector2(bounds.size.x * 0.88f, bounds.size.y * 0.94f);
            Collider2D[] hits = Physics2D.OverlapBoxAll(testCenter, testSize, 0f);
            foreach (Collider2D hit in hits)
            {
                if (hit != bodyCollider && !hit.isTrigger) return false;
            }

            body.position += new Vector2(horizontalOffset, 0.03f);
            Vector2 velocity = body.linearVelocity;
            velocity.y = upwardVelocityBeforeCollision;
            body.linearVelocity = velocity;
            cornerCorrectionUsed = true;
            return true;
        }

        private void HandleFlammableContact(Collider2D other)
        {
            if (!other.TryGetComponent(out FlammableTile tile)) return;
            tile.TryIgnite();
        }

        private bool IsGrounded()
        {
            return HasGroundBelow(groundProbeDistance);
        }

        private bool ShouldWaitForLanding(Vector2 velocity)
        {
            if (velocity.y >= 0f || landingJumpBufferTime <= 0f) return false;
            float lookAheadTime = Mathf.Min(landingJumpBufferTime, jumpRemaining);
            float gravity = Mathf.Abs(Physics2D.gravity.y) * initialGravity * fallGravityMultiplier;
            float distance = -velocity.y * lookAheadTime + 0.5f * gravity * lookAheadTime * lookAheadTime;
            return HasGroundBelow(Mathf.Max(groundProbeDistance, distance));
        }

        private bool HasGroundBelow(float distance)
        {
            return TryFindGround(distance, out _);
        }

        private bool TryFindGround(float distance, out RaycastHit2D closestHit)
        {
            closestHit = default;
            float closestDistance = float.PositiveInfinity;
            Bounds bounds = bodyCollider.bounds;
            Vector2 origin = new Vector2(bounds.center.x, bounds.min.y + 0.02f);
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(Physics2D.AllLayers);
            filter.useTriggers = false;
            int hitCount = Physics2D.BoxCast(origin, new Vector2(bounds.size.x * 0.8f, 0.05f),
                0f, Vector2.down, filter, groundHits, distance);
            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit2D hit = groundHits[i];
                if (hit.collider != null && hit.collider.attachedRigidbody != body
                    && !hit.collider.isTrigger && hit.normal.y > 0.65f && hit.distance < closestDistance)
                {
                    closestDistance = hit.distance;
                    closestHit = hit;
                }
            }
            return closestHit.collider != null;
        }

        private Vector2 ReadHeldDirection()
        {
            Vector2 direction = Vector2.zero;
            if (Keyboard.current.leftArrowKey.isPressed) direction.x -= 1f;
            if (Keyboard.current.rightArrowKey.isPressed) direction.x += 1f;
            if (Keyboard.current.upArrowKey.isPressed) direction.y += 1f;
            if (Keyboard.current.downArrowKey.isPressed) direction.y -= 1f;
            return direction == Vector2.zero ? Vector2.zero : direction.normalized;
        }

        private void LateUpdate()
        {
            if ((state != FlameState.Free && state != FlameState.Anchored)
                || (absorbTargetPreview != null && !absorbTargetPreview.IsBurning))
                SetAbsorbTargetPreview(null);
            UpdateAbsorbTargetLine();
            UpdateAbsorbTargetGlow();
        }

        internal bool TryInteractWithNearbyTrialAltar()
        {
            return nearbyTrialAltar != null && nearbyTrialAltar.TryBeginCeremony(this);
        }

        internal void RequestFireAbsorb()
        {
            if (controlsLocked || abilityUnlocks == null
                || !abilityUnlocks.IsUnlocked(PlayerAbility.FireAbsorb)) return;
            TryAbsorb();
        }

        internal void RequestLaunchFromFire()
        {
            if (!controlsLocked && state == FlameState.Anchored) LaunchFromFire();
        }

        internal void RequestIgnitionRing()
        {
            if (controlsLocked || state != FlameState.Free || abilityUnlocks == null
                || !abilityUnlocks.IsUnlocked(PlayerAbility.IgnitionRing)) return;
            TryIgnitionBurst();
        }

        internal void RequestBurstDash(Vector2 desiredDirection)
        {
            if (controlsLocked || state != FlameState.Bursting || abilityUnlocks == null
                || !abilityUnlocks.IsUnlocked(PlayerAbility.BurstDash)) return;
            StartBurstDash(desiredDirection);
        }

        internal void BeginRingShotAim()
        {
            if (controlsLocked || state != FlameState.Bursting || burstChargeRemaining <= 0f
                || abilityUnlocks == null || !abilityUnlocks.IsUnlocked(PlayerAbility.FlameShot)) return;
            flameShotAbility.BeginAim();
        }

        internal void UpdateRingShotAim(Vector2 requestedDirection)
        {
            flameShotAbility?.UpdateAim(requestedDirection);
        }

        internal void CancelRingShotAim()
        {
            flameShotAbility?.CancelAim();
        }

        internal void ReleaseRingShotAim()
        {
            if (state != FlameState.Bursting || flameShotAbility == null) return;

            Vector2 direction = flameShotAbility.EndAim();
            if (direction == Vector2.zero || burstChargeRemaining <= 0f) return;

            flameShotAbility.Fire(direction);
            burstChargeRemaining = 0f;
            state = FlameState.Free;
            body.gravityScale = initialGravity;
            body.linearVelocity = Vector2.zero;
            flameFeedback.HideLaunchRing();
            flameFeedback.PlayLaunch(direction);
        }

        private void TryAbsorb()
        {
            if (absorbTargetPreview != null && absorbTargetPreview.IsBurning)
                BeginFireTravel(absorbTargetPreview);
        }

        private void UpdateAbsorbTargetPreview(Vector2 inputDirection)
        {
            if (state == FlameState.Anchored && inputDirection == Vector2.zero)
            {
                SetAbsorbTargetPreview(null);
                return;
            }
            Vector2 direction = inputDirection == Vector2.zero ? Vector2.right * facingDirection : inputDirection;
            SetAbsorbTargetPreview(FindBurningFire(direction));
        }

        private void SetAbsorbTargetPreview(FlammableTile preview)
        {
            if (absorbTargetPreview == preview) return;
            absorbTargetPreview = preview;
            SetAbsorbTargetLineEnabled(preview != null);
            UpdateAbsorbTargetLine();
            UpdateAbsorbTargetGlow();
        }

        private void UpdateAbsorbTargetGlow()
        {
            if (!showAbsorbTargetGlow || absorbTargetPreview == null || !absorbTargetPreview.IsBurning
                || (state != FlameState.Free && state != FlameState.Anchored))
            {
                if (absorbTargetGlowLight != null) absorbTargetGlowLight.enabled = false;
                return;
            }

            if (absorbTargetGlowLight == null)
            {
                GameObject glowObject = new GameObject("Absorb Target Preview Glow");
                absorbTargetGlowLight = glowObject.AddComponent<Light2D>();
                absorbTargetGlowLight.lightType = Light2D.LightType.Point;
                absorbTargetGlowLight.pointLightInnerRadius = 0.05f;
            }

            Vector2 position = absorbTargetPreview.AnchorPosition;
            absorbTargetGlowLight.transform.position = new Vector3(
                position.x, position.y, absorbTargetPreview.transform.position.z);
            absorbTargetGlowLight.color = absorbTargetGlowColor;
            absorbTargetGlowLight.pointLightOuterRadius = absorbTargetGlowRadius;
            absorbTargetGlowLight.intensity = Mathf.Max(0f, absorbTargetGlowIntensity
                + Mathf.Sin(Time.time * absorbTargetGlowPulseSpeed * Mathf.PI * 2f)
                * absorbTargetGlowPulseAmount);
            absorbTargetGlowLight.enabled = true;
        }

        private void CreateAbsorbTargetLine()
        {
            if (!showAbsorbTargetLine) return;

            runtimeAbsorbTargetLineMaterial = CreateAbsorbTargetLineMaterial();
            absorbTargetLineSegments = Mathf.Clamp(absorbTargetLineSegments, 4, 24);
            absorbTargetLinePositions = new Vector3[absorbTargetLineSegments + 1];

            SpriteRenderer playerVisual = GetComponentInChildren<SpriteRenderer>();
            int sortingLayerId = 0;
            int baseSortingOrder = absorbTargetLineSortingOrderOffset;
            if (playerVisual != null)
            {
                sortingLayerId = playerVisual.sortingLayerID;
                baseSortingOrder += playerVisual.sortingOrder;
            }

            Color glowColor = absorbTargetLineColor;
            glowColor.a *= 0.18f;
            Color coreColor = Color.Lerp(absorbTargetLineColor, Color.white, 0.72f);

            absorbTargetLineGlow = CreateAbsorbTargetLineRenderer(
                "Absorb Target Line Glow", absorbTargetLineWidth * absorbTargetLineGlowWidthMultiplier,
                glowColor, sortingLayerId, baseSortingOrder);
            absorbTargetLine = CreateAbsorbTargetLineRenderer(
                "Absorb Target Line", absorbTargetLineWidth,
                absorbTargetLineColor, sortingLayerId, baseSortingOrder + 1);
            absorbTargetLineCore = CreateAbsorbTargetLineRenderer(
                "Absorb Target Line Core", absorbTargetLineWidth * 0.32f,
                coreColor, sortingLayerId, baseSortingOrder + 2);
            SetAbsorbTargetLineEnabled(false);
        }

        private LineRenderer CreateAbsorbTargetLineRenderer(
            string objectName, float width, Color color, int sortingLayerId, int sortingOrder)
        {
            GameObject lineObject = new GameObject(objectName);
            lineObject.transform.SetParent(transform, false);

            LineRenderer line = lineObject.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.loop = false;
            line.positionCount = absorbTargetLinePositions.Length;
            line.widthMultiplier = width;
            line.numCapVertices = 4;
            line.startColor = color;
            line.endColor = color;
            line.textureMode = LineTextureMode.Tile;
            line.textureScale = Vector2.one;
            line.sharedMaterial = runtimeAbsorbTargetLineMaterial;
            line.sortingLayerID = sortingLayerId;
            line.sortingOrder = sortingOrder;
            line.enabled = false;
            return line;
        }

        private Material CreateAbsorbTargetLineMaterial()
        {
            if (absorbTargetLineMaterial != null)
            {
                runtimeAbsorbTargetLineMaterial = new Material(absorbTargetLineMaterial)
                {
                    name = "Absorb Target Line (Runtime)"
                };
            }
            else
            {
                Shader shader = Shader.Find("Sprites/Default");
                if (shader == null) shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                if (shader == null) return null;

                runtimeAbsorbTargetLineMaterial = new Material(shader)
                {
                    name = "Absorb Target Line (Runtime)"
                };
            }

            if (runtimeAbsorbTargetLineMaterial.mainTexture == null)
            {
                runtimeAbsorbTargetLineTexture = CreateAbsorbTargetLineTexture();
                runtimeAbsorbTargetLineMaterial.mainTexture = runtimeAbsorbTargetLineTexture;
            }

            return runtimeAbsorbTargetLineMaterial;
        }

        private static Texture2D CreateAbsorbTargetLineTexture()
        {
            const int textureWidth = 64;
            const int textureHeight = 16;
            var texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false)
            {
                name = "Absorb Target Flame Texture (Runtime)",
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear
            };

            var pixels = new Color[textureWidth * textureHeight];
            var random = new System.Random(9281);
            for (int x = 0; x < textureWidth; x++)
            {
                float edgeJitter = (float)random.NextDouble() * 0.22f - 0.11f;
                for (int y = 0; y < textureHeight; y++)
                {
                    float across = Mathf.Abs((y + 0.5f) / textureHeight * 2f - 1f);
                    float noise = Mathf.PerlinNoise(x * 0.31f, y * 0.47f);
                    float edge = 0.78f + edgeJitter + (noise - 0.5f) * 0.22f;
                    float alpha = 1f - Mathf.SmoothStep(edge - 0.12f, edge + 0.08f, across);
                    alpha *= Mathf.Lerp(0.58f, 1f, noise);
                    if (random.NextDouble() < 0.035) alpha *= 0.25f;

                    float core = 1f - Mathf.SmoothStep(0.08f, 0.76f, across);
                    Color color = Color.Lerp(
                        new Color(1f, 0.22f, 0.025f),
                        new Color(1f, 0.96f, 0.54f),
                        core);
                    pixels[y * textureWidth + x] = new Color(color.r, color.g, color.b, alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(false, true);
            return texture;
        }

        private void UpdateAbsorbTargetLine()
        {
            if (runtimeAbsorbTargetLineMaterial != null
                && runtimeAbsorbTargetLineMaterial.HasProperty("_MainTex"))
            {
                runtimeAbsorbTargetLineMaterial.mainTextureOffset =
                    Vector2.left * (Time.time * absorbTargetLineTextureScrollSpeed);
            }

            if (absorbTargetLine == null) return;
            if ((state != FlameState.Free && state != FlameState.Anchored)
                || absorbTargetPreview == null || !absorbTargetPreview.IsBurning)
            {
                SetAbsorbTargetLineEnabled(false);
                return;
            }

            Vector2 start = body.position;
            Vector2 end = absorbTargetPreview.AnchorPosition;
            Vector2 direction = end - start;
            Vector2 perpendicular = direction.sqrMagnitude > 0.0001f
                ? new Vector2(-direction.y, direction.x).normalized
                : Vector2.up;
            float time = Time.time * absorbTargetLineWobbleSpeed;
            for (int i = 0; i < absorbTargetLinePositions.Length; i++)
            {
                float t = i / (float)(absorbTargetLinePositions.Length - 1);
                Vector2 point = Vector2.Lerp(start, end, t);
                float envelope = Mathf.Sin(t * Mathf.PI);
                float broadNoise = Mathf.PerlinNoise(12.7f + i * 0.39f, time) * 2f - 1f;
                float fineNoise = Mathf.PerlinNoise(41.3f + i * 0.93f, time * 1.7f) * 2f - 1f;
                float offset = (broadNoise * 0.72f + fineNoise * 0.28f)
                    * absorbTargetLineWobbleAmount * envelope;
                point += perpendicular * offset;
                absorbTargetLinePositions[i] = new Vector3(point.x, point.y, transform.position.z);
            }

            absorbTargetLine.SetPositions(absorbTargetLinePositions);
            absorbTargetLineGlow.SetPositions(absorbTargetLinePositions);
            absorbTargetLineCore.SetPositions(absorbTargetLinePositions);
            SetAbsorbTargetLineEnabled(true);
        }

        private void SetAbsorbTargetLineEnabled(bool enabled)
        {
            if (absorbTargetLine != null) absorbTargetLine.enabled = enabled;
            if (absorbTargetLineGlow != null) absorbTargetLineGlow.enabled = enabled;
            if (absorbTargetLineCore != null) absorbTargetLineCore.enabled = enabled;
        }

        private void OnDestroy()
        {
            if (absorbTargetGlowLight != null) Destroy(absorbTargetGlowLight.gameObject);
            if (runtimeAbsorbTargetLineMaterial != null) Destroy(runtimeAbsorbTargetLineMaterial);
            if (runtimeAbsorbTargetLineTexture != null) Destroy(runtimeAbsorbTargetLineTexture);
        }

        private FlammableTile FindBurningFire(Vector2 direction)
        {
            int hitCount = Physics2D.OverlapCircleNonAlloc(body.position, fireTravelRange, fireTargetHits);
            if (hitCount == fireTargetHits.Length)
            {
                System.Array.Resize(ref fireTargetHits, fireTargetHits.Length * 2);
                hitCount = Physics2D.OverlapCircleNonAlloc(body.position, fireTravelRange, fireTargetHits);
            }
            FlammableTile selected = null;
            float bestScore = float.NegativeInfinity;
            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hit = fireTargetHits[i];
                if (hit == null) continue;
                if (!hit.TryGetComponent(out FlammableTile tile) || !tile.IsBurning) continue;
                if (state == FlameState.Anchored && tile == targetFire) continue;
                Vector2 offset = tile.AnchorPosition - body.position;
                float distance = offset.magnitude;
                if (distance <= 0.05f) continue;
                float directionMatch = Vector2.Dot(offset / distance, direction);
                if (directionMatch < fireTargetDirectionDot) continue;
                float score = directionMatch * 2f - distance / fireTravelRange;
                if (state == FlameState.Anchored && distance <= fireNetworkStepRange
                    && directionMatch >= fireNetworkDirectionDot)
                    score += 0.4f;
                if (score > bestScore)
                {
                    bestScore = score;
                    selected = tile;
                }
            }
            return selected;
        }

        private void TryIgnitionBurst()
        {
            if (state != FlameState.Free || !burstAvailable) return;
            appliedPlatformVelocity = Vector2.zero;
            SetAbsorbTargetPreview(null);
            burstAvailable = false;
            TriggerIgnitionBurst();
            flameFeedback.PlayBurst();
            state = FlameState.Bursting;
            burstChargeRemaining = ignitionBurstChargeTime;
            jumpRemaining = coyoteRemaining = 0f;
            body.gravityScale = 0f;
            body.linearVelocity = Vector2.zero;
            normalJump = false;
            jumpRiseActive = false;
            flameFeedback.ShowTimedLaunchRing(ignitionBurstChargeTime);
        }

        private void StartBurstDash(Vector2 desiredDirection)
        {
            if (burstChargeRemaining <= 0f) return;

            if (!burstDashAbility.TryBegin(desiredDirection, fireTravelObstacleMask.value)) return;
            burstChargeRemaining = 0f;
            jumpRemaining = 0f;
            state = FlameState.BurstDashing;
            body.gravityScale = 0f;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.linearVelocity = Vector2.zero;
            flameFeedback.HideLaunchRing();
            flameFeedback.PlayLaunch(burstDashAbility.Direction);
            Camera.main?.GetComponent< CelesteRoomCamera >()?.ShakeDash();
            afterimageEffect.BeginTrail();
        }

        private void EndBurstDash(bool blocked)
        {
            if (state != FlameState.BurstDashing) return;

            Vector2 exitVelocity = burstDashAbility.End(blocked);
            if (burstDashAbility.NeedsSafeReset)
            {
                KillAndRespawn();
                return;
            }
            state = FlameState.Free;
            body.gravityScale = initialGravity;
            body.collisionDetectionMode = initialCollisionDetectionMode;
            body.linearVelocity = exitVelocity;
            launchProtection = blocked ? 0f : burstDashExitControlLockTime;
            afterimageEffect.StopTrail();
            wasGrounded = false;
        }

        private static float AccelerateToMaximumSpeed(
            float currentSpeed,
            float maximumSpeed,
            float accelerationTime,
            float deltaTime)
        {
            if (maximumSpeed <= 0f || accelerationTime <= 0f) return maximumSpeed;

            float acceleration = maximumSpeed / accelerationTime;
            return Mathf.MoveTowards(currentSpeed, maximumSpeed, acceleration * deltaTime);
        }

        private void TriggerIgnitionBurst()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(body.position, ignitionBurstRadius);
            foreach (Collider2D hit in hits)
            {
                if (hit.TryGetComponent(out FlammableTile tile)) tile.TryIgnite();
            }

            CreateIgnitionBurstEffect();
        }

        private void CreateIgnitionBurstEffect()
        {
            ClearBurstEffect();

            activeBurstEffect = new GameObject("Ignition Burst Effect");
            LineRenderer line = activeBurstEffect.AddComponent<LineRenderer>();
            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null) line.sharedMaterial = new Material(shader);
            line.loop = true;
            line.useWorldSpace = true;
            line.positionCount = 48;
            line.numCornerVertices = 3;
            line.widthMultiplier = 0.12f;
            line.sortingOrder = 20;
            StartCoroutine(AnimateBurstCircle(line, body.position));
        }

        private IEnumerator AnimateBurstCircle(LineRenderer line, Vector2 center)
        {
            const int pointCount = 48;
            Vector3[] points = new Vector3[pointCount];
            float elapsed = 0f;

            while (elapsed < ignitionBurstVisualTime && line != null)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / ignitionBurstVisualTime);
                float radius = Mathf.Lerp(0.2f, ignitionBurstRadius, progress);
                Color color = new Color(1f, Mathf.Lerp(0.9f, 0.25f, progress), 0.05f, 1f - progress);
                line.startColor = line.endColor = color;

                for (int i = 0; i < pointCount; i++)
                {
                    float angle = i * Mathf.PI * 2f / pointCount;
                    points[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                }
                line.SetPositions(points);
                yield return null;
            }

            if (line != null && activeBurstEffect == line.gameObject) ClearBurstEffect();
        }

        private void ClearBurstEffect()
        {
            if (activeBurstEffect == null) return;
            LineRenderer line = activeBurstEffect.GetComponent<LineRenderer>();
            Material material = line != null ? line.sharedMaterial : null;
            Destroy(activeBurstEffect);
            if (material != null) Destroy(material);
            activeBurstEffect = null;
        }

        private void BeginFireTravel(FlammableTile destination)
        {
            appliedPlatformVelocity = Vector2.zero;
            SetAbsorbTargetPreview(null);
            flameFeedback.HideLaunchRing();
            FlammableTile source = state == FlameState.Anchored ? targetFire : null;
            bool hasSafeRoute = TryBuildFireTravelPath(
                body.position,
                source,
                destination,
                out Vector2 waypoint,
                out bool useWaypoint);
            if (!hasSafeRoute)
            {
                // A blocked target remains selectable, but travel stops safely at the obstacle.
                waypoint = default;
                useWaypoint = false;
            }

            jumpRemaining = coyoteRemaining = burstChargeRemaining = 0f;
            normalJump = false;
            jumpRiseActive = false;
            wallSpeedRetentionRemaining = 0f;
            fireTravelCurrentSpeed = 0f;
            travelSourceFire = source;
            targetFire = destination;
            fireTravelWaypoint = waypoint;
            hasFireTravelWaypoint = useWaypoint;
            state = FlameState.Travelling;
            body.gravityScale = 0f;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.linearVelocity = Vector2.zero;
            bodyCollider.enabled = true;
            afterimageEffect.BeginTrail();
        }

        private bool IsFireTravelBlocked(Collision2D collision)
        {
            if (targetFire == null) return false;

            Vector2 travelDirection = CurrentFireTravelDestination() - body.position;
            if (travelDirection.sqrMagnitude <= 0.0001f) return false;
            travelDirection.Normalize();

            for (int i = 0; i < collision.contactCount; i++)
            {
                ContactPoint2D contact = collision.GetContact(i);
                if (Vector2.Dot(travelDirection, contact.normal) < -0.2f) return true;
            }

            return false;
        }

        private bool TryCastFireTravelStep(Vector2 direction, float distance)
        {
            int obstacleMask = fireTravelObstacleMask.value == 0
                ? Physics2D.AllLayers
                : fireTravelObstacleMask.value;
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(obstacleMask);
            filter.useTriggers = false;

            Collider2D castCollider = state == FlameState.BurstDashing
                ? burstDashAbility.CastCollider : bodyCollider;
            int hitCount = castCollider.Cast(direction, filter, fireTravelCastHits, distance + 0.03f);

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit2D hit = fireTravelCastHits[i];
                if (hit.collider == null || hit.distance <= 0.001f) continue;
                if (Vector2.Dot(direction, hit.normal) >= -0.2f) continue;
                return true;
            }

            return false;
        }

        private Vector2 CurrentFireTravelDestination()
        {
            return hasFireTravelWaypoint ? fireTravelWaypoint : targetFire.AnchorPosition;
        }

        private bool TryBuildFireTravelPath(
            Vector2 start,
            FlammableTile source,
            FlammableTile destination,
            out Vector2 waypoint,
            out bool useWaypoint)
        {
            waypoint = default;
            useWaypoint = false;
            if (destination == null) return false;

            Vector2 end = destination.AnchorPosition;
            if (HasClearFireTravelSegment(start, end, source, destination)) return true;
            if (fireTravelCornerAssistDistance <= 0f) return false;

            Vector2 displacement = end - start;
            float distance = displacement.magnitude;
            if (distance <= 0.001f) return true;

            Vector2 perpendicular = new Vector2(-displacement.y, displacement.x) / distance;
            float step = Mathf.Max(0.05f, fireTravelCornerAssistStep);
            float bestLength = float.PositiveInfinity;
            bool found = false;

            for (float offset = step; offset <= fireTravelCornerAssistDistance + 0.001f; offset += step)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    for (int midpointIndex = 0; midpointIndex < 3; midpointIndex++)
                    {
                        float midpoint = 0.35f + midpointIndex * 0.15f;
                        Vector2 candidate = Vector2.Lerp(start, end, midpoint) + perpendicular * (offset * side);
                        if (!HasClearFireTravelSegment(start, candidate, source, destination)) continue;
                        if (!HasClearFireTravelSegment(candidate, end, source, destination)) continue;

                        float routeLength = Vector2.Distance(start, candidate) + Vector2.Distance(candidate, end);
                        if (routeLength >= bestLength) continue;
                        bestLength = routeLength;
                        waypoint = candidate;
                        found = true;
                    }
                }

                // The first successful offset is the least intrusive correction.
                if (found) break;
            }

            useWaypoint = found;
            return found;
        }

        private bool HasClearFireTravelSegment(
            Vector2 start,
            Vector2 end,
            FlammableTile source,
            FlammableTile destination)
        {
            Vector2 displacement = end - start;
            float distance = displacement.magnitude;
            if (distance <= 0.001f) return true;

            Vector2 probeSize = fireTravelProbeSize * fireTravelCollisionScale;
            Vector2 probeOrigin = start + fireTravelProbeOffset;
            int obstacleMask = fireTravelObstacleMask.value == 0
                ? Physics2D.AllLayers
                : fireTravelObstacleMask.value;
            RaycastHit2D[] hits = Physics2D.BoxCastAll(
                probeOrigin,
                probeSize,
                0f,
                displacement / distance,
                distance,
                obstacleMask);

            foreach (RaycastHit2D hit in hits)
            {
                Collider2D hitCollider = hit.collider;
                if (hitCollider == null || hitCollider == bodyCollider || hitCollider.isTrigger) continue;

                FlammableTile hitFire = hitCollider.GetComponentInParent<FlammableTile>();
                if (hitFire == source || hitFire == destination) continue;
                return false;
            }

            return true;
        }

        private void StopFireTravelAtObstacle()
        {
            afterimageEffect.StopTrail();
            state = FlameState.Free;
            targetFire = null;
            travelSourceFire = null;
            hasFireTravelWaypoint = false;
            fireTravelCurrentSpeed = 0f;
            wallSpeedRetentionRemaining = 0f;
            horizontalVelocityBeforeCollision = 0f;
            upwardVelocityBeforeCollision = 0f;
            burstDashAbility.Cancel();
            normalJump = false;
            jumpRiseActive = false;
            cornerCorrectionUsed = false;
            launchProtection = 0f;
            wasGrounded = false;
            body.gravityScale = initialGravity;
            body.collisionDetectionMode = initialCollisionDetectionMode;
            body.linearVelocity = Vector2.zero;
            transform.localScale = initialScale;
        }

        private void EnterFire()
        {
            afterimageEffect.StopTrail();
            state = FlameState.Anchored;
            body.position = targetFire.AnchorPosition;
            travelSourceFire = null;
            hasFireTravelWaypoint = false;
            fireTravelCurrentSpeed = 0f;
            body.collisionDetectionMode = initialCollisionDetectionMode;
            body.linearVelocity = Vector2.zero;
            bodyCollider.enabled = false;
            transform.localScale = Vector3.one * 0.45f;
            anchoredAimDirection = Vector2.zero;
            airJumpsRemaining = maxAirJumps;
            burstAvailable = true;
            SetAbsorbTargetPreview(null);
            flameFeedback.ShowAnchoredLaunchRing();
            Camera.main?.GetComponent<CelesteRoomCamera>()?.ShakeAbsorb();
        }

        private void LaunchFromFire()
        {
            appliedPlatformVelocity = Vector2.zero;
            Vector2 direction = ReadHeldDirection();
            if (direction == Vector2.zero) direction = Vector2.up;
            if (!TryFindSafeFireLaunchPosition(direction, out Vector2 launchPosition)) return;

            SetAbsorbTargetPreview(null);
            flameFeedback.HideLaunchRing();
            state = FlameState.Free;
            targetFire = null;
            travelSourceFire = null;
            hasFireTravelWaypoint = false;
            wallSpeedRetentionRemaining = 0f;
            body.gravityScale = initialGravity;
            body.collisionDetectionMode = initialCollisionDetectionMode;
            bodyCollider.enabled = true;
            transform.localScale = initialScale;
            body.position = launchPosition;
            body.linearVelocity = direction * launchSpeed;
            upwardVelocityBeforeCollision = Mathf.Max(0f, body.linearVelocity.y);
            normalJump = false;
            jumpRiseActive = false;
            cornerCorrectionUsed = false;
            launchProtection = fireLaunchControlLockTime;
            flameFeedback.PlayLaunch(direction);
            Camera.main?.GetComponent<CelesteRoomCamera>()?.ShakeFireLaunch();
            afterimageEffect.PlayTimedTrail(launchAfterimageDuration);
            wasGrounded = false;
        }

        private bool TryFindSafeFireLaunchPosition(Vector2 direction, out Vector2 launchPosition)
        {
            Vector2 start = body.position;
            Vector2 side = new Vector2(-direction.y, direction.x);
            launchPosition = start;

            if (IsSafeFireLaunchPosition(start, start + direction * 0.7f))
            {
                launchPosition = start + direction * 0.7f;
                return true;
            }

            for (float offset = 0.08f; offset <= fireLaunchCornerAssistDistance + 0.001f; offset += 0.08f)
            {
                Vector2 first = start + direction * 0.7f + side * offset;
                Vector2 second = start + direction * 0.7f - side * offset;
                if (IsSafeFireLaunchPosition(start, first))
                {
                    launchPosition = first;
                    return true;
                }
                if (IsSafeFireLaunchPosition(start, second))
                {
                    launchPosition = second;
                    return true;
                }
            }

            return false;
        }

        private bool IsSafeFireLaunchPosition(Vector2 start, Vector2 end)
        {
            Vector2 displacement = end - start;
            Vector2 size = fireTravelProbeSize;
            Vector2 origin = start + fireTravelProbeOffset;
            Vector2 destination = end + fireTravelProbeOffset;
            RaycastHit2D[] sweepHits = Physics2D.BoxCastAll(
                origin, size, 0f, displacement.normalized, displacement.magnitude);
            foreach (RaycastHit2D hit in sweepHits)
            {
                if (hit.collider == null || hit.collider == bodyCollider || hit.collider.isTrigger) continue;
                if (hit.distance > 0.01f) return false;
            }

            Collider2D[] overlaps = Physics2D.OverlapBoxAll(destination, size, 0f);
            foreach (Collider2D overlap in overlaps)
            {
                if (overlap != bodyCollider && !overlap.isTrigger) return false;
            }
            return true;
        }

        public void SetNearbyTrialAltar(TrialAltar altar)
        {
            nearbyTrialAltar = altar;
        }

        public void ClearNearbyTrialAltar(TrialAltar altar)
        {
            if (nearbyTrialAltar == altar) nearbyTrialAltar = null;
        }

        public void SetControlsLocked(bool locked)
        {
            if (body == null || controlsLocked == locked) return;

            if (locked)
            {
                CancelRingShotAim();
                constraintsBeforeControlLock = body.constraints;
                gravityBeforeControlLock = body.gravityScale;
                controlsLocked = true;
                horizontalInput = jumpRemaining = 0f;
                jumpHeld = false;
                body.linearVelocity = Vector2.zero;
                body.angularVelocity = 0f;
                body.gravityScale = 0f;
                body.constraints = RigidbodyConstraints2D.FreezeAll;
                SetAbsorbTargetPreview(null);
            }
            else
            {
                body.constraints = constraintsBeforeControlLock;
                body.gravityScale = gravityBeforeControlLock;
                body.linearVelocity = Vector2.zero;
                controlsLocked = false;
                horizontalInput = jumpRemaining = 0f;
                jumpHeld = false;
            }
        }

        public void ResetAt(Vector2 position)
        {
            appliedPlatformVelocity = Vector2.zero;
            if (body == null || bodyCollider == null) return;

            CancelRingShotAim();
            burstDashAbility.Cancel();
            if (nearbyTrialAltar != null) nearbyTrialAltar.CancelCeremonyFor(this);
            SetControlsLocked(false);
            SetAbsorbTargetPreview(null);

            state = FlameState.Free;
            targetFire = null;
            travelSourceFire = null;
            hasFireTravelWaypoint = false;
            horizontalInput = jumpRemaining = coyoteRemaining = launchProtection = burstChargeRemaining = 0f;
            fireTravelCurrentSpeed = 0f;
            anchoredAimDirection = Vector2.zero;
            normalJump = jumpReleased = jumpHeld = cornerCorrectionUsed = false;
            jumpRiseActive = false;
            upwardVelocityBeforeCollision = 0f;
            horizontalVelocityBeforeCollision = retainedWallSpeedX = wallSpeedRetentionRemaining = 0f;
            airJumpsRemaining = maxAirJumps;
            burstAvailable = true;
            groundStateInitialized = false;
            wasGrounded = false;
            StopAllCoroutines();
            ClearBurstEffect();
            flameFeedback?.ResetFeedback();
            afterimageEffect?.StopTrail(true);
            transform.localScale = initialScale;
            body.gravityScale = initialGravity;
            body.collisionDetectionMode = initialCollisionDetectionMode;
            bodyCollider.enabled = true;
            body.position = position;
            body.linearVelocity = Vector2.zero;
        }

        public void KillAndRespawn()
        {
            Vector2 position = respawnPoint != null
                ? (Vector2)respawnPoint.position
                : initialSpawnPosition;
            ResetAt(position);
        }

        public void SetRespawnPoint(Transform newRespawnPoint)
        {
            respawnPoint = newRespawnPoint;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.4f, 0f, 0.55f);
            Gizmos.DrawWireSphere(transform.position, ignitionBurstRadius);
        }
    }
}
