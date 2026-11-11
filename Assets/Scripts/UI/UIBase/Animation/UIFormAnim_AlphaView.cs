using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIFormAnim_AlphaView : UIFormAnimViewBase
{
    public float OpenScale => UIFormAnimSo.floatValue[0];
    public float CloseScale => UIFormAnimSo.floatValue[1];
    public float DelayScale => UIFormAnimSo.floatValue.Count > 2 ? UIFormAnimSo.floatValue[2] : 0f;
    public float OpenDuration => UIFormAnimSo.defaultInTime;
    public float CloseDuration => UIFormAnimSo.defaultOutTime;


    public UIFormAnim_AlphaView(UIFormAnimSo uiFormAnimSo, Transform transform, CanvasGroup canvasOfPanel,
        Image blackMask) : base(uiFormAnimSo, transform, canvasOfPanel, blackMask)
    {
    }

    public UIFormAnim_AlphaView(UIFormAnimSo uiFormAnimSo, Transform transform, Func<float> getCanvasGroup,
        Action<float> setCanvasGroup,
        Image blackMask) : base(uiFormAnimSo, transform, getCanvasGroup, setCanvasGroup, blackMask)

    {
    }

    public override void OnStartSet()
    {
        if ((GetCanvasGroup?.Invoke() ?? -1f) >= 0f)
        {
            CanvasGroup?.Invoke(0f);
        }
    }

    public override Sequence GetOpenAnimSeq()
    {
        var seq = DOTween.Sequence();
        if ((GetCanvasGroup?.Invoke() ?? -1f) >= 0f)
        {
            CanvasGroup?.Invoke(0f);
            float value = 0f;
            seq.Join(DOTween
                .To(() => value,
                    t =>
                    {
                        value = t;
                        CanvasGroup?.Invoke(t);
                    }, 1f, OpenDuration).SetEase(UIFormAnimSo.inEase));
        }

        return DOTween.Sequence().AppendInterval(DelayScale).Append(seq);
    }

    public override void OnCloseSet()
    {
        if ((GetCanvasGroup?.Invoke() ?? -1f) >= 0f)
        {
            CanvasGroup?.Invoke(1f);
        }
    }

    public override Sequence GetCloseAnimSeq()
    {
        var seq = DOTween.Sequence();
        if ((GetCanvasGroup?.Invoke() ?? -1f) >= 0f)
        {
            float currentAlpha = GetCanvasGroup?.Invoke() ?? 0f;
            seq.Join(
                DOTween
                    .To(() => currentAlpha,
                        t =>
                        {
                            currentAlpha = t;
                            CanvasGroup?.Invoke(t);
                        }, 0f, CloseDuration).SetEase(UIFormAnimSo.outEase));
        }

        return seq;
    }

    public override (float from, float to) OpenFormToValue
    {
        get { return (OpenScale, 1f); }
    }

    public override (float from, float to) CloseFormToValue
    {
        get { return (1f, CloseScale); }
    }
}