//------------------------------------------------------------
// File : TopcoverForm.cs
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
                Framework.UIMgr.Instance.OpenUIForm(UIFormType.TopcoverForm);
            }
        }
        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            Instance = this;
        }
    }
}
