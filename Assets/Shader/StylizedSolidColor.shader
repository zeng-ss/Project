Shader "Custom/StylizedSolidColor"
{
    Properties
    {
        [Header(Main Color)]
        _BaseColor("Base Color", Color) = (1, 0.15, 0.6, 1)
        _ShadowColor("Shadow Color", Color) = (0.55, 0.05, 0.28, 1)
        _HighlightColor("Highlight Color", Color) = (1, 0.45, 0.75, 1)

        [Header(Lighting)]
        _ShadowThreshold("Shadow Threshold", Range(0, 1)) = 0.25
        _LightSmoothness("Light Smoothness", Range(0.01, 1)) = 0.55
        _LightIntensity("Light Intensity", Range(0, 2)) = 1.0

        [Header(Fresnel)]
        _FresnelColor("Fresnel Color", Color) = (1, 0.4, 0.75, 1)
        _FresnelPower("Fresnel Power", Range(0.5, 8)) = 3.5
        _FresnelStrength("Fresnel Strength", Range(0, 1)) = 0.12

        [Header(Bottom Gradient)]
        _BottomDarkColor("Bottom Dark Color", Color) = (0.7, 0.08, 0.35, 1)
        _BottomDarkStrength("Bottom Dark Strength", Range(0, 1)) = 0.18
        _BottomHeight("Bottom Height", Range(-2, 2)) = -0.5
        _BottomRange("Bottom Range", Range(0.01, 3)) = 1.0

        [Header(Specular)]
        _SpecularColor("Specular Color", Color) = (1,1,1,1)
        _SpecularStrength("Specular Strength", Range(0, 1)) = 0.08
        _SpecularPower("Specular Power", Range(1, 128)) = 24

        [Header(Ambient)]
        _AmbientStrength("Ambient Strength", Range(0, 1)) = 0.25
    }

        SubShader
        {
            Tags
            {
                "RenderType" = "Opaque"
                "Queue" = "Geometry"
            }

            LOD 200

            Pass
            {
                Tags
                {
                    "LightMode" = "ForwardBase"
                }

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #pragma multi_compile_fwdbase

                #include "UnityCG.cginc"
                #include "Lighting.cginc"
                #include "AutoLight.cginc"

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

                    LIGHTING_COORDS(3,4)
                };

                fixed4 _BaseColor;
                fixed4 _ShadowColor;
                fixed4 _HighlightColor;

                float _ShadowThreshold;
                float _LightSmoothness;
                float _LightIntensity;

                fixed4 _FresnelColor;
                float _FresnelPower;
                float _FresnelStrength;

                fixed4 _BottomDarkColor;
                float _BottomDarkStrength;
                float _BottomHeight;
                float _BottomRange;

                fixed4 _SpecularColor;
                float _SpecularStrength;
                float _SpecularPower;

                float _AmbientStrength;

                v2f vert(appdata v)
                {
                    v2f o;

                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                    o.worldNormal = UnityObjectToWorldNormal(v.normal);

                    // 模型本地坐标，用于控制底部渐暗
                    o.localPos = v.vertex.xyz;

                    TRANSFER_VERTEX_TO_FRAGMENT(o);

                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    // ============================
                    // 基础方向
                    // ============================

                    float3 N = normalize(i.worldNormal);
                    float3 L = normalize(_WorldSpaceLightPos0.xyz);
                    float3 V = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);

                    // ============================
                    // 柔和主光
                    // ============================

                    float NdotL = dot(N, L);

                    // 把 -1~1 映射到 0~1
                    float halfLambert = NdotL * 0.5 + 0.5;

                    // 控制柔和的明暗过渡
                    float lightValue = smoothstep(
                        _ShadowThreshold,
                        _ShadowThreshold + _LightSmoothness,
                        halfLambert
                    );

                    lightValue *= _LightIntensity;
                    lightValue = saturate(lightValue);

                    // 阴影颜色 -> 基础颜色
                    float3 diffuseColor = lerp(
                        _ShadowColor.rgb,
                        _BaseColor.rgb,
                        lightValue
                    );

                    // 最亮区域稍微往 HighlightColor 靠
                    float highlightMask = smoothstep(
                        0.65,
                        1.0,
                        halfLambert
                    );

                    diffuseColor = lerp(
                        diffuseColor,
                        _HighlightColor.rgb,
                        highlightMask * 0.25
                    );

                    // ============================
                    // Unity 实时阴影
                    // ============================

                    UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos);

                    float shadowAtten = lerp(
                        0.65,
                        1.0,
                        atten
                    );

                    diffuseColor *= shadowAtten;

                    // ============================
                    // 环境光
                    // ============================

                    float3 ambient = UNITY_LIGHTMODEL_AMBIENT.rgb
                                   * _BaseColor.rgb
                                   * _AmbientStrength;

                    // ============================
                    // Fresnel 轮廓提亮
                    // ============================

                    float fresnel = 1.0 - saturate(dot(N, V));
                    fresnel = pow(fresnel, _FresnelPower);

                    float3 fresnelColor =
                        _FresnelColor.rgb
                        * fresnel
                        * _FresnelStrength;

                    // ============================
                    // 杯底渐暗
                    // ============================

                    float bottomMask = saturate(
                        (i.localPos.y - _BottomHeight)
                        / _BottomRange
                    );

                    // y 越低越暗
                    bottomMask = 1.0 - bottomMask;

                    bottomMask *= _BottomDarkStrength;

                    diffuseColor = lerp(
                        diffuseColor,
                        diffuseColor * _BottomDarkColor.rgb,
                        bottomMask
                    );

                    // ============================
                    // 柔和高光
                    // ============================

                    float3 H = normalize(L + V);

                    float spec = pow(
                        saturate(dot(N, H)),
                        _SpecularPower
                    );

                    spec *= _SpecularStrength;
                    spec *= atten;

                    float3 specular =
                        _SpecularColor.rgb * spec;

                    // ============================
                    // 输出
                    // ============================

                    float3 finalColor =
                        diffuseColor
                        + ambient
                        + fresnelColor
                        + specular;

                    return fixed4(finalColor, _BaseColor.a);
                }

                ENDCG
            }

            // 阴影投射
            UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
        }

            FallBack "Diffuse"
}