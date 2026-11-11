using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExceptionHelper : MonoBehaviour
{
    // 异常资源下载中
    private bool IsErrResDownloading = false;
    private int TimeOut = 20;

    private void Awake()
    {
        Application.logMessageReceived += OnLog;
    }

    private void OnLog(string condition, string stackTrace, LogType type)
    {
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= OnLog;
    }
}
