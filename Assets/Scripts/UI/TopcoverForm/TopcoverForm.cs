//------------------------------------------------------------
// File : TopcoverForm.cs
// Email: mailto:zhiqiang.yang@kingboat.io
// Desc : 
//------------------------------------------------------------

namespace CasualGame
{
    public class TopcoverForm : UGuiForm
    {
        public static TopcoverForm Instance;
        public static void InitForm()
        {
            if (Instance == null)
            {
                GameEntry.UI.OpenUIForm(UIFormType.TopcoverForm);
            }
        }
        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            Instance = this;
        }
    }
}
