using CasualGame;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace Tutorial.Condition
{
    [Category("✫ Tutorial")]
    public class CheckProcedure : ConditionTask
    {
        public BBParameter<string> valueA;
        protected override string info
        {
            get
            {
                return "游戏状态 == " + valueA.value;
            }
        }

        protected override bool OnCheck()
        {
            // var procedure = GameEntry.Procedure.CurrentProcedure as CasualGame.ProcedureBase;
            return ProcedureBase.CurrentSceneName == valueA.value;
        }
    }
}
