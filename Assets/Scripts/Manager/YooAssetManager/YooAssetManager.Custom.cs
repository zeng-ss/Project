using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework.Resource;
using CasualGame;
using UnityEngine;
using UnityEngine.SceneManagement;
using YooAsset;
using Cysharp.Threading.Tasks;
using System.IO;
using HorstRill;

public partial class YooAssetManager
{
    public const string DefaultPackage = "DefaultPackage";
    public const string UncompressedPackage = "UncompressedPackage";

    private InitResourcesCompleteCallback _initResourcesCompleteCallback;

    private Dictionary<object, List<AssetHandle>> _assetHandleListDictionary =
        new Dictionary<object, List<AssetHandle>>();
    private Dictionary<string, SceneHandle> _sceneHandleListDictionary =
        new Dictionary<string, SceneHandle>();

    private bool _isUnloadUnusedAssetsNow = false;
    private EPlayMode _ePlayMode = EPlayMode.EditorSimulateMode;
    private const string PACKAGE_VERSION = "2025-06-30-248";

    private RemoteServices _currentRemoteServices;

    private async UniTaskVoid InitYooAsset()
    {
        // 初始化资源系统
        YooAssets.Initialize();

        await InitDefaultPackage(YooAssetManagerInitSuccess).ToUniTask();
    }

    private IEnumerator InitDefaultPackage(Action initSuccessAction)
    {
        string packageName = DefaultPackage;

        // 创建默认的资源包
        ResourcePackage package = YooAssets.CreatePackage(packageName);

        // 设置该资源包为默认的资源包，可以使用YooAssets相关加载接口加载该资源包内容。
        YooAssets.SetDefaultPackage(package);

        if (GameEntry.Base.EditorResourceMode)
        {
            yield return InitPackage(package, GetInitializeParameters(EPlayMode.EditorSimulateMode, packageName), initSuccessAction);
        }
        else
        {
            switch (GameEntry.Resource.ResourceMode)
            {
                case ResourceMode.Package:
                {
                    yield return InitPackage(package, GetInitializeParameters(EPlayMode.OfflinePlayMode, packageName), initSuccessAction);
                    break;
                }
                case ResourceMode.Updatable:
                {
                    yield return InitPackage(package, GetInitializeParameters(EPlayMode.HostPlayMode, packageName), initSuccessAction);
                    break;
                }
                case ResourceMode.WebPlay:
                {
                    yield return InitPackage(package, GetInitializeParameters(EPlayMode.WebPlayMode, packageName), initSuccessAction);
                    break;
                }
            }
        }
    }

    private InitializeParameters GetInitializeParameters(EPlayMode ePlayMode, string packageName)
    {
        _ePlayMode = ePlayMode;

        switch (ePlayMode)
        {
            case EPlayMode.EditorSimulateMode:
            {
                var buildResult = EditorSimulateModeHelper.SimulateBuild(packageName);
                var packageRoot = buildResult.PackageRootDirectory;
                var editorFileSystemParams = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
                var initParameters = new EditorSimulateModeParameters();
                initParameters.EditorFileSystemParameters = editorFileSystemParams;

                return initParameters;
            }
            case EPlayMode.OfflinePlayMode:
            {
                var buildinFileSystemParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
                var initParameters = new OfflinePlayModeParameters();
                initParameters.BuildinFileSystemParameters = buildinFileSystemParams;

                return initParameters;
            }
            case EPlayMode.HostPlayMode:
            {
                string defaultHostServer = $"{BaseURL.URL}/{PACKAGE_VERSION}";
                string fallbackHostServer = defaultHostServer;
                _currentRemoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
                var cacheFileSystemParams = FileSystemParameters.CreateDefaultCacheFileSystemParameters(_currentRemoteServices);
                var buildinFileSystemParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();

                var initParameters = new HostPlayModeParameters();
                initParameters.BuildinFileSystemParameters = buildinFileSystemParams;
                initParameters.CacheFileSystemParameters = cacheFileSystemParams;

                return initParameters;
            }
            case EPlayMode.WebPlayMode:
            {
                string url = "http://localhost:8000/StreamingAssets/yoo/DefaultPackage";
                //string url = HorstRillConfig.GetHorstRillConfig().baseUrl + "/StreamingAssets/yoo/DefaultPackage";

                IRemoteServices remoteServices = new RemoteServices(url, url);
#if UNITY_EDITOR
                var webRemoteFileSystemParams = FileSystemParameters.CreateDefaultWebRemoteFileSystemParameters(remoteServices, disableUnityWebCache: true); //支持跨域下载
#else
                var webRemoteFileSystemParams = FileSystemParameters.CreateDefaultWebRemoteFileSystemParameters(remoteServices); //支持跨域下载
#endif
                var initParameters = new WebPlayModeParameters();
                initParameters.WebRemoteFileSystemParameters = webRemoteFileSystemParams;
                initParameters.BundleLoadingMaxConcurrency = 12;
                return initParameters;
            }
        }

        return null;
    }

