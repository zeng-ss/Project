using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

namespace CasualGame
{
    public class NewItemDialog : ScaleDialog
    {
        [FormerlySerializedAs("_desText")] [SerializeField] private TextMeshProUGUI desText;
        [FormerlySerializedAs("_icon")] [SerializeField] private Image icon;
        [FormerlySerializedAs("_CloseButton")] [SerializeField] private Button closeButton;
        private bool _isClosingHandled;
        private ItemData _itemData;
        private ItemType _unlockType;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            _isClosingHandled = false;
            _unlockType = MParams.GetAnyParams<ItemType>("unlockType");
            _itemData = new ItemData((int)_unlockType);

            switch (_unlockType)
            {
                case ItemType.Hidden:
                    //_titleText.text = "Hidden";
                    desText.text = LocalizationMgr.Instance.GetString("89");
                    AtlasLoader.LoadSprite(icon, _itemData.GetSpritePath());
                    break;
                case ItemType.Glass:
                    //_titleText.text = "Glass";
                    desText.text = "Glass";
                    AtlasLoader.LoadSprite(icon, _itemData.GetSpritePath());
                    break;
                case ItemType.Container:
                    // _titleText.text = "Container";
                    desText.text = LocalizationMgr.Instance.GetString("90");
                    AtlasLoader.LoadSprite(icon, _itemData.GetSpritePath());
                    break;
                case ItemType.Colored:
                    //_titleText.text = "Colored";
                    desText.text = "Colored";
                    AtlasLoader.LoadSprite(icon, _itemData.GetSpritePath());
                    break;
            }

            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(OnClick_CloseBtn);
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
            if (_isClosingHandled)
            {
                return;
            }
            ProxyManager.UserProxy.UnlockBooster(_itemData.id);

            base.OnClose(isShutdown, userData);
        }
    }
}
