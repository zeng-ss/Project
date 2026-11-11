using System;
using UnityEngine;

namespace CasualGame
{
    public class EnergyManager
    {
        public static void UpdateEnergyTimeStamp()
        {
            if (ProxyManager.UserProxy.userModel.energyRecoverTimeStamp > 0)
            {
                var remainTime = GameManager.Instance.GetTimeStampOfSeconds() - ProxyManager.UserProxy.userModel.energyRecoverTimeStamp;
                if (remainTime > Constant.GameConfig.SingleEnergyRecoverTime)
                {
                    int count = remainTime / Constant.GameConfig.SingleEnergyRecoverTime;
                    var curCount = ProxyManager.UserProxy.ItemCount((int)ItemType.Energy);
                    ProxyManager.UserProxy.AddItem((int)ItemType.Energy, Mathf.Min(Constant.GameConfig.MAXEnergy - curCount, count));
                    ProxyManager.UserProxy.UpdateEnergyTimeStamp(GameManager.Instance.GetTimeStampOfSeconds());
                    ProxyManager.UserProxy.NotifyCurrency((int)ItemType.Energy);
                }
            }

            // 保底
            bool full = ProxyManager.UserProxy.ItemCount((int)ItemType.Energy) >= Constant.GameConfig.MAXEnergy;
            if (ProxyManager.UserProxy.userModel.energyRecoverTimeStamp <= 0 && !full)
            {
                ProxyManager.UserProxy.UpdateEnergyTimeStamp(GameManager.Instance.GetTimeStampOfSeconds());
            }

            if (ProxyManager.UserProxy.userModel.energyRecoverTimeStamp > 0 && full)
            {
                ProxyManager.UserProxy.UpdateEnergyTimeStamp(0);
            }

            // 无限体力
            if (ProxyManager.UserProxy.userModel.energyUnlimitedTimeStamp > 0 && !IsUnlimitedTime())
            {
                DateTime _d = TimeUtils.GetDateTime(ProxyManager.UserProxy.userModel.energyUnlimitedTimeStamp);
                if (!TimeUtils.IsSameDay(_d))
                {
                    ProxyManager.UserProxy.UpdateUnlimitedTimeStamp(0);
                }
            }
        }

        public static int GetNextEnergyRecoverTime(out bool showFull)
        {
            showFull = ProxyManager.UserProxy.ItemCount((int)ItemType.Energy) >= Constant.GameConfig.MAXEnergy;
            var result = 0;
            if (!showFull)
            {
                var remainTime = GameManager.Instance.GetTimeStampOfSeconds() - ProxyManager.UserProxy.userModel.energyRecoverTimeStamp;
                result = Constant.GameConfig.SingleEnergyRecoverTime - remainTime;
                if (result < 0)
                    UpdateEnergyTimeStamp();
            }

            return result;
        }

        public static bool CanUseEnergy(int count = 1)
        {

            if (IsUnlimitedTime())
            {
                return true;
            }

            if (ProxyManager.UserProxy.ItemCount((int)ItemType.Energy) >= count)
            {
                return true;
            }

            // GameEntry.UI.OpenUIForm(UIFormType.LackOfPower);
            return false;
        }
        public static void UseEnergy(int count)
        {
            if (IsUnlimitedTime())
            {
                return;
            }

            int curLevel = ProxyManager.UserProxy.GameLevel;
            if (curLevel <= GameEntry.Config.GetInt("EntraceHomeLevel", 12))
            {
                return;
            }

            ProxyManager.UserProxy.CostItem((int)ItemType.Energy, count);
            ProxyManager.UserProxy.NotifyCurrency((int)ItemType.Energy);
        }

        public static void AddEnergy(int count)
        {
            bool showFull = ProxyManager.UserProxy.ItemCount((int)ItemType.Energy) >= Constant.GameConfig.MAXEnergy;
            if (showFull)
            {
                return;
            }

            ProxyManager.UserProxy.AddItem((int)ItemType.Energy, count);
            ProxyManager.UserProxy.NotifyCurrency((int)ItemType.Energy);
        }

        // 无限体力
        public static bool IsUnlimitedTime()
        {
            int unlimitedTime = ProxyManager.UserProxy.userModel.energyUnlimitedTimeStamp + Constant.GameConfig.UnlimitedEnergyTime;
            int currentTime = GameManager.Instance.GetTimeStampOfSeconds();
            return unlimitedTime > currentTime;
        }

        public static bool CanGetUnlimitedBuff()
        {
            return ProxyManager.UserProxy.userModel.energyUnlimitedTimeStamp <= 0;
        }

        public static int GetRemainUnlimitedTime()
        {
            return ProxyManager.UserProxy.userModel.energyUnlimitedTimeStamp + Constant.GameConfig.UnlimitedEnergyTime - GameManager.Instance.GetTimeStampOfSeconds();
        }

        public static void AddUnlimitedCount(int count)
        {
            ProxyManager.UserProxy.AddUnlimitedCount(count);
            if (ProxyManager.UserProxy.userModel.energyUnlimitedCount >= Constant.GameConfig.UnlimitedEnergy)
            {
                ProxyManager.UserProxy.UpdateUnlimitedTimeStamp(GameManager.Instance.GetTimeStampOfSeconds());
            }
        }

        public static void AddFullEnergy()
        {
            ProxyManager.UserProxy.AddItem((int)ItemType.Energy, Constant.GameConfig.MAXEnergy - ProxyManager.UserProxy.ItemCount((int)ItemType.Energy));
            ProxyManager.UserProxy.NotifyCurrency((int)ItemType.Energy);
        }
    }
}