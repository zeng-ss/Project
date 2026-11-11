using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HorstRill;

#if UNITY_ANDROID
using Google.Play.Review;

public class RateAndroid : MonoBehaviour, IRate
{
    private ReviewManager _reviewManager;
    private PlayReviewInfo _playReviewInfo;

    public void Show()
    {
        StartCoroutine(ShowReview());
    }

    public void Init()
    {
        _reviewManager = new ReviewManager();
    }

    private IEnumerator ShowReview()
    {
        LogHandler.Info("GooglePlay.ShowReview");
        var requestFlowOperation = _reviewManager.RequestReviewFlow();
        yield return requestFlowOperation;
        if (requestFlowOperation.Error != ReviewErrorCode.NoError)
        {
            // Log error. For example, using requestFlowOperation.Error.ToString().
            LogHandler.Info("GooglePlay.ReviewErrorCode1");
            yield break;
        }
        _playReviewInfo = requestFlowOperation.GetResult();
        LogHandler.Info("GooglePlay._playReviewInfo " + _playReviewInfo);

        var launchFlowOperation = _reviewManager.LaunchReviewFlow(_playReviewInfo);
        yield return launchFlowOperation;
        _playReviewInfo = null; // Reset the object
        if (launchFlowOperation.Error != ReviewErrorCode.NoError)
        {
            // Log error. For example, using requestFlowOperation.Error.ToString().
            LogHandler.Info("GooglePlay.ReviewErrorCode2");
            yield break;
        }
        // KbSDK.Usage.record("rate_show");
        LogHandler.Info("GooglePlay.ShowReviewFinish：" + requestFlowOperation.Error.ToString() + ",:" + launchFlowOperation.Error);

    }
}

#endif