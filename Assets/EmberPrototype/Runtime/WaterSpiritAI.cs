using UnityEngine;

namespace EmberPrototype
{
    [AddComponentMenu("Ember Prototype/Water Spirit AI")]
    [RequireComponent(typeof(WaterSpirit))]
    public sealed class WaterSpiritAI : MonoBehaviour
    {
        public enum Phase { Tracking, Warning, Firing, Recovery, Suspended, Rushing }
        [SerializeField, Min(0f)] private float screenPadding = 0.15f;
        [SerializeField, Min(0.1f)] private float rushSpeed = 16f;
        [SerializeField, Min(0.1f)] private float rushDistance = 10f;
        [SerializeField] private Collider2D playerBody;
        [SerializeField] private Camera gameCamera;
        [SerializeField] private WaterSpiritBehavior initialBehavior;
        [SerializeField] private WaterSpiritWarning warning;
        private WaterSpirit spirit;
        private WaterSpiritBehavior currentBehavior, pendingBehavior;
        private bool initialized, pausedForRespawn, reportedInvalid;
        private float elapsed;
        private WaterSpiritAttack attack;
        private bool nextRush;
        private float rushTravelled;
        public WaterSpirit Spirit => spirit;
        public Camera GameCamera => gameCamera;
        public Vector2 PlayerCenter => playerBody.enabled ? (Vector2)playerBody.bounds.center :
            (Vector2)playerBody.transform.TransformPoint(playerBody.offset);
        public Phase CurrentPhase { get; private set; } = Phase.Suspended;
        public WaterSpiritBehavior CurrentBehavior => currentBehavior;
        public WaterSpiritBehavior PendingBehavior => pendingBehavior;
        public WaterSpiritAttack PlannedAttack => attack;
        public float PhaseElapsed => elapsed;

        private void Awake()
        {
            spirit = GetComponent<WaterSpirit>();
            spirit.AttachAI(this);
        }

        private void OnEnable()
        {
            Camera.onPreCull += BeforeCamera;
            UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering += BeforePipelineCamera;
            initialized = false;
            pausedForRespawn = false;
            pendingBehavior = null;
            reportedInvalid = false;
        }

        public void Configure(Collider2D player, Camera camera, WaterSpiritBehavior behavior)
        {
            playerBody = player; gameCamera = camera; initialBehavior = behavior;
            Restart(false);
        }

        private bool IsConnected(WaterSpiritBehavior behavior) =>
            behavior != null && behavior.gameObject == gameObject && behavior.isActiveAndEnabled;

        private bool ReferencesValid => spirit != null && spirit.isActiveAndEnabled && playerBody != null &&
            playerBody.gameObject.activeInHierarchy && gameCamera != null && gameCamera.isActiveAndEnabled &&
            gameCamera.orthographic && IsConnected(initialBehavior) && spirit.WaveTemplate != null && warning != null &&
            (!initialized || IsConnected(currentBehavior));

