using UnityEngine;

namespace EmberPrototype
{
    public enum PlayerAbility
    {
        FireAbsorb,
        IgnitionRing,
        BurstDash,
        FlameShot
    }

    [DisallowMultipleComponent]
    public sealed class PlayerAbilityUnlocks : MonoBehaviour
    {
        [Header("Stage Ability Unlocks")]
        [SerializeField] private bool fireAbsorbUnlocked = true;
        [SerializeField] private bool ignitionRingUnlocked = true;
        [SerializeField] private bool burstDashUnlocked = true;
        [SerializeField] private bool flameShotUnlocked = true;

        public bool IsUnlocked(PlayerAbility ability)
        {
            switch (ability)
            {
                case PlayerAbility.FireAbsorb: return fireAbsorbUnlocked;
                case PlayerAbility.IgnitionRing: return ignitionRingUnlocked;
                case PlayerAbility.BurstDash: return burstDashUnlocked;
                case PlayerAbility.FlameShot: return flameShotUnlocked;
                default: return false;
            }
        }

        public void SetUnlocked(PlayerAbility ability, bool unlocked)
        {
            switch (ability)
            {
                case PlayerAbility.FireAbsorb: fireAbsorbUnlocked = unlocked; break;
                case PlayerAbility.IgnitionRing: ignitionRingUnlocked = unlocked; break;
                case PlayerAbility.BurstDash: burstDashUnlocked = unlocked; break;
                case PlayerAbility.FlameShot: flameShotUnlocked = unlocked; break;
            }
        }
    }
}
