using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "GlobalTexturePostprocessorSetting",
    menuName = "AssetPostprocessor/Texture/Create GlobalSetting")]
public class GlobalTexturePostprocessorSetting : ScriptableObject
{
    [Serializable]
    public class TextureImporterSettings
    {
        public TextureImporterType textureType;

        public List<string> assetPaths = new List<string>();

        public bool isReadable;
        public TextureImporterNPOTScale npotScale;
        public bool mipmapEnabled;
        public SpriteImportMode spriteImportMode;

        public int maxTextureSize = -1;

        // 各平台配置
        public List<TextureImporterPlatformSettings> platformSettings =
            new List<TextureImporterPlatformSettings>();
    }

    public List<TextureImporterSettings> textureImporterSettings = new List<TextureImporterSettings>()
    {
        new TextureImporterSettings()
        {
            textureType = TextureImporterType.Default,
            isReadable = false,
            npotScale = TextureImporterNPOTScale.ToNearest,
            mipmapEnabled = false,
            spriteImportMode = SpriteImportMode.Single,
            platformSettings = new List<TextureImporterPlatformSettings>()
            {
                new TextureImporterPlatformSettings()
                {
                    name = "iPhone",
                    textureCompression = TextureImporterCompression.Compressed,
                    format = TextureImporterFormat.ASTC_8x8,
                    overridden = true,
                },
                new TextureImporterPlatformSettings()
                {
                    name = "Android",
                    textureCompression = TextureImporterCompression.Compressed,
                    format = TextureImporterFormat.ASTC_8x8,
                    overridden = true,
                },
            }
        },
        new TextureImporterSettings()
        {
            textureType = TextureImporterType.Sprite,
            isReadable = false,
            npotScale = TextureImporterNPOTScale.ToNearest,
            mipmapEnabled = false,
            spriteImportMode = SpriteImportMode.Single,
            platformSettings = new List<TextureImporterPlatformSettings>()
            {
                new TextureImporterPlatformSettings()
                {
                    name = "iPhone",
                    textureCompression = TextureImporterCompression.Compressed,
                    format = TextureImporterFormat.ASTC_4x4,
                    overridden = true,
                },
                new TextureImporterPlatformSettings()
                {
                    name = "Android",
                    textureCompression = TextureImporterCompression.Compressed,
                    format = TextureImporterFormat.ASTC_4x4,
                    overridden = true,
                },
            }
        }
    };



#if UNITY_EDITOR
    /// <summary>
    /// 加载相关的配置文件
    /// </summary>
    public static TSetting LoadSettingData<TSetting>() where TSetting : ScriptableObject
    {
        var settingType = typeof(TSetting);
        var guids = UnityEditor.AssetDatabase.FindAssets($"t:{settingType.Name}");
        if (guids.Length == 0)
        {
            Debug.LogWarning($"Create new {settingType.Name}.asset");
            var setting = ScriptableObject.CreateInstance<TSetting>();
            string filePath = $"searchInFolder/{settingType.Name}.asset";
            UnityEditor.AssetDatabase.CreateAsset(setting, filePath);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            return setting;
        }
        else
        {
            if (guids.Length != 1)
            {
                foreach (var guid in guids)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                    Debug.LogWarning($"Found multiple file : {path}");
                }
                throw new System.Exception($"Found multiple {settingType.Name} files !");
            }

            string filePath = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
            var setting = UnityEditor.AssetDatabase.LoadAssetAtPath<TSetting>(filePath);
            return setting;
        }
    }
#endif
}