//------------------------------------------------------------
// File : UseItemForm.cs
// Email: mailto:TemplateAuthor@kingboat.io
// Desc : 获取道具面板
//------------------------------------------------------------

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Coffee.UIExtensions;
using PuzzleTrack;
using DG.Tweening;
using Cysharp.Threading.Tasks;

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
            curItemType = m_Params.GetEnumParams<ItemType>("ItemType");
            int titleText = m_Params.GetIntParams("TitleText");
            _titleText.text = GameEntry.Localization.GetString(titleText.ToString());
            _atlasLoader.LoadSprite(_icon, Constant.AtlasId.Item, new ItemData((int)curItemType).GetIconName());
            //_desText.text = GameEntry.Localization.GetString(PropBtn.GetPropDes(curItemType));
            switch (curItemType)
            {
                case ItemType.Undo:
                    itemCount = GameEntry.Config.GetInt(Constant.Config.PropCost11, 100);
                    _itemCount.text = itemCount.ToString();
                    break;
                case ItemType.Shuffle:
                    itemCount = GameEntry.Config.GetInt(Constant.Config.PropCost12, 100);
                    _itemCount.text = itemCount.ToString();
                    break;
                case ItemType.ExpandRoad:
                    itemCount = GameEntry.Config.GetInt(Constant.Config.PropCost13, 100);
                    _itemCount.text = itemCount.ToString();
                    break;
                case ItemType.EliminateColor:
                    itemCount = GameEntry.Config.GetInt(Constant.Config.PropCost14, 100);
                    _itemCount.text = itemCount.ToString();
                    break;
            }

            _closeBtn.onClick.RemoveAllListeners();
            _buyBtn.onClick.RemoveAllListeners();
            _adBtn.onClick.RemoveAllListeners();
            _closeBtn.onClick.AddListener(Close);
            _buyBtn.onClick.AddListener(OnBuyBtnClick);
            _adBtn.onClick.AddListener(OnAdBtnClick);

            isBuySuccess = false;

            if (m_Params.HasKey("Fail"))
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

                PuzzleTracking.Level.UseHelper(curItemType.ToString(), "coin");
                PuzzleTracking.Currency.SoftCurrencyChange(-itemCount, "coin", "buy_prop");
                // if (ShipJamGame.Instance.IsDeadlockFailed)
                // {
                //     PuzzleTracking.Level.Revive("fail_continue", "coin");
                // }

                /*var target = m_Params.GetAnyParams<Vector3>("Target");
                var icon = m_Params.GetAnyParams<Transform>("Icon");
                EffectManager.PlayFlyEffectToTarget(new ItemData((int)curItemType, 1), _icon.transform.position, target,
                    () =>
                    {
                        ProxyManager.UserProxy.AddItem((int)curItemType, 1);
                        Mutils.Animation.FlyScale(icon);
                        ProxyManager.UserProxy.NotifyCurrency((int)curItemType);
                    }, size: 140f);*/
                Close();
            }
            else
            {
                isBuySuccess = false;
                GameEntry.UI.OpenUIForm(UIFormType.GlobalToast,
                    UGUIParams.Create().AddValue("Toast", GameEntry.Localization.GetString("58")));
            }
        }

        private void OnAdBtnClick()
        {
            // if (ShipJamGame.Instance.IsDeadlockFailed)
            // {
            //     PuzzleTracking.Level.Revive("fail_continue", "ad");
            // }
            PuzzleTracking.Level.UseHelper(curItemType.ToString(), "ad");
            PuzzleTracking.Level.MarkAds();
            HorstRillSdk.Ads.showRewardedAD($"{Constant.Ad.tt_rv_add_booster}_{(int)curItemType}", null, null, null,
                (unitId, reward, adInfo) =>
                {
                    isBuySuccess = true;
                    Close();
                }, null);
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