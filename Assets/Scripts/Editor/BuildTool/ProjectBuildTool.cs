using System.IO;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityGameFramework.Editor.ResourceTools;
using UnityGameFramework.Editor;

public static class ProjectBuildTool
{
    private const string EnableMaterialDefineSymbol = "ENABLE_MATERIAL";

    public enum DevelopEnvironment
    {
        None = 0,
        Production = 1, // 正式服
        staging = 2, // 测试服
    }

    public enum AndroidBuildState
    {
        None = 0,
        APK = 2,
        AppBundle = 3,
    };

    // 通过 Name 获取参数
    public static string GetArgument(string name)
    {
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].Contains(name))
            {
                return args[i + 1];
            }
        }

        return null;

    }


    public static string ProjectName
    {
        get
        {
            foreach (string arg in System.Environment.GetCommandLineArgs())
            {
                if (arg.StartsWith("project", System.StringComparison.Ordinal))
                {
                    return arg.Split("-"[0])[1];
                }
            }

            return "outputs";
        }
    }

    public static T GetEnum<T>(string text)
    {
        return (T)Enum.Parse(typeof(T), text);
    }

    // 导出安卓工程项目
    public static void ExportProjectForAndroid()
    {
        string Version = GetArgument("Version");
        string InternalResourceVersion = GetArgument("InternalResourceVersion");
        string IsAAB = GetArgument("IsAAB");
        string IsBuildBundle = GetArgument("IsBuildBundle");
        string ForceRebuildAssetBundle = GetArgument("ForceRebuildAssetBundle");
        string IsMaterial = GetArgument("IsMaterial");
        string PkgName = GetArgument("PkgName");

        Debug.Log($"Version: {Version}");
        Debug.Log($"InternalResourceVersion: {InternalResourceVersion}");
        Debug.Log($"IsAAB: {IsAAB}");
        Debug.Log($"IsBuildBundle: {IsBuildBundle}");
        Debug.Log($"ForceRebuildAssetBundle: {ForceRebuildAssetBundle}");

        if (bool.TryParse(IsBuildBundle, out bool isBuildAssetBundle) && isBuildAssetBundle)
        {
            if (!AssetBundleBuildTool.BuildAssetBundle(UnityEditor.BuildTarget.Android, Version, int.Parse(InternalResourceVersion), bool.Parse(ForceRebuildAssetBundle)))
            {
                Debug.LogError("Build AssetBundle Failed");
                return;
            }
        }

        if (bool.TryParse(IsAAB, out bool isAAB) && isAAB)
        {
            UnityEditor.EditorUserBuildSettings.buildAppBundle = true;
            UnityEditor.PlayerSettings.Android.splitApplicationBinary = true;
            UnityEditor.EditorPrefs.SetString("EXPORT_GRADLE_PROJECT", "AAB");
        }
        else
        {
            UnityEditor.EditorUserBuildSettings.buildAppBundle = false;
            UnityEditor.PlayerSettings.Android.splitApplicationBinary = false;
            UnityEditor.EditorPrefs.SetString("EXPORT_GRADLE_PROJECT", "APK");
        }

        // 设置包名
        UnityEditor.PlayerSettings.applicationIdentifier = PkgName;
        UnityEditor.PlayerSettings.bundleVersion = Version;
        UnityEditor.PlayerSettings.Android.bundleVersionCode = int.Parse(Version.Replace(".", ""));
        UnityEditor.EditorUserBuildSettings.androidBuildSystem = UnityEditor.AndroidBuildSystem.Gradle;
        UnityEditor.EditorUserBuildSettings.exportAsGoogleAndroidProject = true;

        // 设置签名信息
        UnityEditor.PlayerSettings.Android.keystoreName = "/data/keystores/FantasySort.keystore";
        UnityEditor.PlayerSettings.Android.keystorePass = "1qaz2wsx3edc";
        UnityEditor.PlayerSettings.Android.keyaliasName = "fantasysort";
        UnityEditor.PlayerSettings.Android.keyaliasPass = "1qaz2wsx3edc";

        UnityEditor.BuildOptions buildOption = UnityEditor.BuildOptions.None;
        buildOption |= UnityEditor.BuildOptions.CompressWithLz4HC;


        RemoveEnableMaterialDefineSymbol();

        // 开始构建工程
        UnityEditor.BuildPipeline.BuildPlayer(GetScenes(), ProjectName, UnityEditor.BuildTarget.Android,
            buildOption);
    }

    // 导出 XCode 工程
    public static void ExportProjectForiOS()
    {
        string Version = GetArgument("Version");
        string InternalResourceVersion = GetArgument("InternalResourceVersion");
        string IsBuildBundle = GetArgument("IsBuildBundle");
        string ForceRebuildAssetBundle = GetArgument("ForceRebuildAssetBundle");
        string buildNumber = GetArgument("buildNumber");
        string PkgName = GetArgument("PkgName");

        Debug.Log($"Version: {Version}");
        Debug.Log($"InternalResourceVersion: {InternalResourceVersion}");
        Debug.Log($"IsBuildBundle: {IsBuildBundle}");
        Debug.Log($"ForceRebuildAssetBundle: {ForceRebuildAssetBundle}");

        if (Directory.Exists(ProjectName))
            Directory.Delete(ProjectName, true);

        if (bool.TryParse(IsBuildBundle, out bool isBuildAssetBundle) && isBuildAssetBundle)
        {
            if (!AssetBundleBuildTool.BuildAssetBundle(UnityEditor.BuildTarget.iOS, Version, int.Parse(InternalResourceVersion), bool.Parse(ForceRebuildAssetBundle)))
            {
                Debug.LogError("Build AssetBundle Failed");
                return;
            }
            else
            {
                Debug.Log("Build AssetBundle Success");
            }
        }

        // 导出iOS工程
        UnityEditor.PlayerSettings.SplashScreen.show = false;
        UnityEditor.PlayerSettings.iOS.appleEnableAutomaticSigning = true;
        UnityEditor.PlayerSettings.iOS.sdkVersion = UnityEditor.iOSSdkVersion.DeviceSDK;
        UnityEditor.PlayerSettings.iOS.allowHTTPDownload = true;
        UnityEditor.PlayerSettings.iOS.requiresFullScreen = true;
        UnityEditor.PlayerSettings.statusBarHidden = true;
        UnityEditor.PlayerSettings.iOS.showActivityIndicatorOnLoading = UnityEditor.iOSShowActivityIndicatorOnLoading.DontShow;
        UnityEditor.PlayerSettings.SetArchitecture(UnityEditor.BuildTargetGroup.iOS, 1);//arm64
        UnityEditor.PlayerSettings.iOS.appleDeveloperTeamID = "6S633UUV65";//变更开发者账号时，需要修改此处
        UnityEditor.PlayerSettings.defaultInterfaceOrientation = UnityEditor.UIOrientation.Portrait;//如果需要支持横屏，修改此处
        UnityEditor.PlayerSettings.iOS.targetDevice = UnityEditor.iOSTargetDevice.iPhoneAndiPad;//如果需要支持ipad，修改此处
        UnityEditor.PlayerSettings.iOS.buildNumber = buildNumber;

        UnityEditor.PlayerSettings.bundleVersion = Version;
        // UnityEditor.PlayerSettings.iOS.applicationDisplayName = "Fantasy Sort";
        UnityEditor.PlayerSettings.SetApplicationIdentifier(UnityEditor.BuildTargetGroup.iOS, PkgName);
        UnityEditor.BuildOptions buildOption = UnityEditor.BuildOptions.None;
        buildOption |= UnityEditor.BuildOptions.CompressWithLz4HC;

        // 开始构建工程
        UnityEditor.BuildPipeline.BuildPlayer(GetScenes(), ProjectName, UnityEditor.BuildTarget.iOS,
            buildOption);
    }

    private static UnityEditor.EditorBuildSettingsScene[] GetScenes()
    {
        UnityEditor.EditorBuildSettingsScene[] scenes = new UnityEditor.EditorBuildSettingsScene[1];
        scenes[0] = new UnityEditor.EditorBuildSettingsScene("Assets/Resources/Scenes/KBScene.unity", true);
        UnityEditor.EditorBuildSettings.scenes = scenes;

        return UnityEditor.EditorBuildSettings.scenes;
    }

    public static void BuildApk()
    {
        // 获取当前系统时间
        DateTime currentTime = DateTime.Now;

        // 将时间格式化为 "yyyyMMdd" 格式
        string formattedTime = currentTime.ToString("yyyyMMddHHmm");

        string PROJECT_NAME = $"../Build/Project/Android/Puzzle_{formattedTime}.apk";

        UnityEditor.EditorUserBuildSettings.buildAppBundle = false;
        UnityEditor.PlayerSettings.Android.splitApplicationBinary = false;
        UnityEditor.EditorUserBuildSettings.exportAsGoogleAndroidProject = false;

        UnityEditor.BuildOptions buildOption = UnityEditor.BuildOptions.None;
        buildOption |= UnityEditor.BuildOptions.CompressWithLz4HC;

        // 开始构建工程
        UnityEditor.BuildPipeline.BuildPlayer(GetScenes(), PROJECT_NAME, UnityEditor.BuildTarget.Android,
            buildOption);
    }

    [UnityEditor.MenuItem("Tools/打包/直接打Bundle")]
    public static void BuildAssetBundle()
    {
        if (AssetBundleBuildTool.BuildAssetBundle(UnityEditor.BuildTarget.Android, Application.version, 1, true))
        {
            Debug.Log("Build AssetBundle Success");
        }
        else
        {
            Debug.LogError("Build AssetBundle Failed");
        }
    }

    public static void AddEnableMaterialDefineSymbol()
    {
        var targetGroup = UnityEditor.BuildTargetGroup.Android;
        ScriptingDefineSymbols.AddScriptingDefineSymbol(targetGroup, EnableMaterialDefineSymbol);
    }

    public static void RemoveEnableMaterialDefineSymbol()
    {
        var targetGroup = UnityEditor.BuildTargetGroup.Android;
        ScriptingDefineSymbols.RemoveScriptingDefineSymbol(targetGroup, EnableMaterialDefineSymbol);
    }

    [UnityEditor.MenuItem("Tools/打包/Export Android", false, 1)]
    public static void ExportAndroid()
    {
        string KeystoreName = string.Empty;
        string KeystorePassword = string.Empty;
        string KeystoreAliasName = string.Empty;
        string KeystoreAliasPassword = string.Empty;

        string Version = "0.0.1";
        string InternalResourceVersion = "1";
        string IsAAB = "false";
        string IsBuildBundle = "false";
        string ForceRebuildAssetBundle = "true";
        string IsMaterial = "false";
        string buildNumber = "1";

        Debug.Log($"Version: {Version}");
        Debug.Log($"InternalResourceVersion: {InternalResourceVersion}");
        Debug.Log($"IsAAB: {IsAAB}");
        Debug.Log($"IsBuildBundle: {IsBuildBundle}");
        Debug.Log($"ForceRebuildAssetBundle: {ForceRebuildAssetBundle}");
        Debug.Log($"IsMaterial: {IsMaterial}");

        if (Directory.Exists(ProjectName))
            Directory.Delete(ProjectName, true);

        if (bool.TryParse(IsBuildBundle, out bool isBuildAssetBundle) && isBuildAssetBundle)
        {
            if (!AssetBundleBuildTool.BuildAssetBundle(UnityEditor.BuildTarget.Android, Version, int.Parse(InternalResourceVersion), bool.Parse(ForceRebuildAssetBundle)))
            {
                Debug.LogError("Build AssetBundle Failed");
                return;
            }
            else
            {
                Debug.Log("Build AssetBundle Success");
            }
        }

        if (bool.TryParse(IsAAB, out bool isAAB) && isAAB)
        {
            UnityEditor.EditorUserBuildSettings.buildAppBundle = true;
            UnityEditor.PlayerSettings.Android.splitApplicationBinary = true;
            UnityEditor.EditorPrefs.SetString("EXPORT_GRADLE_PROJECT", "AAB");
        }
        else
        {
            UnityEditor.EditorUserBuildSettings.buildAppBundle = false;
            UnityEditor.PlayerSettings.Android.splitApplicationBinary = false;
            UnityEditor.EditorPrefs.SetString("EXPORT_GRADLE_PROJECT", "APK");
        }

        UnityEditor.PlayerSettings.bundleVersion = Version;
        UnityEditor.PlayerSettings.Android.bundleVersionCode = int.Parse(Version.Replace(".", ""));
        UnityEditor.EditorUserBuildSettings.androidBuildSystem = UnityEditor.AndroidBuildSystem.Gradle;
        UnityEditor.EditorUserBuildSettings.exportAsGoogleAndroidProject = true;

        // 设置签名信息
        UnityEditor.PlayerSettings.Android.keystoreName = "/data/keystores/FantasySort.keystore";
        UnityEditor.PlayerSettings.Android.keystorePass = "1qaz2wsx3edc";
        UnityEditor.PlayerSettings.Android.keyaliasName = "fantasysort";
        UnityEditor.PlayerSettings.Android.keyaliasPass = "1qaz2wsx3edc";

        UnityEditor.BuildOptions buildOption = UnityEditor.BuildOptions.None;
        buildOption |= UnityEditor.BuildOptions.CompressWithLz4HC;

        if (bool.TryParse(IsMaterial, out bool isMaterialPackage) && isMaterialPackage)
        {
            AddEnableMaterialDefineSymbol();
        }
        else
        {
            RemoveEnableMaterialDefineSymbol();
        }

        // 开始构建工程
        UnityEditor.BuildPipeline.BuildPlayer(GetScenes(), ProjectName, UnityEditor.BuildTarget.Android,
            buildOption);

        Debug.Log("Export Android Success");
    }

    [UnityEditor.MenuItem("Tools/打包/Export iOS", false, 1)]
    public static void ExportIOS()
    {
        string Version = "0.0.4";
        string InternalResourceVersion = "1";
        string IsAAB = "false";
        string IsBuildBundle = "true";
        string ForceRebuildAssetBundle = "true";
        string IsMaterial = "false";
        string buildNumber = "1";

        Debug.Log($"Version: {Version}");
        Debug.Log($"InternalResourceVersion: {InternalResourceVersion}");
        Debug.Log($"IsAAB: {IsAAB}");
        Debug.Log($"IsBuildBundle: {IsBuildBundle}");
        Debug.Log($"ForceRebuildAssetBundle: {ForceRebuildAssetBundle}");
        Debug.Log($"IsMaterial: {IsMaterial}");

        if (Directory.Exists(ProjectName))
            Directory.Delete(ProjectName, true);

        if (bool.TryParse(IsBuildBundle, out bool isBuildAssetBundle) && isBuildAssetBundle)
        {
            if (!AssetBundleBuildTool.BuildAssetBundle(UnityEditor.BuildTarget.iOS, Version, int.Parse(InternalResourceVersion), bool.Parse(ForceRebuildAssetBundle)))
            {
                Debug.LogError("Build AssetBundle Failed");
                return;
            }
            else
            {
                Debug.Log("Build AssetBundle Success");
            }
        }

        UnityEditor.PlayerSettings.SplashScreen.show = false;
        UnityEditor.PlayerSettings.iOS.appleEnableAutomaticSigning = true;
        UnityEditor.PlayerSettings.iOS.sdkVersion = UnityEditor.iOSSdkVersion.DeviceSDK;
        UnityEditor.PlayerSettings.iOS.allowHTTPDownload = true;
        UnityEditor.PlayerSettings.iOS.requiresFullScreen = true;
        UnityEditor.PlayerSettings.statusBarHidden = true;
        UnityEditor.PlayerSettings.iOS.showActivityIndicatorOnLoading = UnityEditor.iOSShowActivityIndicatorOnLoading.DontShow;
        UnityEditor.PlayerSettings.SetArchitecture(UnityEditor.BuildTargetGroup.iOS, 1);//arm64
        UnityEditor.PlayerSettings.iOS.appleDeveloperTeamID = "6S633UUV65";//变更开发者账号时，需要修改此处
        UnityEditor.PlayerSettings.defaultInterfaceOrientation = UnityEditor.UIOrientation.Portrait;//如果需要支持横屏，修改此处
        UnityEditor.PlayerSettings.iOS.targetDevice = UnityEditor.iOSTargetDevice.iPhoneAndiPad;//如果需要支持ipad，修改此处
        UnityEditor.PlayerSettings.iOS.buildNumber = buildNumber;

        UnityEditor.PlayerSettings.bundleVersion = Version;
        // UnityEditor.PlayerSettings.iOS.applicationDisplayName = "Fantasy Sort";
        UnityEditor.PlayerSettings.SetApplicationIdentifier(UnityEditor.BuildTargetGroup.iOS, Application.identifier);
        UnityEditor.BuildOptions buildOption = UnityEditor.BuildOptions.None;
        buildOption |= UnityEditor.BuildOptions.CompressWithLz4HC;

        // 开始构建工程
        UnityEditor.BuildPipeline.BuildPlayer(GetScenes(), ProjectName, UnityEditor.BuildTarget.iOS,
            buildOption);

        Debug.Log("Export iOS Success");
    }

    [UnityEditor.MenuItem("Tools/打包/Export WebGL", false, 2)]
    public static void ExportWebGL()
    {
        string Version = "0.0.1";
        string InternalResourceVersion = "1";
        string IsBuildBundle = "true";
        string ForceRebuildAssetBundle = "true";
        string IsMaterial = "false";

        Debug.Log($"Version: {Version}");
        Debug.Log($"InternalResourceVersion: {InternalResourceVersion}");
        Debug.Log($"IsBuildBundle: {IsBuildBundle}");
        Debug.Log($"ForceRebuildAssetBundle: {ForceRebuildAssetBundle}");
        Debug.Log($"IsMaterial: {IsMaterial}");

        // 清理旧目录
        if (Directory.Exists(ProjectName))
            Directory.Delete(ProjectName, true);

        // =============================
        // AssetBundle 构建
        // =============================
        if (bool.TryParse(IsBuildBundle, out bool isBuildAssetBundle) && isBuildAssetBundle)
        {
            if (!AssetBundleBuildTool.BuildAssetBundle(
                    BuildTarget.WebGL,
                    Version,
                    int.Parse(InternalResourceVersion),
                    bool.Parse(ForceRebuildAssetBundle)))
            {
                Debug.LogError("Build AssetBundle Failed");
                return;
            }
            else
            {
                Debug.Log("Build AssetBundle Success");
            }
        }

        // =============================
        // PlayerSettings（WebGL 专用）
        // =============================

        PlayerSettings.bundleVersion = Version;

        // 模板设置
        PlayerSettings.WebGL.template = "PROJECT:TikTokTemplate";

        // WebGL 内存（很关键，TikTok 环境建议控制）
        PlayerSettings.WebGL.memorySize = 256; // 可根据项目调 256~512

        // 压缩格式（推荐 Brotli）
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;

        // 不启用 fallback（减小包体）
        PlayerSettings.WebGL.decompressionFallback = false;

        // Data Caching（提升二次加载）
        PlayerSettings.WebGL.dataCaching = true;

        // Debug 关闭（正式包）
        EditorUserBuildSettings.development = false;

        // 减小包体
        PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
        PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.None;

        // =============================
        // Scripting Backend
        // =============================
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.WebGL, ScriptingImplementation.IL2CPP);

        // 代码裁剪（强烈建议）
        PlayerSettings.stripEngineCode = true;
        PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.WebGL, ManagedStrippingLevel.Low);

        // 异常关闭（减少体积）
        PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.WebGL, Il2CppCompilerConfiguration.Release);

        // =============================
        // 宏控制
        // =============================
        if (bool.TryParse(IsMaterial, out bool isMaterialPackage) && isMaterialPackage)
        {
            AddEnableMaterialDefineSymbol();
        }
        else
        {
            RemoveEnableMaterialDefineSymbol();
        }

        // =============================
        // BuildOptions
        // =============================
        BuildOptions buildOption = BuildOptions.None;
        buildOption |= BuildOptions.CompressWithLz4HC;

        // =============================
        // 构建
        // =============================
        BuildPipeline.BuildPlayer(
            GetScenes(),
            ProjectName,
            BuildTarget.WebGL,
            buildOption
        );

        Debug.Log("Export WebGL Success");
    }

    private static readonly string[] ModelExtensions = { ".fbx", ".obj", ".blend", ".3ds", ".dae", ".dxf", ".mb", ".ma" };

    [UnityEditor.MenuItem("Tools/改Mesh Read")]
    public static void ApplyMeshRead()
    {
        string[] meshGuids = UnityEditor.AssetDatabase.FindAssets("t:mesh", new[] { "Assets/RawAsset/Screw3D" });
        string[] modelGuids = UnityEditor.AssetDatabase.FindAssets("t:Model", new[] { "Assets/RawAsset/Screw3D" });
        var allGuids = meshGuids.Concat(modelGuids).Distinct().ToArray();
        int modifiedCount = 0;
        var processedPaths = new HashSet<string>();

        foreach (var guid in allGuids)
        {
            string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            if (processedPaths.Contains(assetPath)) continue;

            bool isModel = false;
            foreach (var ext in ModelExtensions)
            {
                if (assetPath.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                {
                    isModel = true;
                    break;
                }
            }

            if (isModel)
            {
                try
                {
                    var importer = UnityEditor.AssetImporter.GetAtPath(assetPath) as UnityEditor.ModelImporter;
                    if (importer != null && !importer.isReadable)
                    {
                        importer.isReadable = true;
                        importer.SaveAndReimport();
                        processedPaths.Add(assetPath);
                        modifiedCount++;
                        Debug.Log($"已修改 FBX/Model: {assetPath}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"处理文件 {assetPath} 时出错: {ex.Message}");
                }
            }
            else if (assetPath.EndsWith(".asset"))
            {
                try
                {
                    string content = File.ReadAllText(assetPath);
                    string modifiedContent = content.Replace("m_IsReadable: 0", "m_IsReadable: 1");
                    if (modifiedContent != content)
                    {
                        File.WriteAllText(assetPath, modifiedContent);
                        modifiedCount++;
                        Debug.Log($"已修改文件: {assetPath}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"处理文件 {assetPath} 时出错: {ex.Message}");
                }
            }
        }

        if (modifiedCount > 0)
        {
            UnityEditor.AssetDatabase.Refresh();
            Debug.Log($"总共修改了 {modifiedCount} 个文件的 Read/Write");
        }
        else
        {
            Debug.Log("没有找到需要修改的文件");
        }
    }

}