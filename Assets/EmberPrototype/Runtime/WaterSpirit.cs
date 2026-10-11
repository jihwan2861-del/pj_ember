using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace EmberPrototype
{
    [AddComponentMenu("Ember Prototype/Water Spirit")]
    [RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
    public sealed class WaterSpirit : MonoBehaviour
    {
        [SerializeField] private WaterWave wavePrefab;
        [SerializeField] private Transform waveSpawn;
        [SerializeField] private WaterWave.TravelDirection direction;
        [SerializeField] private bool autoAttack = true;
        [SerializeField, Min(0.1f)] private float attackInterval = 3f;
        [SerializeField, Min(0f)] private float waveReleaseTime = 0.25f;
        [SerializeField, Min(0.1f)] private float attackDuration = 0.5f;
        [Header("Wave Screen Shake")]
        [SerializeField, Min(0f)] private float waveShakeDuration = 0.1f;
        [SerializeField, Min(0f)] private float waveShakeMagnitude = 0.06f;
        [Header("Rush Afterimages")]
        [SerializeField] private Color rushAfterimageColor = new Color(0.15f, 0.55f, 1f, 0.62f);
        private PlayerAfterimageEffect afterimageEffect;
        [Header("Spirit Light")]
        [SerializeField] private Light2D spiritLight;
        [SerializeField] private Color spiritLightColor = new Color(0.15f, 0.55f, 1f, 1f);
        [SerializeField, Min(0f)] private float spiritLightIntensity = 0.8f;
        [SerializeField, Min(0.1f)] private float spiritLightRadius = 2.5f;
        private Animator animator;
        private SpriteRenderer visual;
        private WaterWave wave;
        private Vector2 moveVelocity;
        private float attackElapsed, cooldown;
        private bool released;
        private bool pausedForRespawn;
        private Vector3 initialPosition;
        private WaterWave.TravelDirection initialDirection;
        private WaterWave.TravelDirection attackDirection;
        private WaterSpiritAI ai;
        private bool usesPlannedAttack;
        private WaterSpiritAttack plannedAttack;
        private GameObject rushHitbox;
        private bool isRushing;
        private static readonly int IdleState = Animator.StringToHash("Idle");
        private static readonly int MoveState = Animator.StringToHash("Move");
        private static readonly int AttackState = Animator.StringToHash("Attack");
        public bool IsAttacking { get; private set; }
        public WaterWave ActiveWave => wave;
        public WaterWave WaveTemplate => wavePrefab;
        public bool HasReleasedWave => released;
        public bool IsRushing => isRushing;
        private bool AIControlled => ai != null && ai.isActiveAndEnabled;
        internal void AttachAI(WaterSpiritAI controller) => ai = controller;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            visual = GetComponent<SpriteRenderer>();
            afterimageEffect = GetComponent<PlayerAfterimageEffect>();
            if (afterimageEffect == null) afterimageEffect = gameObject.AddComponent<PlayerAfterimageEffect>();
            afterimageEffect.SetColor(rushAfterimageColor);
            if (spiritLight == null)
            {
                GameObject lightObject = new GameObject("Spirit Light");
                lightObject.transform.SetParent(transform, false);
                spiritLight = lightObject.AddComponent<Light2D>();
                spiritLight.lightType = Light2D.LightType.Point;
            }
            ConfigureLight();
            initialPosition = transform.position;
            initialDirection = direction;
        }

        private void OnEnable()
        {
            if (spiritLight != null) { ConfigureLight(); spiritLight.enabled = true; }
            IsAttacking = false;
            pausedForRespawn = false;
            moveVelocity = Vector2.zero;
            released = false;
            cooldown = attackInterval;
            ApplyFacing(direction);
            animator.Play(IdleState, 0, 0f);
        }

        private void Update()
        {
            if (pausedForRespawn) return;
            if (AIControlled) ai.Tick(Time.deltaTime);
            if (AIControlled)
            {
                transform.position += (Vector3)(moveVelocity * Time.deltaTime);
                ai.ConstrainToScreen();
            }
            if (IsAttacking)
            {
                attackElapsed += Time.deltaTime;
                if (!released && attackElapsed >= waveReleaseTime)
                {
                    released = true;
                    ReleaseWave();
                }
                if (attackElapsed >= Mathf.Max(attackDuration, waveReleaseTime))
                {
                    IsAttacking = false;
                    cooldown = attackInterval;
                    PlayLocomotion();
                }
                return;
            }
            if (!AIControlled) transform.position += (Vector3)(moveVelocity * Time.deltaTime);
            if (!AIControlled && autoAttack && (cooldown -= Time.deltaTime) <= 0f) Attack();
        }

        // External movement can be supplied later without adding another boss behaviour now.
        public void SetMoveVelocity(Vector2 velocity)
        {
            moveVelocity = velocity;
            if (Mathf.Abs(velocity.x) > 0.001f)
                direction = velocity.x > 0f ? WaterWave.TravelDirection.LeftToRight : WaterWave.TravelDirection.RightToLeft;
            if (!IsAttacking)
            {
                ApplyFacing(direction);
                PlayLocomotion();
            }
        }

        public void Attack()
        {
            if (AIControlled) return;
            usesPlannedAttack = false;
            StartAttack();
        }

        internal bool TryAttack(WaterSpiritAttack plan)
        {
            if (!plan.IsValid) return false;
            plannedAttack = plan;
            direction = plan.Direction;
            usesPlannedAttack = true;
            return StartAttack();
        }

        private bool StartAttack()
        {
            if (!isActiveAndEnabled || pausedForRespawn || IsAttacking || wavePrefab == null || (wave != null && wave.IsMoving)) return false;
            IsAttacking = true;
            attackElapsed = 0f;
            released = false;
            attackDirection = direction;
            ApplyFacing(attackDirection);
            animator.Play(AttackState, 0, 0f);
            return true;
        }

        private void ReleaseWave()
        {
            // Reuse one projectile per spirit, so repeated attacks cannot leave stray waves.
            if (wave == null)
            {
                wave = Instantiate(wavePrefab, transform.position, Quaternion.identity);
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(wave.gameObject, gameObject.scene);
                wave.gameObject.SetActive(true);
            }
            if (usesPlannedAttack) wave.Launch(plannedAttack);
            else wave.Launch(GetWaveSpawn(attackDirection), attackDirection);
            Camera camera = AIControlled ? ai.GameCamera : Camera.main;
            if (camera != null)
                camera.GetComponent<CelesteRoomCamera>()?.Shake(waveShakeDuration, waveShakeMagnitude);
        }

        public Vector2 GetWaveSpawn(WaterWave.TravelDirection facing)
        {
            float sign = facing == WaterWave.TravelDirection.LeftToRight ? 1f : -1f;
            Vector3 offset = waveSpawn != null ? transform.InverseTransformPoint(waveSpawn.position) : new Vector3(0.8f, -0.5f, 0f);
            return transform.TransformPoint(new Vector3(Mathf.Abs(offset.x) * sign, offset.y, offset.z));
        }

        internal void Face(WaterWave.TravelDirection facing) { direction = facing; ApplyFacing(facing); }

        internal Bounds VisibleBounds => visual.bounds;

        internal void BeginRush(WaterSpiritAttack plan)
        {
            if (rushHitbox == null)
            {
                rushHitbox = new GameObject("Rush Hitbox");
                rushHitbox.SetActive(false);
                rushHitbox.transform.SetParent(transform, false);
                rushHitbox.AddComponent<BoxCollider2D>().isTrigger = true;
                rushHitbox.AddComponent<RoomHazard>();
            }
            BoxCollider2D hitbox = rushHitbox.GetComponent<BoxCollider2D>();
            hitbox.offset = transform.InverseTransformPoint(plan.Start + Vector2.up * plan.Height / 2f);
            hitbox.size = new Vector2(plan.Width / Mathf.Abs(transform.lossyScale.x), plan.Height / Mathf.Abs(transform.lossyScale.y));
            isRushing = true;
            rushHitbox.SetActive(plan.Deadly);
            Face(plan.Direction);
            afterimageEffect.SetColor(rushAfterimageColor);
            afterimageEffect.BeginTrail();
        }

        internal void EndRush()
        {
            isRushing = false;
            afterimageEffect?.StopTrail();
            if (rushHitbox != null) rushHitbox.SetActive(false);
        }
        internal void CancelAction()
        {
            afterimageEffect?.StopTrail(true);
            IsAttacking = false;
            released = false;
            usesPlannedAttack = false;
            moveVelocity = Vector2.zero;
            if (wave != null) wave.Stop();
            EndRush();
            if (animator != null && isActiveAndEnabled) animator.Play(IdleState, 0, 0f);
        }

        private void ApplyFacing(WaterWave.TravelDirection facing)
        {
            // The authored spirit frames face left; waves are authored facing right.
            visual.flipX = facing == WaterWave.TravelDirection.LeftToRight;
        }

        public void ResetForRespawn(bool deferStart)
        {
            afterimageEffect?.StopTrail(true);
            IsAttacking = false;
            pausedForRespawn = deferStart;
            transform.position = initialPosition;
            direction = initialDirection;
            moveVelocity = Vector2.zero;
            cooldown = attackInterval;
            if (wave != null) wave.Stop();
            EndRush();
            ApplyFacing(direction);
            animator.Play(IdleState, 0, 0f);
            ai?.Restart(deferStart);
        }

        public void ResumeAfterRespawn() { pausedForRespawn = false; ai?.ResumeAfterRespawn(); }

        private void PlayLocomotion()
        {
            int state = moveVelocity.sqrMagnitude > 0.0001f ? MoveState : IdleState;
            if (!animator.GetCurrentAnimatorStateInfo(0).shortNameHash.Equals(state)) animator.Play(state);
        }

        private void OnDisable()
        {
            if (spiritLight != null) spiritLight.enabled = false;
            afterimageEffect?.StopTrail(true);
            IsAttacking = false;
            if (wave != null) wave.Stop();
            EndRush();
            ai?.Restart(false);
        }

        private void OnDestroy()
        {
            if (wave != null) Destroy(wave.gameObject);
        }

        private void ConfigureLight()
        {
            if (spiritLight == null) return;
            spiritLight.color = spiritLightColor;
            spiritLight.intensity = Mathf.Max(0f, spiritLightIntensity);
            spiritLight.pointLightOuterRadius = Mathf.Max(0.1f, spiritLightRadius);
            spiritLight.pointLightInnerRadius = Mathf.Min(0.3f, spiritLight.pointLightOuterRadius);
        }

        private void OnValidate() => ConfigureLight();
    }
}


