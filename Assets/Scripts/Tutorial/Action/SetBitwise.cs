using CasualGame;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace Tutorial.Action
{
    [Category("✫ Tutorial")]
    public class SetWeakMask : ActionTask
    {
        public BBParameter<eGameBitwise> puzzleStatus = eGameBitwise.None;
        protected override string info
        {
            get
            {
                return "设置弱引导状态 :" + puzzleStatus.value.ToString();
            }
        }

        protected override void OnExecute()
        {
            base.OnExecute();

            int weakmask = (int)TutorialManager.Instance.BitwiseStatus;
            Utility.Bitwise.SetBit(ref weakmask, (int)puzzleStatus.value, true);
            TutorialManager.Instance.BitwiseStatus = (eGameBitwise)weakmask;
            // switch (puzzleStatus.value)
            // {
            //     case WeakGuideMask.GuideBag:
            //         GLog.lm_user_guide_event("guide_bag");
            //         break;
            //     case WeakGuideMask.GuideBubble:
            //         GLog.lm_user_guide_event("guide_bubble");
            //         break;
            //     case WeakGuideMask.GuideNotEnoughtEnergy:
            //         GLog.lm_user_guide_event("guide_energy");
            //         break;
            //     case WeakGuideMask.GuideShop:
            //         GLog.lm_user_guide_event("guide_shop");
            //         break;
            // }
            EndAction();
        }

    }
}
