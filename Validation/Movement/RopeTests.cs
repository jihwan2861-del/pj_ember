using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EmberMovementRegression
{
    public sealed partial class InputAndPhysicsTests
    {
        private static Component CreateRope(Fixture f, Vector2 pivot, float angle = 0f, float angularSpeed = 0f)
        {
            GameObject root = new GameObject("Test Rope");
            root.transform.position = pivot;
            GameObject end = new GameObject("End");
            end.transform.SetParent(root.transform);
            end.AddComponent<CircleCollider2D>().isTrigger = true;
            Component tile = end.AddComponent(RuntimeType("EmberPrototype.FlammableTile"));
            Call(tile, "Awake");
            Component rope = root.AddComponent(RuntimeType("EmberPrototype.BurningRope"));
            Call(rope, "Configure", tile, 3f, angle, angularSpeed);
            f.Track(root);
            return rope;
        }

        [Test]
        public void RopeKeepsLengthAcceleratesDownwardAndRespondsToInput()
        {
            using (var f = new Fixture())
            {
                Component rope = CreateRope(f, new Vector2(0f, 5f), 45f);
                float speedBefore = ((Vector2)Property(rope, "EndVelocity")).magnitude;
                for (int i = 0; i < 20; i++) Call(rope, "Advance", Tick, 0f);
                Assert.That(Vector2.Distance((Vector2)Property(rope, "Pivot"), (Vector2)Property(rope, "EndPosition")), Is.EqualTo(3f).Within(0.0001f));
                Assert.That(((Vector2)Property(rope, "EndVelocity")).magnitude, Is.GreaterThan(speedBefore + 1f));
                Call(rope, "ResetMotion");
                Call(rope, "Configure", Property(rope, "EndFire"), 3f, 0f, 0f);
                Call(rope, "Advance", Tick, 1f);
                Assert.That(((Vector2)Property(rope, "EndVelocity")).x, Is.GreaterThan(0f));
            }
        }

        [Test]
        public void RopeOnlyHasOneFireAtItsEndAndRequiresIgnition()
        {
            using (var f = new Fixture())
            {
                Component rope = CreateRope(f, new Vector2(0f, 5f));
                Assert.That((bool)Property(rope, "IsAvailable"), Is.False);
                Component tile = (Component)Property(rope, "EndFire");
                Call(tile, "TryIgnite");
                Assert.That((bool)Property(rope, "IsAvailable"), Is.True);
                Assert.That(rope.GetComponentsInChildren(RuntimeType("EmberPrototype.FlammableTile")).Length, Is.EqualTo(1));
                Assert.That((Vector2)Property(tile, "AnchorPosition"), Is.EqualTo((Vector2)Property(rope, "EndPosition")));
            }
        }

        private void ReachRope(Fixture f, Component rope, Vector2 incoming)
        {
            Component tile = (Component)Property(rope, "EndFire");
            Call(tile, "TryIgnite");
            Physics2D.SyncTransforms();
            f.Body.linearVelocity = incoming;
            Frame(f, Key.RightArrow, Key.X);
            Assert.That(f.State, Is.EqualTo("Travelling"));
            int guard = 100;
            while (f.State == "Travelling" && guard-- > 0)
            {
                Call(rope, "FixedUpdate");
                Step(f, 1);
            }
            Assert.That(f.State, Is.EqualTo("RopeAnchored"));
            Assert.That(f.BodyCollider.enabled, Is.False);
        }

        [TestCase(5f)]
        [TestCase(12f)]
        public void RopeEntryTransfersVelocityOnceAndFollowsEnd(float speed)
        {
            using (var f = new Fixture())
            {
                Component rope = CreateRope(f, new Vector2(2f, 23f));
                ReachRope(f, rope, new Vector2(speed, 0f));
                Assert.That(((Vector2)Property(rope, "EndVelocity")).x, Is.EqualTo(speed).Within(0.001f));
                Assert.That(Get<Vector2>(f.Player, "ropeEntryVelocity"), Is.EqualTo(Vector2.zero));
                Frame(f, Key.RightArrow);
                Step(f, 1);
                Assert.That(Vector2.Distance(f.Body.position, (Vector2)Property(rope, "EndPosition")), Is.LessThan(0.001f));
                Assert.That(((Vector2)Property(rope, "EndVelocity")).x, Is.LessThan(speed + 1f), "Entry must not be added twice.");
            }
        }

        [Test]
        public void RopeRadialEntryDoesNotBecomeTangentialSpeed()
        {
            using (var f = new Fixture())
            {
                Component rope = CreateRope(f, new Vector2(2f, 23f));
                ReachRope(f, rope, new Vector2(0f, 8f));
                Assert.That(((Vector2)Property(rope, "EndVelocity")).magnitude, Is.LessThan(0.001f));
                Frame(f, Key.Z, Key.LeftArrow);
                Assert.That(f.State, Is.EqualTo("Free"));
                Assert.That(f.Body.linearVelocity.magnitude, Is.LessThan(0.001f));
                Step(f, 1);
                Assert.That(f.Body.linearVelocity.y, Is.LessThan(0f));
            }
        }

        [TestCase(0f)]
        [TestCase(35f)]
        public void RopeKickUsesSwingDirectionAndLeavesOppositeImpulse(float angle)
        {
            using (var f = new Fixture())
            {
                Component rope = CreateRope(f, new Vector2(2f, 23f));
                ReachRope(f, rope, new Vector2(12f, 0f));
                Call(rope, "Configure", Property(rope, "EndFire"), 3f, angle, 180f);
                // Reattach after changing the test pose; entry velocity equals the pose's end velocity.
                Vector2 before = (Vector2)Property(rope, "EndVelocity");
                Call(rope, "Attach", f.Player, before);
                f.Body.position = (Vector2)Property(rope, "EndPosition");
                Frame(f, Key.LeftArrow, Key.DownArrow, Key.Z);
                Assert.That(f.State, Is.EqualTo("Free"));
                Vector2 launched = f.Body.linearVelocity;
                Assert.That(Vector2.Distance(launched, before + before.normalized * 4f), Is.LessThan(0.001f));
                Assert.That(Vector2.Dot((Vector2)Property(rope, "EndVelocity"), before.normalized), Is.LessThan(before.magnitude));
                Frame(f);
                Step(f, 1);
                Assert.That(f.Body.linearVelocity.x, Is.EqualTo(launched.x).Within(0.001f));
                Assert.That(Get<int>(f.Player, "airJumpsRemaining"), Is.EqualTo(1), "Z to detach must not trigger an air jump.");
                if (angle > 0f) Assert.That(f.Body.linearVelocity.y, Is.GreaterThan(launched.y - 1f), "Releasing Z must not cut a rope launch.");
                Vector2 oldEnd = (Vector2)Property(rope, "EndPosition");
                Call(rope, "FixedUpdate");
                Assert.That(Vector2.Distance(oldEnd, (Vector2)Property(rope, "EndPosition")), Is.GreaterThan(0.01f));
            }
        }

        [Test]
        public void RopeAirInputGraduallyChangesMomentumAndSpringOwnsNextVelocity()
        {
            using (var f = new Fixture())
            {
                Component rope = CreateRope(f, new Vector2(2f, 23f));
                ReachRope(f, rope, new Vector2(12f, 0f));
                Frame(f, Key.Z);
                float start = f.Body.linearVelocity.x;
                Frame(f, Key.LeftArrow);
                Step(f, 1);
                Assert.That(f.Body.linearVelocity.x, Is.EqualTo(start - 8f * Tick).Within(0.001f));
                Assert.That((bool)Call(f.Player, "TrySpringBounce", 10f), Is.True);
                Assert.That(Get<bool>(f.Player, "ropeMomentum"), Is.False);
                Assert.That(f.Body.linearVelocity.y, Is.EqualTo(10f));
            }
        }

        [Test]
        public void RopeResetAndTargetLossCleanUpAttachment()
        {
            using (var f = new Fixture())
            {
                Component rope = CreateRope(f, new Vector2(2f, 23f));
                ReachRope(f, rope, new Vector2(7f, 0f));
                Call(f.Player, "SetControlsLocked", true);
                Vector2 pausedEnd = (Vector2)Property(rope, "EndPosition");
                Step(f, 3);
                Call(rope, "FixedUpdate");
                Assert.That((Vector2)Property(rope, "EndPosition"), Is.EqualTo(pausedEnd));
                Call(f.Player, "SetControlsLocked", false);
                Call((Component)Property(rope, "EndFire"), "ResetFire");
                Step(f, 1);
                Assert.That(f.State, Is.EqualTo("Free"));
                Assert.That(f.BodyCollider.enabled, Is.True);
                Call(f.Player, "ResetAt", new Vector2(0f, 20f));
                Assert.That(Get<object>(f.Player, "activeRope"), Is.Null);
                Assert.That(Get<Vector2>(f.Player, "ropeEntryVelocity"), Is.EqualTo(Vector2.zero));
                Assert.That(((Vector2)Property(rope, "EndVelocity")).magnitude, Is.LessThan(0.001f));
            }
        }

        [Test]
        public void RopeTravelToMovingEndAndBlockedTravelClearStoredEntry()
        {
            using (var f = new Fixture())
            {
                Component rope = CreateRope(f, new Vector2(2f, 23f), 0f, 60f);
                ReachRope(f, rope, new Vector2(9f, 0f));
                Assert.That(((Vector2)Property(rope, "EndVelocity")).magnitude, Is.GreaterThan(4f));
            }
            SetKeys();
            using (var f = new Fixture())
            {
                Component rope = CreateRope(f, new Vector2(2f, 23f));
                Call((Component)Property(rope, "EndFire"), "TryIgnite");
                f.Box(new Vector2(1f, 20f), new Vector2(0.3f, 15f));
                Set(f.Player, "fireTravelCornerAssistDistance", 0f);
                f.Body.linearVelocity = new Vector2(12f, 0f);
                Frame(f, Key.RightArrow, Key.X);
                int guard = 50;
                while (f.State == "Travelling" && guard-- > 0) Step(f, 1);
                Assert.That(f.State, Is.EqualTo("Free"));
                Assert.That(f.Body.position.x, Is.LessThan(1f));
                Assert.That(Get<object>(f.Player, "pendingRope"), Is.Null);
                Assert.That(Get<Vector2>(f.Player, "ropeEntryVelocity"), Is.EqualTo(Vector2.zero));
            }
        }

        [Test]
        public void RopeVisualKeepsEndpointsAndLimitsBending()
        {
            using (var f = new Fixture())
            {
                Component rope = CreateRope(f, new Vector2(2f, 23f), 0f, 120f);
                Call((Component)Property(rope, "EndFire"), "TryIgnite");
                Call(rope, "UpdateVisual", Tick);
                Call(rope, "Advance", Tick, 0f);
                Call(rope, "UpdateVisual", Tick);
                LineRenderer line = rope.GetComponentInChildren<LineRenderer>();
                Assert.That(line.startColor.r, Is.EqualTo(1f));
                Assert.That((Vector2)line.GetPosition(0), Is.EqualTo((Vector2)Property(rope, "Pivot")));
                Assert.That(Vector2.Distance(line.GetPosition(line.positionCount - 1), (Vector2)Property(rope, "EndPosition")), Is.LessThan(0.001f));
                int mid = line.positionCount / 2;
                Vector2 straight = Vector2.Lerp((Vector2)Property(rope, "Pivot"), (Vector2)Property(rope, "EndPosition"), 0.5f);
                float bend = Vector2.Distance(straight, line.GetPosition(mid));
                Assert.That(bend, Is.GreaterThan(0f).And.LessThanOrEqualTo(0.14f));
            }
        }

        [Test]
        public void RopeChainPreservesKickIntoNextSwingAndBoundsExtremeSpeed()
        {
            using (var f = new Fixture())
            {
                Set(f.Player, "fireTravelRange", 8f);
                Component first = CreateRope(f, new Vector2(2f, 23f));
                Component second = CreateRope(f, new Vector2(7f, 23f));
                ReachRope(f, first, new Vector2(12f, 0f));
                Frame(f, Key.RightArrow, Key.Z);
                Frame(f, Key.RightArrow);
                Step(f, 1);
                float incoming = f.Body.linearVelocity.x;
                ReachRope(f, second, f.Body.linearVelocity);
                Assert.That(((Vector2)Property(second, "EndVelocity")).x, Is.EqualTo(incoming).Within(0.001f));
                Assert.That(incoming, Is.GreaterThan(15f));
                Frame(f, Key.Z);
                Assert.That(f.Body.linearVelocity.x, Is.GreaterThan(incoming));
                Assert.That(f.Body.linearVelocity.magnitude, Is.LessThanOrEqualTo(30f));
            }
            SetKeys();
            using (var f = new Fixture())
            {
                Component rope = CreateRope(f, new Vector2(2f, 23f));
                ReachRope(f, rope, new Vector2(200f, 0f));
                Assert.That(((Vector2)Property(rope, "EndVelocity")).magnitude, Is.EqualTo(24f).Within(0.001f));
                Frame(f, Key.Z);
                Assert.That(f.Body.linearVelocity.magnitude, Is.LessThanOrEqualTo(30f));
            }
        }

        [Test]
        public void RopeAttachedSweepStopsAtWallWithoutMovingPlayerThroughIt()
        {
            using (var f = new Fixture())
            {
                Component rope = CreateRope(f, new Vector2(2f, 23f));
                ReachRope(f, rope, new Vector2(12f, 0f));
                f.Box(new Vector2(2.45f, 20f), new Vector2(0.2f, 8f));
                Step(f, 1);
                Assert.That(f.Body.position.x, Is.EqualTo(2f).Within(0.001f));
                Assert.That(((Vector2)Property(rope, "EndVelocity")).magnitude, Is.LessThan(0.001f));
                Frame(f, Key.Z);
                Assert.That(f.State, Is.EqualTo("Free"));
                Assert.That(f.BodyCollider.enabled, Is.True);
            }
        }

        [Test]
        public void RopeTargetDestructionDuringTravelAndResetWhileAttachedRestoreBody()
        {
            using (var f = new Fixture())
            {
                Component rope = CreateRope(f, new Vector2(2f, 23f));
                Call((Component)Property(rope, "EndFire"), "TryIgnite");
                Physics2D.SyncTransforms();
                f.Body.linearVelocity = new Vector2(12f, 0f);
                Frame(f, Key.RightArrow, Key.X);
                UnityEngine.Object.DestroyImmediate(rope.gameObject);
                Step(f, 1);
                Assert.That(f.State, Is.EqualTo("Free"));
                Assert.That(Get<Vector2>(f.Player, "ropeEntryVelocity"), Is.EqualTo(Vector2.zero));
            }
            SetKeys();
            using (var f = new Fixture())
            {
                Component rope = CreateRope(f, new Vector2(2f, 23f));
                ReachRope(f, rope, new Vector2(8f, 0f));
                Call(f.Player, "ResetAt", new Vector2(0f, 20f));
                Assert.That(f.State, Is.EqualTo("Free"));
                Assert.That(f.BodyCollider.enabled, Is.True);
                Assert.That(f.Body.gravityScale, Is.EqualTo(1.8f));
                Assert.That(f.Body.position, Is.EqualTo(new Vector2(0f, 20f)));
                Call(rope, "FixedUpdate");
                Assert.That(((Vector2)Property(rope, "EndVelocity")).magnitude, Is.LessThan(0.001f));
            }
        }

        [Test]
        public void RopeSceneTeardownCanClearAlreadyDestroyedAfterimages()
        {
            using (var f = new Fixture())
            {
                Component afterimage = f.Player.GetComponent(RuntimeType("EmberPrototype.PlayerAfterimageEffect"));
                if (Get<Transform>(afterimage, "poolRoot") == null) Call(afterimage, "Awake");
                Transform poolRoot = Get<Transform>(afterimage, "poolRoot");
                UnityEngine.Object.DestroyImmediate(poolRoot.gameObject);
                Assert.DoesNotThrow(() => Call(afterimage, "StopTrail", true));
            }
        }

        [Test]
        public void RopeKickSettingsAndTransferCanBeTunedIndependently()
        {
            using (var f = new Fixture())
            {
                Component rope = CreateRope(f, new Vector2(2f, 23f));
                Set(rope, "entryTransfer", 0.5f);
                Set(f.Player, "ropeTravelSpeedFactor", 2f);
                Set(f.Player, "ropeKickBoost", 7f);
                ReachRope(f, rope, new Vector2(12f, 0f));
                Assert.That(((Vector2)Property(rope, "EndVelocity")).x, Is.EqualTo(6f).Within(0.001f));
                Frame(f, Key.Z);
                Assert.That(f.Body.linearVelocity.x, Is.EqualTo(13f).Within(0.001f));
            }
        }

        [Test]
        public void RopeAirMomentumEndsAtLandingTorchAndRing()
        {
            using (var f = new Fixture())
            {
                Component rope = CreateRope(f, new Vector2(2f, 23f));
                ReachRope(f, rope, new Vector2(10f, 0f));
                Frame(f, Key.Z);
                f.Box(new Vector2(4f, 19.4f), new Vector2(10f, 0.4f));
                Frame(f);
                Step(f, 30);
                Assert.That(Get<bool>(f.Player, "ropeMomentum"), Is.False);
            }
            SetKeys();
            using (var f = new Fixture())
            {
                Set(f.Player, "fireTravelRange", 8f);
                Component rope = CreateRope(f, new Vector2(2f, 23f));
                ReachRope(f, rope, new Vector2(10f, 0f));
                f.Fire(new Vector2(5f, 20f));
                Frame(f, Key.RightArrow, Key.Z);
                Frame(f, Key.RightArrow);
                Step(f, 1);
                Frame(f, Key.RightArrow, Key.X);
                Assert.That(Get<bool>(f.Player, "ropeMomentum"), Is.False);
                int guard = 100;
                while (f.State == "Travelling" && guard-- > 0) Step(f, 1);
                Assert.That(f.State, Is.EqualTo("Anchored"));
            }
            SetKeys();
            using (var f = new Fixture())
            {
                Component rope = CreateRope(f, new Vector2(2f, 23f));
                ReachRope(f, rope, new Vector2(10f, 0f));
                Frame(f, Key.Z);
                Frame(f);
                Frame(f, Key.RightArrow, Key.C, Key.Z);
                Assert.That(f.State, Is.EqualTo("BurstDashing"));
                Assert.That(Get<bool>(f.Player, "ropeMomentum"), Is.False);
            }
        }
    }
}
