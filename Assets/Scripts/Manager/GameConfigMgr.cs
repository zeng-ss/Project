using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace CasualGame
{
    /// <summary>
    /// 全局数值配置：读取 StreamingAssets/Configs/GlobalConfig.csv。
    /// CSV 格式：|Id|Key|Value|，以 # 开头的行是表头/注释。
    /// </summary>
    public class GameConfigMgr : Singleton<GameConfigMgr>
    {
        private readonly Dictionary<string, string> _values = new Dictionary<string, string>();

        public void Load()
        {
            _values.Clear();

            string path = Path.Combine(Application.streamingAssetsPath, "Configs", "GlobalConfig.csv");
            if (!File.Exists(path))
            {
                Debug.LogError($"[GameConfig] 配置文件不存在: {path}");
                return;
            }

            foreach (string line in File.ReadAllLines(path))
            {
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#")) continue;

                string[] parts = line.Trim().Trim('|').Split('|');
                if (parts.Length < 3) continue;

                _values[parts[1].Trim()] = parts[2].Trim();
            }

            Debug.Log($"[GameConfig] {_values.Count} 项");
        }

        public bool HasKey(string key) => _values.ContainsKey(key);

        public string GetString(string key, string defaultValue = "")
        {
            return _values.GetValueOrDefault(key, defaultValue);
        }

        public int GetInt(string key, int defaultValue = 0)
        {
            return _values.TryGetValue(key, out string value) && int.TryParse(value, out int result)
                ? result
                : defaultValue;
        }

        public float GetFloat(string key, float defaultValue = 0f)
        {
            return _values.TryGetValue(key, out string value) &&
                   float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result)
                ? result
                : defaultValue;
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            return _values.TryGetValue(key, out string value) && bool.TryParse(value, out bool result)
                ? result
                : defaultValue;
        }
    }
}