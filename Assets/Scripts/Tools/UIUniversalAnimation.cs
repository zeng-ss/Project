//------------------------------------------------------------
// File : UIButtonEffect.cs
// Desc : 
//------------------------------------------------------------

using UnityEngine;
using DG.Tweening;

namespace CasualGame
{
    [DisallowMultipleComponent]
    public class UIUniversalAnimation : MonoBehaviour
    {
        public enum EffectType
        {
            Zoom = 1,           // 放大缩小
            Shake = 2,          // 晃动
            Float = 3,          // 浮动
        }

        [SerializeField] private bool isStart = true;
        [SerializeField] private float duration = 0.7f;
        [UnityEngine.Header("Shake配置")]
        [SerializeField] private float shakeStrength = 6f;
        [SerializeField] private float shakeInterval = 0.7f;
        [UnityEngine.Header("Zoom配置")]
        [SerializeField] private float zoomStrength = 0.1f;

        [Header("Float配置")]
        [SerializeField] private float floatStrength = 10f;
        [SerializeField] private float floatInterval = 0.7f;

        private Sequence _sequence;
        private Vector3 _defaultScale = Vector3.one;
        private Vector3 _defaultEulerAngles = Vector3.zero;
        private Vector2 _defaultAnchoredPosition = Vector2.zero;
        private bool _isAnimation = false;
        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _defaultAnchoredPosition = _rectTransform.anchoredPosition;
            _defaultScale = _rectTransform.localScale;
            _defaultEulerAngles = _rectTransform.localEulerAngles;
        }


        public bool IsAnimation => _isAnimation;

        public bool AnimationStart
        {
            get
            {
                return isStart;
            }
            set
            {
                if (isStart == value)
                {
                    return;
                }

                isStart = value;
                if (isStart)
                {
                    OnEnable();
                }
                else
                {
                    ButtonStop();
                }
            }
        }


        public EffectType effectType = EffectType.Zoom;
        [SerializeField] private Ease ease = Ease.Linear;

        private void OnEnable()
        {
            if (!isStart) return;

            ButtonStop();
            _isAnimation = true;
            switch (effectType)
            {
                case EffectType.Zoom:
                    ButtonScale();
                    break;
                case EffectType.Shake:
                    ButtonShake();
                    break;
                case EffectType.Float:
                {
                    ButtonFloat();
                    break;
                }
            }
        }

        private void OnDisable()
        {
            ButtonStop();
        }

        //改变按钮大小
        private void ButtonScale()
        {
            //在自身的大小上加上0.2倍
            Vector3 effectScale = transform.localScale + (new Vector3(1f, 1f, 0f) * zoomStrength);
            //设置动画
            _sequence.Append(transform.DOScale(effectScale, duration).SetEase(ease).SetLoops(-1, LoopType.Yoyo));
            _sequence.Play();
        }

        // 按钮晃动
        private void ButtonShake()
        {
            Vector3 eulerAngles = transform.eulerAngles;
            _sequence = DOTween.Sequence();
            Vector3 effectEulerAngles0 = eulerAngles;
            Vector3 effectEulerAngles1 = eulerAngles + new Vector3(0f, 0f, shakeStrength);
            Vector3 effectEulerAngles2 = eulerAngles - new Vector3(0f, 0f, shakeStrength);
            _sequence.Append(transform.DORotate(effectEulerAngles1, duration * 0.3f).SetEase(ease));
            _sequence.Append(transform.DORotate(effectEulerAngles0, duration * 0.2f).SetEase(ease));
            _sequence.Append(transform.DORotate(effectEulerAngles2, duration * 0.3f).SetEase(ease));
            _sequence.Append(transform.DORotate(effectEulerAngles0, duration * 0.2f).SetEase(ease));
            _sequence.AppendInterval(shakeInterval);
            _sequence.SetEase(ease).SetLoops(-1, LoopType.Restart);
            _sequence.Play();
        }

        private void ButtonFloat()
        {
            _sequence = DOTween.Sequence();

            _sequence.Append(_rectTransform.DOAnchorPosY(_defaultAnchoredPosition.y + floatStrength, duration * 0.5f).SetEase(ease));
            _sequence.Append(_rectTransform.DOAnchorPosY(_defaultAnchoredPosition.y, duration * 0.5f).SetEase(ease));
            _sequence.AppendInterval(floatInterval);
            _sequence.SetEase(ease).SetLoops(-1, LoopType.Restart);
            _sequence.Play();
        }

        // 按钮停止
        private void ButtonStop()
        {
            _isAnimation = false;
            transform.DOKill();
            _rectTransform.DOKill();
            if (_sequence.IsActive())
            {
                _sequence.Kill();
            }

            transform.eulerAngles = _defaultEulerAngles;
            transform.localScale = _defaultScale;
            _rectTransform.anchoredPosition = _defaultAnchoredPosition;
        }
    }
}


