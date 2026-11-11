using System;
using Cysharp.Threading.Tasks;
using YooAsset;
using UnityEngine;

/// <summary>
/// YooAsset预载资源管理器 - 支持去重、并发控制、自动重试
/// </summary>
public partial class YooAssetDownloadManager
{
    /// <summary>
    /// 检测单个资源是否需要下载
    /// </summary>
    public bool CheckAssetNeedDownloadAsync(string location, string packageName = DefaultPackage)
    {
        var package = YooAssets.GetPackage(packageName);
        if (package == null)
        {
            return false;
        }

        try
        {
            bool needDownload = package.IsNeedDownloadFromRemote(location);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
            return false;
        }
    }

    /// <summary>
    /// 检测多个资源是否需要下载
    /// </summary>
    public bool CheckAssetsNeedDownloadAsync(string[] locations, string packageName = DefaultPackage)
    {
        var package = YooAssets.GetPackage(packageName);
        if (package == null)
        {
            return false;
        }

        try
        {
            int needDownloadCount = 0;
            foreach (var location in locations)
            {
                if (package.IsNeedDownloadFromRemote(location))
                {
                    needDownloadCount++;
                    break;
                }
            }

            return needDownloadCount > 0;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
            return false;
        }
    }

    /// <summary>
    /// 检测指定标签的资源是否需要下载
    /// </summary>
    public bool CheckAssetsByTagsNeedDownloadAsync(string[] tags, string packageName = DefaultPackage)
    {
        var package = YooAssets.GetPackage(packageName);
        if (package == null)
        {
            return false;
        }

        try
        {
            var assetInfos = package.GetAssetInfos(tags);
            int needDownloadCount = 0;

            foreach (var assetInfo in assetInfos)
            {
                if (package.IsNeedDownloadFromRemote(assetInfo))
                {
                    needDownloadCount++;
                    break;
                }
            }

            return needDownloadCount > 0;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
            return false;
        }
    }
}