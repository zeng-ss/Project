using UnityEngine;

/// <summary>
/// 全局日志。支持 Log.Info("x {0}", a) 这种格式化写法。
/// 替代原 UnityGameFramework.Runtime.Log。
/// </summary>
public static class Log
{
    public static void Debug(string format, params object[] args)
    {
        UnityEngine.Debug.Log(Format(format, args));
    }

    public static void Info(string format, params object[] args)
    {
        UnityEngine.Debug.Log(Format(format, args));
    }

    public static void Warning(string format, params object[] args)
    {
        UnityEngine.Debug.LogWarning(Format(format, args));
    }

    public static void Error(string format, params object[] args)
    {
        UnityEngine.Debug.LogError(Format(format, args));
    }

    private static string Format(string format, object[] args)
    {
        if (args == null || args.Length == 0) return format;

        try
        {
            return string.Format(format, args);
        }
        catch (System.FormatException)
        {
            // 日志里带 { } 但参数不匹配时不要让日志本身抛异常
            return format + " | " + string.Join(", ", args);
        }
    }
}
