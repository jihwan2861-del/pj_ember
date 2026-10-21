using UnityEditor;
using UnityEngine;

namespace EmberPrototype.Editor
{
    public static class WeightPlatformBuilder
    {
        public const string PrefabPath = "Assets/EmberPrototype/Prefabs/WeightPlatform.prefab";
        [MenuItem("Ember Prototype/Create Weight Platform Prefab")]
        public static void Build()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null) return;
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/EmberPrototype/Prefabs/NonFlammableTile.prefab");
            GameObject root = Object.Instantiate(source);
            root.name = "WeightPlatform";
            root.transform.position = Vector3.zero;
            root.transform.localScale = new Vector3(3f,0.4f,1f);
            BoxCollider2D surface = root.GetComponent<BoxCollider2D>();
            surface.size = Vector2.one;
            surface.offset = Vector2.zero;
            surface.isTrigger = false;
            Rigidbody2D body = root.GetComponent<Rigidbody2D>();
            if (body == null) body = root.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic; body.gravityScale = 0f; body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            root.AddComponent<WeightPlatform>();
            PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
            Object.DestroyImmediate(root);
        }
    }
}
