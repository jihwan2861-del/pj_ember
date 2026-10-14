using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EmberMovementRegression
{
    public sealed partial class InputAndPhysicsTests
    {
        private Component CreateFlower(Fixture f, Vector2 position)
        {
            var root = new GameObject("QA fire flower");
            root.transform.position = position;
            f.Track(root);
            Component flower = root.AddComponent(RuntimeType("EmberPrototype.FireFlower"));
            Call(flower, "Awake");
            Call(flower, "OnEnable");
            return flower;
        }
        [Test]
        public void FireFlowerCActivationLocksPositionAndIgnoresMovementAndRepeat()
        {
            using (var f = new Fixture(new Vector2(1f, 20f)))
            {
                Component flower = CreateFlower(f, new Vector2(0f, 20f));
                Frame(f, Key.C);
                Assert.That((bool)Property(flower, "IsPreparingShot"), Is.True);
                Assert.That((Vector2)Property(flower, "LockedTarget"), Is.EqualTo(new Vector2(1f, 20f)));
                f.Body.position = new Vector2(2f, 22f);
                Assert.That((bool)Call(flower, "TryActivate", f.Body.position), Is.False);
                Assert.That((Vector2)Property(flower, "LockedTarget"), Is.EqualTo(new Vector2(1f, 20f)));
                Set(flower, "remaining", -1f);
                Call(flower, "Update");
                var shots = Object.FindObjectsByType(RuntimeType("EmberPrototype.FireFlowerProjectile"), FindObjectsSortMode.None);
                Assert.That(shots, Has.Length.EqualTo(1));
                Component shot = (Component)shots[0];
                f.Track(shot.gameObject);
                Physics2D.SyncTransforms();
                Call(shot, "FixedUpdate");
                Assert.That(shot.transform.position.y, Is.EqualTo(20f).Within(0.001f));
                Assert.That(shot.transform.position.x, Is.GreaterThan(0f));
                Call(flower, "ResetForRespawn");
                Assert.That((bool)Call(flower, "TryActivate", new Vector2(8f, 20f)), Is.False);
            }
        }
        [TestCase(false)]
        [TestCase(true)]
        public void FireFlowerProjectilePurifiesPollutionAndStopsAtWalls(bool wall)
        {
            using (var f = new Fixture(new Vector2(10f, 20f)))
            {
                Component flower = CreateFlower(f, new Vector2(0f, 20f));
                var barrier = f.Box(new Vector2(2f, 20f), new Vector2(0.4f, 1f));
                Component pollution = barrier.gameObject.AddComponent(RuntimeType("EmberPrototype.PollutionObstacle"));
                Call(pollution, "Awake");
                if (wall) f.Box(new Vector2(1f, 20f), new Vector2(0.3f, 1f));
                Assert.That((bool)Call(flower, "TryActivate", new Vector2(1f, 20f)), Is.True);
                Set(flower, "remaining", -1f);
                Call(flower, "Update");
                var shots = Object.FindObjectsByType(RuntimeType("EmberPrototype.FireFlowerProjectile"), FindObjectsSortMode.None);
                Assert.That(shots, Has.Length.EqualTo(1));
                Component shot = (Component)shots[0];
                f.Track(shot.gameObject);
                for (int i = 0; i < 20 && shot != null && shot.gameObject.activeSelf; i++)
                {
                    Physics2D.SyncTransforms();
                    Call(shot, "FixedUpdate");
                }
                Assert.That((bool)Property(pollution, "IsPurified"), Is.EqualTo(!wall));
                if (!wall)
                {
                    Assert.That(barrier.enabled, Is.False);
                    Call(pollution, "ResetForRespawn");
                    Assert.That(barrier.enabled, Is.True);
                    Assert.That((bool)Property(pollution, "IsPurified"), Is.False);
                }
            }
        }
        [Test]
        public void FireFlowerPollutionIsNotPurifiedByDirectCBurst()
        {
            using (var f = new Fixture())
            {
                Component pollution = f.Box(new Vector2(0.8f, 20f), Vector2.one)
                    .gameObject.AddComponent(RuntimeType("EmberPrototype.PollutionObstacle"));
                Call(pollution, "Awake");
                Frame(f, Key.C);
                Assert.That((bool)Property(pollution, "IsPurified"), Is.False);
            }
        }
        [Test]
        public void FireFlowerMapResetCancelsWarningAndRestoresPurifiedBarrier()
        {
            using (var f = new Fixture(new Vector2(10f, 20f)))
            {
                Component flower = CreateFlower(f, new Vector2(0f, 20f));
                Component pollution = f.Box(new Vector2(2f, 20f), Vector2.one)
                    .gameObject.AddComponent(RuntimeType("EmberPrototype.PollutionObstacle"));
                Call(pollution, "Awake");
                Call(flower, "TryActivate", new Vector2(1f, 20f));
                Call(pollution, "TryPurifyFromFlower");
                RuntimeType("EmberPrototype.MapReset").GetMethod("ResetScene")
                    .Invoke(null, new object[] { f.Player.gameObject.scene, false });
                Assert.That((bool)Property(flower, "IsPreparingShot"), Is.False);
                Assert.That((bool)Property(pollution, "IsPurified"), Is.False);
                Assert.That(pollution.GetComponent<Collider2D>().enabled, Is.True);
            }
        }
    }
}
