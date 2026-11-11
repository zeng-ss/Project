//------------------------------------------------------------
// File : UserProxy.cs
// Email: mailto:zhiqiang.yang@kingboat.io
// Desc : 玩家数据存储
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.DataTable;
using Tutorial;
using UnityEngine;
using Custom.Activity;


namespace CasualGame
{
    public partial class UserProxy : BaseProxy<UserModel>
    {
        public UserModel userModel
        {
            get { return (UserModel)this.Data; }
        }

        public int GameLevel => userModel.level;

        public IDataTable<DRItem> ItemTables;

        public bool IsNewUser => IsProxyNull;

        public static int UserAvatar
        {
            get => ProxyManager.UserProxy.userModel.userAvatar;
            set
            {
                ProxyManager.UserProxy.userModel.userAvatar = value;
                ProxyManager.UserProxy.Save();
            }
        }

        public static int UserAvatarFrame
        {
            get => ProxyManager.UserProxy.userModel.userAvatarFrame;
            set
            {
                ProxyManager.UserProxy.userModel.userAvatarFrame = value;
                ProxyManager.UserProxy.Save();
            }
        }


        public static string UserName
        {
            get => ProxyManager.UserProxy.userModel.userName;
            set
            {
                ProxyManager.UserProxy.userModel.userName = value;
                ProxyManager.UserProxy.Save();
            }
        }

        public static string GetDeviceId()
        {
#if UNITY_EDITOR
            return SystemInfo.deviceUniqueIdentifier;
#endif
            return HorstRillSdk.GetDeviceId();
        }

        public static string GetDistinctId()
        {
            return HorstRillSdk.GetDistinctId();
        }

        public static string GetChannel()
        {
            string channel = "";
#if UNITY_ANDROID && !UNITY_EDITOR
            channel = "GooglePlay";
#elif UNITY_IOS && !UNITY_EDITOR
            channel = "AppleStore";
#else
            channel = "Editor";
#endif
            return channel;
        }

        public UserProxy(string proxyName, object data = null) : base(proxyName, data)
        {
            if (IsProxyNull)
            {
                AddItem((int)ItemType.Energy, Constant.GameConfig.MAXEnergy);
            }

        }

        public override void OnRegister()
        {
            ItemTables = GameEntry.DataTable.GetDataTable<DRItem>();
        }


        public override void OnDataTableLoaded()
        {
        }

        public void Login(long loginTime)
        {
            userModel.lastLoginTime = loginTime;
            Save();
        }

        public void SetGameLevel(int level)
        {
            userModel.level = level;
            SaveImmediately();
        }

        public void AddGameLevel()
        {
            userModel.level += 1;
            SaveImmediately();
        }

        #region item

        public void AddItems(List<ItemData> items, string source = "")
        {
            foreach (var item in items)
            {
                AddItem(item.id, item.num, source);
            }
        }

        public bool AddItem(int itemId, int count = 1, string source = "")
        {
            string itemKey = itemId.ToString();
            if (userModel.Items.ContainsKey(itemKey))
                userModel.Items[itemKey] += count;
            else
                userModel.Items[itemKey] = count;
            Save();
            return true;
        }

        public void NotifyCurrency(int itemId)
        {
            LogHandler.Info($"==> lyly UserProxy NotifyCurrency - itemId : {itemId}");
            EventManager.NotifyEvent(Constant.Event.RefreshItem, new RefreshItemEventArgs() { itemId = itemId });
            EventManager.NotifyEvent(Constant.Event.UpdateBtnRedPoint);
        }

        public int ItemCount(int itemId)
        {
            if (userModel.Items.ContainsKey(itemId.ToString()))
            {
                return userModel.Items[itemId.ToString()];
            }

            return 0;
        }

