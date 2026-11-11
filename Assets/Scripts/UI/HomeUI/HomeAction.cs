//------------------------------------------------------------
// File : HomeAction.cs
// Email: mailto:zhiqiang.yang
// Desc : 
//------------------------------------------------------------
using GameFramework;

namespace CasualGame
{
    public class HomeAction : IHomeTask
    {
        private GameFramework.GameFrameworkAction gameFrameworkAction;

        private GameFrameworkFunc<bool> m_FinishCb; //返回true表示Task完成

        public HomeAction(GameFramework.GameFrameworkAction action)
        {
            this.gameFrameworkAction = action;
        }

        //等待finishCb返回true表示完成
        public HomeAction(GameFramework.GameFrameworkAction action, GameFrameworkFunc<bool> finishCb)
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
