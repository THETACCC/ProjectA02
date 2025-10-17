Shader "UI/DiamondProgressBar"
{
    Properties
    {
        _FillColor     ("Fill Color", Color) = (1,0.85,0,1)
        _EmptyColor    ("Empty Edge Color", Color) = (0.22,0.22,0.22,0.85)
        _Background    ("Background Color", Color) = (0,0,0,0)

        _Progress      ("Progress (0..1)", Range(0,1)) = 0
        _Thickness     ("Ring Thickness", Range(0.001,0.5)) = 0.12
        _Softness      ("Edge Softness", Range(0,0.05)) = 0.002
        _Aspect        ("Rect Aspect (w/h)", Float) = 1.0

        _StripeDensity ("Stripe Density", Range(1,50)) = 12
        _FlowSpeed     ("Flow Speed (CCW+)", Float) = 0.6
        _StripeAlpha   ("Stripe Alpha", Range(0,1)) = 0.6
        _ShowFlow      ("Show Flow", Float) = 1.0

        _MainTex       ("Sprite", 2D) = "white" {}
        _Color         ("Tint", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "CanUseSpriteAtlas"="True"
        }
        Stencil { Ref 0 Comp Always Pass Keep Fail Keep ZFail Keep }
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 texcoord1: TEXCOORD1; // mask/clip
            };

            struct v2f {
                float4 vertex   : SV_POSITION;
                float4 color    : COLOR;
                float2 uv       : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                float2 texcoord1: TEXCOORD2; // mask/clip
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            fixed4 _FillColor, _EmptyColor, _Background;

            float _Progress, _Thickness, _Softness, _Aspect;
            float _StripeDensity, _FlowSpeed, _StripeAlpha, _ShowFlow;

            float4 _ClipRect;

            v2f vert (appdata_t IN)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(IN.vertex);
                o.uv = TRANSFORM_TEX(IN.texcoord, _MainTex);
                o.color = IN.color * _Color;
                o.worldPos = IN.vertex;
                o.texcoord1 = IN.texcoord1.xy;
                return o;
            }

            // Map a point to [0..1] around the diamond perimeter, starting at top and CCW
            float perimeter_t(float2 p)
            {
                float sum = abs(p.x) + abs(p.y);
                if (sum < 1e-5) return 0.0;
                float2 q = p / sum; // project to |x|+|y|=1

                float seg, s;
                if (q.y >= 0 && q.x <= 0)       { seg = 0.0; s = -q.x; } // top->left
                else if (q.x <= 0 && q.y <= 0)  { seg = 1.0; s = -q.y; } // left->bottom
                else if (q.y <= 0 && q.x >= 0)  { seg = 2.0; s =  q.x; } // bottom->right
                else                            { seg = 3.0; s =  q.y; } // right->top

                s = saturate(s);
                return (seg + s) * 0.25;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // UI clipping
                float2 localPos = i.worldPos.xy;
                float4 m = UnityGet2DClipping(localPos, _ClipRect);
                if (m.x < 0.5) discard;

                // Centered coords with aspect correction
                float2 uv = i.uv;
                float2 p;
                p.x = (uv.x - 0.5) * 2.0 * _Aspect;
                p.y = (uv.y - 0.5) * 2.0;

                // Diamond ring mask
                float sum = abs(p.x) + abs(p.y);
                float outer = 1.0 - smoothstep(1.0, 1.0 + _Softness, sum);
                float inner =  smoothstep(1.0 - _Thickness, 1.0 - _Thickness + _Softness, sum);
                float band  = outer * inner;

                if (band <= 0.0001)
                {
                    fixed4 bg = _Background;
                    return bg;
                }

                float t = perimeter_t(p);            // 0..1 along edge, CCW from top
                float filled = step(t, _Progress);   // 1 if within progress
                fixed4 edgeCol = lerp(_EmptyColor, _FillColor, filled);

                // Flow stripes on filled region
                if (_ShowFlow > 0.5 && filled > 0.5)
                {
                    float stripes = 0.5 + 0.5 * cos(6.2831853 * (t * _StripeDensity - _Time.y * _FlowSpeed));
                    float flowMask = saturate(stripes) * _StripeAlpha;
                    edgeCol.rgb = lerp(edgeCol.rgb, 1.0.xxx, flowMask);
                }

                edgeCol.a *= band;
                return edgeCol;
            }
            ENDCG
        }
    }
}
