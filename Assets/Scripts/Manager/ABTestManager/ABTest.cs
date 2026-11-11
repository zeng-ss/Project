using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using CasualGame;

namespace Ubase
{
    public static class ABTest
    {
        private const string TAG = "ABTest";

        /// <summary>
        /// 触发实验分组，每次只触发一个Diversion, 注意触发时机。
        /// 触发结果，默认组为0，对照组为1，实验组1为2，实验组2为3，以此类推...
        /// </summary>
        /// <param name="diversion">实验Key</param>
        /// <param name="totalExpNum">实验分组的数量，比如1个对照组和1个实验组，则为2</param>
        public static void triggerDiversion(string diversion, int totalExpNum, int flowPercent = 100)
        {
            if (BelongsToExperiment(diversion))
            {
                // KbLog.Log(TAG, "abtest has trigger : " + diversion);
                return;
            }

            if (string.IsNullOrEmpty(diversion))
            {
                // KbLog.LogE(TAG, "abtest invalid diversion : " + diversion);
                return;
            }
            else if (totalExpNum <= 1)
            {
                // KbLog.LogE(TAG, "abtest invalid exp num : " + totalExpNum);
                return;
            }
            else if (flowPercent < 0 || flowPercent > 100)
            {
                // KbLog.LogE(TAG, "abtest invalid flow percent : " + flowPercent);
                return;
            }

            System.Random rdFlow = new System.Random(Guid.NewGuid().GetHashCode());
            int flow = rdFlow.Next(0, 100);
            if (flow < flowPercent)
            {
                // 命中，进入实验
                System.Random rdExp = new System.Random(Guid.NewGuid().GetHashCode());
                int exp = rdExp.Next(1, totalExpNum + 1);
                GameEntry.Setting.SetInt(diversion, exp);
                // KbLog.Log(TAG, "abtest trigger : " + exp);

                // 埋点
                var dic = new Dictionary<string, object>();
                dic.Add("exp_name", diversion);
                dic.Add("exp_id", exp);
            }
            else
            {
                GameEntry.Setting.SetInt(diversion, 0);
                // KbLog.Log(TAG, "abtest trigger : " + 0);
                // 埋点
                var dic = new Dictionary<string, object>();
                dic.Add("exp_name", diversion);
                dic.Add("exp_id", 0);
            }
        }

        /// <summary>
        /// 判断当前用户是否属于某个实验
        /// </summary>
        /// <param name="diversion">实验Key</param>
        public static bool BelongsToExperiment(string diversion)
        {
            int exp = GameEntry.Setting.GetInt(diversion, -1);
            return exp != -1 && exp != 0;
        }

        /// <summary>
        /// 获取实验分组
        /// </summary>
        /// <param name="diversion">实验Key</param>
        /// <param name="defaultValue">默认值</param>
        public static int GetParamValue(string diversion, int defaultValue = 0)
        {
            int exp = GameEntry.Setting.GetInt(diversion, -1);
            return (exp != -1) ? exp : defaultValue;
        }
    }
}