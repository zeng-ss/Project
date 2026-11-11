using System;
using System.Collections;
using System.Collections.Generic;
using CasualGame;
using Custom.Activity;
using KBbase;
using HorstRill.Ad;
using UnityEngine;
using PuzzleTrack;

public partial class AdManager : MonoBehaviour
{
    // Inter
    private bool IsInterstitialAdReady()
    {
        return HorstRillAdManager.Instance.IsInterstitialAdReady();
    }

    private void LoadInterstitialAd()
    {
        HorstRillAdManager.Instance.LoadInterstitialAd();
    }

    private void ShowInterstitialAd(string placementID)
    {
        PlaySoundManager.Instance.StopAudio();
        PuzzleTracking.Session.PauseTimer();
        HorstRillAdManager.Instance.ShowInterstitialAd(placementID);
        PuzzleTracking.Ads.NewPuzzleAdsImpression("INTER");
    }

    private void SetInterstitialCloseAction(Action<string, bool> closeCallBack)
    {
        HorstRillAdManager.Instance.interstitialCloseAction = (adInfo) =>
        {
            PuzzleTracking.Session.ResumeTimer();
            Interstitial.PreviousTime = Utility.getUnixTimestamp();
            Utility.AddIntersAdCount();
            PlaySoundManager.Instance.ResumeAudio();
            closeCallBack?.Invoke(adInfo, true);
        };
    }

    // Reward
    private bool IsRewardAdReady()
    {
        return HorstRillAdManager.Instance.IsRewardAdReady();
    }

    private void ShowRewardAd(string placementID)
    {
        PlaySoundManager.Instance.StopAudio();
        HorstRillAdManager.Instance.ShowRewardAd(placementID);
        PuzzleTracking.Ads.NewPuzzleAdsImpression("REWARDED");
    }

    private void LoadRewardAd()
    {
        HorstRillAdManager.Instance.LoadRewardAd();
    }

    private void SetRewardShowAction(Action<string, AdInfo> shownCallBack)
    {
        HorstRillAdManager.Instance.rewardShowAction = (placementId, info) =>
        {
            PuzzleTracking.Session.PauseTimer();
            shownCallBack?.Invoke(placementId, info);
        };
    }

    private void SetRewardClickAction(Action<string, AdInfo> clickCallBack)
    {
        HorstRillAdManager.Instance.rewardClickAction = clickCallBack;
    }

    private void SetRewardCloseAction(Action<string, AdInfo> closeCallBack)
    {
        HorstRillAdManager.Instance.rewardCloseAction = (json, info) =>
        {
            PuzzleTracking.Session.ResumeTimer();
            Interstitial.PreviousTime = Utility.getUnixTimestamp();
            PlaySoundManager.Instance.ResumeAudio();
            closeCallBack?.Invoke(json, info);
        };
    }

    private void SetRewardRewardedAction(Action<string, Reward, AdInfo> rewardedCallBack)
    {
        HorstRillAdManager.Instance.rewardRewardedAction = (json, reward, info) =>
        {
            Utility.AddRewardAdCount();
            PlaySoundManager.Instance.ResumeAudio();
            rewardedCallBack?.Invoke(json, reward, info);
        };
    }

    private void SetRewardShowFailAction(Action<string, ErrorInfo, AdInfo> failCallBack)
    {
        HorstRillAdManager.Instance.rewardShowFailAction = (msg, error, info) =>
        {
            PlaySoundManager.Instance.ResumeAudio();
            failCallBack?.Invoke(msg, error, info);
        };
    }

    // Banner
    private void ShowBanner(string placementID)
    {
        HorstRillAdManager.Instance.ShowBanner(placementID);
    }

    private void HideBanner(string placementID)
    {
        HorstRillAdManager.Instance.HideBanner(placementID);
    }

    public float GetAdaptiveBannerHeight()
    {
        return HorstRillAdManager.Instance.GetAdaptiveBannerHeight();
    }

    public float GetScreenDensity()
    {
        return HorstRillAdManager.Instance.GetScreenDensity();
    }

    // Web
    public bool CanShowWebViewIcon()
    {
        return HorstRillAdManager.Instance.CanShowWebViewIcon();
    }
}
