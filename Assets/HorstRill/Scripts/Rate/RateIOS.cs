using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HorstRill;

#if UNITY_IOS
using UnityEngine.iOS;

public class RateIOS : MonoBehaviour, IRate
{
    public void Show()
    {
        Device.RequestStoreReview();
        // KbSDK.Usage.record("rate_show");
    }

    public void Init()
    {
       
    }
}

#endif