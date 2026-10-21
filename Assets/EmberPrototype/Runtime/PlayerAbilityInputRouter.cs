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
            if (GameSessionControls.BlockGameplayInput) return;
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

            if (player.IsCircularOpening)
            {
                if (player.CanUseFreeAbilities && EmberPlayerInput.IgnitionBurstPressed)
                {
                    if (!player.TryInteractWithNearbyTrialAltar() && IsUnlocked(PlayerAbility.IgnitionRing))
                        player.RequestIgnitionRing();
                }
                return;
            }
            if (player.IsRidingFlower)
            {
                if (keyboard.zKey.wasPressedThisFrame) player.TryExitFlowerRide();
                return;
            }
            // An available sunflower carrier takes priority over the existing X actions.
            if (EmberPlayerInput.ActionPressed && !keyboard.zKey.wasPressedThisFrame
                && IsUnlocked(PlayerAbility.FireAbsorb) && player.RequestFlowerRide()) return;
            if (player.IsTravellingToFire)
            {
                if (keyboard.zKey.wasPressedThisFrame) player.TryDetachFireTravel();
                return;
            }
            if (player.IsInsideFire)
            {
                // Z always lifts the player: jump outside, launch/kick inside.
                if (keyboard.zKey.wasPressedThisFrame)
                    player.RequestLaunchFromFire();
                else if (EmberPlayerInput.ActionPressed && IsUnlocked(PlayerAbility.FireAbsorb))
                    player.RequestFireAbsorb();
                return;
            }

            if (player.IsInIgnitionRing)
            {
                if (keyboard.zKey.wasPressedThisFrame && IsUnlocked(PlayerAbility.BurstDash))
                {
                    player.CancelRingShotAim();
                    player.RequestBurstDash(EmberPlayerInput.MovementDirection());
                    return;
                }

                if (keyboard.zKey.wasPressedThisFrame) return;

                // X keeps its fire-entry action in the C ring; it no longer aims or fires a shot.
                if (EmberPlayerInput.ActionPressed && IsUnlocked(PlayerAbility.FireAbsorb))
                    player.RequestFireAbsorb();
                return;
            }

            if (!player.CanUseFreeAbilities) return;

            if (EmberPlayerInput.IgnitionBurstPressed && player.TryInteractWithNearbyTrialAltar())
            {
                return;
            }
            if (EmberPlayerInput.IgnitionBurstPressed && IsUnlocked(PlayerAbility.IgnitionRing))
            {
                player.RequestIgnitionRing();
                if (keyboard.zKey.wasPressedThisFrame && IsUnlocked(PlayerAbility.BurstDash))
                    player.RequestBurstDash(EmberPlayerInput.MovementDirection());
                return;
            }
            if (EmberPlayerInput.ActionPressed)
            {
                // X follows an aimed lit fire when one is available; otherwise it is the dash button.
                bool travelled = IsUnlocked(PlayerAbility.FireAbsorb) && player.RequestFireAbsorb();
                if (!travelled && IsUnlocked(PlayerAbility.BurstDash))
                    player.RequestBurstDash(EmberPlayerInput.MovementDirection());
                return;
            }
        }

        private bool IsUnlocked(PlayerAbility ability)
        {
            return abilityUnlocks != null && abilityUnlocks.IsUnlocked(ability);
        }

    }
}
