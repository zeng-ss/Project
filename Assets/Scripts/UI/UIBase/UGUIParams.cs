//------------------------------------------------------------
// File : UGUIParams.cs
// Email: mailto:zhiqiang.yang
// Desc : UGUIForm参数类，用于界面交互参数传递
//------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework;
using UnityGameFramework.Runtime;

namespace CasualGame
{
    public delegate void UGUIParamsDelegate(params object[] args);

    public class UGUIParams : IReference
    {
        private Dictionary<string, object> ParamMaps = new Dictionary<string, object>();
        private Dictionary<string, UGUIParamsDelegate> ActionMaps = new Dictionary<string, UGUIParamsDelegate>();

        public GameFrameworkAction<UGuiForm> OnOpenCallback;
        public GameFrameworkAction OnCloseCallback;

        //界面Button点击事件 （string 拿个按钮被点击）
        public GameFrameworkAction<string> OnClickCallback;

        public virtual void Clear()
        {
            OnCloseCallback = null;
            OnClickCallback = null;
            OnOpenCallback = null;
            ParamMaps.Clear();
            ActionMaps.Clear();
        }

        public bool HasKey(string key)
        {
            return ParamMaps.ContainsKey(key) || ActionMaps.ContainsKey(key);
        }

        public T GetEnumParams<T>(string key)
        {
            if (ParamMaps.ContainsKey(key))
            {
                return (T)ParamMaps[key];
            }

            Log.Error("the key [{0}] not exists", key);
            return default(T);
        }

        public int GetIntParams(string key)
        {
            if (ParamMaps.ContainsKey(key)) // && ParamMaps[key] is int
            {
                return (int)ParamMaps[key];
            }

            Log.Error("the key [{0}] not exists", key);
            return 0;
        }


        public float GetFloatParams(string key)
        {
            if (ParamMaps.ContainsKey(key) && ParamMaps[key] is float)
            {
                return (float)ParamMaps[key];
            }

            Log.Error("the key [{0}] not exists", key);
            return 0;
        }

        public string GetStringParams(string key)
        {
            if (ParamMaps.ContainsKey(key) && ParamMaps[key] is string)
            {
                return (string)ParamMaps[key];
            }

            Log.Error("the key [{0}] not exists", key);
            return string.Empty;
        }

        public bool GetBoolParams(string key)
        {
            if (ParamMaps.ContainsKey(key) && ParamMaps[key] is bool)
            {
                return (bool)ParamMaps[key];
            }

            Log.Error("the key [{0}] not exists", key);
            return false;
        }

        public object GetObjectParams(string key)
        {
            if (ParamMaps.ContainsKey(key))
            {
                return ParamMaps[key];
            }

            Log.Error("the key [{0}] not exists", key);
            return null;
        }

        public T GetAnyParams<T>(string key)
        {
            if (ParamMaps.ContainsKey(key))
            {
                return (T)ParamMaps[key];
            }

            Log.Error("the key [{0}] not exists", key);
            return default(T);
        }

        public UGUIParamsDelegate GetDelegage(string key)
        {
            if (ActionMaps.ContainsKey(key))
            {
                return ActionMaps[key];
            }

            Log.Error("the key [{0}] not exists", key);
            return null;
        }

        public UGUIParams AddValue(string key, object obj)
        {
            if (ParamMaps.ContainsKey(key))
            {
                Log.Error("the key [{0}] already exists", key);
                return this;
            }

            ParamMaps.Add(key, obj);
            return this;
        }

        public UGUIParams AddDelegage(string key, UGUIParamsDelegate obj)
        {
            if (ActionMaps.ContainsKey(key))
            {
                Log.Error("the key [{0}] already exists", key);
                return this;
            }

            ActionMaps.Add(key, obj);
            return this;
        }

        public static UGUIParams Create()
        {
            var _params = ReferencePool.Acquire<UGUIParams>();
            return _params;
        }

        public static UGUIParams Create(GameFrameworkAction closeCB)
        {
            var _params = ReferencePool.Acquire<UGUIParams>();
            _params.OnCloseCallback = closeCB;
            return _params;
        }

        public static UGUIParams Create(GameFrameworkAction closeCB, GameFrameworkAction<string> onclickCB)
        {
            var _params = ReferencePool.Acquire<UGUIParams>();
            _params.OnClickCallback = onclickCB;
            _params.OnCloseCallback = closeCB;
            return _params;
        }

        public static UGUIParams Create(GameFrameworkAction closeCB, GameFrameworkAction<UGuiForm> openCallback)
        {
            var _params = ReferencePool.Acquire<UGUIParams>();
            _params.OnCloseCallback = closeCB;
            _params.OnOpenCallback = openCallback;
            return _params;
        }

        public static UGUIParams Create(GameFrameworkAction closeCB, GameFrameworkAction<UGuiForm> openCallback,
            GameFrameworkAction<string> onclickCB)
        {
            var _params = ReferencePool.Acquire<UGUIParams>();
            _params.OnCloseCallback = closeCB;
            _params.OnOpenCallback = openCallback;
            _params.OnClickCallback = onclickCB;
            return _params;
        }
    }
}