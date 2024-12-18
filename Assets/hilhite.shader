Shader "Custom/FrontBackOutlineAura"
{
    Properties
    {
        _FrontTex ("Front Texture", 2D) = "white" {}  // 前面用のテクスチャ
        _BackTex ("Back Texture", 2D) = "white" {}   // 背面用のテクスチャ
        _OtherColor ("Other Color", Color) = (1, 0, 0, 1) // 赤色

        _OutlineColor ("Outline Color", Color) = (1, 1, 0, 1) // アウトラインの色
        _OutlineWidth ("Outline Width", Range(0.0, 0.09)) = 0.01

        _AuraColor ("Aura Color", Color) = (0.5, 0.5, 1, 0.5) // オーラの色（半透明）
        _AuraSize ("Aura Size", Range(1.0, 1.5)) = 1.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        // ------------------
        // Pass 1: アウトライン描画
        // ------------------
        Pass
        {
            Name "OUTLINE"
            Cull Front
            ZWrite On
            ColorMask RGB

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            float _OutlineWidth;
            fixed4 _OutlineColor;

            v2f vert (appdata_t v)
            {
                v2f o;
                // 法線方向に拡大してアウトラインを描画
                v.vertex.xyz += v.normal * _OutlineWidth;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _OutlineColor;
            }
            ENDCG
        }

        // ------------------
        // Pass 2: 通常の面の描画
        // ------------------
        Pass
        {
            Name "MAIN"
            Cull Back

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _FrontTex;
            sampler2D _BackTex;
            fixed4 _OtherColor;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 normal : TEXCOORD1;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.normal = normalize(v.normal);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                if (abs(i.normal.z) > 0.9)
                {
                    if (i.normal.z > 0.0)
                        return tex2D(_FrontTex, i.uv);
                    else
                        return tex2D(_BackTex, float2(i.uv.x, 1.0 - i.uv.y));
                }
                else
                {
                    return _OtherColor;
                }
            }
            ENDCG
        }

        // ------------------
        // Pass 3: オーラ描画
        // ------------------
        Pass
        {
            Name "AURA"
            Cull Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            float _AuraSize;
            fixed4 _AuraColor;

            v2f vert (appdata_t v)
            {
                v2f o;
                v.vertex.xyz *= _AuraSize; // オーラサイズで拡大
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _AuraColor; // 半透明のオーラ色
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
