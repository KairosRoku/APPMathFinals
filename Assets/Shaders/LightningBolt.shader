Shader "Custom/LightningBolt"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 0, 1)
        _Jitter ("Jitter", Float) = 0.5
        _Speed ("Speed", Float) = 10
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend One One
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR; // Capture LineRenderer vertex colors
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float _Jitter;
                float _Speed;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // Add some math-based jitter to the vertices
                float noise = sin(_Time.y * _Speed + input.positionOS.x * 10);
                input.positionOS.xyz += normalize(input.positionOS.xyz) * noise * _Jitter;

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 col = tex * _Color * input.color; // Multiply by vertex color
                return col;
            }
            ENDHLSL
        }
    }
}
