using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YooAsset;
using Cysharp.Threading.Tasks;

/// <summary>
/// YooAsset预载资源管理器 - 支持去重、并发控制、自动重试
/// </summary>
public partial class YooAssetDownloadManager : Singleton<YooAssetDownloadManager>
{
    public const string DefaultPackage = "DefaultPackage";

    // 配置参数
    private readonly int _maxConcurrentDownloads = 3; // 最大并发下载任务数
    private readonly int _downloadingMaxNumber = 10;  // 单个下载器最大并发数
    private readonly int _failedTryAgain = 3;         // 失败重试次数

    // 任务管理
    private readonly Dictionary<TaskKey, DownloadTask> _activeTasks = new();
    private readonly Queue<DownloadTask> _pendingTasks = new();
    private readonly List<DownloadTask> _runningTasks = new();

    // 全局状态
    private int _totalActiveTasks = 0;
    private long _totalActiveBytes = 0;
    private int _completedActiveTasks = 0;
    private long _completedActiveBytes = 0;

    public void InitDownloader()
    {
        // 启动任务调度循环
        ScheduleTasksAsync().Forget();
    }

    #region 公共接口

    /// <summary>
    /// 预下载指定资源
    /// </summary>
    public async UniTask<bool> DownloadAssetAsync(string location, DownloadCallbacks callbacks = null, string packageName = DefaultPackage)
    {
        var taskKey = new TaskKey(TaskType.SingleAsset, packageName, new[] { location });
        return await RequestDownloadAsync(taskKey, callbacks);
    }

    /// <summary>
    /// 预下载资源列表
    /// </summary>
    public async UniTask<bool> DownloadAssetsAsync(string[] locations, DownloadCallbacks callbacks = null, string packageName = DefaultPackage)
    {
        var taskKey = new TaskKey(TaskType.AssetList, packageName, locations);
        return await RequestDownloadAsync(taskKey, callbacks);
    }

    /// <summary>
    /// 预下载指定标签资源
    /// </summary>
    public async UniTask<bool> DownloadAssetsByTagsAsync(string[] tags, DownloadCallbacks callbacks = null, string packageName = DefaultPackage)
    {
        var taskKey = new TaskKey(TaskType.Tags, packageName, tags);
        return await RequestDownloadAsync(taskKey, callbacks);
    }

    /// <summary>
    /// 预下载全部资源
    /// </summary>
    public async UniTask<bool> DownloadAllAssetsAsync(DownloadCallbacks callbacks = null, string packageName = DefaultPackage)
    {
        var taskKey = new TaskKey(TaskType.AllAssets, packageName, null);
        return await RequestDownloadAsync(taskKey, callbacks);
    }

    /// <summary>
    /// 获取全局下载状态
    /// </summary>
    public GlobalDownloadStatus GetGlobalDownloadStatus()
    {
        return new GlobalDownloadStatus
        {
            TotalTasks = _totalActiveTasks,
            CompletedTasks = _completedActiveTasks,
            TotalBytes = _totalActiveBytes,
            CompletedBytes = _completedActiveBytes,
            RunningTasks = _runningTasks.Count,
            PendingTasks = _pendingTasks.Count,
            Progress = _totalActiveBytes > 0 ? (float)_completedActiveBytes / _totalActiveBytes : 0f
        };
    }

    /// <summary>
    /// 获取指定任务状态
    /// </summary>
    public TaskDownloadStatus? GetTaskStatus(string location, string packageName = DefaultPackage)
    {
        var taskKey = new TaskKey(TaskType.SingleAsset, packageName, new[] { location });
        if (_activeTasks.TryGetValue(taskKey, out var task))
        {
            return task.GetStatus();
        }
        return null;
    }

    /// <summary>
    /// 暂停所有下载
    /// </summary>
    public void PauseAllDownloads()
    {
        foreach (var task in _runningTasks)
        {
            task.Pause();
        }
    }

    /// <summary>
    /// 恢复所有下载
    /// </summary>
    public void ResumeAllDownloads()
    {
        foreach (var task in _runningTasks)
        {
            task.Resume();
        }
    }

    /// <summary>
    /// 取消所有下载
    /// </summary>
    public void CancelAllDownloads()
    {
        var allTasks = _activeTasks.Values.ToList();
        foreach (var task in allTasks)
        {
            task.Cancel();
        }

        _activeTasks.Clear();
        _pendingTasks.Clear();
        _runningTasks.Clear();
        ResetGlobalCounters();
    }

    #endregion

    #region 私有方法

