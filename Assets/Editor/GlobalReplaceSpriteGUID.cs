using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

//type ： 1 内置资源
//type :  2 另一个场景资源
//type :  3 Project 资源（带 GUID 的 *.asset, *.png, *.mat 等文件）
public enum UnityAssetType
{
    // m_Material: {fileID: 2100000, guid: f910c9a4405b748f3a9d79d5a6fdd04a, type: 2}
    Material,
    // m_Sprite: {fileID: 21300000, guid: 11d80b6a9282d4057a65993bb1ced86b, type: 3}
    Sprite,
    // m_Script: {fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}
    Script,
    //m_Texture: {fileID: 8900000, guid: e886fa4327966c845a816d07f5a685cd, type: 3}
    Shader,
    //m_Texture: {fileID: 8900000, guid: e886fa4327966c845a816d07f5a685cd, type: 3}
    Texture,
    //m_Mesh: {fileID: 4300000, guid: 4025265a43ca6484ebb7f38b5bcb06cb, type: 2}
    Mesh
}

//全局脚本替换逻辑
public class UnityAssetKv
{
    public UnityEngine.Object originObj;
    public UnityEngine.Object newObj;
    public UnityAssetType unityAssetType;

    public string originGuid;
    public string newGuid;
}

public class GlobalReplaceSpriteGUID : EditorWindow
{
    private UnityEngine.Object FilterPath;
    //Material
    //Script
    //Sprite
    //TODO ...
    public List<UnityAssetKv> unityAssets = new List<UnityAssetKv>();


    [MenuItem("Assets/Ripper/ReplaceGUID")]
    public static void ReplaceGUID()
    {
        GlobalReplaceSpriteGUID window = GetWindow<GlobalReplaceSpriteGUID>();
        window.Show();
    }
    private void OnGUI()
    {
        FilterPath = EditorGUILayout.ObjectField("路径:", FilterPath, typeof(UnityEngine.Object), true);
        EditorGUILayout.LabelField("收集第一个版本的所有的脚本信息", EditorStyles.boldLabel);
        int removeIndex = -1;
        // 展示已选文件夹对象，可编辑
        for (int i = 0; i < unityAssets.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            Type _type = typeof(Material);
            if (unityAssets[i].unityAssetType == UnityAssetType.Script)
            {
                _type = typeof(MonoScript);
            }
            else if (unityAssets[i].unityAssetType == UnityAssetType.Sprite)
            {
                _type = typeof(Sprite);
            }
            //TODO:

            unityAssets[i].originObj = EditorGUILayout.ObjectField("Old:", unityAssets[i].originObj, _type, true);
            unityAssets[i].newObj = EditorGUILayout.ObjectField("New", unityAssets[i].newObj, _type, true);
            unityAssets[i].unityAssetType = (UnityAssetType)EditorGUILayout.EnumPopup("资源类型:", unityAssets[i].unityAssetType);

            if (GUILayout.Button("移除", GUILayout.MaxWidth(50)))
            {
                removeIndex = i;
            }
            EditorGUILayout.EndHorizontal();
        }
        // 移除选中的文件夹
        if (removeIndex >= 0)
        {
            unityAssets.RemoveAt(removeIndex);
        }

        if (GUILayout.Button("添加资源对"))
        {
            unityAssets.Add(new UnityAssetKv());
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("替换"))
        {
            if (unityAssets.Count == 0 || FilterPath == null) return;
            for (int i = 0; i < unityAssets.Count; i++)
            {
                var assetKV = unityAssets[i];

                string path = AssetDatabase.GetAssetPath(assetKV.originObj);
                string guid = AssetDatabase.AssetPathToGUID(path);
                assetKV.originGuid = guid;

                path = AssetDatabase.GetAssetPath(assetKV.newObj);
                guid = AssetDatabase.AssetPathToGUID(path);
                assetKV.newGuid = guid;
            }

            string filterPath = AssetDatabase.GetAssetPath(FilterPath);
            GlobalReplaceGUID(unityAssets, filterPath);
        }
    }

