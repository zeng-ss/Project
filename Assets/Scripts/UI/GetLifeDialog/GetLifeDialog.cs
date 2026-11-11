//------------------------------------------------------------
// File : GetLifeDialog.cs
// Email: mailto:TemplateAuthor@kingboat.io
// Desc : 
//------------------------------------------------------------
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using CasualGame.CoffeeAway;
namespace CasualGame
{
    public class GetLifeDialog : ScaleDialog
    {
        [SerializeField] private Button m_CloseBtn, m_CoinBtn, m_ADBtn;
        [SerializeField] private TextMeshProUGUI m_TimeText, m_PriceText;

        private int m_Price = 80;
        private bool reward = false;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            reward = false;
            m_CloseBtn.onClick.RemoveAllListeners();
            m_CoinBtn.onClick.RemoveAllListeners();
            m_ADBtn.onClick.RemoveAllListeners();
            m_CloseBtn.onClick.AddListener(Close);
            m_CoinBtn.onClick.AddListener(OnClickCoinBtn);
            m_Price = GameEntry.Config.GetInt("OneHeartCost", 80);
            m_PriceText.text = m_Price.ToString();
        }

        private void Update()
        {
            int remain = EnergyManager.GetNextEnergyRecoverTime(out bool showFull);
            if (showFull)
            {
                m_TimeText.text = GameEntry.Localization.GetString("Full");
                return;
            }
            m_TimeText.text = TimeUtils.TimeFormat(remain, TimeFormatType.MM_SS, "{0}:{1}", true, false);
        }

        private void OnClickCoinBtn()
        {
            PlaySoundManager.Instance.PlaySound(Constant.Audio.ClickButton);
            if (reward) return;

            if (ProxyManager.UserProxy.CostItem((int)ItemType.Coin, m_Price))
            {
                reward = true;
                ProxyManager.UserProxy.NotifyCurrency((int)ItemType.Coin);
                Close();
            }
            else
            {
                GameEntry.UI.OpenUIForm(UIFormType.GlobalToast, UGUIParams.Create().AddValue("Toast", GameEntry.Localization.GetString("58")));
            }
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            bool fromHome = m_Params.GetBoolParams("fromHome");
            bool gotReward = reward;

            base.OnClose(isShutdown, userData);

            if (gotReward)
            {
                if (fromHome)
                    SceneHelper.LoadGameScene();
                else
                    CoffeeAwayGame.Instance.Restart();
            }
            else if (!fromHome)
            {
                SceneHelper.LoadHomeScene(0);
            }
        }
    }
}
