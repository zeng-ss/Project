//------------------------------------------------------------
// File : UGUIParams.cs
// Desc : UGUIForm 参数类，用于界面间传参与回调
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

namespace CasualGame
{
    public delegate void UGUIParamsDelegate(params object[] args);

    public class UGUIParams
    {
        private readonly Dictionary<string, object> ParamMaps = new Dictionary<string, object>();
        private readonly Dictionary<string, UGUIParamsDelegate> ActionMaps = new Dictionary<string, UGUIParamsDelegate>();

        public Action<UGuiForm> OnOpenCallback;
        public Action OnCloseCallback;

        /// <summary>界面按钮点击事件（参数是被点击按钮的 key）。</summary>
        public Action<string> OnClickCallback;

        public void Clear()
        {
            OnCloseCallback = null;
            OnClickCallback = null;
            OnOpenCallback = null;
            ParamMaps.Clear();
            ActionMaps.Clear();
        }

        public bool HasKey(string key) => ParamMaps.ContainsKey(key) || ActionMaps.ContainsKey(key);

        public T GetEnumParams<T>(string key) => GetAnyParams<T>(key);

        public int GetIntParams(string key) => GetAnyParams<int>(key);

        public float GetFloatParams(string key) => GetAnyParams<float>(key);

        public string GetStringParams(string key) => GetAnyParams<string>(key) ?? string.Empty;

        public bool GetBoolParams(string key) => GetAnyParams<bool>(key);

        public object GetObjectParams(string key)
        {
            if (ParamMaps.TryGetValue(key, out object value)) return value;

            Debug.LogError($"[UGUIParams] 参数不存在: {key}");
            return null;
        }

        public T GetAnyParams<T>(string key)
        {
            if (ParamMaps.TryGetValue(key, out object value))
            {
                if (value is T typed) return typed;

                Debug.LogError($"[UGUIParams] 参数 {key} 类型不匹配，期望 {typeof(T).Name}");
                return default;
            }

            Debug.LogError($"[UGUIParams] 参数不存在: {key}");
            return default;
        }

        public UGUIParamsDelegate GetDelegage(string key)
        {
            if (ActionMaps.TryGetValue(key, out UGUIParamsDelegate action)) return action;

            Debug.LogError($"[UGUIParams] 回调不存在: {key}");
            return null;
        }

        public UGUIParams AddValue(string key, object obj)
        {
            if (ParamMaps.ContainsKey(key))
            {
                Debug.LogError($"[UGUIParams] 参数已存在: {key}");
                return this;
            }

            ParamMaps.Add(key, obj);
            return this;
        }

        public UGUIParams AddDelegage(string key, UGUIParamsDelegate obj)
        {
            if (ActionMaps.ContainsKey(key))
            {
                Debug.LogError($"[UGUIParams] 回调已存在: {key}");
                return this;
            }

            ActionMaps.Add(key, obj);
            return this;
        }

        public static UGUIParams Create() => new UGUIParams();

        public static UGUIParams Create(Action closeCallback)
        {
            return new UGUIParams { OnCloseCallback = closeCallback };
        }

        public static UGUIParams Create(Action closeCallback, Action<string> clickCallback)
        {
            return new UGUIParams { OnCloseCallback = closeCallback, OnClickCallback = clickCallback };
        }

        public static UGUIParams Create(Action closeCallback, Action<UGuiForm> openCallback)
        {
            return new UGUIParams { OnCloseCallback = closeCallback, OnOpenCallback = openCallback };
        }

        public static UGUIParams Create(Action closeCallback, Action<UGuiForm> openCallback,
            Action<string> clickCallback)
        {
            return new UGUIParams
            {
                OnCloseCallback = closeCallback,
                OnOpenCallback = openCallback,
                OnClickCallback = clickCallback
            };
        }
    }
}
