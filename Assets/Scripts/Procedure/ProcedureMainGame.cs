using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;
using UnityGameFramework.Runtime;
using GameFramework.Event;

namespace CasualGame
{
    public class ProcedureMainGame : ProcedureColdBoot<UIMainForm>
    {
        protected override UIFormType FormType => UIFormType.UIMainForm;
        private string _buildScenePath;
        private bool _isBuildSceneLoaded;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            CurrentSceneName = Constant.Scene.Home;
            _isBuildSceneLoaded = false;
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            PreviousSceneName = CurrentSceneName;
        }

        protected override bool OnCustomLoad()
        {
            UIMainForm mainForm = GameEntry.UI.GetUIForm<UIMainForm>(UIFormType.UIMainForm);
            if (mainForm == null)
            {
                return false;
            }

            return true;
        }

        protected override void OnCustomLoadSuccess()
        {
        }


    }
}