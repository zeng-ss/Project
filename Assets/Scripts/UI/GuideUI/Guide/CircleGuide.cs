using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CircleGuide : GuideBase
{
    private float r;//镂空半径
    private float scaleR;//变化之后的半径大小

    //继承GuideBase基类，重写他的获取目标位置同时修改半径的方法
    public override void Guide(Canvas canvas, RectTransform target)
    {
        base.Guide(canvas, target);//继承基类里面获取中心点的计算
        //计算半径
        float width = (targetCorners[3].x - targetCorners[0].x) / 2;
        float height = (targetCorners[1].y - targetCorners[0].y) / 2;

        // r = Mathf.Sqrt(width * width + height * height);
        r = Mathf.Max(width, height);
        this.material.SetFloat("_Slider", r);
    }
    //重写基类动画方法，获取半径值来达到动画效果
    public override void Guide(Canvas canvas, RectTransform target, float scale, float time)
    {
        this.Guide(canvas, target);//需要中心点，所以直接调用上一个方法
        scaleR = r * scale;
        // scaleR = 0;
        this.material.SetFloat("_Slider", scaleR);

        this.time = time;
        isScaling = true;
        timer = 0;
    }

    public override void Clear()
    {
        r = 0;
        this.material.SetFloat("_Slider", r);
    }

    protected override void Update()
    {
        base.Update();
        if (isScaling)
        {
            this.material.SetFloat("_Slider", Mathf.Lerp(scaleR, r, timer));
        }
    }

    protected override void FixRange()
    {
        this.material.SetFloat("_Slider", r);
    }

}