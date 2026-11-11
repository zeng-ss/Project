using System;
using CasualGame;
using Tutorial;

namespace Custom.Activity
{
    [Serializable]
    public class PanelActivityData : ActivityData
    {
        public PanelActivityData(int activityId) : base(activityId)
        {
        }
    }

    public abstract class PanelActivity<T> : AbstractActivity<T> where T : PanelActivityData
    {
        protected override T CreateActivityData()
        {
            if (typeof(T) == typeof(PanelActivityData))
                return (T)(object)new PanelActivityData((int)ActivityType);
            throw new NotSupportedException($"{GetType().Name} must override CreateActivityData() when using a custom PanelActivityData subtype.");
        }

        public override void Initial()
        {

        }

        public override void Activate()
        {
            Data.isEnabled = true;
            Save();
        }

        public override void Close()
        {
            Data.isEnabled = false;
            Save();
        }
    }
}
