using System;

/// <summary>
/// 下载回调函数集
/// </summary>
public sealed class DownloadCallbacks
{
    private readonly DownloadProgressCallback m_DownloadProgressCallback;
    private readonly DownloadCompleteCallback m_DownloadCompleteCallback;
    private readonly DownloadErrorCallback m_DownloadErrorCallback;
    private readonly DownloadFileBeginCallback m_DownloadFileBeginCallback;

    /// <summary>
    /// 初始化下载回调函数集的新实例
    /// </summary>
    /// <param name="downloadSuccessCallback">下载完成回调函数</param>
    public DownloadCallbacks(DownloadCompleteCallback downloadSuccessCallback)
        : this(null, downloadSuccessCallback, null, null)
    {
    }

    /// <summary>
    /// 初始化下载回调函数集的新实例
    /// </summary>
    /// <param name="downloadProgressCallback">下载进度回调函数</param>
    /// <param name="downloadSuccessCallback">下载完成回调函数</param>
    public DownloadCallbacks(DownloadProgressCallback downloadProgressCallback,
        DownloadCompleteCallback downloadSuccessCallback)
        : this(downloadProgressCallback, downloadSuccessCallback, null, null)
    {
    }

    /// <summary>
    /// 初始化下载回调函数集的新实例
    /// </summary>
    /// <param name="downloadProgressCallback">下载进度回调函数</param>
    /// <param name="downloadSuccessCallback">下载完成回调函数</param>
    /// <param name="downloadErrorCallback">下载错误回调函数</param>
    public DownloadCallbacks(DownloadProgressCallback downloadProgressCallback,
        DownloadCompleteCallback downloadSuccessCallback,
        DownloadErrorCallback downloadErrorCallback)
        : this(downloadProgressCallback, downloadSuccessCallback, downloadErrorCallback, null)
    {
    }

    /// <summary>
    /// 初始化下载回调函数集的新实例
    /// </summary>
    /// <param name="downloadProgressCallback">下载进度回调函数</param>
    /// <param name="downloadSuccessCallback">下载完成回调函数</param>
    /// <param name="downloadErrorCallback">下载错误回调函数</param>
    /// <param name="downloadFileBeginCallback">下载文件开始回调函数</param>
    public DownloadCallbacks(DownloadProgressCallback downloadProgressCallback,
        DownloadCompleteCallback downloadSuccessCallback,
        DownloadErrorCallback downloadErrorCallback,
        DownloadFileBeginCallback downloadFileBeginCallback)
    {
        if (downloadSuccessCallback == null)
        {
            throw new System.Exception("Download complete callback is invalid.");
        }

        m_DownloadProgressCallback = downloadProgressCallback;
        m_DownloadCompleteCallback = downloadSuccessCallback;
        m_DownloadErrorCallback = downloadErrorCallback;
        m_DownloadFileBeginCallback = downloadFileBeginCallback;
    }

    /// <summary>
    /// 获取下载进度回调函数
    /// </summary>
    public DownloadProgressCallback DownloadProgressCallback
    {
        get { return m_DownloadProgressCallback; }
    }

    /// <summary>
    /// 获取下载完成回调函数
    /// </summary>
    public DownloadCompleteCallback DownloadCompleteCallback
    {
        get { return m_DownloadCompleteCallback; }
    }

    /// <summary>
    /// 获取下载错误回调函数
    /// </summary>
    public DownloadErrorCallback DownloadErrorCallback
    {
        get { return m_DownloadErrorCallback; }
    }

    /// <summary>
    /// 获取下载文件开始回调函数
    /// </summary>
    public DownloadFileBeginCallback DownloadFileBeginCallback
    {
        get { return m_DownloadFileBeginCallback; }
    }
}

/// <summary>
/// 下载进度回调函数 (已下载数量, 已下载字节数, 总数量, 总字节数)
/// </summary>
public delegate void DownloadProgressCallback(int downloadedCount, long downloadedBytes, int totalCount, long totalBytes);

/// <summary>
/// 下载完成回调函数
/// </summary>
public delegate void DownloadCompleteCallback();

/// <summary>
/// 下载错误回调函数 (错误信息)
/// </summary>
public delegate void DownloadErrorCallback(string errorInfo);

/// <summary>
/// 下载文件开始回调函数 (文件名, 文件大小)
/// </summary>
public delegate void DownloadFileBeginCallback(string fileName, long fileSize);