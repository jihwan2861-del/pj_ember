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
        private FlameShotAbility flameShot;

        private void Awake()
        {
            player = GetComponent<FlamePlayerController>();
            abilityUnlocks = GetComponent<PlayerAbilityUnlocks>();
            flameShot = GetComponent<FlameShotAbility>();
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
            if (flameShot == null) flameShot = GetComponent<FlameShotAbility>();
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

                if (player.IsRingShotAiming)
                {
                    player.UpdateRingShotAim(ReadHeldDirection());
                    if (EmberPlayerInput.ActionReleased)
                        player.ReleaseRingShotAim();
                    return;
                }

                if (keyboard.zKey.wasPressedThisFrame) return;

                if (EmberPlayerInput.ActionPressed && IsUnlocked(PlayerAbility.FlameShot))
                {
                    player.BeginRingShotAim();
                    player.UpdateRingShotAim(ReadHeldDirection());
                }
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

        private Vector2 ReadHeldDirection() => EmberPlayerInput.AimDirection(player != null ? player.FacingDirection : Vector2.right);
    }
}
