using System;
using DG.Tweening;
using UnityEngine;
using CasualGame;
using GameFramework;
using System.Collections.Generic;
using Custom.Activity;
using Cysharp.Threading.Tasks;
using System.Threading;

public enum FlyEffectType
{
    None,
    Up,
    Target,
}
public static class EffectManager
{
    /// <summary>
    /// 飞行特效参数类
    /// </summary>
    public class FlyEffectParams : IReference
    {
        /// <summary>
        /// 物品数据
        /// </summary>
        public ItemData ItemData { get; set; }

        /// <summary>
        /// 飞行起点
        /// </summary>
        public Vector3 StartPosition { get; set; }

        /// <summary>
        /// 飞行终点
        /// </summary>
        public Vector3 EndPosition { get; set; }

        /// <summary>
        /// 第一个特效完成的回调
        /// </summary>
        public Action OnFirstCompleted { get; set; }

        /// <summary>
        /// 所有特效完成的回调
        /// </summary>
        public Action OnAllCompleted { get; set; }

        /// <summary>
        /// 单个特效完成的回调
        /// </summary>
        public Action OnSingleCompleted { get; set; }

        // 以下为动画相关的辅助参数
        /// <summary>
        /// 散布半径
        /// </summary>
        public float Radius { get; set; } = 50f;

        /// <summary>
        /// 特效大小
        /// </summary>
        public float Size { get; set; } = 90f;

        /// <summary>
        /// 创建飞行特效参数
        /// </summary>
        public static FlyEffectParams Create(ItemData itemData, Vector3 startPos, Vector3 endPos,
            Action onFirstCompleted = null, Action onAllCompleted = null, float radius = 50f, float size = 90f)
        {
            FlyEffectParams flyParams = ReferencePool.Acquire<FlyEffectParams>();
            flyParams.ItemData = itemData;
            flyParams.StartPosition = startPos;
            flyParams.EndPosition = endPos;
            flyParams.OnFirstCompleted = onFirstCompleted;
            flyParams.OnAllCompleted = onAllCompleted;
            flyParams.Radius = radius;
            flyParams.Size = size;
            return flyParams;
        }

        /// <summary>
        /// 清理引用
        /// </summary>
        public void Clear()
        {
            ItemData = null;
            StartPosition = Vector3.zero;
            EndPosition = Vector3.zero;
            OnFirstCompleted = null;
            OnAllCompleted = null;
            OnSingleCompleted = null;
            Radius = 50f;
            Size = 90f;
        }
    }

    private static Vector2 WorldToUIPosition(Vector3 worldPos, RectTransform uiParent)
    {
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(GameEntry.UI.UICamera, worldPos);

        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            uiParent, screenPos, GameEntry.UI.UICamera, out localPos);

