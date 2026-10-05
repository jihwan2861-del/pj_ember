using EmberPrototype;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace EmberPrototype.Editor
{
    /// <summary>Adds the authored jump clip to the existing movement Blend Tree.</summary>
    public static class PlayerJumpAnimationSetup
    {
        private const string ControllerPath = "Assets/player/new_Player/animaition/New_Player_Animator.controller";
        private const string JumpClipPath = "Assets/player/new_Player/animaition/Jump.anim";
        private const string BaseStateName = "Blend Tree";
        private const string JumpStateName = "Jump";
        private const string AirborneParameterName = "Airborne";
        private const string SpeedParameterName = "Speed";
        private const float JumpThreshold = 2f;

        [InitializeOnLoadMethod]
        private static void QueueSetupOnLoad()
        {
            EditorApplication.delayCall += TrySetupWhenReady;
        }

        [MenuItem("Tools/Ember/Add Jump to Blend Tree")]
        public static void ConnectJumpAnimation()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
                return;

            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            AnimationClip jumpClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(JumpClipPath);
            if (controller == null || jumpClip == null)
            {
                Debug.LogError($"Jump animation setup needs both {ControllerPath} and {JumpClipPath}.");
                return;
            }

            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            AnimatorState baseState = FindState(stateMachine, BaseStateName);
            if (baseState == null) baseState = stateMachine.defaultState;
            if (baseState == null)
            {
                Debug.LogError("Jump animation setup stopped: the Animator Controller has no base/default state.", controller);
                return;
            }

            BlendTree movementTree = baseState.motion as BlendTree;
            if (movementTree == null)
            {
                Debug.LogError("Jump animation setup stopped: the existing Blend Tree motion could not be found.", baseState);
                return;
            }

            AnimatorState jumpState = FindState(stateMachine, JumpStateName);
            if (jumpState != null && jumpState.motion != null && jumpState.motion != jumpClip)
            {
                Debug.LogError("Jump animation setup stopped: the existing Jump state uses a different motion.", jumpState);
                return;
            }

            bool changed = false;
            if (movementTree.blendType != BlendTreeType.Simple1D)
            {
                movementTree.blendType = BlendTreeType.Simple1D;
                changed = true;
            }
            if (movementTree.blendParameter != SpeedParameterName)
            {
                movementTree.blendParameter = SpeedParameterName;
                changed = true;
            }
            if (movementTree.useAutomaticThresholds)
            {
                movementTree.useAutomaticThresholds = false;
                changed = true;
            }

            ChildMotion[] children = movementTree.children;
            int jumpChildIndex = -1;
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i].motion == jumpClip)
                {
                    jumpChildIndex = i;
                    break;
                }
            }

            if (jumpChildIndex < 0)
            {
                for (int i = 0; i < children.Length; i++)
                {
                    if (Mathf.Approximately(children[i].threshold, JumpThreshold))
                    {
                        Debug.LogError("Jump animation setup stopped: another motion already uses the jump threshold (2).", movementTree);
                        return;
                    }
                }

                movementTree.AddChild(jumpClip, JumpThreshold);
                children = movementTree.children;
                jumpChildIndex = children.Length - 1;
                changed = true;
            }

            if (!Mathf.Approximately(children[jumpChildIndex].threshold, JumpThreshold))
            {
                children[jumpChildIndex].threshold = JumpThreshold;
                movementTree.children = children;
                changed = true;
            }

            // Remove the temporary separate Jump state created by the previous setup.
            if (jumpState != null)
            {
                foreach (AnimatorStateTransition transition in baseState.transitions)
                {
                    if (transition.destinationState == jumpState)
                        baseState.RemoveTransition(transition);
                }

                stateMachine.RemoveState(jumpState);
                changed = true;

                int airborneParameterIndex = FindParameterIndex(controller, AirborneParameterName);
                if (airborneParameterIndex >= 0 &&
                    controller.parameters[airborneParameterIndex].type == AnimatorControllerParameterType.Bool)
                {
                    controller.RemoveParameter(airborneParameterIndex);
                    changed = true;
                }
            }

            if (changed)
            {
                EditorUtility.SetDirty(movementTree);
                EditorUtility.SetDirty(controller);
                AssetDatabase.SaveAssetIfDirty(controller);
                Debug.Log("Jump animation added to the existing Speed Blend Tree at threshold 2. Ground movement remains at its current thresholds.", controller);
            }
        }

        private static void TrySetupWhenReady()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            {
                EditorApplication.update -= TrySetupWhenReady;
                EditorApplication.update += TrySetupWhenReady;
                return;
            }

            EditorApplication.update -= TrySetupWhenReady;
            ConnectJumpAnimation();
        }

        private static int FindParameterIndex(AnimatorController controller, string parameterName)
        {
            AnimatorControllerParameter[] parameters = controller.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].name == parameterName) return i;
            }

            return -1;
        }

        private static AnimatorState FindState(AnimatorStateMachine stateMachine, string stateName)
        {
            foreach (ChildAnimatorState childState in stateMachine.states)
            {
                if (childState.state.name == stateName) return childState.state;
            }

            return null;
        }

    }
}
