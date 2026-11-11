using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
public class ShaderTool
{
    public interface IShaderTool
    {
        public List<Material> GetAllMaterials(GameObject gameObject);
    }

    private static IShaderTool _iShaderTool;

    private static void CheckAndCreateIShaderTool()
    {
        if (_iShaderTool != null) return;

        Assembly[] s_Assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var baseType = typeof(IShaderTool);

        foreach (var assembly in s_Assemblies)
        {
            var subTypes = assembly.GetTypes()
                .Where(t => baseType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var type in subTypes)
            {
                IShaderTool iShaderTool = (IShaderTool)Activator.CreateInstance(type);
                _iShaderTool = iShaderTool;
                return;
            }
        }
    }

    public static void ResetEditorShader(UnityEngine.Object unityEngineObject, string assetName)
    {
        CheckAndCreateIShaderTool();

        UnityEngine.Object tempObject = UnityEditor.AssetDatabase.LoadAssetAtPath(assetName, unityEngineObject.GetType());

        List<Material> editorMaterials = GetAllMaterial(tempObject);

        List<Material> materials = GetAllMaterial(unityEngineObject);

        UseEditorShader(editorMaterials, materials);
    }

    private static List<Material> GetAllMaterial(UnityEngine.Object tempObject)
    {
        List<Material> materials = new List<Material>();
        switch (tempObject)
        {
            case GameObject gameObject:
                {
                    materials.AddRange(_iShaderTool.GetAllMaterials(gameObject));

                    break;
                }
            case Material material:
                {
                    materials.Add(material);
                    break;
                }
        }

        return materials;
    }

    private static void UseEditorShader(List<Material> editorMaterials, List<Material> materials)
    {
        foreach (var material in materials)
        {
            if (material == null) continue;

            string shaderName = material.shader.name;

            Material editorMaterial = GetEditorMaterial(editorMaterials, material);
            if (editorMaterial != null)
            {
                shaderName = editorMaterial.shader.name;
            }

            Shader newShader = Find(shaderName);

            if (newShader != null)
            {
                material.shader = newShader;
            }
        }
    }

    private static Material GetEditorMaterial(List<Material> materials, Material material)
    {
        foreach (var tempMaterial in materials)
        {
            if (tempMaterial == null) continue;

            if (tempMaterial.name == material.name)
            {
                return tempMaterial;
            }
        }

        return null;
    }

    private static Shader Find(string shaderName)
    {
        Shader findShader = Shader.Find(shaderName);

        if (findShader == null || shaderName == "Hidden/InternalErrorShader")
        {
            findShader = Shader.Find("Standard");
        }

        return findShader;
    }
}
#endif