//------------------------------------------------------------
// File : UIMainForm.cs
// Email: mailto:TemplateAuthor@kingboat.io
// Desc : 
//------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CasualGame
{
    public class UIMainForm : UGuiForm
    {
        [SerializeField] private Button m_GameBtn;
        [SerializeField] private TextMeshProUGUI m_GameBtnText;
        [SerializeField] private CurrencyCell m_Coin, m_Energy;
        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            //m_GameBtnText.text = GameEntry.Localization.GetString("1") + " " + (ProxyManager.UserProxy.GameLevel + 1).ToString();
            m_Coin.SetData((int)ItemType.Coin, new AtlasLoader());
            m_Energy.SetData((int)ItemType.Energy, new AtlasLoader());
            m_Coin.Spawn();
            m_Energy.Spawn();
            m_GameBtn.onClick.RemoveAllListeners();
            m_GameBtn.onClick.AddListener(OnClick_GameBtn);
            m_GameBtnText.text = GameEntry.Localization.GetString("1") + " " + (ProxyManager.UserProxy.GameLevel + 1).ToString();
        }
        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
            m_Coin.DeSpawn();
        }

        private void OnClick_GameBtn()
        {
            if (!EnergyManager.CanUseEnergy(1))
            {
                UGUIParams @params = UGUIParams.Create().AddValue("fromHome", true);
                GameEntry.UI.OpenUIForm(UIFormType.GetLifeDialog, @params);
                return;
            }
            SceneHelper.LoadGameScene();
        }

        private void OnClick_SettingBtn()
        {
            UGUIParams _params = UGUIParams.Create();
            _params.AddValue("FromHome", true);
            GameEntry.UI.OpenUIForm(UIFormType.SettingDialog, _params);
        }
    }
}
