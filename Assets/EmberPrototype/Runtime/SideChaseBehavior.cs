using UnityEngine;

namespace EmberPrototype
{
    [AddComponentMenu("Ember Prototype/Water Spirit Side Chase")]
    public sealed class SideChaseBehavior : WaterSpiritBehavior
    {
        [SerializeField, Min(0f)] private float screenMargin = 1f;
        [SerializeField, Min(0.01f)] private float arrivalTolerance = 0.1f;
        [SerializeField, Min(0.1f)] private float followDistance = 3f;
        [SerializeField] private float rideHeightOffset = 0.9f;
        [SerializeField] private bool useRidingRush = true;
        public override bool CanRush => useRidingRush;

        public override Vector2 GetMoveTarget(WaterSpiritAI ai)
        {
            return ai.PlayerCenter + new Vector2(-followDistance, rideHeightOffset);
        }

        public override bool IsReady(WaterSpiritAI ai) => true;

        public override WaterSpiritAttack SelectAttack(WaterSpiritAI ai)
        {
            WaterWave template = ai.Spirit.WaveTemplate;
            WaterWave.TravelDirection facing = ai.PlayerCenter.x >= ai.transform.position.x
                ? WaterWave.TravelDirection.LeftToRight : WaterWave.TravelDirection.RightToLeft;
            Vector2 start = ai.Spirit.GetWaveSpawn(facing);
            start.y = ai.PlayerCenter.y - template.WorldHeight / 2f;
            return template.DescribeAttack(start, facing);
        }
    }
}
