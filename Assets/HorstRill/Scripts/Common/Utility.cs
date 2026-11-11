using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using PuzzleTrack;

namespace KBbase
{
    public class Utility
    {

        public const string PROTOCAL_HTTPS = "https://";
        public const string PROTOCAL_HTTP = "http://";
#if UNITY_ANDROID && !UNITY_EDITOR
        private static AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        private static AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
#endif

        public static void SetAdShowCount(string adId)
        {
            string currentKey = DateTime.Now.ToShortDateString().Replace('/', '_') + "_" + adId;
            int adShownCount = PlayerPrefs.GetInt(currentKey, 0);

            Debug.Log(currentKey);

            PlayerPrefs.SetInt(currentKey, adShownCount + 1);
        }

        public static int GetAdShownCount(string adId)
        {
            string currentKey = DateTime.Now.ToShortDateString().Replace('/', '_') + "_" + adId;
            return PlayerPrefs.GetInt(currentKey, 0);
        }

        private const string KB_TOTAL_AD_NUM_REWARDED = "KB_TOTAL_AD_NUM_REWARDED";
        private const string KB_TOTAL_AD_NUM_INTERS = "KB_TOTAL_AD_NUM_INTERS";
        private const string KB_TOTAL_AD_NUM = "KB_TOTAL_AD_NUM";

        public static void AddRewardAdCount()
        {
            int count = GetRewardAdCount();
            int total = GetTotalAdCount();
            PlayerPrefs.SetInt(KB_TOTAL_AD_NUM_REWARDED, count + 1);
            PlayerPrefs.SetInt(KB_TOTAL_AD_NUM, total + 1);
            // 接口上报
            UploadAdData(total + 1);
        }

        public static int GetRewardAdCount()
        {
            return PlayerPrefs.GetInt(KB_TOTAL_AD_NUM_REWARDED, 0);
        }

        public static void AddIntersAdCount()
        {
            int count = GetIntersAdCount();
            int total = GetTotalAdCount();
            PlayerPrefs.SetInt(KB_TOTAL_AD_NUM_INTERS, count + 1);
            PlayerPrefs.SetInt(KB_TOTAL_AD_NUM, total + 1);
            // 接口上报
            UploadAdData(total + 1);
        }

        public static int GetIntersAdCount()
        {
            return PlayerPrefs.GetInt(KB_TOTAL_AD_NUM_INTERS, 0);
        }

        public static int GetTotalAdCount()
        {
            return PlayerPrefs.GetInt(KB_TOTAL_AD_NUM, 0);
        }

        public static void SetAdCountDebug(int adCount)
        {
            PlayerPrefs.SetInt(KB_TOTAL_AD_NUM, adCount);
            UploadAdData(adCount);
        }

        public static void SetInterCountDebug(int adCount)
        {
            PlayerPrefs.SetInt(KB_TOTAL_AD_NUM_INTERS, adCount);
        }

        // 上报广告数据
        public static void UploadAdData(int count)
        {
            PuzzleTracking.Ads.ApUaAds(count);

            // RequestManager.Post<bool>($"/user/ads/count?count={count}", new object(), (data) =>
            // {
            //     // 事件
            //     EventManager.Instance.NotifyEvent(Constant.Event.AfterShowAd);

            // }, (err) => { });

            // // 事件检测
            // UACManager.Instance.CheckUAC25Event();
            // UACManager.Instance.CheckUAC30Event();
        }


        public static AndroidJavaObject ToJavaList(List<string> list)
        {
            AndroidJavaObject ret = new AndroidJavaObject("java.util.ArrayList");
            foreach (string s in list)
            {
                ret.Call<bool>("add", s);
            }
            return ret;
        }

