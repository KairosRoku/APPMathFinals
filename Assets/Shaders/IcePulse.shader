Shader "Custom/IcePulse"
{
    Properties
    {
        _Color ("Base Color", Color) = (0.5, 0.8, 1, 0.5)
        _RimColor ("Rim Color", Color) = (1, 1, 1, 1)
        _PulseSpeed ("Pulse Speed", Float) = 2
        _Thickness ("Thickness", Range(0, 0.5)) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
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
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _RimColor;
                float _PulseSpeed;
                float _Thickness;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 centeredUV = input.uv - 0.5;
                float dist = length(centeredUV) * 2;
                
                float pulse = frac(_Time.y * _PulseSpeed);
                float ring = smoothstep(pulse - _Thickness, pulse, dist) - smoothstep(pulse, pulse + _Thickness, dist);
                
                half4 finalColor = _Color;
                finalColor.rgb += ring * _RimColor.rgb;
                finalColor.a *= (1.0 - pulse); // Fade out as it expands
                
                return finalColor;
            }
            ENDHLSL
        }
    }
}
