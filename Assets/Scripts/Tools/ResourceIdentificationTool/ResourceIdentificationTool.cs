using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using UnityEngine;

public partial class ResourceIdentificationTool : Singleton<ResourceIdentificationTool>
{
    private struct ResourceIdentificationInfo
    {
        public int AssetID;
        public string AssetPath;
    }

    private Dictionary<int, ResourceIdentificationInfo> _resourceIdentificationInfoMaps =
        new Dictionary<int, ResourceIdentificationInfo>();

    private Action _initSuccessEvent;

    public void InitResourceIdentificationInfos(TextAsset textAsset)
    {
        using (MemoryStream ms = new MemoryStream(textAsset.bytes))
        using (BinaryReader reader = new BinaryReader(ms))
        {
            // 读取资源数量
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                ResourceIdentificationInfo resourceIdentificationInfo = new ResourceIdentificationInfo()
                {
                    AssetID = reader.ReadInt32(),
                    AssetPath = reader.ReadString(),
                };

                _resourceIdentificationInfoMaps[resourceIdentificationInfo.AssetID] = resourceIdentificationInfo;
            }
        }
    }

    public string GetAssetPathById(int resourceIdentificationTypeId)
    {
        if (_resourceIdentificationInfoMaps.Count == 0)
        {
            Debug.LogError($"工具没有初始化");
            return String.Empty;
        }

        if (_resourceIdentificationInfoMaps.TryGetValue((int)resourceIdentificationTypeId,
                out ResourceIdentificationInfo resourceIdentificationInfo))
        {
            return $"Assets/{resourceIdentificationInfo.AssetPath}";
        }
        else
        {
            Debug.LogError($"预制体不存在: {resourceIdentificationTypeId}");
        }

        return String.Empty;
    }
}

#if UNITY_EDITOR
public partial class ResourceIdentificationTool
{
    private static Dictionary<int, ResourceIdentificationInfo> _resourceIdentificationInfoMaps_Static =
        new Dictionary<int, ResourceIdentificationInfo>();

    private static ResourceIdentificationToolSetting Setting =>
        ResourceIdentificationToolSetting.LoadSettingData<ResourceIdentificationToolSetting>();

    private const string ResourceIdentificationTypeScriptTemplate =
        @"public enum ResourceIdentificationType
{
**Content**
}
";

    [UnityEditor.MenuItem("Tools/ResourceTool/InitResourcesIdentification")]
    public static void InitResourceIdentificationInfoMapInProject()
    {
        string sharpTag = "#";
        string metaTag = ".meta";

        List<FileInfo> assetsFile = new List<FileInfo>();
        foreach (var path in Setting.CheckPaths)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            assetsFile.AddRange(directoryInfo.GetFiles($"*{sharpTag}*", SearchOption.AllDirectories).ToList());
        }

        _resourceIdentificationInfoMaps_Static.Clear();

        // 遍历所有资源并检查名称中是否包含"#"符号
        foreach (FileInfo prefabfile in assetsFile)
        {
            if (prefabfile.Name.Contains(metaTag)) continue;
            if (!prefabfile.Name.Contains(sharpTag)) continue;

            string fileName = prefabfile.Name;

            int assetID = int.Parse(GetNumbersFromString(fileName));
            string assetPath = GetPathInAsset(prefabfile.FullName);

            if (_resourceIdentificationInfoMaps_Static.ContainsKey(assetID))
            {
                Debug.LogError(
                    $"ResourceIdentificationTool日志:assetID:{assetID}, assetPath:{assetPath}, 存在相同ID，请重新设置资源ID",
                    UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath));
                return;
            }

            ResourceIdentificationInfo resourceIdentificationInfo = new ResourceIdentificationInfo()
            {
                AssetID = assetID,
                AssetPath = assetPath.Replace("Assets/", ""),
            };

