using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace EmberMovementRegression
{
    // Run in an isolated project. Runtime sources are copied unchanged into its Assets/Runtime.
    // Reflection allows tests to reference the original Assembly-CSharp without changing its layout.
    [TestFixture]
    public sealed partial class InputAndPhysicsTests
    {
        private const float Tick = 0.02f;
        private Keyboard keyboard;
        private InputSettings.UpdateMode originalInputMode;
        private SimulationMode2D originalSimulationMode;
        private Vector2 originalGravity;
        private float originalFixedDeltaTime;
        private float originalTimeScale;
        private object inputManager;
        private bool originalRunPlayerUpdatesInEditMode;
        private HashSet<Key> previousKeys;

        [SetUp]
        public void SetUp()
        {
            originalInputMode = InputSystem.settings.updateMode;
            originalSimulationMode = Physics2D.simulationMode;
            originalGravity = Physics2D.gravity;
            originalFixedDeltaTime = Time.fixedDeltaTime;
            originalTimeScale = Time.timeScale;
            inputManager = typeof(InputSystem).GetField("s_Manager", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            PropertyInfo editModePlayerUpdates = inputManager.GetType().GetProperty("runPlayerUpdatesInEditMode", Flags);
            originalRunPlayerUpdatesInEditMode = (bool)editModePlayerUpdates.GetValue(inputManager);
            editModePlayerUpdates.SetValue(inputManager, true);
            InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsManually;
            Physics2D.simulationMode = SimulationMode2D.Script;
            Physics2D.gravity = new Vector2(0f, -9.81f);
            Time.timeScale = 1f;
            Time.fixedDeltaTime = Tick;
            keyboard = InputSystem.AddDevice<Keyboard>();
            previousKeys = new HashSet<Key>();
            foreach (var key in keyboard.allKeys)
            {
                // Initialize inter-frame edge tracking before the first synthetic press.
                bool pressed = key.wasPressedThisFrame;
                bool released = key.wasReleasedThisFrame;
            }
            SetKeys();
        }

        [TearDown]
        public void TearDown()
        {
            if (keyboard != null) InputSystem.RemoveDevice(keyboard);
            inputManager.GetType().GetProperty("runPlayerUpdatesInEditMode", Flags).SetValue(inputManager, originalRunPlayerUpdatesInEditMode);
            InputSystem.settings.updateMode = originalInputMode;
            Physics2D.simulationMode = originalSimulationMode;
            Physics2D.gravity = originalGravity;
            Time.fixedDeltaTime = originalFixedDeltaTime;
            Time.timeScale = originalTimeScale;
        }

        [TestCase(Key.UpArrow)]
        [TestCase(Key.DownArrow)]
        public void WalkingDiagonallyPreservesHorizontalSpeed(Key vertical)
        {
            float straight;
            using (var f = new Fixture())
            {
                Frame(f, Key.RightArrow);
                Step(f, 12);
                straight = f.Body.linearVelocity.x;
                Assert.That(straight, Is.EqualTo(3.3f).Within(0.001f));
            }
            SetKeys();
            using (var f = new Fixture())
            {
                Frame(f, Key.RightArrow, vertical);
                Step(f, 12);
                Assert.That(f.Body.linearVelocity.x, Is.EqualTo(straight).Within(0.001f));
            }
        }

        [Test]
        public void IgnitionAndDashChordStartsDashInOneInputFrame()
        {
            using (var f = new Fixture())
            {
                Frame(f, Key.RightArrow, Key.C, Key.Z);
                Assert.That(f.State, Is.EqualTo("BurstDashing"));
                Assert.That(Get<int>(f.Player, "airJumpsRemaining"), Is.EqualTo(1));
                Step(f, 1);
                Assert.That(f.Body.linearVelocity.x, Is.GreaterThan(0f));
            }
        }

        [Test]
        public void DashChordBlockedImmediatelyDoesNotReplayItsZAsAnAirJump()
        {
            using (var f = new Fixture())
            {
                f.Box(f.Body.position + Vector2.right * 0.35f, new Vector2(0.1f, 1.2f));
                Frame(f, Key.RightArrow, Key.C, Key.Z);
                Assert.That(f.State, Is.EqualTo("BurstDashing"));
                int guard = 20;
                while (f.State == "BurstDashing" && guard-- > 0) Step(f, 1);
                Assert.That(f.State, Is.EqualTo("Free"));
                Frame(f, Key.RightArrow);
                Step(f, 1);
                Assert.That(Get<int>(f.Player, "airJumpsRemaining"), Is.EqualTo(1));
                Assert.That(f.Body.linearVelocity.y, Is.LessThan(1f));
            }
        }

        [Test]
        public void JumpPressedNearDashEndSurvivesIntoFreeState()
        {
            using (var f = new Fixture())
            {
                Frame(f, Key.RightArrow, Key.C, Key.Z);
                Frame(f, Key.RightArrow); // Release the Z which started the dash.
                while (Get<float>(f.Dash, "remainingDuration") > 0.041f) Step(f, 1);
                Frame(f, Key.RightArrow, Key.Z);
                int guard = 20;
                while (f.State == "BurstDashing" && guard-- > 0) Step(f, 1);
                Assert.That(f.State, Is.EqualTo("Free"));
                Step(f, 1);
                Assert.That(f.Body.linearVelocity.y, Is.GreaterThan(6f));
                Assert.That(Get<int>(f.Player, "airJumpsRemaining"), Is.EqualTo(0));
            }
        }

        [Test]
        public void ReleasedJumpIsSampledDuringRingAndRemainsReleasedAfterExpiry()
        {
            using (var f = new Fixture())
            {
                SetKeys(Key.Z);
                Call(f.Player, "Update");
                Assert.That(Get<bool>(f.Player, "jumpHeld"), Is.True);
                Frame(f, Key.C, Key.Z);
                // Disable dash for this scenario so Z does not turn the ring into a dash.
                Call(f.Unlocks, "SetUnlocked", EnumValue("EmberPrototype.PlayerAbility", "BurstDash"), false);
                Assert.That(f.State, Is.EqualTo("Bursting"));
                Frame(f, Key.C);
                Assert.That(Get<bool>(f.Player, "jumpHeld"), Is.False);
                Step(f, 18);
                Assert.That(f.State, Is.EqualTo("Free"));
                Assert.That(Get<bool>(f.Player, "jumpHeld"), Is.False);
            }
        }

        [Test]
        public void AnchorLaunchDoesNotReplayJumpPressedToLaunch()
        {
            using (var f = new Fixture())
            {
                ReachAnchor(f);
                Frame(f, Key.RightArrow, Key.Z);
                Assert.That(f.State, Is.EqualTo("Free"));
                Step(f, 1);
                Assert.That(Get<int>(f.Player, "airJumpsRemaining"), Is.EqualTo(1));
                Assert.That(f.Body.linearVelocity.y, Is.LessThan(1f));
                Frame(f, Key.RightArrow);
                Assert.That(Get<bool>(f.Player, "jumpHeld"), Is.False);
            }
        }

        [Test]
        public void ShotReleasePreservesHeldWalkingDirectionOnFirstFreePhysicsTick()
        {
            using (var f = new Fixture())
            {
                Frame(f, Key.RightArrow, Key.C);
                Frame(f, Key.RightArrow, Key.X);
                Assert.That((bool)Property(f.Player, "IsRingShotAiming"), Is.True);
                Frame(f, Key.RightArrow);
                Assert.That(f.State, Is.EqualTo("Free"));
                Assert.That(Time.timeScale, Is.EqualTo(1f).Within(0.001f));
                Step(f, 1);
                Assert.That(f.Body.linearVelocity.x, Is.GreaterThan(0f));
                foreach (UnityEngine.Object shot in Resources.FindObjectsOfTypeAll(RuntimeType("EmberPrototype.EmberProjectile")))
                    UnityEngine.Object.DestroyImmediate(((Component)shot).gameObject);
            }
        }

        [Test]
        public void HoldingForwardRetainsMoreFireLaunchSpeedThanReleasingDirection()
        {
            float forward = RunFireLaunch(false);
            SetKeys();
            float released = RunFireLaunch(true);
            Assert.That(forward, Is.GreaterThan(released + 0.5f));
            TestContext.WriteLine("Forward/released velocity after 0.16s: " + forward + " / " + released);
        }

        [Test]
        public void ReverseInputBrakesDuringFireLaunchControlLock()
        {
            using (var f = new Fixture())
            {
                ReachAnchor(f);
                Frame(f, Key.RightArrow, Key.Z);
                float launchSpeed = f.Body.linearVelocity.x;
                Assert.That(Get<float>(f.Player, "launchProtection"), Is.GreaterThan(0.1f));
                Frame(f, Key.LeftArrow);
                Step(f, 1);
                Assert.That(Get<float>(f.Player, "launchProtection"), Is.GreaterThan(0f));
                Assert.That(f.Body.linearVelocity.x, Is.LessThan(launchSpeed - 1f));
            }
        }

        [Test]
        public void JumpNearLandingWaitsForGroundInsteadOfConsumingAirJump()
        {
            using (var f = new Fixture(new Vector2(0f, 0.6751478f)))
            {
                f.Box(new Vector2(0f, -0.1f), new Vector2(10f, 0.2f));
                f.Body.linearVelocity = new Vector2(0f, -5f);
                Physics2D.SyncTransforms();
                Frame(f, Key.Z);
                Step(f, 1);
                Assert.That(f.Body.linearVelocity.y, Is.LessThan(0f));
                Assert.That(Get<int>(f.Player, "airJumpsRemaining"), Is.EqualTo(1));
                int guard = 10;
                while (f.Body.linearVelocity.y <= 0f && guard-- > 0) Step(f, 1);
                Assert.That(f.Body.linearVelocity.y, Is.GreaterThan(6f));
                Assert.That(Get<int>(f.Player, "airJumpsRemaining"), Is.EqualTo(1));
            }
        }

        [Test]
        public void AirJumpAwayFromGroundIsImmediate()
        {
            using (var f = new Fixture())
            {
                f.Body.linearVelocity = new Vector2(0f, -5f);
                Frame(f, Key.Z);
                Step(f, 1);
                Assert.That(f.Body.linearVelocity.y, Is.GreaterThan(6f));
                Assert.That(Get<int>(f.Player, "airJumpsRemaining"), Is.EqualTo(0));
            }
        }

        [TestCase(1f, 0f)]
        [TestCase(1f, 1f)]
        public void DashPreservesRequestedDirectionNearObstacle(float x, float y)
        {
            using (var f = new Fixture())
            {
                Vector2 direction = new Vector2(x, y).normalized;
                f.Box(f.Body.position + direction * 0.42f, new Vector2(0.12f, 0.26f));
                Physics2D.SyncTransforms();
                Assert.That((bool)Call(f.Dash, "TryBegin", direction, Physics2D.AllLayers), Is.True);
                Vector2 actual = (Vector2)Property(f.Dash, "Direction");
                Assert.That(Vector2.Distance(actual, direction), Is.LessThan(0.0001f));
            }
        }

        [Test]
        public void DashRestoresLastSafeFullBodyPositionWhenEndingInSmallGap()
        {
            using (var f = new Fixture())
            {
                Assert.That((bool)Call(f.Dash, "TryBegin", Vector2.right, Physics2D.AllLayers), Is.True);
                Assert.That(f.BodyCollider.enabled, Is.False);
                f.Body.position += Vector2.right;
                Vector2 safe = f.Body.position;
                Physics2D.SyncTransforms();
                Call(f.Dash, "AdvanceSpeed", Tick);
                f.Body.position += Vector2.right;
                f.Box(f.Body.position + new Vector2(0f, 0.28f), new Vector2(0.5f, 0.14f));
                Physics2D.SyncTransforms();
                Vector2 exit = (Vector2)Call(f.Dash, "End", false);
                Assert.That(Vector2.Distance(f.Body.position, safe), Is.LessThan(0.001f));
                Assert.That(exit, Is.EqualTo(Vector2.zero));
                Assert.That(f.BodyCollider.enabled, Is.True);
                Assert.That(f.DashCollider.enabled, Is.False);
                Assert.That((bool)Property(f.Dash, "NeedsSafeReset"), Is.False);
            }
        }

        [Test]
        public void DashSignalsResetWhenRecordedPositionsBecomeBlocked()
        {
            using (var f = new Fixture())
            {
                Vector2 start = f.Body.position;
                Call(f.Dash, "TryBegin", Vector2.right, Physics2D.AllLayers);
                f.Body.position += Vector2.right;
                Physics2D.SyncTransforms();
                Call(f.Dash, "AdvanceSpeed", Tick);
                f.Box(start, new Vector2(0.6f, 0.8f));
                f.Box(f.Body.position, new Vector2(0.6f, 0.8f));
                f.Body.position += Vector2.right;
                f.Box(f.Body.position, new Vector2(0.6f, 0.8f));
                Physics2D.SyncTransforms();
                Assert.That((Vector2)Call(f.Dash, "End", false), Is.EqualTo(Vector2.zero));
                Assert.That((bool)Property(f.Dash, "NeedsSafeReset"), Is.True);
                Assert.That(f.BodyCollider.enabled, Is.False);
                Assert.That(f.DashCollider.enabled, Is.False);
            }
        }

        [Test]
        public void DashKeepsFullBodyColliderWhenStartSpaceIsBlocked()
        {
            using (var f = new Fixture())
            {
                f.Box(f.Body.position + new Vector2(0f, 0.28f), new Vector2(0.5f, 0.14f));
                Physics2D.SyncTransforms();
                Call(f.Dash, "TryBegin", Vector2.right, Physics2D.AllLayers);
                Assert.That(f.BodyCollider.enabled, Is.True);
                Assert.That(f.DashCollider.enabled, Is.False);
            }
        }

        private float RunFireLaunch(bool releaseDirection)
        {
            using (var f = new Fixture())
            {
                ReachAnchor(f);
                Frame(f, Key.RightArrow, Key.Z);
                if (releaseDirection) Frame(f);
                else Frame(f, Key.RightArrow);
                Step(f, 8);
                return f.Body.linearVelocity.x;
            }
        }

        private void ReachAnchor(Fixture f)
        {
            f.Fire(f.Body.position + Vector2.right);
            Frame(f, Key.RightArrow, Key.X);
            Assert.That(f.State, Is.EqualTo("Travelling"));
            Frame(f, Key.RightArrow);
            int guard = 80;
            while (f.State == "Travelling" && guard-- > 0) Step(f, 1);
            Assert.That(f.State, Is.EqualTo("Anchored"));
        }

        private void SetKeys(params Key[] keys)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
            InputSystem.Update();
            var next = new HashSet<Key>(keys);
            foreach (Key key in new[] { Key.Z, Key.X, Key.C, Key.RightArrow, Key.LeftArrow, Key.UpArrow, Key.DownArrow })
            {
                Assert.That(keyboard[key].isPressed, Is.EqualTo(next.Contains(key)), "Synthetic held state for " + key);
                Assert.That(keyboard[key].wasPressedThisFrame, Is.EqualTo(next.Contains(key) && !previousKeys.Contains(key)), "Synthetic press edge for " + key);
                Assert.That(keyboard[key].wasReleasedThisFrame, Is.EqualTo(!next.Contains(key) && previousKeys.Contains(key)), "Synthetic release edge for " + key);
            }
            previousKeys = next;
        }

        private void Frame(Fixture f, params Key[] keys)
        {
            SetKeys(keys);
            Call(f.Player, "Update");
            Call(f.Router, "Update");
        }

        private static void Step(Fixture f, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Physics2D.SyncTransforms();
                Call(f.Player, "FixedUpdate");
                Physics2D.Simulate(Tick);
            }
        }

        private static Type RuntimeType(string name)
        {
            Type result = Type.GetType(name + ", Assembly-CSharp", true);
            return result;
        }

        private static object EnumValue(string name, string value) => Enum.Parse(RuntimeType(name), value);
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        private static object Call(object instance, string method, params object[] args)
        {
            MethodInfo info = instance.GetType().GetMethod(method, Flags);
            Assert.That(info, Is.Not.Null, "Missing runtime method " + method);
            try { return info.Invoke(instance, args); }
            catch (TargetInvocationException exception) { throw exception.InnerException ?? exception; }
        }
        private static T Get<T>(object instance, string name) => (T)instance.GetType().GetField(name, Flags).GetValue(instance);
        private static void Set(object instance, string name, object value)
        {
            FieldInfo field = instance.GetType().GetField(name, Flags);
            Assert.That(field, Is.Not.Null, "Missing runtime field " + name);
            field.SetValue(instance, value);
        }
        private static object Property(object instance, string name) => instance.GetType().GetProperty(name, Flags).GetValue(instance);

        private sealed class Fixture : IDisposable
        {
            private readonly List<GameObject> owned = new List<GameObject>();
            public readonly Rigidbody2D Body;
            public readonly BoxCollider2D BodyCollider;
            public readonly CircleCollider2D DashCollider;
            public readonly Component Player;
            public readonly Component Router;
            public readonly Component Dash;
            public readonly Component Unlocks;
            public string State => Get<object>(Player, "state").ToString();

            public Fixture() : this(new Vector2(0f, 20f)) { }
            public Fixture(Vector2 position)
            {
                GameObject root = Own("QA player");
                root.SetActive(false);
                root.transform.position = position;
                Body = root.AddComponent<Rigidbody2D>();
                Body.gravityScale = 1.8f;
                Body.constraints = RigidbodyConstraints2D.FreezeRotation;
                BodyCollider = root.AddComponent<BoxCollider2D>();
                BodyCollider.size = new Vector2(0.388545f, 0.616889f);
                BodyCollider.offset = new Vector2(0.0113015f, 0.0248522f);
                GameObject dashChild = new GameObject("Dash Collider");
                dashChild.transform.SetParent(root.transform, false);
                DashCollider = dashChild.AddComponent<CircleCollider2D>();
                DashCollider.radius = 0.15f;
                DashCollider.enabled = false;
                root.AddComponent<SpriteRenderer>();
                Player = root.AddComponent(RuntimeType("EmberPrototype.FlamePlayerController"));
                Dash = root.GetComponent(RuntimeType("EmberPrototype.BurstDashAbility"));
                Router = root.GetComponent(RuntimeType("EmberPrototype.PlayerAbilityInputRouter"));
                Unlocks = root.GetComponent(RuntimeType("EmberPrototype.PlayerAbilityUnlocks"));
                Component shot = root.GetComponent(RuntimeType("EmberPrototype.FlameShotAbility"));
                Set(Player, "moveSpeed", 3.3f);
                Set(Player, "jumpSpeed", 6.09f);
                Set(Player, "airDeceleration", 28f);
                Set(Player, "apexVelocityThreshold", 1f);
                Set(Player, "apexGravityMultiplier", 0.65f);
                Set(Player, "fallGravityMultiplier", 1.6f);
                Set(Player, "ignitionBurstChargeTime", 0.3f);
                Set(Player, "fireTravelSpeed", 19.5f);
                Set(Player, "fireTravelAccelerationTime", 0.07f);
                Set(Player, "fireTravelRange", 4f);
                Set(Player, "launchSpeed", 10f);
                Set(Player, "fireLaunchControlLockTime", 0.12f);
                Set(Player, "burstDashExitControlLockTime", 0.02f);
                Set(Player, "showAbsorbTargetLine", false);
                Set(Player, "showAbsorbTargetGlow", false);
                Set(Dash, "dashCollider", DashCollider);
                root.SetActive(true);
                if (Get<object>(Dash, "body") == null) Call(Dash, "Awake");
                if (Get<object>(shot, "body") == null) Call(shot, "Awake");
                if (Get<object>(Player, "body") == null) Call(Player, "Awake");
                if (Get<object>(Router, "player") == null) Call(Router, "Awake");
                Physics2D.SyncTransforms();
            }

            private GameObject Own(string name)
            {
                var result = new GameObject(name);
                owned.Add(result);
                return result;
            }

            public BoxCollider2D Box(Vector2 position, Vector2 size)
            {
                GameObject go = Own("QA solid");
                go.transform.position = position;
                BoxCollider2D collider = go.AddComponent<BoxCollider2D>();
                collider.size = size;
                Physics2D.SyncTransforms();
                return collider;
            }

            public void Fire(Vector2 position)
            {
                GameObject go = Own("QA fire");
                go.transform.position = position;
                go.AddComponent<BoxCollider2D>().isTrigger = true;
                Component fire = go.AddComponent(RuntimeType("EmberPrototype.FlammableTile"));
                Call(fire, "Awake");
                Call(fire, "TryIgnite");
                Physics2D.SyncTransforms();
            }

            public void Dispose()
            {
                // Runtime Destroy is deferred; explicit fixture cleanup prevents EditMode leftovers.
                var effect = Get<GameObject>(Player, "activeBurstEffect");
                if (effect != null)
                {
                    LineRenderer line = effect.GetComponent<LineRenderer>();
                    if (line != null && line.sharedMaterial != null) UnityEngine.Object.DestroyImmediate(line.sharedMaterial);
                    UnityEngine.Object.DestroyImmediate(effect);
                    Set(Player, "activeBurstEffect", null);
                }
                Component afterimage = Player.GetComponent(RuntimeType("EmberPrototype.PlayerAfterimageEffect"));
                if (afterimage != null)
                {
                    Transform pool = Get<Transform>(afterimage, "poolRoot");
                    if (pool != null)
                    {
                        Set(afterimage, "poolRoot", null);
                        UnityEngine.Object.DestroyImmediate(pool.gameObject);
                    }
                }
                foreach (GameObject go in owned) if (go != null) UnityEngine.Object.DestroyImmediate(go);
            }
        }
    }
}
