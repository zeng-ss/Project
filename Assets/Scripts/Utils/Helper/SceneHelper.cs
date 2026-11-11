//------------------------------------------------------------
// File : SceneHelper.cs
// Email: mailto:yang.li@kingboat.io
// Desc : 
//------------------------------------------------------------

using System;
using Cysharp.Threading.Tasks;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;
using CasualGame.CoffeeAway;
using UnityEngine;

namespace CasualGame
{
    public class SceneHelper
    {
        public static void LoadHomeScene(int hard, Action onCompleteAction = null)
        {
            try
            {
                if (IsHomeScene())
                {
                    onCompleteAction?.Invoke();
                    return;
                }

                var state = GameEntry.Procedure.CurrentProcedure as ProcedureBase;
                TransitionForm.Open(0, async () =>
                {
                    state.SwitchScene(Constant.Scene.Home, (owner) =>
                    {
                        VarAction<ProcedureOwner> varAction = new VarAction<ProcedureOwner>();
                        varAction.Value = (owner) => onCompleteAction?.Invoke();
                        state.SwitchProcedure<ProcedureMainGame>(varAction);
                    });
                    await UniTask.WaitUntil(() => GameEntry.UI.GetUIForm<UIMainForm>(UIFormType.UIMainForm) != null);
                });
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public static void LoadGameScene(Action onCompleteAction = null)
        {
            if (IsGamePlayScene())
            {
                onCompleteAction?.Invoke();
                return;
            }

            var state = GameEntry.Procedure.CurrentProcedure as ProcedureBase;
            int hard = 0;
            //EnergyManager.UseEnergy(1);

            TransitionForm.Open(hard, async () =>
            {
                state.SwitchScene(Constant.Scene.MyGame, (owner) =>
                {
                    VarAction<ProcedureOwner> varAction = new VarAction<ProcedureOwner>();
                    varAction.Value = (owner) => onCompleteAction?.Invoke();
                    state.SwitchProcedure<ProcedureCoffeeGame>(varAction);
                });
                await UniTask.WaitUntil(() => CoffeeAwayGame.Instance != null);
            });
        }

        public static bool IsHomeScene()
        {
            return GameEntry.Procedure.CurrentProcedure is ProcedureMainGame;
        }

        public static bool IsGamePlayScene()
        {
            return GameEntry.Procedure.CurrentProcedure is ProcedureCoffeeGame;
        }
    }
}
