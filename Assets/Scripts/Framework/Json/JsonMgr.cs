using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// JSON 数据管理：从 StreamingAssets 读取配置表，或把数据写到 persistentDataPath。
    /// 配合 LuBan 使用：gen.bat 生成 JSON 到 Assets/StreamingAssets，运行时用 LoadData 读取。
    /// </summary>
    public class JsonMgr : SingleTonMono<JsonMgr>
    {
        /// <summary>LuBan 生成的配置表所在子目录（相对 StreamingAssets）。</summary>
        public const string ConfigDir = "LubanConfig";

        /// <summary>
        /// 读取指定名字的 JSON（不带扩展名）。优先 StreamingAssets/LubanConfig，其次持久化目录。
        /// 例：JsonMgr.Instance.LoadData&lt;List&lt;DRItem&gt;&gt;("tbitem")
        /// </summary>
        public T LoadData<T>(string fileName, string loadPath = "") where T : new()
        {
            string path = Path.Combine(Application.streamingAssetsPath, ConfigDir, fileName + ".json");
            if (!File.Exists(path))
            {
                path = string.IsNullOrEmpty(loadPath)
                    ? Path.Combine(Application.persistentDataPath, fileName + ".json")
                    : Path.Combine(loadPath, fileName + ".json");
            }

            if (!File.Exists(path))
            {
                Debug.LogWarning($"[JsonMgr] 配置文件不存在: {fileName}.json，返回默认值。");
                return new T();
            }

            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>写入 JSON 到持久化目录（运行时存档用）。</summary>
        public void SaveData(object data, string fileName, string directPath = "")
        {
            string dir = Path.Combine(Application.persistentDataPath, directPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(Path.Combine(dir, fileName + ".json"), json);
        }
    }
}
