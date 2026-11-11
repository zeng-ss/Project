//------------------------------------------------------------
// File : UIMainForm.cs
// Desc : 
//------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

namespace CasualGame
{
    public class UIMainForm : UGuiForm
    {
        [FormerlySerializedAs("m_GameBtn")] [SerializeField] private Button mGameBtn;
        [FormerlySerializedAs("m_GameBtnText")] [SerializeField] private TextMeshProUGUI mGameBtnText;
        [FormerlySerializedAs("m_Coin")] [SerializeField] private CurrencyCell mCoin;
        [FormerlySerializedAs("m_Energy")] [SerializeField] private CurrencyCell mEnergy;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            mCoin.SetData((int)ItemType.Coin, new AtlasLoader());
            mEnergy.SetData((int)ItemType.Energy, new AtlasLoader());
            mCoin.Spawn();
            mEnergy.Spawn();
            mGameBtn.onClick.RemoveAllListeners();
            mGameBtn.onClick.AddListener(OnClick_GameBtn);
            mGameBtnText.text = LocalizationMgr.Instance.GetString("1") + " " + (ProxyManager.UserProxy.GameLevel + 1);
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
            mCoin.DeSpawn();
        }

        private void OnClick_GameBtn()
        {
            if (!EnergyManager.CanUseEnergy())
            {
                UGUIParams @params = UGUIParams.Create().AddValue("fromHome", true);
                Framework.UIMgr.Instance.OpenUIForm(UIFormType.GetLifeDialog, @params);
                return;
            }

            SceneHelper.LoadGameScene();
        }

        private void OnClick_SettingBtn()
        {
            UGUIParams @params = UGUIParams.Create();
            @params.AddValue("FromHome", true);
            Framework.UIMgr.Instance.OpenUIForm(UIFormType.SettingDialog, @params);
        }
    }
}