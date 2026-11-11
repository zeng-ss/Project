using CasualGame;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace Tutorial.Action
{
    public class MaskActionData : IActionData, IReference
    {
        public ActionType ActionType => ActionType.ShowMask;
        public int Alpha { get; set; }
        public bool IsShow { get; set; }
        public bool ClearHole { get; set; }
        public System.Action OnClickMask { get; set; }

        public void Clear()
        {
            Alpha = 255;
            IsShow = true;
            ClearHole = true;
            OnClickMask = null;
        }
        public static MaskActionData Create(int alpha, bool isShow, bool clearHole = true, System.Action onClickMask = null)
        {
            var data = ReferencePool.Acquire<MaskActionData>();
            data.Alpha = alpha;
            data.IsShow = isShow;
            data.ClearHole = clearHole;
            data.OnClickMask = onClickMask;
            return data;
        }
    }

    [Category("✫ Tutorial")]
    public class ShowMask : ActionTask
    {
        public static int DefaultAlpha = 245;

        public BBParameter<int> alpha = DefaultAlpha;
        public BBParameter<bool> isShow = true;
        //清理之前扣洞
        public BBParameter<bool> clearHole = true;

        protected override string info
        {
            get { return string.Format("{0}  alpha : {1}", isShow.value ? "显示" : "隐藏", alpha); }
        }

        protected override void OnExecute()
        {
            base.OnExecute();

            if (TutorialManager.Instance.tutorialForm != null)
            {
                var actionData = MaskActionData.Create(alpha.value, isShow.value, clearHole.value);
                // if (usingFirstMask.value)
                //     TutorialManager.Instance.tutorialForm.SetMasking(isShow.value, alpha.value, clearMask.value);
                // else
                // {
                //     TutorialManager.Instance.tutorialForm.SetMaskByCode(isShow.value);
                // }
                TutorialManager.Instance.tutorialForm.ExecuteAction(actionData);
            }
            EndAction();
        }
    }
}
