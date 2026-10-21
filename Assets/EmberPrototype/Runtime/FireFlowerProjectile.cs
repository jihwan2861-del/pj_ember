using System.Collections.Generic;
using UnityEngine;

namespace EmberPrototype
{
    [DefaultExecutionOrder(-50)]
    public sealed class FireFlowerProjectile : MonoBehaviour
    {
        private const float MaximumRideSeconds = 2f;
        private const float PostRideSeconds = 1f;
        private static readonly List<FireFlowerProjectile> activeShots = new List<FireFlowerProjectile>();
        private readonly RaycastHit2D[] hits = new RaycastHit2D[64];
        private Vector2 direction;
        private float launchSpeed, speed, radius, remainingRange;
        private Transform owner;
        private bool stopped;
        private bool initialized;
        private bool afterRide;
        private float rideRemaining;
        private float afterRideRemaining;
        private float steeringVelocity;
        private FlamePlayerController rider;
        [SerializeField] private Sprite[] animationFrames;
        [SerializeField, Min(1f)] private float framesPerSecond = 12f;
        [Header("Flower Ride")]
        [SerializeField, Tooltip("태양꽃 불꽃의 탑승 기능입니다. 첫 탑승은 최대 2초, 하차 뒤에는 1초 직진합니다.")]
        private bool rideable;
        [SerializeField, Min(1f)] private float steeringDegreesPerSecond = 300f;
        [SerializeField, Min(0.01f)] private float steeringSmoothTime = 0.12f;
        [Header("Ride Acceleration")]
        [SerializeField] private bool accelerateWhileRiding = true;
        [SerializeField, Range(0.05f, 1f), Tooltip("탑승 직후 속도 / 태양꽃의 Projectile Speed")]
        private float rideStartSpeedMultiplier = 0.6f;
        [SerializeField, Min(0.05f), Tooltip("탑승 최고 속도 / 태양꽃의 Projectile Speed")]
        private float rideMaxSpeedMultiplier = 1.2f;
        [SerializeField, Min(0f), Tooltip("탑승 중 초당 증가하는 속도(U/게임초²). 0이면 가속하지 않습니다.")]
        private float rideAcceleration = 4.5f;
        [Header("Despawn")]
        [SerializeField, Tooltip("충돌·사거리·하차 후 수명 종료 때 재생할 일회성 파티클. 리셋/씬 종료에는 재생하지 않습니다.")]
        private ParticleSystem despawnParticlesPrefab;
        [Header("Despawn Screen Shake")]
        [SerializeField, Tooltip("화면 안에서 불꽃이 터질 때 기존 룸 카메라를 살짝 흔듭니다.")]
        private bool shakeOnDespawn;
        [SerializeField, Min(0f), Tooltip("소멸 쉐이크 지속 시간(게임초).")]
        private float despawnShakeDuration = 0.12f;
        [SerializeField, Min(0f), Tooltip("소멸 쉐이크 강도(월드 단위).")]
        private float despawnShakeMagnitude = 0.05f;
        private float animationTime;

        public bool CanBoard => rideable && initialized && !stopped && !HasBeenRidden
            && rider == null && isActiveAndEnabled;
        public bool HasBeenRidden { get; private set; }
        public bool HasRider => rider != null;
        public Vector2 Direction => direction;
        public float CurrentSpeed => speed;
        public Vector2 Velocity => direction * speed;
        public float RideSecondsRemaining => CanBoard ? MaximumRideSeconds : rideRemaining;
        public float AfterRideSecondsRemaining => afterRideRemaining;

        internal bool CanActivateFlower(FireFlower flower)
        {
            if (!initialized || stopped || !isActiveAndEnabled || flower == null || owner == null
                || flower.gameObject.scene != gameObject.scene) return false;
            FireFlower sourceFlower = owner.GetComponentInParent<FireFlower>();
            return sourceFlower != null && sourceFlower != flower;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRegistry() => activeShots.Clear();

        private void OnEnable()
        {
            if (!activeShots.Contains(this)) activeShots.Add(this);
        }

        private void OnDisable()
        {
            activeShots.Remove(this);
            stopped = true;
            FlamePlayerController attached = rider;
            rider = null;
            rideRemaining = afterRideRemaining = 0f;
            if (attached != null) attached.OnFlowerProjectileLost(this);
        }

        private void Update()
        {
            if (rideable && GameSessionControls.BlockGameplayInput) return;
            if (animationFrames == null || animationFrames.Length == 0) return;
            animationTime += Time.deltaTime;
            SpriteRenderer visual = GetComponent<SpriteRenderer>();
            if (visual != null)
                visual.sprite = animationFrames[(int)(animationTime * framesPerSecond) % animationFrames.Length];
        }

        public static FireFlowerProjectile Spawn(Vector2 origin, Vector2 direction, float speed,
            float range, float radius, Transform owner, FireFlowerProjectile prefab = null)
        {
            Vector2 forward = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;
            Quaternion rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg);
            var root = prefab != null ? Instantiate(prefab.gameObject, origin, rotation)
                : new GameObject("Flower purification flame");
            if (owner != null)
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root, owner.gameObject.scene);
            root.transform.SetPositionAndRotation(origin, rotation);
            if (prefab == null)
            {
                root.transform.localScale = Vector3.one * (radius * 2f);
                root.AddComponent<PrototypeSprite>().Color = new Color(1f, 0.78f, 0.15f);
            }
            SpriteRenderer visual = root.GetComponent<SpriteRenderer>();
            if (visual != null) visual.sortingOrder = 40;
            var shot = root.GetComponent<FireFlowerProjectile>();
            if (shot == null) shot = root.AddComponent<FireFlowerProjectile>();
            shot.direction = forward;
            shot.launchSpeed = shot.speed = Mathf.Max(0.1f, speed);
            shot.radius = Mathf.Max(0.01f, radius);
            shot.remainingRange = Mathf.Max(0.1f, range);
            shot.owner = owner;
            shot.initialized = true;
            return shot;
        }

