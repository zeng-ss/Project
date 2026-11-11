using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameFramework.Resource;
using CasualGame;
using TMPro;
using UnityEngine;

public static class UIFormTextHelper
{
    public static Sequence PlayTextEffect(this string id, TMP_Text tMP_Text, UIFormTextSo uIFormTextSo)
    {
        Sequence mySeq = DOTween.Sequence();
        var view = uIFormTextSo.GetSoundView(tMP_Text);
        view.PlayText(mySeq);
        return mySeq;
    }

    public static Dictionary<TMP_Text, Sequence> textEffectDic = new Dictionary<TMP_Text, Sequence>();

    public static void ClearTextEffect(this TMP_Text tMP_Text)
    {
        if (textEffectDic.ContainsKey(tMP_Text))
        {
            var seq = textEffectDic[tMP_Text];
            if (seq.IsActive())
            {
                seq.Kill();
            }

            textEffectDic.Remove(tMP_Text);
        }
    }
    public static UIFormTextViewBase GetSoundView(this UIFormTextSo uIFormSoundData, TMP_Text tMP_Text)
    {
        switch (uIFormSoundData.mode)
        {
            case UIFormTextPlayMode.CycleCharJump:
                return new UIFormTextView_CycleCharJump(uIFormSoundData, tMP_Text);
            case UIFormTextPlayMode.CycleCharScale:
                return new UIFormTextView_CycleCharScale(uIFormSoundData, tMP_Text);
            case UIFormTextPlayMode.RotateJumpIn:
                return new UIFormTextView_RotateJumpIn(uIFormSoundData, tMP_Text);
            case UIFormTextPlayMode.SlideDoorIn:
                return new UIFormTextView_SlideDoorIn(uIFormSoundData, tMP_Text);
            case UIFormTextPlayMode.WaterFloat:
                return new UIFormTextView_WaterFloat(uIFormSoundData, tMP_Text);
            case UIFormTextPlayMode.ArcCurve:
                return new UIFormTextView_ArcCurve(uIFormSoundData, tMP_Text);
            case UIFormTextPlayMode.CycleLineScale:
                return new UIFormTextView_CycleLineScale(uIFormSoundData, tMP_Text);
            default:
                return null;
        }
    }
}
