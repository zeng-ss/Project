//------------------------------------------------------------
// File : GuideController.cs
// Email: mailto:zhiqiang.yang
// Desc : 
//------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;
using UnityEngine.UI;

namespace CasualGame
{
    public enum UIGuideType
    {
        Rect = 0,
        Circle = 1,
    }

    [RequireComponent(typeof(CircleGuide))]
    [RequireComponent(typeof(RectGuide))]
    public class GuideController : MonoBehaviour, ICanvasRaycastFilter
    {
        private CircleGuide circleGuide;
        private RectGuide rectGuide;

        private GuideBase currentGuide;

        public Material rectMat;
        public Material circleMat;
        private Image mask;//就是本身，纯黑半透明
        private RectTransform m_target;

        private Material newRectMat;
        private Material newCircleMat;

        private void Awake()
        {
            mask = transform.GetComponent<Image>();
            if (mask == null) { throw new System.Exception("mask初始化失败"); }
            if (rectMat == null || circleMat == null) { throw new System.Exception("材质未赋值"); }
            rectGuide = transform.GetComponent<RectGuide>();
            circleGuide = transform.GetComponent<CircleGuide>();

            newRectMat = Instantiate<Material>(rectMat);
            newCircleMat = Instantiate<Material>(circleMat);
        }

        public void Guide(Canvas canvas, RectTransform target, UIGuideType guideType)
        {
            this.m_target = target;
            switch (guideType)
            {
                case UIGuideType.Rect:
                    mask.material = newRectMat;
                    currentGuide = rectGuide;
                    currentGuide.Guide(canvas, target);
                    break;
                case UIGuideType.Circle:
                    mask.material = newCircleMat;
                    currentGuide = circleGuide;
                    currentGuide.Guide(canvas, target);
                    break;

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="target"></param>
        /// <param name="guideType"></param>
        /// <param name="scale">镂空初始的大小，在time 过后变成和按钮吻合的大小</param>
        /// <param name="time"></param>
        /// <param name="canRaycast">是否可以被点击</param>
        public void Guide(Canvas canvas, RectTransform target, UIGuideType guideType, float scale, float time, bool canRaycast = true)
        {
            switch (guideType)
            {
                case UIGuideType.Rect:
                    mask.material = newRectMat;
                    currentGuide = rectGuide;
                    currentGuide.Guide(canvas, target, scale, time);
                    break;
                case UIGuideType.Circle:
                    mask.material = newCircleMat;
                    currentGuide = circleGuide;
                    currentGuide.Guide(canvas, target, scale, time);
                    break;
            }
            if (canRaycast)
                this.m_target = target;
        }

        public void Clear()
        {
            if (currentGuide != null)
                currentGuide.Clear();

            m_target = null;
        }

        //这里的方法代表是否镂空内容可被点击，返回false则可以，true则不可以
        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            if (m_target == null) { return true; }
            return !RectTransformUtility.RectangleContainsScreenPoint(m_target, sp, eventCamera);
        }

    }
}


