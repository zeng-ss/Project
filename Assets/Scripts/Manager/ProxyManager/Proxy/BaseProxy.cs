//------------------------------------------------------------
// File : BaseProxy.cs
// Email: mailto:zhiqiang.yang@kingboat.io
// Desc : 数据存储基类
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityGameFramework.Runtime;

namespace CasualGame
{
    public abstract class BaseProxy<T> where T : BaseModel
    {
        protected T Data { get; private set; }
        public T ProxyData => Data;

        protected bool IsProxyNull;

        /// <summary>
        /// 数据名称
        /// </summary>
        public string ProxyName
        {
            get;
            private set;
        }

        public BaseProxy(string proxyName, object data = null)
        {
            ProxyName = proxyName;

            if (!GameEntry.Setting.HasSetting(this.ProxyName))
            {
                this.Data = Activator.CreateInstance<T>();

                IsProxyNull = true;

                SaveImmediately();
            }
            else
            {
                this.Data = GameEntry.Setting.GetObject<T>(this.ProxyName);
            }
        }

        /// <summary>
        /// 数据模型注册
        /// </summary>
        public virtual void OnRegister()
        {

        }

        public virtual void OnDataTableLoaded()
        {

        }

        public virtual void RegisterForNewVersion()
        {

        }

        /// <summary>
        /// 数据保存
        /// </summary>
        public void Save()
        {
            MEC.Timing.RunCoroutineSingleton(SaveOperation(), ProxyName, MEC.SingletonBehavior.Abort);
        }

        private IEnumerator<float> SaveOperation()
        {
            yield return MEC.Timing.WaitForSeconds(UnityEngine.Random.Range(0.5f, 1.5f));

            SaveImmediately();
        }

        public void SaveImmediately()
        {
            GameEntry.Setting.SetObject(this.ProxyName, Data);

            GameEntry.Setting.Save();
        }

        public virtual void ClearDataOnNewDay() { }
    }
}