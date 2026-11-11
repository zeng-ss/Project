Shader "Custom/DynamicCutout"
{
    Properties
    {
        _MainTex ("主纹理", 2D) = "white" {}
        _MaskTex ("镂空遮罩(Logo)", 2D) = "white" {}
        _Color ("颜色", Color) = (1,1,1,1)
        _CutoutAmount ("镂空程度", Range(0, 1)) = 0
        _LogoScale ("Logo 大小", Range(0.1, 5)) = 1.0
        _OverallAlpha ("整體透明度", Range(0, 1)) = 1.0
        _AspectRatio ("長寬比(寬/高, 0=自動)", Float) = 0
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _MaskTex;
            float4 _MaskTex_ST;
            float4 _MaskTex_TexelSize;
            fixed4 _Color;
            float _CutoutAmount;
            float _LogoScale;
            float _OverallAlpha;
            float _AspectRatio;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, TRANSFORM_TEX(i.uv, _MainTex)) * _Color;
                
                // 長寬比：0=自動(UV梯度)，>0=手動覆寫
                float ar;
                if (_AspectRatio > 0.001)
                    ar = _AspectRatio;
                else {
                    float du = length(float2(ddx(i.uv).x, ddy(i.uv).x));
                    float dv = length(float2(ddx(i.uv).y, ddy(i.uv).y));
                    ar = max(0.001, dv / du);
                }
                float2 baseUV = (ar >= 1.0)
                    ? float2((i.uv.x - 0.5) * ar + 0.5, i.uv.y)
                    : float2(i.uv.x, (i.uv.y - 0.5) / ar + 0.5);
                // 依 Mask 紋理長寬比修正，避免 Logo 變形（371x512 → maskAspect≈0.72）
                float maskAspect = _MaskTex_TexelSize.z / max(0.001, _MaskTex_TexelSize.w);
                baseUV.x = (baseUV.x - 0.5) * maskAspect + 0.5;
                // 等比例從中心向外擴，_LogoScale>1 時 Logo 更大
                float scale = lerp(0.001, 1.0, _CutoutAmount) / max(0.1, _LogoScale);
                float2 maskUV = (baseUV - 0.5) / scale + 0.5;
                float maskAlpha;
                if (maskUV.x < 0 || maskUV.x > 1 || maskUV.y < 0 || maskUV.y > 1)
                    maskAlpha = 0.0;  // 超出範圍=顯示（非 Logo 區域）
                else
                    maskAlpha = tex2D(_MaskTex, TRANSFORM_TEX(maskUV, _MaskTex)).a;
                
                // 只鏤空 Logo 輪廓（含內部洞）：Logo=透明，背景=顯示，無過渡透明度
                float alpha = 1.0 - step(0.5, maskAlpha);
                
                fixed4 finalColor = texColor;
                finalColor.a *= alpha * _OverallAlpha;
                return finalColor;
            }
            ENDCG
        }
    }
    
    FallBack "Sprites/Default"
}