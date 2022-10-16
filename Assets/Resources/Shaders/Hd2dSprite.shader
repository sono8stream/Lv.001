Shader "Custom/Hd2dSprite"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Pass
        {
            Tags { "LightMode" = "ForwardBase" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            #include "UnityCG.cginc" 
            #include "UnityLightingCommon.cginc"
            #include "Lighting.cginc" 
            #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                fixed4 diff : COLOR0;
                float4 pos : SV_POSITION; // pos固定。TRANSFER_SHADOWがposでないと受け付けない。
                SHADOW_COORDS(1)
                UNITY_FOG_COORDS(2)// shadowCoordとsemanticsが合わないよう2を指定
            };

            sampler2D _CameraDepthTexture;
            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert(appdata v)
            {
                v2f o;
                // オブジェクト座標を2次元座標に割り当て
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                half NdotL = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                NdotL = saturate(dot(worldNormal, _WorldSpaceLightPos0.xyz));
                float diffuse = 0.2;
                o.diff = NdotL * _LightColor0 * diffuse + (1 - diffuse);
                o.diff.rgb += ShadeSH9(half4(worldNormal, 1));
                TRANSFER_SHADOW(o)
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                UNITY_TRANSFER_FOG(o, o.pos);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
            // 透明部分は次の描画命令を実行しない
           
                if (col.a - 0.1 < 0) {
                    discard;
                }

                // 影を計算
                fixed4 shadow = SHADOW_ATTENUATION(i);
                col *= saturate(shadow);
                col *= saturate(i.diff);

                // Fogを計算
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
        
        Pass
        {
            // 距離情報を付与して描画
            ZWrite On
            Tags{ "LightMode" = "ShadowCaster" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord : TEXCOORD0;
            };

            struct shadowcaster {
                V2F_SHADOW_CASTER;
            };

            struct v2f
            {
                float2 uv : TEXCOORD1;
                shadowcaster shad;
            };

            sampler2D _MainTex;

            v2f vert(appdata v)
            {
                v2f o;
                o.uv = v.texcoord;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o.shad)
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                if (col.a - 0.1 < 0) {
                    discard;
                }
                SHADOW_CASTER_FRAGMENT(i.shad)
            }
            ENDCG
        }
    }
}