        return localPos;
    }


    public static Vector2 UIWorldPosToUiLocalPos(Vector3 worldPos)
    {
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(GameEntry.UI.MainCanvas.worldCamera, worldPos);
        var floatForm = GameEntry.UI.GetUIForm<TopcoverForm>(UIFormType.TopcoverForm);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(floatForm.Rect, screenPoint, GameEntry.UI.MainCanvas.worldCamera, out Vector2 localPos);
        return localPos;
    }

    /// <summary>
    /// 执行飞行特效
    /// </summary>
    /// <param name="flyParams">飞行特效参数</param>
    public static async void ExecuteFlyEffect(FlyEffectParams flyParams, FlyEffectType flyEffectType)
    {
        if (flyParams == null) return;

        Sequence seq = DOTween.Sequence().Pause();

        int effectCount = Mathf.Clamp(flyParams.ItemData.num, 1, 10);

        double angle = Mathf.PI * 2f / (double)effectCount;
        int[] randomOrder = CUtility.Math.GetRandomArray(0, effectCount - 1);

        RectTransform loadingGroup = TopcoverForm.Instance.Rect;
        //((UGuiGroupHelper)GameEntry.UI.GetUIGroup("Loading").Helper).transform as RectTransform;

        Vector2 uiStartPos = WorldToUIPosition(flyParams.StartPosition, loadingGroup);
        Vector2 uiEndPos = WorldToUIPosition(flyParams.EndPosition, loadingGroup);

        int validEffectCount = 0; // 记录有效的特效数量

        for (int i = 0; i < effectCount; i++)
        {
            int currentIndex = i;
            EffectPool effectCell = await PrefabPool.Instance.Spawn<EffectPool>(31005, default);
            if (effectCell == null)
            {
                Debug.LogError($"[EffectManager] 第{currentIndex}个特效对象创建失败");
                continue;
            }

            var effT = effectCell.transform;
            effT.SetParent(loadingGroup);
            effectCell.SetData(flyParams.ItemData);
            effectCell.ChangeSize(flyParams.Size);
            effectCell.gameObject.SetActive(false);
            var effectSequence = DOTween.Sequence();

            int randomIndex = randomOrder[i];
            float randomFloat = UnityEngine.Random.Range(randomIndex - 0.5f, randomIndex + 0.5f);

            // 计算随机起始位置（圆形分布）
            var randomStartPos = uiStartPos +
                new Vector2(Mathf.Sin((float)angle * randomFloat), Mathf.Cos((float)angle * randomFloat)) *
                UnityEngine.Random.Range(flyParams.Radius * 0.9f, flyParams.Radius * 1.1f);

            effT.localPosition = randomStartPos;
            effT.localScale = Vector3.one * 0.4f;

            switch (flyEffectType)
            {
                case FlyEffectType.Target:
                {
                    effectSequence
                        .AppendCallback(() => effectCell.gameObject.SetActive(true))
                        .Append(effT.DOScale(0.8f, 0.2f).SetEase(Ease.OutBack, 10f))
                        .Join(effT.DOLocalMove(randomStartPos + new Vector2(0, flyParams.Radius * 0.3f), 0.1f).SetEase(Ease.InSine))
                        .Append(effT.DOLocalMove(randomStartPos - new Vector2(0, flyParams.Radius * 1.3f), 0.4f).SetEase(Ease.OutSine))
                        .AppendCallback(() =>
                        {
                            effectCell.PlayTrailParticle();
                        })
                        .Append(effT.DOLocalMove(uiEndPos, 0.3f).SetEase(Ease.InSine))
                        .AppendCallback(() =>
                        {
                            effectCell.PlayBaoParticle();
                            flyParams.OnSingleCompleted?.Invoke();
                            if (currentIndex == 0)
                            {
                                flyParams.OnFirstCompleted?.Invoke();
                                LogHandler.Info("[EffectManager] 第一个特效完成回调已执行");
                            }
                        })
                        .Join(effT.DOScale(new Vector3(0.6f, 0.6f, 0.6f), 0.3f).SetEase(Ease.InSine))
                        .Join(effectCell.Icon.DOFade(0.3f, 0.3f).SetEase(Ease.InSine))
                        .AppendInterval(0.5f);

                    effectSequence.OnComplete(() =>
                    {
                        PrefabPool.Instance.DeSpawn(effectCell);
                    });
                    break;
                }
                case FlyEffectType.Up:
                {
                    Vector2 endPos = new Vector2(randomStartPos.x, uiStartPos.y + 150f);
                    effectSequence
                        .AppendCallback(() => effectCell.gameObject.SetActive(true))
                        .Append(effT.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack))
                        .AppendInterval(0.05f)
                        .Append(effT.DOLocalMove(randomStartPos + Vector2.down * 50f, 0.3f).SetEase(Ease.OutSine))
                        .Append(effT.DOLocalMove(endPos, 0.5f).SetEase(Ease.InSine))
                        .AppendCallback(() =>
                        {
                            effectCell.PlayBaoParticle();
                        })
                        .AppendInterval(0.2f);

                    effectSequence.OnComplete(() =>
                    {
                        PrefabPool.Instance.DeSpawn(effectCell);
                        flyParams.OnSingleCompleted?.Invoke();
                        if (currentIndex == 0)
                        {
                            flyParams.OnFirstCompleted?.Invoke();
                            LogHandler.Info("[EffectManager] 第一个特效完成回调已执行");
                        }
                    });
                    break;
                }
            }

            float delay = 0.05f * currentIndex;
            seq.Insert(delay, effectSequence);
            validEffectCount++;
        }

        if (validEffectCount == 0)
        {
            Debug.LogError("[EffectManager] 没有有效的特效，直接执行完成回调");
            flyParams.OnAllCompleted?.Invoke();
            ReferencePool.Release(flyParams);
            return;
        }

        seq.OnComplete(() =>
        {
            LogHandler.Info("[EffectManager] 所有特效完成！");
            flyParams.OnAllCompleted?.Invoke();
            ReferencePool.Release(flyParams);
        });
        seq.Play();
    }

    /// <summary>
    /// 便捷方法：根据物品数据执行飞行特效
    /// </summary>
    /// <param name="itemData">物品数据</param>
    /// <param name="effectCount">特效数量</param>
    /// <param name="startPos">起始位置</param>
    /// <param name="endPos">结束位置</param>
    /// <param name="onFirstCompleted">第一个特效完成回调</param>
    /// <param name="onAllCompleted">所有特效完成回调</param>
    public static void PlayFlyEffect(ItemData itemData, Vector3 startPos, Vector3 endPos, Action onFirstCompleted = null, Action onAllCompleted = null, float size = 90f, bool isPlaySound = true)
    {
        if (itemData.num <= 0)
        {
            onFirstCompleted?.Invoke();
            onAllCompleted?.Invoke();
            return;
        }

        var flyParams = FlyEffectParams.Create(itemData, startPos, endPos, onFirstCompleted, onAllCompleted, size: size);
        IEffectTarget effectTarget = GetEffectTarget(itemData.id);
        FlyEffectType flyEffectType = effectTarget == null ? FlyEffectType.Up : FlyEffectType.Target;
        switch (flyEffectType)
        {
            case FlyEffectType.Up:
            {
                flyParams.OnSingleCompleted = () =>
                {
                    if (isPlaySound)
                        PlayCurrencySound(itemData.id);
                };
                break;
            }
            default:
            {
                if (effectTarget != null)
                {
                    flyParams.OnSingleCompleted = () =>
                    {
                        effectTarget.PlayParticle();
                    };
                }
                break;
            }
        }

        ExecuteFlyEffect(flyParams, flyEffectType);
    }

    private static IEffectTarget GetEffectTarget(int itemId)
    {
        CasualCurrencyGroup casualCurrencyGroup = CasualCurrencyGroup.GetCasualCurrencyGroup();
        CurrencyCell currencyCell = casualCurrencyGroup.GetCurrencyCell(itemId);
        if (currencyCell != null)
        {
            return currencyCell;
        }
        else
        {
            UIMainForm uIMainForm = GameEntry.UI.GetUIForm<UIMainForm>(UIFormType.UIMainForm);
            if (uIMainForm != null)
            {

            }
            return null;
        }
    }

    public static void PlayFlyEffectToTarget(ItemData itemData, Vector3 startPos, Vector3 endPos, Action onFirstCompleted = null, Action onAllCompleted = null, float size = 90f, bool isPlaySound = true)
    {
        var flyParams = FlyEffectParams.Create(itemData, startPos, endPos, onFirstCompleted, onAllCompleted, size: size);
        flyParams.OnSingleCompleted = () =>
        {
            if (isPlaySound)
                PlayCurrencySound(itemData.id);
        };
        ExecuteFlyEffect(flyParams, FlyEffectType.Target);
    }

    public static Vector3 GetFlyEndPosition(int itemId)
    {
        var cell = GetEffectTarget(itemId);
        return cell == null ? Vector3.zero : cell.GetTransform().position;
    }

    public static void PlayCurrencySound(int id)
    {
        switch ((ItemType)id)
        {
            case ItemType.Energy:
            case ItemType.Coin:
                PlaySoundManager.Instance.PlaySound(Constant.Audio.RecivedGold);
                break;
            default:
                PlaySoundManager.Instance.PlaySound(Constant.Audio.GeneralReward);
                break;
        }
    }
}