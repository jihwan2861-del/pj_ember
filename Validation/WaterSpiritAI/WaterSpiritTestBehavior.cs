using UnityEngine;
using EmberPrototype;

// Copied into the isolated project's runtime assembly only, never into the shipped Assets.
public sealed class WaterSpiritTestBehavior : WaterSpiritBehavior
{
    public int EnterCount { get; private set; }
    public int ExitCount { get; private set; }
    public override void Enter(WaterSpiritAI ai) => EnterCount++;
    public override void Exit(WaterSpiritAI ai) => ExitCount++;
    public override Vector2 GetMoveTarget(WaterSpiritAI ai) => ai.transform.position;
    public override bool IsReady(WaterSpiritAI ai) => false;
    public override WaterSpiritAttack SelectAttack(WaterSpiritAI ai) =>
        ai.Spirit.WaveTemplate.DescribeAttack(ai.transform.position, WaterWave.TravelDirection.RightToLeft);
}
