Shader "Ember/Corrupted Sun Doodle Waver"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _WaverStrength ("Waver Strength (UV)", Range(0,0.04)) = 0.006
        _WaverSpeed ("Waver Speed", Range(0,8)) = 1.4
        _WaverScale ("Waver Scale", Range(1,40)) = 13
        _CorruptionStart ("Corruption Starts At U", Range(0,1)) = 0.46
        _MaskSoftness ("Mask Softness", Range(0.001,0.25)) = 0.09
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Pass
        {
            Tags { "LightMode"="Universal2D" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float _WaverStrength;
                float _WaverSpeed;
                float _WaverScale;
                float _CorruptionStart;
                float _MaskSoftness;
            CBUFFER_END
            struct Attributes { float3 positionOS:POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; UNITY_VERTEX_OUTPUT_STEREO };
            Varyings Vert(Attributes input)
            {
                UNITY_SETUP_INSTANCE_ID(input); SetUpSpriteInstanceProperties();
                Varyings o; UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                float3 p=UnityFlipSprite(input.positionOS,unity_SpriteProps.xy);
                o.positionCS=TransformObjectToHClip(p); o.uv=input.uv;
                o.color=input.color*_Color*unity_SpriteColor; return o;
            }
            half4 Frag(Varyings i):SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float t=_Time.y*_WaverSpeed;
                float2 uv=i.uv;
                half4 baseCol=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,uv)*i.color;
                // Use the sprite's purple ink and dark right-hand ink as the corruption mask.
                // This preserves the warm half even where the painted boundary wanders.
                float purpleInk=saturate((baseCol.b-baseCol.r)*3.2)*saturate((baseCol.b-baseCol.g)*2.2);
                float luminance=dot(baseCol.rgb,float3(0.299,0.587,0.114));
                float darkInk=saturate((0.48-luminance)*3.2)*smoothstep(_CorruptionStart,_CorruptionStart+_MaskSoftness,uv.x);
                float ink=max(purpleInk,darkInk)*smoothstep(0.015,0.16,baseCol.a);
                float2 wobble=float2(sin(uv.y*_WaverScale+t+sin(uv.y*5.0-t*0.31)),
                    sin(uv.x*(_WaverScale*0.73)-t*0.82+sin(uv.x*4.2+t*0.27)));
                float2 warped=uv+wobble*(_WaverStrength*ink);
                return SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,warped)*i.color;
            }
            ENDHLSL
        }
    }
    FallBack "Sprites/Default"
}
