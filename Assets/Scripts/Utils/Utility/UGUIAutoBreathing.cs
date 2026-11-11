using DG.Tweening;
using UnityEngine;

public class UGUIAutoBreathing : MonoBehaviour
{
    [Header("動畫配置")]
    [SerializeField] private float breatheDuration = 1.5f; // 單次呼吸週期（吸或呼）
    [SerializeField] private float scaleAmount = 1.1f; // 呼吸時最大縮放倍數
    [SerializeField] private float delay = 0f; // 開始前延遲

    private RectTransform rectTransform;
    private Vector3 originalScale;
    private Tween breatheTween;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogError("UGUIAutoBreathing: 未找到 RectTransform 組件！");
            return;
        }

        originalScale = rectTransform.localScale;
    }

    private void OnEnable()
    {
        StartBreathing();
    }

    private void OnDisable()
    {
        StopBreathing();
    }

    public void StartBreathing()
    {
        if (rectTransform == null) return;

        StopBreathing();
        CreateBreathingTween();
    }

    public void StopBreathing()
    {
        if (breatheTween != null && breatheTween.IsActive())
        {
            breatheTween.Kill();
        }

        if (rectTransform != null)
        {
            rectTransform.localScale = originalScale;
        }
    }

    private void CreateBreathingTween()
    {
        breatheTween = rectTransform
            .DOScale(originalScale * scaleAmount, breatheDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetDelay(delay)
            .Play();
    }

    public void SetBreatheDuration(float duration)
    {
        breatheDuration = duration;
    }

    public void SetScaleAmount(float scale)
    {
        scaleAmount = scale;
    }
}
