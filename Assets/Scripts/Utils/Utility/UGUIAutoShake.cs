using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class UGUIAutoShake : MonoBehaviour
{
    [Header("动画配置")]
    [SerializeField] private float scaleDuration = 0.15f; // 缩放动画持续时间
    [SerializeField] private float scaleAmount = 1.2f; // 放大倍数
    [SerializeField] private float delayBetweenCycles = 2f; // 每次循环之间的延迟时间
    [SerializeField] private float delay = 0f;

    private RectTransform rectTransform;
    private Vector3 originalScale;
    private Sequence animationSequence;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogError("UGUIAutoShake: 未找到RectTransform组件！");
            return;
        }

        originalScale = rectTransform.localScale;
    }

    private void OnEnable()
    {
        StartShakeAnimation();
    }

    private void OnDisable()
    {
        StopShakeAnimation();
    }

    /// <summary>
    /// 开始摇晃动画
    /// </summary>
    public void StartShakeAnimation()
    {
        if (rectTransform == null) return;

        StopShakeAnimation();
        CreateAnimationSequence();
    }

    /// <summary>
    /// 停止摇晃动画
    /// </summary>
    public void StopShakeAnimation()
    {
        if (animationSequence.IsActive()) animationSequence.Kill();

        // 重置到初始状态
        if (rectTransform != null)
        {
            rectTransform.localScale = originalScale;
        }
    }

    /// <summary>
    /// 创建动画序列
    /// </summary>
    private void CreateAnimationSequence()
    {
        animationSequence = DOTween.Sequence();
        animationSequence.AppendInterval(delay);

        animationSequence.Append(rectTransform.DOScale(originalScale * scaleAmount, scaleDuration));
        animationSequence.Append(rectTransform.DOScale(originalScale, scaleDuration));
        animationSequence.Append(rectTransform.DOScale(originalScale * scaleAmount, scaleDuration));
        animationSequence.Append(rectTransform.DOScale(originalScale, scaleDuration));
        animationSequence.AppendInterval(1f);

        animationSequence.SetLoops(-1, LoopType.Restart);
        animationSequence.Play();
    }

    /// <summary>
    /// 设置延迟时间
    /// </summary>
    /// <param name="delay">延迟时间（秒）</param>
    public void SetDelay(float delay)
    {
        delayBetweenCycles = delay;
    }

    /// <summary>
    /// 设置缩放倍数
    /// </summary>
    /// <param name="scale">缩放倍数</param>
    public void SetScaleAmount(float scale)
    {
        scaleAmount = scale;
    }
}