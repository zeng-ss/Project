using CasualGame;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace Tutorial.Action
{
    [Category("✫ Tutorial")]
    public class CloseUIForm : ActionTask
    {
        public BBParameter<UIFormType> formType = UIFormType.Undefined;
        protected override string info
        {
            get { return string.Format("关闭{0}界面", formType); }
        }
        protected override void OnExecute()
        {
            base.OnExecute();
            if (Framework.UIMgr.Instance.HasUIForm(formType.value))
            {
                Framework.UIMgr.Instance.CloseUIForm(formType.value);
            }
            EndAction();
        }

    }
}
