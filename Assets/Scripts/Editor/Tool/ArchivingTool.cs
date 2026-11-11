//------------------------------------------------------------
// File : LMTools.cs
// Email: mailto:zhiqiang.yang
// Desc : 
//------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using GameFramework;
using System;
using System.Globalization;
using System.Reflection;
using System.Text;
using CasualGame;

public class ArchivingTool : EditorWindow
{
    bool reviewVersion;

    [MenuItem("Tools/存档工具")]
    public static void OpenWindow()
    {
        ArchivingTool window = EditorWindow.GetWindow(typeof(ArchivingTool)) as ArchivingTool;

        if (window == null) return;

        window.Show();
    }

    private const string ArchivingPath = "Assets/Editor/Archiving";
    private Vector2 _scrollViewPosition;
    private string _archivingName = String.Empty;
    private static List<FileInfo> _fileInfoList = new List<FileInfo>();
    private int _year, _month, _day, _hour, _minute, _second;

    private static void GetFiles(DirectoryInfo directory, string pattern, ref List<FileInfo> fileList)
    {
        if (directory == null || !directory.Exists || string.IsNullOrEmpty(pattern)) return;

        foreach (FileInfo info in directory.GetFiles(pattern))
        {
            string path = info.FullName;
            fileList.Add(
                new FileInfo(path.Substring(path.IndexOf("Assets/",
                    StringComparison.Ordinal))));
        }

        foreach (DirectoryInfo info in directory.GetDirectories())
        {
            GetFiles(info, pattern, ref fileList);
        }
    }

    private void OnEnable()
    {
        RefreshUI();
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();

        var lastReviewVersion = reviewVersion;
        GUILayout.Label("是否审核版");
        reviewVersion = EditorGUILayout.Toggle("", reviewVersion, GUILayout.Width(20));
        if (lastReviewVersion != reviewVersion)
        {
            UnityEditor.EditorPrefs.SetBool("REVIEWVERSION", reviewVersion);
        }

        if (GUILayout.Button("刷新"))
        {
            RefreshUI();
        }

        if (GUILayout.Button("跳转"))
        {
            UnityEngine.Object obj = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(ArchivingPath);
            AssetDatabase.OpenAsset(obj);
        }

        if (GUILayout.Button("清空本地数据"))
        {
            PlayerPrefsTool.DeleteAllData();
        }

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        _year = EditorGUILayout.IntField(_year, GUILayout.Width(40));
        EditorGUILayout.LabelField("/", GUILayout.Width(10));
        _month = EditorGUILayout.IntField(_month, GUILayout.Width(20));
        EditorGUILayout.LabelField("/", GUILayout.Width(10));
        _day = EditorGUILayout.IntField(_day, GUILayout.Width(20));
        EditorGUILayout.Space(10, false);
        _hour = EditorGUILayout.IntField(_hour, GUILayout.Width(20));
        EditorGUILayout.LabelField(":", GUILayout.Width(10));
        _minute = EditorGUILayout.IntField(_minute, GUILayout.Width(20));
        EditorGUILayout.LabelField(":", GUILayout.Width(10));
        _second = EditorGUILayout.IntField(_second, GUILayout.Width(20));

        if (GUILayout.Button("加一天"))
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(TimeUtils.NowTimeStamp);
            DateTime dateTime = dateTimeOffset.ToOffset(TimeSpan.FromHours(+8)).DateTime;
            dateTime = dateTime.AddDays(1);

            _year = dateTime.Year;
            _month = dateTime.Month;
            _day = dateTime.Day;
            _hour = dateTime.Hour;
            _minute = dateTime.Minute;
            _second = dateTime.Second;


            dateTimeOffset = new DateTimeOffset(dateTime);
            long timeStamp = dateTimeOffset.ToUnixTimeMilliseconds();
            // if playing
            GameManager.Instance.pastTimeMillTime = timeStamp;
            Custom.Activity.ActivityManager.Instance.CheckActivity();
            // //自定义时间
            UIManager.Toast("Use user-defined Time");
        }

