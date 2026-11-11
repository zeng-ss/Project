using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using TMPro;
using UnityEngine;

public class CustomShaderTool : ShaderTool.IShaderTool
{
    public List<Material> GetAllMaterials(GameObject gameObject)
    {
        List<Material> materials = new List<Material>();
        
        List<Renderer> renderers = new List<Renderer>();
        //物件上的材质
        gameObject.GetComponentsInChildren(true, renderers);

        foreach (var renderer in renderers)
        {
            foreach (var material in renderer.sharedMaterials)
            {
                materials.Add(material);
            }

            //粒子
            if (renderer is ParticleSystemRenderer particleRender)
            {
                materials.Add(particleRender.trailMaterial);
            }
        }
        
        //贴图上的材质
        List<UnityEngine.UI.Image> images = new List<UnityEngine.UI.Image>();
        gameObject.GetComponentsInChildren(true, images);
        foreach (var image in images)
        {
            materials.Add(image.material);
        }
               
        //贴图上的材质
        List<UnityEngine.UI.RawImage> rawImages = new List<UnityEngine.UI.RawImage>();
        gameObject.GetComponentsInChildren(true, rawImages);
        foreach (var image in rawImages)
        {
            materials.Add(image.material);
        }

        // 骨骼动画上的材质
        List<SkeletonGraphic> skeletonGraphics = new List<SkeletonGraphic>();
        gameObject.GetComponentsInChildren(true, skeletonGraphics);
        foreach (var skeletonGraphic in skeletonGraphics)
        {
            materials.Add(skeletonGraphic.material);
        }
               
        // TextMeshPro 上的材质
        List<TextMeshProUGUI> textMeshProUGUIs = new List<TextMeshProUGUI>();
        gameObject.GetComponentsInChildren(true, textMeshProUGUIs);
        foreach (var textMeshProUGUI in textMeshProUGUIs)
        {
            materials.Add(textMeshProUGUI.fontSharedMaterial);
        }

        return materials;
    }
}
