using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework.Resource;
using UnityEngine;
using UnityGameFramework.Runtime;
using Object = UnityEngine.Object;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace CasualGame
{
    public static class ResourceComponentExtension
    {
        private static UniTask<T> LoadAsset<T>(ResourceComponent resourceComponent, string assetPath)
        {
            var tcs = new UniTaskCompletionSource<T>();
            resourceComponent.LoadAsset(assetPath, typeof(T), Constant.AssetPriority.NormalAsset,
                new LoadAssetCallbacks(
                    (assetName, asset, duration, userData) =>
                    {
                        if (asset is T tObject)
                        {
                            tcs.TrySetResult(tObject);
                        }
                        else if (asset is GameObject gameObject)
                        {
                            T component = gameObject.GetComponent<T>();
                            tcs.TrySetResult(component);
                        }
                        else
                        {
                            tcs.TrySetException(new Exception($"资源{assetPath}:加载失败"));
                        }
                    },
                    (assetName, status, errorMessage, userData) =>
                    {
                        tcs.TrySetException(new Exception($"资源{assetPath}:加载失败"));
                    }), null);
            return tcs.Task;
        }

        public static async UniTask<T> LoadAsset<T>(this ResourceComponent resourceComponent, int assetID, CancellationToken cancellationToken)
        {
            string assetPath = ResourceIdentificationTool.Instance.GetAssetPathById(assetID);
            return await LoadAsset<T>(resourceComponent, assetPath, cancellationToken);
        }

        public static async UniTask<T> LoadAndInstance<T>(this ResourceComponent resourceComponent, int assetID, CancellationToken cancellationToken) where T : Component
        {
            string assetPath = ResourceIdentificationTool.Instance.GetAssetPathById(assetID);
            GameObject com = await LoadAsset<GameObject>(resourceComponent, assetPath, cancellationToken);
            GameObject go = Object.Instantiate(com);
            return go.GetComponent<T>();
        }

        public static async UniTask<T> LoadAsset<T>(this ResourceComponent resourceComponent, string assetPath, CancellationToken cancellationToken)
        {
            T _asset = default;
            try
            {
                _asset = await LoadAsset<T>(resourceComponent, assetPath);
                cancellationToken.ThrowIfCancellationRequested();

                return _asset;
            }
            catch (OperationCanceledException)
            {
                if (_asset != null)
                {
                    GameEntry.Resource.UnloadAsset(_asset);
                }

                LogHandler.Warning($"资源加载被取消: {assetPath}");
                throw;
            }
            catch (Exception e)
            {
                LogHandler.Error($"资源加载异常 [路径: {assetPath}]: {e.Message}\n{e.StackTrace}");
                throw;
            }
        }

        // /// <summary>
        // /// 异步加载资源
        // /// </summary>
        // /// <typeparam name="T"></typeparam>
        // /// <param name="resourceComponent"></param>
        // /// <param name="assetID"></param>
        // /// <param name="loadAssetSuccessCallback"></param>
        // /// <param name="loadAssetFailureCallback"></param>
        // public static void LoadAsset<T>(this ResourceComponent resourceComponent, int assetID,
        //     Action<T> loadAssetSuccessCallback, Action loadAssetFailureCallback)
        // {
        //     string assetPath =
        //         ResourceIdentificationTool.Instance.GetAssetPathById(assetID);

        //     LoadAsset<T>(resourceComponent, assetPath, loadAssetSuccessCallback, loadAssetFailureCallback);
        // }

        // /// <summary>
        // /// 异步加载资源
        // /// </summary>
        // /// <typeparam name="T"></typeparam>
        // /// <param name="resourceComponent"></param>
        // /// <param name="assetPath"></param>
        // /// <param name="loadAssetSuccessCallback"></param>
        // /// <param name="loadAssetFailureCallback"></param>
        // public static void LoadAsset<T>(this ResourceComponent resourceComponent, string assetPath,
        //     Action<T> loadAssetSuccessCallback, Action loadAssetFailureCallback)
        // {
        //     resourceComponent.LoadAsset(assetPath, typeof(T), Constant.AssetPriority.NormalAsset,
        //         new LoadAssetCallbacks(
        //             (assetName, asset, duration, userData) =>
        //             {
        //                 if (asset is T tObject)
        //                 {
        //                     loadAssetSuccessCallback.Invoke(tObject);
        //                 }
        //                 else if (asset is GameObject gameObject)
        //                 {
        //                     T component = gameObject.GetComponent<T>();
        //                     loadAssetSuccessCallback.Invoke(component);
        //                 }
        //                 else
        //                 {
        //                     Debug.LogError($"资源{assetPath}:加载失败");
        //                 }
        //             },
        //             (assetName, status, errorMessage, userData) =>
        //             {
        //                 loadAssetFailureCallback?.Invoke();
        //             }), null);
        // }

        // /// <summary>
        // /// 同步加载资源
        // /// </summary>
        // /// <typeparam name="T"></typeparam>
        // /// <param name="resourceComponent"></param>
        // /// <param name="assetID"></param>
        // /// <param name="loadAssetSuccessCallback"></param>
        // /// <param name="loadAssetFailureCallback"></param>
        // public static void LoadAssetSync<T>(this ResourceComponent resourceComponent,
        //     int assetID,
        //     Action<T> loadAssetSuccessCallback, Action loadAssetFailureCallback = null)
        // {
        //     string assetPath =
        //         ResourceIdentificationTool.Instance.GetAssetPathById(assetID);

        //     if (string.IsNullOrEmpty(assetPath)) return;

        //     LoadAssetSync<T>(resourceComponent, assetPath, loadAssetSuccessCallback, loadAssetFailureCallback);
        // }

        // /// <summary>
        // /// 同步加载资源
        // /// </summary>
        // /// <typeparam name="T"></typeparam>
        // /// <param name="resourceComponent"></param>
        // /// <param name="assetPath"></param>
        // /// <param name="loadAssetSuccessCallback"></param>
        // /// <param name="loadAssetFailureCallback"></param>
        // public static void LoadAssetSync<T>(this ResourceComponent resourceComponent,
        //     string assetPath,
        //     Action<T> loadAssetSuccessCallback, Action loadAssetFailureCallback = null)
        // {
        //     Type assetType = typeof(T).IsSubclassOf(typeof(MonoBehaviour)) ? typeof(Object) : typeof(T);

        //     resourceComponent.LoadAssetSync(assetPath, assetType,
        //         new LoadAssetCallbacks(
        //             (assetName, asset, duration, userData) =>
        //             {
        //                 if (asset is T tObject)
        //                 {
        //                     loadAssetSuccessCallback.Invoke(tObject);
        //                 }
        //                 else if (asset is GameObject gameObject)
        //                 {
        //                     T component = gameObject.GetComponent<T>();
        //                     loadAssetSuccessCallback.Invoke(component);
        //                 }
        //                 else
        //                 {
        //                     Debug.LogError($"资源{assetPath}:加载失败");
        //                 }
        //             },
        //             (assetName, status, errorMessage, userData) =>
        //             {
        //                 Debug.LogError($"资源{assetPath}:加载失败");
        //                 loadAssetFailureCallback?.Invoke();
        //             }), null);
        // }
    }
}