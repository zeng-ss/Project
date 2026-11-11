using System;
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CasualGame
{
    [System.Serializable]
    public class ActivityModel : BaseModel
    {
        public SpinAdModel spinAdModel;
    }

    public class SpinAdModel
    {
        public bool Enabled;
        public int spinCoin;
        public int free;
        public int ad;
    }

}