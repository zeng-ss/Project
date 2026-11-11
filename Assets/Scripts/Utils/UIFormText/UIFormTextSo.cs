using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public enum UIFormTextPlayMode
{
    CycleCharJump = 0,//逐个字符跳跃
    CycleCharScale = 1,//逐个字符缩放
    RotateJumpIn = 2,//逐个字符旋转跳入（未显示的先隐藏）
    SlideDoorIn = 3,//推拉门效果（字符从左侧滑入，先重叠后归位）
    WaterFloat = 4,//水面漂浮效果（每个字符独立浮动）
    ArcCurve = 5,//弧形弯曲效果（文字沿扇形弧线排列，贴合弯曲标题框）
    CycleLineScale = 6,//逐行缩放效果（每一行单独缩放，一行一行播放，然后循环往复）
}

[CreateAssetMenu(fileName = "UIFormTextData", menuName = "MX/UI面板Text动画")]
public class UIFormTextSo : ScriptableObject
{
    public UIFormTextPlayMode mode;
    public List<AnimationCurve> animationCurves;
    public List<Vector3> v3Value;
    public List<float> floatValue;
    public List<Ease> easePresets;
    public List<bool> boolPresets;
}


