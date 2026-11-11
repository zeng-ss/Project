using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

public class AtlasTool
{
    private const string SpritePath = "Assets/Game/Sprites";
    private const string AtlasPath = "Assets/RawAsset/Atlas";
    private const string NoAtlasTag = "_NoAtlas";

    // 各平台配置
    private static List<TextureImporterPlatformSettings> _platformSettings =
        new List<TextureImporterPlatformSettings>()
        {
            new TextureImporterPlatformSettings()
            {
                name = "iPhone",
                maxTextureSize = 1024,
                format = TextureImporterFormat.ASTC_4x4,
                overridden = true,
            },
            new TextureImporterPlatformSettings()
            {
                name = "Android",
                maxTextureSize = 1024,
                format = TextureImporterFormat.ASTC_4x4,
                overridden = true,
            },
        };

    [UnityEditor.MenuItem("Tools/Sprites/Auto Create SpriteAtlas", false, 0)]
    public static void AutoCreateAtlas()
    {
        string atlasFullPath = GetFullPath(AtlasPath);

        if (!Directory.Exists(atlasFullPath))
        {
            Directory.CreateDirectory(atlasFullPath);
        }

        string spriteFullPath = GetFullPath(SpritePath);

        if (!Directory.Exists(spriteFullPath))
        {
            return;
        }

        // 先获取所有图片文件夹
        List<string> spriteDirectoryFullNames = GetAllDirectories(spriteFullPath);

        if (spriteDirectoryFullNames == null) return;

        // 将所有文件夹打包为图集，包括子文件夹的子文件夹(方便分类)
        foreach (var spriteDirectoryFullName in spriteDirectoryFullNames)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(spriteDirectoryFullName);
            string atlasName = directoryInfo.Name;

            if (atlasName.Contains(NoAtlasTag)) continue;

            SpriteAtlas spriteAtlas = GetSpriteAtlas(atlasName);

            if (spriteAtlas != null) continue;

            spriteAtlas = CreateSpriteAtlas();

            UnityEngine.Object directoryObject =
                UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(GetAssetPath(spriteDirectoryFullName));
            spriteAtlas.Add(new[] { directoryObject });

            UnityEditor.AssetDatabase.CreateAsset(spriteAtlas, $"{AtlasPath}/Atlas_{atlasName}.spriteatlas");
        }

        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
    }

    private static List<string> GetAllDirectories(string path)
    {
        if (IsHaveDirectoryAndFile(path))
        {
            Debug.LogError($"图集处理失败:{path}，文件夹下同时存在文件和文件夹，请处理成文件夹下只能有文件夹或文件一种类型");
            return null;
        }

        List<string> directories = new List<string>();

        // 获取当前文件夹下的所有子文件夹
        string[] subDirectories = Directory.GetDirectories(path);

        // 递归获取所有子文件夹的子文件夹
        foreach (string subDir in subDirectories)
        {
            List<string> subDirNames = GetAllDirectories(subDir);

            if (subDirNames == null) continue;

            if (subDirNames.Count <= 0)
            {
                directories.Add(subDir);
            }
            else
            {
                directories.AddRange(subDirNames);
            }
        }

        return directories;
    }

    private static bool IsHaveDirectoryAndFile(string path)
    {
        int directoryCount = Directory.GetDirectories(path).Length;
        bool hasDirectories = directoryCount > 0;
        string[] files = Directory.GetFiles(path);
        bool hasFiles = files.Any(file => !file.EndsWith(".meta") && !file.Contains(".DS_Store"));

        return hasDirectories && hasFiles;
    }

    private static string GetFullPath(string assetPath)
    {
        return $"{Application.dataPath}/{assetPath.Replace("Assets/", "")}";
    }

    private static string GetAssetPath(string fullPath)
    {
        int index = fullPath.IndexOf("Assets/", StringComparison.Ordinal);
        return fullPath.Substring(index, fullPath.Length - index);
    }

    private static SpriteAtlas GetSpriteAtlas(string name)
    {
        return UnityEditor.AssetDatabase.LoadAssetAtPath<SpriteAtlas>($"{AtlasPath}/Atlas_{name}.spriteatlas");
    }

    private static SpriteAtlas CreateSpriteAtlas()
    {
        // 创建 SpriteAtlas
        SpriteAtlas atlas = new SpriteAtlas();

        atlas.SetIncludeInBuild(true);

        // 设置 SpriteAtlas 的打包设置
        SpriteAtlasPackingSettings packingSettings = new SpriteAtlasPackingSettings
        {
            enableRotation = false,
            enableTightPacking = false,
            enableAlphaDilation = false,
            padding = 8
        };
        atlas.SetPackingSettings(packingSettings);

        // 设置 SpriteAtlas 的纹理设置
        SpriteAtlasTextureSettings textureSettings = new SpriteAtlasTextureSettings
        {
            readable = false,
            generateMipMaps = false,
            sRGB = true,
            filterMode = FilterMode.Bilinear
        };
        atlas.SetTextureSettings(textureSettings);

        // 配置各个平台设置
        foreach (var platformSetting in _platformSettings)
        {
            atlas.SetPlatformSettings(platformSetting);
        }

        return atlas;
    }
}