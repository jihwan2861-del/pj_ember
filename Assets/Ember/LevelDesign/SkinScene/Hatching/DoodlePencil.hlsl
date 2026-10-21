#ifndef EMBER_DOODLE_PENCIL_INCLUDED
#define EMBER_DOODLE_PENCIL_INCLUDED

float PencilHash(float2 p)
{
    float3 q = frac(float3(p.xyx) * float3(0.1031, 0.1030, 0.0973));
    q += dot(q, q.yzx + 33.33);
    return frac((q.x + q.y) * q.z);
}

float PencilGrain(float2 p)
{
    float2 cell = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    return lerp(lerp(PencilHash(cell), PencilHash(cell + float2(1, 0)), f.x),
                lerp(PencilHash(cell + float2(0, 1)), PencilHash(cell + 1.0), f.x), f.y);
}

// Each row has independently staggered strokes; no stroke ends on a tile boundary.
float PencilStrokeLayer(float2 p, float2 direction, float depth,
                        float spacing, float width, float salt)
{
    float2 across = float2(-direction.y, direction.x);
    float u = dot(p, direction);
    float v = dot(p, across);
    float span = spacing * 7.7;
    float row = floor(v / spacing + 0.5);
    float result = 0.0;
    float pixel = max(length(float2(ddx(u), ddy(u))), 0.0001);

    [unroll] for (int r = -1; r <= 1; r++)
    {
        float rowId = row + r;
        float rowSeed = PencilHash(float2(rowId, salt));
        float rowShift = (rowSeed - 0.5) * spacing * 0.42;
        float stagger = PencilHash(float2(rowId + 91.7, salt)) * span;
        float segment = floor((u + stagger) / span);

        [unroll] for (int s = -1; s <= 1; s++)
        {
            float2 id = float2(rowId + salt * 13.7, segment + s);
            float a = PencilHash(id);
            float b = PencilHash(id + float2(23.4, 17.1));
            float c = PencilHash(id + float2(71.8, 3.6));
            float start = (segment + s) * span - stagger + span * (0.025 + a * 0.12);
            float strokeLength = span * (0.67 + b * 0.30);
            float t = saturate((u - start) / strokeLength);
            float endDistance = max(start - u, u - start - strokeLength);
            float endAA = max(pixel * 0.8, 0.0005);
            float cap = 1.0 - smoothstep(-endAA, endAA, endDistance);

            float bend = sin(t * 4.5 + a * 6.2832) * spacing * 0.038;
            bend += sin(u * 31.0 + rowSeed * 21.0) * spacing * 0.018;
            float slant = (t - 0.5) * (c - 0.5) * spacing * 0.34;
            float center = rowId * spacing + rowShift + bend + slant;
            float distanceToStroke = abs(v - center);
            float aa = max(fwidth(v - center) * 0.55, 0.0005);

            // A lightly tapered, uneven pressure profile leaves a drawn pencil finish.
            float taper = pow(saturate(sin(t * 3.141593)), 0.28);
            float pressure = lerp(0.48, 1.08, taper) * lerp(0.73, 1.13, c);
            pressure *= 0.90 + 0.10 * sin(t * 17.0 + b * 12.0);
            float grain = PencilGrain(p * 63.0 + id * 0.71);
            // Suppress unresolved graphite grain instead of letting it shimmer.
            float grainVisibility = 1.0 - smoothstep(0.014, 0.035, pixel);
            pressure *= lerp(1.0, lerp(0.72, 1.16, grain), grainVisibility);
            float halfWidth = width * 0.58 * pressure;
            float coverage = 1.0 - smoothstep(halfWidth - aa, halfWidth + aa, distanceToStroke);
            float emergence = smoothstep(a * 0.19, a * 0.19 + 0.31, depth);
            float graphite = lerp(0.70, 0.96, b) * lerp(0.88, 1.0, grain);
            float stroke = coverage * cap * emergence * graphite;
            result = 1.0 - (1.0 - result) * (1.0 - stroke);
        }
    }
    return saturate(result);
}

float DoodlePencilInk(float2 p, float depth, float spacing, float width)
{
    depth = saturate(depth);
    spacing = max(spacing, 0.015);
    width = max(width, 0.001);
    float primary = PencilStrokeLayer(p, float2(0.743145, -0.669131),
                                     depth, spacing, width, 2.3);
    float crossDepth = smoothstep(0.40, 0.98, depth);
    float secondary = PencilStrokeLayer(p, float2(0.615662, 0.788011),
                                       crossDepth, spacing * 1.31, width * 0.73, 17.9);
    return saturate(1.0 - (1.0 - primary) * (1.0 - secondary * 0.62));
}

#endif
