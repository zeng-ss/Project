using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;
using System.Text.RegularExpressions;

public class MaskImageAndRenameTool : EditorWindow
{
    private string folderPath = "Assets/Game/Prefabs";
    private string maskNodeName = "Mask"; // 按名称查找的节点名
    private const byte TargetAlpha = 150;

    [MenuItem("Tools/Mask节点子物体Image设为黑色半透明 + 预制体重命名")]
    public static void ShowWindow()
    {
        GetWindow<MaskImageAndRenameTool>("Mask 节点处理工具");
    }

    private void OnGUI()
    {
        GUILayout.Label("1. 名称=\"Mask\" 节点的所有子物体 Image → 黑色 (Alpha=150)\n2. 预制体名去掉 #xxxx 后缀", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        folderPath = EditorGUILayout.TextField("总文件夹", folderPath);
        if (GUILayout.Button("选择", GUILayout.Width(60)))
        {
            string selected = EditorUtility.OpenFolderPanel("选择总文件夹", folderPath, "");
            if (!string.IsNullOrEmpty(selected))
            {
                if (selected.StartsWith(Application.dataPath))
                    folderPath = "Assets" + selected.Substring(Application.dataPath.Length);
                else
                    folderPath = selected;
            }
        }

        EditorGUILayout.EndHorizontal();

        maskNodeName = EditorGUILayout.TextField("Mask 节点名称", maskNodeName);

        EditorGUILayout.Space();
        if (GUILayout.Button("开始处理", GUILayout.Height(30)))
        {
            ProcessAll();
        }
    }

    private void ProcessAll()
    {
        if (!Directory.Exists(folderPath))
        {
            EditorUtility.DisplayDialog("错误", $"文件夹不存在：{folderPath}", "确定");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });
        if (guids.Length == 0)
        {
            EditorUtility.DisplayDialog("提示", "该文件夹下没有找到预制体。", "确定");
            return;
        }

        int total = guids.Length;
        int colorModified = 0;
        int renamed = 0;
        int renameFailed = 0;

        try
        {
            for (int i = 0; i < total; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                EditorUtility.DisplayProgressBar("处理预制体", path, (float)i / total);

                // ---------- 1. 改名称="Mask"节点的所有子物体 Image 颜色 ----------
                bool modified = SetMaskChildImageColor(path);
                if (modified) colorModified++;

                // ---------- 2. 重命名预制体：去掉 #xxxx ----------
                string fileName = Path.GetFileNameWithoutExtension(path);
                string newName = Regex.Replace(fileName, "#\\d+$", "");

                if (newName != fileName && !string.IsNullOrEmpty(newName))
                {
                    string dir = Path.GetDirectoryName(path).Replace("\\", "/");
                    string newPath = $"{dir}/{newName}.prefab";

                    if (AssetDatabase.LoadAssetAtPath<GameObject>(newPath) != null)
                    {
                        Debug.LogWarning($"[跳过重命名] 目标已存在：{newPath}（源：{path}）");
                        renameFailed++;
                    }
                    else
                    {
                        string error = AssetDatabase.RenameAsset(path, newName);
                        if (string.IsNullOrEmpty(error))
                            renamed++;
                        else
                        {
                            Debug.LogWarning($"[重命名失败] {path} → {newName}：{error}");
                            renameFailed++;
                        }
                    }
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        EditorUtility.DisplayDialog("完成",
            $"共扫描 {total} 个预制体\n" +
            $"修改颜色：{colorModified} 个\n" +
            $"重命名成功：{renamed} 个\n" +
            $"重命名失败/跳过：{renameFailed} 个",
            "确定");
    }

    /// <summary>
    /// 找到预制体内名称等于 maskNodeName 的节点，把其所有后代（含自身？不含）的 Image 设为黑色 Alpha=150
    /// </summary>
    private bool SetMaskChildImageColor(string prefabPath)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
        bool modified = false;

        try
        {
            // 遍历整棵层级，按名称匹配
            Transform[] all = root.GetComponentsInChildren<Transform>(true);
            foreach (var t in all)
            {
                if (t.name != maskNodeName) continue;

                // 该节点下所有后代（包含未激活）的 Image
                Image[] images = t.GetComponentsInChildren<Image>(true);
                foreach (var img in images)
                {
                    // 如果 Mask 节点自身也有 Image，不想改的话可加这个判断：
                    // if (img.transform == t) continue;

                    img.color = new Color(0f, 0f, 0f, TargetAlpha / 255f);
                    modified = true;
                }
            }

            if (modified)
            {
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }

        return modified;
    }
}