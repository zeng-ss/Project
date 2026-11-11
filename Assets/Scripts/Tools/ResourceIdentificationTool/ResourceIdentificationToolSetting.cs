using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceIdentificationToolSetting",
    menuName = "ResourceIdentificationTool/Create Setting")]
public class ResourceIdentificationToolSetting : ScriptableObject
{
    public string FilePath = "Assets/Game/Configs/ResourcesIdentification.bytes";
    public string ResourceIdentificationTypeScriptPath =
        "Assets/Scripts/Definition/Enum/ResourceIdentificationType.cs";

    public List<string> CheckPaths = new List<string>()
    {
        "Assets/Game",
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
            string filePath = $"Assets/{settingType.Name}.asset";
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