Shader "Custom/ClothSmoothFade"
{
    Properties
    {
        [Header(Texture)]
        _MainTex("Main Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)

        [Header(Fade Animation)]
        _Fade("Fade", Range(0,1)) = 1.0

        [Header(Lighting)]
        _ShadowColor("Shadow Color", Color) = (0.65,0.5,0.35,1)
        _ShadowStrength("Shadow Strength", Range(0,1)) = 0.35
        _LightSmoothness("Light Smoothness", Range(0.01,1)) = 0.6
        _AmbientStrength("Ambient Strength", Range(0,1)) = 0.25

        [Header(Highlight)]
        _HighlightColor("Highlight Color", Color) = (1,1,1,1)
        _HighlightStrength("Highlight Strength", Range(0,1)) = 0.08
        _HighlightPower("Highlight Power", Range(1,128)) = 24
    }

        SubShader
        {
            Tags
            {
                "Queue" = "Transparent"
                "RenderType" = "Transparent"
                "IgnoreProjector" = "True"
            }

            Cull Off


            // =====================================================
            // PASS 1
            // Depth Prepass
            //
            // 先把桌布最前面的表面写入深度
            // 减少同一个复杂布料Mesh内部的透明排序错误
            // =====================================================

            Pass
            {
                Name "DEPTH PREPASS"

                ZWrite On
                ZTest LEqual
                ColorMask 0

                CGPROGRAM

                #pragma vertex vert
                #pragma fragment fragDepth

                #include "UnityCG.cginc"

                sampler2D _MainTex;
                float4 _MainTex_ST;

                fixed4 _Color;
                float _Fade;

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float4 pos : SV_POSITION;
                    float2 uv : TEXCOORD0;
                };

                v2f vert(appdata v)
                {
                    v2f o;

                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                    return o;
                }

                fixed4 fragDepth(v2f i) : SV_Target
                {
                    fixed4 tex = tex2D(_MainTex, i.uv);

                // 贴图本身如果有透明区域，则不写深度
                clip(tex.a* _Color.a - 0.001);

                // Fade = 0 时完全停止写深度
                clip(_Fade - 0.001);

                return 0;
            }

            ENDCG
        }


            // =====================================================
            // PASS 2
            // Smooth Transparent Color
            // =====================================================

            Pass
            {
                Name "FORWARD"

                Tags
                {
                    "LightMode" = "ForwardBase"
                }

                Blend SrcAlpha OneMinusSrcAlpha

                ZWrite Off

                // 只画深度预写得到的最前表面
                ZTest Equal

                Cull Off


                CGPROGRAM

                #pragma vertex vert
                #pragma fragment frag
                #pragma target 3.0

                #pragma multi_compile_fwdbase

                #include "UnityCG.cginc"
                #include "Lighting.cginc"
                #include "AutoLight.cginc"


                sampler2D _MainTex;
                float4 _MainTex_ST;

                fixed4 _Color;

                float _Fade;

                fixed4 _ShadowColor;
                float _ShadowStrength;
                float _LightSmoothness;
                float _AmbientStrength;

                fixed4 _HighlightColor;
                float _HighlightStrength;
                float _HighlightPower;


                struct appdata
                {
                    float4 vertex : POSITION;
                    float3 normal : NORMAL;
                    float2 uv : TEXCOORD0;
                };


                struct v2f
                {
                    float4 pos : SV_POSITION;

                    float2 uv : TEXCOORD0;

                    float3 worldPos : TEXCOORD1;
                    float3 worldNormal : TEXCOORD2;

                    LIGHTING_COORDS(3,4)
                };


                v2f vert(appdata v)
                {
                    v2f o;

                    o.pos =
                        UnityObjectToClipPos(v.vertex);

                    o.uv =
                        TRANSFORM_TEX(v.uv, _MainTex);

                    o.worldPos =
                        mul(
                            unity_ObjectToWorld,
                            v.vertex
                        ).xyz;

                    o.worldNormal =
                        UnityObjectToWorldNormal(
                            v.normal
                        );

                    TRANSFER_VERTEX_TO_FRAGMENT(o);

                    return o;
                }


                fixed4 frag(
                    v2f i,
                    fixed facing : VFACE
                ) : SV_Target
                {
                    // ============================
                    // Texture
                    // ============================

                    fixed4 tex =
                        tex2D(
                            _MainTex,
                            i.uv
                        );

                    fixed4 baseColor =
                        tex * _Color;


                    // ============================
                    // Double-sided normal
                    //
                    // 背面法线翻转
                    // 避免桌布背面光照完全错误
                    // ============================

                    float3 N =
                        normalize(
                            i.worldNormal
                        );

                    if (facing < 0)
                    {
                        N = -N;
                    }


                    // ============================
                    // Light
                    // ============================

                    float3 L =
                        normalize(
                            _WorldSpaceLightPos0.xyz
                        );

                    float3 V =
                        normalize(
                            _WorldSpaceCameraPos.xyz -
                            i.worldPos
                        );


                    // ============================
                    // Soft Half Lambert
                    // ============================

                    float NdotL =
                        dot(N, L);

                    float halfLambert =
                        NdotL * 0.5 + 0.5;


                    float lightMask =
                        smoothstep(
                            0.15,
                            0.15 + _LightSmoothness,
                            halfLambert
                        );


                    // ============================
                    // Shadow Tint
                    // ============================

                    float3 shadowCol =
                        baseColor.rgb *
                        _ShadowColor.rgb;


                    float3 diffuse =
                        lerp(
                            shadowCol,
                            baseColor.rgb,
                            lightMask
                        );


                    diffuse =
                        lerp(
                            baseColor.rgb,
                            diffuse,
                            _ShadowStrength
                        );


                    // ============================
                    // Realtime Shadow
                    // ============================

                    UNITY_LIGHT_ATTENUATION(
                        atten,
                        i,
                        i.worldPos
                    );


                    diffuse *=
                        lerp(
                            0.7,
                            1.0,
                            atten
                        );


                    // ============================
                    // Ambient
                    // ============================

                    float3 ambient =
                        UNITY_LIGHTMODEL_AMBIENT.rgb *
                        baseColor.rgb *
                        _AmbientStrength;


                    // ============================
                    // Soft Highlight
                    // ============================

                    float3 H =
                        normalize(
                            L + V
                        );


                    float spec =
                        pow(
                            saturate(
                                dot(N,H)
                            ),
                            _HighlightPower
                        );


                    spec *=
                        _HighlightStrength *
                        atten;


                    float3 highlight =
                        _HighlightColor.rgb *
                        spec;


                    // ============================
                    // Final Color
                    // ============================

                    float3 finalColor =
                        diffuse +
                        ambient +
                        highlight;


                    // ============================
                    // 真正平滑透明
                    // ============================

                    float alpha =
                        saturate(
                            baseColor.a *
                            _Fade
                        );


                    return fixed4(
                        finalColor,
                        alpha
                    );
                }

                ENDCG
            }
        }

            FallBack Off
}