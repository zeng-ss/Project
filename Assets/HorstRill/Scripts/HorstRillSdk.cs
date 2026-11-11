
using System.Collections.Generic;
using UnityEngine;
using System;
using HorstRill;
using HorstRill.Data;
using UnityEngine.Networking;

public static class HorstRillSdk
{
    public const string TAG = "HorstRillSdk";

    public static AdManager Ads => AdManager.Instance;
    public static HorstRillDataManager Data => HorstRillDataManager.Instance;
    public static IAPManager Iap => IAPManager.Instance;
    public static RateManager Rate => RateManager.Instance;

    private static GameObject _gameObject;

    public static void InitRoot(GameObject gameObject)
    {
        _gameObject = gameObject;
    }

    /// <summary>
    /// 预初始化，支持埋点等可能尽早需要使用的功能
    /// </summary>
    /// <param name="enableAd"></param>
    public static void PreInitSDK(
        bool enableLog = false,
        GULogLevel logLevel = GULogLevel.WARNING,
        string lang = "English",
        string country = "",
        string nickname = "",
        string email = "",
        string baseUrl = "")
    {
        HorstRillLog.Initialize(enableLog, logLevel);
        HorstRillLog.Log(TAG, "pre init SDK..");
    }


    /// <summary>
    /// 初始化，依赖生命周期或者不那么着急用到的功能
    /// </summary>
    public static void InitSDK()
    {
        HorstRillLog.Log(TAG, "init SDK..");
        //
        HorstRillBehaviour._instance.Init();
    }

    public static void InitAnalytics(DataPropAction propAction)
    {
        HorstRillLog.Log(TAG, "Initializing Analytics");
        // 埋点
        Data.PropAction = propAction;
        // AppsFlyer依赖TA
        if (HorstRillBehaviour._instance != null)
            HorstRillBehaviour._instance.InitAppsFlyer();
    }

    public static string GetDeviceId()
    {
        return Data.GetDeviceId();
    }

    public static string GetDistinctId()
    {
        return Data.GetDistinctId();
    }

#if !UNITY_WEBGL
    public class KbCertificateHandler : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        }
    }
#endif
}
