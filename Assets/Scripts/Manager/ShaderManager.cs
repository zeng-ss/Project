using System.Collections;
using System.Collections.Generic;
using CasualGame;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class ShaderManager : Singleton<ShaderManager>
{
    /// <summary>
    /// 修改一个AssetBundle内的所有shader
    /// </summary>
    public void ResetAllMaterials(AssetBundle bundle)
    {
        if (bundle == null) return;
        if (bundle.isStreamedSceneAssetBundle) return;

        //对Material进行更改
        Material[] materials = bundle.LoadAllAssets<Material>();

        foreach (Material material in materials)
        {
            var shaderName = material.shader.name;
            if (shaderName == "Hidden/InternalErrorShader")
                continue;

            Shader newShader = Find(shaderName);
            if (newShader != null)
            {
                material.shader = newShader;
            }
            else
            {
                Debug.LogWarning("unable to refresh shader: " + shaderName + " in material " + material.name);
            }
        }

        //对GameObject进行更改
        GameObject[] gameObjects = bundle.LoadAllAssets<GameObject>();
        foreach (var gameObject in gameObjects)
        {
            List<Renderer> renderers = new List<Renderer>();
            //物件上的材质
            gameObject.GetComponentsInChildren<Renderer>(true, renderers);

            foreach (var renderer in renderers)
            {
                foreach (var material in renderer.sharedMaterials)
                {
                    UseEditorShader(material);
                }

                //粒子
                if (renderer is ParticleSystemRenderer particleRender)
                {
                    UseEditorShader(particleRender.trailMaterial);
                }
            }

            //贴图上的材质
            List<Image> images = new List<Image>();
            gameObject.GetComponentsInChildren<Image>(true, images);
            foreach (var image in images)
            {
                UseEditorShader(image.material);
            }

            List<SkeletonGraphic> skeletonGraphics = new List<SkeletonGraphic>();
            gameObject.GetComponentsInChildren<SkeletonGraphic>(true, skeletonGraphics);
            foreach (var skeletonGraphic in skeletonGraphics)
            {
                UseEditorShader(skeletonGraphic.material);
            }
        }
    }

    private void UseEditorShader(Material material)
    {
        if (material == null || material.shader == null)
            return;

        var newShader = Find(material.shader.name);

        if (newShader != null)
        {
            material.shader = newShader;
        }
    }

    private Shader Find(string shaderName)
    {
        Shader outShader = Shader.Find(shaderName);
        if (outShader == null)
        {
            outShader = Shader.Find("Standard");
        }

        return outShader;
    }
}