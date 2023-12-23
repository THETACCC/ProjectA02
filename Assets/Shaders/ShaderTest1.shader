Shader "Unlit/ShaderTest1"
{
    Properties 
    {
        _albedo ("albedo", 2D) = "white" {}
        _albedo2 ("albedo", 2D) = "white" {}
        [NoScaleOffset] _normalMap ("normal map", 2D) = "bump" {}
        [NoScaleOffset] _normalMap2 ("normal map", 2D) = "bump" {}
        [NoScaleOffset] _normalMap3 ("normal map", 2D) = "bump" {}
        [NoScaleOffset] _normalMap4 ("normal map", 2D) = "bump" {}
        [NoScaleOffset] _displacementMap ("displacement map", 2D) = "white" {}
        [NoScaleOffset] _displacementMap2 ("displacement map", 2D) = "white" {}
        _gloss ("gloss", Range(0,1)) = 1
        _normalIntensity ("normal intensity", Range(0, 1)) = 1
        _displacementIntensity ("displacement intensity", Range(0,1)) = 0.5
        _refractionIntensity ("refraction intensity", Range(0, 0.5)) = 0.1
        _opacity ("opacity", Range(0,1)) = 0.9


        _scale ("noise scale", Range(2, 100)) = 15.5
        _displacement ("displacement", Range(0, 0.1)) = 0.05



        _diffuseLightSteps ("diffuse light steps", Int) = 4
        _specularLightSteps ("specular light steps", Int) = 2



        _scale ("noise scale", Range(2, 100)) = 15.5
        _surfaceIntersectionSize ("surface intersection size", Range(0, 1)) = 0.1
        _depthFog ("depth fog", Range(0, 2)) = 0.1

        _stencilRef ("stencil reference", Int) = 1
    }
    SubShader
    {
        // this tag is required to use _LightColor0
        // this shader won't actually use transparency, but we want it to render with the transparent objects
        Tags {  "RenderType"="Opaque" "Queue"="Geometry+1" "IgnoreProjector"="True" "LightMode"="ForwardBase" }

        GrabPass {
            "_BackgroundTex"
        }
        /*
        Stencil
        {
            Ref [_stencilRef]
            
            // reference comp stencil buffer
            Comp Equal
        }
        */
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc" // might be UnityLightingCommon.cginc for later versions of unity

            #define MAX_SPECULAR_POWER 256

            // get depth texture and background grab pass texture
            sampler2D _CameraDepthTexture;
            sampler2D _BackgroundTex;

            sampler2D _albedo; float4 _albedo_ST;
            sampler2D _albedo2;
            sampler2D _normalMap;
            sampler2D _normalMap2;
            sampler2D _normalMap3;
            sampler2D _normalMap4;
            sampler2D _displacementMap;
            sampler2D _displacementMap2;


            float _gloss;
            float _normalIntensity;
            float _displacementIntensity;
            float _refractionIntensity;
            float _opacity;

            float _surfaceIntersectionSize;
            float _depthFog;

            float _scale;
            float _displacement;

            int _diffuseLightSteps;
            int _specularLightSteps;


            float wave (float2 uv) {
                float wave1 = sin(((uv.x + uv.y) * _scale) + _Time.z) * 0.5 + 0.5;
                float wave2 = (cos(((uv.x - uv.y) * _scale/2.568) + _Time.z) + 1) * sin(_Time.x * 5.2321 + (uv.x * uv.y)) * 0.5 + 0.5;
                return (wave1 + wave2) / 3;
            }
            struct MeshData
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD0;
                float surfZ : TEXCOORD1;
            };

            struct Interpolators
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float3 tangent : TEXCOORD2;
                float3 bitangent : TEXCOORD3;
                float3 posWorld : TEXCOORD4;
                
                // create a variable to hold two float2 direction vectors that we'll use to pan our textures
                float4 uvPan : TEXCOORD5;
                float4 uvPan2 : TEXCOORD9;
                float4 uvPan3 : TEXCOORD10;
                float4 uvPan4 : TEXCOORD11;
                float4 screenUV : TEXCOORD6;

                float disp : TEXCOORD7;
                //screen pos and surfZ
                float surfZ : TEXCOORD8;
                float2 worldUV : TEXCOORD12;
                //float4 screenPos : TEXCOORD9;
            };

            Interpolators vert (MeshData v)
            {
                Interpolators o;
                o.worldUV = mul(unity_ObjectToWorld, v.vertex).xz * 0.2;
                o.disp = sin(((v.uv.x - v.uv.y) * _scale) + _Time.z) * 0.5 + 0.5;
                o.worldUV = o.worldUV * o.disp;
                
                //o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.uv = TRANSFORM_TEX(v.uv, _albedo);
           
                // panning
                o.uvPan = float4(float2(0.9, 0.2) * _Time.x * 2, float2(0.5, -0.2) * _Time.z);
                o.uvPan2 = float4(float2(0.01, 0.9) * _Time.x * 1.5, float2(0.01, 0.9) * _Time.z);
                o.uvPan3 = float4(float2(0.01, 0.9) * _Time.x * 0.5, float2(0.01, 0.9) * _Time.z);
                o.uvPan4 = float4(float2(0.01, 0.9) * _Time.x * 3, float2(0.01, 0.9) * _Time.z);
                // add our panning to our displacement texture sample
                float height1 = tex2Dlod(_displacementMap, float4(o.uv + o.uvPan.xy, 0, 0)).r;
                float height2 = tex2Dlod(_displacementMap2, float4(o.uv + o.uvPan3.xy, 0, 0)).r;

                float height = height1 * height2;
                v.vertex.xyz += v.normal * height * _displacementIntensity * o.disp;
                v.vertex.xyz += v.normal * o.disp * _displacement;
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.tangent = UnityObjectToWorldNormal(v.tangent);
                o.bitangent = cross(o.normal, o.tangent) * v.tangent.w;

                o.vertex = UnityObjectToClipPos(v.vertex);
                
                o.screenUV = ComputeGrabScreenPos(o.vertex);
                
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                

                // screenPos and surfZ
                //o.screenPos = ComputeGrabScreenPos(o.vertex);
                o.surfZ = -UnityObjectToViewPos(v.vertex).z;
                return o;
            }

            float4 frag (Interpolators i) : SV_Target
            {
                float w = i.disp * 0.5;
                float2 uv = i.uv;
                float2 screenUV = i.screenUV.xy / i.screenUV.w;
                float rand = 0;
                    float3 colorEdge = (0,0,0);





                float3 tangentSpaceNormal1 = UnpackNormal(tex2D(_normalMap, uv + i.uvPan.xy));
                tangentSpaceNormal1 += float3 (0.5 , 0,0);
                float3 tangentSpaceNormal2 = UnpackNormal(tex2D(_normalMap2, uv + i.uvPan2.xy));
                tangentSpaceNormal2 += float3 (0.5 , 0,0);
                float3 tangentSpaceNormal3 = UnpackNormal(tex2D(_normalMap3, uv + i.uvPan3.xy));
                tangentSpaceNormal3 += float3 (0.5 , 0,0);
                float3 tangentSpaceNormal4 = UnpackNormal(tex2D(_normalMap4, uv + i.uvPan4.xy));
                tangentSpaceNormal4 += float3 (0.5 , 0,0);
                float3 tangentSpaceNormal = tangentSpaceNormal1 * tangentSpaceNormal2 * tangentSpaceNormal3 * tangentSpaceNormal4;


                //rand = frac(sin( tangentSpaceNormal) * 90321 * (sin(_Time.z) * 2 + 1));
                //tangentSpaceNormal = rand * tangentSpaceNormal;
                tangentSpaceNormal = normalize(lerp(float3(0, 0, 1), tangentSpaceNormal, _normalIntensity));
                



                float2 refractionUV = screenUV.xy + (tangentSpaceNormal.xy * _refractionIntensity);


                float3x3 tangentToWorld = float3x3 
                (
                    i.tangent.x, i.bitangent.x, i.normal.x,
                    i.tangent.y, i.bitangent.y, i.normal.y,
                    i.tangent.z, i.bitangent.z, i.normal.z
                );

                float3 normal = mul(tangentToWorld, tangentSpaceNormal);


                // blinn phong

                float3 surfaceColor = tex2D(_albedo, uv + i.uvPan.xy).rgb + tex2D(_albedo2, uv + i.uvPan2.xy).rgb * 0.25;

                float3 lightDirection = _WorldSpaceLightPos0;
                float3 lightColor = _LightColor0; // includes intensity

                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld);
                float3 halfDirection = normalize(viewDirection + lightDirection);

                float diffuseFalloff = max(0, dot(normal, lightDirection));
                float specularFalloff = max(0, dot(normal, halfDirection));


                float3 specular = pow(specularFalloff, _gloss * MAX_SPECULAR_POWER + 0.0001) * _gloss * lightColor;
                specular += float3(0,0,0);
                float3 diffuse = diffuseFalloff * surfaceColor * lightColor;


                diffuse = ceil(diffuse * _diffuseLightSteps) / _diffuseLightSteps;
                specular = floor(specular * _specularLightSteps) / _specularLightSteps ;
   
                
                // calculate screenUV coordinates
                // depth intersection
                float depth = Linear01Depth(tex2D(_CameraDepthTexture, screenUV)).r;
                float depthDifference = abs((depth / _ProjectionParams.w) - i.surfZ);

                float intersection = 1 - smoothstep(0, _surfaceIntersectionSize, depthDifference);

                intersection *= saturate(saturate(sin(2 * _Time.z + 16 * (1-intersection))) + smoothstep(0.95, 1, intersection));
               

                // refraction effect
                float2 distortedScreenUV = screenUV + (float2(0.1, 0.4) * (w * saturate(i.normal.y) * _refractionIntensity * 2));

                float3 background = tex2D(_BackgroundTex, distortedScreenUV);

                float distortedDepth = Linear01Depth(tex2D(_CameraDepthTexture, distortedScreenUV)).r;
                float distortedDepthDifference = abs((distortedDepth / _ProjectionParams.w) - i.surfZ);
                float underwaterDepth = 0.85 - smoothstep(0, _depthFog, distortedDepthDifference);
                background = background * underwaterDepth;
                float3 color = (diffuse * _opacity ) + (background * (1 - _opacity)) + specular;
                color = saturate(color + background * 0.5 * (1 - _opacity));

                color += smoothstep(0.1, 1, intersection) * 0.5;
                return float4(color, 1);
            }
            ENDCG
        }
    }
}
