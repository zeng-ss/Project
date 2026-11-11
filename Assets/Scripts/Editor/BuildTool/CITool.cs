using UnityEditor;
using UnityEngine;
using System;
using UnityEditor.Purchasing;
using System.Collections.Generic;
using System.IO;
using UnityGameFramework.Editor.ResourceTools;
using UnityGameFramework.Editor;
using GooglePlayServices;

public static class CITool
{
    public class PurchaseTool
    {
        private const string m_GeneratedCredentialsTemplateFilename = "IAPGeneratedCredentials.cs.template";
        private const string m_GeneratedCredentialsTemplateFilenameNoExtension = "IAPGeneratedCredentials.cs";
        private const string m_googlePlayPublicKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAm+4OirkjLjrBp6nWIWUY0/LYspvoPqLZnOh7JCeBhHu+UTr/JqcKYWrkT78bqz/cQaE1iiuxBOXMXypamc287RodC8AiND+GES+KU6QeOn3qhITxQUXpFUo9wt2lJ5dAt1WpyqsxZ8+Ep5xsIHHwkYg4Puw5NDOHi3U17L3/sTXWmn73n3SdD0F9gZTNb3fEzC+83CXUMed+byKgyDpXKZgNLqFNH6xppUtO3++fkPX54rvfzOBljjB9MOaQTtwDmN9V51EhML4hz4Dden5xfkOQ144BZU1b7dw2BvFdRlxO/J2M7KEUUSKsPK96dT6xeYwCxD0p4ADWCmyQ4tnu2QIDAQAB";

        static internal class TangleFileConsts
        {
            internal const string k_OutputPath = "Assets/Scripts/UnityPurchasing/generated";
            internal const string k_GooglePlayClassPrefix = "GooglePlay";
            internal const string k_ObfuscationClassSuffix = "Tangle.cs";
        }

        public static string ObfuscateGoogleSecrets()
        {
            var googleError = WriteObfuscatedGooglePlayClassAsAsset(m_googlePlayPublicKey);

            AssetDatabase.Refresh();

            return googleError;
        }

        public static string WriteObfuscatedGooglePlayClassAsAsset(string googlePlayPublicKey)
        {
            string googleError = null;
            var key = 0;
            var order = new int[0];
            var tangled = new byte[0];
            try
            {
                var bytes = Convert.FromBase64String(googlePlayPublicKey);
                order = new int[bytes.Length / 20 + 1];

                tangled = TangleObfuscator.Obfuscate(bytes, order, out key);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Invalid Google Play Public Key. Generating incomplete credentials file. " + e);
                googleError =
                    "  The Google Play License Key is invalid. GooglePlayTangle was generated with incomplete credentials.";
            }
            WriteObfuscatedClassAsAsset(TangleFileConsts.k_GooglePlayClassPrefix, key, order, tangled, tangled.Length != 0);

            return googleError;
        }

        public static void WriteObfuscatedClassAsAsset(string classnamePrefix, int key, int[] order, byte[] data, bool populated)
        {
            var substitutionDictionary = new Dictionary<string, string>()
            {
                {"{NAME}", classnamePrefix.ToString()},
                {"{KEY}", key.ToString()},
                {"{ORDER}", String.Format("{0}",String.Join(",", Array.ConvertAll(order, i => i.ToString())))},
                {"{DATA}", Convert.ToBase64String(data)},
                {"{POPULATED}", populated.ToString().ToLowerInvariant()} // Defaults to XML-friendly values
            };

            var templateText = LoadTemplateText(out var templateRelativePath);

            if (templateText != null)
            {
                var outfileText = templateText;

                // Apply the parameters to the template
                foreach (var pair in substitutionDictionary)
                {
                    outfileText = outfileText.Replace(pair.Key, pair.Value);
                }
                Directory.CreateDirectory(TangleFileConsts.k_OutputPath);
                File.WriteAllText(FullPathForTangleClass(classnamePrefix), outfileText);
            }
        }

