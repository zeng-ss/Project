using Custom.Activity;
using UnityEngine;

namespace CasualGame
{
    public abstract class PanelBase : ActivatablePoolPrefabBase
    {
        public abstract ActivityType ActivityType { get; }

        public abstract RectTransform PanelRectTransform { get; }

        public abstract void InitPanel(AtlasLoader loader = null);

        public abstract void RefreshPanel();

        public abstract void ReleasePanel();

        public bool IsSelectd { get; set; }

        [HideInInspector] public UIMainForm UIMainForm;

        protected IActivity Activity => ActivityManager.Instance.TryGetActivityByType(ActivityType);

        protected bool IsPanelUnlocked()
        {
            if (Activity == null)
                return true;
            return Activity.IsActivated;
        }
    }
}