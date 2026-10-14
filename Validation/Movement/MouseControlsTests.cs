using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EmberMovementRegression
{
    public sealed partial class InputAndPhysicsTests
    {
        [TestCase()]
        [TestCase(Key.UpArrow)]
        [TestCase(Key.LeftArrow, Key.RightArrow)]
        public void ArrowControlsNoHorizontalInputStaysStill(params Key[] keys)
        {
            using (var f = new Fixture())
            {
                Frame(f, keys);
                Step(f, 12);
                Assert.That(f.Body.linearVelocity.x, Is.EqualTo(0f).Within(0.001f));
            }
        }

        [Test]
        public void ArrowControlsReleasingRightArrowStopsWalking()
        {
            using (var f = new Fixture())
            {
                Frame(f, Key.RightArrow);
                Step(f, 12);
                Frame(f);
                Step(f, 12);
                Assert.That(f.Body.linearVelocity.x, Is.EqualTo(0f).Within(0.001f));
            }
        }

        [Test]
        public void CStartsIgnitionBurstAndLightsNearbyFuel()
        {
            using (var f = new Fixture())
            {
                var fuel = f.Box(new Vector2(-0.8f, 20f), Vector2.one * 0.15f).gameObject
                    .AddComponent(RuntimeType("EmberPrototype.FlammableTile"));
                Frame(f, Key.LeftArrow, Key.C);
                UnityEngine.Object[] sceneShots = Object.FindObjectsByType(RuntimeType("EmberPrototype.EmberProjectile"), FindObjectsSortMode.None);
                Assert.That(sceneShots, Is.Empty);
                Assert.That(f.State, Is.EqualTo("Bursting"));
                Assert.That((bool)fuel.GetType().GetProperty("IsBurning").GetValue(fuel), Is.True);
            }
        }

        [Test]
        public void ArrowAimAndXControlFireTravel()
        {
            using (var f = new Fixture())
            {
                f.Fire(f.Body.position + Vector2.right);
                Frame(f, Key.RightArrow);
                Assert.That(f.State, Is.EqualTo("Free"));
                Frame(f, Key.RightArrow, Key.X);
                Assert.That(f.State, Is.EqualTo("Travelling"));
                Step(f, 40);
                Assert.That(f.State, Is.EqualTo("Anchored"));
                Frame(f, Key.LeftArrow, Key.Z);
                Assert.That(f.State, Is.EqualTo("Free"));
                Assert.That(f.Body.linearVelocity.x, Is.LessThan(0f));
            }
        }

        [Test]
        public void ArrowControlsRightWalksWithoutJumping()
        {
            using (var f = new Fixture())
            {
                Frame(f, Key.RightArrow);
                Step(f, 12);
                Assert.That(f.Body.linearVelocity.x, Is.EqualTo(3.3f).Within(0.01f));
                Assert.That(Get<int>(f.Player, "airJumpsRemaining"), Is.EqualTo(1));
            }
        }
    }
}
