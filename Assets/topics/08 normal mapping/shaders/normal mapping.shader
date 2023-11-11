Shader "examples/week 8/normal mapping"
{
    Properties 
    {
        _albedo ("albedo", 2D) = "white" {}
        [NoScaleOffset] _normalMap ("normal map", 2D) = "bump" {}
        _gloss ("gloss", Range(0,1)) = 1
        _normalIntensity ("normal intensity", Range(0,1)) = 1
    }
    SubShader
    {
        // this tag is required to use _LightColor0
        Tags { "LightMode"="ForwardBase" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // might be UnityLightingCommon.cginc for later versions of unity
            #include "Lighting.cginc"

            #define MAX_SPECULAR_POWER 256

            sampler2D _albedo; float4 _albedo_ST;
            sampler2D _normalMap;
            float _gloss;
            float _normalIntensity;

            struct MeshData
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT; 
                float2 uv : TEXCOORD0;
            };

            struct Interpolators
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float3 tangent : TEXCOORD2;
                float3 bitangent : TEXCOORD3;
                float3 worldPos : TEXCOORD4;
            };

            Interpolators vert (MeshData v)
            {
                Interpolators o;
                
                // using transform tex in vert shader
                o.uv = TRANSFORM_TEX(v.uv, _albedo);
                
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.tangent = UnityObjectToWorldNormal(v.tangent);
                o.bitangent = cross(o.normal, o.tangent) * v.tangent.w;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);

                return o;
            }

            float3 blinnphong (float2 uv, float3 normal, float3 worldPos) {
                float3 surfaceColor = tex2D(_albedo, uv).rgb;

                float3 lightDirection = _WorldSpaceLightPos0;
                float3 lightColor = _LightColor0; // includes intensity

                // blinn-phong
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - worldPos);
                float3 halfDirection = normalize(viewDirection + lightDirection);

                float diffuseFalloff = max(0, dot(normal, lightDirection));
                float specularFalloff = max(0, dot(normal, halfDirection));

                float3 diffuse = diffuseFalloff * surfaceColor * lightColor;

                // the specular power, which controls the sharpness of the direct specular light is dependent on the glossiness (smoothness)
                float3 specular = pow(specularFalloff, _gloss * MAX_SPECULAR_POWER + 0.0001) * _gloss * lightColor;

                return diffuse + specular;
            }

            float4 frag (Interpolators i) : SV_Target
            {
                float2 uv = i.uv;
                float3 color = 0;

                float3 tangent = normalize(i.tangent);
                float3 bitangent = normalize(i.bitangent);
                float3 normal = normalize(i.normal);

                
                float3 tangentSpaceNormal = UnpackNormal(tex2D(_normalMap, uv));

                tangentSpaceNormal = normalize(lerp(float3(0,0,1), tangentSpaceNormal, _normalIntensity));

                
                float3x3 tangentToWorld = float3x3
                (
                    tangent.x, bitangent.x, normal.x,
                    tangent.y, bitangent.y, normal.y,
                    tangent.z, bitangent.z, normal.z
                );

                float3 sampledNormal = mul(tangentToWorld, tangentSpaceNormal);
                
                color = blinnphong(uv, sampledNormal, i.worldPos);
                return float4(color, 1.0);
            }
            ENDCG
        }
    }
}
