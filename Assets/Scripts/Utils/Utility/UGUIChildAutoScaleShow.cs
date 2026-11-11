using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UGUIChildAutoScaleShow : MonoBehaviour
{
    [SerializeField] private float downOffset = 50f;
    [SerializeField] private bool isCustomByCode = false;
    private CanvasGroup _canvasGroup;
    private Vector3 _myStartLocalPos;
    private float showDelay = 0f;
    private Sequence _sequence;

    public void SetDelay(float delay)
    {
        showDelay = delay;
    }

    public void Play()
    {
        if (_canvasGroup == null)
            _canvasGroup = GetComponent<CanvasGroup>();

        if (!isCustomByCode)
        {
            if (_sequence.IsActive()) _sequence.Kill(true);
            _myStartLocalPos = (transform as RectTransform).anchoredPosition;
            (transform as RectTransform).anchoredPosition = _myStartLocalPos + new Vector3(0f, -downOffset, 0);
            _canvasGroup.alpha = 0f;
            _sequence = DOTween.Sequence().AppendInterval(showDelay).Append(DOTween.Sequence()
                .Join((transform as RectTransform).DOAnchorPos(_myStartLocalPos, 0.6f)).Join(DOTween.To(
                    () => (_canvasGroup.alpha),
                    s => { _canvasGroup.alpha = s; }, 1f, 0.55f))).OnComplete(() =>
            {
                (transform as RectTransform).anchoredPosition = _myStartLocalPos;
            });
        }
    }

    public void Stop()
    {
        if (!isCustomByCode && _sequence.IsActive())
        {
            _sequence.Kill(true);
        }
    }
}