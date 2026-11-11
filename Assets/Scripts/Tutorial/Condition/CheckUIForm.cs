using CasualGame;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace Tutorial.Condition
{
    [Category("✫ Tutorial")]
    public class CheckUIForm : ConditionTask
    {
        public BBParameter<UIFormType> formType = UIFormType.Undefined;
        public BBParameter<bool> MustVisible = false;
        protected override string info
        {
            get
            {
                if (!invert)
                    return string.Format("界面{0}是否打开", formType.value.ToString());
                else
                    return string.Format("界面{0}是否关闭", formType.value.ToString());
            }
        }
        protected override bool OnCheck()
        {
            var uiForm = Framework.UIMgr.Instance.GetUIForm<UGuiForm>(formType.value);
            if (MustVisible.value)
            {
                return (uiForm != null) && uiForm.Visible;
            }
            else
            {
                return uiForm != null;
            }
        }
    }
}
