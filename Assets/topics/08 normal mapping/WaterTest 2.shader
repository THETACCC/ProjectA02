Shader "Unlit/WaterTest 2"
{
    Properties 
    {
        _albedo ("albedo", 2D) = "white" {}
        [NoScaleOffset] _normalMap ("normal map", 2D) = "bump" {}
        [NoScaleOffset] _displacementMap ("displacement map", 2D) = "white" {}
        _gloss ("gloss", Range(0,1)) = 1
        _normalIntensity ("normal intensity", Range(0, 1)) = 1
        _displacementIntensity ("displacement intensity", Range(0,1)) = 0.5
        _refractionIntensity ("refraction intensity", Range(0, 0.5)) = 0.1
        _opacity ("opacity", Range(0,1)) = 0.9
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fragment _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _MIXED_LIGHTING_SUBTRACTIVE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct Attributes
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float3 tangent : TEXCOORD2;
                float3 bitangent : TEXCOORD3;
                float3 posWorld : TEXCOORD4;
                float4 uvPan : TEXCOORD5;
                float4 screenUV : TEXCOORD6;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.uv = TRANSFORM_TEX(IN.uv, _albedo);
                OUT.uvPan = float4(float2(0.9, 0.2) * _Time.x, float2(0.5, -0.2) * _Time.x);

                float height = tex2Dlod(_displacementMap, float4(OUT.uv + OUT.uvPan.xy, 0, 0)).r;
                IN.vertex.xyz += IN.normal * height * _displacementIntensity;

                OUT.normal = UnityObjectToWorldNormal(IN.normal);
                OUT.tangent = UnityObjectToWorldDir(IN.tangent.xyz);
                OUT.bitangent = cross(OUT.normal, OUT.tangent) * IN.tangent.w;

                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.screenUV = ComputeGrabScreenPos(OUT.vertex);
                OUT.posWorld = mul(unity_ObjectToWorld, IN.vertex);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
                float2 uv = IN.uv;
                float2 screenUV = IN.screenUV.xy / IN.screenUV.w;

                float3 tangentSpaceNormal = UnpackNormal(tex2D(_normalMap, uv + IN.uvPan.xy));
                tangentSpaceNormal = normalize(lerp(float3(0, 0, 1), tangentSpaceNormal, _normalIntensity));

                float2 refractionUV = screenUV.xy + (tangentSpaceNormal.xy * _refractionIntensity);
                float3 background = tex2D(_BackgroundTex, refractionUV);

                float3x3 tangentToWorld = float3x3(IN.tangent, IN.bitangent, IN.normal);
                float3 normal = mul(tangentToWorld, tangentSpaceNormal);

                // Lighting calculations here
                float3 albedo = tex2D(_albedo, uv + IN.uvPan.xy).rgb;
                float3 lighting = UniversalFragmentPBR(IN.posWorld, normal, albedo, _gloss);

                float3 color = (lighting * _opacity) + (background * (1 - _opacity));

                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}