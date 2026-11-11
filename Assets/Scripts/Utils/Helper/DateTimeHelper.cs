//------------------------------------------------------------
// File : DateTimeHelper.cs
// Email: mailto:zhiqiang.yang
// Desc : 登陆、登出、跨天时间管理 （暂时用本地时间）
//------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;
using System;
using Custom.Activity;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CasualGame
{
    public static class DateTimeHelper
    {
        private static long theNextDayTimeStamp;//计算出下一天都时间搓
        public static long NowTimeStamp => TimeUtils.NowTimeStamp;

        public static void Login()
        {
            long nowTimeStamp = NowTimeStamp;
            //夸天处理
            long lastLoginTimestamp = ProxyManager.UserProxy.userModel.lastLoginTime;
            if (!TimeUtils.IsSameDay(TimeUtils.GetDateTime(lastLoginTimestamp)) && nowTimeStamp > lastLoginTimestamp)
            {
                ClearData();
            }
            ProxyManager.UserProxy.Login(nowTimeStamp);

            //下一天的时间戳
            theNextDayTimeStamp = TimeUtils.ConvertDateTimeLong(TimeUtils.GetNextDay());
            int offsetToNextDay = (int)(theNextDayTimeStamp - nowTimeStamp);
            if (offsetToNextDay > 0)
            {
                LogHandler.Info($"==> lyly 距离下一天还有 {offsetToNextDay}s  当前时间戳 {nowTimeStamp}  凌晨时间戳 {theNextDayTimeStamp}");
            }
            else
            {
                LogHandler.Info("修改了本地时间");
            }

            GameEntry.Base.AttachTimer(1, OneSecondCall, null, true, true);
            GameEntry.Setting.Save();
        }

        public static void LoginOut()
        {
            GameEntry.Setting.Save();
        }

        /// <summary>
        /// 非相同的天，清理数据
        /// </summary>
        private static void ClearData()
        {
            //清理每日免费礼包
            ProxyManager.Instance.ClearDataOnNewDay();
            ActivityManager.Instance.NewDay();
        }

        private static void OneSecondCall()
        {
            //每秒都检测更新
            if (NowTimeStamp - theNextDayTimeStamp > 0)
            {
                //跨天了
                OnNextDayCome(true);
            }
        }

        public static void OnNextDayCome(bool delayNotify = false)
        {
            ClearData();
            theNextDayTimeStamp = TimeUtils.ConvertDateTimeLong(TimeUtils.GetNextDay());//再次标记下一天都时间搓
            Log.Info("凌晨切换天");
        }
    }
}
