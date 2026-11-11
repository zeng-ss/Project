//------------------------------------------------------------
// File : FailUIForm.cs
// Desc : 通关失败面板
//------------------------------------------------------------

using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace CasualGame.CoffeeAway
{
    public class FailUIForm : ScaleDialog
    {
        [SerializeField] private Button m_RestartBtn;
        [SerializeField] private FailUIPeekListener m_PeekListener;
        private const float PeekFadeDuration = 0.2f;

        private bool m_IsPeeking;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            BindPeekMask();
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            // m_BackBtn.onClick.RemoveAllListeners();
            // m_BackBtn.onClick.AddListener(BackToRevice);
            m_RestartBtn.onClick.RemoveAllListeners();
            m_RestartBtn.onClick.AddListener(RestartGame);


            //m_BackBtn.gameObject.SetActive(false);
            // if (ProxyManager.UserProxy.GameLevel < 3)
            // {
            //     m_BackBtn.gameObject.SetActive(false);
            // }
            // else
            // {
            //     m_BackBtn.gameObject.SetActive(true);
            // }
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            EndPeek();
            base.OnClose(isShutdown, userData);
        }

        private void BindPeekMask()
        {
            m_PeekListener.onPeekBegin = BeginPeek;
            m_PeekListener.onPeekEnd = EndPeek;
        }

        private void BeginPeek()
        {
            if (m_IsPeeking || !UIAnimationFinish)
                return;

            m_IsPeeking = true;

            if (MCanvasGroup != null)
            {
                MCanvasGroup.DOKill();
                MCanvasGroup.interactable = false;
                MCanvasGroup.blocksRaycasts = false;
                MCanvasGroup.DOFade(0f, PeekFadeDuration);
            }

            UGuiScreenMaskHelper.SetPeekSuppressed(true);
        }

        private void EndPeek()
        {
            if (!m_IsPeeking)
                return;

            m_IsPeeking = false;
            UGuiScreenMaskHelper.SetPeekSuppressed(false);

            if (MCanvasGroup != null)
            {
                MCanvasGroup.DOKill();
                MCanvasGroup.interactable = true;
                MCanvasGroup.blocksRaycasts = true;
                MCanvasGroup.DOFade(1f, PeekFadeDuration);
            }
        }

        private void RestartGame()
        {
            //这才是真正的输掉比赛
            if (EnergyManager.CanUseEnergy(1))
            {
                CoffeeAwayGame.Instance.Restart();
                EnergyManager.UseEnergy(1);
                Close();
                return;
            }
            UGUIParams @params = UGUIParams.Create().AddValue("fromHome", false);
            Framework.UIMgr.Instance.OpenUIForm(UIFormType.GetLifeDialog, @params);
            Close();
        }
    }
}
