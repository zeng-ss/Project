using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HorstRill;


public interface IRate
{
    void Show();
    void Init();
}

public class RateManager
{
    private IRate _rater;
    private static RateManager _instance;
    public static RateManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new RateManager();
            return _instance;
        }
    }

    private RateManager()
    {
        var go = new GameObject("KbRateManager");
        GameObject.DontDestroyOnLoad(go);
#if UNITY_ANDROID
        go.AddComponent<RateAndroid>();
        _rater = go.GetComponent<RateAndroid>();
#elif UNITY_IOS
        go.AddComponent<RateIOS>();
        _rater = go.GetComponent<RateIOS>();
#endif
        _rater?.Init();
    }

    private bool enable(bool doSet)
    {
#if UNITY_IOS && !UNITY_EDITOR
        // 生命周期内只展示一次评分弹窗
        if (PlayerPrefs.GetInt("rate_shown") == 1)
        {
            return false;
        }
        if(doSet){
            PlayerPrefs.SetInt("rate_shown", 1);
        }
        
        return true;
#endif
        return true;
    }

    //public void JumpToMail()
    //{
    //    string email = "目标邮箱";
    //    string subject = "邮件预设主题";
    //    string body = "邮件预设内容";
    //    Application.OpenURL("mailto:" + email + "?subject=" + subject + "&body=" + body);
    //}

    //传入的参数，如果是android平台则为packageName，如果为ios平台则为appid
    private void JumpToReview(string args)
    {
        if (!enable(true))
        {
            return;
        }
        string APP_ID = args;
        string PACKAGE_NAME = args;

        //var packageName = Application.identifier;

#if UNITY_IPHONE || UNITY_EDITOR || UNITY_IOS
        var url = string.Format(
            "itms-apps://itunes.apple.com/cn/app/id{0}?mt=8&action=write-review",
            APP_ID);
        Application.OpenURL(url);
#elif UNITY_ANDROID
        Application.OpenURL("market://details?id=" + PACKAGE_NAME);
#endif
    }

    public void Show()
    {
        _rater?.Show();
    }

    /// <summary>
    /// 展示评分引导
    /// </summary>
    /// <returns>评分引导是否展示成功</returns>
    // public void Show(Action closeAction, float scale = -1)
    // {
    //     //业务本身判断条件需要在自身判断
    //     //ScoreConfigData scoreConfigData = JsonUtility.FromJson<ScoreConfigData>(RemoteConfigManager.Instance.evaluationShouldShow);
    //     //        int versionNum = 0;
    //     //        bool parseSuccess = int.TryParse(Application.version.Replace(".", ""), out versionNum);
    //     //        if (parseSuccess)
    //     //        {
    //     //            // 版本号解析成功
    //     //#if UNITY_IOS
    //     //            if (versionNum >= scoreConfigData.ios.version && !scoreConfigData.ios.should_show)
    //     //            {
    //     //                // 版本号大于配置内版本号，且配置说明需要关闭评分弹窗
    //     //                return;
    //     //            }
    //     //#elif UNITY_ANDROID
    //     //            if (versionNum >= scoreConfigData.android.version && !scoreConfigData.android.should_show)
    //     //            {
    //     //                // 版本号大于配置内版本号，且配置说明需要关闭评分弹窗
    //     //                closeAction?.Invoke();
    //     //                return;
    //     //            }
    //     //#endif
    //     //        }
    //     //        else
    //     //        {
    //     //            // 版本号解析失败
    //     //            closeAction?.Invoke();
    //     //            return;
    //     //        }

    //     //提交过，不再展示
    //     int submitCnt = PlayerPrefs.GetInt(Consts.RATE_PREFSKEY_SUBMIT_COUNT, 0);
    //     if (submitCnt >= Consts.RATE_SUBMIT_SHOW_TIMES)
    //     {
    //         closeAction?.Invoke();
    //         return;
    //     }

    //     //later 2次不再展示
    //     int laterCnt = PlayerPrefs.GetInt(Consts.RATE_PREFSKEY_LATER_COUNT, 0);
    //     if (laterCnt >= Consts.RATE_LATER_SHOW_TIMES)
    //     {
    //         closeAction?.Invoke();
    //         return;
    //     }

    //     //时间未到24小时间隔不再展示
    //     string lastTimeStampStr = PlayerPrefs.GetString(Consts.RATE_PREFSKEY_TIMESTAMP, "0");
    //     long timestamp = 0;
    //     long.TryParse(lastTimeStampStr, out timestamp);
    //     long tsSub = DateTimeHelper.NowTimeStamp - timestamp;
    //     if (tsSub < Consts.RATE_LATER_SHOW_INTERVAL)
    //     {
    //         closeAction?.Invoke();
    //         return;
    //     }

    //     //GameEntry.UI.OpenUIForm(UIFormType.ScoreGuideUI, closeAction);
    //     GameObject Prefab = (GameObject)Resources.Load("Prefabs/Rate");
    //     var gameObject = GameObject.Instantiate(Prefab, KbSDK.GetRoot().transform);
    //     var cls = gameObject.GetComponent<RateBehaviour>();
    //     cls.AutoScale(scale);
    //     cls.OnOpen(closeAction);
    //     return;
    // }

}
