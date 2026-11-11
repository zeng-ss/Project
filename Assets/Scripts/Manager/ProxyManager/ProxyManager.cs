//------------------------------------------------------------
// File : ProxyManager.cs
// Email: mailto:zhiqiang.yang
// Desc : 数据模式存储管理类
//------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using MEC;
using UnityEngine;

namespace CasualGame
{
    public class ProxyManager : Singleton<ProxyManager>
    {
        private readonly ConcurrentDictionary<string, object> _proxyMap = new ConcurrentDictionary<string, object>();
        public ConcurrentDictionary<string, object> ProxyMap => _proxyMap;
        private int _currentProxyCount;
        private int _totalProxyCount;

        private Action _initSuccessAction;

        public bool IsInit => _currentProxyCount >= _totalProxyCount;

        public void InitProxy(Action initSuccessAction)
        {
            _proxyMap.Clear();

            _initSuccessAction = initSuccessAction;

            RegisterProxy(() => new UserProxy(Constant.Setting.UserProxy));
        }

        public void OnDataTableLoaded()
        {
            UserProxy.OnDataTableLoaded();
        }

        private void RegisterProxy<T>(Func<BaseProxy<T>> proxyFunc) where T : BaseModel
        {
            _totalProxyCount++;
            MEC.Timing.RunCoroutineSingleton(RegisterProxy_IE(proxyFunc), this.GetHashCode(), SingletonBehavior.Wait);
        }

        private IEnumerator<float> RegisterProxy_IE<T>(Func<BaseProxy<T>> proxyFunc) where T : BaseModel
        {
            BaseProxy<T> proxy = proxyFunc?.Invoke();

            _proxyMap[proxy.ProxyName] = proxy;

            proxy.OnRegister();

            yield return MEC.Timing.WaitForOneFrame;

            _currentProxyCount++;

            if (_currentProxyCount >= _totalProxyCount)
            {
                _initSuccessAction?.Invoke();
            }
        }

        private T GetProxy<T>()
        {
            foreach (var proxyPair in _proxyMap)
            {
                if (proxyPair.Value is T t)
                {
                    return t;
                }
            }
            return default;
        }

        public virtual void ClearDataOnNewDay()
        {
            UserProxy.ClearDataOnNewDay();
        }

        public static UserProxy UserProxy => Instance.GetProxy<UserProxy>();
    }
}