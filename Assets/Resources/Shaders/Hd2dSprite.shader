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
            ZWrite Off
            Stencil
            {
                Ref 2
                Comp always
                Pass replace
            }

            CGPROGRAM
            sampler2D _MainTex;
            #pragma vertex vert_img  
            #pragma fragment frag  
            #include "UnityCG.cginc"  

            fixed4 frag(v2f_img i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv);
            // 不要な透明ピクセルを破棄
            if (c.a - 0.1 < 0) {
                discard;
        }
                return c;
            }
            ENDCG
        }

        Pass
        {
            //ZWrite On
            Tags { "LightMode" = "ForwardBase" }

            Stencil
            {
                Ref 2
                Comp equal
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
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
                float4 pos : SV_POSITION; // posに変更する！！TRANSFER_SHADOWがposとうい名前でないと受け付けない。
                SHADOW_COORDS(1)
            };

            sampler2D _CameraDepthTexture;
            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert(appdata v)
            {
                v2f o;
                // ここの左辺もposに変更
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                half NdotL = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                o.diff = NdotL * _LightColor0;
                o.diff.rgb += ShadeSH9(half4(worldNormal, 1));
                TRANSFER_SHADOW(o)

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed depth = UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture,i.uv));
                fixed4 col = tex2D(_MainTex, i.uv);
                //col = fixed4(depth,depth,depth,1);
                // 影を計算
                fixed4 shadow = SHADOW_ATTENUATION(i);
                //col *= shadow;
                col *= i.diff;
                if (col.a - 0.1 < 0) {
                    discard;
                }
                return col;
            }
            ENDCG
        }

        Pass
        {

                /*
                Stencil
                {
                    Ref 2
                    Comp equal
                }
                */
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
                float2 uv : TEXCOORD0;
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
