using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CasualGame;
using KBbase;
using HorstRill;
using HorstRill.Ad;
using UnityEngine;
using PuzzleTrack;

public partial class AdManager : MonoBehaviour
{
    private const string TAG = "AD";
    [HideInInspector]
    public bool SkipAd = false;
    [HideInInspector]
    public bool SkipInterstitial = false;

    // 是否进入Loading状态等待重试（最长等待8秒）
    private bool waitForRetry = false;
    private float waitTimeForRetry = 8f;
    private Action showLoadingAction, closeLoadingAction, showFailToastAction;
    public static AdManager _instance;
    public static AdManager Instance => _instance;
    private int adrevenue_rate = 0;

    private sealed class InterstitialState
    {
        public long Space;
        public int StartLevel;
        public long PreviousTime;
    }

    private InterstitialState m_InterstitialState;
    private InterstitialState Interstitial
    {
        get
        {
            ChangeInterstitialInterval();

            return m_InterstitialState;
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            _instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }


    /// <summary>
    /// 初始化
    /// </summary>
    public void Init()
    {
        HorstRillLog.Log(TAG, "init ad module..");
    }

    /// <summary>
    /// 是否开启强制等待，开启后若无缓存则会进入Loading状态等待重试（最长等待8秒）
    /// </summary>
    public void EnableWaitForRetry(bool enable, float waitTime, Action showLoading, Action closeLoading, Action showFailToast)
    {
        waitForRetry = enable;
        waitTimeForRetry = waitTime;
        showLoadingAction = showLoading;
        closeLoadingAction = closeLoading;
        showFailToastAction = showFailToast;
    }

    private void SetInterstitialState(int space, int coldStart, int startLevel)
    {
        bool shouldInitializePreviousTime = m_InterstitialState == null;
        m_InterstitialState ??= new InterstitialState();

        m_InterstitialState.Space = space;
        m_InterstitialState.StartLevel = startLevel;
        if (shouldInitializePreviousTime)
        {
            m_InterstitialState.PreviousTime = Utility.getUnixTimestamp() - space + coldStart;
        }
    }

    public bool IsInterstitialAndBannerCanShow()
    {
        return IsInterstitialAndBannerCanShow(Interstitial);
    }

    private bool IsInterstitialAndBannerCanShow(InterstitialState interstitial)
    {
        return ProxyManager.UserProxy.GameLevel >= interstitial.StartLevel;
    }

    public bool IsVip()
    {
        return false;
    }

    //显示插屏广告
    public bool showInterstitialAD(string placementID, bool immediate = false, Action<string, bool> closeCallBack = null)
    {
        SkipInterstitial = IsVip();
#if UNITY_EDITOR
        SkipInterstitial = true;
        // immediate = true;
#endif
        long curTime = Utility.getUnixTimestamp();
        InterstitialState interstitial = Interstitial;
        HorstRillLog.Log(TAG,
            "showInterstitialAD : " + (curTime - interstitial.PreviousTime) + interstitial.Space);

        bool condition = immediate || curTime - interstitial.PreviousTime > interstitial.Space;

        if (IsInterstitialAndBannerCanShow(interstitial) && condition)
        {
            // 判断是否跳过广告
            if (SkipInterstitial)
            {
                SetInterstitialCloseAction(closeCallBack);
                HorstRillAdManager.Instance.interstitialCloseAction?.Invoke(placementID);
                interstitial.PreviousTime = Utility.getUnixTimestamp();
                return true;
            }

            HorstRillLog.Log(TAG, "Interstitial Should Show : " + placementID);

            if (IsInterstitialAdReady())
            {
                ShowInterstitialAd(placementID);
                SetInterstitialCloseAction(closeCallBack);
                HorstRillLog.Log(TAG, "Interstitial Show : " + placementID);
                return true;
            }
            else
            {
                closeCallBack?.Invoke(placementID, false);
                // 加载插屏广告
                LoadInterstitialAd();
                PuzzleTracking.Ads.AdsOpportunityMissing("interstitial", placementID, "not_ready");
            }
        }
        else
        {
            closeCallBack?.Invoke(placementID, false);
        }
        return false;
    }

    //激励广告
    public void showRewardedAD(string placementID,
        Action<string, AdInfo> shownCallBack = null,
        Action<string, AdInfo> clickCallBack = null,
        Action<string, AdInfo> closeCallBack = null,
        Action<string, Reward, AdInfo> rewardCallBack = null,
        Action<string, ErrorInfo, AdInfo> failCallBack = null)
    {
#if UNITY_EDITOR
        SkipAd = true;
#endif
        // 判断是否跳过广告
        if (SkipAd)
        {
            rewardCallBack?.Invoke(placementID, new Reward(), null);
            return;
        }

        // 尝试展示广告
        if (IsRewardAdReady())
        {
            SetRewardShowAction(shownCallBack);
            SetRewardClickAction(clickCallBack);
            SetRewardCloseAction(closeCallBack);
            SetRewardRewardedAction(rewardCallBack);
            SetRewardShowFailAction(failCallBack);

            ShowRewardAd(placementID);
            return;
        }

        LoadRewardAd();
        PuzzleTracking.Ads.AdsOpportunityMissing("rewarded", placementID, "not_ready");

        if (waitForRetry)
        {
            StartCoroutine(waitToShowRewardedAD(
                placementID, shownCallBack, clickCallBack,
                closeCallBack, rewardCallBack, failCallBack));
        }
    }

    public bool CanShowRewardAd(string adUnitID)
    {
        return true;
    }

    private IEnumerator waitToShowRewardedAD(
        string placementID,
        Action<string, AdInfo> shownCallBack = null,
        Action<string, AdInfo> clickCallBack = null,
        Action<string, AdInfo> closeCallBack = null,
        Action<string, Reward, AdInfo> rewardCallBack = null,
        Action<string, ErrorInfo, AdInfo> failCallBack = null)
    {
        float timeTick = 0f;
        showLoadingAction?.Invoke();

        yield return new WaitUntil(() =>
        {
            timeTick += Time.deltaTime;
            return IsRewardAdReady() || timeTick >= waitTimeForRetry;
        });
        closeLoadingAction?.Invoke();

        if (IsRewardAdReady())
        {
            SetRewardShowAction(shownCallBack);
            SetRewardClickAction(clickCallBack);
            SetRewardCloseAction(closeCallBack);
            SetRewardRewardedAction(rewardCallBack);
            SetRewardShowFailAction(failCallBack);

            ShowRewardAd(placementID);
        }
        else
        {
            failCallBack?.Invoke(placementID, null, null);
            showFailToastAction?.Invoke();
        }
    }

    // Banner广告
    public void showBannerAD(string placementID)
    {
        if (IsVip())
        {
            return;
        }

        if (IsInterstitialAndBannerCanShow())
        {
            ShowBanner(placementID);
#if UNITY_EDITOR
            AdaptiveBannerUtility.CreateBanner(GameEntry.UI.MainCanvas);
#endif
        }
    }

    public void hideBannerAD(string placementID)
    {
        HideBanner(placementID);
#if UNITY_EDITOR
        AdaptiveBannerUtility.DestroyBanner();
#endif
    }
}
