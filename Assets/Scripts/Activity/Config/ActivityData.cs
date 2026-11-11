using System;

namespace Custom.Activity
{
    [Serializable]
    public class ActivityData
    {
        public bool isEnabled;
        public int activityId;
        public int startTime;
        public int endTime;
        public int popBit;
        public int cycleNum;

        public ActivityData(int activityId)
        {
            this.isEnabled = false;
            this.activityId = activityId;
        }

        public bool IsBit(int wise) => Utility.Bitwise.IsBit(this.popBit, 1 << wise);
        public void SetBit(int wise, bool value) => Utility.Bitwise.SetBit(ref this.popBit, 1 << wise, value);
        public static readonly int BIT_POPUP_TODAY = 31;
    }
}