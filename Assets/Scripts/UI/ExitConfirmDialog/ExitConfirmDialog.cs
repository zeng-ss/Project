//------------------------------------------------------------
// File : ExitConfirmDialog.cs
// Desc : 
//------------------------------------------------------------

using CasualGame.CoffeeAway;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace CasualGame
{
    public class ExitConfirmDialog : ScaleDialog
    {
        [SerializeField] private Button m_CloseBtn;
        [SerializeField] private Button m_ConfirmBtn;
        [SerializeField] private TextMeshProUGUI m_BtnTxt;
        [SerializeField] private FailUIPeekListener m_PeekListener;
        private const float PeekFadeDuration = 0.2f;
        private bool m_IsPeeking;
        private bool _isBackHome;
        private bool _shouldOpenSettingDialog = false;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            BindPeekMask();
        }
        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            _isBackHome = MParams.GetBoolParams("is_back_home");
            if (_isBackHome)
            {
                m_BtnTxt.text = LocalizationMgr.Instance.GetString("101");
                _shouldOpenSettingDialog = true;
            }
            else
            {
                m_BtnTxt.text = LocalizationMgr.Instance.GetString("102");
                _shouldOpenSettingDialog = false;
            }
            m_ConfirmBtn.onClick.RemoveAllListeners();
            m_CloseBtn.onClick.RemoveAllListeners();
            m_ConfirmBtn.onClick.AddListener(OnClick_ConfirmBtn);
            m_CloseBtn.onClick.AddListener(() =>
            {
                PlaySoundManager.Instance.PlaySound(Constant.Audio.ClickButton);
                Close();
            });
            RefreshUI();
        }

        private void RefreshUI()
        {
            //_atlasLoader.LoadSprite(m_Icon, );
            //m_DesTxt.text = LocalizationMgr.Instance.GetString("");
        }

        private void OnClick_ConfirmBtn()
        {
            PlaySoundManager.Instance.PlaySound(Constant.Audio.ClickButton);
            if (_isBackHome)
            {
                SceneHelper.LoadHomeScene(0);
                _shouldOpenSettingDialog = false;
            }
            else
            {
                if (EnergyManager.CanUseEnergy(1))
                {
                    CoffeeAwayGame.Instance.Restart();
                    EnergyManager.UseEnergy(1);
                    Close();
                    return;
                }
                //体力不足
                UGUIParams @params = UGUIParams.Create().AddValue("fromHome", false);
                Framework.UIMgr.Instance.OpenUIForm(UIFormType.GetLifeDialog, @params);
            }
            Close();
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
            if (_shouldOpenSettingDialog)
            {
                UGUIParams @params = UGUIParams.Create().AddValue("FromHome", false);
                Framework.UIMgr.Instance.OpenUIForm(UIFormType.SettingDialog, @params);
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