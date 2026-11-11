//------------------------------------------------------------
// File : DevicePerformanceUtil.cs
// Email: mailto:zhuofeng.cai@kingboat.io
// Desc : 
//------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CasualGame
{
    /// <summary>
    /// 硬件设备性能适配工具
    /// </summary>
    public static class DevicePerformanceUtil
    {
        public static QualityLevel CurrentQualityLevel = QualityLevel.Low;

        /// <summary>
        /// 获取设备性能评级
        /// </summary>
        /// <returns>性能评级</returns>
        public static QualityLevel GetDevicePerformanceLevel()
        {
            if (SystemInfo.graphicsDeviceVendorID == 32902)
            {
                //集显
                return QualityLevel.Low;
            }
            else //NVIDIA系列显卡(N卡)和AMD系列显卡
            {
                //根据目前硬件配置三个平台设置了不一样的评判标准(仅个人意见)
                //CPU核心数
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                if (SystemInfo.processorCount <= 2)
#elif UNITY_STANDALONE_OSX || UNITY_IPHONE
                if (SystemInfo.processorCount < 2)
#elif UNITY_ANDROID
                if (SystemInfo.processorCount <= 4)
#else
                if (SystemInfo.processorCount <= 2)
#endif
                {
                    //CPU核心数<=2判定为低端
                    return QualityLevel.Low;
                }
                else
                {
                    //显存
                    int graphicsMemorySize = SystemInfo.graphicsMemorySize;
                    //内存
                    int systemMemorySize = SystemInfo.systemMemorySize;
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                    return QualityLevel.High;
#elif UNITY_STANDALONE_OSX || UNITY_IPHONE
                    if (systemMemorySize >= 6000)
                        return QualityLevel.High;
                    else if (systemMemorySize >= 4000)
                        return QualityLevel.Mid;
                    else
                        return QualityLevel.Low;
#elif UNITY_ANDROID
                    if (graphicsMemorySize >= 6000 && systemMemorySize >= 8000)
                        return QualityLevel.High;
                    else if (graphicsMemorySize >= 2000 && systemMemorySize >= 4000)
                        return QualityLevel.Mid;
                    else
                        return QualityLevel.Low;
#else
                    return QualityLevel.High;
#endif
                }
            }
        }

        /// <summary>
        /// 根据机型配置自动设置质量
        /// </summary>
        public static void ModifySettingsBasedOnPerformance()
        {
            bool shouldChangeResolution = false;
            int pixelWidth = Display.main.systemWidth;

            QualityLevel level = GetDevicePerformanceLevel();
            switch (level)
            {
                case QualityLevel.Low:
                    if (pixelWidth > 750)
                    {
                        shouldChangeResolution = true;
                        pixelWidth = 750;
                    }

                    Application.targetFrameRate = 30;
                    break;
                case QualityLevel.Mid:
                    if (pixelWidth > 1280)
                    {
                        shouldChangeResolution = true;
                        pixelWidth = 1280;
                    }

                    Application.targetFrameRate = 60;
                    break;
                case QualityLevel.High:
                    Application.targetFrameRate = 60;
                    break;
            }

            SetQualitySettings(level);

            if (shouldChangeResolution)
            {
                float scale = Display.main.systemWidth / (float)pixelWidth;
                Screen.SetResolution(pixelWidth, (int)(Display.main.systemHeight / scale), Screen.fullScreen);
            }
        }

        /// <summary>
        /// 根据自身需要调整各级别需要修改的设置，可根据需求修改低中高三种方案某一项具体设置
        /// </summary>
        /// <param name="qualityLevel">质量等级</param>
        public static void SetQualitySettings(QualityLevel qualityLevel)
        {
            if (qualityLevel < QualityLevel.Low || qualityLevel > QualityLevel.High) return;

            CurrentQualityLevel = qualityLevel;
            QualitySettings.SetQualityLevel((int)qualityLevel);
        }
    }

    public enum QualityLevel
    {
        Low = 0,
        Mid = 1,
        High = 2,
    }
}