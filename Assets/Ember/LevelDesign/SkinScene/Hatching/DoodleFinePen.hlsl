#ifndef EMBER_DOODLE_FINE_PEN_INCLUDED
#define EMBER_DOODLE_FINE_PEN_INCLUDED

float PenHash(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

// Each stroke has its own length, offset and pressure, rather than a continuous lattice.
float PenStrokeFamily(float2 p, float angle, float spacing, float width, float seed)
{
    float2 along = float2(cos(angle), sin(angle));
    float2 across = float2(-along.y, along.x);
    float x = dot(p, along), y = dot(p, across);
    float band = floor(y / spacing);
    float coverage = 0.0;
    [unroll] for (int n = -1; n <= 1; n++)
    {
        float row = band + n;
        float rowSeed = PenHash(float2(row, seed));
        float length = spacing * lerp(3.8, 7.0, rowSeed);
        float shifted = x + rowSeed * length;
        float segment = floor(shifted / length);
        float segmentSeed = PenHash(float2(row + seed * 9.7, segment));
        float u = frac(shifted / length);
        float taper = smoothstep(0.0, 0.12, u) * (1.0 - smoothstep(0.79, 0.98, u));
        float offset = (rowSeed - 0.5) * spacing * 0.36;
        float bend = sin(x * 17.0 + rowSeed * 35.0) * 0.0035
            + sin(x * 6.3 + segmentSeed * 31.0) * 0.004;
        float distance = abs(y - (row + 0.5) * spacing - offset - bend);
        float pressure = lerp(0.65, 1.15, segmentSeed)
            * (0.88 + 0.12 * sin(x * 33.0 + rowSeed * 19.0));
        float halfWidth = width * 0.5 * pressure * lerp(0.5, 1.0, taper);
        float aa = max(fwidth(distance) * 0.65, 0.0004);
        float penMark = 1.0 - smoothstep(halfWidth - aa, halfWidth + aa, distance);
        coverage = max(coverage, penMark * taper * lerp(0.72, 1.0, segmentSeed));
    }
    return saturate(coverage);
}

float DoodleFinePenInk(float2 p, float depth, float spacing, float width)
{
    float a = PenStrokeFamily(p, 0.68, spacing, width * lerp(0.70, 1.12, depth), 2.3);
    float b = PenStrokeFamily(p, -0.81, spacing * 1.24, width * 0.7, 8.7);
    float c = PenStrokeFamily(p, 1.21, spacing * 1.83, width * 0.55, 19.1);
    a *= smoothstep(0.015, 0.36, depth) * 0.92;
    b *= smoothstep(0.28, 0.83, depth) * 0.66;
    c *= smoothstep(0.72, 1.0, depth) * 0.42;
    return 1.0 - (1.0 - a) * (1.0 - b) * (1.0 - c);
}
#endif
