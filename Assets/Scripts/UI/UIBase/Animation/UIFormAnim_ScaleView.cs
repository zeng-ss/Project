using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIFormAnim_ScaleView : UIFormAnimViewBase
{
    public float OpenScale => UIFormAnimSo.floatValue[0];
    public float CloseScale => UIFormAnimSo.floatValue[1];
    public float DelayScale => UIFormAnimSo.floatValue.Count > 2 ? UIFormAnimSo.floatValue[2] : 0f;
    public float OpenDuration => UIFormAnimSo.defaultInTime;
    public float CloseDuration => UIFormAnimSo.defaultOutTime;

    public bool IsNeedAlphaCanvasGroup =>
        (UIFormAnimSo.boolPresets ?? null) is not { Count: > 0 } || UIFormAnimSo.boolPresets[0];

    public UIFormAnim_ScaleView(UIFormAnimSo uiFormAnimSo, Transform transform, CanvasGroup canvasOfPanel,
        Image blackMask) : base(uiFormAnimSo, transform, canvasOfPanel, blackMask)
    {
    }

    public UIFormAnim_ScaleView(UIFormAnimSo uiFormAnimSo, Transform transform, Func<float> getCanvasGroup,
        Action<float> setCanvasGroup,
        Image blackMask) : base(uiFormAnimSo, transform, getCanvasGroup, setCanvasGroup, blackMask)

    {
    }

    public override void OnStartSet()
    {
        AnimTrans.localScale = OpenFormToValue.from * Vector3.one;
        if ((GetCanvasGroup?.Invoke() ?? -1f) >= 0f && IsNeedAlphaCanvasGroup)
        {
            CanvasGroup?.Invoke(0f);
        }
    }

    public override Sequence GetOpenAnimSeq()
    {
        AnimTrans.localScale = OpenFormToValue.from * Vector3.one;
        var seq = DOTween.Sequence().Join(AnimTrans.DOScale(1f, OpenDuration).SetEase(UIFormAnimSo.inEase));
        if ((GetCanvasGroup?.Invoke() ?? -1f) >= 0f && IsNeedAlphaCanvasGroup)
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
        AnimTrans.localScale = CloseFormToValue.from * Vector3.one;
        if ((GetCanvasGroup?.Invoke() ?? -1f) >= 0f && IsNeedAlphaCanvasGroup)
        {
            CanvasGroup?.Invoke(1f);
        }
    }

    public override Sequence GetCloseAnimSeq()
    {
        var seq = DOTween.Sequence().Join(AnimTrans.DOScale(CloseScale, CloseDuration).SetEase(UIFormAnimSo.outEase));
        if ((GetCanvasGroup?.Invoke() ?? -1f) >= 0f && IsNeedAlphaCanvasGroup)
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