    /// <summary>
    /// 远端资源地址查询服务类
    /// </summary>
    private class RemoteServices : IRemoteServices
    {
        private string _defaultHostServer;
        private readonly string _fallbackHostServer;
        private readonly string _originalDefaultHostServer;

        public RemoteServices(string defaultHostServer, string fallbackHostServer)
        {
            _defaultHostServer = defaultHostServer;
            _fallbackHostServer = fallbackHostServer;
            _originalDefaultHostServer = defaultHostServer;
        }

        /// <summary>
        /// 切换到本地服务器地址
        /// </summary>
        /// <param name="localHostServer">本地服务器地址</param>
        public void SwitchToLocalServer(string localHostServer)
        {
            _defaultHostServer = localHostServer;
        }

        /// <summary>
        /// 恢复到原始服务器地址
        /// </summary>
        public void RestoreOriginalServer()
        {
            _defaultHostServer = _originalDefaultHostServer;
        }

        string IRemoteServices.GetRemoteMainURL(string fileName)
        {
            return $"{_defaultHostServer}/{fileName}";
        }
        string IRemoteServices.GetRemoteFallbackURL(string fileName)
        {
            return $"{_fallbackHostServer}/{fileName}";
        }
    }

    private IEnumerator InitPackage(ResourcePackage package, InitializeParameters initParameters, Action initSuccessAction)
    {
        var initOperation = package.InitializeAsync(initParameters);
        yield return initOperation;

        if (initParameters is HostPlayModeParameters && _currentRemoteServices != null)
        {
            string localHostServer = GetStreamingAssetURL("manifest");
            _currentRemoteServices.SwitchToLocalServer(localHostServer);
            LogHandler.Info($"已切换到本地服务器: {localHostServer}");
            initSuccessAction += () => _currentRemoteServices.RestoreOriginalServer();
        }

        if (initOperation.Status == EOperationStatus.Succeed)
        {
            LogHandler.Info("资源包初始化成功！");

            yield return RequestPackageVersion(package, initSuccessAction);
        }
        else
        {
            Debug.LogError($"资源包初始化失败：{initOperation.Error}");
        }
    }

    private string GetStreamingAssetURL(string relativePath)
    {
        var fullPath = Path.Combine(Application.streamingAssetsPath, relativePath);

#if UNITY_ANDROID && !UNITY_EDITOR
        return fullPath;
#else
        return fullPath.StartsWith("file://") ? fullPath : "file://" + fullPath;
#endif
    }

    private IEnumerator RequestPackageVersion(ResourcePackage package, Action initSuccessAction)
    {
        var operation = package.RequestPackageVersionAsync(false);
        yield return operation;

        if (operation.Status == EOperationStatus.Succeed)
        {
            //更新成功
            string packageVersion = operation.PackageVersion;
            LogHandler.Info($"Request package Version : {packageVersion}");

            yield return UpdatePackageManifest(package, packageVersion, initSuccessAction);
        }
        else
        {
            //更新失败
            Debug.LogError(operation.Error);
        }
    }

