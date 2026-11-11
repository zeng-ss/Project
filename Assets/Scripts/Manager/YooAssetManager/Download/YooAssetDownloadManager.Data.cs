using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YooAsset;
using Cysharp.Threading.Tasks;

#region 辅助类和结构体

/// <summary>
/// 任务键值 - 用于去重识别
/// </summary>
public readonly struct TaskKey : IEquatable<TaskKey>
{
    public TaskType Type { get; }
    public string PackageName { get; }
    public string[] Targets { get; }

    public TaskKey(TaskType type, string packageName, string[] targets)
    {
        Type = type;
        PackageName = packageName ?? YooAssetDownloadManager.DefaultPackage;
        Targets = targets ?? Array.Empty<string>();
    }

    public bool Equals(TaskKey other)
    {
        return Type == other.Type &&
               string.Equals(PackageName, other.PackageName) &&
               (Targets?.SequenceEqual(other.Targets ?? Array.Empty<string>()) ?? other.Targets == null);
    }

    public override bool Equals(object obj) => obj is TaskKey other && Equals(other);

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Type);
        hashCode.Add(PackageName);
        if (Targets != null)
        {
            foreach (var target in Targets)
            {
                hashCode.Add(target);
            }
        }
        return hashCode.ToHashCode();
    }

    public override string ToString()
    {
        var targets = Targets != null ? string.Join(",", Targets) : "null";
        return $"{Type}:{PackageName}:[{targets}]";
    }
}

/// <summary>
/// 任务类型
/// </summary>
public enum TaskType
{
    SingleAsset, // 单个资源
    AssetList, // 资源列表
    Tags, // 标签
    AllAssets // 全部资源
}

/// <summary>
/// 下载任务
/// </summary>
public class DownloadTask
{
    public TaskKey TaskKey { get; }
    public int TotalCount { get; private set; }
    public long TotalBytes { get; private set; }
    public int CurrentCount { get; private set; }
    public long CurrentBytes { get; private set; }
    public bool IsCompleted { get; private set; }
    public bool IsSuccess { get; private set; }
    public float Progress => TotalBytes > 0 ? (float)CurrentBytes / TotalBytes : 0f;

    private readonly ResourceDownloaderOperation _downloader;
    private readonly List<DownloadCallbacks> _callbacksList = new();
    private UniTaskCompletionSource<bool> _completionSource;

    public DownloadTask(TaskKey taskKey, ResourceDownloaderOperation downloader, DownloadCallbacks callbacks)
    {
        TaskKey = taskKey;
        _downloader = downloader;
        TotalCount = downloader.TotalDownloadCount;
        TotalBytes = downloader.TotalDownloadBytes;

        if (callbacks != null)
        {
            _callbacksList.Add(callbacks);
        }

        _completionSource = new UniTaskCompletionSource<bool>();
        SetupCallbacks();
    }

    public void AddCallbacks(DownloadCallbacks callbacks)
    {
        if (callbacks != null)
        {
            _callbacksList.Add(callbacks);
        }
    }

    public async UniTaskVoid StartAsync()
    {
        try
        {
            LogHandler.Info($"开始下载: {TaskKey}, 文件数: {TotalCount}, 大小: {FormatBytes(TotalBytes)}");

            _downloader.BeginDownload();
            await _downloader.ToUniTask();

            IsSuccess = _downloader.Status == EOperationStatus.Succeed;
            IsCompleted = true;

            _completionSource.TrySetResult(IsSuccess);
        }
        catch (Exception ex)
        {
            LogHandler.Error($"下载任务异常: {TaskKey}, 错误: {ex.Message}");
            IsSuccess = false;
            IsCompleted = true;
            _completionSource.TrySetResult(false);
        }
    }

    public async UniTask<bool> WaitForCompletionAsync()
    {
        return await _completionSource.Task;
    }

    public void Pause() => _downloader?.PauseDownload();
    public void Resume() => _downloader?.ResumeDownload();
    public void Cancel()
    {
        _downloader?.CancelDownload();
        IsCompleted = true;
        IsSuccess = false;
        _completionSource?.TrySetResult(false);
    }

    public TaskDownloadStatus GetStatus()
    {
        return new TaskDownloadStatus
        {
            TaskKey = TaskKey,
            TotalCount = TotalCount,
            TotalBytes = TotalBytes,
            CurrentCount = CurrentCount,
            CurrentBytes = CurrentBytes,
            Progress = Progress,
            IsCompleted = IsCompleted,
            IsSuccess = IsSuccess
        };
    }

    private void SetupCallbacks()
    {
        _downloader.DownloadFinishCallback += (data) =>
        {
            if (data.Succeed)
            {
                foreach (var callbacks in _callbacksList)
                {
                    callbacks.DownloadCompleteCallback?.Invoke();
                }
            }
        };

        _downloader.DownloadUpdateCallback += (data) =>
        {
            CurrentCount = data.CurrentDownloadCount;
            CurrentBytes = data.CurrentDownloadBytes;

            foreach (var callbacks in _callbacksList)
            {
                callbacks.DownloadProgressCallback?.Invoke(
                    CurrentCount, CurrentBytes, TotalCount, TotalBytes);
            }
        };

        _downloader.DownloadErrorCallback += (data) =>
        {
            LogHandler.Error($"下载文件失败: {data.FileName}, 错误: {data.ErrorInfo}");
            foreach (var callbacks in _callbacksList)
            {
                callbacks.DownloadErrorCallback?.Invoke($"下载文件失败: {data.FileName}, 错误: {data.ErrorInfo}");
            }
        };

        _downloader.DownloadFileBeginCallback += (data) =>
        {
            LogHandler.Info($"开始下载文件: {data.FileName}, 大小: {FormatBytes(data.FileSize)}", LogColor.Purple);
            foreach (var callbacks in _callbacksList)
            {
                callbacks.DownloadFileBeginCallback?.Invoke(data.FileName, data.FileSize);
            }
        };
    }

    private string FormatBytes(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024f:F1} KB";
        if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024f * 1024f):F1} MB";
        return $"{bytes / (1024f * 1024f * 1024f):F1} GB";
    }
}

/// <summary>
/// 全局下载状态
/// </summary>
public struct GlobalDownloadStatus
{
    public int TotalTasks;
    public int CompletedTasks;
    public long TotalBytes;
    public long CompletedBytes;
    public int RunningTasks;
    public int PendingTasks;
    public float Progress;
}

/// <summary>
/// 任务下载状态
/// </summary>
public struct TaskDownloadStatus
{
    public TaskKey TaskKey;
    public int TotalCount;
    public long TotalBytes;
    public int CurrentCount;
    public long CurrentBytes;
    public float Progress;
    public bool IsCompleted;
    public bool IsSuccess;
}
#endregion