
namespace CasualGame
{
    public abstract class IHomeTask
    {
        public bool Done { get; protected set; }
        protected bool m_Executed = false;
        public void DoExecute(UIMainForm uIHome)
        {
            if (!m_Executed)
            {
                Execute(uIHome);
                m_Executed = true;
            }
        }
        public abstract void Execute(UIMainForm uIHome);
        public abstract void Update();
    }
}