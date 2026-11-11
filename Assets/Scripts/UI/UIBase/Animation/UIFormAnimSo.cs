using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using System;
using UnityEngine.UI;

public enum UIFormAnimMode
{
    NormalScale = 0, //默认缩放效果
    SideInAndSideOut = 1, //边缘进出效果

    // ButtonScaleInOut = 2, //从按钮位置缩放
    AlphaInOut = 2, //渐变效果
    CellAssemble = 3, //单元格组装效果
}

[CreateAssetMenu(fileName = "UIFormAnimData", menuName = "Tools/UI面板显示动画")]
public class UIFormAnimSo : ScriptableObject
{
    public UIFormAnimMode uiFormAnimMode = UIFormAnimMode.NormalScale;
    public float defaultInTime;
    public float defaultOutTime;
    public Ease inEase;
    public Ease outEase;

    #region 额外数据

    public List<AnimationCurve> animationCurves;
    public List<Vector3> v3Value;
    public List<float> floatValue;
    public List<Ease> easePresets;
    public List<bool> boolPresets;
    public float fitValue;

    #endregion



}


public static class UIFormAnimSoExtension
{
    public static UIFormAnimViewBase GetUIFormAnimView(this UIFormAnimSo uiFormAnimSo, Transform panel,
       Func<float> getAlpha, Action<float> setAlpha,
       Image blackMask)
    {
        switch (uiFormAnimSo.uiFormAnimMode)
        {
            case UIFormAnimMode.NormalScale:
                return new UIFormAnim_ScaleView(uiFormAnimSo, panel, getAlpha, setAlpha, blackMask);
            case UIFormAnimMode.SideInAndSideOut:
                return new UIFormAnim_PopFromSideView(uiFormAnimSo, panel, getAlpha, setAlpha, blackMask);
            case UIFormAnimMode.AlphaInOut:
                return new UIFormAnim_AlphaView(uiFormAnimSo, panel, getAlpha, setAlpha, blackMask);
            case UIFormAnimMode.CellAssemble:
                return new UIFormAnim_CellAssembleView(uiFormAnimSo, panel, getAlpha, setAlpha, blackMask);
        }

        return new UIFormAnim_ScaleView(uiFormAnimSo, panel, getAlpha, setAlpha, blackMask);
    }

    public static UIFormAnimViewBase GetUIFormAnimView(this UIFormAnimSo uiFormAnimSo, Transform panel,
      CanvasGroup canvasOfPanel,
      Image blackMask)
    {
        switch (uiFormAnimSo.uiFormAnimMode)
        {
            case UIFormAnimMode.NormalScale:
                return new UIFormAnim_ScaleView(uiFormAnimSo, panel, canvasOfPanel, blackMask);
                break;
            case UIFormAnimMode.SideInAndSideOut:
                return new UIFormAnim_PopFromSideView(uiFormAnimSo, panel, canvasOfPanel, blackMask);
                break;
            case UIFormAnimMode.AlphaInOut:
                return new UIFormAnim_AlphaView(uiFormAnimSo, panel, canvasOfPanel, blackMask);
                break;
            case UIFormAnimMode.CellAssemble:
                return new UIFormAnim_CellAssembleView(uiFormAnimSo, panel, canvasOfPanel, blackMask);
                break;
                // case UIFormAnimMode.ButtonScaleInOut:
                //     break;
        }

        return new UIFormAnim_ScaleView(uiFormAnimSo, panel, canvasOfPanel, blackMask);
    }
}