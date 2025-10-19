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

        // HDR glow controls
        _GlowColor     ("Glow Color (HDR)", Color) = (1,0.85,0,1)
        _GlowIntensity ("Glow Intensity (HDR)", Range(0,10)) = 2.5
        _GlowWidth     ("Glow Width (diamond units)", Range(0.0,0.6)) = 0.18
        _GlowFalloff   ("Glow Falloff", Range(0.5,8)) = 2.5
        _GlowOnlyWhenFilled ("Glow Only On Filled", Float) = 1.0
        _GlowFollowStripes  ("Glow Follows Stripes", Float) = 0

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
            float4 _Color;

            // Use half/float so we can output HDR > 1.0
            half4 _FillColor, _EmptyColor, _Background;

            float _Progress, _Thickness, _Softness, _Aspect;
            float _StripeDensity, _FlowSpeed, _StripeAlpha, _ShowFlow;

            half4 _GlowColor;
            float _GlowIntensity, _GlowWidth, _GlowFalloff, _GlowOnlyWhenFilled, _GlowFollowStripes;

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

            half4 frag (v2f i) : SV_Target
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
                    half4 bg = _Background;
                    return bg;
                }

                // Progress along edge
                float t = perimeter_t(p);          // 0..1 along edge, CCW from top
                float filled = step(t, _Progress); // 1 if within progress

                half4 edgeCol = lerp(_EmptyColor, _FillColor, filled);

                // Prepare a flow mask (always defined)
                float flowMask = 1.0;

                // Flow stripes on filled region (visual only)
                if (_ShowFlow > 0.5 && filled > 0.5)
                {
                    float stripes = 0.5 + 0.5 * cos(6.2831853 * (t * _StripeDensity - _Time.y * _FlowSpeed));
                    flowMask = saturate(stripes) * _StripeAlpha;
                    edgeCol.rgb = lerp(edgeCol.rgb, half3(1.0,1.0,1.0), flowMask);
                }

                // Glow distance from the ring centerline (diamond metric)
                float rOuter = 1.0;
                float rInner = 1.0 - _Thickness;
                float rCenter = 0.5 * (rOuter + rInner);
                float distFromCenter = abs(sum - rCenter);

                // Halo shape
                float g = 1.0 - saturate(distFromCenter / max(_GlowWidth, 1e-4));
                g = pow(g, _GlowFalloff);

                // Gate glow to filled arc
                if (_GlowOnlyWhenFilled > 0.5)
                {
                    g *= filled;
                }

                // (Optional) make glow follow stripe pulsation
                if (_GlowFollowStripes > 0.5)
                {
                    g *= flowMask;
                }

                // Additive HDR emission (picked up by Bloom)
                float3 glowRGB = (float3)_GlowColor.rgb * _GlowIntensity;
                float3 rgb = (float3)edgeCol.rgb;
                rgb += glowRGB * g;

                // Alpha from band + tiny boost from glow for nicer edges
                float a = edgeCol.a * band;
                a = max(a, band * 0.02 * g);

                return half4(rgb, a);
            }
            ENDCG
        }
    }
}