    private IEnumerator UpdatePackageManifest(ResourcePackage package, string packageVersion, Action initSuccessAction)
    {
        var operation = package.UpdatePackageManifestAsync(packageVersion);
        yield return operation;

        if (operation.Status == EOperationStatus.Succeed)
        {
            PlayerPrefs.SetString("GAME_VERSION", packageVersion);

            switch (_ePlayMode)
            {
                case EPlayMode.HostPlayMode:
                {
                    initSuccessAction?.Invoke();
                    YooAssetDownloadManager.Instance.InitDownloader();
                    break;
                }
                default:
                {
                    //更新成功
                    initSuccessAction?.Invoke();
                    break;
                }
            }
        }
        else
        {
            switch (_ePlayMode)
            {
                case EPlayMode.HostPlayMode:
                {
                    // 获取上次成功记录的版本
                    string version = PlayerPrefs.GetString("GAME_VERSION", string.Empty);
                    if (string.IsNullOrEmpty(version))
                    {
                        yield break;
                    }

                    // 加载本地缓存的资源清单文件
                    var manifestOp = package.UpdatePackageManifestAsync(version);
                    yield return manifestOp;
                    if (manifestOp.Status != EOperationStatus.Succeed)
                    {
                        yield break;
                    }

                    // 在正常开始游戏之前，还需要验证本地清单内容的完整性。
                    var downloader = package.CreateResourceDownloader(10, 3);
                    if (downloader.TotalDownloadCount > 0)
                    {
                        yield break;
                    }

                    initSuccessAction?.Invoke();

                    break;
                }
                default:
                {
                    //更新失败
                    Debug.LogError(operation.Error);

                    break;
                }
            }
        }
    }

    private IEnumerator Download(Action initSuccessAction, string packageName)
    {
        int downloadingMaxNum = 10;
        int failedTryAgain = 3;
        var package = YooAssets.GetPackage(packageName);
        var downloader = package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);

        //没有需要下载的资源
        if (downloader.TotalDownloadCount == 0)
        {
            initSuccessAction?.Invoke();
            yield break;
        }

        //需要下载的文件总数和总大小
        int totalDownloadCount = downloader.TotalDownloadCount;
        long totalDownloadBytes = downloader.TotalDownloadBytes;

        //注册回调方法
        downloader.DownloadFinishCallback = null; //当下载器结束（无论成功或失败）
        downloader.DownloadErrorCallback = null; //当下载器发生错误
        downloader.DownloadUpdateCallback = null; //当下载进度发生变化
        downloader.DownloadFileBeginCallback = null; //当开始下载某个文件

        //开启下载
        downloader.BeginDownload();
        yield return downloader;

