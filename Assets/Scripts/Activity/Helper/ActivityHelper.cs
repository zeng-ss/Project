//------------------------------------------------------------
// File : ActivityHelper.cs
// Email: yang.li@kingboat.io
// Desc : 
//------------------------------------------------------------
using System;
using CasualGame;
using UnityEngine;

namespace Custom.Activity
{
    public static class ActivityHelper
    {
        /// <summary>
        /// 程序元年开启时间戳：2025-08-01 00:00:00 UTC（Unix 秒）
        /// </summary>
        private static readonly long EpochStartTimestamp = 1754006400;

        private static long GetCurrentTimestamp()
        {
            return GameManager.Instance.GetTimeStampOfSeconds();
        }

        private static bool CheckLevelCondition(this DRActivityInfo dRActivityInfo, int currentLevel)
        {
            return currentLevel >= dRActivityInfo.UnlockLv;
        }

        /// <summary> 与总秒数同号；a%b 在 C# 中可能为负，统一成 [0, b) </summary>
        private static long ModPositive(long a, long b)
        {
            if (b == 0) return 0;
            long m = a % b;
            return m < 0 ? m + b : m;
        }

        private static long GetCycleLengthSeconds(this DRActivityInfo info)
        {
            return (long)(info.Duration + info.Interval) * 3600L;
        }

        private static long GetActivityWindowSeconds(this DRActivityInfo info)
        {
            return (long)info.Duration * 3600L;
        }

        private static bool CheckTimeCondition(this DRActivityInfo dRActivityInfo, long currentTimestamp)
        {
            if (dRActivityInfo.Duration == -1)
            {
                return true;
            }

            long t = currentTimestamp - EpochStartTimestamp;
            if (t < 0)
            {
                return false;
            }

            long l = dRActivityInfo.GetCycleLengthSeconds();
            long d = dRActivityInfo.GetActivityWindowSeconds();
            if (l <= 0 || d < 0 || d > l)
            {
                return false;
            }

            long pos = ModPositive(t, l);
            return pos < d;
        }

        private static int GetCurrentCycleNumber(this DRActivityInfo dRActivityInfo, long currentTimestamp)
        {
            if (dRActivityInfo.Duration == -1)
            {
                Debug.LogError("活动永久开启, 请从内部控制CycleNumber");
                return 0;
            }

            long t = currentTimestamp - EpochStartTimestamp;
            if (t < 0)
            {
                return -1;
            }

            long l = dRActivityInfo.GetCycleLengthSeconds();
            if (l <= 0)
            {
                Debug.LogError("活动周期时长无效, 请从内部控制CycleNumber");
                return -1;
            }

            return (int)(t / l);
        }

        /// <summary>
        /// 返回<strong>绝对 Unix 秒</strong>：下一次「周期起点」= 下一段可开启窗的开始（与 <see cref="GetCurrentEndTimestamp"/> 同一套 k·L 划分）。
        /// 要「距现在剩余秒」请用 <c>GetNextStartTimestamp() - 当前时间</c>，见 <see cref="AbstractActivity.GetNextRoundRemainTimeText"/>。
        /// </summary>
        private static long GetNextStartTimestamp(this DRActivityInfo dRActivityInfo, long currentTimestamp)
        {
            if (dRActivityInfo.Duration == -1)
            {
                return -1;
            }

            long l = dRActivityInfo.GetCycleLengthSeconds();
            long d = dRActivityInfo.GetActivityWindowSeconds();
            if (l <= 0 || d > l)
            {
                return -1;
            }

            long t = currentTimestamp - EpochStartTimestamp;
            if (t < 0)
            {
                return EpochStartTimestamp;
            }

            long k = t / l;
            return EpochStartTimestamp + (k + 1) * l;
        }

