using System;
//------------------------------------------------------------
// File : HomeAction.cs
// Email: mailto:zhiqiang.yang
// Desc : 
//------------------------------------------------------------

namespace CasualGame
{
    public class HomeAction : IHomeTask
    {
        private Action gameFrameworkAction;

        private Func<bool> m_FinishCb; //返回true表示Task完成

        public HomeAction(Action action)
        {
            this.gameFrameworkAction = action;
        }

        //等待finishCb返回true表示完成
        public HomeAction(Action action, Func<bool> finishCb)
        {
            this.gameFrameworkAction = action;
            this.m_FinishCb = finishCb;
        }

        public override void Execute(UIMainForm uIHome)
        {
            this.gameFrameworkAction?.Invoke();
        }

        public override void Update()
        {
            if (m_FinishCb == null)
            {
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
