//------------------------------------------------------------
// File : NetworkErrorDialog.cs
// Email: yang.li@kingboat.io
// Desc : 
//------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameFramework;
using GameFramework.DataTable;
using TMPro;

namespace CasualGame
{
    public class NetworkErrorDialog : ScaleDialog
    {
        private GameFrameworkAction retryCB;
        private GameFrameworkAction failCB;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            if (m_Params.HasKey("retryCB"))
                retryCB = (GameFrameworkAction)m_Params.GetObjectParams("retryCB");

            if (m_Params.HasKey("failCB"))
                failCB = (GameFrameworkAction)m_Params.GetObjectParams("failCB");
        }

        private void OnClick_RetryBtn()
        {
            retryCB?.Invoke();
            Close();
        }

        private void OnClick_CloseBtn()
        {
            failCB?.Invoke();
            Close();
        }
    }
}