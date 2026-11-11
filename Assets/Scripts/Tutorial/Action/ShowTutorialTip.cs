using CasualGame;
using GameFramework;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace Tutorial.Action
{
    public class CleanActionData : IActionData, IReference
    {
        public ActionType ActionType => ActionType.Clean;
        public void Clear()
        {
        }
        public static CleanActionData Create()
        {
            var data = ReferencePool.Acquire<CleanActionData>();
            return data;
        }
    }

    // public class CloseUIFormActionData : IActionData, IReference
    // {
    //     public ActionType ActionType => ActionType.CloseUIForm;
    //     public void Clear()
    //     {
    //     }
    // }

    public class ShowTutorialTipActionData : IActionData, IReference
    {
        public ActionType ActionType => ActionType.ShowTutorialTip;
        public string Tips { get; set; }
        public string LocalizationKey { get; set; }
        public Vector2 TipPosition { get; set; }
        public bool HasContinue { get; set; }
        public GameFrameworkAction OnClickTargetBtn { get; set; }

        public static ShowTutorialTipActionData Create(ShowTutorialTip tutorialTip, GameFrameworkAction onClickTargetBtn)
        {
            var data = ReferencePool.Acquire<ShowTutorialTipActionData>();
            data.Tips = tutorialTip.tips.value;
            data.LocalizationKey = tutorialTip.localizationKey.value;
            data.TipPosition = tutorialTip.tipPosition.value;
            data.HasContinue = tutorialTip.hasContinue.value;
            data.OnClickTargetBtn = onClickTargetBtn;
            return data;
        }
        public void Clear()
        {
            Tips = string.Empty;
            LocalizationKey = string.Empty;
            TipPosition = Vector2.zero;
            HasContinue = false;
        }
    }



    [Category("✫ Tutorial")]
    public class ShowTutorialTip : ActionTask
    {
        public BBParameter<string> tips = string.Empty; //提示文案
        public BBParameter<string> localizationKey = string.Empty; //是否只是高亮UI
        public BBParameter<Vector2> tipPosition = Vector2.zero; //提示位置
        public BBParameter<bool> hasContinue = false;//是否出现Continue

        // public BBParameter<float> tipsWidth = 414f;//是否出现Continue
        // public BBParameter<float> tipsHeightY = 0;

        private bool isClickCallBack = false;//是否点击了锁定元素

        protected override string info
        {
            get
            {
                string newTips = tips.value;
                if (newTips.Length >= 13)
                {
                    newTips = newTips.Substring(0, 13) + "...";
                }
                return string.Format("Tip: {0} ({1},{2}) k={3}", newTips, tipPosition.value.x, tipPosition.value.y, localizationKey.value);
            }
        }

        protected override void OnExecute()
        {
            base.OnExecute();
            isClickCallBack = false;
            var actionData = ShowTutorialTipActionData.Create(this, this.OnClickTargetBtn);
            TutorialManager.Instance.tutorialForm.ExecuteAction(actionData);
            if (!hasContinue.value)
            {
                EndAction(true); //非中断直接结束
            }
        }

        private void OnClickTargetBtn()
        {
            isClickCallBack = true;
            //清理按钮
            var tutorialHandler = TutorialManager.Instance.tutorialForm;
            if (tutorialHandler != null)
            {
                tutorialHandler.ExecuteAction(CleanActionData.Create());
                //通知客户端继续
                // EventManager.Instance.NotifyEvent(Constant.Event.TutorialContinue);
            }
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            if (hasContinue.value && isClickCallBack)
            {
                EndAction(true);
                return;
            }
        }


    }
}
