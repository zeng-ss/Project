//------------------------------------------------------------
// File : TextureAgentHelper.cs
// Email: yang.li@kingboat.io
// Desc : 
//------------------------------------------------------------

using GameFramework;
using GameFramework.FileSystem;
using GameFramework.Resource;
using System;
using UnityEngine;
using UnityEngine.Networking;

public partial class TextureAgentHelper : MonoBehaviour
{
    private string m_RequestFullPath = null;
    private float m_LastProgress = 0f;
    private int m_RetryCount = 0;
    private UnityWebRequest m_UnityWebRequest = null;
    private GameFrameworkAction<DownloadingInfo> m_LoadResourceAgentHelperUpdateAction = null;
    private GameFrameworkAction<byte[], Texture2D> m_LoadResourceAgentHelperRequestCompleteAction = null;
    private GameFrameworkAction<string> m_LoadResourceAgentHelperErrorAction = null;
    private int currentRequestCount = 0;
    private DownloadingInfo m_DownloadingInfo = null;
    public bool IsBusy => m_RequestFullPath != null;

    public void Request(string fullPath, int retryCount, GameFrameworkAction<DownloadingInfo> updateAction, GameFrameworkAction<byte[], Texture2D> requestCompleteAction, GameFrameworkAction<string> errorAction)
    {
        m_RequestFullPath = fullPath;
        m_RetryCount = retryCount;
        m_LoadResourceAgentHelperUpdateAction = updateAction;
        m_LoadResourceAgentHelperRequestCompleteAction = requestCompleteAction;
        m_LoadResourceAgentHelperErrorAction = errorAction;
        currentRequestCount = 0;
        m_DownloadingInfo = DownloadingInfo.Create(m_RequestFullPath);
        m_UnityWebRequest = UnityWebRequestTexture.GetTexture(GameFramework.Utility.Path.GetRemotePath(m_RequestFullPath));
        m_UnityWebRequest.SendWebRequest();
    }

    public void Retry(string error)
    {
        if (currentRequestCount < m_RetryCount)
        {
            currentRequestCount++;
            m_UnityWebRequest = UnityWebRequestTexture.GetTexture(GameFramework.Utility.Path.GetRemotePath(m_RequestFullPath));
            m_UnityWebRequest.SendWebRequest();
        }
        else
        {
            m_LoadResourceAgentHelperErrorAction?.Invoke(error);
            Reset();
        }
    }

    public void Reset()
    {
        m_RequestFullPath = null;
        m_LastProgress = 0f;
        m_LoadResourceAgentHelperUpdateAction = null;
        m_LoadResourceAgentHelperRequestCompleteAction = null;
        m_LoadResourceAgentHelperErrorAction = null;
        ReferencePool.Release(m_DownloadingInfo);
        m_DownloadingInfo = null;

        if (m_UnityWebRequest != null)
        {
            m_UnityWebRequest.Dispose();
            m_UnityWebRequest = null;
        }
    }
    private void Update()
    {
        UpdateUnityWebRequest();
    }

    private void UpdateUnityWebRequest()
    {
        if (m_UnityWebRequest != null)
        {
            m_DownloadingInfo.UpdateSize(m_UnityWebRequest.downloadedBytes);
            m_LoadResourceAgentHelperUpdateAction?.Invoke(m_DownloadingInfo);

            if (m_UnityWebRequest.isDone)
            {
                if (string.IsNullOrEmpty(m_UnityWebRequest.error))
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(m_UnityWebRequest);
                    m_LoadResourceAgentHelperRequestCompleteAction?.Invoke(m_UnityWebRequest.downloadHandler.data, texture);
                    Reset();
                }
                else
                {
                    bool isError = false;
                    isError = m_UnityWebRequest.result != UnityWebRequest.Result.Success;
                    string error = GameFramework.Utility.Text.Format("Can not load asset '{0}' with error message '{1}'.", m_RequestFullPath, isError ? m_UnityWebRequest.error : null);
                    Retry(error);
                }
            }
            else if (m_UnityWebRequest.downloadProgress != m_LastProgress)
            {
                m_LastProgress = m_UnityWebRequest.downloadProgress;
            }
        }
    }
}
public class DownloadingInfo : IReference
{
    public string url;
    public ulong downloadedSize;
    public void Clear()
    {
        this.url = "";
        this.downloadedSize = 0;
    }
    public void UpdateSize(ulong size)
    {
        this.downloadedSize = Math.Max(this.downloadedSize, size);
    }
    public static DownloadingInfo Create(string url)
    {
        DownloadingInfo downloadingInfo = ReferencePool.Acquire<DownloadingInfo>();
        downloadingInfo.url = url;
        downloadingInfo.downloadedSize = 0;
        return downloadingInfo;
    }
}