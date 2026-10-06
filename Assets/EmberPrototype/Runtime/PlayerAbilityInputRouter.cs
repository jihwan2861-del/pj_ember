using UnityEngine;
using UnityEngine.InputSystem;

namespace EmberPrototype
{
    [DefaultExecutionOrder(100)]
    [DisallowMultipleComponent]
    public sealed class PlayerAbilityInputRouter : MonoBehaviour
    {
        private FlamePlayerController player;
        private PlayerAbilityUnlocks abilityUnlocks;

        private void Awake()
        {
            player = GetComponent<FlamePlayerController>();
            abilityUnlocks = GetComponent<PlayerAbilityUnlocks>();
        }

        private void OnDisable()
        {
            player?.CancelRingShotAim();
        }

        private void Update()
        {
            if (player == null) player = GetComponent<FlamePlayerController>();
            if (abilityUnlocks == null) abilityUnlocks = GetComponent<PlayerAbilityUnlocks>();
            if (player == null) return;

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                player.CancelRingShotAim();
                return;
            }

            if (player.ControlsLocked)
            {
                player.CancelRingShotAim();
                return;
            }

            if (player.IsInsideFire)
            {
                // The controller selects torch launch or rope kick; X/Z bindings stay identical.
                if (keyboard.zKey.wasPressedThisFrame)
                    player.RequestLaunchFromFire();
                else if (keyboard.xKey.wasPressedThisFrame && IsUnlocked(PlayerAbility.FireAbsorb))
                    player.RequestFireAbsorb();
                return;
            }

            if (player.IsInIgnitionRing)
            {
                if (keyboard.zKey.wasPressedThisFrame && IsUnlocked(PlayerAbility.BurstDash))
                {
                    player.CancelRingShotAim();
                    player.RequestBurstDash(ReadHeldDirection());
                    return;
                }

                if (player.IsRingShotAiming)
                {
                    player.UpdateRingShotAim(ReadHeldDirection());
                    if (keyboard.xKey.wasReleasedThisFrame)
                        player.ReleaseRingShotAim();
                    return;
                }

                if (keyboard.zKey.wasPressedThisFrame) return;

                if (keyboard.xKey.wasPressedThisFrame && IsUnlocked(PlayerAbility.FlameShot))
                {
                    player.BeginRingShotAim();
                    player.UpdateRingShotAim(ReadHeldDirection());
                }
                return;
            }

            if (!player.CanUseFreeAbilities) return;

            if (keyboard.xKey.wasPressedThisFrame && IsUnlocked(PlayerAbility.FireAbsorb))
                player.RequestFireAbsorb();

            if (!keyboard.cKey.wasPressedThisFrame) return;
            if (player.TryInteractWithNearbyTrialAltar()) return;
            if (IsUnlocked(PlayerAbility.IgnitionRing))
            {
                player.RequestIgnitionRing();
                // Resolve the chord after entering the ring, so C+Z works in one frame too.
                if (keyboard.zKey.wasPressedThisFrame && IsUnlocked(PlayerAbility.BurstDash))
                    player.RequestBurstDash(ReadHeldDirection());
            }
        }

        private bool IsUnlocked(PlayerAbility ability)
        {
            return abilityUnlocks != null && abilityUnlocks.IsUnlocked(ability);
        }

        private static Vector2 ReadHeldDirection()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return Vector2.zero;

            Vector2 direction = Vector2.zero;
            if (keyboard.leftArrowKey.isPressed) direction.x -= 1f;
            if (keyboard.rightArrowKey.isPressed) direction.x += 1f;
            if (keyboard.upArrowKey.isPressed) direction.y += 1f;
            if (keyboard.downArrowKey.isPressed) direction.y -= 1f;
            return direction == Vector2.zero ? Vector2.zero : direction.normalized;
        }
    }
}
