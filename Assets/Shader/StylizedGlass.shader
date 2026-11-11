Shader "Custom/StylizedGlass"
{
    Properties
    {
        [Header(Base)]
        _GlassColor("Glass Color", Color) = (0.55, 0.9, 0.95, 1)
        _Opacity("Opacity", Range(0,1)) = 0.22

        [Header(Fresnel)]
        _FresnelColor("Fresnel Color", Color) = (0.75, 1.0, 1.0, 1)
        _FresnelPower("Fresnel Power", Range(0.5,8)) = 3.0
        _FresnelStrength("Fresnel Strength", Range(0,2)) = 0.65
        _EdgeOpacity("Edge Opacity", Range(0,1)) = 0.45

        [Header(Specular)]
        _SpecularColor("Specular Color", Color) = (1,1,1,1)
        _SpecularStrength("Specular Strength", Range(0,2)) = 0.45
        _SpecularPower("Specular Power", Range(1,128)) = 36

        [Header(Lighting)]
        _LightStrength("Light Strength", Range(0,2)) = 0.45
        _ShadowTint("Shadow Tint", Color) = (0.25,0.55,0.6,1)

        [Header(Reflection Fake)]
        _TopHighlightStrength("Top Highlight", Range(0,1)) = 0.18
    }

        SubShader
        {
            Tags
            {
                "Queue" = "Transparent"
                "RenderType" = "Transparent"
                "IgnoreProjector" = "True"
            }

            LOD 200

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            Pass
            {
                Tags { "LightMode" = "ForwardBase" }

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fwdbase

                #include "UnityCG.cginc"
                #include "Lighting.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float3 normal : NORMAL;
                };

                struct v2f
                {
                    float4 pos : SV_POSITION;
                    float3 worldPos : TEXCOORD0;
                    float3 worldNormal : TEXCOORD1;
                    float3 localPos : TEXCOORD2;
                };

                fixed4 _GlassColor;
                float _Opacity;

                fixed4 _FresnelColor;
                float _FresnelPower;
                float _FresnelStrength;
                float _EdgeOpacity;

                fixed4 _SpecularColor;
                float _SpecularStrength;
                float _SpecularPower;

                float _LightStrength;
                fixed4 _ShadowTint;

                float _TopHighlightStrength;

                v2f vert(appdata v)
                {
                    v2f o;

                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                    o.worldNormal = UnityObjectToWorldNormal(v.normal);
                    o.localPos = v.vertex.xyz;

                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    float3 N = normalize(i.worldNormal);
                    float3 V = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);
                    float3 L = normalize(_WorldSpaceLightPos0.xyz);

                    // ------------------------
                    // Fresnel 边缘
                    // ------------------------
                    float NdotV = saturate(dot(N, V));

                    float fresnel =
                        pow(1.0 - NdotV, _FresnelPower);

                    // ------------------------
                    // 主光
                    // ------------------------
                    float NdotL = saturate(dot(N, L));

                    float3 lightTint =
                        lerp(
                            _ShadowTint.rgb,
                            _GlassColor.rgb,
                            NdotL
                        );

                    float3 col =
                        _GlassColor.rgb +
                        lightTint * _LightStrength;

                    // ------------------------
                    // Fresnel 颜色
                    // ------------------------
                    col +=
                        _FresnelColor.rgb *
                        fresnel *
                        _FresnelStrength;

                    // ------------------------
                    // 高光
                    // ------------------------
                    float3 H = normalize(L + V);

                    float spec =
                        pow(
                            saturate(dot(N, H)),
                            _SpecularPower
                        );

                    col +=
                        _SpecularColor.rgb *
                        spec *
                        _SpecularStrength;

                    // ------------------------
                    // 假顶部反光
                    // 朝上的面稍微变亮
                    // ------------------------
                    float upFacing =
                        saturate(dot(N, float3(0,1,0)));

                    col +=
                        _FresnelColor.rgb *
                        upFacing *
                        _TopHighlightStrength;

                    // ------------------------
                    // Alpha
                    // 正面透明
                    // 边缘更加不透明
                    // ------------------------
                    float alpha =
                        _Opacity +
                        fresnel * _EdgeOpacity;

                    alpha = saturate(alpha);

                    return fixed4(col, alpha);
                }

                ENDCG
            }
        }

            FallBack Off
}