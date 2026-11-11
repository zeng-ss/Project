//------------------------------------------------------------
// File : NetworkErrorDialog.cs
// Desc : 
//------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace CasualGame
{
    public class NetworkErrorDialog : ScaleDialog
    {
        private Action retryCB;
        private Action failCB;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            if (MParams.HasKey("retryCB"))
                retryCB = (Action)MParams.GetObjectParams("retryCB");

            if (MParams.HasKey("failCB"))
                failCB = (Action)MParams.GetObjectParams("failCB");
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