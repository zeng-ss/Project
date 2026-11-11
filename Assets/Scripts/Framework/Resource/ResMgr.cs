using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Framework
{
    /// <summary>
    /// 基于 Addressables 的资源管理器（本地加载模式，暂不接热更新）。
    /// 资源地址 = 资源路径（Assets/...），由 Addressables 分组里的 Address 决定。
    /// </summary>
    public class ResMgr
    {
        private readonly Dictionary<string, AsyncOperationHandle> _handles = new();
        private readonly Dictionary<string, Object> _cache = new();

        /// <summary>资源 → 地址，UnloadAsset(asset) 时反查用。</summary>
        private readonly Dictionary<Object, string> _assetToAddress = new();

        #region 异步加载（UniTask）

        /// <summary>
        /// 把「资源路径」归一化成 Addressables 地址：Address 现在用文件名（不含扩展名）。
        /// 例：Assets/Game/Prefabs/UIMainForm/UIMainForm#10001.prefab → UIMainForm#10001
        /// 已经是纯名字（不含 /）时原样返回。
        /// </summary>
        private static string Normalize(string address)
        {
            if (string.IsNullOrEmpty(address)) return address;
            if (address.IndexOf('/') < 0 && address.IndexOf('\\') < 0) return address;

            string fileName = address.Substring(address.LastIndexOfAny(new[] { '/', '\\' }) + 1);
            int dot = fileName.LastIndexOf('.');
            return dot > 0 ? fileName.Substring(0, dot) : fileName;
        }

        /// <summary>加载资源。T 为组件类型时返回 GameObject 上的该组件。</summary>
        public async UniTask<T> LoadAsset<T>(string address, CancellationToken cancellationToken = default)
            where T : Object
        {
            address = Normalize(address);

            if (string.IsNullOrEmpty(address))
            {
                Debug.LogError("[ResMgr] 资源地址为空");
                return null;
            }

            if (_cache.TryGetValue(address, out Object cached) && cached is T cachedTyped)
            {
                return cachedTyped;
            }

            // 组件类型：先按 GameObject 加载，再取组件
            if (typeof(Component).IsAssignableFrom(typeof(T)))
            {
                GameObject prefab = await LoadAsset<GameObject>(address, cancellationToken);
                return prefab == null ? null : prefab.GetComponent<T>();
            }

            AsyncOperationHandle<T> handle = GetOrCreateHandle<T>(address);
            T result = await handle.ToUniTask(cancellationToken: cancellationToken);

            if (result == null)
            {
                Debug.LogError($"[ResMgr] 加载失败: {address} - {handle.OperationException}");
                return null;
            }

            _cache[address] = result;
            _assetToAddress[result] = address;
            return result;
        }

        /// <summary>按资源ID加载（ID → 路径 由 ResourceIdentificationTool 提供）。</summary>
        public UniTask<T> LoadAssetById<T>(int assetId, CancellationToken cancellationToken = default)
            where T : Object
        {
            string address = ResourceIdentificationTool.Instance.GetAssetPathById(assetId);
            return LoadAsset<T>(address, cancellationToken);
        }

        /// <summary>加载并实例化预制体。</summary>
        public async UniTask<T> LoadAndInstance<T>(string address, Transform parent = null,
            CancellationToken cancellationToken = default) where T : Component
        {
            GameObject prefab = await LoadAsset<GameObject>(address, cancellationToken);
            if (prefab == null) return null;

            GameObject instance = Object.Instantiate(prefab, parent);
            instance.name = prefab.name;
            return instance.GetComponent<T>();
        }

        /// <summary>按资源ID加载并实例化。</summary>
        public UniTask<T> LoadAndInstanceById<T>(int assetId, Transform parent = null,
            CancellationToken cancellationToken = default) where T : Component
        {
            string address = ResourceIdentificationTool.Instance.GetAssetPathById(assetId);
            return LoadAndInstance<T>(address, parent, cancellationToken);
        }

        /// <summary>回调版加载（给不方便 await 的地方用）。</summary>
        public void LoadAssetAsync<T>(string address, Action<T> onComplete) where T : Object
        {
            LoadAsset<T>(address).ContinueWith(onComplete).Forget();
        }

        /// <summary>回调版加载并实例化。</summary>
        public void LoadAndInstantiateAsync(string address, Transform parent = null,
            Action<GameObject> onComplete = null)
        {
            LoadAndInstance<Transform>(address, parent).ContinueWith(t =>
                onComplete?.Invoke(t == null ? null : t.gameObject)).Forget();
        }

        public void LoadSpriteAsync(string address, Action<Sprite> onComplete)
        {
            LoadAsset<Sprite>(address).ContinueWith(onComplete).Forget();
        }

        #endregion

        #region 释放

        /// <summary>按资源对象释放（与原 Framework.GameRoot.Res.UnloadAsset 对应）。</summary>
        public void UnloadAsset(Object asset)
        {
            if (asset == null) return;
            if (!_assetToAddress.TryGetValue(asset, out string address)) return;

            ReleaseAsset(address);
        }

        /// <summary>按地址释放。</summary>
        public void ReleaseAsset(string address)
        {
            address = Normalize(address);
            if (string.IsNullOrEmpty(address)) return;

            if (_handles.TryGetValue(address, out AsyncOperationHandle handle))
            {
                if (handle.IsValid()) Addressables.Release(handle);
                _handles.Remove(address);
            }

            if (_cache.TryGetValue(address, out Object asset))
            {
                _assetToAddress.Remove(asset);
                _cache.Remove(address);
            }
        }

        public void ReleaseAll()
        {
            foreach (AsyncOperationHandle handle in _handles.Values)
            {
                if (handle.IsValid()) Addressables.Release(handle);
            }

            _handles.Clear();
            _cache.Clear();
            _assetToAddress.Clear();
        }

        #endregion

        private AsyncOperationHandle<T> GetOrCreateHandle<T>(string address) where T : Object
        {
            if (_handles.TryGetValue(address, out AsyncOperationHandle existing) && existing.IsValid())
            {
                return existing.Convert<T>();
            }

            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(address);
            _handles[address] = handle;
            return handle;
        }
    }
}