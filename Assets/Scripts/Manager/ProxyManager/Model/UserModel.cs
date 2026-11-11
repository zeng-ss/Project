//------------------------------------------------------------
// File : .cs
// Email: yang.li@kingboat.io
// Desc : 
//------------------------------------------------------------

using System;
using UnityEngine;
using System.Collections.Generic;
using UnityGameFramework.Runtime;
using Newtonsoft.Json;
using UnityEngine.Serialization;

namespace CasualGame
{
    [System.Serializable]
    public class UserModel : BaseModel
    {
        public int level;

        // base
        public long lastLoginTime;
        public string countryCode;
        public Dictionary<string, int> Items = new Dictionary<string, int>();

        // 内购相关
        public List<string> transactionIDs = new List<string>();
        public float totalPayAmountUsd;
        public float totalPayAmount;
        public int totalPayNum;
        public string payCurrency = "";

        // vip
        public bool isVip;

        // energy
        public int energyRecoverTimeStamp = 0;
        public int energyUnlimitedTimeStamp = 0;
        public int energyUnlimitedCount = 0;

        // media
        public int media = 0;

        //Activity
        public Custom.Activity.ActivityModel ActivityModel;
        public int activateTime;

        public int userAvatar = 1;
        public int userAvatarFrame = 1;
        public string userName = "";
        public bool hasScore = false;

        //道具解锁标记
        public int boosterUnlockMask;
        public int bit;
        public int PlayCnt = 0;

        // 排行榜相关
        public List<RankSerializableData> rankSerializableDatas = new List<RankSerializableData>();
    }

    [Serializable]
    public class RankSerializableData
    {
        public int currentWeekAdd;
        public int currentRankSettleTimeStamp;
    }
}