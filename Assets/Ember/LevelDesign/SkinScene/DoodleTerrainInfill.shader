Shader "Ember/Doodle Terrain Infill"
{
    Properties
    {
        _BaseMap ("Stone Texture", 2D) = "white" {}
        _BaseColor ("Stone Tint", Color) = (1,1,1,1)
        _InfillColor ("Interior Color", Color) = (0.085,0.08,0.1,1)
        _InkColor ("Hatching Ink", Color) = (0.006,0.005,0.009,1)
        _EdgeDepth ("Clean Stone Edge", Range(0.1,2)) = 0.35
        _EdgeFeather ("Hatching Gradient Width", Range(0.05,2)) = 1.1
        _EdgeVariation ("Irregular Edge Depth", Range(0,0.25)) = 0.08
        _HatchSpacing ("Hatching Spacing", Range(0.04,0.25)) = 0.085
        _HatchWidth ("Hatching Line Width", Range(0.004,0.09)) = 0.014
        [Enum(Original,0,FinePen,1,RoughPencil,2,LooseScribble,3,BoldDiagonal,4)] _HatchStyle ("Hatching Style", Float) = 0
        _SolidCoreStart ("Bold Hatching Solid Core Start", Range(0.4,0.95)) = 0.68
        [Toggle] _UseHatchMap ("Use Painted Hatching", Float) = 0
        _HatchMap ("Painted Hatching Texture", 2D) = "gray" {}
        _HatchTextureSize ("Painted Hatching World Size", Range(0.5,6)) = 3.6
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            Tags { "LightMode"="Universal2D" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Hatching/DoodleFinePen.hlsl"
            #include "Hatching/DoodlePencil.hlsl"
            #include "Hatching/DoodleScribble.hlsl"
            #include "Hatching/DoodleBoldDiagonal.hlsl"
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_HatchMap);
            SAMPLER(sampler_HatchMap);
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half4 _InfillColor;
                half4 _InkColor;
                float _EdgeDepth;
                float _EdgeFeather;
                float _EdgeVariation;
                float _HatchSpacing;
                float _HatchWidth;
                float _HatchStyle;
                float _SolidCoreStart;
                float _UseHatchMap;
                float _HatchTextureSize;
            CBUFFER_END
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float2 distanceData : TEXCOORD1;
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 worldXY : TEXCOORD1;
                float boundaryDistance : TEXCOORD2;
            };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 world = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(world);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.worldXY = world.xy;
                output.boundaryDistance = input.distanceData.x;
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                half4 stone = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
                float2 p = input.worldXY;
                // Continuous world-space irregularity keeps the internal boundary off the tile grid.
                float noise = sin(p.x * 9.3 + sin(p.y * 7.1)) * 0.55
                    + sin(p.y * 18.7 + p.x * 13.1) * 0.3
                    + sin(p.x * 32.3 - p.y * 23.9) * 0.15;
                float depth = _EdgeDepth + noise * _EdgeVariation;
                float inside = smoothstep(depth, depth + max(_EdgeFeather, 0.001), input.boundaryDistance);
                half3 paper = lerp(stone.rgb, _InfillColor.rgb, inside);

                // The bold style keeps a readable stone rim, with no texture left in its core.
                [branch] if (_HatchStyle > 3.5)
                {
                    if (inside >= 0.99) return half4(_InfillColor.rgb, stone.a);
                    [branch] if (_UseHatchMap > 0.5)
                    {
                        half3 pigment = SAMPLE_TEXTURE2D(_HatchMap, sampler_HatchMap,
                            p / max(_HatchTextureSize, 0.01)).rgb;
                        // Rank whole rough strokes with a coarser mip, retaining paper grain
                        // in the final color. Density and ink opacity grow over the full band.
                        half3 strokeTone = SAMPLE_TEXTURE2D_LOD(_HatchMap, sampler_HatchMap,
                            p / max(_HatchTextureSize, 0.01), 2.0).rgb;
                        float strokeLuma = dot(strokeTone, half3(0.2126,0.7152,0.0722));
                        // This texture's measured linear 10th/90th percentiles are .0117/.0730.
                        float cutoff = lerp(0.008, 0.090, inside * inside);
                        float density = 1.0 - smoothstep(cutoff - 0.003, cutoff + 0.003, strokeLuma);
                        float opacity = smoothstep(0.02, 0.84, inside);
                        float cover = density * opacity;
                        half3 underpaint = lerp(stone.rgb, _InfillColor.rgb, inside * 0.25);
                        half3 painted = lerp(underpaint, pigment, cover);
                        float paintedCore = smoothstep(_SolidCoreStart, 0.99, inside);
                        return half4(lerp(painted, _InfillColor.rgb, paintedCore), stone.a);
                    }
                    float boldInk = DoodleBoldDiagonalInk(p, inside, _HatchSpacing, _HatchWidth);
                    half3 boldPaper = lerp(stone.rgb, _InfillColor.rgb, inside * 0.5);
                    half3 hatched = lerp(boldPaper, _InkColor.rgb, boldInk);
                    float core = smoothstep(_SolidCoreStart, 0.99, inside);
                    return half4(lerp(hatched, _InfillColor.rgb, core), stone.a);
                }

                // Fixed spacing keeps individual pen strokes continuous as density grows.
                float wobbleA = sin(p.y * 11.0 + p.x * 2.1) * 0.006 + sin(p.x * 27.0) * 0.003;
                float wobbleB = sin(p.x * 9.0 - p.y * 3.1) * 0.007;
                float spacing = max(_HatchSpacing, 0.01);
                float phaseA = (dot(p, float2(0.7071, 0.7071)) + wobbleA) / spacing;
                float phaseB = (dot(p, float2(0.7660, -0.6428)) + wobbleB) / (spacing * 1.17);
                float lineDistanceA = abs(frac(phaseA) - 0.5) * spacing;
                float lineDistanceB = abs(frac(phaseB) - 0.5) * spacing * 1.17;
                float aaA = max(fwidth(phaseA) * spacing * 0.6, 0.0005);
                float aaB = max(fwidth(phaseB) * spacing * 1.17 * 0.6, 0.0005);
                float pressureA = 0.88 + sin(p.x * 16.7 + p.y * 11.3) * 0.12;
                float pressureB = 0.84 + sin(p.y * 19.1 - p.x * 7.3) * 0.16;
                float halfWidthA = _HatchWidth * lerp(0.5, 0.85, inside) * pressureA;
                float halfWidthB = _HatchWidth * lerp(0.35, 0.7, inside) * pressureB;
                float strokeA = 1.0 - smoothstep(halfWidthA - aaA, halfWidthA + aaA, lineDistanceA);
                float strokeB = 1.0 - smoothstep(halfWidthB - aaB, halfWidthB + aaB, lineDistanceB);
                float opacityA = smoothstep(0.0, 0.4, inside) * 0.95;
                float opacityB = smoothstep(0.22, 0.8, inside) * 0.52;
                float ink = 1.0 - (1.0 - strokeA * opacityA) * (1.0 - strokeB * opacityB);
                [branch] if (_HatchStyle > 2.5)
                    ink = DoodleScribbleInk(p, inside, spacing, _HatchWidth);
                else if (_HatchStyle > 1.5)
                    ink = DoodlePencilInk(p, inside, spacing, _HatchWidth);
                else if (_HatchStyle > 0.5)
                    ink = DoodleFinePenInk(p, inside, spacing, _HatchWidth);
                return half4(lerp(paper, _InkColor.rgb, ink), stone.a);
            }
            ENDHLSL
        }
    }
}
