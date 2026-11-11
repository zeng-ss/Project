using System;
using GameFramework;
using GameFramework.Event;
using GameFramework.Resource;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace CasualGame
{
    [OPS.Obfuscator.Attribute.DoNotObfuscateClass]
    public class ProcedureColdBoot<T> : ProcedureBase where T : UGuiForm
    {
        protected bool needCloseLaunch = false;
        protected bool openUISuccess = false;
        protected virtual UIFormType FormType => UIFormType.Undefined;
        protected Action<ProcedureOwner> _switchProcedureSuccessAction;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            openUISuccess = false;
            needCloseLaunch = false;
            if (procedureOwner.HasData(Constant.ProcedureData.ColdBoot))
            {
                needCloseLaunch = procedureOwner.GetData<VarBoolean>(Constant.ProcedureData.ColdBoot).Value;
                procedureOwner.RemoveData(Constant.ProcedureData.ColdBoot);
            }

            if (procedureOwner.HasData(Constant.ProcedureData.SwitchProcedureSuccessAction))
            {
                _switchProcedureSuccessAction = procedureOwner.GetData<VarAction<ProcedureOwner>>(Constant.ProcedureData.SwitchProcedureSuccessAction).Value;
                procedureOwner.RemoveData(Constant.ProcedureData.SwitchProcedureSuccessAction);
            }

            GameEntry.UI.OpenUIForm(FormType, UGUIParams.Create(() => { }, (UGuiForm ugui) =>
            {
                openUISuccess = true;
            }));
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            GameEntry.UI.CloseUIForm(FormType);
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            if (openUISuccess)
            {
                if (needCloseLaunch)
                {
                    if (!OnCustomLoad()) return;
                    needCloseLaunch = false;
                    LaunchPage.CloseUI();
                    OnCustomLoadSuccess();
                }
                _switchProcedureSuccessAction?.Invoke(ownerState);
                _switchProcedureSuccessAction = null;
                openUISuccess = false;
                PlaySoundManager.Instance.PlayBgm();
            }
        }

        protected virtual bool OnCustomLoad()
        {
            return true;
        }

        protected virtual void OnCustomLoadSuccess()
        {

        }
    }
}