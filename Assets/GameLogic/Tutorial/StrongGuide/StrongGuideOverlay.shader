Shader "UI/StrongGuideOverlay"
{
    Properties
    {
        _Color ("Tint", Color) = (0,0,0,0.75)

        _UseHole1 ("Use Hole 1", Float) = 1
        _Hole1Shape ("Hole 1 Shape", Float) = 0
        _Hole1Center ("Hole 1 Center", Vector) = (0.5, 0.5, 0, 0)
        _Hole1Size ("Hole 1 Size", Vector) = (0.1, 0.1, 0, 0)

        _UseHole2 ("Use Hole 2", Float) = 0
        _Hole2Shape ("Hole 2 Shape", Float) = 0
        _Hole2Center ("Hole 2 Center", Vector) = (0.5, 0.5, 0, 0)
        _Hole2Size ("Hole 2 Size", Vector) = (0.1, 0.1, 0, 0)

        _EdgeSoftness ("Edge Softness", Float) = 0.01
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 uv       : TEXCOORD0;
            };

            fixed4 _Color;

            float _UseHole1;
            float _Hole1Shape;
            float4 _Hole1Center;
            float4 _Hole1Size;

            float _UseHole2;
            float _Hole2Shape;
            float4 _Hole2Center;
            float4 _Hole2Size;

            float _EdgeSoftness;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            float RectMask(float2 uv, float2 center, float2 halfSize, float softness)
            {
                float2 d = abs(uv - center) - halfSize;
                float outside = length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
                return smoothstep(0.0, softness, outside);
            }

            float CircleMask(float2 uv, float2 center, float2 radiusXY, float softness)
            {
                float2 delta = (uv - center) / max(radiusXY, float2(0.0001, 0.0001));
                float dist = length(delta);
                float outside = dist - 1.0;
                return smoothstep(0.0, softness, outside);
            }

            float HoleMask(float2 uv, float shape, float2 center, float2 sizeXY, float softness)
            {
                // shape: 0 = rectangle, 1 = circle/ellipse
                if (shape < 0.5)
                {
                    return RectMask(uv, center, sizeXY * 0.5, softness);
                }
                else
                {
                    return CircleMask(uv, center, sizeXY * 0.5, softness);
                }
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float alpha = i.color.a;

                if (_UseHole1 > 0.5)
                {
                    float m1 = HoleMask(i.uv, _Hole1Shape, _Hole1Center.xy, _Hole1Size.xy, _EdgeSoftness);
                    alpha *= m1;
                }

                if (_UseHole2 > 0.5)
                {
                    float m2 = HoleMask(i.uv, _Hole2Shape, _Hole2Center.xy, _Hole2Size.xy, _EdgeSoftness);
                    alpha *= m2;
                }

                return fixed4(i.color.rgb, alpha);
            }
            ENDCG
        }
    }
}