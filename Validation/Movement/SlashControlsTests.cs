using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EmberMovementRegression
{
    public sealed partial class InputAndPhysicsTests
    {
        [Test]
        public void ArrowControlsZJumpsWithoutAnAbility()
        {
            using (var f = new Fixture())
            {
                Frame(f, Key.Z);
                Step(f, 1);
                Assert.That(f.Body.linearVelocity.y, Is.GreaterThan(0f));
                Assert.That(Get<int>(f.Player, "airJumpsRemaining"), Is.EqualTo(0));
            }
        }

        [Test]
        public void ArrowControlsXOnlyDashesAndCannotRepeatInAir()
        {
            using (var f = new Fixture())
            {
                Frame(f, Key.UpArrow, Key.X);
                Assert.That(f.State, Is.EqualTo("BurstDashing"));
                Assert.That(Get<int>(f.Player, "airJumpsRemaining"), Is.EqualTo(1));
                Step(f, 15);
                Frame(f);
                Frame(f, Key.X);
                Assert.That(f.State, Is.EqualTo("Free"));
            }
        }

        [Test]
        public void CTriggersNearbyIgnitionBurstWithoutJumping()
        {
            using (var f = new Fixture())
            {
                Frame(f, Key.C);
                Assert.That(f.State, Is.EqualTo("Bursting"));
                Assert.That(Get<float>(f.Player, "jumpRemaining"), Is.EqualTo(0f));
                UnityEngine.Object[] shots = Object.FindObjectsByType(RuntimeType("EmberPrototype.EmberProjectile"), FindObjectsSortMode.None);
                Assert.That(shots, Is.Empty);
            }
        }

        [Test]
        public void IgnitionBurstLightsNearbyFuelButNotFuelOutsideItsRadius()
        {
            using (var f = new Fixture())
            {
                Component fuel = f.Box(new Vector2(0.8f, 20f), Vector2.one * 0.15f)
                    .gameObject.AddComponent(RuntimeType("EmberPrototype.FlammableTile"));
                Component distantFuel = f.Box(new Vector2(2f, 20f), Vector2.one * 0.15f)
                    .gameObject.AddComponent(RuntimeType("EmberPrototype.FlammableTile"));
                Frame(f, Key.C);
                UnityEngine.Object[] shots = Object.FindObjectsByType(RuntimeType("EmberPrototype.EmberProjectile"), FindObjectsSortMode.None);
                Assert.That(shots, Is.Empty);
                Assert.That(f.State, Is.EqualTo("Bursting"));
                Assert.That((bool)fuel.GetType().GetProperty("IsBurning").GetValue(fuel), Is.True);
                Assert.That((bool)distantFuel.GetType().GetProperty("IsBurning").GetValue(distantFuel), Is.False);
            }
        }
    }
}
