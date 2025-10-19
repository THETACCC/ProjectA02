Shader "UI/GlowSprite"
{
    Properties
    {
        _MainTex      ("Sprite (Alpha as Mask)", 2D) = "white" {}
        _BaseTint     ("Base Tint (RGB from script)", Color) = (1,1,1,1)
        _GlowTint     ("Glow Tint (HDR)", Color) = (1,1,1,1)
        _GlowIntensity("Glow Intensity (HDR)", Range(0,10)) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Cull Off Lighting Off ZWrite Off ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
            struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; float4 vcol:COLOR; };

            sampler2D _MainTex; float4 _MainTex_ST;
            float4 _BaseTint;
            float4 _GlowTint;
            float  _GlowIntensity;

            v2f vert (appdata v){
                v2f o; o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv,_MainTex);
                o.vcol = v.color; // Image.color 乘进来
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 只用贴图 alpha 做遮罩
                fixed a = tex2D(_MainTex, i.uv).a * _BaseTint.a * i.vcol.a;

                // 屏幕底色 = 你设的颜色（Image.color * _BaseTint）
                fixed3 baseRGB = (i.vcol.rgb * _BaseTint.rgb);

                // 发光用加法叠加，不漂白底色
                fixed3 glowRGB = _GlowTint.rgb * _GlowIntensity;

                return fixed4(baseRGB + glowRGB, a);
            }
            ENDCG
        }
    }
}
