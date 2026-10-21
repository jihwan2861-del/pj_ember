using UnityEditor;
using UnityEngine;

namespace EmberPrototype.Editor
{
    public static class WaterSpiritAISetup
    {
        [MenuItem("Ember Prototype/Add AI To Water Spirit Prefab")]
        public static void AttachToPrefab()
        {
            WaterSpiritPrefabBuilder.Build();
            string path = WaterSpiritPrefabBuilder.PrefabPath;
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                bool changed = false;
                SideChaseBehavior behavior = root.GetComponent<SideChaseBehavior>();
                if (behavior == null) { behavior = root.AddComponent<SideChaseBehavior>(); changed = true; }
                WaterSpiritWarning warning = root.GetComponentInChildren<WaterSpiritWarning>(true);
                if (warning == null)
                {
                    GameObject marker = new GameObject("Wave Warning");
                    marker.transform.SetParent(root.transform, false);
                    LineRenderer line = marker.AddComponent<LineRenderer>();
                    line.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat");
                    line.enabled = false;
                    warning = marker.AddComponent<WaterSpiritWarning>();
                    changed = true;
                }
                WaterSpiritAI ai = root.GetComponent<WaterSpiritAI>();
                if (ai == null) { ai = root.AddComponent<WaterSpiritAI>(); changed = true; }
                SerializedObject data = new SerializedObject(ai);
                if (data.FindProperty("initialBehavior").objectReferenceValue == null)
                { data.FindProperty("initialBehavior").objectReferenceValue = behavior; changed = true; }
                if (data.FindProperty("warning").objectReferenceValue == null)
                { data.FindProperty("warning").objectReferenceValue = warning; changed = true; }
                data.ApplyModifiedPropertiesWithoutUndo();
                if (changed) PrefabUtility.SaveAsPrefabAsset(root, path);
                AssetDatabase.SaveAssets();
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
    }
}