        internal static FireFlowerProjectile FindBoardable(FlamePlayerController player,
            Vector2 origin, Vector2 aim, float range, float minimumDirectionDot)
        {
            FireFlowerProjectile selected = null;
            float bestScore = float.NegativeInfinity;
            for (int i = activeShots.Count - 1; i >= 0; i--)
            {
                FireFlowerProjectile shot = activeShots[i];
                if (shot == null || !shot.CanBoard || shot.gameObject.scene != player.gameObject.scene) continue;
                Vector2 offset = (Vector2)shot.transform.position - origin;
                float distance = offset.magnitude;
                if (distance > range) continue;
                float alignment = distance > 0.001f ? Vector2.Dot(offset / distance, aim) : 1f;
                if (alignment < minimumDirectionDot) continue;
                float score = alignment * 2f - distance / Mathf.Max(0.01f, range);
                if (score <= bestScore) continue;
                selected = shot;
                bestScore = score;
            }
            return selected;
        }

        internal bool TryAttachRider(FlamePlayerController player)
        {
            if (!CanBoard || player == null) return false;
            rider = player;
            HasBeenRidden = true;
            rideRemaining = MaximumRideSeconds;
            afterRideRemaining = 0f;
            steeringVelocity = 0f;
            if (accelerateWhileRiding && rideAcceleration > 0f)
                speed = launchSpeed * Mathf.Clamp(rideStartSpeedMultiplier, 0.05f, 1f);
            return true;
        }

        internal void ReleaseRider(FlamePlayerController player)
        {
            if (rider != player) return;
            rider = null;
            rideRemaining = 0f;
            afterRide = true;
            afterRideRemaining = stopped ? 0f : PostRideSeconds;
        }

        private void FixedUpdate()
        {
            if (stopped || !initialized) return;
            if (rideable && (GameSessionControls.BlockGameplayInput
                || (rider != null && (rider.ControlsLocked || rider.IsDying)))) return;

            float remainingStep = Time.fixedDeltaTime;
            if (rider != null)
            {
                float rideStep = Mathf.Min(remainingStep, rideRemaining);
                Vector2 input = EmberPlayerInput.MovementDirection();
                if (input.sqrMagnitude > 0.0001f)
                {
                    float angle = Mathf.SmoothDampAngle(Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg,
                        Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg, ref steeringVelocity,
                        steeringSmoothTime, steeringDegreesPerSecond, rideStep);
                    direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                }
                else steeringVelocity = 0f;
                if (!AdvanceDistance(AdvanceRideSpeed(rideStep), false)) return;
                rideRemaining = Mathf.Max(0f, rideRemaining - rideStep);
                remainingStep -= rideStep;
                // Repeated fixed-step subtraction can leave a few millionths of a second.
                // End on the intended physics tick instead of granting an extra tick.
                if (rideRemaining <= 0.00001f)
                {
                    rideRemaining = 0f;
                    rider.EndFlowerRide(this, Velocity, true);
                }
                else return;
            }

            if (afterRide)
            {
                // Consume only the remainder of this physics tick at the phase boundary.
                float tailStep = Mathf.Min(remainingStep, afterRideRemaining);
                if (tailStep > 0f && !AdvanceDistance(speed * tailStep, false)) return;
                afterRideRemaining = Mathf.Max(0f, afterRideRemaining - tailStep);
                if (afterRideRemaining <= 0.000001f) Retire();
            }
            else if (AdvanceDistance(speed * remainingStep, true) && remainingRange <= 0f) Retire();
        }

        private float AdvanceRideSpeed(float deltaTime)
        {
            if (!accelerateWhileRiding || rideAcceleration <= 0f) return speed * deltaTime;
            float targetSpeed = launchSpeed * Mathf.Max(
                Mathf.Clamp(rideStartSpeedMultiplier, 0.05f, 1f), rideMaxSpeedMultiplier);
            float previousSpeed = speed;
            speed = Mathf.MoveTowards(speed, targetSpeed, rideAcceleration * deltaTime);
            // Integrate the ramp and any time spent at the cap separately, including the final ride tick.
            float rampSeconds = Mathf.Min(deltaTime, Mathf.Abs(speed - previousSpeed) / rideAcceleration);
            return (previousSpeed + speed) * 0.5f * rampSeconds + speed * (deltaTime - rampSeconds);
        }

