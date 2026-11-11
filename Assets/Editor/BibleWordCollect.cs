using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class BibleWord
{
    public string LanguageCode;
    public string CategoryId;
    public string[] HiddenWords;
}

public static class BibleWordCollect
{
    public static string path = "Assets/Editor/Archiving";

    [MenuItem("Tools/Bible/Collect")]
    static void Collect()
    {
        // 递归遍历path下所有的文件，不包括.meta
        string targetPath = path;
        string[] files = System.IO.Directory.GetFiles(targetPath, "*.*", System.IO.SearchOption.AllDirectories);

        HashSet<string> wordsSet = new HashSet<string>();
        foreach (var file in files)
        {
            if (file.EndsWith(".json"))
            {
                List<string> words = GetAllWords(file);
                foreach (var word in words)
                {
                    string _w = word.ToUpper();
                    if (!wordsSet.Contains(_w))
                        wordsSet.Add(_w);
                }
            }
        }

        StringBuilder stringBuilder = new StringBuilder();
        foreach (var word in wordsSet)
        {
            if (word.Length >= 3 && word.Length <= 7)
                stringBuilder.AppendLine(word);
        }
        // Debug.Log(stringBuilder.ToString());

        System.IO.File.WriteAllText(Application.persistentDataPath + "/words.txt", stringBuilder.ToString());
    }


    private static List<string> GetAllWords(string filePath)
    {
        string jsonText = System.IO.File.ReadAllText(filePath);
        BibleWord word = JsonUtility.FromJson<BibleWord>(jsonText);
        List<string> words = word.HiddenWords.ToList();
        words.Add(word.CategoryId);
        return words;
    }

    // [MenuItem("Tools/Reference")]
    // private static void GetAssetReferences()
    // {
    //     // 获取选择的文件被哪些资源引用
    //     // 打印哪些资源（如prefab, mat, asset等）中包含了选中的asset
    //     UnityEngine.Object[] selection = Selection.objects;
    //     if (selection == null || selection.Length == 0)
    //     {
    //         Debug.LogWarning("请在Project视图先选中文件！");
    //         return;
    //     }

    //     string[] selectedAssetPaths = selection.Select(obj => AssetDatabase.GetAssetPath(obj)).ToArray();
    //     string searchInAssets = "Assets"; // 可以换成 "Assets/Prefab" 或其他

    //     // 可以自定义需要查找哪些类型的资源
    //     string[] searchTypes = new string[] { "*.prefab", "*.unity", "*.mat", "*.asset", "*.controller", "*.playable", "*.anim" };

    //     // 收集所有资源文件
    //     List<string> allAssets = new List<string>();
    //     foreach (var type in searchTypes)
    //     {
    //         var found = System.IO.Directory.GetFiles(searchInAssets, type, System.IO.SearchOption.AllDirectories)
    //             .Where(path => !path.EndsWith(".meta")).ToArray();
    //         allAssets.AddRange(found);
    //     }

    //     // 统计引用情况
    //     Dictionary<string, List<string>> refMap = new Dictionary<string, List<string>>();
    //     foreach (var selPath in selectedAssetPaths)
    //     {
    //         refMap[selPath] = new List<string>();
    //     }

    //     int total = allAssets.Count;
    //     for (int i = 0; i < total; ++i)
    //     {
    //         string assetPath = allAssets[i];
    //         string guid = AssetDatabase.AssetPathToGUID(assetPath);

    //         // 异步可改为EditorCoroutine如有大量资源
    //         string raw = System.IO.File.ReadAllText(assetPath);

    //         foreach (var selPath in selectedAssetPaths)
    //         {
    //             string selGuid = AssetDatabase.AssetPathToGUID(selPath);
    //             if (raw.Contains(selGuid))
    //             {
    //                 refMap[selPath].Add(assetPath);
    //             }
    //         }
    //     }

    //     foreach (var kv in refMap)
    //     {
    //         Debug.Log($"被引用资源: {kv.Key}");
    //         if (kv.Value.Count == 0)
    //         {
    //             Debug.Log("    无引用。");
    //         }
    //         else
    //         {
    //             foreach (var s in kv.Value)
    //             {
    //                 Debug.Log("    引用它的资源：" + s);
    //             }
    //         }
    //     }
    // }


    [MenuItem("Assets/Word Export")]
    private static void WordExport()
    {
        string path = $"{Application.dataPath}/Editor/Word.txt";


        //|3|default|ABOVE
        var allline = System.IO.File.ReadAllText(path).Split("\n");

        StringBuilder stringBuilder = new StringBuilder();

        List<string> allWorld = new List<string>(3000);

        HashSet<string> wordSet = new HashSet<string>();
        for (int i = 0; i < allline.Length; i++)
        {
            var line = allline[i];

            // var split = line.Split('|');

            var word = line.Trim();
            word = word.ToUpper();
            word = word.Replace(" ", "");
            if (word.Length >= 3 && word.Length <= 7)
            {
                // Ensure the word consists only of uppercase A-Z letters
                bool isAllUppercaseAZ = word.All(c => c >= 'A' && c <= 'Z');
                if (!isAllUppercaseAZ)
                {
                    continue;
                }

                if (!wordSet.Contains(word))
                {
                    allWorld.Add(word);
                    wordSet.Add(word);
                }
            }
        }

        // Randomly shuffle the allWorld list
        ShuffleList(allWorld);


        foreach (var word in allWorld)
        {
            stringBuilder.AppendLine(word);
        }

        System.IO.File.WriteAllText($"{Application.dataPath}/../../Doc/DictionaryEN.txt", stringBuilder.ToString());

    }

    private static void ShuffleList<T>(List<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }


}



