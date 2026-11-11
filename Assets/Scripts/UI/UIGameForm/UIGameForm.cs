using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

namespace CasualGame.CoffeeAway
{
    public class UIGameForm : UGuiForm
    {
        [SerializeField] private TextMeshProUGUI levelText, desText;
        [SerializeField] private Transform m_PropContainer, finger;
        [SerializeField] private Button m_SettingBtn, m_RestartBtn;
        [SerializeField] private AttachParentPrefabPool m_Pool;
        private List<PropBtn> m_PropBtns = new();

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            UpdateVisual();

            m_SettingBtn.onClick.RemoveAllListeners();
            m_SettingBtn.onClick.AddListener(OnClick_SettingBtn);
            m_RestartBtn.onClick.RemoveAllListeners();
            m_RestartBtn.onClick.AddListener(OnClick_RestartBtn);
        }

        public void UpdateVisual()
        {
            ClearPropBtns();
            levelText.text = LocalizationMgr.Instance.GetString("1").ToUpper() + " " +
                             (ProxyManager.UserProxy.GameLevel + 1);
            List<ItemType> propBtns = new List<ItemType>()
                { ItemType.Undo, ItemType.Shuffle, ItemType.ExpandRoad, ItemType.EliminateColor };
            for (int i = 0; i < propBtns.Count; i++)
            {
                var propBtn = m_Pool.Spawn<PropBtn>(0, m_PropContainer);
                propBtn.Init(this, propBtns[i], AtlasLoader);
                propBtn.transform.name = $"PropBtn_{(int)propBtns[i]}";
                m_PropBtns.Add(propBtn);
            }
        }

        #region Guide

        public void SetGuide(bool show, string des = "")
        {
            desText.gameObject.SetActive(show);
            desText.text = des;
        }

        // 设置指引道具手套位置和显隐
        public void SetFingerShow(bool show, Vector3 position)
        {
            finger.gameObject.SetActive(show);
            SetGuide(show, LocalizationMgr.Instance.GetString("95"));
            if (!show || m_PropBtns.Count == 0) return;

            finger.transform.localEulerAngles = new Vector3(0, 0, -28.3f);
            finger.transform.localScale = Vector3.one * -2.5f;
            finger.transform.position = position;
        }

        public void SetBtnShow(bool show)
        {
            foreach (var propBtn in m_PropBtns) propBtn.gameObject.SetActive(show);
        }

        #endregion

        private void OnClick_SettingBtn()
        {
            UGUIParams _params = UGUIParams.Create();
            _params.AddValue("FromHome", false);
            Framework.UIMgr.Instance.OpenUIForm(UIFormType.SettingDialog, _params);
            PlaySoundManager.Instance.PlaySound(Constant.Audio.ClickButton);
        }

        private void OnClick_RestartBtn()
        {
            PlaySoundManager.Instance.PlaySound(Constant.Audio.ClickButton);
            // 挽留 UI
            if (CoffeeAwayGame.Instance.IsBackShouldReduceEnergy && ProxyManager.UserProxy.GameLevel >= GameConfigMgr.Instance.GetInt("EntraceHomeLevel", 12))
            {
                UGUIParams uiParams = UGUIParams.Create();
                uiParams.AddValue("is_back_home", false);
                Framework.UIMgr.Instance.OpenUIForm(UIFormType.ExitConfirmDialog, uiParams);
            }
            else CoffeeAwayGame.Instance.Restart();
        }

        private void ClearPropBtns()
        {
            foreach (var propBtn in m_PropBtns)
            {
                m_Pool.DeSpawn(propBtn);
            }

            m_PropBtns.Clear();
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
            ClearPropBtns();
        }
    }
}