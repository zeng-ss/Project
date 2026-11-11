//------------------------------------------------------------
// File : WinUIForm.cs
// Desc : 胜利通关面板
//------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using Coffee.UIExtensions;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using Spine.Unity;

namespace CasualGame.CoffeeAway
{
    public class WinUIForm : ScaleDialog
    {
        #region private variables

        [SerializeField] private Button claimBtn, lookAdBtn;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private Image newShipImage;
        [SerializeField] private RectTransform titleObj, progressObj, progressBar;
        [SerializeField] private List<RewardCell> rewardCells;
        [SerializeField] private UIParticle rewardsParticle, fireworkParticle, winParticle;
        [SerializeField] private VerticalLayoutGroup verticalLayoutGroup;
        [SerializeField] private SkeletonGraphic skeletonGraphic;

        // "动画参数"
        const float titleTargetY = 403f; // 标题的移动位置
        const float showDuration = 0.3f; // 每个 UI 显示时长
        const float startAniDelay = 1.5f; // 开始播放动画延迟
        const float progressBarMaxWidth = 361f; // 进度条最大长度
        private readonly List<Tween> tweens = new(); // 保存所有 tween 方便统一管理和销毁

        private int m_RewardCount;
        private bool m_IsRewarding;
        private int m_Level;

        #endregion

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            PlaySoundManager.Instance.PlaySound(Constant.Audio.Settlement);

            m_IsRewarding = false;
            ResetUI();

            m_RewardCount = MParams.GetAnyParams<List<ItemData>>("rewards")[0].num;

            claimBtn.onClick.RemoveAllListeners();
            lookAdBtn.onClick.RemoveAllListeners();
            claimBtn.onClick.AddListener(() => GetReward(m_RewardCount));
            lookAdBtn.onClick.AddListener(() => GetReward(m_RewardCount * 3));
            List<ItemData> rewards = MParams.GetAnyParams<List<ItemData>>("rewards");
            m_Level = ProxyManager.UserProxy.GameLevel;
            for (int i = 0; i < rewards.Count; i++)
            {
                rewardCells[i].SetData(rewards[i], new AtlasLoader());
            }

            RefreshUI();
            tweens.Add(DOVirtual.DelayedCall(startAniDelay, StartAni));
            EventManager.NotifyEvent(Constant.Event.LanguageChangeSuccess, null);
        }

        private void StartAni()
        {
            lookAdBtn.gameObject.GetComponent<global::UGUIAutoShake>().enabled = false;
            Sequence seq = DOTween.Sequence();
            tweens.Add(seq);

            seq.Append(titleObj.DOAnchorPosY(titleTargetY, showDuration).SetEase(Ease.Linear));
            seq.InsertCallback(showDuration, () =>
            {
                fireworkParticle.GetComponent<CanvasGroup>().DOFade(0, 1f);
                winParticle.GetComponent<CanvasGroup>().DOFade(0, 1f);
            });
            if (progressObj.gameObject.activeSelf)
            {
                seq.AppendInterval(showDuration);
                seq.Join(progressObj.GetComponent<CanvasGroup>().DOFade(1f, showDuration));
                seq.Join(progressObj.DOScale(1f, showDuration).SetEase(Ease.OutBack));
                seq.AppendInterval(showDuration * 2.5f);
            }
            else
            {
                seq.AppendInterval(showDuration);
            }
            seq.AppendCallback(() =>
            {
                lookAdBtn.gameObject.SetActive(m_Level > 3);
                claimBtn.gameObject.SetActive(true);
                rewardsParticle.gameObject.SetActive(true);
            });

            foreach (var rewardCell in rewardCells)
            {
                seq.Join(rewardCell.transform.DOScale(1f, showDuration).SetEase(Ease.OutBack));
            }

            seq.Join(claimBtn.transform.DOScale(1f, showDuration).SetEase(Ease.OutBack));
            seq.Join(lookAdBtn.transform.DOScale(1f, showDuration).SetEase(Ease.OutBack)).OnComplete(() =>
            {
                lookAdBtn.gameObject.GetComponent<global::UGUIAutoShake>().enabled = true;
            });
        }

