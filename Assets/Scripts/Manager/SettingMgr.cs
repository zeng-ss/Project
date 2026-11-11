using Newtonsoft.Json;
using UnityEngine;

namespace CasualGame
{
    /// <summary>
    /// 本地存档，基于 PlayerPrefs。替代原 GameEntry.Setting。
    /// </summary>
    public class SettingMgr : Singleton<SettingMgr>
    {
        public bool HasSetting(string key) => PlayerPrefs.HasKey(key);

        public void Save() => PlayerPrefs.Save();

        public void DeleteKey(string key) => PlayerPrefs.DeleteKey(key);

        public void SetString(string key, string value) => PlayerPrefs.SetString(key, value);
        public string GetString(string key, string defaultValue = "") => PlayerPrefs.GetString(key, defaultValue);

        public void SetInt(string key, int value) => PlayerPrefs.SetInt(key, value);
        public int GetInt(string key, int defaultValue = 0) => PlayerPrefs.GetInt(key, defaultValue);

        public void SetFloat(string key, float value) => PlayerPrefs.SetFloat(key, value);
        public float GetFloat(string key, float defaultValue = 0f) => PlayerPrefs.GetFloat(key, defaultValue);

        public void SetBool(string key, bool value) => PlayerPrefs.SetInt(key, value ? 1 : 0);

        public bool GetBool(string key, bool defaultValue = false) =>
            PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;

        /// <summary>把对象序列化成 JSON 存进 PlayerPrefs。</summary>
        public void SetObject(string key, object value)
        {
            PlayerPrefs.SetString(key, JsonConvert.SerializeObject(value));
        }

        /// <summary>读回对象，不存在或解析失败返回 null。</summary>
        public T GetObject<T>(string key) where T : class
        {
            if (!PlayerPrefs.HasKey(key)) return null;

            string json = PlayerPrefs.GetString(key);
            if (string.IsNullOrEmpty(json)) return null;

            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Setting] 反序列化 {key} 失败: {e.Message}");
                return null;
            }
        }
    }
}