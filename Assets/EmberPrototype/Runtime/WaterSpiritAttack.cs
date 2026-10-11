using UnityEngine;

namespace EmberPrototype
{
    // The same immutable world-space description drives the warning and the projectile.
    public readonly struct WaterSpiritAttack
    {
        public readonly Vector2 Start;
        public readonly WaterWave.TravelDirection Direction;
        public readonly float Height, Width, Speed, Distance;
        public readonly bool Deadly;

        public WaterSpiritAttack(Vector2 start, WaterWave.TravelDirection direction,
            float height, float width, float speed, float distance, bool deadly)
        {
            Start = start; Direction = direction;
            Height = height; Width = width; Speed = speed; Distance = distance; Deadly = deadly;
        }

        public bool IsValid => Height > 0f && Width > 0f && Speed > 0f && Distance > 0f &&
            float.IsFinite(Start.x) && float.IsFinite(Start.y) && float.IsFinite(Height) &&
            float.IsFinite(Width) && float.IsFinite(Speed) && float.IsFinite(Distance);

        public Bounds DangerBounds
        {
            get
            {
                float sign = Direction == WaterWave.TravelDirection.LeftToRight ? 1f : -1f;
                return new Bounds(Start + new Vector2(sign * Distance / 2f, Height / 2f),
                    new Vector3(Distance + Width, Height, 0.1f));
            }
        }
    }
}
