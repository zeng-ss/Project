//------------------------------------------------------------
// File : PopupForm.cs
// Email: mailto:zhiqiang.yang
// Desc : 
//------------------------------------------------------------
using GameFramework;

namespace CasualGame
{
    public class PopupForm : IHomeTask
    {
        private UIFormType m_FormType;
        private UGUIParams m_GUIParams;
        private GameFrameworkFunc<bool> m_FinishCb; //返回true表示Task完成

        public PopupForm(UIFormType formType, UGUIParams _guiParams)
        {
            this.m_FormType = formType;
            this.m_GUIParams = _guiParams;
        }

        public PopupForm(UIFormType formType, UGUIParams _guiParams, GameFrameworkFunc<bool> finishCb)
        {
            this.m_FormType = formType;
            this.m_GUIParams = _guiParams;
            this.m_FinishCb = finishCb;
        }

        public override void Execute(UIMainForm uIHome)
        {
            GameEntry.UI.OpenUIForm(m_FormType, m_GUIParams);
        }

        public override void Update()
        {
            if (m_FinishCb == null)
            {
                var form = GameEntry.UI.GetUIForm<UIMainForm>(m_FormType);
                if (form == null) return;

                if (form.UIAnimationFinish)
                    Done = true;
            }
            else
            {

                if (m_FinishCb.Invoke())
                    Done = true;
            }
        }
    }
}