    /// <summary>
    /// 请求下载（含去重逻辑）
    /// </summary>
    private async UniTask<bool> RequestDownloadAsync(TaskKey taskKey, DownloadCallbacks callbacks)
    {
        // 检查是否已有相同任务
        if (_activeTasks.TryGetValue(taskKey, out var existingTask))
        {
            LogHandler.Info($"复用现有下载任务: {taskKey}");

            // 添加回调到现有任务
            if (callbacks != null)
            {
                existingTask.AddCallbacks(callbacks);
            }

            // 等待现有任务完成
            return await existingTask.WaitForCompletionAsync();
        }

        // 创建新下载任务
        var newTask = await CreateDownloadTaskAsync(taskKey, callbacks);
        if (newTask == null)
        {
            return false;
        }

        // 注册任务
        _activeTasks[taskKey] = newTask;
        _pendingTasks.Enqueue(newTask);

        // 更新全局计数器
        UpdateGlobalCounters();

        LogHandler.Info($"创建新下载任务: {taskKey}, 队列长度: {_pendingTasks.Count}");

        return await newTask.WaitForCompletionAsync();
    }

    /// <summary>
    /// 创建下载任务
    /// </summary>
    private async UniTask<DownloadTask> CreateDownloadTaskAsync(TaskKey taskKey, DownloadCallbacks callbacks)
    {
        var package = YooAssets.GetPackage(taskKey.PackageName);
        if (package == null)
        {
            callbacks?.DownloadErrorCallback?.Invoke($"Package '{taskKey.PackageName}' not found");
            return null;
        }

        ResourceDownloaderOperation downloader = null;

        try
        {
            // 根据任务类型创建下载器
            downloader = taskKey.Type switch
            {
                TaskType.SingleAsset => package.CreateBundleDownloader(taskKey.Targets[0], _downloadingMaxNumber, _failedTryAgain),
                TaskType.AssetList => package.CreateBundleDownloader(taskKey.Targets, _downloadingMaxNumber, _failedTryAgain),
                TaskType.Tags => package.CreateResourceDownloader(taskKey.Targets, _downloadingMaxNumber, _failedTryAgain),
                TaskType.AllAssets => package.CreateResourceDownloader(_downloadingMaxNumber, _failedTryAgain),
                _ => throw new ArgumentOutOfRangeException()
            };

            if (downloader.TotalDownloadCount == 0)
            {
                LogHandler.Info($"任务无需下载: {taskKey}");
                callbacks?.DownloadCompleteCallback?.Invoke();
                return null;
            }

            var task = new DownloadTask(taskKey, downloader, callbacks);
            return task;
        }
        catch (Exception ex)
        {
            LogHandler.Error($"创建下载任务失败: {taskKey}, 错误: {ex.Message}");
            callbacks?.DownloadErrorCallback?.Invoke($"创建下载任务失败: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 任务调度循环
    /// </summary>
    private async UniTaskVoid ScheduleTasksAsync()
    {
        while (this != null)
        {
            try
            {
                // 清理已完成的运行任务
                for (int i = _runningTasks.Count - 1; i >= 0; i--)
                {
                    var task = _runningTasks[i];
                    if (task.IsCompleted)
                    {
                        _runningTasks.RemoveAt(i);
                        _activeTasks.Remove(task.TaskKey);

                        // 更新全局进度
                        _completedActiveTasks++;
                        _completedActiveBytes += task.TotalBytes;

                        LogHandler.Info($"任务完成: {task.TaskKey}, 成功: {task.IsSuccess}");
                    }
                }

                // 启动新任务（在并发限制内）
                while (_runningTasks.Count < _maxConcurrentDownloads && _pendingTasks.Count > 0)
                {
                    var task = _pendingTasks.Dequeue();
                    _runningTasks.Add(task);
                    task.StartAsync().Forget();

                    LogHandler.Info($"启动下载任务: {task.TaskKey}, 当前运行: {_runningTasks.Count}");
                }

                await UniTask.Delay(100); // 100ms检查一次
            }
            catch (Exception ex)
            {
                LogHandler.Error($"任务调度异常: {ex.Message}");
                await UniTask.Delay(1000);
            }
        }
    }

    /// <summary>
    /// 更新全局计数器
    /// </summary>
    private void UpdateGlobalCounters()
    {
        _totalActiveTasks = _activeTasks.Count;
        _totalActiveBytes = _activeTasks.Values.Sum(t => t.TotalBytes);
        _completedActiveTasks = _activeTasks.Values.Count(t => t.IsCompleted);
        _completedActiveBytes = _activeTasks.Values.Where(t => t.IsCompleted).Sum(t => t.TotalBytes);
    }

    /// <summary>
    /// 重置全局计数器
    /// </summary>
    private void ResetGlobalCounters()
    {
        _totalActiveTasks = 0;
        _totalActiveBytes = 0;
        _completedActiveTasks = 0;
        _completedActiveBytes = 0;
    }

    /// <summary>
    /// 格式化字节数显示
    /// </summary>
    private string FormatBytes(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024f:F1} KB";
        if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024f * 1024f):F1} MB";
        return $"{bytes / (1024f * 1024f * 1024f):F1} GB";
    }

    #endregion
}