#ifndef EMBER_DOODLE_BOLD_DIAGONAL_INCLUDED
#define EMBER_DOODLE_BOLD_DIAGONAL_INCLUDED

float3 BoldDiagonalHash(float2 cell)
{
    float3 h = frac(float3(cell.xyx) * float3(0.1031, 0.1030, 0.0973));
    h += dot(h, h.yxz + 33.33);
    return frac((h.xxy + h.yzz) * h.zyx);
}

// A short, straight pen stroke. Its circular caps and slightly uneven nib edge
// are independent of the cell grid, and remain fixed in world space.
float BoldDiagonalStroke(float2 offset, float3 seed, float pitch, float penWidth)
{
    float angle = 0.78539816 + (seed.x - 0.5) * 0.27925268; // /, with +/-8 degrees.
    float sine, cosine;
    sincos(angle, sine, cosine);
    float2 along = float2(cosine, sine);
    float2 across = float2(-sine, cosine);
    float2 q = float2(dot(offset, along), dot(offset, across));

    float halfLength = pitch * lerp(0.39, 0.70, seed.y);
    float centerShift = (seed.z - 0.5) * pitch * 0.13;
    float nearest = clamp(q.x, -halfLength + centerShift, halfLength + centerShift);
    float distance = length(float2(q.x - nearest, q.y));
    float endToEnd = saturate((nearest + halfLength - centerShift) / (2.0 * halfLength));

    // Gentle pressure variation and small nib irregularities keep the strokes
    // bold and handmade without turning them into curves or distressed dots.
    float pressure = lerp(0.88, 1.13, seed.z)
        * lerp(0.91, 1.07, endToEnd);
    float rough = sin(q.x / max(penWidth, 0.001) * 7.0 + seed.y * 29.0) * 0.055
        + sin((q.x + q.y * 0.7) / max(penWidth, 0.001) * 16.0 + seed.x * 17.0) * 0.025;
    float radius = penWidth * 0.5 * (pressure + rough);
    float aa = max(fwidth(distance) * 0.60, 0.00065);
    return 1.0 - smoothstep(radius - aa, radius + aa, distance);
}

float DoodleBoldDiagonalInk(float2 p, float depth, float spacing, float width)
{
    float pitch = max(spacing, 0.025) * 2.15;
    float penWidth = max(width, 0.003);
    float ink = 0.0;
    depth = saturate(depth);

    // Offset passes accumulate parallel gestures. There is no opposing angle,
    // grid crosshatch, animation, or transparency variation within an active mark.
    [unroll]
    for (int layer = 0; layer < 2; ++layer)
    {
        float2 samplePoint = p + layer * float2(17.371, -13.219);
        float2 cell = floor(samplePoint / pitch);
        [unroll]
        for (int y = -1; y <= 1; ++y)
        {
            [unroll]
            for (int x = -1; x <= 1; ++x)
            {
                float2 key = cell + float2(x, y);
                float3 seed = BoldDiagonalHash(key + layer * float2(31.73, 79.17));
                float2 center = (key + 0.5 + (seed.xy - 0.5) * 0.80) * pitch;
                float stroke = BoldDiagonalStroke(samplePoint - center, seed, pitch, penWidth);
                float onset = lerp(0.015, 0.28, seed.z) + layer * 0.08;
                float appear = smoothstep(onset, onset + 0.075, depth);
                ink = 1.0 - (1.0 - ink) * (1.0 - stroke * appear);
            }
        }
    }
    return saturate(ink);
}

#endif
