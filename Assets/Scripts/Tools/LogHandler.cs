//------------------------------------------------------------
// File : LogHandler.cs
// Email: mailto:yang.li@kingboat.io
// Desc : 日志工具
//------------------------------------------------------------
using CasualGame;
using UnityEngine;

public class LogHandler
{
    public static bool m_enableDebug
    {
        get
        {
            return GameEntry.Setting.GetBool("ENABLE_DEBUG", false);
        }
        set
        {
            GameEntry.Setting.SetBool("ENABLE_DEBUG", value);
        }
    }

    public static bool EnableDebug
    {
        get
        {
#if UNITY_EDITOR
            return true;
#endif

            return m_enableDebug;
        }
        set
        {
        }
    }

    public static void Info(string message, string color = LogColor.White)
    {
        if (EnableDebug)
            Debug.Log($"<color=#{color}>{message}</color>");
    }

    public static void Warning(string message)
    {
        if (EnableDebug)
            Debug.LogWarning(message);
    }

    public static void Error(string message)
    {
        if (EnableDebug)
            Debug.LogError(message);
    }
}

public static class LogColor
{
    public const string White = "ecf0f1";
    public const string Green = "2ecc71";
    public const string Red = "e74c3c";
    public const string Blue = "70a1ff";
    public const string Yellow = "f5f500";
    public const string Pink = "e980b6";
    public const string Purple = "bd90dd";
}