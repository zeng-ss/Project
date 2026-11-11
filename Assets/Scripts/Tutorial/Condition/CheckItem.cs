using NodeCanvas.Framework;
using ParadoxNotion.Design;
using GameFramework;
using ParadoxNotion;
using CasualGame;

namespace Tutorial.Condition
{
    [Category("✫ Tutorial")]
    public class CheckItem : ConditionTask
    {
        public BBParameter<int> itemId;
        public BBParameter<int> count;
        public CompareMethod checkType = CompareMethod.EqualTo;

        protected override string info
        {
            get { return string.Format("物品id-{0}: N{1}{2}", itemId.value.ToString(), OperationTools.GetCompareString(checkType), count); }
        }

        protected override bool OnCheck()
        {
            return OperationTools.Compare(ProxyManager.UserProxy.ItemCount(itemId.value), count.value, checkType);
        }
    }
}
