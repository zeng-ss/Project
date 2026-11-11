using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using CasualGame;

public class GLog
{
    public class Constant
    {
        public const string sdk_success = "sdk_success";
        public const string standrand = "standrand";
        public const string sdk = "sdk";
    }

    public static void offerwall_get_reward_balance(int balance)
    {
        Dictionary<string, object> dict = new Dictionary<string, object>
        {
            { "balance", balance },
        };
        _Record("offerwall_get_reward_balance", dict);
    }

    public static void unity_exception(string condition, string stackTrace)
    {
        Dictionary<string, object> dict = new Dictionary<string, object>
        {
            { "condition", condition },
            { "stack_trace", stackTrace }
        };
        _Record("unity_exception", dict);
    }

    public static void unity_trycache(string condition, string stackTrace)
    {
        Dictionary<string, object> dict = new Dictionary<string, object>
        {
            { "condition", condition },
            { "stack_trace", stackTrace }
        };
        _Record("unity_trycache", dict);
    }

    public static void remote_res_update(string action_value, int res_v, int res_s, int res_t, string res_type = "")
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();
        dict.Add("action_value", action_value);
        dict.Add("action_value_str", action_value);
        dict.Add("res_v", res_v);
        dict.Add("res_s", res_s);
        dict.Add("res_t", res_t);
        dict.Add("res_type", res_type);
        _Record("remote_res_update", dict);
    }

    public static void unity_error(string condition, string stackTrace)
    {
        Dictionary<string, object> dict = new Dictionary<string, object>
        {
            { "condition", condition },
            { "stack_trace", stackTrace }
        };
        _Record("unity_error", dict);
    }

    public static void unity_error_res(string name, string status)
    {
        Dictionary<string, object> dict = new Dictionary<string, object>
        {
            { "res_name", name },
            { "res_status", status }
        };
        _Record("unity_error_res", dict);
    }

    private static void _Record(string path, Dictionary<string, object> paras)
    {
        if (paras == null)
        {
            paras = new Dictionary<string, object>();
        }
        HorstRillSdk.Data.TrackEvent(path, paras);
    }

    private static void _RecordRevenue(string path, Dictionary<string, object> paras, string currencyType, float payAmount, float payAmountAdjust, bool toAdjust = true)
    {
        if (paras == null)
        {
            paras = new Dictionary<string, object>();
        }
        HorstRillSdk.Data.TrackRevenueEvent(path, paras, currencyType, payAmount);
    }

    public static void first_day_ad_revenue(string index)
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();
        _Record($"first_day_ad_revenue_{index}", dict);
    }

    public static void payment_init(string currency_type, float pay_amount, float payment_price_usd, string payment_name, string payment_id, string pay_source, string payment_id_vs)
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();
        dict["currency_type"] = currency_type;
        dict["pay_amount"] = pay_amount;
        dict["payment_price_usd"] = payment_price_usd;
        dict["payment_name"] = payment_name;
        dict["payment_id"] = payment_id;
        dict["payment_id_vs"] = payment_id_vs;
        dict["pay_source"] = pay_source;
        _Record("payment_init", dict);
    }

    public static void payment_fail(string currency_type, float pay_amount, string payment_name, string payment_id, string token, string pay_source, string fail_reason, string payment_id_vs)
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();
        dict["currency_type"] = currency_type;
        dict["pay_amount"] = pay_amount;
        dict["payment_name"] = payment_name;
        dict["payment_id"] = payment_id;
        dict["payment_id_vs"] = payment_id_vs;
        dict["pay_source"] = pay_source;
        dict["fail_reason"] = fail_reason;
        _Record("payment_fail", dict);
    }

    public static void payment_state_check(string state, string payment_id, string transaction_id, string user_type, string payment_id_vs)
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();
        dict["payment_state"] = state;
        dict["payment_id"] = payment_id;
        dict["payment_id_vs"] = payment_id_vs;
        dict["payment_transaction_id"] = transaction_id;
        dict["user_purchase_type"] = user_type;
        _Record("payment_state_check", dict);
    }

    public static void payment_success(string currency_type, float pay_amount, float payment_price_usd, string payment_name, string payment_id, bool is_first_pay, int pay_type, string pay_source, string transaction_id, string payment_id_vs)
    {
        // 是否上报Adjust
        bool toAdjust = true;
        float amountAdjust = pay_amount;
        try
        {
            // 判断累计金额
            // float targetMax = GameEntry.Config.GetFloat(Constant.Config.IapAdjustMax, 50f);
            // toAdjust = ProxyManager.UserProxy.Model.totalPayAmountUsd < targetMax;
            // // 处理单笔金额
            // if (payment_price_usd > 2.99f)
            // {
            //     int discount = GameEntry.Config.GetInt(Constant.Config.IapAdjustDiscount, 30);
            //     amountAdjust = pay_amount * (100 - discount) / 100;
            // }
        }
        catch (Exception e)
        {
            Debug.LogError($"PaymentSuccess : {e.Message} - {e.StackTrace}");
            toAdjust = true;
        }
        // 内购统计
        ProxyManager.UserProxy.AddPayment(pay_amount, payment_price_usd, currency_type);
        // 埋点
        Dictionary<string, object> dict = new Dictionary<string, object>();
        dict["currency_type"] = currency_type;
        dict["pay_amount"] = pay_amount;
        dict["payment_price_usd"] = payment_price_usd;
        dict["payment_name"] = payment_name;
        dict["payment_id"] = payment_id;
        dict["payment_id_vs"] = payment_id_vs;
        dict["is_first_pay"] = is_first_pay;
        dict["pay_type"] = pay_type;
        dict["pay_source"] = pay_source;
        dict["payment_transaction_id"] = transaction_id;
        _RecordRevenue("payment_success", dict, currency_type, pay_amount, amountAdjust, toAdjust);
#if UNITY_IOS
        // 上报FB
        try
        {
            // int fbTrack = GameEntry.Config.GetInt(CasualGame.Constant.Config.IapFbEnableTrack, 0);
            // if (fbTrack == 1)
            // {
            //     Facebook.Unity.FB.LogPurchase(pay_amount, currency_type);
            // }
        }
        catch (Exception e)
        {
            Debug.LogError($"PaymentSuccess : {e.Message} - {e.StackTrace}");
        }
#endif
    }

    public static void subscribe_state_check(string type, bool state)
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();
        dict["vip_check_type"] = type;
        dict["vip_state"] = state;
        _Record("subscribe_state_check", dict);
    }

    public static void transaction_check(string transaction_id, int fail_type, string msg = "")
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();
        dict.Add("transaction_id", transaction_id);
        dict.Add("fail_type", fail_type);
        dict.Add("message", msg);
        _Record("transaction_check", dict);
    }

    public static void notification_rec(string content, int id)
    {
        var dict = new Dictionary<string, object>();
        dict["content"] = content;
        dict["id"] = id;
        _Record("notification_rec", dict);
    }

    public static void notification_open(string content, int id)
    {
        var dict = new Dictionary<string, object>();
        dict["content"] = content;
        dict["id"] = id;
        _Record("notification_open", dict);
    }

    public static void mediation_ad_show(Dictionary<string, object> dict)
    {
        LogHandler.Info("==> lyly Glog mediation_ad_show");
        HorstRillSdk.Data.TrackEvent("mediation_ad_show", dict);
    }

    public static void first_load_m_ad_event(Dictionary<string, object> dict)
    {
        LogHandler.Info("==> lyly Glog first_load_m_ad_event");
        HorstRillSdk.Data.TrackEvent("first_load_m_ad_event", dict);
    }

    public static void load_max_ad_event(Dictionary<string, object> dict)
    {
        LogHandler.Info("==> lyly Glog load_max_ad_event");
        HorstRillSdk.Data.TrackEvent("load_max_ad_event", dict);
    }

    public static void af_conversion_data(Dictionary<string, object> data)
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();
        if (data.ContainsKey("af_status")) dict["af_cv_status"] = data["af_status"];
        if (data.ContainsKey("media_source")) dict["af_cv_media_source"] = data["media_source"];
        if (data.ContainsKey("is_first_launch")) dict["af_cv_is_first_launch"] = data["is_first_launch"];
        if (data.ContainsKey("campaign")) dict["af_cv_campaign"] = data["campaign"];
        if (data.ContainsKey("campaign_id")) dict["af_cv_campaign_id"] = data["campaign_id"];
        _Record("af_conversion_data", dict);
    }

    private static readonly HashSet<int> triggerLevels = new HashSet<int>
    {
        1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95, 100, 150, 200, 300, 400, 500, 600, 700, 800, 900, 1000, 1500, 2000, 3000, 4000, 5000
    };

    public static void s_level(int id)
    {
        int currentLevel = id;
        if (triggerLevels.Contains(currentLevel))
        {
            var dict = new Dictionary<string, object>();
            _Record($"s_level_{currentLevel}", dict);
        }
    }

    public static void ai_puzzle_level_start(int level)
    {
        var dict = new Dictionary<string, object>();
        dict["level"] = level;
        dict["level_mode"] = "level_start";
        _Record("ai_puzzle_level", dict);
    }

    public static void ai_puzzle_level_complete(int level, int duration, int playCnt)
    {
        var dict = new Dictionary<string, object>();
        dict["level_mode"] = "level_complete";
        dict["level"] = level;
        dict["duration"] = duration;
        dict["play_cnt"] = playCnt;
        _Record("ai_puzzle_level", dict);
    }

    public static void ai_puzzle_level_fail(int level, int duration)
    {
        var dict = new Dictionary<string, object>();
        dict["level_mode"] = "level_fail";
        dict["level"] = level;
        dict["duration"] = duration;
        _Record("ai_puzzle_level", dict);
    }

    public static void s_guide_swipe(string state)
    {
        var dict = new Dictionary<string, object>();
        dict["state"] = state;
        _Record("s_guide_swipe", dict);
    }

    public static void word_pet_res(string action_type, int action_value)
    {
        var dict = new Dictionary<string, object>();
        dict["action_type"] = action_type;
        dict["action_value"] = action_value;
        _Record("word_pet_res", dict);
    }

    private static readonly HashSet<int> triggerAds = new HashSet<int>
    {
        10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160, 170, 180, 190, 200, 300, 400, 500, 600, 700
    };

    public static void ap_ua_ads(int total_ad)
    {
        if (triggerAds.Contains(total_ad))
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            _Record($"ap_ua_ads_{total_ad}", dict);
        }
    }

    public static void s_pack_purchase(string pack, float value)
    {
        if (string.IsNullOrEmpty(pack)) return;

        Dictionary<string, object> dict = new Dictionary<string, object>()
        {
            { "value", value },
        };
        _RecordRevenue(pack, dict, "USD", value, value);
    }

    public static async void record_all()
    {
        foreach (var item in triggerLevels)
        {
            s_level(item);
            ai_puzzle_level_complete(item, 0, 1);
            await Cysharp.Threading.Tasks.UniTask.Delay(3000);
        }
        foreach (var item in triggerAds)
        {
            ap_ua_ads(item);
            await Cysharp.Threading.Tasks.UniTask.Delay(3000);
        }

        HorstRillSdk.Iap.SkipPurchase = true;
        GameFramework.DataTable.IDataTable<DRItemIAP> itemIAPTable = GameEntry.DataTable.GetDataTable<DRItemIAP>();
        foreach (DRItemIAP item in itemIAPTable.GetAllDataRows())
        {
            if (item.Platform != 2) continue;

            bool isSuccess = false;
            HorstRillSdk.Iap.BuyProductID(item, IAPSourceType.none, (string productId) =>
            {
                isSuccess = true;
            },
            (string productId, string error) =>
            {
                Debug.LogError("购买失败 " + error);
            });

            await Cysharp.Threading.Tasks.UniTask.WaitUntil(() => isSuccess);
        }
    }
}