using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CasualGame
{
    public class NewItemDialog : ScaleDialog
    {
        [SerializeField] private TextMeshProUGUI _desText;
        [SerializeField] private Image _icon;
        [SerializeField] private Button _CloseButton;
        private bool m_IsClosingHandled;
        private ItemData m_ItemData;
        private ItemType m_UnlockType;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            m_IsClosingHandled = false;
            m_UnlockType = m_Params.GetAnyParams<ItemType>("unlockType");
            m_ItemData = new ItemData((int)m_UnlockType);

            switch (m_UnlockType)
            {
                case ItemType.Hidden:
                    //_titleText.text = "Hidden";
                    _desText.text = GameEntry.Localization.GetString("89");
                    _atlasLoader.LoadSprite(_icon, m_ItemData.GetSpritePath());
                    break;
                case ItemType.Glass:
                    //_titleText.text = "Glass";
                    _desText.text = "Glass";
                    _atlasLoader.LoadSprite(_icon, m_ItemData.GetSpritePath());
                    break;
                case ItemType.Container:
                    // _titleText.text = "Container";
                    _desText.text = GameEntry.Localization.GetString("90");
                    _atlasLoader.LoadSprite(_icon, m_ItemData.GetSpritePath());
                    break;
                case ItemType.Colored:
                    //_titleText.text = "Colored";
                    _desText.text = "Colored";
                    _atlasLoader.LoadSprite(_icon, m_ItemData.GetSpritePath());
                    break;
            }

            _CloseButton.onClick.RemoveAllListeners();
            _CloseButton.onClick.AddListener(OnClick_CloseBtn);
            // _OKBtn.onClick.RemoveAllListeners();
            // _OKBtn.onClick.AddListener(OnClick_CloseBtn);
        }

        private void OnClick_CloseBtn()
        {
            Close();
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            PlaySoundManager.Instance.PlaySound(Constant.Audio.ClickButton);
            if (m_IsClosingHandled)
            {
                return;
            }
            ProxyManager.UserProxy.UnlockBooster(m_ItemData.id);

            base.OnClose(isShutdown, userData);
        }
    }
}
