using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace CasualGame
{
    public class TweenRotation : MonoBehaviour
    {
        public Vector3 startValue = Vector3.zero;
        public Vector3 endValue = new Vector3(0, 0, -360);
        public float duration = 2f;

        public RotateMode rotateMode = RotateMode.WorldAxisAdd;
        public Ease ease = Ease.Linear;
        public LoopType loopType = LoopType.Restart;
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
            if (tweener.IsActive())
            {
                tweener.Kill();
            }
        }

        void Play()
        {
            transform.localEulerAngles = startValue;
            tweener = transform.DOLocalRotate(endValue, duration, rotateMode).SetLoops(loops, loopType).SetEase(ease);
        }
    }
}