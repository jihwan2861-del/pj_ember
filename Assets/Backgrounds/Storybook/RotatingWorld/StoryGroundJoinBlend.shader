Shader "Ember/Story Ground Join Blend"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [HideInInspector] _JoinCircle ("Join Circle", Vector) = (0,0,0,0)
        [HideInInspector] _JoinLeftUV ("Join Left UV", Float) = 0.01105
        [HideInInspector] _JoinWidthUV ("Join Width UV", Float) = 0.05
        [HideInInspector] _JoinSurfaceFeather ("Join Surface Feather", Float) = 0.08
        [HideInInspector] _JoinEnabled ("Join Enabled", Float) = 0
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

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float4 _JoinCircle;
                float _JoinLeftUV;
                float _JoinWidthUV;
                float _JoinSurfaceFeather;
                float _JoinEnabled;
            CBUFFER_END

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 worldXY : TEXCOORD1;
                half4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(Attributes input)
            {
                UNITY_SETUP_INSTANCE_ID(input);
                SetUpSpriteInstanceProperties();
                Varyings output;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                float3 positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);
                float3 positionWS = TransformObjectToWorld(positionOS);
                output.positionCS = TransformWorldToHClip(positionWS);
                output.worldXY = positionWS.xy;
                output.uv = input.uv;
                output.color = input.color * _Color * unity_SpriteColor;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * input.color;
                float overlap = saturate((_JoinCircle.z - distance(input.worldXY, _JoinCircle.xy))
                    / max(_JoinSurfaceFeather, 0.001));

                // Uneven marker-like boundary; approximately 0.05U at the default width.
                float noise = sin(input.worldXY.y * 17.3 + input.worldXY.x * 2.1) * 0.7
                    + sin(input.worldXY.y * 37.1 - input.worldXY.x * 5.3) * 0.3;
                float widthUV = max(_JoinWidthUV, 0.00001);
                float fade = smoothstep(0.0, widthUV,
                    input.uv.x - _JoinLeftUV + noise * widthUV * 0.035);

                // Preserve the slab wherever the circle cannot cover it, especially the top edge.
                color.a *= lerp(1.0, fade, overlap * saturate(_JoinEnabled));
                return color;
            }
            ENDHLSL
        }
    }
}
