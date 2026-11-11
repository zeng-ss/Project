//------------------------------------------------------------
// File : UseItemForm.cs
// Desc : 获取道具面板
//------------------------------------------------------------

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace CasualGame.CoffeeAway
{
    /// <summary>
    /// 获取道具面板
    /// </summary>
    public class GetItemForm : ScaleDialog
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _titleText, _itemCount;
        [SerializeField] private Button _buyBtn, _adBtn, _closeBtn;
        [SerializeField] private FailUIPeekListener m_PeekListener;

        private ItemType curItemType;
        private int itemCount;
        private bool isBuySuccess;
        private bool m_IsPeeking;
        private const float PeekFadeDuration = 0.2f;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            curItemType = MParams.GetEnumParams<ItemType>("ItemType");
            int titleText = MParams.GetIntParams("TitleText");
            _titleText.text = LocalizationMgr.Instance.GetString(titleText.ToString());
            AtlasLoader.LoadSprite(_icon, Constant.AtlasId.Item, new ItemData((int)curItemType).GetIconName());
            //_desText.text = LocalizationMgr.Instance.GetString(PropBtn.GetPropDes(curItemType));
            switch (curItemType)
            {
                case ItemType.Undo:
                    itemCount = GameConfigMgr.Instance.GetInt(Constant.Config.PropCost11, 100);
                    _itemCount.text = itemCount.ToString();
                    break;
                case ItemType.Shuffle:
                    itemCount = GameConfigMgr.Instance.GetInt(Constant.Config.PropCost12, 100);
                    _itemCount.text = itemCount.ToString();
                    break;
                case ItemType.ExpandRoad:
                    itemCount = GameConfigMgr.Instance.GetInt(Constant.Config.PropCost13, 100);
                    _itemCount.text = itemCount.ToString();
                    break;
                case ItemType.EliminateColor:
                    itemCount = GameConfigMgr.Instance.GetInt(Constant.Config.PropCost14, 100);
                    _itemCount.text = itemCount.ToString();
                    break;
            }

            _closeBtn.onClick.RemoveAllListeners();
            _buyBtn.onClick.RemoveAllListeners();
            _adBtn.onClick.RemoveAllListeners();
            _closeBtn.onClick.AddListener(Close);
            _buyBtn.onClick.AddListener(OnBuyBtnClick);

            isBuySuccess = false;

            if (MParams.HasKey("Fail"))
            {
                EnablePeekMask();
            }
            else
            {
                DisablePeekMask();
            }
        }
        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            BindPeekMask();
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
        private void EnablePeekMask()
        {
            m_PeekListener.gameObject.SetActive(true);
        }
        private void DisablePeekMask()
        {
            m_PeekListener.gameObject.SetActive(false);
        }
        // 获取道具
        private void OnBuyBtnClick()
        {
            PlaySoundManager.Instance.PlaySound(Constant.Audio.ClickButton);
            if (ProxyManager.UserProxy.ItemCount((int)ItemType.Coin) >= itemCount)
            {
                isBuySuccess = true;

                ProxyManager.UserProxy.CostItem((int)ItemType.Coin, itemCount);
                ProxyManager.UserProxy.NotifyCurrency((int)ItemType.Coin);
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
            base.OnClose(isShutdown, userData);
            // 关闭之前使用道具
            if (isBuySuccess)
            {
                ProxyManager.UserProxy.AddItem((int)curItemType, 1);
                PropMgr propMgr = CoffeeAwayGame.Instance.PropMgr;
                if (propMgr.UsePropByType(curItemType))
                {
                    ProxyManager.UserProxy.CostItem((int)curItemType, 1);
                }
                ProxyManager.UserProxy.NotifyCurrency((int)curItemType);
            }
        }
    }
}