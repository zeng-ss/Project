using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

public class TexturePostprocessor : AssetPostprocessor
{
    private const string IgnoreTag = "_Ignore";
    private const string mtsTag = "mts";

    // 固定贴图大小
    private readonly List<int> SizeList = new List<int>() { 32, 64, 128, 256, 512, 1024, 2048 };

    private int AdapterSize(int originSize)
    {
        if (originSize <= SizeList[0]) return SizeList[0];
        if (originSize >= SizeList[SizeList.Count - 1]) return SizeList[SizeList.Count - 1];

        for (int i = 0; i < SizeList.Count; i++)
        {
            int size = SizeList[i];

            if (originSize > size) continue;

            // 270 偏向于 256
            float average = (size * 0.3f + SizeList[i - 1] * 0.7f);

            if (originSize >= average)
            {
                return size;
            }
            else
            {
                return SizeList[i - 1];
            }
        }

        return SizeList[SizeList.Count - 1];
    }

    private static string? ExtractValueByKey(string key, string input)
    {
        // 转义键中的特殊字符（如正则符号）
        string escapedKey = Regex.Escape(key);
        // 正则模式：匹配键后紧跟的下划线和数字，数字后需为非数字或字符串结尾
        string pattern = $@"{escapedKey}_(\d+)(?=\D|$)";
        var match = Regex.Match(input, pattern);
        return match.Success ? match.Groups[1].Value : null;
    }

    private void OnPreprocessTexture()
    {
        if (assetPath.Contains(IgnoreTag)) return;

        GlobalTexturePostprocessorSetting globalTexturePostprocessorSetting =
            GlobalTexturePostprocessorSetting.LoadSettingData<GlobalTexturePostprocessorSetting>();

        GlobalTexturePostprocessorSetting.TextureImporterSettings textureImporterSetting = null;
        // 找出最适合的配置
        int adapterPathLength = 0;
        foreach (var tempTextureImporterSetting in globalTexturePostprocessorSetting.textureImporterSettings)
        {
            foreach (var tempPath in tempTextureImporterSetting.assetPaths)
            {
                if (!assetPath.Contains(tempPath)) continue;

                if (tempPath.Length > adapterPathLength)
                {
                    adapterPathLength = tempPath.Length;
                    textureImporterSetting = tempTextureImporterSetting;
                }
            }
        }

        if (textureImporterSetting == null) return;

        TextureImporter textureImport = (TextureImporter)assetImporter;

        textureImport.mipmapEnabled = textureImporterSetting.mipmapEnabled;
        textureImport.isReadable = textureImporterSetting.isReadable;
        textureImport.textureType = textureImporterSetting.textureType;
        textureImport.npotScale = textureImporterSetting.npotScale;
        textureImport.spriteImportMode = textureImporterSetting.spriteImportMode;

        bool haveAlpha = textureImport.DoesSourceTextureHaveAlpha();
        textureImport.alphaIsTransparency = haveAlpha;

        object[] args = new object[2] { 0, 0 };
        MethodInfo methodInfo = typeof(TextureImporter).GetMethod("GetWidthAndHeight",
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        methodInfo.Invoke(textureImport, args);

        int maxSize = Mathf.Max((int)args[0], (int)args[1]);
        int adapterSize = AdapterSize(maxSize);

        if (textureImporterSetting.maxTextureSize > 0 && textureImporterSetting.maxTextureSize < adapterSize)
        {
            adapterSize = textureImporterSetting.maxTextureSize;
        }

        // 匹配命名
        string? customSize = ExtractValueByKey(mtsTag, assetPath);
        if (customSize != null && int.TryParse(customSize, out int customSizeInt))
        {
            adapterSize = customSizeInt;
        }

        TextureImporterFormat formatTexture = TextureImporterFormat.ASTC_4x4;
        if (adapterSize <= 128 || assetPath.Contains("_HD"))
        {
            formatTexture = TextureImporterFormat.RGBA32;
        }

        // 配置各个平台设置
        foreach (var platformSetting in textureImporterSetting.platformSettings)
        {
            TextureImporterPlatformSettings settings = new TextureImporterPlatformSettings();
            platformSetting.CopyTo(settings);
            settings.maxTextureSize = adapterSize;

            textureImport.SetPlatformTextureSettings(settings);
        }

        textureImport.SaveAndReimport();
    }

    private void OnPostprocessTexture(Texture2D texture)
    {
    }
}