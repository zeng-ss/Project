using System;
using UnityEngine;
using System.Collections.Generic;
#if !UNITY_WEBGL
using Facebook.Unity;
#endif
using CasualGame;
using HorstRill.Data;
using Newtonsoft.Json;
using PuzzleTrack;

namespace HorstRill
{
    [OPS.Obfuscator.Attribute.DoNotObfuscateClass]
    internal class HorstRillBehaviour : MonoBehaviour
    {
        private const string TAG = "HorstRill";
        public static HorstRillBehaviour _instance;
        private bool isATTAuthorized = false;

        // 归因
        public static List<string> appsFlyersOldIds = new List<string>()
        {
            "FantasyScrew_Unity_T1_D0_adROAS_20250119",
            "FantasyScrew_Unity_RUMXBR_D7_adROAS_20250126",
            "FantasyScrew_Unity_US_D7_adROAS_20250126",
            "FantasyScrew_Unity_JP_D7_adROAS_20250126",
            "FantasyScrew_Unity_US_D7_hybrid_ROAS_20250211_RR06newgif",
        };

        private void Awake()
        {
            if (transform != transform.root)
                throw new Exception("HorstRillSDK prefab HAS to be at the ROOT level!");

            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(this);
            HorstRillSdk.InitRoot(gameObject);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            // Brought forward after soft closing
            if (!pauseStatus)
            {
#if UNITY_IOS
                // 根据票据补发
                OnRevAppReceiptWhenAppStart("");
#endif
            }
        }

        public void InitAppsFlyer()
        {
        }

        public void Init()
        {
            HorstRillLog.Log(TAG, "init SDK in behaviour..");
            InitFacebook();
        }

        private void InitAd()
        {
            AdManager.Instance.Init();
        }

        private void InitFacebook()
        {
#if !UNITY_EDITOR && !UNITY_WEBGL
            LogHandler.Info("==> lyly InitFacebook start");
            if (!FB.IsInitialized)
            {
                LogHandler.Info("==> lyly InitFacebook start Init");
                FB.Init(InitFbCallback, OnHideUnity);
                LogHandler.Info("==> lyly InitFacebook Init complete");
            }
            else
            {
                InitFbCallback();
            }
#else
            InitFbCallback();
#endif
        }

        private void InitFbCallback()
        {
            LogHandler.Info("==> lyly InitFacebook InitFbCallback");
#if !UNITY_WEBGL
            if (FB.IsInitialized)
            {
                HorstRillLog.Log(TAG, "Initialize Facebook Success");

                _fixFBIOS();
                // Signal an app activation App Event
                FB.ActivateApp();
                sendFBEvent();

                // Continue with Facebook SDK
                // ...
                LogHandler.Info("==> lyly InitFacebook InitFbCallback Success");
            }
            else
            {
#if !UNITY_EDITOR
                HorstRillLog.LogE(TAG, "Failed to Initialize the Facebook SDK");
#endif
                LogHandler.Info("==> lyly InitFacebook InitFbCallback Failed to Initialize the Facebook SDK");
            }
#endif
            // 广告初始化
            InitAd();
        }

        private void _fixFBIOS()
        {
#if UNITY_IOS
            FB.Mobile.SetDataProcessingOptions(new string[] { });
            FB.Mobile.SetAdvertiserTrackingEnabled(isATTAuthorized);
            AudienceNetwork.AdSettings.SetDataProcessingOptions(new string[] { });
            AudienceNetwork.AdSettings.SetAdvertiserTrackingEnabled(isATTAuthorized);
            LogHandler.Info($"==> lyly ATT - fb tracking : {isATTAuthorized}");
#endif
        }

        private void OnHideUnity(bool isGameShown)
        {
            if (!isGameShown)
            {
                // Pause the game - we will need to hide
                Time.timeScale = 0;
            }
            else
            {
                // Resume the game - we're getting focus again
                Time.timeScale = 1;
            }
        }

        private void sendFBEvent()
        {
#if !UNITY_WEBGL
            if (FB.IsInitialized)
            {
                FB.LogAppEvent(AppEventName.ActivatedApp);
                FB.LogAppEvent("gusdk_start_event", 8);
            }
#endif
        }

        public void OnRevPurchasing(string productID)
        {
            LogHandler.Info($"==> lyly IAP - rev purchasing : {productID}");
            IAPManager.Instance.OnRevPurchasing(productID);
        }

        public void OnRevPurchasedProduct(string productID)
        {
            LogHandler.Info($"==> lyly IAP - rev : {productID}");
            IAPManager.Instance.OnRevPurchasedProduct(productID);
        }

        public void OnRevRestoredProduct(string productID)
        {
            LogHandler.Info($"==> lyly IAP - rev restored : {productID}");
            IAPManager.Instance.OnRevRestoredProduct(productID);
        }

        public void OnRevAppReceipt(string receipt)
        {
            //LogHandler.Info($"==> lyly IAP - rev receipt : {receipt}");
            LogHandler.Info($"==> lyly IAP - rev receipt");
            IAPManager.Instance.OnRevAppReceipt(receipt);
        }

        public void OnRevAppReceiptWhenAppStart(string receipt)
        {
            //LogHandler.Info($"==> lyly IAP - rev receipt on app start : {receipt}");
            LogHandler.Info($"==> lyly IAP - rev receipt on app start");
            IAPManager.Instance.OnRevAppReceiptWhenAppStart(receipt);
        }

        public void OnRevATTStatus(string status)
        {
            isATTAuthorized = !string.IsNullOrEmpty(status) && status.Equals("YES");
            LogHandler.Info($"==> lyly ATT - rev : {status}, {isATTAuthorized}");
        }

        // Firebase Remote Config
        public void OnFirebaseRemoteConfigFetchCompleted(string result)
        {
            Debug.Log($"==> Firebase HorstRill Firebase OnFirebaseRemoteConfigFetchCompleted : {result}");
            RemoteConfigManager.Instance.OnFetchCompleted(result);
        }

        // AppsFlyer
        public void OnConversionDataSuccess(string conversionData)
        {
            Dictionary<string, object> conversionDataDictionary = HorstRillDataManager.CallbackStringToDictionary(conversionData);
            // 埋点
            PuzzleTracking.Attribution.AfConversionData(conversionDataDictionary);

            LogHandler.Info("==> lyly AppsFlyer onConversionDataSuccess : " + JsonConvert.SerializeObject(conversionDataDictionary));


            // 数据判断
            if (conversionDataDictionary != null &&
                conversionDataDictionary.ContainsKey("network") &&
                conversionDataDictionary["network"] != null)
            {
                if (!conversionDataDictionary["network"].ToString().Equals("Organic"))
                {
                }
            }

            if (conversionDataDictionary != null &&
                conversionDataDictionary.ContainsKey("campaign") &&
                conversionDataDictionary["campaign"] != null)
            {
                string key = conversionDataDictionary["campaign"].ToString();
                for (int i = 0; i < appsFlyersOldIds.Count; i++)
                {
                    if (appsFlyersOldIds[i].ToLower().Equals(key.ToLower()))
                    {
                        key = string.Concat(key, "_Anime");
                        break;
                    }
                }
            }
        }

        public void OnConversionDataFail(string error)
        {
            LogHandler.Info($"** HorstRill AppsFlyer OnConversionDataFail : {error}");
        }
    }
}