    private void GlobalReplaceGUID(List<UnityAssetKv> _unityAssets, string filterPath)
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab t:Scene t:ScriptableObject", new string[] { filterPath });
        for (int i = 0; i < guids.Length; i++)
        {
            string asssetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            ReplaceGUID(asssetPath, _unityAssets);
        }
    }

    private void ReplaceGUID(string assetPath, List<UnityAssetKv> _unityAssets)
    {
        string input = File.ReadAllText(assetPath);

        //fileId 待商榷？？
        for (int i = 0; i < _unityAssets.Count; i++)
        {
            var kv = _unityAssets[i];
            // m_Material: {fileID: 2100000, guid: f910c9a4405b748f3a9d79d5a6fdd04a, type: 2}
            string _pattern = $"{{fileID: 2100000, guid: {kv.originGuid}, type: 2}}";
            string _beReplace = $"{{fileID: 2100000, guid: {kv.newGuid}, type: 2}}";

            //类型判断
            // if (kv.unityAssetType == UnityAssetType.Sprite)
            // {
            //     _pattern = $"{{fileID: 21300000, guid: {kv.originGuid}, type: 2}}";
            //     _beReplace = $"{{fileID: 21300000, guid: {kv.newGuid}, type: 2}}";
            // }

            input = input.Replace(_pattern, _beReplace);
        }
        File.WriteAllText(assetPath, input);
    }

    public static Dictionary<string, string> guidMap = new Dictionary<string, string>();
    public static string pattern = @"\{fileID: 21300000, guid: \s*([0-9a-f]{32}), type: (\d+)\}";
    public static Dictionary<string, List<string>> spriteNameMap = new Dictionary<string, List<string>>();


    // SpriteAtlasEditor
    [MenuItem("Assets/Ripper/ReplaceSprite/GlobalReplace")]
    public static void GlobalReplace()
    {
        spriteNameMap.Clear();
        string[] guids = AssetDatabase.FindAssets("t:Sprite", new string[] { "Assets" });//Assets/SpriteAtlas
        for (int i = 0; i < guids.Length; i++)
        {
            string spritePath = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (spritePath.EndsWith(".png"))
            {
                string fileName = Path.GetFileNameWithoutExtension(spritePath);

                if (spriteNameMap.ContainsKey(fileName))
                {
                    spriteNameMap[fileName].Add(spritePath);
                }
                else
                {
                    spriteNameMap.Add(fileName, new List<string>() { spritePath });
                }
            }
        }

        var guids1 = AssetDatabase.FindAssets("t:Prefab");
        var guids2 = AssetDatabase.FindAssets("t:Scene");
        // var guids3 = AssetDatabase.FindAssets("t:AnimationClip");
        // var guids4 = AssetDatabase.FindAssets("t:AssetProvider");

        List<string> allGuids = new List<string>();
        allGuids.AddRange(guids1);
        allGuids.AddRange(guids2);
        // allGuids.AddRange(guids3);
        // allGuids.AddRange(guids4);

        foreach (var guid in allGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            ReplaceSpriteGUID(path);
        }
    }

    [MenuItem("Assets/Ripper/ReplaceSprite/GlobalReplaceInSelected")]
    public static void GlobalReplaceInSelected()
    {
        string[] paths = Selection.assetGUIDs;
        foreach (var path in paths)
        {
            ReplaceSpriteGUID(AssetDatabase.GUIDToAssetPath(path));
        }
    }

    private static void ReplaceSpriteGUID(string path)
    {
        string input = File.ReadAllText(path);

        string output = Regex.Replace(input, pattern, match =>
         {
             string guid = match.Groups[1].Value;
             string type = match.Groups[2].Value;

             string newGuid = guid;
             if (guidMap.ContainsKey(guid))
             {
                 newGuid = guidMap[guid];
                 type = "3";
             }
             else
             {
                 string spritePath = AssetDatabase.GUIDToAssetPath(guid);
                 Sprite oldSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                 if (oldSprite != null)
                 {
                     //  Sprite newSprite = SpriteAtlasEditor.GetNewSprite(oldSprite);
                     //  if (newSprite != null)
                     //  {
                     //      newGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(newSprite));
                     //      if (!guidMap.ContainsKey(guid))
                     //      {
                     //          guidMap.Add(guid, newGuid);
                     //      }
                     //      type = "3";
                     //  }
                 }
             }

             return $"{{fileID: 21300000, guid: {newGuid}, type: {type}}}";
         });

        Debug.LogFormat("unity3d: {0}", output);

        File.WriteAllText(path, output);
    }

}
