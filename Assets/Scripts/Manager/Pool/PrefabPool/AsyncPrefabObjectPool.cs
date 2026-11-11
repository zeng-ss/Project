using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Threading;
using System;
using Object = UnityEngine.Object;

namespace CasualGame
{
    public class AsyncPrefabObjectPool : AsyncObjectPoolBase<BaseUnitObject>
    {
        private const float MAX_CLEAR_POOL_TIME = 30;
        private readonly int assetId;
        private float _lastUseTime;
        private Object _asset = null;
        private BaseUnitObject _baseUnitObject = null;
        private int _activeCount = 0;
        private int _residentCount = 0;
        private readonly object _loadingLock = new object();
        private UniTaskCompletionSource<Object> loadingTcs = null;

        public AsyncPrefabObjectPool(int assetId) : base()
        {
            this.assetId = assetId;
            this.loadingTcs = null;
            this._activeCount = 0;
        }

        protected override async UniTask<BaseUnitObject> CreateInstanceAsync(CancellationToken cancellationToken, bool isPreWarm = false)
        {
            if (_baseUnitObject == null)
            {
                Object asset = await LoadAssetAsync(cancellationToken);
                if (asset == null) return null;

                if (asset is not GameObject gameObject)
                {
                    LogHandler.Error($"PrefabPool: pool_{assetId} 资源不是GameObject类型");
                    return null;
                }

                _asset = asset;
                _baseUnitObject = gameObject.GetComponent<BaseUnitObject>();
                if (_baseUnitObject == null)
                {
                    LogHandler.Error($"PrefabPool: pool_{assetId} GameObject缺少BaseUnitObject组件");
                    return null;
                }

                _residentCount = _baseUnitObject.PoolCount;
                LogHandler.Info($"PrefabPool: pool_{assetId} 资源加载完成", LogColor.Pink);
            }

            // 创建实例
            var baseUnitObject = Object.Instantiate(_baseUnitObject);
            baseUnitObject.AssetId = assetId;
            baseUnitObject.OnInit(null);
            if (!isPreWarm)
            {
                OnSpawn(baseUnitObject);
            }
            return baseUnitObject;
        }

        private async UniTask<Object> LoadAssetAsync(CancellationToken cancellationToken)
        {
            if (loadingTcs != null)
            {
                try
                {
                    var asset = await loadingTcs.Task.AttachExternalCancellation(cancellationToken);
                    return asset;
                }
                catch (OperationCanceledException)
                {
                    LogHandler.Info($"PrefabPool: pool_{assetId} 等待加载被取消", LogColor.Purple);
                    throw;
                }
                catch (System.Exception e)
                {
                    LogHandler.Error($"PrefabPool: pool_{assetId} 等待任务加载失败: {e.Message}");
                    throw;
                }
            }

            loadingTcs = new UniTaskCompletionSource<Object>();

            try
            {
                _asset = await GameEntry.Resource.LoadAsset<Object>(assetId, cancellationToken);
                loadingTcs.TrySetResult(_asset);
                return _asset;
            }
            catch (OperationCanceledException ex)
            {
                LogHandler.Info($"PrefabPool: pool_{assetId} 加载取消", LogColor.Purple);
                loadingTcs.TrySetCanceled(ex.CancellationToken);
                throw;
            }
            catch (Exception e)
            {
                LogHandler.Error($"PrefabPool: pool_{assetId} 加载失败");
                loadingTcs.TrySetException(e);
                throw;
            }
            finally
            {
                loadingTcs = null;
            }
        }

        protected override void OnSpawn(BaseUnitObject instance)
        {
            instance.gameObject.SetActive(true);
            instance.OnSpawn(null);
            _activeCount++;
            _lastUseTime = Time.realtimeSinceStartup;
        }

        protected override void OnDeSpawn(BaseUnitObject buo)
        {
            buo.gameObject.SetActive(false);
            buo.OnDeSpawn(null);
            _activeCount--;
            _lastUseTime = Time.realtimeSinceStartup;
        }

        protected override void OnPreWarmDeSpawn(BaseUnitObject buo)
        {
            buo.gameObject.SetActive(false);
            _lastUseTime = Time.realtimeSinceStartup;
        }

        protected override void OnDestroy(BaseUnitObject buo)
        {
            UnityEngine.Object.Destroy(buo.gameObject);
        }

        protected override void OnDisposed()
        {
            if (_asset != null)
            {
                GameEntry.Resource.UnloadAsset(_asset);
                _asset = null;
            }

            if (loadingTcs != null)
            {
                loadingTcs.TrySetCanceled();
                loadingTcs = null;
            }

            _baseUnitObject = null;
        }

        public void DisposeResident()
        {
            while (stack.Count > _residentCount)
            {
                if (stack.TryPop(out var obj))
                {
                    OnDestroy(obj);
                }
            }
        }

        public bool IsResident => _residentCount > 0;

        public bool NeedPurge
        {
            get
            {
                return _activeCount <= 0 && loadingTcs == null && Time.realtimeSinceStartup - _lastUseTime > MAX_CLEAR_POOL_TIME;
            }
        }
    }
}