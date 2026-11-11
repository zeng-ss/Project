using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace CasualGame
{
    public class GlobalToast : UGuiForm
    {
        #region SerializeField
        private TMPro.TextMeshProUGUI m_Toast => GetRef<TMPro.TextMeshProUGUI>("Toast");
        #endregion

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            transform.localScale = Vector3.one * 0.5f;
            transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                if (m_Params.HasKey("ToastCall"))
                {
                    Action callback = (Action)m_Params.GetObjectParams("ToastCall");
                    callback?.Invoke();
                }
            });
            m_Toast.text = m_Params.GetStringParams("Toast");
            StartCoroutine(DelayAction(2, () =>
            {
                Close();
            }));
        }

        protected IEnumerator DelayAction(float delay, System.Action action)
        {
            yield return YieldHepler.WaitForSeconds(delay);

            action();
        }

    }
}