//------------------------------------------------------------
// File : ContinueUIForm.cs
// Desc : 
//------------------------------------------------------------

using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace CasualGame.CoffeeAway
{
    public class ContinueUIForm : ScaleDialog
    {
        [SerializeField] private Button m_LookAdBtn;
        [SerializeField] private Button m_UseCoinBtn;
        [SerializeField] private Button m_CloseBtn;
        [SerializeField] private FailUIPeekListener m_PeekListener;
        [SerializeField] private TextMeshProUGUI m_CoinCountText;
        private const float PeekFadeDuration = 0.2f;
        private bool m_IsPeeking;
        private int coinCount;
        private bool isBuySuccess;
        private UGUIParamsDelegate onContinue;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            BindPeekMask();
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            m_LookAdBtn.onClick.RemoveAllListeners();
            m_UseCoinBtn.onClick.RemoveAllListeners();
            m_CloseBtn.onClick.RemoveAllListeners();
            m_UseCoinBtn.onClick.AddListener(OnClick_UseCoin);
            m_CloseBtn.onClick.AddListener(OnClick_Close);
            coinCount = GameConfigMgr.Instance.GetInt(Constant.Config.ContinueCost, 400);
            m_CoinCountText.text = coinCount.ToString();
            onContinue = MParams.GetDelegage("OnContinue");
        }

        private void OnClick_Close()
        {
            Framework.UIMgr.Instance.OpenUIForm(UIFormType.FailUIForm);
            Close();
        }

        private void OnClick_UseCoin()
        {
            PlaySoundManager.Instance.PlaySound(Constant.Audio.ClickButton);
            if (ProxyManager.UserProxy.ItemCount((int)ItemType.Coin) >= coinCount)
            {
                ProxyManager.UserProxy.CostItem((int)ItemType.Coin, coinCount);
                ProxyManager.UserProxy.NotifyCurrency((int)ItemType.Coin);
                isBuySuccess = true;
                Close();
            }
            else
            {
                isBuySuccess = false;
                Framework.UIMgr.Instance.OpenUIForm(UIFormType.GlobalToast,
                    UGUIParams.Create().AddValue("Toast", LocalizationMgr.Instance.GetString("58")));
            }
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            EndPeek();
            base.OnClose(isShutdown, userData);
            // 关闭之前出现额外容器
            if (isBuySuccess)
            {
                var core = CoffeeAwayGame.Instance.Core;
                core.ResetAllVehiclesMoveSpeed();
                core.ShowExtraHolder();
                CoffeeAwayGame.Instance.RefitCamera();
                onContinue?.Invoke();
            }
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
    }
}