        public bool CostItem(int itemId, int count)
        {
            if (userModel.Items.ContainsKey(itemId.ToString()))
            {
                int total = userModel.Items[itemId.ToString()];
                if (total >= count)
                {
                    userModel.Items[itemId.ToString()] -= count;
                    Save();
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region 内购

        public void AddPayment(float prize, float prizeUsd, string currency)
        {
            userModel.totalPayAmountUsd += prizeUsd;
            userModel.totalPayAmount += prize;
            userModel.totalPayNum += 1;
            userModel.payCurrency = currency;
            Save();
        }

        public bool HasTransactionID(string transactionID)
        {
            if (string.IsNullOrEmpty(transactionID))
                return false;

            return userModel.transactionIDs != null && userModel.transactionIDs.Contains(transactionID);
        }

        public void AddTransactionID(string transactionID, string productId)
        {
            if (string.IsNullOrEmpty(transactionID))
            {
                LogHandler.Info($"IAP - Add null transactionID : {productId}");
                return;
            }

            if (userModel.transactionIDs == null)
                userModel.transactionIDs = new List<string>();

            if (!userModel.transactionIDs.Contains(transactionID))
                userModel.transactionIDs.Add(transactionID);

            Save();
        }

        public bool IsIapPlayer()
        {
            if (userModel.transactionIDs == null)
            {
                return false;
            }

            return userModel.transactionIDs.Count > 0;
        }

        #endregion

        #region Vip

        public static bool IsVip
        {
            get
            {
                if (ProxyManager.UserProxy == null || ProxyManager.UserProxy.userModel == null)
                {
                    return false;
                }

                return ProxyManager.UserProxy.userModel.isVip;
            }
        }

        public VipType curVipType = VipType.None;
        public TimeSpan curVipRemainTime = TimeSpan.Zero;

        public void ClaimVIP(TimeSpan time)
        {
            userModel.isVip = true;
            RecordVipType(time);
            RecordVipRemainTime(time);
            Save();
        }

        public void ClearVIP()
        {
            userModel.isVip = false;
            curVipType = VipType.None;
            curVipRemainTime = TimeSpan.Zero;
            Save();
        }

        public void RecordVipType(TimeSpan time)
        {
            int duration = time.Days;
            if (duration > 0 && duration <= 7)
            {
                curVipType = VipType.Week;
            }
            else if (duration >= 7 && duration <= 31)
            {
                curVipType = VipType.Month;
            }
            else if (duration > 31)
            {
                curVipType = VipType.Year;
            }

            LogHandler.Info($"==> lyly UserProxy recordvip - duration : {duration} curVipType: {curVipType}");
        }

        public void RecordVipRemainTime(TimeSpan time)
        {
            if (curVipRemainTime != null && curVipRemainTime.Days > time.Days) return;

            curVipRemainTime = time;
            LogHandler.Info(
                $"==> lyly UserProxy - recordvip curVipRemainTime: {curVipRemainTime.Days}, {curVipRemainTime.Hours}, {curVipRemainTime.Minutes}");
        }

        #endregion

        #region 体力

        public void UpdateEnergyTimeStamp(int timeStamp)
        {
            userModel.energyRecoverTimeStamp = timeStamp;
            Save();
        }

        public void UpdateUnlimitedTimeStamp(int timeStamp)
        {
            if (timeStamp <= 0)
            {
                userModel.energyUnlimitedCount = 0;
            }
            userModel.energyUnlimitedTimeStamp = timeStamp;
            Save();
        }

        public void AddUnlimitedCount(int count)
        {
            userModel.energyUnlimitedCount += count;
            Save();
        }


        public int GameBitwiseStatus
        {
            get
            {
                return userModel.bit;
            }
            set
            {
                userModel.bit = value;
                Save();
            }
        }

        public bool IsGameBitwise(eGameBitwise bit)
        {
            return Utility.Bitwise.IsBit(userModel.bit, 1 << (int)bit);
        }
        public void SetGameBitwise(eGameBitwise bit, bool value)
        {
            Utility.Bitwise.SetBit(ref userModel.bit, 1 << (int)bit, value);
            Save();
        }

        #endregion

        #region 设置页

        public void SetMediaMuted(int media, bool close)
        {
            Utility.Bitwise.SetBit(ref userModel.media, 1 << media, close);
            Save();
        }

        public bool IsMediaMuted(int media)
        {
            return Utility.Bitwise.IsBit(userModel.media, 1 << media);
        }

        #endregion


        public bool IsBoosterUnlocked(int index)
        {
            return Utility.Bitwise.IsBit(userModel.boosterUnlockMask, 1 << index);
        }
        public void UnlockBooster(int index)
        {
            Utility.Bitwise.SetBit(ref userModel.boosterUnlockMask, 1 << index, true);
            Save();
        }

        public void AddPlayCnt()
        {
            userModel.PlayCnt++;
            Save();
        }

        public void ClearPlayCnt()
        {
            userModel.PlayCnt = 0;
            Save();
        }
    }
}