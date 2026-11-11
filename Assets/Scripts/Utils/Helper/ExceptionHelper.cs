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
        if (type == LogType.Exception)
        {
            GLog.unity_exception(condition, stackTrace);
        }
        else if (type == LogType.Error)
        {
            GLog.unity_error(condition, stackTrace);
        }
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= OnLog;
    }
}
