using NodeCanvas.Framework;
using ParadoxNotion.Design;
using GameFramework;

namespace Tutorial.Condition
{
    [Category("✫ Tutorial")]
    public class CheckBitwise : ConditionTask
    {
        public BBParameter<eGameBitwise> puzzleStatus = eGameBitwise.None;

        protected override string info
        {
            get { return string.Format("弱引导是否 {0} 已执行", puzzleStatus.value); }
        }

        protected override bool OnCheck()
        {
            return Utility.Bitwise.IsBit((int)TutorialManager.Instance.BitwiseStatus, (int)puzzleStatus.value);
        }
    }
}