        /// <summary>
        /// 当前时间落在「开启且 UTC 与所有人同一套 Epoch」时，返回距离<strong>本段开启窗口结束</strong>的剩余秒数；否则 -1。
        /// 以秒为周期与窗口长度，与 AbstractActivity/UTC0 时间线一致，避免用「整点小时」截断导致边界偏差。
        /// </summary>
        private static long GetCurrentEndTimestamp(this DRActivityInfo dRActivityInfo, long currentTimestamp)
        {
            if (dRActivityInfo.Duration == -1)
            {
                return -1;
            }

            if (!dRActivityInfo.CheckTimeCondition(currentTimestamp))
            {
                return -1;
            }

            long t = currentTimestamp - EpochStartTimestamp;
            long l = dRActivityInfo.GetCycleLengthSeconds();
            long d = dRActivityInfo.GetActivityWindowSeconds();
            if (l <= 0 || t < 0 || d > l) return -1;

            long k = t / l;
            long pos = ModPositive(t, l);
            if (pos >= d) return -1;

            long activityEndAbs = EpochStartTimestamp + k * l + d;
            return activityEndAbs - currentTimestamp;
        }

        /// <summary>
        /// 获取当前活动周期数
        /// </summary>
        /// <param name="dRActivityInfo"></param>
        /// <returns></returns>
        public static int GetCurrentCycleNumber(this DRActivityInfo dRActivityInfo)
        {
            return dRActivityInfo.GetCurrentCycleNumber(GetCurrentTimestamp());
        }

        /// <summary>
        /// 判断是否满足时间条件
        /// </summary>
        /// <param name="dRActivityInfo"></param>
        /// <returns></returns>
        public static bool CheckTimeCondition(this DRActivityInfo dRActivityInfo)
        {
            return dRActivityInfo.CheckTimeCondition(GetCurrentTimestamp());
        }

        /// <summary>
        /// 下次活动周期起点的 <strong>Unix 时间戳（秒）</strong>，非倒數秒。UI 用剩余秒请自减 <see cref="GetCurrentTimestamp"/> 或 <see cref="AbstractActivity.GetNextRoundRemainTimeText"/>。
        /// </summary>
        public static long GetNextStartTimestamp(this DRActivityInfo dRActivityInfo)
        {
            return dRActivityInfo.GetNextStartTimestamp(GetCurrentTimestamp());
        }

        /// <summary>
        /// 获取当前活动剩余时间（距离结束的秒数）
        /// </summary>
        /// <param name="dRActivityInfo"></param>
        /// <returns></returns>
        public static long GetCurrentEndTimestamp(this DRActivityInfo dRActivityInfo)
        {
            return dRActivityInfo.GetCurrentEndTimestamp(GetCurrentTimestamp());
        }

        /// <summary>
        /// 判断是否满足等级条件
        /// </summary>
        /// <param name="dRActivityInfo"></param>
        /// <param name="currentLevel"></param>
        /// <returns></returns>
        public static bool IsLevelCondition(this DRActivityInfo dRActivityInfo, int currentLevel)
        {
            return dRActivityInfo.CheckLevelCondition(currentLevel);
        }

        /// <summary>
        /// 活动是否开启
        /// </summary>
        /// <param name="dRActivityInfo"></param>
        /// <param name="currentLevel"></param>
        /// <param name="currentTimestamp"></param>
        /// <returns></returns>
        public static bool IsTimeCondition(this DRActivityInfo dRActivityInfo)
        {
            return dRActivityInfo.CheckTimeCondition(GetCurrentTimestamp());
        }

        /// <summary>
        /// 判断是否满足等级和时间条件
        /// </summary>
        /// <param name="dRActivityInfo"></param>
        /// <param name="currentLevel"></param>
        /// <returns></returns>
        public static bool IsLevelAndTimeCondition(this DRActivityInfo dRActivityInfo, int currentLevel)
        {
            return IsLevelCondition(dRActivityInfo, currentLevel) && IsTimeCondition(dRActivityInfo);
        }

        /// <summary>
        /// 获取活动条件类型
        /// </summary>
        /// <param name="dRActivityInfo"></param>
        /// <returns></returns>
        public static ActivityConditionType GetActivityConditionType(this DRActivityInfo dRActivityInfo)
        {
            return (ActivityConditionType)dRActivityInfo.ConditionType;
        }
    }
}