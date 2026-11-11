using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using CasualGame;
using Newtonsoft.Json;
using UnityEngine;
using System.IO;

public static class PlayerPrefsTool
{
    [Serializable]
    public class PlayerPrefsData
    {
        public string type;
        public string content;
    }

    public static List<string> GetAllPlayerPrefsKeys()
    {
        return AssemblyTool.GetAllFiledValues(typeof(Constant.Setting));
    }

    public static Dictionary<string, PlayerPrefsData> GetAllData()
    {
        Dictionary<string, PlayerPrefsData> playerPrefsData = new Dictionary<string, PlayerPrefsData>();

        // 获取所有的键
        List<string> keys = GetAllPlayerPrefsKeys();

        // 获取每个键对应的数据
        foreach (string key in keys)
        {
            if (!PlayerPrefs.HasKey(key)) continue;

            // 根据键的类型获取对应的数据
            if (PlayerPrefs.GetInt(key, int.MinValue) != int.MinValue)
            {
                playerPrefsData[key] = new PlayerPrefsData()
                {
                    type = "Int",
                    content = PlayerPrefs.GetInt(key).ToString(),
                };
            }
            else if (Math.Abs(PlayerPrefs.GetFloat(key, float.MinValue) - float.MinValue) > 0.0001f)
            {
                playerPrefsData[key] = new PlayerPrefsData()
                {
                    type = "Float",
                    content = PlayerPrefs.GetFloat(key).ToString(CultureInfo.InvariantCulture),
                };
            }
            else if (!string.IsNullOrEmpty(PlayerPrefs.GetString(key)))
            {
                playerPrefsData[key] = new PlayerPrefsData()
                {
                    type = "String",
                    content = PlayerPrefs.GetString(key),
                };
            }
        }

        return playerPrefsData;
    }

    // 将游戏数据转为 Json 文件
    public static string GetDataJson()
    {
        JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
        };
        // 将数据转换为 JSON 格式
        string json = JsonConvert.SerializeObject(GetAllData(), jsonSerializerSettings);
        return json;
    }

    public static string GetDataJson_Base64()
    {
        return StringConvertTool.StringToBase64(GetDataJson());
    }

    public static void LoadDataJson_Base64(string jsonBase64)
    {
        LoadDataJson(StringConvertTool.Base64ToString(jsonBase64));
    }

    public static void LoadDataJson(string json)
    {
        DeleteAllData();

        Dictionary<string, PlayerPrefsData> datas = JsonConvert.DeserializeObject<Dictionary<string, PlayerPrefsData>>(json);

        foreach (var data in datas)
        {
            string key = data.Key;
            string type = data.Value.type;
            string content = data.Value.content;

            switch (type)
            {
                case "Int":
                    {
                        if (int.TryParse(content, out int resultInt))
                        {
                            PlayerPrefs.SetInt(key, resultInt);
                        }
                        break;
                    }
                case "Float":
                    {
                        if (float.TryParse(content, out float resultFloat))
                        {
                            PlayerPrefs.SetFloat(key, resultFloat);
                        }
                        break;
                    }
                default:
                    {
                        PlayerPrefs.SetString(key, content);
                        break;
                    }
            }
        }

        PlayerPrefs.Save();
    }

    public static void DeleteAllData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        System.IO.Directory.Delete(Application.persistentDataPath, true);
        string yoopath = $"{Application.dataPath}/../yoo";
        if (Directory.Exists(yoopath))
            System.IO.Directory.Delete(yoopath, true);
    }
}
