//------------------------------------------------------------
// File : AniBaseDialog.cs
// Email: mailto:zhiqiang.yang
// Desc : 
//------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using GameFramework;

namespace CasualGame
{
    [Serializable]
    public class UIRectAni
    {
        public float timeline = 0; //触发时间
        public RectTransform tran;
        public float duration = 0.2f;
        public float durationExit = 0.2f;
        public Ease ease = Ease.Linear;
        public Ease easeExit = Ease.Linear;
    }

    [Serializable]
    public class UIPath : UIRectAni
    {
        public Vector2 startPos;
        public Vector2 endPos;
    }

    [Serializable]
    public class UIScale : UIRectAni
    {
        public Vector3 startScale = new Vector3(0, 0, 0);
        public Vector3 endScale = new Vector3(1, 1, 1);
    }

    public class AniBaseDialog : UGuiForm
    {
        [SerializeField]
        public bool m_HasExitAni = true; //是否有退出动画

        [SerializeField]
        private List<UIPath> uIPaths;

        [SerializeField]
        private List<UIScale> uIScales;

        private float m_Duration;
        private List<UIRectAni> tempAnimation = new List<UIRectAni>();
        private float m_EndTimeStamp = 0;
        private bool m_IsExiting;           //是否正在退出动画？
        private int _status = 0;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            var imgMask = gameObject.transform.Find("ClickMask");
            if (imgMask != null && imgMask.GetComponent<Image>() != null)
            {
                UGUIEventListener.Get(imgMask.gameObject).onClick = OnClickMask;
            }
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            _status = 0;
            DoStartAni();
        }

        public override void Close()
        {
            DoEndAni();
        }

        public virtual void OnAnimationFinish(bool isExiting) { }

        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
            m_Duration += Time.deltaTime;
            if (tempAnimation.Count > 0)
            {
                for (int i = tempAnimation.Count - 1; i >= 0; i--)
                {
                    UIRectAni ani = tempAnimation[i];
                    if (m_Duration >= ani.timeline)
                    {
                        if (!m_IsExiting) //进入
                        {
                            if (ani is UIPath uIPath)
                            {
                                uIPath.tran.DOAnchorPos(uIPath.endPos, uIPath.duration).SetEase(uIPath.ease);
                            }
                            else if (ani is UIScale uIScale)
                            {
                                uIScale.tran.DOScale(uIScale.endScale, uIScale.duration).SetEase(uIScale.ease);
                            }
                        }
                        else            //退出
                        {
                            if (ani is UIPath uIPath)
                            {
                                uIPath.tran.DOAnchorPos(uIPath.startPos, uIPath.durationExit).SetEase(uIPath.easeExit);
                            }
                            else if (ani is UIScale uIScale)
                            {
                                uIScale.tran.DOScale(uIScale.startScale, uIScale.durationExit).SetEase(uIScale.easeExit);
                            }
                        }
                        tempAnimation.RemoveAt(i);
                    }
                }
            }
            else
            {
                if (m_Duration >= m_EndTimeStamp && Utility.Bitwise.IsBit(_status, 1 << 3))
                {
                    UIAnimationFinish = true;
                    Utility.Bitwise.SetBit(ref _status, 1 << 3, false);

                    if (!Utility.Bitwise.IsBit(_status, 1 << 1) && !m_IsExiting)
                    {
                        OnAnimationFinish(m_IsExiting);
                        Utility.Bitwise.SetBit(ref _status, 1 << 1, true);
                    }
                    else if (!Utility.Bitwise.IsBit(_status, 1 << 2) && m_IsExiting)
                    {
                        OnAnimationFinish(m_IsExiting);
                        Utility.Bitwise.SetBit(ref _status, 1 << 2, true);
                    }

                    if (m_IsExiting) base.Close();
                }
            }
        }

        public virtual void OnClickMask(GameObject obj)
        {
        }

        private void DoStartAni()
        {
            Utility.Bitwise.SetBit(ref _status, 1 << 3, true);//开始退出
            m_Duration = 0;
            m_EndTimeStamp = 0;
            m_IsExiting = false;
            UIAnimationFinish = false;

            if (uIPaths != null)
            {
                tempAnimation.AddRange(uIPaths);
                foreach (var item in uIPaths)
                {
                    item.tran.anchoredPosition = item.startPos;
                    float curTime = item.timeline + item.duration;
                    if (curTime > m_EndTimeStamp)
                        m_EndTimeStamp = curTime;
                }
            }
            if (uIScales != null)
            {
                tempAnimation.AddRange(uIScales);
                foreach (var item in uIScales)
                {
                    item.tran.localScale = item.startScale;
                    float curTime = item.timeline + item.duration;
                    if (curTime > m_EndTimeStamp)
                        m_EndTimeStamp = curTime;
                }
            }
        }

        private void DoEndAni()
        {
            Utility.Bitwise.SetBit(ref _status, 1 << 3, true);//开始退出

            if (m_HasExitAni)
            {
                UIAnimationFinish = false;
                m_Duration = 0;
                m_EndTimeStamp = 0;
                m_IsExiting = true;

                if (uIPaths != null)
                {
                    tempAnimation.AddRange(uIPaths);
                    foreach (var item in uIPaths)
                    {
                        // item.tran.anchoredPosition = item.startPos;
                        float curTime = item.timeline + item.durationExit;
                        if (curTime > m_EndTimeStamp)
                            m_EndTimeStamp = curTime;
                    }
                }
                if (uIScales != null)
                {
                    tempAnimation.AddRange(uIScales);
                    foreach (var item in uIScales)
                    {
                        // item.tran.localScale = item.startScale;
                        float curTime = item.timeline + item.durationExit;
                        if (curTime > m_EndTimeStamp)
                            m_EndTimeStamp = curTime;
                    }
                }

                if (tempAnimation.Count <= 0)
                {
                    UIAnimationFinish = true;
                    base.Close();
                }
            }
            else
            {
                UIAnimationFinish = true;
                base.Close();
            }
        }

    }
}
