//------------------------------------------------------------
// File : DebugHandler.cs
// Email: mailto:zhiqiang.yang@kingboat.io
// Desc : 作弊指令处理
/*
 * AddCoin=100
 * AddDiamond=100
 * AddPower=100
 * AddGameCoin=100
 * JumpLevel=3
 * SetVip=0  //0关闭vip , 1开启vip
 */
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using IMGUIDebugger;
using UnityEngine;

namespace CasualGame
{
    public partial class DebugHandler
    {
        public readonly List<InfoData> InfoDatas = new List<InfoData>()
        {
            new InfoData()
            {
                Name = "TimeStamp",
                Info = () =>
                    $"{TimeUtils.GetDateTime(TimeUtils.NowTimeStamp):u}",
            },
            new InfoData()
            {
                Name = "SystemInfo",
                Info = () =>
                    $"GraphicsMemorySize:{SystemInfo.graphicsMemorySize:f1}, \nSystemMemorySize:{SystemInfo.systemMemorySize:f1}, \nProcessorCount:{SystemInfo.processorCount}, \nProcessorFrequency:{SystemInfo.processorFrequency}",
            },
            new InfoData()
            {
                Name = "PackInfo",
                Info = () =>
                    $"PackageName:{Application.identifier},\nAppVersion:{Application.version},\nProductName:{Application.productName},\nCompanyName:{Application.companyName}",
            },
            new InfoData()
            {
                Name = "AdInfo",
                Info = () =>
                    $"AdCount:{KBbase.Utility.GetTotalAdCount()}, \nIntersAdCount:{KBbase.Utility.GetIntersAdCount()}, \nRewardAdCount:{KBbase.Utility.GetRewardAdCount()}",
            },
        };
    }
}