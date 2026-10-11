using UnityEngine;

namespace EmberPrototype
{
    public abstract class WaterSpiritBehavior : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float moveSpeed = 8f;
        [SerializeField, Min(0f)] private float trackingTime = 2f;
        [SerializeField, Min(0f)] private float warningTime = 0.7f;
        [SerializeField, Min(0f)] private float recoveryTime = 0.5f;
        public float MoveSpeed => moveSpeed;
        public float TrackingTime => trackingTime;
        public float WarningTime => warningTime;
        public float RecoveryTime => recoveryTime;
        public virtual bool CanRush => false;
        public virtual void Enter(WaterSpiritAI ai) { }
        public virtual void Exit(WaterSpiritAI ai) { }
        public abstract Vector2 GetMoveTarget(WaterSpiritAI ai);
        public abstract bool IsReady(WaterSpiritAI ai);
        public abstract WaterSpiritAttack SelectAttack(WaterSpiritAI ai);
    }
}
