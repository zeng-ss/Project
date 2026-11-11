using System;
using System.Collections.Generic;
using Custom.Activity;
using IMGUIDebugger;
using UnityEngine;

namespace CasualGame
{
    public partial class DebugHandler : Singleton<DebugHandler>
    {
        public enum DebugType
        {
            None = 0,
            Game = 1,
            SDK = 2,
            Data = 3,
            Time = 4,
            Test = 5,
        }

        public readonly List<CommandData> CommandDatas = new List<CommandData>()
        {
            new CommandData()
            {
                Name = "Open UI",
                Action = (param) =>
                {
                    if (!int.TryParse(param, out int result)) return;

                    Framework.UIMgr.Instance.OpenUIForm((UIFormType)result);

                    CloseDebuggerUI();
                },
            },
            new CommandData()
            {
                Name = "Close UI",
                Action = (param) =>
                {
                    if (!int.TryParse(param, out int result)) return;

                    Framework.UIMgr.Instance.CloseUIForm((UIFormType)result);

                    CloseDebuggerUI();
                },
            },
            new CommandData()
            {
                Name = "GameSpeed",
                Action = (param) =>
                {
                    if (!int.TryParse(param, out int result)) return;

                    Framework.GameRoot.Instance.GameSpeed = result;
                },
                Info = () => $"speed: {Framework.GameRoot.Instance.GameSpeed}",
            },
            new CommandData()
            {
                Name = "Disable\nDebug",
                Action = (param) =>
                {
                    EnableDebug = false;
                    SetGMButton(false);
                },
            },
            new CommandData()
            {
                Name = "QualityLevel",
                Action = (param) =>
                {
                    if (!int.TryParse(param, out int result)) return;

                    DevicePerformanceUtil.SetQualitySettings((QualityLevel)result);
                },
                Info = () => $"level:{DevicePerformanceUtil.CurrentQualityLevel}",
            },
            new CommandData()
            {
                Name = "FrameRate",
                Action = (param) =>
                {
                    if (!int.TryParse(param, out int result)) return;

                    Application.targetFrameRate = result;
                },
                Info = () => $"FPS:{Application.targetFrameRate}",
            },
            new CommandData()
            {
                Name = "OpenLog",
                Action = (param) =>
                {
                    LogHandler.m_enableDebug = !LogHandler.m_enableDebug;
                },
                Info = () => $"Log:{LogHandler.EnableDebug}",
            },
            new CommandData()
            {
                Name = "AddEnergy",
                Action = (param) =>
                {
                    if (!int.TryParse(param, out int result)) return;

                    EnergyManager.AddEnergy(result);
                    ProxyManager.UserProxy.NotifyCurrency((int)ItemType.Energy);
                },
            },
            new CommandData()
            {
                Name = "AddCoin",
                Action = (param) =>
                {
                    if (!int.TryParse(param, out int result)) return;

                    ProxyManager.UserProxy.AddItem((int)ItemType.Coin, result);
                    ProxyManager.UserProxy.NotifyCurrency((int)ItemType.Coin);
                }
            },
            new CommandData()
            {
                Name = "AddItem",
                Action = (param) =>
                {
                    if (!int.TryParse(param, out int result)) return;

                    for(int i = (int)ItemType.Undo; i <= (int)ItemType.EliminateColor; i++)
                    {
                        ProxyManager.UserProxy.AddItem(i, result);
                        ProxyManager.UserProxy.NotifyCurrency(i);
                    }
                }
            },
            new CommandData()
            {
                Name = "JumpLevel",
                Action = (param) =>
                {
                    if (!int.TryParse(param, out int result)) return;

                    ProxyManager.UserProxy.SetGameLevel(result );
                    CloseDebuggerUI();
                    EventManager.NotifyEvent(Constant.Event.OnGameRestart);
                }
            },
            new CommandData()
            {
                Name = "NormalUser",
                Action = (param) =>
                {
                    ProxyManager.UserProxy.SetGameLevel(0);
                    SettingMgr.Instance.SetBool("IsOrganic", false);
                    SettingMgr.Instance.SetBool("IsConfirmIsOrganic", true);
                    UIManager.Toast("RESTART GAME!");
                    YieldHepler.CallAfterOnSeconds(() => Application.Quit(), 1f);
                }
            },
            new CommandData()
            {
                Name = "GreenScreen",
                Action = (param) =>
                {
                    EventManager.NotifyEvent(Constant.Event.OnGreenScreen);
                }
            },
            new CommandData()
            {
                Name = "ResumeAudio",
                Action = (param) =>
                {
                    PlaySoundManager.Instance.ResumeAudio();
                }
            },
            new CommandData()
            {
                Name = "StopAudio",
                Action = (param) =>
                {
                    PlaySoundManager.Instance.StopAudio();
                }
            },
        };
    }
}