using System;
using System.Collections.Generic;
using DG.Tweening;
using CasualGame;
using UnityEngine;
using UnityEngine.UI;

public class UIFormAnim_CellAssembleView : UIFormAnimViewBase
{
    #region 配置参数

    // floatValue[0]: 开始延迟
    // floatValue[1]: 动画时长
    // floatValue[2]: 单元格延迟
    // floatValue[3]: 最小距离
    // floatValue[4]: 最大距离
    // floatValue[5]: 初始缩放范围X
    // floatValue[6]: 初始缩放范围Y

    private float GetStartDelay => UIFormAnimSo.floatValue.Count > 0 ? UIFormAnimSo.floatValue[0] : 0f;
    private float GetDuration => UIFormAnimSo.floatValue.Count > 1 ? UIFormAnimSo.floatValue[1] : 0.7f;
    private float GetCellDelay => UIFormAnimSo.floatValue.Count > 2 ? UIFormAnimSo.floatValue[2] : 0.1f;
    private float GetMinDistance => UIFormAnimSo.floatValue.Count > 3 ? UIFormAnimSo.floatValue[3] : 100f;
    private float GetMaxDistance => UIFormAnimSo.floatValue.Count > 4 ? UIFormAnimSo.floatValue[4] : 150f;
    private Vector2 GetStartScaleRange => UIFormAnimSo.floatValue.Count > 6
        ? new Vector2(UIFormAnimSo.floatValue[5], UIFormAnimSo.floatValue[6])
        : new Vector2(0.3f, 0.5f);

    private Ease GetMoveEase => UIFormAnimSo.easePresets != null && UIFormAnimSo.easePresets.Count > 0
        ? UIFormAnimSo.easePresets[0]
        : Ease.OutCubic;

    private Ease GetScaleEase => UIFormAnimSo.easePresets != null && UIFormAnimSo.easePresets.Count > 1
        ? UIFormAnimSo.easePresets[1]
        : Ease.OutBack;

    #endregion

    #region 私有字段

    private int m_CellIndex = 0;
    private Sequence m_CellAnimSequence;
    private RectTransform m_CellRect;
    private CanvasGroup m_CellCanvasGroup;
    private Vector2 m_OriginalPos;

    #endregion

    public UIFormAnim_CellAssembleView(UIFormAnimSo uiFormAnimSo, Transform transform, CanvasGroup canvasOfPanel,
        Image blackMask) : base(uiFormAnimSo, transform, canvasOfPanel, blackMask)
    {
        InitializeCellComponents();
    }

    public UIFormAnim_CellAssembleView(UIFormAnimSo uiFormAnimSo, Transform transform, Func<float> getCanvasGroup,
        Action<float> setCanvasGroup,
        Image blackMask) : base(uiFormAnimSo, transform, getCanvasGroup, setCanvasGroup, blackMask)
    {
        InitializeCellComponents();
    }

    /// <summary>
    /// 初始化格子组件
    /// </summary>
    private void InitializeCellComponents()
    {
        m_CellRect = AnimTrans as RectTransform;
        if (m_CellRect != null)
        {
            m_CellCanvasGroup = AnimTrans.GetComponent<CanvasGroup>();
            if (m_CellCanvasGroup == null)
            {
                m_CellCanvasGroup = AnimTrans.gameObject.AddComponent<CanvasGroup>();
            }
        }
    }

    /// <summary>
    /// 设置格子的索引（用于计算延迟），并更新原始位置
    /// </summary>
    public void SetCellIndex(int index)
    {
        m_CellIndex = index;
        // 更新原始位置（此时位置应该已经设置好了）
        if (m_CellRect != null)
        {
            m_OriginalPos = m_CellRect.anchoredPosition;
        }
    }

    public override void OnStartSet()
    {
        if (m_CellRect == null) return;

        // 设置初始状态
        float angle = UnityEngine.Random.Range(0f, 90f);
        float distance = UnityEngine.Random.Range(GetMinDistance, GetMaxDistance);
        Vector2 startOffset = new Vector2(
            Mathf.Cos(angle * Mathf.Deg2Rad) * distance,
            Mathf.Sin(angle * Mathf.Deg2Rad) * distance
        );

        Vector2 startPos = m_OriginalPos + startOffset;
        float startScale = UnityEngine.Random.Range(GetStartScaleRange.x, GetStartScaleRange.y);
        float startRotation = UIFormAnimSo.fitValue > 0f ? UIFormAnimSo.fitValue : UnityEngine.Random.Range(-30f, 30f);

        m_CellRect.anchoredPosition = startPos;
        m_CellRect.localScale = Vector3.one * startScale;
        m_CellRect.localRotation = Quaternion.Euler(0, 0, startRotation);
        if (m_CellCanvasGroup != null)
        {
            m_CellCanvasGroup.alpha = 0f;
        }
    }

    public override Sequence GetOpenAnimSeq()
    {
        if (m_CellAnimSequence != null && m_CellAnimSequence.IsActive())
        {
            m_CellAnimSequence.Kill();
        }

        if (m_CellRect == null) return DOTween.Sequence();

        m_CellAnimSequence = DOTween.Sequence();

        // 计算延迟
        float delay = GetStartDelay + m_CellIndex * GetCellDelay;

        // 播放动画
        m_CellAnimSequence.Insert(delay,
            m_CellRect.DOAnchorPos(m_OriginalPos, GetDuration)
                .SetEase(GetMoveEase));
        m_CellAnimSequence.Insert(delay,
            m_CellRect.DOScale(1f, GetDuration)
                .SetEase(GetScaleEase));
        m_CellAnimSequence.Insert(delay,
            m_CellRect.DORotate(Vector3.zero, GetDuration)
                .SetEase(Ease.OutCubic));

        if (m_CellCanvasGroup != null)
        {
            m_CellAnimSequence.Insert(delay,
                m_CellCanvasGroup.DOFade(1f, GetDuration * 0.8f)
                    .SetEase(Ease.OutCubic));

            // 可选的高光效果
            if (UnityEngine.Random.value > 0.5f)
            {
                m_CellAnimSequence.Insert(delay + GetDuration * 0.7f,
                    m_CellCanvasGroup.DOFade(1.3f, GetDuration * 0.15f)
                        .SetEase(Ease.OutCubic)
                        .OnComplete(() => m_CellCanvasGroup.alpha = 1f));
            }
        }

        return m_CellAnimSequence;
    }

    public override void OnCloseSet()
    {
        // 关闭时不需要特殊设置
    }

    public override Sequence GetCloseAnimSeq()
    {
        // 关闭动画可以返回空序列，或者实现淡出效果
        var seq = DOTween.Sequence();
        if ((GetCanvasGroup?.Invoke() ?? -1f) >= 0f)
        {
            float currentAlpha = GetCanvasGroup?.Invoke() ?? 1f;
            seq.Join(
                DOTween
                    .To(() => currentAlpha,
                        t =>
                        {
                            currentAlpha = t;
                            CanvasGroup?.Invoke(t);
                        }, 0f, UIFormAnimSo.defaultOutTime).SetEase(UIFormAnimSo.outEase));
        }

        return seq;
    }

    public override (float from, float to) OpenFormToValue
    {
        get { return (1f, 1f); }
    }

    public override (float from, float to) CloseFormToValue
    {
        get { return (1f, 1f); }
    }
}
