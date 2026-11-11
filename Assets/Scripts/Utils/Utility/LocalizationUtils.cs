using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CasualGame;

public class LocalizationUtils
{
    public static string GetString(int key, string defaultStr = "")
    {
        var ret = GameEntry.Localization.GetString(key.ToString());

        if (string.IsNullOrEmpty(ret) || ret.Contains("<NoKey>"))
            return defaultStr;
        else
            return ret;
    }

    public static string GetString(string key, string defaultStr = "")
    {
        var ret = GameEntry.Localization.GetString(key);

        if (string.IsNullOrEmpty(ret) || ret.Contains("<NoKey>"))
            return defaultStr;
        else
            return ret;
    }
}
