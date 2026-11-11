using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using CasualGame;

public partial class AdManager : MonoBehaviour
{
    [Serializable]
    public class InterstitialAdConfig
    {
        public InterstitialData ios;
        public InterstitialData android;
    }

    [Serializable]
    public class InterstitialData
    {
        public int start_lv;
        public int cold;
        public string normal;
        public string iap;
        public int adrevenue_rate;
    }

    private const string key_InterstitialADConfig = "InterstitialADConfig";
    private const string default_InterstitialADConfig = "{\"ios\":{\"start_lv\":12,\"cold\":80,\"normal\":\"10,90;15,80;50,70;1000,60\",\"iap\":\"1000,90\",\"adrevenue_rate\":100},\"android\":{\"start_lv\":12,\"cold\":80,\"normal\":\"10,90;15,80;50,70;1000,60\",\"iap\":\"1000,90\",\"adrevenue_rate\":100}}";

    private void ChangeInterstitialInterval()
    {
        string interstitialAdConfig = RemoteConfigManager.Instance.GetStringValue(key_InterstitialADConfig, default_InterstitialADConfig);
        Debug.Log("==> [AdManager] remote admanager config: " + interstitialAdConfig);

        if (TryApplyInterstitialConfig(interstitialAdConfig))
        {
            return;
        }

        if (interstitialAdConfig != default_InterstitialADConfig)
        {
            Debug.LogError("==> [AdManager] remote/cached admanager config invalid, try built-in default config");
            if (TryApplyInterstitialConfig(default_InterstitialADConfig))
            {
                return;
            }
        }

        Debug.LogError(
            $"==> [AdManager] built-in default admanager config invalid, use constant fallback: " +
            $"interval={Constant.Ad.interstitialInterval}, " +
            $"coldStart={Constant.Ad.interstitialColdstart}, startLevel=12");
        SetInterstitialState(
            Constant.Ad.interstitialInterval,
            Constant.Ad.interstitialColdstart,
            12);
    }

    private bool TryApplyInterstitialConfig(string interstitialAdConfig)
    {
        try
        {
            InterstitialAdConfig config = JsonConvert.DeserializeObject<InterstitialAdConfig>(interstitialAdConfig);
            Debug.Log("==> [AdManager] remote admanager config DeserializeObject success!");
#if UNITY_IOS
            var adConfig = config.ios;
#else
            var adConfig = config.android;
#endif
            if (adConfig == null)
            {
                return false;
            }

            int result = 0;
            int maxLevelInterval = 0;
            int coldStart = adConfig.cold;
            var isIap = ProxyManager.UserProxy.IsIapPlayer();
            string[] data = null;
            if (isIap)
                data = adConfig.iap.Split(';');
            else
                data = adConfig.normal.Split(';');

            int level = 1;
            int curLevel = level;
            if (data != null)
            {
                int maxLevel = 0;
                for (int i = data.Length - 1; i >= 0; i--)
                {
                    var item = data[i].Split(',');
                    if (int.TryParse(item[0], out level) && int.TryParse(item[1], out int interval))
                    {
                        if (curLevel < level)
                            result = interval;

                        if (maxLevel < level)
                        {
                            maxLevel = level;
                            maxLevelInterval = interval;
                        }
                    }
                }
            }

            adrevenue_rate = adConfig.adrevenue_rate;
            if (result <= 0)
                result = maxLevelInterval > 0 ? maxLevelInterval : Constant.Ad.interstitialInterval;

            if (coldStart <= 0)
                coldStart = Constant.Ad.interstitialColdstart;

            Debug.Log("==> remote admanager interval: " + result + " coldStart: " + coldStart);

            SetInterstitialState(result, coldStart, adConfig.start_lv);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("==> remote admanager config DeserializeObject failed: " + e.Message);
            return false;
        }
    }
}
