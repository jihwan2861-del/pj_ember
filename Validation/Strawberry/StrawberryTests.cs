using System;
using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace StrawberryValidation
{
    public class StrawberryTests
    {
        static Type TypeFor(string name) => Type.GetType("EmberPrototype."+name+", Assembly-CSharp",true);
        static Component player;
        static Rigidbody2D body;
        static Component ledger => player.GetComponent(TypeFor("StrawberryCollection"));
        static int Count => ledger == null ? 0 : (int)ledger.GetType().GetProperty("CollectedCount").GetValue(ledger);
        static bool Flag(Component berry,string name) => (bool)berry.GetType().GetProperty(name).GetValue(berry);
        static void Build()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            Type.GetType("EmberPrototype.Editor.StrawberryPickupBuilder, Assembly-CSharp-Editor",true).GetMethod("Build").Invoke(null,null);
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),"Assets/StrawberryQA.unity");
        }
        static void SpawnPlayer()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/prefab/player.prefab");
            GameObject root = UnityEngine.Object.Instantiate(prefab,new Vector3(0,3,0),Quaternion.identity);
            player=root.GetComponent(TypeFor("FlamePlayerController")); body=root.GetComponent<Rigidbody2D>();
            body.constraints=RigidbodyConstraints2D.FreezeAll;
            Physics2D.SyncTransforms();
        }
        static Component Berry(Vector3 position)
        {
            GameObject prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/EmberPrototype/Prefabs/ChickenPickup.prefab");
            return UnityEngine.Object.Instantiate(prefab,position,Quaternion.identity).GetComponent(TypeFor("StrawberryPickup"));
        }
        static void Land()
        {
            var ground=new GameObject("QA safe ground"); ground.transform.position=new Vector3(0,-0.5f,0);
            ground.AddComponent<BoxCollider2D>().size=new Vector2(20,1);
            Collider2D collider=player.GetComponent<Collider2D>();
            float offset=collider.bounds.min.y-player.transform.position.y;
            body.constraints=RigidbodyConstraints2D.FreezeRotation; body.position=new Vector2(0,-offset+0.01f);
            body.linearVelocity=Vector2.zero; Physics2D.SyncTransforms();
        }
        static IEnumerator Wait(float seconds)
        {
            float end=Time.time+seconds, deadline=Time.realtimeSinceStartup+8;
            while(Time.time<end && Time.realtimeSinceStartup<deadline) yield return null;
            Assert.That(Time.time,Is.GreaterThanOrEqualTo(end));
        }
        [UnityTearDown] public IEnumerator Cleanup() { if(Application.isPlaying) yield return new ExitPlayMode(); }
        [UnityTest] public IEnumerator TriggerFollowsInAirThenCollectsOnceOnLanding()
        {
            Build(); yield return new EnterPlayMode(); SpawnPlayer();
            Component berry=Berry(player.transform.position);
            Vector3 origin=berry.transform.position;
            yield return Wait(0.4f);
            Assert.That(Flag(berry,"IsFollowing"),Is.True); Assert.That(Count,Is.Zero);
            Assert.That(Vector3.Distance(origin,berry.transform.position),Is.GreaterThan(0.1f));
            Land(); yield return Wait(0.8f);
            Assert.That(Flag(berry,"IsCollected"),Is.True); Assert.That(Count,Is.EqualTo(1));
            Assert.That(berry.GetComponentInChildren<SpriteRenderer>().enabled,Is.False);
            Assert.That(berry.GetComponent<Collider2D>().enabled,Is.False);
            yield return Wait(0.4f); Assert.That(Count,Is.EqualTo(1));
            TypeFor("MapReset").GetMethod("ResetScene").Invoke(null,new object[]{SceneManager.GetActiveScene(),false});
            Assert.That(Flag(berry,"IsCollected"),Is.True); Assert.That(Count,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator DeathReturnsUnconfirmedBerryToItsSpawn()
        {
            Build(); yield return new EnterPlayMode(); SpawnPlayer();
            Vector3 origin=player.transform.position; Component berry=Berry(origin);
            yield return Wait(0.3f); Assert.That(Flag(berry,"IsFollowing"),Is.True);
            player.GetType().GetMethod("KillAndRespawn").Invoke(player,null);
            yield return null; yield return null;
            Assert.That(Flag(berry,"IsFollowing"),Is.False); Assert.That(Flag(berry,"IsCollected"),Is.False);
            Assert.That(Vector3.Distance(berry.transform.position,origin),Is.LessThan(0.001f));
            Assert.That(berry.GetComponent<Collider2D>().enabled,Is.True); Assert.That(Count,Is.Zero);
        }
        [UnityTest] public IEnumerator MultipleBerriesCountIndependentlyAndResetPendingTogether()
        {
            Build(); yield return new EnterPlayMode(); SpawnPlayer();
            Component a=Berry(player.transform.position); Component b=Berry(player.transform.position);
            yield return Wait(0.4f);
            Assert.That((int)ledger.GetType().GetProperty("PendingCount").GetValue(ledger),Is.EqualTo(2));
            Assert.That(Vector3.Distance(a.transform.position,b.transform.position),Is.GreaterThan(0.1f));
            Land(); yield return Wait(0.8f); Assert.That(Count,Is.EqualTo(2));
            body.position=new Vector2(0,4); body.constraints=RigidbodyConstraints2D.FreezeAll; Physics2D.SyncTransforms();
            Component c=Berry(new Vector3(0,4,0)); yield return Wait(0.3f); Assert.That(Flag(c,"IsFollowing"),Is.True);
            player.GetType().GetMethod("ResetAt").Invoke(player,new object[]{new Vector2(5,4)});
            Assert.That(Flag(c,"IsFollowing"),Is.False); Assert.That(c.transform.position.y,Is.EqualTo(4f).Within(0.01f));
            Assert.That(Count,Is.EqualTo(2));
        }
        [UnityTest] public IEnumerator NonPlayerCannotPickupAndDisablingFollowerReturnsIt()
        {
            Build(); yield return new EnterPlayMode(); SpawnPlayer();
            Component berry=Berry(new Vector3(5,3,0));
            var box=new GameObject("QA non player"); box.transform.position=berry.transform.position;
            box.AddComponent<BoxCollider2D>(); box.AddComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;
            yield return Wait(0.1f); Assert.That(Flag(berry,"IsFollowing"),Is.False);
            berry.GetType().GetMethod("TryPickup").Invoke(berry,new object[]{player});
            yield return Wait(0.1f); berry.gameObject.SetActive(false);
            Assert.That(Flag(berry,"IsFollowing"),Is.False);
            Assert.That((int)ledger.GetType().GetProperty("PendingCount").GetValue(ledger),Is.Zero);
            Assert.That(berry.transform.position.x,Is.EqualTo(5f).Within(0.001f));
        }
    }
}
