using UnityEngine;
using UnityEngine.Rendering;

namespace EmberPrototype
{
    [DefaultExecutionOrder(100)]
    [DisallowMultipleComponent, RequireComponent(typeof(Camera))]
    public sealed class WaterAmbientParticles : MonoBehaviour
    {
        [SerializeField] private Color particleColor = new Color(0.2f, 0.65f, 1f, 0.65f);
        [SerializeField, Min(0f)] private float particlesPerSecond = 8f;
        [SerializeField] private int sortingOrder = 2;
        private Camera view;
        private ParticleSystem particles;
        private Material material;
        private Texture2D texture;
        private Vector3 previousCenter;

        private void Awake()
        {
            view = GetComponent<Camera>();
            GameObject emitter = new GameObject("Blue Ambient Particles");
            emitter.transform.SetParent(transform, false);
            particles = emitter.AddComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = particles.main;
            main.loop = true; main.prewarm = true; main.duration = 6f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = new ParticleSystem.MinMaxCurve(3f, 6f);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.025f, 0.07f);
            main.startColor = particleColor; main.maxParticles = 64;
            var emission = particles.emission; emission.rateOverTime = particlesPerSecond;
            var shape = particles.shape; shape.shapeType = ParticleSystemShapeType.Box;
            var velocity = particles.velocityOverLifetime;
            velocity.enabled = true; velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = new ParticleSystem.MinMaxCurve(-0.08f, 0.08f);
            velocity.y = new ParticleSystem.MinMaxCurve(0.04f, 0.16f);
            velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);
            var fade = particles.colorOverLifetime; fade.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.25f), new GradientAlphaKey(0f, 1f) });
            fade.color = gradient;
            texture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            texture.name = "Ambient Particle Soft Dot"; texture.wrapMode = TextureWrapMode.Clamp;
            for (int y = 0; y < 32; y++) for (int x = 0; x < 32; x++)
            {
                float radius = new Vector2((x + 0.5f) / 16f - 1f, (y + 0.5f) / 16f - 1f).magnitude;
                float alpha = Mathf.Pow(Mathf.Clamp01(1f - radius), 2f);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
            texture.Apply();
            material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            material.name = "Blue Ambient Particle Material";
            material.SetTexture("_BaseMap", texture);
            material.SetColor("_BaseColor", Color.white);
            material.SetFloat("_Surface", 1f); material.SetFloat("_ZWrite", 0f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); material.renderQueue = 3000;
            ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material; renderer.sortingOrder = sortingOrder;
            FollowCamera();
        }

        private void OnEnable() { if (particles != null) { FollowCamera(); particles.Play(); } }
        private void OnDisable() { if (particles != null) particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); }
        private void LateUpdate() => FollowCamera();
        private void FollowCamera()
        {
            if (particles == null || view == null || !view.orthographic) return;
            Vector3 center = new Vector3(view.transform.position.x, view.transform.position.y, 0f);
            float height = view.orthographicSize * 2f, width = height * view.aspect;
            // Existing particles stay in the world; a room jump discards the old room's particles.
            if (Vector3.Distance(center, previousCenter) > Mathf.Max(width, height)) particles.Clear();
            particles.transform.position = center;
            var shape = particles.shape; shape.scale = new Vector3(width, height, 0.01f);
            var main = particles.main; main.startColor = particleColor;
            var emission = particles.emission; emission.rateOverTime = particlesPerSecond;
            particles.GetComponent<ParticleSystemRenderer>().sortingOrder = sortingOrder;
            previousCenter = center;
        }

        private void OnDestroy()
        {
            if (particles != null) Destroy(particles.gameObject);
            if (material != null) Destroy(material);
            if (texture != null) Destroy(texture);
        }
    }
}
