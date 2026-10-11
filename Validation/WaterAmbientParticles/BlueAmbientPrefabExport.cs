using System.Reflection;
using UnityEditor;
using UnityEngine;
using EmberPrototype;
public static class BlueAmbientPrefabExport
{
    public static void Run()
    {
        const string folder = "Assets/EmberPrototype/Materials";
        if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/EmberPrototype", "Materials");
        GameObject root = new GameObject("Ambient Export Camera");
        Camera camera = root.AddComponent<Camera>(); camera.orthographic = true; camera.orthographicSize = 5f; camera.aspect = 16f / 9f;
        WaterAmbientParticles effect = root.AddComponent<WaterAmbientParticles>();
        if (root.GetComponentInChildren<ParticleSystem>() == null)
            typeof(WaterAmbientParticles).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(effect, null);
        ParticleSystem particles = root.GetComponentInChildren<ParticleSystem>();
        var renderer = particles.GetComponent<ParticleSystemRenderer>();
        Texture2D texture = Object.Instantiate((Texture2D)renderer.sharedMaterial.GetTexture("_BaseMap"));
        texture.name = "Blue Ambient Soft Dot";
        AssetDatabase.CreateAsset(texture, folder + "/BlueAmbientSoftDot.asset");
        Material material = new Material(renderer.sharedMaterial); material.name = "BlueAmbientParticles";
        material.SetTexture("_BaseMap", texture);
        AssetDatabase.CreateAsset(material, folder + "/BlueAmbientParticles.mat");
        GameObject prefab = Object.Instantiate(particles.gameObject);
        prefab.name = "Blue Ambient Particles"; prefab.transform.SetParent(null); prefab.transform.position = Vector3.zero;
        prefab.GetComponent<ParticleSystemRenderer>().sharedMaterial = material;
        PrefabUtility.SaveAsPrefabAsset(prefab, "Assets/EmberPrototype/Prefabs/BlueAmbientParticles.prefab");
        AssetDatabase.SaveAssets();
        Debug.Log("Blue ambient editable Particle System prefab exported.");
        Object.DestroyImmediate(prefab); Object.DestroyImmediate(root);
    }
}