            _resourceIdentificationInfoMaps_Static[resourceIdentificationInfo.AssetID] = resourceIdentificationInfo;
        }

        // 按键升序排序
        _resourceIdentificationInfoMaps_Static =
            _resourceIdentificationInfoMaps_Static.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

        InitFile();
        InitResourceIdentificationType();

        UnityEditor.EditorApplication.delayCall += () =>
        {
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
        };
    }

    private static void InitFile()
    {
        // 打开文件流以写入二进制数据
        using (FileStream fileStream = new FileStream(Setting.FilePath, FileMode.Create))
        using (BinaryWriter writer = new BinaryWriter(fileStream))
        {
            // 写入资源数量
            writer.Write(_resourceIdentificationInfoMaps_Static.Count);

            foreach (var resourceIdentificationInfoMap in _resourceIdentificationInfoMaps_Static)
            {
                ResourceIdentificationInfo resourceIdentificationInfo = resourceIdentificationInfoMap.Value;
                // 写入AssetID
                writer.Write(resourceIdentificationInfo.AssetID);
                // 写入AssetPath
                writer.Write(resourceIdentificationInfo.AssetPath);
            }
        }
    }

    private static void InitResourceIdentificationType()
    {
        string enumType = string.Empty;

        string assetTag = "Assets/";

        int index = 0;
        foreach (var resourceIdentificationInfoMap in _resourceIdentificationInfoMaps_Static)
        {
            ResourceIdentificationInfo resourceIdentificationInfo = resourceIdentificationInfoMap.Value;

            FileInfo fileInfo = new FileInfo(Path.Combine(Application.dataPath,
                resourceIdentificationInfo.AssetPath.Replace(assetTag, "")));

            string assetName = GetFileNameLettersWithoutExtensionInPath(fileInfo.Name);

            enumType +=
                $"    {assetName + resourceIdentificationInfo.AssetID} = {resourceIdentificationInfo.AssetID},";

            index++;
            if (index < _resourceIdentificationInfoMaps_Static.Count)
            {
                enumType += "\n";
            }
        }

        string temScript = ResourceIdentificationTypeScriptTemplate;
        temScript = temScript.Replace("**Content**", enumType);
        File.WriteAllText($"{Setting.ResourceIdentificationTypeScriptPath}", temScript,
            Encoding.UTF8);
    }

    public static string GetNumbersFromString(string input)
    {
        // 使用正则表达式匹配所有数字
        Regex regex = new Regex(@"#(\d+)");
        MatchCollection matches = regex.Matches(input);

        // 将匹配到的数字拼接成一个字符串
        string result = "";
        foreach (Match match in matches)
        {
            result += match.Groups[1].Value;
        }

        return result;
    }

    private static string GetFileNameLettersWithoutExtensionInPath(string filePath)
    {
        string fileName = Path.GetFileNameWithoutExtension(filePath);
        string letters = Regex.Replace(fileName, @"[^a-zA-Z]", "");
        return letters;
    }

    private static string GetPathInAsset(string fullPath)
    {
        string assetPath = fullPath.Replace("\\", "/");
        assetPath = assetPath.Substring(assetPath.IndexOf("Assets", StringComparison.Ordinal));

        return assetPath;
    }

    public static void InitInEditor()
    {
        TextAsset textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(Setting.FilePath);

        Instance.InitResourceIdentificationInfos(textAsset);
    }

    public static int GetNotSameAssetIDByRange(int startIndex)
    {
        InitInEditor();

        int temAssetID = startIndex + 1;
        foreach (var resourceIdentificationInfo in Instance._resourceIdentificationInfoMaps)
        {
            ResourceIdentificationInfo info = resourceIdentificationInfo.Value;
            int assetID = info.AssetID;

            if (assetID < startIndex + 1)
            {
                continue;
            }

            if (temAssetID != assetID)
            {
                return temAssetID;
            }

            temAssetID++;
        }

        return temAssetID;
    }
}
#endif