        // Called by WaterSpirit before its movement/animation tick, avoiding execution-order settings.
        internal void Tick(float deltaTime)
        {
            if (!isActiveAndEnabled || pausedForRespawn) return;
            if (!ReferencesValid)
            {
                if (!reportedInvalid)
                {
                    Debug.LogWarning("WaterSpiritAI: assign valid references. Missing or inactive: " + InvalidReferenceName(), this);
                    reportedInvalid = true;
                }
                Suspend();
                return;
            }
            reportedInvalid = false;
            if (pendingBehavior != null && !IsConnected(pendingBehavior)) pendingBehavior = null;
            if (!initialized) SwitchTo(pendingBehavior != null ? pendingBehavior : initialBehavior);
            if (deltaTime <= 0f) return;
            if (CurrentPhase == Phase.Tracking && pendingBehavior != null &&
                (spirit.ActiveWave == null || !spirit.ActiveWave.IsMoving)) SwitchTo(pendingBehavior);
            elapsed += deltaTime;
            switch (CurrentPhase)
            {
                case Phase.Tracking:
                    FollowPlayer(deltaTime);
                    if (elapsed >= currentBehavior.TrackingTime &&
                        ((nextRush && currentBehavior.CanRush) || currentBehavior.IsReady(this)))
                    {
                        attack = currentBehavior.SelectAttack(this);
                        if (!attack.IsValid) { Debug.LogWarning("WaterSpiritAI: invalid attack description.", this); Suspend(); return; }
                        if (nextRush && currentBehavior.CanRush)
                        {
                            ConstrainToScreen();
                            Bounds screen = ScreenBounds();
                            Bounds visible = spirit.VisibleBounds;
                            float extent = visible.extents.x + Mathf.Abs(visible.center.x - transform.position.x);
                            float sign = attack.Direction == WaterWave.TravelDirection.LeftToRight ? 1f : -1f;
                            float available = sign > 0f ? screen.max.x - extent - transform.position.x : transform.position.x - screen.min.x - extent;
                            float distance = Mathf.Min(rushDistance, available);
                            nextRush = false;
                            if (distance <= 0.05f) { ChangePhase(Phase.Tracking); break; }
                            attack = new WaterSpiritAttack(new Vector2(visible.center.x, visible.min.y), attack.Direction, visible.size.y, visible.size.x, rushSpeed, distance, attack.Deadly);
                            spirit.SetMoveVelocity(Vector2.zero);
                            spirit.Face(attack.Direction);
                            warning.Show(attack);
                            ChangePhase(Phase.Warning);
                        }
                        else if (spirit.TryAttack(attack))
                        {
                            warning.Hide(); nextRush = true;
                            ChangePhase(Phase.Firing);
                        }
                    }
                    break;
                case Phase.Warning:
                    spirit.SetMoveVelocity(Vector2.zero);
                    // The warned rush is committed, even when the player has already dodged it.
                    if (elapsed >= currentBehavior.WarningTime)
                    {
                        warning.Hide(); spirit.BeginRush(attack); rushTravelled = 0f;
                        ChangePhase(Phase.Rushing);
                        AdvanceRush(deltaTime);
                    }
                    break;
                case Phase.Firing:
                    FollowPlayer(deltaTime);
                    spirit.Face(attack.Direction);
                    if (!spirit.IsAttacking) ChangePhase(Phase.Tracking);
                    break;
                case Phase.Rushing:
                    if (rushTravelled >= attack.Distance)
                    {
                        spirit.EndRush(); ChangePhase(Phase.Tracking); FollowPlayer(deltaTime);
                        break;
                    }
                    AdvanceRush(deltaTime);
                    break;
            }
        }

        private void AdvanceRush(float deltaTime)
        {
            float step = Mathf.Min(attack.Speed * deltaTime, attack.Distance - rushTravelled);
            rushTravelled += step;
            spirit.SetMoveVelocity(Vector2.right * (attack.Direction == WaterWave.TravelDirection.LeftToRight ? step : -step) / deltaTime);
        }

        private void FollowPlayer(float deltaTime)
        {
            Vector2 difference = currentBehavior.GetMoveTarget(this) - (Vector2)transform.position;
            spirit.SetMoveVelocity(Vector2.ClampMagnitude(difference / deltaTime, currentBehavior.MoveSpeed));
            spirit.Face(PlayerCenter.x >= transform.position.x ? WaterWave.TravelDirection.LeftToRight : WaterWave.TravelDirection.RightToLeft);
        }

        private Bounds ScreenBounds()
        {
            float depth = transform.position.z - gameCamera.transform.position.z;
            Vector3 min = gameCamera.ViewportToWorldPoint(new Vector3(0f, 0f, depth));
            Vector3 max = gameCamera.ViewportToWorldPoint(new Vector3(1f, 1f, depth));
            Bounds bounds = new Bounds((min + max) / 2f, max - min);
            bounds.Expand(-2f * screenPadding);
            return bounds;
        }