        public static Dictionary<string, string> JListToDictionary(AndroidJavaObject hashMap)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            AndroidJavaObject itertor = hashMap.Call<AndroidJavaObject>("keySet").Call<AndroidJavaObject>("iterator");
            while (itertor.Call<bool>("hasNext"))
            {
                string key = itertor.Call<string>("next");
                string value = hashMap.Call<string>("get", key);
                dic.Add(key, value);
            }
            return dic;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        public static AndroidJavaObject Dictionary2Map(IDictionary dict)
        {
            AndroidJavaObject ret = new AndroidJavaObject("java.util.HashMap");
            foreach (string key in dict.Keys)
            {
                if (dict[key] is IDictionary)
                {
                    ret.Call<AndroidJavaObject>("put", key, Dictionary2Map((IDictionary)dict[key]));
                }
                else
                {
                    if (dict[key] is int)
                    {
                        AndroidJavaObject intObj = new AndroidJavaObject("java.lang.Integer", new object[] { dict[key].ToString() });
                        ret.Call<AndroidJavaObject>("put", key, intObj);
                    }
                    else if (dict[key] is float)
                    {
                        AndroidJavaObject floatObj = new AndroidJavaObject("java.lang.Float", new object[] { dict[key].ToString() });
                        ret.Call<AndroidJavaObject>("put", key, floatObj);
                    }
                    else if (dict[key] is double)
                    {
                        AndroidJavaObject doubleObj = new AndroidJavaObject("java.lang.Double", new object[] { dict[key].ToString() });
                        ret.Call<AndroidJavaObject>("put", key, doubleObj);
                    }
                    else if (dict[key] is long)
                    {
                        AndroidJavaObject longObj = new AndroidJavaObject("java.lang.Long", new object[] { dict[key].ToString() });
                        ret.Call<AndroidJavaObject>("put", key, longObj);
                    }
                    else if (dict[key] is bool)
                    {
                        AndroidJavaObject boolObj = new AndroidJavaObject("java.lang.Boolean", new object[] { dict[key].ToString() });
                        ret.Call<AndroidJavaObject>("put", key, boolObj);
                    }
                    else
                    {
                        ret.Call<AndroidJavaObject>("put", key, dict[key]);
                    }

                }
            }
            return ret;
        }
#elif UNITY_IOS
#elif UNITY_2018
#endif

        /// <summary>
        /// Gets the type of the network.
        /// </summary>
        /// <returns>The network type. Refer to the begining of this file</returns>
        public static string GetDomain()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJavaObject androidJavaObject = new AndroidJavaClass("com.wwkk.business.utils.DavinciHelper").GetStatic<AndroidJavaObject>("INSTANCE");
    		return androidJavaObject.Call<string>("getDomain");
#elif UNITY_IOS
            return null;
#elif UNITY_2018
            return null;
#else
            return null;
#endif
        }

        /// <summary>
        /// Gets the type of the network.
        /// </summary>
        /// <returns>The network type. Refer to the begining of this file</returns>
        public static string GetCDNDomain()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJavaObject androidJavaObject = new AndroidJavaClass("com.wwkk.business.locating.ServerLocator").GetStatic<AndroidJavaObject>("INSTANCE");
            return androidJavaObject.CallStatic<string>("getCDNServerAddress");
#elif UNITY_IOS
            return null;
#elif UNITY_2018
            return null;
#else
            return null;
#endif
        }

        public static string GetChannelCode()
        {
            return "000000";
        }

        public static string Dictionary2Json(IDictionary dict)
        {
            string jsonString = JsonConvert.SerializeObject(dict);
            return jsonString;
        }

        /// <summary>
        /// Gets the unix timestamp.
        /// </summary>
        /// <returns>The unix timestamp.</returns>
        public static long getUnixTimestamp()
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            long timeStamp = (long)(DateTime.Now - startTime).TotalSeconds;
            return timeStamp;
        }

        public static long getMillsTimestamp()
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            long timeStamp = (long)(DateTime.Now - startTime).TotalMilliseconds;
            return timeStamp;
        }

        public static Dictionary<string, string> ToDictionary(IDictionary dictionary)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            foreach (DictionaryEntry item in dictionary)
            {
                dict.Add(item.Key.ToString(), item.Value.ToString());
            }

            return dict;
        }

        public static string iOSDeviceType()
        {
#if UNITY_ANDROID
            float physicscreen = Mathf.Sqrt(Screen.width * Screen.width + Screen.height * Screen.height) / Screen.dpi;
            if (physicscreen >= 7f)
            {
                return "AndroidPad";
            }
            else
            {
                return "AndroidPhone";
            }
#elif UNITY_IOS
            try
            {
                if (UnityEngine.iOS.Device.generation.ToString().Substring(0, 3) == "iPh")
                {
                    return "iPhone";
                }
                else
                {
                    return "iPad";
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                Debug.Log(e.InnerException.Message);
            }
#endif
            return "Others";
        }
    }
}
