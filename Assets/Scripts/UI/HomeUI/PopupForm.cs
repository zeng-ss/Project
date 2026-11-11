using System;
//------------------------------------------------------------
// File : PopupForm.cs
// Email: mailto:zhiqiang.yang
// Desc : 
//------------------------------------------------------------

namespace CasualGame
{
    public class PopupForm : IHomeTask
    {
        private UIFormType m_FormType;
        private UGUIParams m_GUIParams;
        private Func<bool> m_FinishCb; //返回true表示Task完成

        public PopupForm(UIFormType formType, UGUIParams _guiParams)
        {
            this.m_FormType = formType;
            this.m_GUIParams = _guiParams;
        }

        public PopupForm(UIFormType formType, UGUIParams _guiParams, Func<bool> finishCb)
        {
            this.m_FormType = formType;
            this.m_GUIParams = _guiParams;
            this.m_FinishCb = finishCb;
        }

        public override void Execute(UIMainForm uIHome)
        {
            Framework.UIMgr.Instance.OpenUIForm(m_FormType, m_GUIParams);
        }

        public override void Update()
        {
            if (m_FinishCb == null)
            {
                var form = Framework.UIMgr.Instance.GetUIForm<UIMainForm>(m_FormType);
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
