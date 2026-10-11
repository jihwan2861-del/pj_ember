using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace WaveAuthoringValidation
{
    public sealed class WaterWaveAuthoringTests
    {
        private GameObject root;
        private Component wave;
        private PolygonCollider2D collider;
        private SpriteRenderer visual;
        private Texture2D texture;
        private Sprite sprite;
        private Type WaveType => Type.GetType("EmberPrototype.WaterWave, Assembly-CSharp", true);
        private readonly Vector2[] shape = { new Vector2(-0.4f, 0f), new Vector2(0.9f, 0f), new Vector2(0.2f, 1.5f) };

        [SetUp]
        public void Create()
        {
            root = new GameObject("Wave authoring QA");
            root.SetActive(false);
            collider = root.AddComponent<PolygonCollider2D>();
            collider.SetPath(0, shape);
            collider.offset = new Vector2(0.3f, 0.1f);
            wave = root.AddComponent(WaveType);
            GameObject art = new GameObject("Wave Visual");
            art.transform.SetParent(root.transform, false);
            visual = art.AddComponent<SpriteRenderer>();
            texture = new Texture2D(16, 16);
            sprite = Sprite.Create(texture, new Rect(0, 0, 16, 16), new Vector2(0.25f, 0.5f), 16f, 0, SpriteMeshType.FullRect);
            visual.sprite = sprite;
        }
        private void Set(string name, object value) => WaveType.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(wave, value);
        private void Call(string name) => WaveType.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Invoke(wave, null);
        private void Capture()
        {
            Set("waveCollider", collider);
            Call("CaptureAuthoredCollider");
        }

        [Test]
        public void InspectorRefreshPreservesHandEditedPathsAndOffset()
        {
            Set("width", 4f);
            Call("OnValidate");
            Call("RefreshShape");
            CollectionAssert.AreEqual(shape, collider.GetPath(0));
            Assert.That(collider.offset, Is.EqualTo(new Vector2(0.3f, 0.1f)));
        }

        [Test]
        public void DirectionRoundTripsMirrorSavedShapeWithoutCumulativeFlips()
        {
            Capture();
            Set("direction", Enum.ToObject(WaveType.GetNestedType("TravelDirection"), 1));
            Call("RefreshShape");
            Vector2[] mirrored = collider.GetPath(0);
            for (int i = 0; i < shape.Length; i++) Assert.That(mirrored[i], Is.EqualTo(new Vector2(-shape[i].x, shape[i].y)));
            Assert.That(collider.offset.x, Is.EqualTo(-0.3f).Within(0.001f));
            Call("RefreshShape");
            CollectionAssert.AreEqual(mirrored, collider.GetPath(0));
            Set("direction", Enum.ToObject(WaveType.GetNestedType("TravelDirection"), 0));
            Call("RefreshShape");
            CollectionAssert.AreEqual(shape, collider.GetPath(0));
        }

        [Test]
        public void BottomAlignmentAndVisualOffsetDoNotChangeCollider()
        {
            Set("visualOffset", new Vector2(0.4f, 0.2f));
            Set("visualSizeMultiplier", new Vector2(2f, 0.5f));
            Call("RefreshShape");
            Assert.That(visual.bounds.min.y, Is.EqualTo(0.2f).Within(0.001f));
            Assert.That(visual.bounds.center.x, Is.EqualTo(0.4f).Within(0.001f));
            Assert.That(visual.bounds.size.x, Is.EqualTo(2.4f).Within(0.001f));
            Assert.That(visual.bounds.size.y, Is.EqualTo(1.325f).Within(0.001f));
            CollectionAssert.AreEqual(shape, collider.GetPath(0));
            Set("direction", Enum.ToObject(WaveType.GetNestedType("TravelDirection"), 1));
            Call("RefreshShape");
            Assert.That(visual.bounds.center.x, Is.EqualTo(-0.4f).Within(0.001f));
        }

        [Test]
        public void SingleVisualIsAutomaticallyConnectedAndHiddenOnStop()
        {
            Call("RefreshShape");
            Call("Stop");
            Assert.That(visual.enabled, Is.False);
        }

        [TearDown]
        public void Cleanup()
        {
            UnityEngine.Object.DestroyImmediate(root);
            UnityEngine.Object.DestroyImmediate(sprite);
            UnityEngine.Object.DestroyImmediate(texture);
        }
    }
}
