//------------------------------------------------------------
// File : FailUIForm.cs
// Email: mailto:TemplateAuthor@kingboat.io
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

            if (m_CanvasGroup != null)
            {
                m_CanvasGroup.DOKill();
                m_CanvasGroup.interactable = false;
                m_CanvasGroup.blocksRaycasts = false;
                m_CanvasGroup.DOFade(0f, PeekFadeDuration);
            }

            UGuiScreenMaskHelper.SetPeekSuppressed(true);
        }

        private void EndPeek()
        {
            if (!m_IsPeeking)
                return;

            m_IsPeeking = false;
            UGuiScreenMaskHelper.SetPeekSuppressed(false);

            if (m_CanvasGroup != null)
            {
                m_CanvasGroup.DOKill();
                m_CanvasGroup.interactable = true;
                m_CanvasGroup.blocksRaycasts = true;
                m_CanvasGroup.DOFade(1f, PeekFadeDuration);
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
            GameEntry.UI.OpenUIForm(UIFormType.GetLifeDialog, @params);
            Close();
        }
    }
}
