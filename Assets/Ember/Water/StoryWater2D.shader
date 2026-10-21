Shader "Ember/Story Water 2D"
{
    Properties
    {
        [PerRendererData] _MainTex("Water texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        _ShallowColor("Shallow water", Color) = (0.12,0.62,0.69,1)
        _DeepColor("Deep water", Color) = (0.015,0.19,0.3,1)
        _ReflectionStrength("Reflection strength", Range(0,1)) = 0.78
        _Distortion("Horizontal distortion", Range(0,0.04)) = 0.007
        _WaveSpeed("Wave speed", Range(0,3)) = 0.65
        _WaveFrequency("Wave frequency", Range(1,40)) = 14
        _SurfaceY("Surface world Y", Float) = -2.3
        _WaterDepth("Depth", Float) = 5
        _TextureStrength("Watercolor grain", Range(0,1)) = 0.12
        _PaintTiling("Water paint tiling", Vector) = (1,1,0,0)
        _FoamColor("Surface highlight", Color) = (0.65,0.91,0.94,1)
        _PastTex("Healthy past landscape (upright)", 2D) = "white" {}
        _PastBlend("Past reflection reveal", Range(0,1)) = 0
        [HideInInspector] _HasPastLandscape("Has past landscape", Float) = 1
        [HideInInspector] _PastSkyOnly("Preserve present geography", Float) = 0
        [HideInInspector] _PastLayerCount("Past reflection layer count", Float) = 0
        [HideInInspector] _PastLayerTex0("Past layer 0", 2D) = "black" {}
        [HideInInspector] _PastLayerTex1("Past layer 1", 2D) = "black" {}
        [HideInInspector] _PastLayerTex2("Past layer 2", 2D) = "black" {}
        [HideInInspector] _PastLayerTex3("Past layer 3", 2D) = "black" {}
        [HideInInspector] _PastLayerTex4("Past layer 4", 2D) = "black" {}
        [HideInInspector] _PastLayerTex5("Past layer 5", 2D) = "black" {}
        [HideInInspector] _PastLayerTex6("Past layer 6", 2D) = "black" {}
        [HideInInspector] _PastLayerTex7("Past layer 7", 2D) = "black" {}
        [HideInInspector] _PastLayerRect0("Past layer rectangle 0", Vector) = (0,0,1,1)
        [HideInInspector] _PastLayerRect1("Past layer rectangle 1", Vector) = (0,0,1,1)
        [HideInInspector] _PastLayerRect2("Past layer rectangle 2", Vector) = (0,0,1,1)
        [HideInInspector] _PastLayerRect3("Past layer rectangle 3", Vector) = (0,0,1,1)
        [HideInInspector] _PastLayerRect4("Past layer rectangle 4", Vector) = (0,0,1,1)
        [HideInInspector] _PastLayerRect5("Past layer rectangle 5", Vector) = (0,0,1,1)
        [HideInInspector] _PastLayerRect6("Past layer rectangle 6", Vector) = (0,0,1,1)
        [HideInInspector] _PastLayerRect7("Past layer rectangle 7", Vector) = (0,0,1,1)
        [HideInInspector] _PastLayerFlip0("Past layer flip and enabled 0", Vector) = (0,0,0,0)
        [HideInInspector] _PastLayerFlip1("Past layer flip and enabled 1", Vector) = (0,0,0,0)
        [HideInInspector] _PastLayerFlip2("Past layer flip and enabled 2", Vector) = (0,0,0,0)
        [HideInInspector] _PastLayerFlip3("Past layer flip and enabled 3", Vector) = (0,0,0,0)
        [HideInInspector] _PastLayerFlip4("Past layer flip and enabled 4", Vector) = (0,0,0,0)
        [HideInInspector] _PastLayerFlip5("Past layer flip and enabled 5", Vector) = (0,0,0,0)
        [HideInInspector] _PastLayerFlip6("Past layer flip and enabled 6", Vector) = (0,0,0,0)
        [HideInInspector] _PastLayerFlip7("Past layer flip and enabled 7", Vector) = (0,0,0,0)
        [HideInInspector] _PastLayerColor0("Past layer color 0", Color) = (1,1,1,1)
        [HideInInspector] _PastLayerColor1("Past layer color 1", Color) = (1,1,1,1)
        [HideInInspector] _PastLayerColor2("Past layer color 2", Color) = (1,1,1,1)
        [HideInInspector] _PastLayerColor3("Past layer color 3", Color) = (1,1,1,1)
        [HideInInspector] _PastLayerColor4("Past layer color 4", Color) = (1,1,1,1)
        [HideInInspector] _PastLayerColor5("Past layer color 5", Color) = (1,1,1,1)
        [HideInInspector] _PastLayerColor6("Past layer color 6", Color) = (1,1,1,1)
        [HideInInspector] _PastLayerColor7("Past layer color 7", Color) = (1,1,1,1)
        [HideInInspector] _PastRepeatX("Repeat past with parallax", Float) = 0
        [HideInInspector] _ReflectionDarkening("Reflected sky darkening", Range(0,1)) = 0
        [HideInInspector] _WitheredSkyBrightness("Withered sky brightness", Range(0.01,1)) = 0.24
        [HideInInspector] _WitheredSkySaturation("Withered sky saturation", Range(0,1)) = 0.3
        _PastWorldRect("Past world rectangle", Vector) = (67,-2.3,22,8)
        [HideInInspector] _PlayerReflectionPosition("Player reflection", Vector) = (0,0,0,0)
        _FlowTex("Water light pattern", 2D) = "gray" {}
        _FlowStrength("Water light strength", Range(0,0.15)) = 0.035
        [HideInInspector] _AnimationTimeOverride("Capture time (-1 = game time)", Float) = -1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        Pass
        {
            Tags { "LightMode"="Universal2D" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D_X(_CameraSortingLayerTexture); SAMPLER(sampler_CameraSortingLayerTexture);
            TEXTURE2D(_PastTex); SAMPLER(sampler_PastTex);
            TEXTURE2D(_PastLayerTex0); SAMPLER(sampler_PastLayerTex0);
            TEXTURE2D(_PastLayerTex1); SAMPLER(sampler_PastLayerTex1);
            TEXTURE2D(_PastLayerTex2); SAMPLER(sampler_PastLayerTex2);
            TEXTURE2D(_PastLayerTex3); SAMPLER(sampler_PastLayerTex3);
            TEXTURE2D(_PastLayerTex4); SAMPLER(sampler_PastLayerTex4);
            TEXTURE2D(_PastLayerTex5); SAMPLER(sampler_PastLayerTex5);
            TEXTURE2D(_PastLayerTex6); SAMPLER(sampler_PastLayerTex6);
            TEXTURE2D(_PastLayerTex7); SAMPLER(sampler_PastLayerTex7);
            TEXTURE2D(_FlowTex); SAMPLER(sampler_FlowTex);
            CBUFFER_START(UnityPerMaterial)
                float4 _Color, _ShallowColor, _DeepColor, _FoamColor;
                float _ReflectionStrength, _Distortion, _WaveSpeed, _WaveFrequency;
                float _SurfaceY, _WaterDepth, _TextureStrength;
                float4 _PastWorldRect, _PlayerReflectionPosition, _PaintTiling;
                float _PastBlend, _PastRepeatX, _FlowStrength, _AnimationTimeOverride;
                float _HasPastLandscape, _PastSkyOnly, _PastLayerCount, _ReplacePastSky;
                float _ReflectionDarkening, _WitheredSkyBrightness, _WitheredSkySaturation;
                float4 _PastSkyReplaceRect;
                float4 _PastLayerRect0, _PastLayerRect1, _PastLayerRect2, _PastLayerRect3;
                float4 _PastLayerFlip0, _PastLayerFlip1, _PastLayerFlip2, _PastLayerFlip3;
                float4 _PastLayerRect4, _PastLayerRect5, _PastLayerRect6, _PastLayerRect7;
                float4 _PastLayerFlip4, _PastLayerFlip5, _PastLayerFlip6, _PastLayerFlip7;
                float4 _PastLayerColor0, _PastLayerColor1, _PastLayerColor2, _PastLayerColor3;
                float4 _PastLayerColor4, _PastLayerColor5, _PastLayerColor6, _PastLayerColor7;
            CBUFFER_END
            half4 SamplePastLayer(TEXTURE2D_PARAM(layerTexture, layerSampler), float2 world,
                float4 rect, float4 flip, float4 color)
            {
                float2 layerUV=(world-rect.xy)/max(rect.zw,0.01);
                float inside=step(0,layerUV.x)*step(layerUV.x,1)
                    *step(0,layerUV.y)*step(layerUV.y,1)*flip.w;
                layerUV=lerp(layerUV,1-layerUV,flip.xy);
                half4 layer=SAMPLE_TEXTURE2D(layerTexture,layerSampler,saturate(layerUV))*color;
                layer.a*=inside;
                // Designated layers fade to their present reflection; foliage also loses green.
                float wither=saturate(flip.z);
                float foliage=smoothstep(0.01,0.12,layer.g-max(layer.r,layer.b*0.85));
                float luminance=dot(layer.rgb,float3(0.299,0.587,0.114));
                half3 dryColor=luminance*float3(0.9,0.72,0.49);
                layer.rgb=lerp(layer.rgb,dryColor,saturate(wither*2)*foliage);
                layer.a*=1-smoothstep(0,1,wither);
                return layer;
            }
            void CompositePastLayer(inout half3 reflection, inout float memory, half4 layer, float blend)
            {
                float amount=saturate(layer.a*blend);
                reflection=lerp(reflection,layer.rgb,amount);
                memory=max(memory,amount);
            }
            struct Attributes { float3 positionOS:POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; float3 world:TEXCOORD1; float4 color:COLOR; };
            Varyings Vert(Attributes v)
            {
                Varyings o;
                o.world=TransformObjectToWorld(v.positionOS);
                o.positionCS=TransformWorldToHClip(o.world);
                o.uv=v.uv; o.color=v.color*_Color;
                return o;
            }
            half4 Frag(Varyings i):SV_Target
            {
                float t=(_AnimationTimeOverride>=0 ? _AnimationTimeOverride : _Time.y)*_WaveSpeed;
                float depth=saturate((_SurfaceY-i.world.y)/max(_WaterDepth,0.01));
                float wave=sin(i.world.y*_WaveFrequency+t*1.8+i.world.x*0.35)
                    +0.4*sin(i.world.y*_WaveFrequency*2.7-t*2.1+i.world.x*0.7);
                float3 reflectedWorld=float3(i.world.x,2*_SurfaceY-i.world.y,i.world.z);
                float4 reflectedClip=TransformWorldToHClip(reflectedWorld);
                float4 projected=ComputeScreenPos(reflectedClip);
                float2 uv=projected.xy/projected.w;
                uv.x+=wave*_Distortion*(0.3+depth);
                uv.y+=sin(i.world.x*3+t)*_Distortion*0.08;
                float valid=step(0,uv.y)*step(uv.y,1)*step(0,uv.x)*step(uv.x,1);
                half3 reflection=SAMPLE_TEXTURE2D_X(_CameraSortingLayerTexture,sampler_CameraSortingLayerTexture,saturate(uv)).rgb;
                // Past art is authored upright. Reflect the SAME world point into it.
                float2 pastUV=(reflectedWorld.xy-_PastWorldRect.xy)/max(_PastWorldRect.zw,0.01);
                pastUV.x+=wave*_Distortion*0.65;
                float pastEdge=smoothstep(0,0.04,pastUV.x)*(1-smoothstep(0.96,1,pastUV.x));
                // The background may show adjacent parallax copies. Repeat by the
                // same whole-tile period so recentering the source cannot flip it.
                if (_PastRepeatX > 0.5)
                {
                    pastUV.x=frac(pastUV.x);
                    pastEdge=1;
                }
                pastEdge*=step(0,pastUV.y)*step(pastUV.y,1);
                half3 past=SAMPLE_TEXTURE2D(_PastTex,sampler_PastTex,saturate(pastUV)).rgb;
                if (_PastRepeatX > 0.5)
                {
                    // The illustration is not a seamless tile. Feather both
                    // border strips into the same edge blend instead of teal.
                    float seam=1-smoothstep(0,0.055,min(pastUV.x,1-pastUV.x));
                    half3 opposite=SAMPLE_TEXTURE2D(_PastTex,sampler_PastTex,float2(1-pastUV.x,saturate(pastUV.y))).rgb;
                    past=lerp(past,opposite,seam*0.5);
                }
                // Keep Ember's warm reflection visible through the landscape dissolve.
                float ember=step(reflection.g*1.4,reflection.r)*step(reflection.b*1.8,reflection.r)*step(0.55,reflection.r);
                ember*=step(abs(reflectedWorld.x-_PlayerReflectionPosition.x),0.75)
                    *step(abs(reflectedWorld.y-_PlayerReflectionPosition.y),1.5)*_PlayerReflectionPosition.w;
                float pastBlend=saturate(_PastBlend)*(1-ember);
                float skyMask=smoothstep(0.72,0.94,min(reflection.r,min(reflection.g,reflection.b)));
                // Clear an explicitly selected current object before drawing its past form.
                float2 sunUV=(reflectedWorld.xy-_PastSkyReplaceRect.xy)/max(_PastSkyReplaceRect.zw,0.01);
                float sunArea=step(0,sunUV.x)*step(sunUV.x,1)*step(0,sunUV.y)*step(sunUV.y,1);
                skyMask=max(skyMask,sunArea*_ReplacePastSky);
                float memory=pastBlend*pastEdge*_HasPastLandscape*lerp(1,skyMask,saturate(_PastSkyOnly));
                reflection=lerp(reflection,past,memory);
                // These invisible source renderers share the current trees' world
                // placement. Only their mirrored, alpha-masked art appears here.
                float2 layerWorld=reflectedWorld.xy;
                layerWorld.x+=wave*_Distortion*(0.3+depth)*unity_OrthoParams.x*2;
                layerWorld.y+=sin(i.world.x*3+t)*_Distortion*0.08*unity_OrthoParams.y*2;
                if (_PastLayerCount > 0.5)
                    CompositePastLayer(reflection,memory,SamplePastLayer(
                        TEXTURE2D_ARGS(_PastLayerTex0,sampler_PastLayerTex0),layerWorld,_PastLayerRect0,_PastLayerFlip0,_PastLayerColor0),pastBlend);
                if (_PastLayerCount > 1.5)
                    CompositePastLayer(reflection,memory,SamplePastLayer(
                        TEXTURE2D_ARGS(_PastLayerTex1,sampler_PastLayerTex1),layerWorld,_PastLayerRect1,_PastLayerFlip1,_PastLayerColor1),pastBlend);
                if (_PastLayerCount > 2.5)
                    CompositePastLayer(reflection,memory,SamplePastLayer(
                        TEXTURE2D_ARGS(_PastLayerTex2,sampler_PastLayerTex2),layerWorld,_PastLayerRect2,_PastLayerFlip2,_PastLayerColor2),pastBlend);
                if (_PastLayerCount > 3.5)
                    CompositePastLayer(reflection,memory,SamplePastLayer(
                        TEXTURE2D_ARGS(_PastLayerTex3,sampler_PastLayerTex3),layerWorld,_PastLayerRect3,_PastLayerFlip3,_PastLayerColor3),pastBlend);
                if (_PastLayerCount > 4.5)
                    CompositePastLayer(reflection,memory,SamplePastLayer(
                        TEXTURE2D_ARGS(_PastLayerTex4,sampler_PastLayerTex4),layerWorld,_PastLayerRect4,_PastLayerFlip4,_PastLayerColor4),pastBlend);
                if (_PastLayerCount > 5.5)
                    CompositePastLayer(reflection,memory,SamplePastLayer(
                        TEXTURE2D_ARGS(_PastLayerTex5,sampler_PastLayerTex5),layerWorld,_PastLayerRect5,_PastLayerFlip5,_PastLayerColor5),pastBlend);
                if (_PastLayerCount > 6.5)
                    CompositePastLayer(reflection,memory,SamplePastLayer(
                        TEXTURE2D_ARGS(_PastLayerTex6,sampler_PastLayerTex6),layerWorld,_PastLayerRect6,_PastLayerFlip6,_PastLayerColor6),pastBlend);
                if (_PastLayerCount > 7.5)
                    CompositePastLayer(reflection,memory,SamplePastLayer(
                        TEXTURE2D_ARGS(_PastLayerTex7,sampler_PastLayerTex7),layerWorld,_PastLayerRect7,_PastLayerFlip7,_PastLayerColor7),pastBlend);
                half4 grain=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv*_PaintTiling.xy+_PaintTiling.zw);
                half3 water=lerp(_ShallowColor.rgb,_DeepColor.rgb,depth);
                water=lerp(water,grain.rgb,_TextureStrength);
                float reflectivity=lerp(_ReflectionStrength,0.86,memory)*lerp(1,0.72,depth)*max(valid,memory);
                half3 rgb=lerp(water,reflection*float3(0.78,0.94,0.98),reflectivity);
                float whiteBrush=smoothstep(0.83,0.98,min(grain.r,min(grain.g,grain.b)));
                rgb=lerp(rgb,_FoamColor.rgb,whiteBrush*_TextureStrength*0.3);
                float ripple=pow(saturate(sin(i.world.y*32+sin(i.world.x*2.5+t)*1.4+t*2)),22);
                rgb+=_FoamColor.rgb*ripple*0.035;
                float streak=pow(saturate(sin(i.world.y*42+sin(i.world.x*0.7+t)*1.8+t)),28);
                streak*=smoothstep(0.45,0.85,sin(i.world.x*2.3-t+i.world.y*1.5));
                rgb=lerp(rgb,_FoamColor.rgb,streak*0.19);
                float2 flowUV=i.world.xy*float2(0.13,0.55);
                half flowA=SAMPLE_TEXTURE2D(_FlowTex,sampler_FlowTex,flowUV+float2(t*0.025,t*0.018)).r;
                half flowB=SAMPLE_TEXTURE2D(_FlowTex,sampler_FlowTex,flowUV*1.31+float2(-t*0.018,t*0.011)).g;
                rgb+=_FoamColor.rgb*min(flowA,flowB)*_FlowStrength;
                float surfaceWave=_SurfaceY+sin(i.world.x*2+t*2)*0.025+sin(i.world.x*4.7-t)*0.012;
                float surface=saturate(1-abs(surfaceWave-i.world.y)/0.055);
                rgb=lerp(rgb,_FoamColor.rgb,surface*0.7);
                // Fade the sky, painted water and white ripples together. Apply
                // this after all highlights so the bright paint cannot undo dusk.
                float dusk=smoothstep(0,1,saturate(_ReflectionDarkening))*(1-ember);
                float skyDusk=dusk*lerp(0.55,1,saturate(skyMask));
                float luminance=dot(rgb,float3(0.299,0.587,0.114));
                rgb=lerp(luminance.xxx,rgb,lerp(1,_WitheredSkySaturation,skyDusk));
                rgb*=lerp(1,_WitheredSkyBrightness,skyDusk);
                float alpha=i.color.a*(1-smoothstep(surfaceWave-0.035,surfaceWave+0.02,i.world.y));
                return half4(rgb*i.color.rgb,alpha);
            }
            ENDHLSL
        }
    }
}