        //检测下载结果
        if (downloader.Status == EOperationStatus.Succeed)
        {
            //下载成功
            initSuccessAction?.Invoke();
        }
        else
        {
            //下载失败
        }
    }

    private void YooAssetManagerInitSuccess()
    {
        _initResourcesCompleteCallback?.Invoke();
    }

    public HasAssetResult HasAsset(string assetName)
    {
        AssetInfo assetInfo = YooAsset.YooAssets.GetAssetInfo(assetName);

        if (assetInfo.IsInvalid)
        {
            return HasAssetResult.NotExist;
        }
        else
        {
            return HasAssetResult.AssetOnDisk;
        }
    }

    public void LoadAsset(string assetName, Type assetType, int priority, LoadAssetCallbacks loadAssetCallbacks, object userData)
    {
        float startTime = Time.realtimeSinceStartup;
        AssetHandle assetHandle = null;

        if (assetType != null)
        {
            assetHandle = YooAsset.YooAssets.LoadAssetAsync(assetName, assetType, (uint)priority);
        }
        else
        {
            assetHandle = YooAsset.YooAssets.LoadAssetAsync(assetName, (uint)priority);
        }

        assetHandle.Completed += (handle) =>
        {
            AddAssetHandle(handle);

            if (handle.Status == EOperationStatus.Succeed)
            {
#if UNITY_EDITOR
                if (!GameEntry.Base.EditorResourceMode)
                {
                    ShaderTool.ResetEditorShader(handle.AssetObject, assetName);
                }
#endif

                loadAssetCallbacks.LoadAssetSuccessCallback?.Invoke(assetName, handle.AssetObject, Time.realtimeSinceStartup - startTime, userData);
            }
            else
            {

            }
        };
    }

    public void LoadAssetSync(string assetName, Type assetType, LoadAssetCallbacks loadAssetCallbacks, object userData)
    {
        float startTime = Time.realtimeSinceStartup;

        AssetHandle assetHandle = YooAsset.YooAssets.LoadAssetSync(assetName, assetType);

        assetHandle.Completed += (handle) =>
        {
            AddAssetHandle(handle);

            if (handle.Status == EOperationStatus.Succeed)
            {
#if UNITY_EDITOR
                if (!GameEntry.Base.EditorResourceMode)
                {
                    ShaderTool.ResetEditorShader(handle.AssetObject, assetName);
                }
#endif

                loadAssetCallbacks.LoadAssetSuccessCallback?.Invoke(assetName, handle.AssetObject, Time.realtimeSinceStartup - startTime, userData);
            }
            else
            {

            }
        };
    }

    public void LoadRawAsset(string assetName, int priority, LoadAssetCallbacks loadAssetCallbacks, object userData)
    {
        float startTime = Time.realtimeSinceStartup;
        RawFileHandle rawFileHandle = YooAssets.LoadRawFileAsync(assetName, (uint)priority);
        rawFileHandle.Completed += (handle) =>
        {
            if (handle.Status == EOperationStatus.Succeed)
            {
                loadAssetCallbacks.LoadAssetSuccessCallback?.Invoke(assetName, handle.GetRawFileData(), Time.realtimeSinceStartup - startTime, userData);
                handle.Dispose();
            }
            else
            {

            }
        };
    }

    private void AddAssetHandle(AssetHandle assetHandle)
    {
        if (assetHandle.Status != EOperationStatus.Succeed) return;

        if (!_assetHandleListDictionary.TryGetValue(assetHandle.AssetObject, out List<AssetHandle> assetHandleList))
        {
            assetHandleList = new List<AssetHandle>();
            _assetHandleListDictionary.Add(assetHandle.AssetObject, assetHandleList);
        }

        assetHandleList.Add(assetHandle);
    }

    private void RemoveAssetHandle(AssetHandle assetHandle)
    {
        if (_assetHandleListDictionary.TryGetValue(assetHandle.AssetObject, out List<AssetHandle> assetHandleList))
        {
            assetHandleList.Remove(assetHandle);
        }
    }

    public void UnloadAsset(object asset)
    {
        if (!_assetHandleListDictionary.TryGetValue(asset, out List<AssetHandle> assetHandleList)) return;

        AssetHandle assetHandle = assetHandleList[0];

        RemoveAssetHandle(assetHandle);

        assetHandle.Release();

        if (assetHandleList.Count <= 0)
        {
            _assetHandleListDictionary.Remove(asset);

            if (!_isUnloadUnusedAssetsNow)
            {
                _isUnloadUnusedAssetsNow = true;

                UnloadUnusedAssets();
            }
        }
    }

    // 卸载所有引用计数为零的资源包。
    // 可以在切换场景之后调用资源释放方法或者写定时器间隔时间去释放。
    private async UniTaskVoid UnloadUnusedAssets()
    {
        while (true)
        {
            await UniTask.WaitForSeconds(AssetAutoReleaseInterval);

            var package = YooAssets.GetPackage(DefaultPackage);
            var operation = package.UnloadUnusedAssetsAsync();
            await operation;
        }
    }

    public void LoadScene(string sceneAssetName, int priority, LoadSceneCallbacks loadSceneCallbacks, object userData)
    {
        float startTime = Time.realtimeSinceStartup;
        SceneHandle sceneHandle = YooAsset.YooAssets.LoadSceneAsync(sceneAssetName, LoadSceneMode.Additive, priority: (uint)priority);

        sceneHandle.Completed += (handle) =>
        {
            AddSceneHandle(handle);

            if (handle.Status == EOperationStatus.Succeed)
            {
                loadSceneCallbacks.LoadSceneSuccessCallback?.Invoke(sceneAssetName, Time.realtimeSinceStartup - startTime, userData);
            }
        };
    }

    private void AddSceneHandle(SceneHandle sceneHandle)
    {
        if (sceneHandle.Status != EOperationStatus.Succeed) return;

        _sceneHandleListDictionary[sceneHandle.SceneObject.path] = sceneHandle;
    }

    private void RemoveSceneHandle(SceneHandle sceneHandle)
    {
        _sceneHandleListDictionary.Remove(sceneHandle.SceneObject.path, out SceneHandle _);
    }

    public void UnloadScene(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks, object userData)
    {
        UnloadScene_Async(sceneAssetName, unloadSceneCallbacks, userData);
    }

    private async UniTaskVoid UnloadScene_Async(string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks, object userData)
    {
        if (!_sceneHandleListDictionary.TryGetValue(sceneAssetName, out SceneHandle sceneHandle)) return;

        RemoveSceneHandle(sceneHandle);

        UnloadSceneOperation unloadSceneOperation = sceneHandle.UnloadAsync();
        await unloadSceneOperation.ToUniTask();

        if (unloadSceneOperation.Status == EOperationStatus.Succeed)
        {
            unloadSceneCallbacks?.UnloadSceneSuccessCallback?.Invoke(sceneAssetName, userData);
        }
    }
}
