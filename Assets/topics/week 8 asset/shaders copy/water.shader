Shader "examples/week 8/water"
{
    Properties 
    {
        _surfaceColor ("surface color", Color) = (0.4, 0.1, 0.9)
        _ReflectionColor ("Reflection Color", Color) = (1, 1, 1)
        _albedo ("albedo", 2D) = "white" {}
        _waterFormTexture ("water form texture", 2D) = "white" {}
        [NoScaleOffset] _normalMap ("normal map", 2D) = "bump" {}
        [NoScaleOffset] _normalMap2 ("normal map2", 2D) = "bump" {}
        [NoScaleOffset] _displacementMap ("displacement map", 2D) = "white" {}
        _gloss ("gloss", Range(0,1)) = 1
        _normalIntensity ("normal intensity", Range(0, 1)) = 1
        _normalIntensity2 ("normal intensity2", Range(0, 1)) = 1
        _normalIntensity3 ("normal intensity3", Range(0, 1)) = 1
        _displacementIntensity ("displacement intensity", Range(0,1)) = 0.5
        _refractionIntensity ("refraction intensity", Range(0, 0.5)) = 0.1
        _opacity ("opacity", Range(0,1)) = 0.9
        _diffuseLightSteps ("diffuse light steps", Int) = 4
        _specularLightSteps ("specular light steps", Int) = 2
        _Scale ("Scale", Range(0, 5)) = 0.5
        _FoamIntensityScale ("Foam Intensity Scale", Range(0, 10)) = 1
        _FoamColor ("Foam Color", Color) = (1, 1, 1, 1)
        _FoamColorIntensity ("Foam Color Intensity", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "LightMode"="ForwardBase" }

        GrabPass {
            "_BackgroundTex"
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            #define MAX_SPECULAR_POWER 256

            sampler2D _albedo; float4 _albedo_ST;
            sampler2D _normalMap;
            sampler2D _normalMap2;
            sampler2D _displacementMap;
            sampler2D _BackgroundTex;
            sampler2D _waterFormTexture;
            sampler2D _CameraDepthTexture;

            float3 _surfaceColor;
            float3 _ReflectionColor;
            float _gloss;
            float _normalIntensity;
            float _normalIntensity2;
            float _normalIntensity3;
            float _displacementIntensity;
            float _refractionIntensity;
            float _opacity;
            int _diffuseLightSteps;
            int _specularLightSteps;
            float _Scale; 
            float _FoamIntensityScale;
            float3 _FoamColor;
            float _FoamColorIntensity;

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
                float3 posWorld : TEXCOORD4;
                float4 uvPan : TEXCOORD5;
                float4 screenUV : TEXCOORD6;
            };

            Interpolators vert (MeshData v)
            {
                Interpolators o;
                o.uv = v.uv;
                o.uvPan = float4(float2(0.9, 0.2) * _Time.x, float2(0.5, -0.2) * _Time.x);

                float height = tex2Dlod(_displacementMap, float4(o.uv + o.uvPan.xy, 0, 0)).r;
                v.vertex.xyz += v.normal * height * _displacementIntensity;

                o.normal = UnityObjectToWorldNormal(v.normal);
                o.tangent = UnityObjectToWorldNormal(v.tangent);
                o.bitangent = cross(o.normal, o.tangent) * v.tangent.w;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenUV = ComputeGrabScreenPos(o.vertex);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                
                return o;
            }

            float4 frag (Interpolators i) : SV_Target
            {
                float2 uv = i.uv;
                float2 screenUV = i.screenUV.xy / i.screenUV.w;
                float3 color = 0; 
                
                float2 distortion = tex2D(_displacementMap, uv).rgb;
                float2 distortedUv = uv  + 0.75 * distortion; 
                float3 waterForm = tex2D(_waterFormTexture, (distortedUv -i.uvPan.x )* _Scale).rgb;
                
                float3 tangentSpaceNormal = UnpackNormal(tex2D(_normalMap, uv + i.uvPan.xy));
                tangentSpaceNormal = normalize(lerp(float3(0, 0, 1), tangentSpaceNormal, _normalIntensity));
                float tangentSpaceNormal2 = UnpackNormal(tex2D(_normalMap2, uv - i.uvPan.yx));
                tangentSpaceNormal2 = normalize(lerp(float3(0, 0, 1), tangentSpaceNormal2, _normalIntensity2));
                tangentSpaceNormal = BlendNormals(tangentSpaceNormal, tangentSpaceNormal2) * _normalIntensity3;

                float2 refractionUV = screenUV.xy + (tangentSpaceNormal.xy * _refractionIntensity);
                float3 background = tex2D(_BackgroundTex, refractionUV);

                float3x3 tangentToWorld = float3x3 
                (
                    i.tangent.x, i.bitangent.x, i.normal.x,
                    i.tangent.y, i.bitangent.y, i.normal.y,
                    i.tangent.z, i.bitangent.z, i.normal.z
                );
                float3 normal = mul(tangentToWorld, tangentSpaceNormal);

                float3 surfaceColor = _surfaceColor;

                float3 lightDirection = _WorldSpaceLightPos0;
                float3 lightColor = _LightColor0;

                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld);
                float3 halfDirection = normalize(viewDirection + lightDirection);

                float diffuseFalloff = max(0, dot(normal, lightDirection));
                float specularFalloff = max(0, dot(normal, halfDirection));

                // diffuseFalloff = ceil(diffuseFalloff * _diffuseLightSteps) / _diffuseLightSteps;
                // specularFalloff = floor(specularFalloff * _specularLightSteps) / _specularLightSteps;

                float3 specular = pow(specularFalloff, _gloss * MAX_SPECULAR_POWER + 0.0001) * _gloss * lightColor;
                float3 diffuse = diffuseFalloff * surfaceColor * lightColor;
            
                float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.screenUV);
                depth = LinearEyeDepth(depth);
                float depthDiff = depth - SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.screenUV + float2(0.01, 0.01));
                depthDiff = abs(depthDiff);

                float foamIntensity = saturate(depthDiff * _FoamIntensityScale);
                float3 foamColor = lerp(_surfaceColor, _FoamColor, foamIntensity);

                color = (2 * diffuse) + (background * (1 - _opacity)) + specular;
                color += (_ReflectionColor * _opacity *(1-diffuseFalloff));
                color += 0.55*(waterForm);

                return float4(color, 1);
            }
            ENDCG
        }
    }
}