using System;
using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;

namespace WeightPlatformValidation
{
    public class WeightPlatformTests
    {
        static Type T => Type.GetType("EmberPrototype.WeightPlatform, Assembly-CSharp",true);
        static Component Platform;
        static void Build()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            Type.GetType("EmberPrototype.Editor.WeightPlatformBuilder, Assembly-CSharp-Editor",true).GetMethod("Build").Invoke(null,null);
            var root = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/EmberPrototype/Prefabs/WeightPlatform.prefab"));
            root.name="QA weight platform";
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),"Assets/WeightPlatformQA.unity");
        }
        static void Find() => Platform = GameObject.Find("QA weight platform").GetComponent(T);
        static void Set(string name,float value) { var data=new SerializedObject(Platform); data.FindProperty(name).floatValue=value; data.ApplyModifiedPropertiesWithoutUndo(); }
        static bool Loaded => (bool)T.GetProperty("HasLoad").GetValue(Platform);
        static float Y => Platform.GetComponent<Rigidbody2D>().position.y;
        static GameObject Box(Vector2 point,bool dynamic=true)
        {
            var go=new GameObject("QA load"); go.transform.position=point; go.AddComponent<BoxCollider2D>().size=Vector2.one*0.4f;
            if(dynamic) go.AddComponent<Rigidbody2D>().gravityScale=1f;
            Physics2D.SyncTransforms(); return go;
        }
        static IEnumerator Wait(float seconds)
        {
            float end=Time.time+seconds, deadline=Time.realtimeSinceStartup+8;
            while(Time.time<end && Time.realtimeSinceStartup<deadline) yield return null;
            Assert.That(Time.time,Is.GreaterThanOrEqualTo(end));
        }
        [UnityTearDown] public IEnumerator Cleanup() { if(Application.isPlaying) yield return new ExitPlayMode(); }
        [UnityTest] public IEnumerator LoadDescendsToLimitAndReturnsAfterLastObjectLeaves()
        {
            Build(); yield return new EnterPlayMode(); Find(); Set("descendDistance",0.6f); Set("descendSpeed",0.5f); Set("returnSpeed",0.5f);
            float top=Platform.GetComponent<BoxCollider2D>().bounds.max.y;
            var a=Box(new Vector2(-0.4f,top+0.2f)); var b=Box(new Vector2(0.4f,top+0.2f));
            yield return Wait(0.5f); Assert.That(Loaded,Is.True); Assert.That(Y,Is.InRange(-0.4f,-0.1f));
            UnityEngine.Object.Destroy(a); yield return Wait(1.2f); Assert.That(Loaded,Is.True); Assert.That(Y,Is.EqualTo(-0.6f).Within(0.02f));
            UnityEngine.Object.Destroy(b); yield return Wait(1.5f); Assert.That(Loaded,Is.False); Assert.That(Y,Is.EqualTo(0f).Within(0.02f));
            var load=Box(new Vector2(0,top+0.2f)); yield return Wait(0.3f);
            T.GetMethod("ResetForRespawn").Invoke(Platform,null); Assert.That(Y,Is.EqualTo(0f).Within(0.01f));
        }
        [UnityTest] public IEnumerator SideUndersideAndTriggerDoNotPressPlatform()
        {
            Build(); yield return new EnterPlayMode(); Find();
            Bounds bounds=Platform.GetComponent<BoxCollider2D>().bounds;
            Box(new Vector2(bounds.max.x+0.3f,bounds.center.y),false);
            Box(new Vector2(0,bounds.min.y-0.2f),false);
            var trigger=Box(new Vector2(0,bounds.max.y+0.2f),false); trigger.GetComponent<BoxCollider2D>().isTrigger=true;
            Physics2D.SyncTransforms(); yield return Wait(0.4f); Assert.That(Loaded,Is.False); Assert.That(Y,Is.EqualTo(0f).Within(0.01f));
        }
        [UnityTest] public IEnumerator ActualPlayerRidesDescendingPlatformWithoutDroppingThrough()
        {
            Build(); yield return new EnterPlayMode(); Find();
            var player=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/prefab/player.prefab"));
            var collider=player.GetComponent<Collider2D>(); var body=player.GetComponent<Rigidbody2D>();
            Physics2D.SyncTransforms();
            float offset=collider.bounds.min.y-player.transform.position.y;
            body.position=new Vector2(0,Platform.GetComponent<BoxCollider2D>().bounds.max.y-offset+0.01f);
            Physics2D.SyncTransforms(); yield return Wait(0.8f);
            Assert.That(Loaded,Is.True); Assert.That(Y,Is.LessThan(-0.2f));
            float gap=collider.bounds.min.y-Platform.GetComponent<BoxCollider2D>().bounds.max.y;
            Assert.That(gap,Is.InRange(-0.06f,0.12f));
        }
    }
}
