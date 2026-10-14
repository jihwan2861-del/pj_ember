using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EmberMovementRegression
{
    public sealed partial class InputAndPhysicsTests
    {
        [Test]
        public void FireExitXWithoutTargetDoesNotStartDash()
        {
            using (var f = new Fixture())
            {
                ReachAnchor(f);
                Frame(f, Key.X);
                Assert.That(f.State, Is.EqualTo("Anchored"));
                Frame(f);
                Assert.That(f.State, Is.EqualTo("Anchored"));
            }
        }

        [Test]
        public void FireExitZLaunchesWithoutAirJumpAndLeavesDash()
        {
            using (var f = new Fixture())
            {
                ReachAnchor(f);
                Frame(f, Key.RightArrow, Key.Z);
                Assert.That(f.State, Is.EqualTo("Free"));
                Assert.That(f.Body.linearVelocity.x, Is.GreaterThan(0f));
                Step(f, 1);
                Assert.That(Get<int>(f.Player, "airJumpsRemaining"), Is.EqualTo(1));
                Frame(f, Key.RightArrow, Key.Z);
                Assert.That(Get<float>(f.Player, "jumpRemaining"), Is.LessThanOrEqualTo(0f));
                Frame(f, Key.RightArrow, Key.X);
                Assert.That(f.State, Is.EqualTo("BurstDashing"));
            }
        }

        [Test]
        public void FireExitXTransfersImmediatelyOnPress()
        {
            using (var f = new Fixture())
            {
                ReachAnchor(f);
                f.Fire(f.Body.position + Vector2.right * 2f);
                Frame(f, Key.RightArrow, Key.X);
                Assert.That(f.State, Is.EqualTo("Travelling"));
                Step(f, 40);
                Assert.That(f.State, Is.EqualTo("Anchored"));
                Frame(f, Key.RightArrow, Key.X);
                Assert.That(f.State, Is.EqualTo("Anchored"));
                Frame(f);
                Assert.That(f.State, Is.EqualTo("Anchored"));
            }
        }

        [Test]
        public void FireExitZTakesPriorityOverX()
        {
            using (var f = new Fixture())
            {
                ReachAnchor(f);
                f.Fire(f.Body.position + Vector2.right * 2f);
                Frame(f, Key.RightArrow, Key.Z, Key.X);
                Assert.That(f.State, Is.EqualTo("Free"));
            }
        }
    }
}
