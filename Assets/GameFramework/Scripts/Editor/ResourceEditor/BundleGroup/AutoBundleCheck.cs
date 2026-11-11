//------------------------------------------------------------
// File : AutoBundleCheck.cs
// Email: mailto:zhiqiang.yang@kingboat.io
// Desc : 
//------------------------------------------------------------

using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;
using UnityEditor;
using GameFramework;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;


namespace UnityGameFramework.Editor.ResourceTools
{
    public class AutoBundleCheck
    {
        private static string _configurationPath;
        private static SortedDictionary<string, Resource> _assetBundles;
        private static SortedDictionary<string, Asset> _assets;

        [UnityEditor.MenuItem("Tools/ResourceTool/AutoDivBundle")]
        public static void AutoDivBundle()
        {
            _configurationPath = $"{Application.dataPath}/GameFramework/Configs/ResourceCollection.xml";
            _assetBundles = new SortedDictionary<string, Resource>(StringComparer.Ordinal);
            _assets = new SortedDictionary<string, Asset>(StringComparer.Ordinal);

            Group[] groups = EditorUtilityExtend.FindAssets<Group>();

            Dictionary<string, GroupAsset> dependencyGroupAssets = new Dictionary<string, GroupAsset>();

            foreach (var group in groups)
            {
                Group.CollectAssets(group, (path, entry) =>
                {
                    GroupAsset groupAsset = group.CreateAsset(path, entry);
                    groupAsset.assetBundleName =
                        Group.PackAsset(groupAsset).Replace(Settings.BundleExtension, "");
                    groupAsset.assetBundleName = groupAsset.assetBundleName.Replace("#", "");

                    if (!_assetBundles.TryGetValue(groupAsset.assetBundleName, out Resource resource))
                    {
                        resource = Resource.Create(groupAsset.assetBundleName, null, null,
                            LoadType.LoadFromFile,
                            false, null);
                        _assetBundles.Add(resource.Name, resource);
                    }

                    Asset asset = Asset.Create(groupAsset.guid, _assetBundles[groupAsset.assetBundleName]);
                    resource.AssignAsset(asset, path.EndsWith(".unity"));
                    _assets.Add(asset.Guid, asset);
                    
                    //记录依赖添加
                    List<string> dependencyInRawAsset = Group.GetDependencyInRawAsset(path);
                    foreach (var dependencyPath in dependencyInRawAsset)
                    {
                        if (dependencyGroupAssets.ContainsKey(dependencyPath)) continue;
                        
                        GroupAsset depAsset = group.CreateAsset(dependencyPath, path);
                        depAsset.assetBundleName = groupAsset.assetBundleName;
                        dependencyGroupAssets.Add(dependencyPath, depAsset);
                    }
                });
            }
            
            // 将依赖资源自动分进第一次获得依赖的包
            foreach (var keyValuePair in dependencyGroupAssets)
            {
                GroupAsset groupAsset = keyValuePair.Value;
                if (_assets.ContainsKey(groupAsset.guid)) continue;
                
                Asset asset = Asset.Create(groupAsset.guid, _assetBundles[groupAsset.assetBundleName]);
                _assets.Add(asset.Guid, asset);
            }

            Save();

            Resource resource = _assetBundles[Settings.ShadersAssetBundleName];
            List<string> assetPaths = new List<string>();
            foreach (var asset in resource.GetAssets())
            {
                assetPaths.Add(asset.Name);
            }
            SetIncludedShaders(assetPaths);
        }
        
        private static List<string> _defaultShaderList = new List<string>()
        {
            "Legacy Shaders/Diffuse",
            "Hidden/CubeBlur",
            "Hidden/CubeCopy",
            "Hidden/CubeBlend",
            "Sprites/Default",
            "UI/Default",
        };

        public static void SetIncludedShaders(List<string> includedShaderPaths)
        {
            UnityEditor.SerializedObject serializedObject =
                new UnityEditor.SerializedObject(
                    UnityEditor.AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);
            UnityEditor.SerializedProperty serializedProperty = serializedObject.GetIterator();
        
            while (serializedProperty.NextVisible(true))
            {
                if (serializedProperty.name != "m_AlwaysIncludedShaders") continue;
            
                serializedProperty.ClearArray();

                int index = 0;
                for (int i = 0; i < _defaultShaderList.Count; i++)
                {
                    serializedProperty.InsertArrayElementAtIndex(index);
                    UnityEditor.SerializedProperty dataPoint = serializedProperty.GetArrayElementAtIndex(index);

                    string shaderName = _defaultShaderList[i];
                    dataPoint.objectReferenceValue = UnityEngine.Shader.Find(shaderName);
                    index++;
                }

                for (int i = 0; i < includedShaderPaths.Count; i++)
                {
                    serializedProperty.InsertArrayElementAtIndex(index);
                    UnityEditor.SerializedProperty dataPoint = serializedProperty.GetArrayElementAtIndex(index);
                
                    string shaderPath = includedShaderPaths[i];
                    dataPoint.objectReferenceValue = UnityEditor.AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);

                    index++;
                }

                serializedObject.ApplyModifiedProperties();
            }
        }
        
