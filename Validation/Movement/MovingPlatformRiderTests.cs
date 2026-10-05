using System;
using NUnit.Framework;
using UnityEngine;

namespace EmberMovementRegression
{
    public sealed partial class InputAndPhysicsTests
    {
        [TestCase(1f, 0f)]
        [TestCase(-1f, 0f)]
        [TestCase(0f, 1f)]
        [TestCase(0f, -1f)]
        public void IdlePlayerRidesLinearPlatformAndRemainsGrounded(float x, float y)
        {
            using (var f = new Fixture(new Vector2(0f, .54f)))
            using (var platform = new RiderPlatform(new Vector2(x, y) * 6f))
            {
                for (int i = 0; i < 120; i++) RiderStep(f, platform);
                Vector2 relative = f.Body.position - platform.Body.position;
                Assert.That(relative.x, Is.EqualTo(0f).Within(.08f));
                Assert.That(relative.y, Is.EqualTo(.54f).Within(.06f));
                Assert.That(Get<bool>(f.Player, "wasGrounded"), Is.True);
            }
        }

        [TestCase(2f)]
        [TestCase(8f)]
        public void IdlePlayerRidesCircleThroughReversalsAndWrap(float speed)
        {
            using (var f = new Fixture(new Vector2(2f, .54f)))
            using (var platform = new RiderPlatform(Vector2.zero, true))
            {
                Set(platform.Block, "moveSpeed", speed);
                for (int i = 0; i < 380; i++)
                {
                    RiderStep(f, platform);
                    Vector2 relative = f.Body.position - platform.Body.position;
                    Assert.That(relative.x, Is.EqualTo(0f).Within(.08f), "tick=" + i);
                    Assert.That(relative.y, Is.EqualTo(.54f).Within(.06f), "tick=" + i);
                }
            }
        }

        [Test]
        public void JumpLeavesPlatformAndRespawnDoesNotRetainItsVelocity()
        {
            using (var f = new Fixture(new Vector2(0f, .54f)))
            using (var platform = new RiderPlatform(new Vector2(6f, 0f)))
            {
                for (int i = 0; i < 20; i++) RiderStep(f, platform);
                Set(f.Player, "jumpRemaining", .1f);
                Set(f.Player, "jumpHeld", true);
                RiderStep(f, platform);
                Assert.That(f.Body.linearVelocity.y, Is.GreaterThan(5f));
                Assert.That(f.Body.linearVelocity.x, Is.EqualTo(0f).Within(.05f));
                Call(f.Player, "ResetAt", new Vector2(20f, 20f));
                RiderStep(f, platform);
                Assert.That(f.Body.linearVelocity.x, Is.EqualTo(0f).Within(.01f));
            }
        }

        [Test]
        public void WalkingOnPlatformUsesRelativeWalkingSpeed()
        {
            using (var f = new Fixture(new Vector2(0f, .54f)))
            using (var platform = new RiderPlatform(new Vector2(6f, 0f), false, 8f))
            {
                var material = new PhysicsMaterial2D("Rider QA frictionless") { friction = 0f, bounciness = 0f };
                try
                {
                    f.BodyCollider.sharedMaterial = material;
                    platform.Body.GetComponent<BoxCollider2D>().sharedMaterial = material;
                    for (int i = 0; i < 5; i++) RiderStep(f, platform);
                    Set(f.Player, "horizontalInput", -1f);
                    for (int i = 0; i < 11; i++) RiderStep(f, platform);
                    Vector2 beforePlayer = f.Body.position;
                    Vector2 beforePlatform = platform.Body.position;
                    RiderStep(f, platform);
                    float relativeSpeed = ((f.Body.position - beforePlayer) - (platform.Body.position - beforePlatform)).x / Tick;
                    Assert.That(relativeSpeed, Is.EqualTo(-3.3f).Within(.01f));
                }
                finally { UnityEngine.Object.DestroyImmediate(material); }
            }
        }

        [Test]
        public void CarryingIntoWallDoesNotTeleportPlayerThroughIt()
        {
            using (var f = new Fixture(new Vector2(0f, .54f)))
            using (var platform = new RiderPlatform(new Vector2(6f, 0f)))
            {
                f.Box(new Vector2(1.2f, 0f), new Vector2(.2f, 20f));
                for (int i = 0; i < 80; i++)
                {
                    RiderStep(f, platform);
                    Assert.That(f.BodyCollider.bounds.max.x, Is.LessThanOrEqualTo(1.12f));
                }
            }
        }

        [Test]
        public void PlatformStoppingDoesNotSlideItsRider()
        {
            using (var f = new Fixture(new Vector2(0f, .54f)))
            using (var platform = new RiderPlatform(new Vector2(.5f, 0f)))
            {
                for (int i = 0; i < 120; i++) RiderStep(f, platform);
                Assert.That(f.Body.position.x - platform.Body.position.x, Is.EqualTo(0f).Within(.08f));
                Assert.That(f.Body.linearVelocity.x, Is.EqualTo(0f).Within(.01f));
            }
        }

        private static void RiderStep(Fixture f, RiderPlatform platform)
        {
            Call(platform.Block, "FixedUpdate");
            Call(f.Player, "FixedUpdate");
            Physics2D.Simulate(Tick);
            Physics2D.SyncTransforms();
        }

        private sealed class RiderPlatform : IDisposable
        {
            private readonly GameObject pathObject, blockObject;
            public readonly Rigidbody2D Body;
            public readonly Component Block;

            public RiderPlatform(Vector2 endpoint, bool circle = false, float width = 2f)
            {
                pathObject = new GameObject("Rider QA path");
                if (!circle)
                    for (int i = 0; i < 2; i++)
                    {
                        var point = new GameObject("Point " + i);
                        point.transform.SetParent(pathObject.transform);
                        point.transform.position = i == 0 ? Vector2.zero : endpoint;
                    }
                Component path = pathObject.AddComponent(RuntimeType("EmberPrototype.MovementPath"));
                if (circle) Set(path, "shape", Enum.ToObject(path.GetType().GetNestedType("PathShape"), 2));
                Call(path, "Rebuild");
                blockObject = new GameObject("Rider QA platform");
                blockObject.AddComponent<BoxCollider2D>().size = new Vector2(width, .5f);
                Block = blockObject.AddComponent(RuntimeType("EmberPrototype.PathMovingBlock"));
                Body = blockObject.GetComponent<Rigidbody2D>();
                Set(Block, "path", path);
                Set(Block, "moveSpeed", 2f);
                if (circle) Set(Block, "travelMode", Enum.ToObject(Block.GetType().GetNestedType("TravelMode"), 2));
                Call(Block, "Awake");
                Call(Block, "Activate");
                Physics2D.SyncTransforms();
            }

            public void Dispose()
            {
                UnityEngine.Object.DestroyImmediate(blockObject);
                UnityEngine.Object.DestroyImmediate(pathObject);
            }
        }
    }
}
