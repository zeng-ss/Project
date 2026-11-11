using CasualGame;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace Tutorial.Action
{
    [Category("✫ Tutorial")]
    public class NotifyEvent : ActionTask
    {
        public BBParameter<int> eventId = 0;
        protected override string info
        {
            get { return string.Format("通知⌚️ {0}", eventId.value); }
        }
        protected override void OnExecute()
        {
            base.OnExecute();
            EventManager.NotifyEvent(eventId.value);
            EndAction();
        }

    }
}
