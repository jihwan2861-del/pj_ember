using NUnit.Framework;
using UnityEngine;

namespace EmberMovementRegression
{
    public sealed partial class InputAndPhysicsTests
    {
        private static Component CreateSpring(Fixture f)
        {
            BoxCollider2D collider = f.Box(f.Body.position + Vector2.down * 0.4f, new Vector2(1f, 0.2f));
            Component spring = collider.gameObject.AddComponent(RuntimeType("EmberPrototype.Spring"));
            Call(spring, "Awake");
            return spring;
        }

        [Test]
        public void SpringBouncesUpPreservesHorizontalSpeedAndRestoresAbilities()
        {
            using (var f = new Fixture())
            {
                Component spring = CreateSpring(f);
                f.Body.linearVelocity = new Vector2(2f, -4f);
                Set(f.Player, "airJumpsRemaining", 0);
                Set(f.Player, "burstAvailable", false);
                Call(spring, "OnTriggerEnter2D", f.BodyCollider);
                Assert.That(f.Body.linearVelocity, Is.EqualTo(new Vector2(2f, 10f)));
                Assert.That(Get<int>(f.Player, "airJumpsRemaining"), Is.EqualTo(1));
                Assert.That(Get<bool>(f.Player, "burstAvailable"), Is.True);
                Assert.That(Get<bool>(f.Player, "wasGrounded"), Is.False);
                Step(f, 1);
                Assert.That(f.Body.linearVelocity.y, Is.GreaterThan(9f), "Releasing jump must not cut the spring launch.");
            }
        }

        [Test]
        public void SpringIgnoresAscendingSideAndBottomContacts()
        {
            using (var f = new Fixture())
            {
                Component spring = CreateSpring(f);
                f.Body.linearVelocity = Vector2.up * 3f;
                Call(spring, "OnTriggerEnter2D", f.BodyCollider);
                Assert.That(f.Body.linearVelocity.y, Is.EqualTo(3f));
                f.Body.position += Vector2.down;
                f.Body.linearVelocity = Vector2.down * 4f;
                Physics2D.SyncTransforms();
                Call(spring, "OnTriggerEnter2D", f.BodyCollider);
                Assert.That(f.Body.linearVelocity.y, Is.EqualTo(-4f));
            }
        }

        [Test]
        public void SpringDoesNotRetriggerWhileRisingAndSupportsTunedSpeed()
        {
            using (var f = new Fixture())
            {
                Component spring = CreateSpring(f);
                Set(spring, "bounceSpeed", 12f);
                f.Body.linearVelocity = Vector2.down;
                Call(spring, "OnTriggerEnter2D", f.BodyCollider);
                Assert.That(f.Body.linearVelocity.y, Is.EqualTo(12f));
                f.Body.linearVelocity = Vector2.up * 8f;
                Call(spring, "OnTriggerStay2D", f.BodyCollider);
                Assert.That(f.Body.linearVelocity.y, Is.EqualTo(8f));
            }
        }

        [Test]
        public void SpringRespectsControlLockAndAbilityStates()
        {
            using (var f = new Fixture())
            {
                Component spring = CreateSpring(f);
                Call(f.Player, "SetControlsLocked", true);
                Call(spring, "OnTriggerEnter2D", f.BodyCollider);
                Assert.That(f.Body.linearVelocity, Is.EqualTo(Vector2.zero));
                Call(f.Player, "SetControlsLocked", false);
                Call(f.Player, "RequestIgnitionRing");
                Call(spring, "OnTriggerStay2D", f.BodyCollider);
                Assert.That(f.State, Is.EqualTo("Bursting"));
                Assert.That(f.Body.linearVelocity, Is.EqualTo(Vector2.zero));
            }
        }

        [Test]
        public void SpringAllowsAirJumpAfterBounce()
        {
            using (var f = new Fixture())
            {
                Component spring = CreateSpring(f);
                Call(spring, "OnTriggerEnter2D", f.BodyCollider);
                Frame(f, UnityEngine.InputSystem.Key.Z);
                Step(f, 1);
                Assert.That(Get<int>(f.Player, "airJumpsRemaining"), Is.Zero);
                Assert.That(f.Body.linearVelocity.y, Is.GreaterThan(6f));
            }
        }

        [Test]
        public void SpringCatchesFastFallAcrossTopWithinOnePhysicsStep()
        {
            using (var f = new Fixture())
            {
                Component spring = CreateSpring(f);
                f.Body.position += Vector2.down * 0.3f;
                f.Body.linearVelocity = Vector2.down * 20f;
                Physics2D.SyncTransforms();
                Call(spring, "OnTriggerEnter2D", f.BodyCollider);
                Assert.That(f.Body.linearVelocity.y, Is.EqualTo(10f));
            }
        }
    }
}
