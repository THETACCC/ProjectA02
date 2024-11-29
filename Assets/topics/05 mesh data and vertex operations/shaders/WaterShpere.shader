Shader "Custom/WaterShpere"
{
    Properties
    {
        _radius ("radius", Float) = 5
        _morph ("morph", Range(0,1)) = 0
        _color ("color", Color) = (0, 0, 0.8, 1)
        _scale ("noise scale", Range(2, 100)) = 15.5
        _displacement ("displacement", Range(0, 0.3)) = 0.05
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _radius;
            float _morph;
            float3 _color;
            float _scale;
            float _displacement;


            struct MeshData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float3 color : COLOR;
            };

            struct Interpolators
            {
                float4 vertex : SV_POSITION;
                float4 objPos : TEXCOORD3;
                float3 normal : TEXCOORD0;
                float2 worldUV : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float dispAmount : TEXCOORD4;
            };

            float wave (float2 uv) {
                // simple sin wave 0-1 with scale adjustment and time animation
                float wave1 = sin(((uv.y) * _scale) + _Time.z) * 0.5 + 0.5;

                // using cos and sin with different uv relationships and time and scale modifiers. 0-2 range
                //float wave2 = (cos((( uv.y) * _scale/2.568) + _Time.z) + 1) * sin(_Time.x * 5.2321 + (uv.y)) * 0.5 + 0.5;
                
                // dividing by 3 to make 0-1 range
                return (wave1) / 3;
            }


            Interpolators vert (MeshData v)
            {
                Interpolators o;
                o.uv = v.uv;
                o.worldUV = mul(unity_ObjectToWorld, v.vertex).xy * 0.02;

                float4 oPos = o.objPos = v.vertex;
                oPos.y -=.5;
                float dispAmount = pow((sin(oPos.y * 20+_Time.y * 4)+1), 3);
                float3 disp = v.vertex.xyz + v.normal * dispAmount *.02;
                float lerpAmount = (0.5-abs(o.objPos.y)) * 2;

                v.vertex.xyz = lerp(v.vertex.xyz, disp, lerpAmount);

                o.dispAmount = dispAmount;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = v.normal;

                return o;
            }

            float4 frag (Interpolators i) : SV_Target
            {
                //float dispAmount = pow(i.dispAmount, .5);
                //dispAmount = smoothstep (.1,1.8,dispAmount);


                //return float4(dispAmount,0,0, 1.0);

                float dispAmount = pow(i.dispAmount, .5);
                dispAmount = smoothstep(.1, 1.8, dispAmount);

                // Apply the color to the output
                float3 finalColor = _color.rgb;

                return float4(finalColor, 1.0); // Use alpha from _color
            }
            ENDCG
        }
    }
}
