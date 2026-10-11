using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TestTools;

namespace WaterContactValidation
{
    public sealed class WaterContactTests
    {
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static Type T(string name) => Type.GetType("EmberPrototype." + name + ", Assembly-CSharp", true);
        private static object P(object value, string name) => value.GetType().GetProperty(name, Flags).GetValue(value);
        private static void Set(object value, string name, object data) => value.GetType().GetField(name, Flags).SetValue(value, data);
        private static object Field(object value, string name) => value.GetType().GetField(name, Flags).GetValue(value);
        private static object Call(object value, string name, params object[] args) => value.GetType().GetMethods(Flags)
            .First(m => m.Name == name && m.GetParameters().Length == args.Length && m.GetParameters().Select((p,i) => args[i] == null || p.ParameterType.IsInstanceOfType(args[i])).All(b => b)).Invoke(value, args);
        private static Component Fire(Vector2 position, bool ignite = true)
        {
            var root = new GameObject("QA fire"); root.transform.position = position;
            root.AddComponent<BoxCollider2D>().size = Vector2.one * 0.4f;
            var art = root.AddComponent<SpriteRenderer>();
            art.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0,0,1,1), Vector2.one * 0.5f, 2f);
            var fire = root.AddComponent(T("FlammableTile"));
            Set(fire, "applyColorTint", true);
            Set(fire, "spreadDelay", 0.15f);
            if (ignite) Call(fire, "TryIgnite");
            return fire;
        }
        private static Component Water(Vector2 position, Vector2 size)
        {
            var root = new GameObject("QA water"); root.transform.position = position;
            root.AddComponent<BoxCollider2D>().size = size;
            root.GetComponent<BoxCollider2D>().isTrigger = true;
            return root.AddComponent(T("WaterContact"));
        }
        private static IEnumerator Start()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            yield return new EnterPlayMode();
        }
        private static IEnumerator Step(float seconds = 0.06f)
        {
            float end = Time.time + seconds;
            float guard = Time.realtimeSinceStartup + 8f;
            while (Time.time < end && Time.realtimeSinceStartup < guard) yield return null;
            Assert.That(Time.time, Is.GreaterThanOrEqualTo(end));
        }
        private static Component Wave()
        {
            var root = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/EmberPrototype/Prefabs/WaterWave.prefab"));
            var wave = root.GetComponent(T("WaterWave"));
            Call(wave, "Stop"); Set(wave, "deadly", false); Set(wave, "repeat", false);
            return wave;
        }
        private static void Capture(string name)
        {
            string folder = Environment.GetEnvironmentVariable("EMBER_WATER_CONTACT_ARTIFACTS");
            if (string.IsNullOrEmpty(folder)) return;
            var camera = new GameObject("QA capture").AddComponent<Camera>();
            camera.transform.position = new Vector3(0,0,-10); camera.orthographic = true; camera.orthographicSize = 2;
            var target = new RenderTexture(640,360,24); var prior = RenderTexture.active;
            camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
            var bitmap = new Texture2D(640,360,TextureFormat.RGB24,false);
            bitmap.ReadPixels(new Rect(0,0,640,360),0,0); bitmap.Apply();
            Directory.CreateDirectory(folder); File.WriteAllBytes(Path.Combine(folder,name + ".png"),bitmap.EncodeToPNG());
            camera.targetTexture = null; RenderTexture.active = prior;
            UnityEngine.Object.Destroy(bitmap); target.Release(); UnityEngine.Object.Destroy(target); UnityEngine.Object.Destroy(camera.gameObject);
        }
        [UnityTearDown] public IEnumerator Cleanup() { if (Application.isPlaying) yield return new ExitPlayMode(); }

        [UnityTest]
        public IEnumerator ExtinguishPreservesInitialFireStopsEffectsAndPendingSpread()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single); yield return new EnterPlayMode();
            Component fire = Fire(Vector2.zero, false), neighbor = Fire(Vector2.right, false);
            int resets = 0;
            ((UnityEvent)Field(fire,"onFireReset")).AddListener(() => resets++);
            var effect = new GameObject("Authored fire effect"); effect.transform.SetParent(fire.transform);
            effect.SetActive(false); Set(fire,"fireEffect",effect);
            Call(fire,"ConfigureStartBurning",true); Call(fire,"ResetFire");
            Assert.That(effect.activeSelf, Is.True); Assert.That(resets,Is.EqualTo(1));
            Capture("01-burning");
            Assert.That(Call(fire,"Extinguish"), Is.True); Assert.That(Call(fire,"Extinguish"), Is.False);
            Assert.That(effect.activeSelf, Is.False); Assert.That(resets,Is.EqualTo(1));
            Capture("02-extinguished");
            yield return Step(0.4f);
            Assert.That(P(neighbor,"IsBurning"), Is.False); Assert.That(P(fire,"IsBurning"), Is.False);
            Call(fire,"ResetFire"); Assert.That(P(fire,"IsBurning"), Is.True);
        }

        [UnityTest]
        public IEnumerator OverlappingWaterAndDuplicateCollidersRemainWetUntilLastExit()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single); yield return new EnterPlayMode();
            Component fire = Fire(Vector2.zero);
            var child = new GameObject("second fire collider"); child.transform.SetParent(fire.transform,false);
            child.AddComponent<BoxCollider2D>().size = Vector2.one * 0.2f;
            Component first = Water(Vector2.zero,Vector2.one*2), second = Water(Vector2.zero,Vector2.one*2);
            Physics2D.SyncTransforms(); yield return Step();
            Assert.That(P(fire,"IsBurning"), Is.False); Assert.That(Call(fire,"TryIgnite"), Is.False);
            first.gameObject.SetActive(false);
            child.GetComponent<BoxCollider2D>().enabled = false;
            Assert.That(Call(fire,"TryIgnite"), Is.False);
            second.gameObject.SetActive(false);
            Assert.That(P(fire,"IsBurning"), Is.False); Assert.That(Call(fire,"TryIgnite"), Is.True);
            second.gameObject.SetActive(true);
            fire.gameObject.SetActive(false); fire.gameObject.SetActive(true);
            Assert.That(P(fire,"IsWet"), Is.True); Assert.That(Call(fire,"TryIgnite"), Is.False);
            UnityEngine.Object.Destroy(second.gameObject); yield return null;
            Assert.That(Call(fire,"TryIgnite"), Is.True);
        }

        [UnityTest]
        public IEnumerator WaterExitAndLargeOverlapQueryDoNotLoseContacts()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single); yield return new EnterPlayMode();
            Component[] fires = Enumerable.Range(0,24).Select(i => Fire(new Vector2(i*0.6f-7f,0),false)).ToArray();
            Component water = Water(Vector2.zero,new Vector2(18,2));
            Physics2D.SyncTransforms(); yield return Step();
            foreach (Component fire in fires) { Assert.That(P(fire,"IsWet"),Is.True); Assert.That(Call(fire,"TryIgnite"),Is.False); }
            water.transform.position = Vector2.up*10; Physics2D.SyncTransforms(); yield return Step();
            foreach (Component fire in fires) { Assert.That(P(fire,"IsWet"),Is.False); Assert.That(Call(fire,"TryIgnite"),Is.True); }
        }

        [UnityTest]
        public IEnumerator WetTargetsRejectProjectileAndSpreadIgnition()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single); yield return new EnterPlayMode();
            Component wet = Fire(Vector2.zero,false); Water(Vector2.zero,Vector2.one);
            Fire(Vector2.right*1.1f);
            var shot = new GameObject("QA ember"); shot.transform.position = Vector2.left*2;
            Component projectile = shot.AddComponent(T("EmberProjectile"));
            Call(projectile,"Launch",Vector2.right*20,5f,null);
            yield return Step(0.5f);
            Assert.That(P(wet,"IsWet"),Is.True); Assert.That(P(wet,"IsBurning"),Is.False);
            Assert.That(Call(wet,"TryIgnite"),Is.False); Assert.That(shot == null,Is.True);
        }

        [UnityTest]
        public IEnumerator WaveStopsInsideFireAndReusesWithoutStaleWetState()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single); yield return new EnterPlayMode(); Component fire = Fire(Vector2.zero); Component wave = Wave();
            Set(wave,"speed",0.1f); Set(wave,"travelDistance",0.1f);
            Call(wave,"PlayAt",Vector2.down*0.3f);
            Assert.That(P(fire,"IsBurning"),Is.False); Assert.That(Call(fire,"TryIgnite"),Is.False);
            Call(wave,"Stop"); Assert.That(Call(fire,"TryIgnite"),Is.True);
            Call(wave,"PlayAt",Vector2.down*0.3f); wave.gameObject.SetActive(false);
            Assert.That(Call(fire,"TryIgnite"),Is.True);
            wave.gameObject.SetActive(true); Call(wave,"PlayAt",Vector2.down*0.3f);
            UnityEngine.Object.Destroy(wave.gameObject); yield return null;
            Assert.That(Call(fire,"TryIgnite"),Is.True);
        }

        [UnityTest]
        public IEnumerator FastNativeWaveExtinguishesAcrossOnePhysicsStep()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single); yield return new EnterPlayMode(); Component fire = Fire(Vector2.zero); Component wave = Wave();
            Set(wave,"speed",1000f); Set(wave,"travelDistance",6f);
            Call(wave,"PlayAt",new Vector2(-3,-0.3f));
            yield return new WaitForFixedUpdate(); yield return null;
            Assert.That(P(fire,"IsBurning"),Is.False);
            yield return Step(); Assert.That(P(wave,"IsMoving"),Is.False);
            Assert.That(Call(fire,"TryIgnite"),Is.True);
        }

        [UnityTest]
        public IEnumerator ResetRestoresInitialFireAndReappliesPermanentWater()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single); yield return new EnterPlayMode(); Component fire = Fire(Vector2.zero);
            Call(fire,"ConfigureStartBurning",true); Call(fire,"Extinguish");
            var reset = T("MapReset").GetMethod("ResetScene");
            reset.Invoke(null,new object[]{fire.gameObject.scene,true}); Assert.That(P(fire,"IsBurning"),Is.True);
            Water(Vector2.zero,Vector2.one*2);
            reset.Invoke(null,new object[]{fire.gameObject.scene,true});
            Assert.That(P(fire,"IsBurning"),Is.False); Assert.That(Call(fire,"TryIgnite"),Is.False);
            reset.Invoke(null,new object[]{fire.gameObject.scene,true}); Assert.That(P(fire,"IsWet"),Is.True);
        }

        [UnityTest]
        public IEnumerator ExtinguishedTargetsReleasePlayerAndNotifyDoor()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single); yield return new EnterPlayMode(); Component fire = Fire(new Vector2(20,20));
            var playerRoot = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/prefab/player.prefab"));
            Component player = playerRoot.GetComponent(T("FlamePlayerController"));
            ((Behaviour)player).enabled = false;
            Set(player,"targetFire",fire); Call(player,"EnterFire");
            Call(fire,"Extinguish"); Call(player,"FixedUpdate");
            Assert.That(Field(player,"state").ToString(),Is.EqualTo("Free"));
            Assert.That(((Collider2D)Field(player,"bodyCollider")).enabled,Is.True);
            Call(fire,"TryIgnite"); Set(player,"targetFire",fire);
            Set(player,"state",Enum.Parse(Field(player,"state").GetType(),"Travelling"));
            Call(fire,"Extinguish"); Call(player,"FixedUpdate");
            Assert.That(Field(player,"state").ToString(),Is.EqualTo("Free"));
            var ropeRoot = new GameObject("QA rope"); Component rope = ropeRoot.AddComponent(T("BurningRope"));
            fire.transform.SetParent(ropeRoot.transform); Call(rope,"Configure",fire,3f,0f,0f);
            Call(fire,"TryIgnite"); Set(player,"activeRope",rope); Set(player,"targetFire",fire);
            Set(player,"state",Enum.Parse(Field(player,"state").GetType(),"RopeAnchored"));
            ((Collider2D)Field(player,"bodyCollider")).enabled = false;
            Call(fire,"Extinguish"); Call(player,"FixedUpdate");
            Assert.That(Field(player,"state").ToString(),Is.EqualTo("Free"));
            Assert.That(((Collider2D)Field(player,"bodyCollider")).enabled,Is.True);
            var doorRoot = new GameObject("QA door"); Component door = doorRoot.AddComponent(T("AllTorchesDoor"));
            ((Behaviour)door).enabled = false;
            Array torches = Array.CreateInstance(T("FlammableTile"),1); torches.SetValue(fire,0); Set(door,"requiredTorches",torches);
            ((Behaviour)door).enabled = true; Call(fire,"TryIgnite");
            Assert.That(doorRoot.GetComponent<BoxCollider2D>().enabled,Is.False);
            Call(fire,"Extinguish"); Assert.That(doorRoot.GetComponent<BoxCollider2D>().enabled,Is.True);
        }
    }
}

