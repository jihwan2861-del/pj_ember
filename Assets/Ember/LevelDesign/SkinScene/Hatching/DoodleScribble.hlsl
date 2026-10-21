#ifndef EMBER_DOODLE_SCRIBBLE_INCLUDED
#define EMBER_DOODLE_SCRIBBLE_INCLUDED

// Stable, world-space pen marks. No time input: the drawing stays still while moving.
float3 ScribbleHash(float2 cell)
{
    float3 h = frac(float3(cell.xyx) * float3(0.1031, 0.1030, 0.0973));
    h += dot(h, h.yxz + 33.33);
    return frac((h.xxy + h.yzz) * h.zyx);
}

float2 ScribbleRotate(float2 p, float angle)
{
    float s, c;
    sincos(angle, s, c);
    return float2(c * p.x - s * p.y, s * p.x + c * p.y);
}

// An incomplete, slightly lopsided elliptical loop, with a pressure-tapered opening.
float ScribbleArc(float2 p, float3 seed, float patchSize, float penWidth)
{
    float2 q = ScribbleRotate(p, seed.x * 6.2831853);
    float radius = patchSize * lerp(0.29, 0.48, seed.y);
    float2 axes = radius * float2(lerp(0.48, 0.95, seed.z), 1.0);
    float2 ellipse = q / max(axes, 0.0001);
    float radial = max(length(ellipse), 0.0001);
    float2 direction = ellipse / radial;
    float angle = atan2(ellipse.y, ellipse.x);
    float wobble = 0.14 * sin(angle * 2.0 + seed.x * 9.3)
        + 0.09 * sin(angle * 3.0 + seed.z * 15.7)
        + 0.04 * sin(angle * 7.0 + seed.y * 12.1);
    float distance = abs(radial - 1.0 - wobble)
        / max(length(direction / max(axes, 0.0001)), 0.001);

    // Different openings and aspect ratios prevent a repeating circle pattern.
    float gate = direction.x - lerp(-0.93, -0.15, seed.z);
    float gateAA = max(fwidth(gate), 0.025);
    float openArc = smoothstep(-gateAA, gateAA, gate);
    float taper = lerp(0.35, 1.0, smoothstep(0.0, 0.50, gate));
    float pressure = 0.76 + 0.24 * sin(angle * 2.3 + seed.x * 13.7);
    float aa = max(fwidth(distance) * 0.65, 0.0007);
    float halfWidth = penWidth * 0.5 * pressure * taper;
    float dryPen = lerp(0.4, 1.0, smoothstep(-0.75, 0.15,
        sin(angle * 8.7 + seed.y * 31.0)));
    return (1.0 - smoothstep(halfWidth - aa, halfWidth + aa, distance)) * openArc * dryPen;
}

// A capped S-shaped flick interrupts the loops with quick, directional pen movement.
float ScribbleFlick(float2 p, float3 seed, float patchSize, float penWidth)
{
    float2 q = ScribbleRotate(p, seed.y * 6.2831853);
    float halfLength = patchSize * lerp(0.40, 0.78, seed.z);
    float x = clamp(q.x, -halfLength, halfLength);
    float phase = x / max(halfLength, 0.0001) * 3.8 + seed.x * 5.0;
    float amplitude = patchSize * lerp(0.08, 0.20, seed.y);
    float curve = sin(phase) * amplitude + sin(phase * 2.1 + seed.z * 9.0) * amplitude * 0.20;
    float slope = (cos(phase) + cos(phase * 2.1 + seed.z * 9.0) * 0.42)
        * amplitude * 3.8 / max(halfLength, 0.0001);
    float normalDistance = (q.y - curve) / sqrt(1.0 + slope * slope);
    float distance = length(float2(normalDistance, q.x - x));
    float taper = lerp(0.35, 1.0, saturate((1.0 - abs(x) / halfLength) * 3.0));
    float pressure = 0.84 + 0.16 * sin(phase * 2.7 + seed.z * 12.0);
    float aa = max(fwidth(distance) * 0.65, 0.0007);
    float halfWidth = penWidth * 0.5 * pressure * taper;
    float dryPen = lerp(0.40, 1.0, smoothstep(-0.75, 0.15,
        sin(phase * 3.7 + seed.y * 23.0)));
    return (1.0 - smoothstep(halfWidth - aa, halfWidth + aa, distance)) * dryPen;
}

float DoodleScribbleInk(float2 p, float depth, float spacing, float width)
{
    float patchSize = max(spacing, 0.015) * 1.85;
    float penWidth = max(width, 0.001);
    float ink = 0.0;
    depth = saturate(depth);

    // Two offset, independently rotated drawings. Each searches only its 3x3 neighbors.
    [unroll]
    for (int layer = 0; layer < 2; ++layer)
    {
        float2 samplePoint = ScribbleRotate(p, layer * 1.137)
            + float2(13.73, -9.41) * layer;
        float2 cell = floor(samplePoint / patchSize);
        [unroll]
        for (int y = -1; y <= 1; ++y)
        {
            [unroll]
            for (int x = -1; x <= 1; ++x)
            {
                float2 key = cell + float2(x, y);
                float3 seed = ScribbleHash(key + layer * float2(37.13, 71.87));
                float2 center = (key + 0.5 + (seed.xy - 0.5) * 0.72) * patchSize;
                float2 local = samplePoint - center;
                float arc = ScribbleArc(local, seed, patchSize, penWidth);
                float flick = ScribbleFlick(local + (seed.yz - 0.5) * patchSize * 0.34,
                    seed.zxy, patchSize, penWidth * 0.90);
                // Overlapping gestures form a wiry drawing rather than isolated curled symbols.
                float arcStrength = lerp(0.28, 0.74, seed.x);
                float mark = 1.0 - (1.0 - arc * arcStrength) * (1.0 - flick);
                float onset = lerp(0.008, 0.27, seed.z) + layer * 0.07;
                float opacity = smoothstep(onset, min(onset + 0.32, 0.98), depth);
                opacity *= lerp(0.78, 0.98, seed.y);
                ink = 1.0 - (1.0 - ink) * (1.0 - mark * opacity);
            }
        }
    }
    return saturate(ink);
}

#endif
