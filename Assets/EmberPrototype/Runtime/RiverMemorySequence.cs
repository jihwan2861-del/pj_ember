using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;
using UnityEngine.VFX;
namespace EmberPrototype
{
    [DefaultExecutionOrder(1050), DisallowMultipleComponent]
    public sealed class RiverMemorySequence : MonoBehaviour
    {
        public enum MemoryPhase { Current, Reflection, Spreading, BeautifulPast, Returning, Afterglow, Completed }
        public enum ReflectionEnding { KeepPast, ReturnToPresent }
        [Serializable] private sealed class SceneryPair
        {
            public SpriteRenderer present;
            public SpriteRenderer past;
            public Transform pastSource;
            [NonSerialized] public Color presentColor, pastColor;
        }
        [Header("Scene")]
        [SerializeField] private WaterSurface2D water;
        [SerializeField] private FlamePlayerController player;
        [SerializeField] private CelesteRoomCamera roomCamera;
        [SerializeField] private Transform cameraFocus;
        [SerializeField] private PlayerEventTrigger startTrigger;
        [SerializeField] private SceneryPair[] scenery = new SceneryPair[0];
        [Header("Timing — Game Seconds")]
        [SerializeField, Min(0f)] private float reflectionRevealSeconds = 2.5f;
        [SerializeField, Min(0f)] private float spreadIntoRealitySeconds = 1.5f;
        [SerializeField, Min(0f)] private float beautifulPastHoldSeconds = 3f;
        [SerializeField, Min(0f)] private float returnToRealitySeconds = 2f;
        [SerializeField, Min(0f)] private float afterglowSeconds = 1f;
        [SerializeField] private ReflectionEnding reflectionEnding = ReflectionEnding.KeepPast;
        [Header("Camera / Controls")]
        [SerializeField] private bool lockPlayerDuringSequence = true;
        [SerializeField, Range(0.3f, 1f)] private float cameraZoom = 1f;
        [SerializeField, Min(0.1f)] private float observationOrthographicSize = 8.5f;
        [SerializeField, Min(0f)] private float cameraBlendSeconds = 0.65f;
        [Header("Lighting")]
        [SerializeField] private Light2D memoryLight;
        [SerializeField, Min(0f)] private float maximumLightIntensity = 0.65f;
        [SerializeField] private Color lightColor = new Color(1f, 0.87f, 0.55f);
        [SerializeField, Range(0f, 0.3f)] private float warmTintStrength = 0.08f;
        [Header("Particle System / VFX Graph")]
        [SerializeField] private ParticleSystem pollen;
        [SerializeField, Min(0f)] private float pollenPerSecond = 26f;
        [SerializeField] private VisualEffect memoryRipples;
        [SerializeField, Range(0.1f, 3f)] private float ripplePlaybackMultiplier = 1.3f;
        [Header("Events")]
        [SerializeField] private UnityEvent onStarted = new UnityEvent();
        [SerializeField] private UnityEvent onPastVisible = new UnityEvent();
        [SerializeField] private UnityEvent onCompleted = new UnityEvent();
        private bool initialized, ownsLock, ownsCamera, playingRipples, notifiedPast;
        private float phaseElapsed;
        public MemoryPhase CurrentPhase { get; private set; }
        public bool IsCompleted => CurrentPhase == MemoryPhase.Completed;
        public bool IsRunning => CurrentPhase != MemoryPhase.Current && !IsCompleted;
        public float ReflectionAmount { get; private set; }
        public float RealityAmount { get; private set; }
        private void Start() => Initialize();
        private void Initialize()
        {
            if(initialized)return;
            foreach(var p in scenery){if(p.present!=null)p.presentColor=p.present.color;if(p.past!=null){p.pastColor=p.past.color;p.past.enabled=true;}}
            initialized=true;ApplyVisuals(0,0);if(memoryRipples!=null)memoryRipples.Stop();
        }
        public void BeginMemory()
        {
            if(!isActiveAndEnabled||CurrentPhase!=MemoryPhase.Current||player==null||player.IsDying)return;
            Initialize();if(lockPlayerDuringSequence&&player.ControlsLocked)return;
            if(lockPlayerDuringSequence){player.SetControlsLocked(true);ownsLock=true;}
            if(roomCamera!=null&&cameraFocus!=null&&!roomCamera.IsCinematicActive){roomCamera.SetNormalOrthographicSize(observationOrthographicSize,false);roomCamera.BeginCinematic(cameraFocus,cameraZoom,cameraBlendSeconds);ownsCamera=true;}
            CurrentPhase=MemoryPhase.Reflection;phaseElapsed=0;onStarted.Invoke();
        }
        private void Update()
        {
            if(!IsRunning)return;
            if(player==null||player.IsDying){ResetMemory();return;}
            if(GameSessionControls.IsPaused||SceneRestartWipe.IsPlaying||SceneFadeTransition.IsPlaying)return;
            phaseElapsed+=Time.deltaTime;
            // Carry time overshoot through phase boundaries, including zero-duration phases.
            for(int i=0;i<6&&IsRunning;i++)
            {
                float duration=Duration();float t=duration>0?Mathf.Clamp01(phaseElapsed/duration):1;float e=Mathf.SmoothStep(0,1,t);
                switch(CurrentPhase)
                {
                    case MemoryPhase.Reflection:ApplyVisuals(e,0);break;
                    case MemoryPhase.Spreading:ApplyVisuals(1,e);break;
                    case MemoryPhase.BeautifulPast:ApplyVisuals(1,1);break;
                    case MemoryPhase.Returning:ApplyVisuals(reflectionEnding==ReflectionEnding.KeepPast?1:1-e,1-e);break;
                    case MemoryPhase.Afterglow:ApplyVisuals(reflectionEnding==ReflectionEnding.KeepPast?1:0,0);break;
                }
                if(t<1)break;
                phaseElapsed=Mathf.Max(0,phaseElapsed-duration);
                if(CurrentPhase==MemoryPhase.Afterglow){CurrentPhase=MemoryPhase.Completed;ReleaseOwnership();onCompleted.Invoke();break;}
                CurrentPhase++;
                if(CurrentPhase==MemoryPhase.BeautifulPast&&!notifiedPast){notifiedPast=true;onPastVisible.Invoke();}
            }
        }
        private float Duration()
        {
            switch(CurrentPhase){case MemoryPhase.Reflection:return Mathf.Max(0,reflectionRevealSeconds);case MemoryPhase.Spreading:return Mathf.Max(0,spreadIntoRealitySeconds);case MemoryPhase.BeautifulPast:return Mathf.Max(0,beautifulPastHoldSeconds);case MemoryPhase.Returning:return Mathf.Max(0,returnToRealitySeconds);default:return Mathf.Max(0,afterglowSeconds);}
        }
        private void LateUpdate()
        {
            if(!initialized)return;
            foreach(var p in scenery)if(p.past!=null&&p.pastSource!=null){p.past.transform.SetPositionAndRotation(p.pastSource.position,p.pastSource.rotation);p.past.transform.localScale=p.pastSource.lossyScale;}
        }
        private void ApplyVisuals(float reflection,float reality)
        {
            ReflectionAmount=reflection;RealityAmount=reality;if(water!=null)water.SetMemoryReflection(reflection);
            foreach(var p in scenery)
            {
                if(p.present!=null){Color c=p.presentColor;c.a*=1-reality;p.present.color=c;}
                if(p.past!=null){Color c=Color.Lerp(p.pastColor,lightColor,reality*warmTintStrength);c.a=p.pastColor.a*reality;p.past.color=c;}
            }
            if(memoryLight!=null){memoryLight.color=lightColor;memoryLight.intensity=maximumLightIntensity*reality;}
            if(pollen!=null){var emission=pollen.emission;emission.rateOverTime=pollenPerSecond*reality;if(reality>0&&!pollen.isPlaying)pollen.Play();else if(reality<=0&&pollen.isPlaying)pollen.Stop(true,ParticleSystemStopBehavior.StopEmitting);}
            if(memoryRipples!=null)
            {
                memoryRipples.playRate=Mathf.Lerp(.5f,ripplePlaybackMultiplier,Mathf.Max(reflection,reality));
                bool show=IsRunning&&(reflection>.02f||reality>0);
                if(show&&!playingRipples)memoryRipples.Play();else if(!show&&playingRipples)memoryRipples.Stop();playingRipples=show;
            }
        }
        private void ReleaseOwnership()
        {
            if(ownsCamera){ownsCamera=false;if(roomCamera!=null)roomCamera.EndCinematic(cameraBlendSeconds);}
            if(ownsLock){ownsLock=false;if(player!=null&&!player.IsDying)player.SetControlsLocked(false);}
            if(memoryRipples!=null)memoryRipples.Stop();playingRipples=false;
            if(pollen!=null)pollen.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        public void ResetMemory()
        {
            if(!initialized)return;ReleaseOwnership();CurrentPhase=MemoryPhase.Current;phaseElapsed=0;notifiedPast=false;ApplyVisuals(0,0);
            if(startTrigger!=null)startTrigger.ResetTrigger();
            if(pollen!=null)pollen.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        private void OnDisable()=>ResetMemory();
    }
}
