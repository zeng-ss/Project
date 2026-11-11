using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public abstract class UIFormAnimViewBase
{
    protected Transform AnimTrans { private set; get; }
    protected Action<float> CanvasGroup { private set; get; }
    protected Func<float> GetCanvasGroup { private set; get; }
    protected Image BlackMask { private set; get; }

    public UIFormAnimViewBase(UIFormAnimSo uiFormAnimSo, Transform transform, CanvasGroup canvasOfPanel,
        Image blackMask)
    {
        UIFormAnimSo = uiFormAnimSo;
        CanvasGroup = (a) =>
        {
            if (canvasOfPanel != null)
            {
                canvasOfPanel.alpha = a;
            }
        };
        GetCanvasGroup = () =>
        {
            if (canvasOfPanel != null)
            {
                return canvasOfPanel.alpha;
            }
            else
            {
                return -1;
            }
        };
        BlackMask = blackMask;
        AnimTrans = transform;
    }

    public UIFormAnimViewBase(UIFormAnimSo uiFormAnimSo, Transform transform, Func<float> getCanvasGroup,
        Action<float> setCanvasGroup,
        Image blackMask)
    {
        UIFormAnimSo = uiFormAnimSo;
        CanvasGroup = setCanvasGroup;
        GetCanvasGroup = getCanvasGroup;
        BlackMask = blackMask;
        AnimTrans = transform;
    }

    public UIFormAnimSo UIFormAnimSo;
    public abstract void OnStartSet();
    public abstract Sequence GetOpenAnimSeq();
    public abstract void OnCloseSet();
    public abstract Sequence GetCloseAnimSeq();

    /// <summary>
    /// OnScaleOpen函数中通知子类使用的缩放系数
    /// </summary>
    public abstract (float from, float to) OpenFormToValue { get; }

    /// <summary>
    /// OnScaleClose函数中通知子类使用的缩放系数
    /// </summary>
    public abstract (float from, float to) CloseFormToValue { get; }
}