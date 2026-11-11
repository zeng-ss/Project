using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIFormAnim_PopFromSideView : UIFormAnimViewBase
{
    public Vector3 InV3Offset => UIFormAnimSo.v3Value[0]; //百分比
    public Vector3 OutV3Offset => UIFormAnimSo.v3Value[1]; //百分比
    public float DelayScale => UIFormAnimSo.floatValue.Count > 0 ? UIFormAnimSo.floatValue[0] : 0f;
    public float OverShoot => UIFormAnimSo.floatValue.Count > 1 ? UIFormAnimSo.floatValue[1] : -1f;
    public float OpenDuration => UIFormAnimSo.defaultInTime;
    public float CloseDuration => UIFormAnimSo.defaultOutTime;

    public bool IsNeedAlphaCanvasGroup =>
        (UIFormAnimSo.boolPresets ?? null) is not { Count: > 0 } || UIFormAnimSo.boolPresets[0];

    public bool IsScreenMode =>
        (UIFormAnimSo.boolPresets ?? null) is not { Count: > 1 } || UIFormAnimSo.boolPresets[1];

    public Vector2 StartPos;

    public UIFormAnim_PopFromSideView(UIFormAnimSo uiFormAnimSo, Transform transform, CanvasGroup canvasOfPanel,
        Image blackMask) : base(uiFormAnimSo, transform, canvasOfPanel, blackMask)
    {
        StartPos = (transform as RectTransform).anchoredPosition;
    }

    public UIFormAnim_PopFromSideView(UIFormAnimSo uiFormAnimSo, Transform transform, Func<float> getCanvasGroup,
        Action<float> setCanvasGroup,
        Image blackMask) : base(uiFormAnimSo, transform, getCanvasGroup, setCanvasGroup, blackMask)

    {
        StartPos = (transform as RectTransform).anchoredPosition;
    }

    public override void OnStartSet()
    {
        RectTransform rc = AnimTrans as RectTransform;
        rc.anchoredPosition = new Vector2(IsScreenMode ? Screen.width * InV3Offset.x : InV3Offset.x
            , IsScreenMode ? Screen.height * InV3Offset.y : InV3Offset.y);
        if ((GetCanvasGroup?.Invoke() ?? -1f) >= 0f && IsNeedAlphaCanvasGroup)
        {
            CanvasGroup?.Invoke(0f);
        }
    }

    public override Sequence GetOpenAnimSeq()
    {
        RectTransform rc = AnimTrans as RectTransform;
        rc.anchoredPosition = new Vector2(IsScreenMode ? Screen.width * InV3Offset.x : InV3Offset.x
            , IsScreenMode ? Screen.height * InV3Offset.y : InV3Offset.y);

        var seq = DOTween.Sequence().Join(rc.DOAnchorPos(StartPos, OpenDuration).SetEase(UIFormAnimSo.inEase));

        if ((GetCanvasGroup?.Invoke() ?? -1f) >= 0f && IsNeedAlphaCanvasGroup)
        {
            CanvasGroup?.Invoke(0f);
            float value = 0f;
            Tween openSeq = DOTween
                .To(() => value,
                    t =>
                    {
                        value = t;
                        CanvasGroup?.Invoke(t);
                    }, 1f, OpenDuration);
            if (OverShoot >= 0f)
            {
                openSeq.SetEase(UIFormAnimSo.inEase, OverShoot);
            }
            else
            {
                openSeq.SetEase(UIFormAnimSo.inEase);
            }

            seq.Join(openSeq);
        }

        return DOTween.Sequence().AppendInterval(DelayScale).Append(seq);
    }

    public override void OnCloseSet()
    {
        RectTransform rc = AnimTrans as RectTransform;
        rc.anchoredPosition = StartPos;

        if ((GetCanvasGroup?.Invoke() ?? -1f) >= 0f && IsNeedAlphaCanvasGroup)
        {
            CanvasGroup?.Invoke(1f);
        }
    }

    public override Sequence GetCloseAnimSeq()
    {
        RectTransform rc = AnimTrans as RectTransform;

        Vector2 targetPos = rc.anchoredPosition +
                            new Vector2(IsScreenMode ? Screen.width * OutV3Offset.x : OutV3Offset.x,
                                IsScreenMode ? Screen.height * OutV3Offset.y : OutV3Offset.y);
        var seq = DOTween.Sequence()
            .Join(rc.DOAnchorPos(targetPos, CloseDuration).SetEase(UIFormAnimSo.outEase));
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
        get { return (1f, 1f); }
    }

    public override (float from, float to) CloseFormToValue
    {
        get { return (1f, 1f); }
    }
}