        /// <summary>
        /// Loads the template file.
        /// </summary>
        /// <returns>The template file's text.</returns>
        /// <param name="templateRelativePath">Relative Assets/ path to template file.</param>
        public static string LoadTemplateText(out string templateRelativePath)
        {
            var assetGUIDs =
                AssetDatabase.FindAssets(m_GeneratedCredentialsTemplateFilenameNoExtension);
            string templateGUID = null;
            templateRelativePath = null;

            if (assetGUIDs.Length > 0)
            {
                templateGUID = assetGUIDs[0];
            }
            else
            {
                Debug.LogError(String.Format("Could not find template \"{0}\"",
                    m_GeneratedCredentialsTemplateFilename));
            }

            string templateText = null;

            if (templateGUID != null)
            {
                templateRelativePath = AssetDatabase.GUIDToAssetPath(templateGUID);

                var templateAbsolutePath =
                    Path.GetDirectoryName(Application.dataPath)
                    + Path.DirectorySeparatorChar
                    + templateRelativePath;

                templateText = File.ReadAllText(templateAbsolutePath);
            }

            return templateText;
        }

        static string FullPathForTangleClass(string classnamePrefix)
        {
            return Path.Combine(TangleFileConsts.k_OutputPath, string.Format($"{classnamePrefix}{TangleFileConsts.k_ObfuscationClassSuffix}"));
        }
    }

    public class KeyStoreTool
    {
        public class KeyStoreData
        {
            public string keyStorePath;
            public string keyStorePassword;
            public string keyAlias;
            public string keyAliasPassword;
        }

        [MenuItem("Tools/CITool/ReadKeyStoreProperties")]
        public static void ReadKeyStoreTest()
        {
            var keyStoreData = ReadKeyStoreProperties();
            Debug.Log(keyStoreData.keyStorePath);
            Debug.Log(keyStoreData.keyStorePassword);
            Debug.Log(keyStoreData.keyAlias);
            Debug.Log(keyStoreData.keyAliasPassword);
        }

