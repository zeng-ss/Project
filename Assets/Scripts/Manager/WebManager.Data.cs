using UnityEngine;
using GameFramework;
using System;
using System.Collections.Generic;


namespace CasualGame
{
    [Serializable]
    public class ServerTimeData
    {
        public long timestamp;
    }

    [Serializable]
    public class ActiveModel<T>
    {
        public string token;
        public T user;
    }

    [Serializable]
    public class UserData
    {
        public string id;
        public string deviceId;
        public string taId;
        public string deviceModel;
        public string deviceName;
        public string deviceType;
        public string deviceSystem;
        public int deviceMemoryInMB;
        public string pkgName;
        public string currentVersion;
        public string channel;
        public string lang;
        public string country;
        public string nickname;
        public string email;
        public string avatar;
        public string avatarFrame;
        public bool whitelist;
    }

    [Serializable]
    public class PatchData
    {
        public PatchInfo[] patches;
    }

    [Serializable]
    public class PatchInfo
    {
        public string field;
        public string value;
    }
}