        private bool AdvanceDistance(float distance, bool useRange)
        {
            if (useRange) distance = Mathf.Min(distance, remainingRange);
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(Physics2D.AllLayers);
            filter.useTriggers = true;
            int count = Physics2D.CircleCast(transform.position, radius, direction, filter, hits, distance);
            int nearest = -1;
            float nearestDistance = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                Collider2D hit = hits[i].collider;
                if (hit == null || hit.transform.IsChildOf(transform)
                    || (owner != null && hit.transform.IsChildOf(owner))) continue;
                if (hit.GetComponentInParent<FlamePlayerController>() != null) continue;
                bool relevant = !hit.isTrigger || hit.GetComponentInParent<PollutionObstacle>() != null
                    || hit.GetComponentInParent<PurifiableSpikeTilemap>() != null
                    || hit.GetComponentInParent<FireFlower>() != null;
                if (rideable && (hit.GetComponentInParent<WaterContact>() != null
                    || hit.GetComponentInParent<RoomHazard>() != null)) relevant = true;
                if (relevant && hits[i].distance < nearestDistance)
                { nearest = i; nearestDistance = hits[i].distance; }
            }
            if (nearest >= 0)
            {
                float safeDistance = Mathf.Max(0f, nearestDistance - 0.03f);
                transform.position += (Vector3)(direction * safeDistance);
                if (rider != null) rider.FollowFlowerRide(this, transform.position);
                Collider2D hit = hits[nearest].collider;
                FireFlower flower = hit.GetComponentInParent<FireFlower>();
                if (flower != null) flower.TryActivateFromFlowerProjectile(this);
                else
                {
                    PollutionObstacle pollution = hit.GetComponentInParent<PollutionObstacle>();
                    if (pollution != null) pollution.TryPurifyFromFlower();
                    else hit.GetComponentInParent<PurifiableSpikeTilemap>()?.TryPurifyAt(hits[nearest].point, direction);
                }
                // Consume even a rejected hit so overlap cannot retry when cooldown expires.
                Retire();
                return false;
            }
            transform.position += (Vector3)(direction * distance);
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            if (rider != null) rider.FollowFlowerRide(this, transform.position);
            if (useRange) remainingRange = Mathf.Max(0f, remainingRange - distance);
            return true;
        }

        private void Retire()
        {
            if (stopped) return;
            stopped = true;
            if (rider != null) rider.EndFlowerRide(this, Velocity, true);
            rideRemaining = afterRideRemaining = 0f;
            PlayDespawnParticles();
            ShakeDespawnCamera();
            gameObject.SetActive(false);
            if (Application.isPlaying) Destroy(gameObject);
            else DestroyImmediate(gameObject);
        }

        private void ShakeDespawnCamera()
        {
            if (!Application.isPlaying || !shakeOnDespawn || Time.timeScale <= 0f
                || SceneFadeTransition.IsPlaying || despawnShakeDuration <= 0f
                || despawnShakeMagnitude <= 0f) return;

            foreach (Camera camera in Camera.allCameras)
            {
                if (camera.gameObject.scene != gameObject.scene
                    || (camera.cullingMask & (1 << gameObject.layer)) == 0) continue;
                CelesteRoomCamera roomCamera = camera.GetComponent<CelesteRoomCamera>();
                if (roomCamera == null || !roomCamera.isActiveAndEnabled || roomCamera.IsMotionSuspended) continue;
                Vector3 viewport = camera.WorldToViewportPoint(transform.position);
                if (viewport.z < camera.nearClipPlane || viewport.z > camera.farClipPlane
                    || viewport.x < 0f || viewport.x > 1f || viewport.y < 0f || viewport.y > 1f) continue;
                roomCamera.Shake(despawnShakeDuration, despawnShakeMagnitude);
                return;
            }
        }

        private void PlayDespawnParticles()
        {
            if (!Application.isPlaying || despawnParticlesPrefab == null) return;
            ParticleSystem particles = Instantiate(despawnParticlesPrefab, transform.position, Quaternion.identity);
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(particles.gameObject, gameObject.scene);
            particles.gameObject.layer = gameObject.layer;
            var main = particles.main;
            main.loop = false;
            main.stopAction = ParticleSystemStopAction.Destroy;
            SpriteRenderer visual = GetComponent<SpriteRenderer>();
            if (visual != null)
            {
                int order = visual.sortingOrder + 1;
                foreach (var renderer in particles.GetComponentsInChildren<ParticleSystemRenderer>(true))
                {
                    renderer.gameObject.layer = gameObject.layer;
                    renderer.sortingLayerID = visual.sortingLayerID;
                    renderer.sortingOrder = order++;
                }
            }
            particles.Play(true);
        }
    }
}
