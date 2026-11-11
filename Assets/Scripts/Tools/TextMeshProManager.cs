using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CasualGame
{
    public static class TextMeshProHelper
    {
        public static void SetOutline(this TextMeshProUGUI textMeshProUGUI, Color color, float softnessFace = 0f,
            float dilateFace = 0.5f,
            float thicknessOutline = 0.5f)
        {
            TextMeshProManager.Instance.SetMat(textMeshProUGUI, color, softnessFace, dilateFace, thicknessOutline);
        }
    }

    public class TextMeshProManager : Singleton<TextMeshProManager>
    {
        public Shader CommonTextMeshProShader;

        public Dictionary<(string htmlColor, float softnessFace, float dilateFace, float thicknessOutline), Material>
            Materials =
                new Dictionary<(string htmlColor, float softnessFace, float dilateFace, float thicknessOutline),
                    Material>();

        public TextMeshProManager()
        {
            CommonTextMeshProShader = Shader.Find("TextMeshPro/Distance Field");
        }

        public Color GetMatColor(TextMeshProUGUI textMeshProUGUI)
        {
            if (textMeshProUGUI.fontSharedMaterial != null &&
                textMeshProUGUI.fontSharedMaterial.HasProperty("_OutlineColor"))
            {
                return textMeshProUGUI.fontSharedMaterial.GetColor("_OutlineColor");
            }
            return Color.white;
        }

        public void SetMat(TextMeshProUGUI textMeshProUGUI, Color color, float softnessFace = 0f,
            float dilateFace = 0.5f,
            float thicknessOutline = 0.5f)
        {
            string str = ColorUtility.ToHtmlStringRGB(color);
            (string htmlColor, float softnessFace, float dilateFace, float thicknessOutline) key = (str, softnessFace,
                dilateFace, thicknessOutline);
            float scale;
            float width;
            float height;
            float scaleX;
            float scaleY;
            float vertexOffsetX;
            float vertexOffsetY;
            float stencil;
            float stencilComp;
            float maskSoftnessX;
            float maskSoftnessY;
            Vector4 clipRect;
            Texture texture;


            if (Materials.TryGetValue(key, out var it))
            {
                textMeshProUGUI.fontSharedMaterial = it;
            }
            else
            {
                if (textMeshProUGUI.fontSharedMaterial != null &&
                    textMeshProUGUI.fontSharedMaterial.HasProperty("_MainTex"))
                {
                    scale = textMeshProUGUI.fontSharedMaterial.GetFloat("_GradientScale");
                    width = textMeshProUGUI.fontSharedMaterial.GetFloat("_TextureWidth");
                    height = textMeshProUGUI.fontSharedMaterial.GetFloat("_TextureHeight");
                    scaleX = textMeshProUGUI.fontSharedMaterial.GetFloat("_ScaleX");
                    scaleY = textMeshProUGUI.fontSharedMaterial.GetFloat("_ScaleY");
                    vertexOffsetX = textMeshProUGUI.fontSharedMaterial.GetFloat("_VertexOffsetX");
                    vertexOffsetY = textMeshProUGUI.fontSharedMaterial.GetFloat("_VertexOffsetY");
                    clipRect = textMeshProUGUI.fontSharedMaterial.GetVector("_ClipRect");
                    stencil = textMeshProUGUI.fontSharedMaterial.GetFloat("_Stencil");
                    stencilComp = textMeshProUGUI.fontSharedMaterial.GetFloat("_StencilComp");
                    maskSoftnessX = textMeshProUGUI.fontSharedMaterial.GetFloat("_MaskSoftnessX");
                    maskSoftnessY = textMeshProUGUI.fontSharedMaterial.GetFloat("_MaskSoftnessY");
                    texture = textMeshProUGUI.fontSharedMaterial.GetTexture("_MainTex");
                }
                else
                {
                    return;
                }

                it = new Material(CommonTextMeshProShader);
                if (thicknessOutline > 0f)
                {
                    it.SetColor("_OutlineColor", color);
                    if (it.HasProperty("_FaceDilate"))
                    {
                        it.SetFloat("_FaceDilate", dilateFace);
                    }

                    if (it.HasProperty("_OutlineWidth"))
                    {
                        it.SetFloat("_OutlineWidth", thicknessOutline);
                    }

                    if (it.HasProperty("_FaceSoftness"))
                    {
                        it.SetFloat("_FaceSoftness", softnessFace);
                    }
                }


                #region 字体属性

                it.SetFloat("_GradientScale", scale);
                it.SetFloat("_TextureWidth", width);
                it.SetFloat("_TextureHeight", height);
                it.SetFloat("_ScaleX", scaleX);
                it.SetFloat("_ScaleY", scaleY);
                it.SetFloat("_VertexOffsetX", vertexOffsetX);
                it.SetFloat("_VertexOffsetY", vertexOffsetY);
                it.SetVector("_ClipRect", clipRect);
                it.SetFloat("_Stencil", stencil);
                it.SetFloat("_StencilComp", stencilComp);
                it.SetFloat("_MaskSoftnessX", maskSoftnessX);
                it.SetFloat("_MaskSoftnessY", maskSoftnessY);
                it.SetTexture("_MainTex", texture);
                // it.SetInt(TMPro.ShaderUtilities.Keyword_Outline, 1);

                #endregion

                textMeshProUGUI.fontSharedMaterial = it;
                Materials[key] = it;
            }
        }
    }
}