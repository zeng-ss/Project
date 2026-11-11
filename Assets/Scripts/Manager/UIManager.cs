//------------------------------------------------------------
// File : UIManager.cs
// Email: mailto:zhiqiang.yang
// Desc : 
//------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;
using GameFramework;
using System;

namespace CasualGame
{
    public static class UIManager
    {
        public static void Toast(string toast, Action callback = null)
        {
            GameEntry.UI.OpenUIForm(UIFormType.GlobalToast, UGUIParams.Create().AddValue("Toast", toast)
                    .AddValue("ToastCall", callback));
        }

        public static void ShowLoading(bool isPurchase = false, float duration = float.MaxValue, GameFrameworkAction closeCB = null)
        {
            UGUIParams uguiParams = UGUIParams.Create(closeCB);
            uguiParams.AddValue("Duration", duration);
            uguiParams.AddValue("isPurchase", isPurchase);
            GameEntry.UI.OpenUIForm(UIFormType.LoadingDialog, uguiParams);
        }

        public static void ShowNetworkErrorDialog(GameFrameworkAction retryCB = null, GameFrameworkAction failCB = null)
        {
            UGUIParams uguiParams = UGUIParams.Create();
            uguiParams.AddValue("retryCB", retryCB);
            uguiParams.AddValue("failCB", failCB);
            GameEntry.UI.OpenUIForm(UIFormType.NetWorkErrorDialog, uguiParams);
        }

        public static void CloseLoading()
        {
            GameEntry.UI.CloseUIForm(UIFormType.LoadingDialog);
        }
    }
}