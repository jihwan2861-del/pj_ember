using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ParallaxValidation
{
    public sealed class ParallaxTests
    {
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
        private GameObject cameraObject;
        private Camera camera;
        private GameObject root;
        private SpriteRenderer source;
        private Component layer;
        private Texture2D texture;
        private Sprite sprite;
        private GameObject parent;

        [SetUp]
        public void SetUp()
        {
            cameraObject = new GameObject("Parallax QA camera");
            camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.aspect = 16f / 9f;
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            root = new GameObject("Parallax QA background");
            root.SetActive(false);
            texture = new Texture2D(160, 90);
            sprite = Sprite.Create(texture, new Rect(0, 0, 160, 90), new Vector2(0.5f, 0.5f), 10f, 0, SpriteMeshType.FullRect);
            source = root.AddComponent<SpriteRenderer>();
            source.sprite = sprite;
            Type type = Type.GetType("EmberPrototype.ParallaxLayer, Assembly-CSharp", true);
            layer = root.AddComponent(type);
            Set("cameraTransform", cameraObject.transform);
            Set("followX", 0.8f);
            Set("followY", 0.8f);
            Set("repeatX", true);
            Call("OnEnable");
        }

        [TearDown]
        public void TearDown()
        {
            if (layer != null) Call("OnDisable");
            if (root != null) Object.DestroyImmediate(root);
            if (parent != null) Object.DestroyImmediate(parent);
            if (cameraObject != null) Object.DestroyImmediate(cameraObject);
            if (sprite != null) Object.DestroyImmediate(sprite);
            if (texture != null) Object.DestroyImmediate(texture);
        }

        private void Set(string field, object value) => layer.GetType().GetField(field, Flags).SetValue(layer, value);
        private void Call(string method) => layer.GetType().GetMethod(method, Flags).Invoke(layer, null);
        private SpriteRenderer[] Copies() => root.GetComponentsInChildren<SpriteRenderer>(true).Where(r => r != source).ToArray();
        private SpriteRenderer[] ActiveCopies() => Copies().Where(r => r.gameObject.activeSelf).ToArray();

        private void AssertCoverage()
        {
            float width = source.sprite.rect.width / source.sprite.pixelsPerUnit * Mathf.Abs(root.transform.lossyScale.x);
            float centerOffset = (source.sprite.rect.width * 0.5f - source.sprite.pivot.x)
                / source.sprite.pixelsPerUnit * root.transform.lossyScale.x * (source.flipX ? -1f : 1f);
            var centers = ActiveCopies().Concat(new[] { source })
                .Select(r => r.transform.position.x + centerOffset).OrderBy(x => x).ToArray();
            float halfView = camera.orthographicSize * camera.aspect;
            Assert.That(centers.First() - width * 0.5f, Is.LessThanOrEqualTo(cameraObject.transform.position.x - halfView + 0.002f));
            Assert.That(centers.Last() + width * 0.5f, Is.GreaterThanOrEqualTo(cameraObject.transform.position.x + halfView - 0.002f));
            for (int i = 1; i < centers.Length; i++)
                Assert.That(centers[i] - centers[i - 1], Is.EqualTo(width).Within(0.002f));
            Assert.That(Mathf.Abs(source.transform.position.x + centerOffset - cameraObject.transform.position.x),
                Is.LessThanOrEqualTo(width * 0.5f + 0.002f));
        }

        [Test]
        public void RepeatOffPreservesFollowAndDepth()
        {
            Set("repeatX", false);
            root.transform.position = new Vector3(3f, 4f, 2f);
            Call("OnEnable");
            cameraObject.transform.position = new Vector3(10f, -5f, -20f);
            Call("LateUpdate");
            Assert.That(root.transform.position, Is.EqualTo(new Vector3(11f, 0f, 2f)));
            Assert.That(Copies(), Is.Empty);
        }

        [Test]
        public void RepetitionCoversPositiveNegativeAndTeleportPositionsWithoutDrift()
        {
            foreach (float x in new[] { 0f, 0.01f, -0.01f, 40f, 40.01f, -40f, -40.01f, 160f, -160f, 10000f, -10000f, 0f })
            {
                cameraObject.transform.position = new Vector3(x, 7f, -10f);
                Call("LateUpdate");
                AssertCoverage();
                Assert.That(root.transform.position.y, Is.EqualTo(5.6f).Within(0.001f));
                Assert.That(root.transform.position.z, Is.EqualTo(0f));
                Assert.That(ActiveCopies().Length, Is.EqualTo(2));
            }
            Assert.That(root.transform.position.x, Is.EqualTo(0f).Within(0.001f));
        }

        [Test]
        public void TransparentBordersRemainInRepeatSpacing()
        {
            // A central eight-unit painted region has four-unit transparent
            // borders on each side. Repetition must keep the full 16-unit cell.
            var pixels = new Color[texture.width * texture.height];
            for (int y = 0; y < texture.height; y++)
                for (int x = 40; x < 120; x++)
                    pixels[y * texture.width + x] = Color.white;
            texture.SetPixels(pixels);
            texture.Apply();
            Assert.That(texture.GetPixel(0, 45).a, Is.EqualTo(0f));
            Assert.That(texture.GetPixel(159, 45).a, Is.EqualTo(0f));
            Assert.That(texture.GetPixel(80, 45).a, Is.EqualTo(1f));
            Assert.That(sprite.rect.width / sprite.pixelsPerUnit, Is.EqualTo(16f));
            Call("LateUpdate");
            foreach (SpriteRenderer copy in ActiveCopies())
                Assert.That(Mathf.Abs(copy.transform.localPosition.x), Is.EqualTo(16f).Within(0.001f));
            AssertCoverage();
        }

        [Test]
        public void ZoomOutAddsCopiesAndReusesThemOnLaterFrames()
        {
            Call("LateUpdate");
            SpriteRenderer[] originalCopies = Copies();
            camera.orthographicSize = 40f;
            Call("LateUpdate");
            Assert.That(ActiveCopies().Length, Is.EqualTo(10));
            AssertCoverage();
            foreach (SpriteRenderer copy in originalCopies) CollectionAssert.Contains(Copies(), copy);
            SpriteRenderer[] expandedCopies = Copies();
            for (int i = 0; i < 100; i++) Call("LateUpdate");
            CollectionAssert.AreEqual(expandedCopies, Copies());
            camera.orthographicSize = 5f;
            Call("LateUpdate");
            Assert.That(ActiveCopies().Length, Is.EqualTo(2));
            AssertCoverage();
        }

        [Test]
        public void ParentScaleNegativeScaleFlipAndCustomPivotKeepSpacing()
        {
            Object.DestroyImmediate(sprite);
            sprite = Sprite.Create(texture, new Rect(0, 0, 160, 90), new Vector2(0.2f, 0.7f), 10f, 0, SpriteMeshType.FullRect);
            source.sprite = sprite;
            source.flipX = true;
            parent = new GameObject("Parallax QA parent");
            parent.transform.localScale = new Vector3(2f, 1.5f, 1f);
            root.transform.SetParent(parent.transform, false);
            root.transform.localScale = new Vector3(-1.5f, 0.75f, 1f);
            Call("OnEnable");
            foreach (float x in new[] { -999f, -40f, 0f, 40f, 999f })
            {
                cameraObject.transform.position = new Vector3(x, 0f, -10f);
                Call("LateUpdate");
                AssertCoverage();
                Assert.That(Mathf.Abs(source.bounds.center.x - cameraObject.transform.position.x),
                    Is.LessThanOrEqualTo(source.bounds.size.x * 0.5f + 0.002f));
            }
        }

        [Test]
        public void CopiesPreserveRenderingButNeverCloneGameplayComponents()
        {
            root.AddComponent<BoxCollider2D>();
            source.color = new Color(0.3f, 0.5f, 0.7f, 0.8f);
            source.sortingOrder = -30;
            source.flipX = source.flipY = true;
            source.enabled = false;
            int property = Shader.PropertyToID("_ParallaxValidation");
            var block = new MaterialPropertyBlock();
            block.SetFloat(property, 0.73f);
            source.SetPropertyBlock(block);
            Call("LateUpdate");
            foreach (SpriteRenderer copy in ActiveCopies())
            {
                Assert.That(copy.sprite, Is.SameAs(source.sprite));
                Assert.That(copy.sharedMaterial, Is.SameAs(source.sharedMaterial));
                Assert.That(copy.color, Is.EqualTo(source.color));
                Assert.That(copy.sortingOrder, Is.EqualTo(-30));
                Assert.That(copy.flipX && copy.flipY, Is.True);
                Assert.That(copy.enabled, Is.False);
                Assert.That(copy.GetComponents<Component>().Length, Is.EqualTo(2));
                copy.GetPropertyBlock(block);
                Assert.That(block.GetFloat(property), Is.EqualTo(0.73f).Within(0.001f));
            }
            source.enabled = true;
            Call("LateUpdate");
            Assert.That(ActiveCopies().All(c => c.enabled), Is.True);
            Assert.That(root.GetComponentsInChildren<BoxCollider2D>(true).Length, Is.EqualTo(1));
        }

        [Test]
        public void TurningRepeatOffAndReEnablingDoesNotAccumulateCopies()
        {
            for (int i = 0; i < 8; i++)
            {
                Set("repeatX", true);
                Call("LateUpdate");
                Assert.That(Copies().Length, Is.EqualTo(2));
                Set("repeatX", false);
                Call("LateUpdate");
                Assert.That(Copies(), Is.Empty);
                Call("OnDisable");
                Call("OnEnable");
            }
        }

        [Test]
        public void MissingSpriteWarnsOnceAndKeepsNormalFollow()
        {
            source.sprite = null;
            LogAssert.Expect(LogType.Warning, "ParallaxLayer: Repeat X needs a SpriteRenderer with a sprite on the same object.");
            cameraObject.transform.position = new Vector3(10f, 0f, -10f);
            Call("LateUpdate");
            Call("LateUpdate");
            Assert.That(root.transform.position.x, Is.EqualTo(8f).Within(0.001f));
            Assert.That(Copies(), Is.Empty);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void InvalidRotationDoesNotSpawnMisalignedTiles()
        {
            root.transform.rotation = Quaternion.Euler(0f, 0f, 15f);
            LogAssert.Expect(LogType.Warning, "ParallaxLayer: Repeat X supports unrotated sprites with Draw Mode set to Simple.");
            Call("LateUpdate");
            Assert.That(Copies(), Is.Empty);
        }

        [Test]
        public void TinySpritesDoNotSpawnAnUnboundedNumberOfCopies()
        {
            root.transform.localScale = Vector3.one * 0.001f;
            LogAssert.Expect(LogType.Warning, "ParallaxLayer: The sprite is too small to cover this camera. Increase its scale or lower the zoom-out.");
            Call("LateUpdate");
            Assert.That(Copies(), Is.Empty);
        }

        [Test]
        public void CleanupPreservesUnrelatedChildren()
        {
            var decoration = new GameObject("Authored decoration");
            decoration.transform.SetParent(root.transform, false);
            Call("LateUpdate");
            Call("OnDisable");
            Assert.That(decoration != null, Is.True);
            Assert.That(root.transform.childCount, Is.EqualTo(1));
        }
    }
}