        if (GUILayout.Button("设置"))
        {
            DateTime dateTime = new DateTime(_year, _month, _day, _hour, _minute, _second);
            DateTimeOffset dateTimeOffset = new DateTimeOffset(dateTime);
            long timeStamp = dateTimeOffset.ToUnixTimeMilliseconds();
            // if playing
            GameManager.Instance.pastTimeMillTime = timeStamp;
            Custom.Activity.ActivityManager.Instance.CheckActivity();
            // //自定义时间
            UIManager.Toast("Use user-defined Time");
        }

        if (GUILayout.Button("同步"))
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(TimeUtils.NowTimeStamp);
            DateTime dateTime = dateTimeOffset.ToOffset(TimeSpan.FromHours(+8)).DateTime;
            _year = dateTime.Year;
            _month = dateTime.Month;
            _day = dateTime.Day;
            _hour = dateTime.Hour;
            _minute = dateTime.Minute;
            _second = dateTime.Second;
        }

        if (GUILayout.Button("重置"))
        {
            // if playing
        }

        GUILayout.EndHorizontal();
        GUILayout.Space(10);

        _scrollViewPosition = EditorGUILayout.BeginScrollView(_scrollViewPosition);
        foreach (var fileInfo in _fileInfoList)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextArea(fileInfo.Name, GUILayout.Height(30));
            EditorGUILayout.LabelField($"{fileInfo.LastWriteTime.ToString(CultureInfo.InvariantCulture)}",
                GUILayout.Height(30), GUILayout.Width(125));

            if (GUILayout.Button("Load", GUILayout.Height(30), GUILayout.Width(40)))
            {
                LoadArchiving(fileInfo);
            }

            if (GUILayout.Button("导出", GUILayout.Height(30), GUILayout.Width(40)))
            {
                TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(GetAssetPath(fileInfo.FullName));

                string dataJson = textAsset.text;

                // 将JSON字符串转换为字节数组
                byte[] jsonBytes = Encoding.UTF8.GetBytes(dataJson);

                // 将字节数组转换为Base64编码的字符串
                string base64String = Convert.ToBase64String(jsonBytes);

                GUIUtility.systemCopyBuffer = base64String;
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();


        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("名称:", GUILayout.Height(30), GUILayout.Width(30));
        _archivingName = EditorGUILayout.TextField(_archivingName, GUILayout.Height(30));
        EditorGUILayout.Space();

        if (GUILayout.Button("Save", GUILayout.Height(30)))
        {
            if (!string.IsNullOrEmpty(_archivingName))
            {
                SaveArchiving();
            }
        }

        if (GUILayout.Button("导入", GUILayout.Height(30)))
        {
            if (!string.IsNullOrEmpty(GUIUtility.systemCopyBuffer))
            {
                PlayerPrefsTool.LoadDataJson_Base64(GUIUtility.systemCopyBuffer);
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    private void LoadArchiving(FileInfo fileInfo)
    {
        TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(GetAssetPath(fileInfo.FullName));

        PlayerPrefsTool.LoadDataJson(textAsset.text);
    }

    private static string GetAssetPath(string fullPath)
    {
        int index = fullPath.IndexOf("Assets/", StringComparison.Ordinal);
        return fullPath.Substring(index, fullPath.Length - index);
    }

    private void SaveArchiving()
    {
        string dataJson = PlayerPrefsTool.GetDataJson();

        File.WriteAllText($"{ArchivingPath}/{_archivingName}.json", dataJson,
            Encoding.UTF8);

        AssetDatabase.Refresh();

        RefreshUI();
    }

    private void RefreshUI()
    {
        _fileInfoList.Clear();
        GetFiles(new DirectoryInfo(ArchivingPath), "*.json",
            ref _fileInfoList);
        _fileInfoList.Sort((a, b) => { return a.LastWriteTime.CompareTo(b.LastWriteTime); });
    }
}