        internal void ConstrainToScreen()
        {
            if (!initialized || !ReferencesValid || pausedForRespawn) return;
            Bounds visible = spirit.VisibleBounds, screen = ScreenBounds();
            Vector3 shift = Vector3.zero;
            shift.x = visible.size.x > screen.size.x ? screen.center.x - visible.center.x :
                Mathf.Max(0f, screen.min.x - visible.min.x) + Mathf.Min(0f, screen.max.x - visible.max.x);
            shift.y = visible.size.y > screen.size.y ? screen.center.y - visible.center.y :
                Mathf.Max(0f, screen.min.y - visible.min.y) + Mathf.Min(0f, screen.max.y - visible.max.y);
            transform.position += shift;
            if (shift.sqrMagnitude > 0.000001f && CurrentPhase == Phase.Warning)
            {
                // Keep the commitment through camera corrections; only the screen limits the path.
                float extent = visible.extents.x + Mathf.Abs(visible.center.x - transform.position.x + shift.x);
                float available = attack.Direction == WaterWave.TravelDirection.LeftToRight
                    ? screen.max.x - extent - transform.position.x
                    : transform.position.x - screen.min.x - extent;
                attack = new WaterSpiritAttack(attack.Start + (Vector2)shift, attack.Direction,
                    attack.Height, attack.Width, attack.Speed, Mathf.Max(0f, Mathf.Min(attack.Distance, available)), attack.Deadly);
                warning.Show(attack);
            }
            // Vertical framing corrections must not interrupt a horizontal rush.
            if (Mathf.Abs(shift.x) > 0.001f && CurrentPhase == Phase.Rushing)
            {
                warning.Hide(); spirit.EndRush(); ChangePhase(Phase.Tracking);
            }
        }

        private void LateUpdate() => ConstrainToScreen();
        private void BeforeCamera(Camera camera) { if (camera == gameCamera) ConstrainToScreen(); }
        private void BeforePipelineCamera(UnityEngine.Rendering.ScriptableRenderContext context, Camera camera) => BeforeCamera(camera);

        public bool RequestBehavior(WaterSpiritBehavior behavior)
        {
            if (!isActiveAndEnabled || pausedForRespawn || !ReferencesValid || !IsConnected(behavior)) return false;
            if (behavior == currentBehavior) { pendingBehavior = null; return true; }
            pendingBehavior = behavior;
            return true;
        }

        private string InvalidReferenceName()
        {
            if (spirit == null || !spirit.isActiveAndEnabled) return "WaterSpirit";
            if (playerBody == null || !playerBody.gameObject.activeInHierarchy) return "Player Body";
            if (gameCamera == null || !gameCamera.isActiveAndEnabled || !gameCamera.orthographic) return "Game Camera (orthographic)";
            if (!IsConnected(initialBehavior)) return "Initial Behavior (same GameObject)";
            if (spirit.WaveTemplate == null) return "Wave Prefab";
            if (warning == null) return "Warning";
            return "Current Behavior";
        }

        private void SwitchTo(WaterSpiritBehavior behavior)
        {
            currentBehavior?.Exit(this);
            pendingBehavior = null;
            currentBehavior = behavior;
            initialized = true;
            nextRush = false;
            warning.Hide();
            spirit.SetMoveVelocity(Vector2.zero);
            currentBehavior.Enter(this);
            ChangePhase(Phase.Tracking);
        }

        private void ChangePhase(Phase phase) { CurrentPhase = phase; elapsed = 0f; }

        private void Suspend()
        {
            if (initialized) currentBehavior?.Exit(this);
            initialized = false;
            pendingBehavior = null;
            currentBehavior = null;
            if (warning != null) warning.Hide();
            spirit?.CancelAction();
            ChangePhase(Phase.Suspended);
        }

        internal void Restart(bool deferStart)
        {
            Suspend();
            pausedForRespawn = deferStart;
        }

        internal void ResumeAfterRespawn() => pausedForRespawn = false;
        private void OnDisable()
        {
            Camera.onPreCull -= BeforeCamera;
            UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering -= BeforePipelineCamera;
            Suspend();
        }
        private void OnDestroy() { if (spirit != null) spirit.AttachAI(null); }
    }
}


