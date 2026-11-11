using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace CasualGame.CoffeeAway
{
    public class FailUIPeekListener : MonoBehaviour, IPointerDownHandler
    {
        public Action onPeekBegin;
        public Action onPeekEnd;

        private bool m_Peeking;
        private int m_PointerId = -1;

        public void OnPointerDown(PointerEventData eventData)
        {
            m_Peeking = true;
            m_PointerId = eventData.pointerId;
            onPeekBegin?.Invoke();
        }

        private void Update()
        {
            if (!m_Peeking || IsPointerHeld(m_PointerId))
                return;

            m_Peeking = false;
            m_PointerId = -1;
            onPeekEnd?.Invoke();
        }

        private void OnDisable()
        {
            if (!m_Peeking)
                return;

            m_Peeking = false;
            m_PointerId = -1;
            onPeekEnd?.Invoke();
        }

        private static bool IsPointerHeld(int pointerId)
        {
            if (pointerId < 0)
                return Input.GetMouseButton(0);

            for (int i = 0; i < Input.touchCount; i++)
            {
                if (Input.GetTouch(i).fingerId == pointerId)
                    return true;
            }

            return false;
        }
    }
}
