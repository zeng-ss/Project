//------------------------------------------------------------
// File : ExitConfirmDialog.cs
// Email: mailto:TemplateAuthor@kingboat.io
// Desc : 
//------------------------------------------------------------

using CasualGame.CoffeeAway;
using PuzzleTrack;
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
            _isBackHome = m_Params.GetBoolParams("is_back_home");
            if (_isBackHome)
            {
                m_BtnTxt.text = GameEntry.Localization.GetString("101");
                _shouldOpenSettingDialog = true;
            }
            else
            {
                m_BtnTxt.text = GameEntry.Localization.GetString("102");
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
            //m_DesTxt.text = GameEntry.Localization.GetString("");
        }

        private void OnClick_ConfirmBtn()
        {
            PlaySoundManager.Instance.PlaySound(Constant.Audio.ClickButton);
            PuzzleTracking.Level.LoseLevel();
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
                GameEntry.UI.OpenUIForm(UIFormType.GetLifeDialog, @params);
            }
            Close();
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
            if (_shouldOpenSettingDialog)
            {
                UGUIParams @params = UGUIParams.Create().AddValue("FromHome", false);
                GameEntry.UI.OpenUIForm(UIFormType.SettingDialog, @params);
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
    }
}