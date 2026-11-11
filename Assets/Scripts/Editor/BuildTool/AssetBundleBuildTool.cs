using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityGameFramework.Editor.ResourceTools;
using YooAsset;
using YooAsset.Editor;

public static class AssetBundleBuildTool
{
    private readonly static List<string> _copyMainfestList = new List<string>()
    {
        "DefaultPackage",
    };

    private static string _YooAssetManagerPath = Path.Combine(Application.dataPath, "Scripts/Manager/YooAssetManager/YooAssetManager.Custom.cs");

    public static bool BuildAssetBundle(BuildTarget platform, string applicableGameVersion, int internalResourceVersion, bool forceRebuildAssetBundle)
    {
        ClearAllStreamingAsset();
        DeleteAllFontAsset();

        ScriptableBuildParameters buildParameters = new ScriptableBuildParameters();
        buildParameters.BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
        buildParameters.BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
        buildParameters.BuildPipeline = EBuildPipeline.ScriptableBuildPipeline.ToString();
        buildParameters.BuildBundleType = (int)EBuildBundleType.AssetBundle;

        buildParameters.BuildTarget = platform;
        buildParameters.PackageName = YooAssetManager.DefaultPackage;
        buildParameters.PackageVersion = applicableGameVersion.Replace(".", "") + "_" + internalResourceVersion;
        buildParameters.EnableSharePackRule = true;
        buildParameters.VerifyBuildingResult = true;
        buildParameters.FileNameStyle = EFileNameStyle.HashName;
        buildParameters.BuildinFileCopyParams = "builtin";
        buildParameters.BuildinFileCopyOption = EBuildinFileCopyOption.ClearAndCopyByTags;
        buildParameters.CompressOption = ECompressOption.LZ4;
        buildParameters.ClearBuildCacheFiles = forceRebuildAssetBundle;
        buildParameters.UseAssetDependencyDB = !forceRebuildAssetBundle;
        buildParameters.EncryptionServices = null;
        buildParameters.TrackSpriteAtlasDependencies = true; //自动建立资源对象对图集的依赖关系

        ScriptableBuildPipeline pipeline = new ScriptableBuildPipeline();
        var buildResult = pipeline.Run(buildParameters, true);
        if (buildResult.Success)
        {
            UnityEditor.EditorUtility.RevealInFinder(buildResult.OutputPackageDirectory);
            // 复制 link.xml 文件
            string targetLinkPath = Path.Combine(Application.dataPath, "link.xml");
            if (File.Exists(targetLinkPath))
            {
                File.Delete(targetLinkPath);
            }
            File.Copy($"{buildResult.OutputPackageDirectory}/link.xml", targetLinkPath);
            // 复制 manifest 文件
            // string outputDir = buildParameters.GetPackageOutputDirectory();
            // CopyManifestFilesAsync(outputDir, buildParameters.PackageVersion);
        }

        return buildResult.Success;
    }

    private static void ClearAllStreamingAsset()
    {
        string streamingAssetsPath = Path.Combine(Application.dataPath, "StreamingAssets");
        if (!Directory.Exists(streamingAssetsPath))
        {
            return;
        }
        string[] fileNames = Directory.GetFiles(streamingAssetsPath, "*", SearchOption.AllDirectories);
        foreach (string fileName in fileNames)
        {
            File.Delete(fileName);
        }
    }

    private static void DeleteAllFontAsset()
    {
        string fontAssetPath = Path.Combine(Application.dataPath, "RawAsset/TTF");
        if (Directory.Exists(fontAssetPath))
        {
            Directory.Delete(fontAssetPath, true);
            AssetDatabase.Refresh();
        }
    }

    private static async Task CopyFileAsync(string sourcePath, string destinationPath)
    {
        using (var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
        using (var destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
        {
            await sourceStream.CopyToAsync(destinationStream);
        }
    }

    private static async Task UpdatePackageVersionAsync(string newPackageVersion)
    {
        try
        {
            if (!File.Exists(_YooAssetManagerPath))
            {
                Debug.LogError($"YooAssetManager.Custom.cs文件不存在: {_YooAssetManagerPath}");
                return;
            }

            string fileContent;
            using (var reader = new StreamReader(_YooAssetManagerPath, System.Text.Encoding.UTF8))
            {
                fileContent = await reader.ReadToEndAsync();
            }

            string pattern = @"private const string PACKAGE_VERSION = ""[^""]*"";";
            string replacement = $"private const string PACKAGE_VERSION = \"{newPackageVersion}\";";

            string updatedContent = Regex.Replace(fileContent, pattern, replacement);

            if (updatedContent != fileContent)
            {
                using (var writer = new StreamWriter(_YooAssetManagerPath, false, System.Text.Encoding.UTF8))
                {
                    await writer.WriteAsync(updatedContent);
                }

                Debug.Log($"已更新YooAssetManager.Custom.cs中的PACKAGE_VERSION为: {newPackageVersion}");
            }
            else
            {
                Debug.LogWarning("未找到PACKAGE_VERSION常量或版本号已是最新");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"更新PACKAGE_VERSION时发生错误: {ex.Message}");
        }
    }
}