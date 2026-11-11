using CasualGame;
using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;


namespace Tutorial.Condition
{
    [Category("✫ Tutorial")]
    public class CheckPuzzleLevel : ConditionTask
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public BBParameter<int> valueA;
        public CompareMethod checkType = CompareMethod.EqualTo;

        protected override string info
        {
            get { return "关卡等级" + OperationTools.GetCompareString(checkType) + valueA; }
        }

        protected override bool OnCheck()
        {
            return OperationTools.Compare(1, valueA.value, checkType);
        }
    }
}