        private static bool Save()
        {
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.AppendChild(xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null));

                XmlElement xmlRoot = xmlDocument.CreateElement("UnityGameFramework");
                xmlDocument.AppendChild(xmlRoot);

                XmlElement xmlCollection = xmlDocument.CreateElement("ResourceCollection");
                xmlRoot.AppendChild(xmlCollection);

                XmlElement xmlResources = xmlDocument.CreateElement("Resources");
                xmlCollection.AppendChild(xmlResources);

                XmlElement xmlAssets = xmlDocument.CreateElement("Assets");
                xmlCollection.AppendChild(xmlAssets);

                XmlElement xmlElement = null;
                XmlAttribute xmlAttribute = null;

                foreach (Resource resource in _assetBundles.Values)
                {
                    xmlElement = xmlDocument.CreateElement("Resource");
                    xmlAttribute = xmlDocument.CreateAttribute("Name");
                    xmlAttribute.Value = resource.Name;
                    xmlElement.Attributes.SetNamedItem(xmlAttribute);

                    if (resource.Variant != null)
                    {
                        xmlAttribute = xmlDocument.CreateAttribute("Variant");
                        xmlAttribute.Value = resource.Variant;
                        xmlElement.Attributes.SetNamedItem(xmlAttribute);
                    }

                    if (resource.FileSystem != null)
                    {
                        xmlAttribute = xmlDocument.CreateAttribute("FileSystem");
                        xmlAttribute.Value = resource.FileSystem;
                        xmlElement.Attributes.SetNamedItem(xmlAttribute);
                    }

                    xmlAttribute = xmlDocument.CreateAttribute("LoadType");
                    xmlAttribute.Value = ((byte)resource.LoadType).ToString();
                    xmlElement.Attributes.SetNamedItem(xmlAttribute);
                    xmlAttribute = xmlDocument.CreateAttribute("Packed");
                    xmlAttribute.Value = resource.Packed.ToString();
                    xmlElement.Attributes.SetNamedItem(xmlAttribute);
                    string[] resourceGroups = resource.GetResourceGroups();
                    if (resourceGroups.Length > 0)
                    {
                        xmlAttribute = xmlDocument.CreateAttribute("ResourceGroups");
                        xmlAttribute.Value = string.Join(",", resourceGroups);
                        xmlElement.Attributes.SetNamedItem(xmlAttribute);
                    }

                    xmlResources.AppendChild(xmlElement);
                }

                foreach (Asset asset in _assets.Values)
                {
                    xmlElement = xmlDocument.CreateElement("Asset");
                    xmlAttribute = xmlDocument.CreateAttribute("Guid");
                    xmlAttribute.Value = asset.Guid;
                    xmlElement.Attributes.SetNamedItem(xmlAttribute);
                    xmlAttribute = xmlDocument.CreateAttribute("ResourceName");
                    xmlAttribute.Value = asset.Resource.Name;
                    xmlElement.Attributes.SetNamedItem(xmlAttribute);
                    if (asset.Resource.Variant != null)
                    {
                        xmlAttribute = xmlDocument.CreateAttribute("ResourceVariant");
                        xmlAttribute.Value = asset.Resource.Variant;
                        xmlElement.Attributes.SetNamedItem(xmlAttribute);
                    }

                    xmlAssets.AppendChild(xmlElement);
                }

                string configurationDirectoryName = Path.GetDirectoryName(_configurationPath);
                if (!Directory.Exists(configurationDirectoryName))
                {
                    Directory.CreateDirectory(configurationDirectoryName);
                }

                xmlDocument.Save(_configurationPath);
                AssetDatabase.Refresh();

                return true;
            }
            catch (Exception e)
            {
                if (File.Exists(_configurationPath))
                {
                    File.Delete(_configurationPath);
                }

                Debug.LogError(e.Message);
                return false;
            }
        }
    }
}