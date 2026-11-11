using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CasualGame
{
    public class PrefabPool : MonoSingleton<PrefabPool>
    {
        private const float _toggleTime = 5;
        private float _toggle = _toggleTime;
        private Dictionary<int, AsyncPrefabObjectPool> _asyncPrefabObjectPoolDictionary = new Dictionary<int, AsyncPrefabObjectPool>();

        public async void PrewarmAsync(int assetId, int count, CancellationToken cancellationToken = default)
        {
            AsyncPrefabObjectPool asyncPrefabObjectPool = null;

            if (!_asyncPrefabObjectPoolDictionary.TryGetValue(assetId, out asyncPrefabObjectPool))
            {
                asyncPrefabObjectPool = new AsyncPrefabObjectPool(assetId);
                _asyncPrefabObjectPoolDictionary[assetId] = asyncPrefabObjectPool;
            }

            await asyncPrefabObjectPool.PrewarmAsync(count, cancellationToken);
        }

        public async UniTask<T> Spawn<T>(int assetId, CancellationToken cancellationToken) where T : BaseUnitObject
        {
            AsyncPrefabObjectPool asyncPrefabObjectPool = null;

            if (!_asyncPrefabObjectPoolDictionary.TryGetValue(assetId, out asyncPrefabObjectPool))
            {
                asyncPrefabObjectPool = new AsyncPrefabObjectPool(assetId);
                _asyncPrefabObjectPoolDictionary[assetId] = asyncPrefabObjectPool;
            }

            var baseUnitObject = await asyncPrefabObjectPool.SpawnAsync(cancellationToken);
            if (baseUnitObject == null)
                return null;

            baseUnitObject.transform.SetParent(transform);
            return baseUnitObject as T;
        }

        public void DeSpawn(BaseUnitObject baseUnitObject)
        {
            AsyncPrefabObjectPool asyncPrefabObjectPool = null;

            if (!_asyncPrefabObjectPoolDictionary.TryGetValue(baseUnitObject.AssetId, out asyncPrefabObjectPool))
            {
                LogHandler.Error($"PrefabPool: DeSpawn 失败，找不到对象池: pool_{baseUnitObject.AssetId}");
                return;
            }

            asyncPrefabObjectPool.DeSpawn(baseUnitObject);
            baseUnitObject.transform.SetParent(transform);
        }

        private void Update()
        {
            _toggle -= Time.deltaTime;

            if (!(_toggle <= 0)) return;

            _toggle = _toggleTime;

            foreach (var item in _asyncPrefabObjectPoolDictionary)
            {
                if (!item.Value.NeedPurge) continue;

                if (item.Value.IsResident)
                {
                    item.Value.DisposeResident();
                    continue;
                }

                item.Value.Dispose();
                _asyncPrefabObjectPoolDictionary.Remove(item.Key);
                _toggle = 0;
                LogHandler.Info($"PrefabPool: 清理了对象池: pool_{item.Key}", LogColor.Pink);
                break;
            }
        }
    }

}