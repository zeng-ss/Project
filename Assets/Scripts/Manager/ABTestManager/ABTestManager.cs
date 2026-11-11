//------------------------------------------------------------
// File : ABTestManager.cs
// Email: mailto:zhiqiang.yang@kingboat.io
// Desc : 实验管理
//------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace CasualGame
{
    public enum ExperimentType
    {
        None = 0,
        Default = 1,
        Experiment1 = 2,
        Experiment2 = 3,
        Experiment3 = 4,
    }

    public class ABTestManager
    {
        public class ABData
        {
            public int div_bit = 1 << 1;
            public string diversion;
            public string name; // 中文实验名
            public int expGroupCount; // 实验组个数
            public int flowPercent; // 流量
            public int TEST_VALUE = -1;

            public ABData(int _bitDiv, string _diversion, string _name, int _expCount, int _flowPercent)
            {
                div_bit = _bitDiv;
                diversion = _diversion;
                name = _name;
                expGroupCount = _expCount;
                flowPercent = _flowPercent;
            }
        }

        private static int abInitState = 0;
        private static int bitCount = 0;
        public static Dictionary<string, ABData> abMap = new Dictionary<string, ABData>();

        public const string DIV_INTER_FIGHTING_ANDROID_TEST = "DIV_INTER_FIGHTING_ANDROID_TEST";

        public static void InitAB()
        {
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.OSXEditor)
            {
                AddABTest(DIV_INTER_FIGHTING_ANDROID_TEST, "战斗模式插屏实验", 1, 100); //对照组：50% 实验组：50%  
            }


            // CheckIsPurchaseVIPAndAddVIPABTest();

            foreach (var item in abMap)
            {
                string KEY = string.Format("TEST_{0}", item.Key);

                if (GameEntry.Setting.HasSetting(KEY))
                {
                    item.Value.TEST_VALUE = GameEntry.Setting.GetInt(KEY, -1);
                }
            }
        }


        // #if TEST

        private static void AddABTest(string diversion, string name, int expCount, int flowPercent)
        {
            bitCount++;
            abMap.Add(diversion, new ABData(1 << bitCount, diversion, name, expCount, flowPercent));
        }

        public static void SetDebugABTEST(string key, int value)
        {
            if (abMap.ContainsKey(key))
            {
                ABData _abdata = abMap[key];
                _abdata.TEST_VALUE = value;

                string KEY = string.Format("TEST_{0}", key);

                GameEntry.Setting.SetInt(KEY, value);
                GameEntry.Setting.Save();
            }
        }

        public static int GetDebugABTEST(string key)
        {
            if (abMap.ContainsKey(key))
            {
                string KEY = string.Format("TEST_{0}", key);
                return GameEntry.Setting.GetInt(KEY, -1);
            }
            else
            {
                return -1;
            }
        }
        // #endif

        private static int GetValue(string divKey)
        {
            ABData _abdata = abMap[divKey];

            if (_abdata.TEST_VALUE >= 0)
            {
                return _abdata.TEST_VALUE;
            }

            if (!GameFramework.Utility.Bitwise.IsBit(abInitState, _abdata.div_bit))
            {
                Ubase.ABTest.triggerDiversion(_abdata.diversion, _abdata.expGroupCount + 1, _abdata.flowPercent);
                GameFramework.Utility.Bitwise.SetBit(ref abInitState, _abdata.div_bit, true);
            }

            int _paramValue = Ubase.ABTest.GetParamValue(_abdata.diversion, 1);
            return _paramValue;
        }

        public static bool IsExperiment1(string divKey)
        {
            return (ExperimentType)GetValue(divKey) == ExperimentType.Experiment1;
        }

        #region Trigger

        public static bool HasOpenVideo => true;
        public static bool HasOpenSeq => true;
        public static bool HasCustomWait => true;

        // 获取Cash展示结果
        public static bool IsCashShowInPlayer => true;

        /// 新手礼包价格逻辑
        public static bool HasNewBiePack => true;

        /// 开始礼包价格逻辑
        public static int Get_DIV_STARTER_PACK_PRICE_TYPE
        {
            get
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    return 3;
                }
                else
                {
                    return 2;
                }
            }
        }

        /// <summary>
        /// 0 对照组 1实验安卓 2 实验IOS
        /// </summary>
        public static int STATE_DIV_INTER_TRIGGER_TIME_TEST
        {
            get
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    return 0; //安卓对照组
                }
                else
                {
                    //此处ios应用实验组
                    return 2;
                }
            }
        }

        public static bool STATE_DIV_INTER_FIGHTING_ANDROID_TEST
        {
            get
            {
                if (Application.platform == RuntimePlatform.Android ||
                    Application.platform == RuntimePlatform.OSXEditor)
                {
                    bool isExper = IsExperiment1(DIV_INTER_FIGHTING_ANDROID_TEST);
                    return isExper;
                }
                else
                {
                    return false;
                }
            }
        }

        /// 去广告价格实验
        public static bool NoForcedAdsPrice => false;

        public static bool IS_DIV_FIRST_PACK_TEST => true;

        public static bool IS_DIV_IAP_PRICE_TEST
        {
            get { return false; }
        }

        #endregion
    }
}