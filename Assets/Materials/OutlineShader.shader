Shader "Custom/PixelOutline"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1, 0, 0, 1)
        _OutlineEnabled ("Enable Outline", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _OutlineColor;
            float _OutlineEnabled;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float4 col = tex2D(_MainTex, uv);

                // If outline is disabled, return base color
                if (_OutlineEnabled < 0.5)
                    return col;

                // If pixel is part of the sprite, return base color
                if (col.a > 0.01)
                    return col;

                // Check neighbor pixels for alpha > 0
                float2 offset = float2(_MainTex_TexelSize.x, _MainTex_TexelSize.y);
                float outline = 0.0;
                outline += tex2D(_MainTex, uv + float2(-offset.x, 0)).a;
                outline += tex2D(_MainTex, uv + float2( offset.x, 0)).a;
                outline += tex2D(_MainTex, uv + float2(0, offset.y)).a;
                outline += tex2D(_MainTex, uv + float2(0, -offset.y)).a;

                // If neighbor pixels are filled, draw outline
                if (outline > 0.01)
                    return _OutlineColor;

                return float4(0, 0, 0, 0); // fully transparent
            }
            ENDCG
        }
    }

    FallBack "Sprites/Default"
}
