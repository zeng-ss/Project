using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HorstRill;

public class BaseURL
{
    public static string URL => GetUrl();
    public static string KEY_SELECT_SERVER = "KEY_SELECT_SERVER";
    public static string KEY_SELECT_SERVER_DEBUG = "KEY_SELECT_SERVER_DEBUG";
    public static string SDK_URL = "";
    private static string STAGE_URL = "https://api-staging.screw-word-out.nfpuzzlegame.fun";

    //0 默认正式服  1 测试服 
    public static bool IsOffical
    {
        get
        {
            // 正常游戏模式
            if (DebugOffical < 0)
            {
                return PlayerPrefs.GetInt(KEY_SELECT_SERVER, 0) == 0;
            }
            return DebugOffical == 0;
        }
        set
        {
            PlayerPrefs.SetInt(KEY_SELECT_SERVER, value ? 0 : 1);
        }
    }

    public static int DebugOffical
    {
        get
        {
            return PlayerPrefs.GetInt(KEY_SELECT_SERVER_DEBUG, -1);
        }
        set
        {
            PlayerPrefs.SetInt(KEY_SELECT_SERVER_DEBUG, value);
        }
    }

    private static string GetUrl()
    {
        if (IsOffical)
        {
            if (string.IsNullOrEmpty(SDK_URL))
            {
                InitUrl();
            }
            return SDK_URL;
        }
        return STAGE_URL;
    }

    //本地默认使用测试服
    public static void Initial(bool offical)
    {
        IsOffical = offical;
        InitUrl();
    }

    public static void InitUrl()
    {
        SDK_URL = HorstRillConfig.GetHorstRillConfig().baseUrl;
    }
}
