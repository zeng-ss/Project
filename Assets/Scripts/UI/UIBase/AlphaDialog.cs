using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CasualGame;
using DG.Tweening;
using UnityEngine.UI;
using System;
using Spine.Unity;

public abstract class AlphaDialog : UGuiForm
{
    private GraphicRaycaster graphicRaycaster;
    protected float maskBgAlpha = 0.9f;

    private bool hasClickedClose;

    private Sequence sequence;

    private Vector3 originalPosition;

    protected override void OnInit(object userData)
    {
        base.OnInit(userData);

        graphicRaycaster = GetComponent<GraphicRaycaster>();
        originalPosition = Panel.transform.localPosition;

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
            MCanvasGroup.alpha = 1f;
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
            sequence.Append(MCanvasGroup.DOFade(0f, duration).SetEase(Ease.Linear)).OnComplete(base.Close);
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
