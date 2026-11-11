using System;
using UnityEngine;

public class Singleton<T> where T : class, new()
{
    private static T s_instance;

    protected Singleton()
    {
    }

    public static T Instance
    {
        get
        {
            if (s_instance == null)
            {
                CreateInstance();
            }
            return s_instance;
        }
    }

    #region 对外辅助接口
    public static void CreateInstance()
    {
        s_instance ??= Activator.CreateInstance<T>();
    }

    public static void DestroyInstance()
    {
        if (s_instance != null)
        {
            s_instance = null;
        }
    }

    public static bool HasInstance()
    {
        return s_instance != null;
    }
    #endregion
}