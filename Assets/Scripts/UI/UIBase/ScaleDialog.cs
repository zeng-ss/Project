using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CasualGame;
using DG.Tweening;
using UnityEngine.UI;
using System;
using Spine.Unity;

public abstract class ScaleDialog : UGuiForm
{
    private GraphicRaycaster graphicRaycaster;
    protected float maskBgAlpha = 0.9f;
    protected bool hasClickedClose;
    private Sequence sequence;

    protected override void OnInit(object userData)
    {
        base.OnInit(userData);
        graphicRaycaster = GetComponent<GraphicRaycaster>();
        var imgMask = gameObject.transform.Find("Mask");
        if (imgMask != null && imgMask.GetComponent<Image>() != null)
        {
            UGUIEventListener.Get(imgMask.gameObject).onClick = OnClickMask;
        }
    }

    public virtual void OnClickMask(GameObject obj)
    {
    }

    private float duration = 0.5f;

    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        UIAnimationFinish = false;

        if (Panel && MCanvasGroup)
        {
            if (sequence != null && sequence.IsActive()) sequence.Kill();
            sequence = DOTween.Sequence();
            MCanvasGroup.alpha = 0.6f;

            Panel.transform.localScale = Vector3.one * 0.3f;
            sequence.Append(Panel.transform.DOScale(Vector3.one, duration).SetEase(Ease.OutBack))
                .Join(MCanvasGroup.DOFade(1f, duration).OnComplete(OnCompleteScale));
        }
        else
        {
            OnCompleteScale();
        }
        graphicRaycaster.enabled = true;
    }

    protected override void OnClose(bool isShutdown, object userData)
    {
        base.OnClose(isShutdown, userData);

        hasClickedClose = false;
    }

    public override void Close()
    {
        if (hasClickedClose) return;
        hasClickedClose = true;
        graphicRaycaster.enabled = false;

        if (Panel && MCanvasGroup)
        {
            if (sequence != null && sequence.IsActive()) sequence.Kill();
            sequence = DOTween.Sequence();

            sequence.Append(Panel.transform.DOScale(Vector3.zero, duration).SetEase(Ease.InBack))
                .Join(MCanvasGroup.DOFade(0f, duration).SetEase(Ease.Linear)).OnComplete(base.Close);
        }
        else
        {
            base.Close();
        }
    }

    protected override void OnOpenCallback() { }

    protected virtual void OnCompleteScale()
    {
        UIAnimationFinish = true;
        if (MParams != null) MParams.OnOpenCallback?.Invoke(this);
    }


    // protected override void OnInit(object userData)
    // {
    //     base.OnInit(userData);

    // }
    // protected override void OnOpen(object userData)
    // {
    //     base.OnOpen(userData);
    // }

    // protected override void OnClose(bool isShutdown, object userData)
    // {
    //     base.OnClose(isShutdown, userData);
    // }
}
