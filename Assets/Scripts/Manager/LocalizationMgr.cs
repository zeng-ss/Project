using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CasualGame
{
    /// <summary>
    /// 多语言文本：读取 StreamingAssets/Localization/uiText_{语言}.csv。
    /// CSV 格式：第一行表头，之后每行 id|文本。
    /// </summary>
    public class LocalizationMgr : Singleton<LocalizationMgr>
    {
        private readonly Dictionary<string, string> _strings = new();

        /// <summary>当前语言名。</summary>
        public string Language { get; private set; } = "English";

        public void Load(string language = null)
        {
            Language = string.IsNullOrEmpty(language) ? DetectLanguage() : language;
            _strings.Clear();

            string dir = Path.Combine(Application.streamingAssetsPath, "Localization");
            string path = Path.Combine(dir, $"uiText_{Language}.csv");

            if (!File.Exists(path))
            {
                Debug.LogWarning($"[Localization] 找不到 {Language}.csv，回退中文。");
                Language = "ChineseSimplified";
                path = Path.Combine(dir, "uiText_ChineseSimplified.csv");
                if (!File.Exists(path))
                {
                    Debug.LogError($"[Localization] 中文表也不存在: {path}");
                    return;
                }
            }

            string[] lines = File.ReadAllLines(path);
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;

                string[] parts = lines[i].Split('|');
                if (parts.Length < 2) continue;

                _strings[parts[0].Trim()] = parts[1].Trim().Replace("\\n", "\n");
            }

            Debug.Log($"[Localization] {Language}: {_strings.Count} 条");
        }

        public string GetString(string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            return _strings.TryGetValue(key, out string value) ? value : $"<NoKey>{key}</NoKey>";
        }

        public string GetString(int key) => GetString(key.ToString());

        /// <summary>带格式参数的取文本，例：GetString("655", 3) → "3 级解锁"。</summary>
        public string GetString(string key, params object[] args)
        {
            string value = GetString(key);
            return args == null || args.Length == 0 ? value : string.Format(value, args);
        }

        /// <summary>设备语言 → csv 后缀，未支持的语言回退英文。</summary>
        private static string DetectLanguage()
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.ChineseSimplified: return "ChineseSimplified";
                case SystemLanguage.ChineseTraditional: return "ChineseTraditional";
                case SystemLanguage.Chinese: return "ChineseSimplified";
                case SystemLanguage.Japanese: return "Japanese";
                case SystemLanguage.Korean: return "Korean";
                case SystemLanguage.Russian: return "Russian";
                case SystemLanguage.French: return "French";
                case SystemLanguage.German: return "German";
                case SystemLanguage.Spanish: return "Spanish";
                case SystemLanguage.Hindi: return "Hindi";
                case SystemLanguage.Italian: return "Italian";
                case SystemLanguage.Portuguese: return "PortuguesePortugal";
                case SystemLanguage.Thai: return "Thai";
                case SystemLanguage.Turkish: return "Turkish";
                case SystemLanguage.Vietnamese: return "Vietnamese";
                case SystemLanguage.Indonesian: return "Indonesian";
                default: return "English";
            }
        }
    }
}