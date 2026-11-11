//------------------------------------------------------------
// File : SettingDialog.cs
// Email: mailto:TemplateAuthor@kingboat.io
// Desc : 游戏中暂停 / 设置面板
//------------------------------------------------------------

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CasualGame.CoffeeAway
{
    public class SettingDialog : ScaleDialog
    {
        [SerializeField] private AttachParentPrefabPool attachParentPrefabPool;
        [SerializeField] private RectTransform m_SettingBtnParent, m_BottomParent;
        private List<SettingBtn> btns = new List<SettingBtn>();
        public Button closeBtn;
        public Button debugBtn;
        public TextMeshProUGUI version;
        public Button homeBtn;
        //public Button restartBtn;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            bool fromHome = m_Params.GetBoolParams("FromHome");
            if (fromHome)
            {
                m_BottomParent.gameObject.SetActive(false);
            }
            else
            {
                m_BottomParent.gameObject.SetActive(true);
            }

            closeBtn.onClick.RemoveAllListeners();
            closeBtn.onClick.AddListener(OnClick_CloseBtn);

            debugBtn.onClick.RemoveAllListeners();
            debugBtn.onClick.AddListener(OnClick_DebugBtn);

            version.text = GameEntry.Localization.GetString("9") + ":" + Application.version;

            homeBtn.onClick.RemoveAllListeners();
            homeBtn.onClick.AddListener(OnClick_HomeBtn);

            bool shouldShowHomeBtn = ProxyManager.UserProxy.GameLevel >= GameEntry.Config.GetInt("EntraceHomeLevel", 12);
            homeBtn.gameObject.SetActive(shouldShowHomeBtn);

            // restartBtn.onClick.RemoveAllListeners();
            // restartBtn.onClick.AddListener(OnClick_RestartBtn);

            int count = System.Enum.GetValues(typeof(Media)).Length;
            for (int i = 0; i < count; i++)
            {
                SettingBtn btn = attachParentPrefabPool.Spawn<SettingBtn>(0, m_SettingBtnParent);
                btn.SetData((Media)i, _atlasLoader);
                btns.Add(btn);
            }

            btns.Horizontal<SettingBtn>(TextAnchor.MiddleCenter, 24f);
            LayoutRebuilder.ForceRebuildLayoutImmediate(m_BottomParent);

            m_BottomParent.gameObject.SetActive(!SceneHelper.IsHomeScene());

            if (ProxyManager.UserProxy.GameLevel >= 3)
            {
                //m_SkipLevelParent.gameObject.SetActive(true);
            }
            else
            {
                //m_SkipLevelParent.gameObject.SetActive(false);
            }
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
            for (int i = 0; i < btns.Count; i++)
            {
                attachParentPrefabPool.DeSpawn(btns[i]);
            }

            btns.Clear();
        }

        private void OnClick_CloseBtn()
        {
            PlaySoundManager.Instance.PlaySound(Constant.Audio.ClickButton);
            Close();
        }

        private void OnClick_DebugBtn() => DebugHandler.TriggerDebugger();


        private void OnClick_HomeBtn()
        {
            PlaySoundManager.Instance.PlaySound(Constant.Audio.ClickButton);
            // 挽留 UI
            if (CoffeeAwayGame.Instance.IsBackShouldReduceEnergy)
            {
                UGUIParams uiParams = UGUIParams.Create();
                uiParams.AddValue("is_back_home", true);
                GameEntry.UI.OpenUIForm(UIFormType.ExitConfirmDialog, uiParams);
                Close();
                return;
            }
            else
            {
                EnergyManager.AddEnergy(1);
            }

            //PuzzleTracking.Level.LoseLevel();
            SceneHelper.LoadHomeScene(0);
            Close();
        }

        // private void OnClick_RestartBtn()
        // {
        //     PlaySoundManager.Instance.PlaySound(Constant.Audio.ClickButton);
        //     // 先记失败再 Restart：Restart 会触发 InitLevel 里的 StartLevel，
        //     // 若顺序颠倒，StartLevel 先结束了旧会话，后面的 LoseLevel 会记成新关卡失败
        //     PuzzleTracking.Level.LoseLevel();
        //     CoffeeAwayGame.Instance.Restart();
        //     Close();
        // }

        // private void OnClick_SkipLevelBtn()
        // {
        //     PlaySoundManager.Instance.PlaySound(Constant.Audio.ClickButton);
        //     HorstRillSdk.Ads.showRewardedAD(Constant.Ad.tt_rv_skip_level, null, null, null, (unitId, reward, adInfo) =>
        //     {
        //         ProxyManager.UserProxy.AddGameLevel();
        //         CoffeeAwayGame.Instance.Restart();
        //         Close();
        //     }, null);
        // }

        public void OnBtnPolicy()
        {
            Application.OpenURL(GameEntry.Config.GetString(Constant.Config.URL_Privacy_Policy));
        }

        public void OnBtnTerm()
        {
            Application.OpenURL(GameEntry.Config.GetString(Constant.Config.URL_User_Agreement));
        }
    }
}