        public static KeyStoreData ReadKeyStoreProperties()
        {
            try
            {
                string projectRoot = Path.GetDirectoryName(Path.GetDirectoryName(Application.dataPath));
                string propertiesPath = Path.Combine(projectRoot, "keystore.properties");

                if (!File.Exists(propertiesPath))
                {
                    Debug.LogError($"KeyStore properties file not found: {propertiesPath}");
                    return null;
                }

                var content = File.ReadAllText(propertiesPath);
                var data = new KeyStoreData();
                var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();
                    if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                        continue;

                    var equalIndex = trimmedLine.IndexOf('=');
                    if (equalIndex <= 0 || equalIndex >= trimmedLine.Length - 1)
                        continue;

                    var key = trimmedLine.Substring(0, equalIndex).Trim();
                    var value = trimmedLine.Substring(equalIndex + 1).Trim();

                    switch (key)
                    {
                        case "storeName":
                            data.keyStorePath = value;
                            break;
                        case "storePassword":
                            data.keyStorePassword = value;
                            break;
                        case "aliasName":
                            data.keyAlias = value;
                            break;
                        case "aliasPassword":
                            data.keyAliasPassword = value;
                            break;
                    }
                }

                Debug.Log($"KeyStore properties: {data.keyStorePath}, {data.keyStorePassword}, {data.keyAlias}, {data.keyAliasPassword}");
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to read KeyStore properties: {ex.Message}");
                return null;
            }
        }
    }

    public class DataTableTool
    {
        public static void GenerateDataTables()
        {
            bool isMac = IsMac();
            System.Reflection.MethodInfo method = null;
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    // 查找包含 GenerateDataTables(bool) 方法的類型
                    var candidateMethod = type.GetMethod("GenerateDataTables",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static,
                        null,
                        new System.Type[] { typeof(bool) },
                        null);

                    if (candidateMethod != null)
                    {
                        method = candidateMethod;
                        break;
                    }
                }
                if (method != null)
                    break;
            }

            if (method == null)
            {
                Debug.LogError("無法找到 GenerateDataTables(bool) 方法");
                return;
            }

            method.Invoke(null, new object[] { isMac });
        }

        private static bool IsMac()
        {
            string os = SystemInfo.operatingSystem;
            if (os.Contains("Windows"))
            {
                return false;
            }
            return true;
        }
    }

    public static void CompileUnity()
    {
        Debug.Log("CompileUnity");
    }

    public static void CompileProject()
    {
        PurchaseTool.ObfuscateGoogleSecrets();
        DataTableTool.GenerateDataTables();

        PlayServicesResolver.DeleteResolvedLibraries();
        PlayServicesResolver.ResolveSync(true);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("CompileProject completed");
    }

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

    public static bool BuildAssetBundle(Platform platform, string applicableGameVersion, int internalResourceVersion, bool forceRebuildAssetBundle)
    {
        switch (platform)
        {
            case Platform.IOS:
            {
                return AssetBundleBuildTool.BuildAssetBundle(BuildTarget.iOS, applicableGameVersion, internalResourceVersion, forceRebuildAssetBundle);
            }
            default:
            {
                return AssetBundleBuildTool.BuildAssetBundle(BuildTarget.Android, applicableGameVersion, internalResourceVersion, forceRebuildAssetBundle);
            }
        }
    }

    private const string EnableMaterialDefineSymbol = "ENABLE_MATERIAL";

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
        Debug.Log($"IsMaterial: {IsMaterial}");

        if (bool.TryParse(IsBuildBundle, out bool isBuildAssetBundle) && isBuildAssetBundle)
        {
            bool SuccessBuild = BuildAssetBundle(Platform.Android, Version, int.Parse(InternalResourceVersion), bool.Parse(ForceRebuildAssetBundle));
            if (!SuccessBuild)
            {
                Debug.LogError("BuildAssetBundle 构建失败！");
                return;
            }
        }

        if (bool.TryParse(IsAAB, out bool isAAB) && isAAB)
        {
            EditorUserBuildSettings.buildAppBundle = true;
            PlayerSettings.Android.splitApplicationBinary = true;
            EditorPrefs.SetString("EXPORT_GRADLE_PROJECT", "AAB");
        }
        else
        {
            EditorUserBuildSettings.buildAppBundle = false;
            PlayerSettings.Android.splitApplicationBinary = false;
            EditorPrefs.SetString("EXPORT_GRADLE_PROJECT", "APK");
        }

        // 设置包名
        PlayerSettings.applicationIdentifier = PkgName;
        PlayerSettings.bundleVersion = Version;
        PlayerSettings.Android.bundleVersionCode = int.Parse(Version.Replace(".", ""));
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        EditorUserBuildSettings.exportAsGoogleAndroidProject = true;

        // 设置签名信息
        var keyStoreData = KeyStoreTool.ReadKeyStoreProperties();
        if (keyStoreData == null)
        {
            Debug.LogError("KeyStore properties file not found");
            return;
        }

        PlayerSettings.Android.keystoreName = keyStoreData.keyStorePath;
        PlayerSettings.Android.keystorePass = keyStoreData.keyStorePassword;
        PlayerSettings.Android.keyaliasName = keyStoreData.keyAlias;
        PlayerSettings.Android.keyaliasPass = keyStoreData.keyAliasPassword;

        BuildOptions buildOption = BuildOptions.None;
        buildOption |= BuildOptions.CompressWithLz4HC;

        if (bool.TryParse(IsMaterial, out bool isMaterialPackage) && isMaterialPackage)
        {
            AddEnableMaterialDefineSymbol();
        }
        else
        {
            RemoveEnableMaterialDefineSymbol();
        }

        // 开始构建工程
        BuildPipeline.BuildPlayer(GetScenes(), "outputs", BuildTarget.Android, buildOption);
    }

    public static EditorBuildSettingsScene[] GetScenes()
    {
        EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[1];
        scenes[0] = new EditorBuildSettingsScene("Assets/Resources/Scenes/KBScene.unity", true);
        EditorBuildSettings.scenes = scenes;

        return EditorBuildSettings.scenes;
    }
}