        private void RefreshUI()
        {
            int level = ProxyManager.UserProxy.GameLevel;
            CoffeeMechanismUnlockRegistry.GetNextFutureTypeFromLevel(level, out var unlockType);
            if (unlockType == ItemType.None)
            {
                verticalLayoutGroup.spacing = 200;
                verticalLayoutGroup.padding = new RectOffset(0, 0, 100, 0);
                progressObj.gameObject.SetActive(false);
                return;
            }
            verticalLayoutGroup.spacing = 120;
            verticalLayoutGroup.padding = new RectOffset(0, 0, 0, 0);
            progressObj.gameObject.SetActive(true);

            // 拿到下一个解锁关卡
            int nextFutureLevel = CoffeeMechanismUnlockRegistry.GetUnlockLevelForItem(unlockType);
            // 拿到前一关解锁的关卡
            int prevUnlockLevel = CoffeeMechanismUnlockRegistry.GetPreviousUnlockLevel(level);

            int segmentLength = nextFutureLevel - prevUnlockLevel;
            int progressCount = level - prevUnlockLevel;
            progressCount++;

            // 计算比例
            float progressRatio = segmentLength > 0 ? Mathf.Clamp01((float)progressCount / segmentLength) : 1f;
            float targetWidth = progressRatio * progressBarMaxWidth;

            // 重置进度条为上一个进度
            progressBar.sizeDelta =
                new Vector2((float)(progressCount - 1) / segmentLength * progressBarMaxWidth, progressBar.sizeDelta.y);

            progressText.text = $"{progressCount}/{segmentLength}";
            tweens.Add(progressBar.DOSizeDelta(new Vector2(targetWidth, progressBar.sizeDelta.y), 1)
                .SetEase(Ease.OutQuad).SetDelay(2f));
            AtlasLoader.LoadSprite(newShipImage, Constant.AtlasId.Item, new ItemData((int)unlockType).GetIconName());
        }

        // 重置 UI 状态
        private void ResetUI()
        {
            foreach (var tween in tweens)
            {
                tween.Kill();
            }

            tweens.Clear();

            // 显示礼花和胜利背景特效
            fireworkParticle.GetComponent<CanvasGroup>().alpha = 1;
            fireworkParticle.Play();
            winParticle.GetComponent<CanvasGroup>().alpha = 1;
            winParticle.Play();

            // 重置所有 UI
            rewardsParticle.gameObject.SetActive(false);
            titleObj.anchoredPosition = Vector2.zero;
            progressObj.GetComponent<CanvasGroup>().alpha = 0;
            foreach (var rewardCell in rewardCells)
            {
                rewardCell.transform.localScale = Vector3.zero;
            }

            lookAdBtn.gameObject.SetActive(false);
            lookAdBtn.transform.localScale = Vector3.zero;
            claimBtn.gameObject.SetActive(false);
            claimBtn.transform.localScale = Vector3.zero;

            skeletonGraphic.AnimationState.ClearTracks();
            skeletonGraphic.Skeleton.SetToSetupPose();
            skeletonGraphic.AnimationState.SetAnimation(0, "Appear", false);
            skeletonGraphic.AnimationState.AddAnimation(0, "Idle", true, 0f);
        }


        private void GetReward(int rewardCount)
        {
            if (m_IsRewarding) return;
            m_IsRewarding = true;

            ProxyManager.UserProxy.AddItem((int)ItemType.Coin, rewardCount);
            EffectManager.PlayFlyEffectToTarget(
                new ItemData((int)ItemType.Coin, rewardCount),
                rewardCells[0].transform.position,
                EffectManager.GetFlyEndPosition((int)ItemType.Coin),
                async () =>
                {
                    ProxyManager.UserProxy.NotifyCurrency((int)ItemType.Coin);
                    await UniTask.Delay(300);

                    Close();
                    CoffeeAwayGame.Instance.Restart();
                });
        }
    }
}