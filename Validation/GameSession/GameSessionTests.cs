using System;
using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace GameSessionValidation
{
    public class GameSessionTests
    {
        static Keyboard keyboard;
        private InputSettings.BackgroundBehavior originalBackground;
        private InputSettings.EditorInputBehaviorInPlayMode originalEditorInput;
        private InputSettings.UpdateMode originalUpdateMode;
        [SetUp] public void SetupInput()
        {
            originalBackground = InputSystem.settings.backgroundBehavior;
            originalEditorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
            originalUpdateMode = InputSystem.settings.updateMode;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;
        }
        static Type T(string name) => Type.GetType("EmberPrototype."+name+", Assembly-CSharp",true);
        static Component Player => GameObject.Find("QA session player").GetComponent(T("FlamePlayerController"));
        static Component Controls => Player.GetComponent(T("GameSessionControls"));
        static bool Paused => (bool)T("GameSessionControls").GetProperty("IsPaused").GetValue(null);
        static void Build()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            GameObject prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/prefab/player.prefab");
            var root=(GameObject)PrefabUtility.InstantiatePrefab(prefab); root.name="QA session player";
            root.transform.position=new Vector3(0,3,0); root.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),"Assets/GameSessionQA.unity");
        }
        static IEnumerator KeyPress(Key key)
        {
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(key)); yield return null; yield return null;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState()); yield return null;
        }
        static void Pause(bool value) => Controls.GetType().GetMethod("SetPaused").Invoke(Controls,new object[]{value});
        static bool Restarting => (bool)T("SceneRestartWipe").GetProperty("IsPlaying").GetValue(null);
        static Component Wipe => GameObject.Find("Restart Screen Wipe").GetComponentInChildren(T("DeathScreenWipe"));
        static float Coverage => (float)Wipe.GetType().GetProperty("Coverage").GetValue(Wipe);
        static IEnumerator WaitRestart()
        {
            float deadline = Time.realtimeSinceStartup + 6f;
            while (Restarting && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(Restarting, Is.False);
            yield return null;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if(keyboard!=null && keyboard.added) InputSystem.RemoveDevice(keyboard);
            keyboard=null;
            if(Application.isPlaying) yield return new ExitPlayMode();
            Time.timeScale=1f;
            InputSystem.settings.backgroundBehavior = originalBackground;
            InputSystem.settings.editorInputBehaviorInPlayMode = originalEditorInput;
            InputSystem.settings.updateMode = originalUpdateMode;
        }
        [UnityTest] public IEnumerator EscapeFreezesPhysicsAndResumes()
        {
            Build(); yield return new EnterPlayMode(); keyboard=InputSystem.AddDevice<Keyboard>();
            Assert.That(Controls,Is.Not.Null);
            var probe = new GameObject("QA falling physics probe"); probe.transform.position = new Vector3(10, 10, 0);
            probe.AddComponent<BoxCollider2D>(); var body = probe.AddComponent<Rigidbody2D>();
            yield return KeyPress(Key.Escape); Assert.That(Paused,Is.True); Assert.That(Time.timeScale,Is.Zero);
            Vector2 position=body.position; yield return new WaitForSecondsRealtime(0.2f);
            Assert.That(Vector2.Distance(position,body.position),Is.LessThan(0.001f));
            yield return KeyPress(Key.Z); Assert.That(body.position,Is.EqualTo(position));
            yield return KeyPress(Key.Escape); Assert.That(Paused,Is.False); Assert.That(Time.timeScale,Is.EqualTo(1f));
            float end = Time.time + 0.2f, deadline = Time.realtimeSinceStartup + 8f;
            while (Time.time < end && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(Time.time, Is.GreaterThanOrEqualTo(end));
            Assert.That(body.position.y,Is.LessThan(position.y),
                "mode=" + Physics2D.simulationMode + " fixedStep=" + Time.fixedDeltaTime + " velocity=" + body.linearVelocity);
        }
        [UnityTest] public IEnumerator RReloadsSceneFromInitialState()
        {
            Build(); yield return new EnterPlayMode(); keyboard=InputSystem.AddDevice<Keyboard>();
            Player.GetComponent<Rigidbody2D>().position=new Vector2(8,4); new GameObject("QA transient marker");
            yield return KeyPress(Key.R); yield return WaitRestart();
            Assert.That(GameObject.Find("QA transient marker"),Is.Null);
            Assert.That(Player.GetComponent<Rigidbody2D>().position,Is.EqualTo(new Vector2(0,3)));
            Assert.That(Paused,Is.False); Assert.That(Time.timeScale,Is.EqualTo(1f));
        }
        [UnityTest] public IEnumerator RWorksWhilePausedAndReloadDoesNotStayFrozen()
        {
            Build(); yield return new EnterPlayMode(); keyboard=InputSystem.AddDevice<Keyboard>();
            Pause(true); Assert.That(Paused,Is.True); new GameObject("QA transient marker");
            yield return KeyPress(Key.R); yield return WaitRestart();
            Assert.That(GameObject.Find("QA transient marker"),Is.Null);
            Assert.That(Paused,Is.False); Assert.That(Time.timeScale,Is.EqualTo(1f));
            Assert.That(Controls,Is.Not.Null);
        }
        [UnityTest] public IEnumerator PauseCancelsAimWithoutBreakingTimeScaleAndDisableRestoresTime()
        {
            Build(); yield return new EnterPlayMode(); keyboard=InputSystem.AddDevice<Keyboard>();
            float fixedStep=Time.fixedDeltaTime;
            Component shot=Player.GetComponent(T("FlameShotAbility")); shot.GetType().GetMethod("BeginAim").Invoke(shot,null);
            Assert.That(Time.timeScale,Is.LessThan(1f));
            Pause(true); Assert.That(Time.timeScale,Is.Zero);
            Assert.That((bool)shot.GetType().GetProperty("IsAiming").GetValue(shot),Is.False);
            Assert.That(Time.fixedDeltaTime,Is.EqualTo(fixedStep));
            yield return new WaitForSecondsRealtime(0.1f); Assert.That(Time.timeScale,Is.Zero);
            ((Behaviour)Controls).enabled=false; Assert.That(Paused,Is.False); Assert.That(Time.timeScale,Is.EqualTo(1f));
        }
        [UnityTest] public IEnumerator DeathSequenceDoesNotRespawnDuringPause()
        {
            Build(); yield return new EnterPlayMode(); keyboard=InputSystem.AddDevice<Keyboard>();
            var body=Player.GetComponent<Rigidbody2D>(); body.position=new Vector2(8,4);
            Player.GetType().GetMethod("KillAndRespawn").Invoke(Player,null); Pause(true);
            yield return new WaitForSecondsRealtime(0.9f);
            Assert.That((bool)Player.GetType().GetProperty("IsDying").GetValue(Player),Is.True);
            Assert.That(body.position.x,Is.EqualTo(8f));
            Pause(false); yield return new WaitForSecondsRealtime(1.2f);
            Assert.That((bool)Player.GetType().GetProperty("IsDying").GetValue(Player),Is.False);
            Assert.That(body.position.x,Is.EqualTo(0f).Within(0.01f));
        }
        [UnityTest] public IEnumerator RestartWipeCoversBeforeReloadAndSurvivesUntilReveal()
        {
            Build(); yield return new EnterPlayMode(); keyboard=InputSystem.AddDevice<Keyboard>();
            Component effect = Player.GetComponent(T("PlayerDeathEffect"));
            var settings = new SerializedObject(effect);
            settings.FindProperty("wipeOutDuration").floatValue = 0.4f;
            settings.FindProperty("wipeInDuration").floatValue = 0.4f;
            settings.FindProperty("blackHoldDuration").floatValue = 0.15f;
            settings.ApplyModifiedPropertiesWithoutUndo();
            new GameObject("QA transient marker");
            Controls.GetType().GetMethod("RestartScene").Invoke(Controls,null);
            Assert.That(Restarting,Is.True); Assert.That(Time.timeScale,Is.Zero);
            GameObject overlay = Wipe.transform.root.gameObject;
            Assert.That(Coverage,Is.LessThan(1f)); Assert.That(GameObject.Find("QA transient marker"),Is.Not.Null);
            Controls.GetType().GetMethod("RestartScene").Invoke(Controls,null);
            Pause(true); Assert.That(Paused,Is.False);
            Assert.That(Wipe.transform.root.gameObject,Is.SameAs(overlay));
            float deadline = Time.realtimeSinceStartup + 6f;
            bool sawFullCover = false;
            while(GameObject.Find("QA transient marker") != null && Time.realtimeSinceStartup < deadline)
            {
                sawFullCover |= Coverage >= 0.999f;
                yield return null;
            }
            Assert.That(GameObject.Find("QA transient marker"),Is.Null); Assert.That(sawFullCover,Is.True);
            Assert.That(Restarting,Is.True); Assert.That(Wipe.transform.root.gameObject,Is.SameAs(overlay));
            Assert.That(Coverage,Is.EqualTo(1f).Within(0.001f));
            yield return WaitRestart(); Assert.That(Time.timeScale,Is.EqualTo(1f));
            Assert.That(GameObject.Find("Restart Screen Wipe"),Is.Null);
        }
        [UnityTest] public IEnumerator RestartDuringDeathCancelsDeathAndCompletesWipe()
        {
            Build(); yield return new EnterPlayMode(); keyboard=InputSystem.AddDevice<Keyboard>();
            Player.GetType().GetMethod("KillAndRespawn").Invoke(Player,null);
            Controls.GetType().GetMethod("RestartScene").Invoke(Controls,null);
            Assert.That(Restarting,Is.True);
            yield return WaitRestart();
            Assert.That((bool)Player.GetType().GetProperty("IsDying").GetValue(Player),Is.False);
            Assert.That((bool)Player.GetType().GetProperty("ControlsLocked").GetValue(Player),Is.False);
            Assert.That(Time.timeScale,Is.EqualTo(1f));
        }
    }
}
