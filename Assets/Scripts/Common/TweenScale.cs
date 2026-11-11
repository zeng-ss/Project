using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TweenScale : MonoBehaviour
{
    public float startValue;
    public float endValue;
    public float duration = 2f;

    public Ease ease = Ease.Linear;
    public LoopType loopType = LoopType.Yoyo;
    public int loops = -1;

    Tweener tweener;

    void OnEnable()
    {
        Kill();
        Play();
    }

    void OnDisable()
    {
        Kill();
    }

    void Kill()
    {
        if (tweener != null)
        {
            tweener.Kill();
        }
    }

    void Play()
    {
        transform.localScale = Vector3.one * startValue;
        tweener = transform.DOScale(Vector3.one * endValue, duration).SetLoops(loops, loopType).SetEase(ease);
    }
}