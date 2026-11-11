//------------------------------------------------------------
// File : UIManager.cs
// Desc : 界面快捷门面（Toast / Loading / 网络错误弹窗）
//------------------------------------------------------------

using System;

namespace CasualGame
{
    public static class UIManager
    {
        public static void Toast(string toast, Action callback = null)
        {
            Framework.UIMgr.Instance.OpenUIForm(UIFormType.GlobalToast,
                UGUIParams.Create().AddValue("Toast", toast).AddValue("ToastCall", callback));
        }

        public static void ShowLoading(bool isPurchase = false, float duration = float.MaxValue, Action closeCB = null)
        {
            UGUIParams uguiParams = UGUIParams.Create(closeCB);
            uguiParams.AddValue("Duration", duration);
            uguiParams.AddValue("isPurchase", isPurchase);
            Framework.UIMgr.Instance.OpenUIForm(UIFormType.LoadingDialog, uguiParams);
        }

        public static void ShowNetworkErrorDialog(Action retryCB = null, Action failCB = null)
        {
            UGUIParams uguiParams = UGUIParams.Create();
            uguiParams.AddValue("retryCB", retryCB);
            uguiParams.AddValue("failCB", failCB);
            Framework.UIMgr.Instance.OpenUIForm(UIFormType.NetWorkErrorDialog, uguiParams);
        }

        public static void CloseLoading()
        {
            Framework.UIMgr.Instance.CloseUIForm(UIFormType.LoadingDialog);
        }
    }
}
