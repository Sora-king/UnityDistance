Shader "Custom/FrontBackAndOthers"
{
    Properties
    {
        _FrontTex ("Front Texture", 2D) = "white" {}  // 前面用のテクスチャ
        _BackTex ("Back Texture", 2D) = "white" {}   // 背面用のテクスチャ
        _OtherColor ("Other Color", Color) = (1, 0, 0, 1) // 赤色
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _FrontTex;    // 前面用のテクスチャ
            sampler2D _BackTex;     // 背面用のテクスチャ
            fixed4 _OtherColor;     // 他の面の色

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
                o.normal = normalize(v.normal);  // 法線を正規化
                o.uv = v.uv;                     // UV座標を渡す
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Z軸の判定（前面: Z > 0, 背面: Z < 0, 他の面はそれ以外）
                if (abs(i.normal.z) > 0.9) // 前面・背面の判定
                {
                    if (i.normal.z > 0.0)
                    {
                        // 正面
                        return tex2D(_FrontTex, i.uv); 
                    }
                    else
                    {
                        // 背面（上下反転して正しく表示）
                        return tex2D(_BackTex, float2(i.uv.x, 1.0 - i.uv.y));
                    }
                }
                else
                {
                    // 正面・背面以外を赤く塗る
                    return _OtherColor;
                }
            }
            ENDCG
        }
    }
}
