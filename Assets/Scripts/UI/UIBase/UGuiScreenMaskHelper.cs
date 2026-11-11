using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace CasualGame
{
    [OPS.Obfuscator.Attribute.DoNotObfuscateClass]
    public class UGuiScreenMaskHelper : MonoBehaviour
    {
        private static UGuiScreenMaskHelper _instance;

        public static void OnDepthChanged(IShowMaskInterface showMaskInterface)
        {
            if (!showMaskInterface.ShowMask)
                return;
            _instance.AddInterface(showMaskInterface);
        }

        public static void CloseUI(IShowMaskInterface showMaskInterface)
        {
            if (!showMaskInterface.ShowMask)
                return;
            _instance.RemoveInterface(showMaskInterface);
        }

        /// <summary>
        /// 长按 peek 时临时隐藏遮罩，松手后恢复。
        /// </summary>
        public static void SetPeekSuppressed(bool suppressed)
        {
            if (_instance == null)
                return;

            _instance.m_PeekSuppressed = suppressed;
            _instance.ApplyVisibility();
        }

        private Canvas m_CachedCanvas = null;
        private Image m_CachedImage = null;
        private List<IShowMaskInterface> m_ShowMaskInterfaces = new List<IShowMaskInterface>();
        private const float MaskAlpha = 253f / 255f;
        private const float PeekFadeDuration = 0.2f;

        private bool m_PeekSuppressed;
        private int m_CurrentDepth;
        private Tween m_PeekFadeTween;

        private void Awake()
        {
            _instance = this;
            m_CachedCanvas = gameObject.GetOrAddComponent<Canvas>();
            gameObject.GetOrAddComponent<GraphicRaycaster>();
            gameObject.layer = LayerMask.NameToLayer("UI");
        }

        private void OnDestroy()
        {
            _instance = null;
        }

        private void Start()
        {
            RectTransform transform = GetComponent<RectTransform>();
            transform.anchorMin = Vector2.zero;
            transform.anchorMax = Vector2.one;
            transform.anchoredPosition = Vector2.zero;
            transform.sizeDelta = Vector2.zero;

            m_CachedImage = gameObject.GetOrAddComponent<Image>();
            m_CachedImage.raycastTarget = true;
            m_CachedImage.color = new Color(0, 0, 0, MaskAlpha);

            SetDepth(0);
        }

        private void AddInterface(IShowMaskInterface showMaskInterface)
        {
            if (!m_ShowMaskInterfaces.Contains(showMaskInterface))
            {
                m_ShowMaskInterfaces.Add(showMaskInterface);
            }

            CheckDepth();
        }

        private void RemoveInterface(IShowMaskInterface showMaskInterface)
        {
            if (!m_ShowMaskInterfaces.Contains(showMaskInterface))
            {
                return;
            }

            m_ShowMaskInterfaces.Remove(showMaskInterface);
            CheckDepth();
        }

        private void CheckDepth()
        {
            if (m_ShowMaskInterfaces.Count <= 0)
            {
                SetDepth(0);
                return;
            }

            int maxDepth = 0;
            for (int i = m_ShowMaskInterfaces.Count - 1; i >= 0; i--)
            {
                int currentDepath = m_ShowMaskInterfaces[i].MaskDepth - 1;
                if (currentDepath > maxDepth)
                {
                    maxDepth = currentDepath;
                }
            }

            SetDepth(maxDepth);
        }

        private void SetDepth(int depth)
        {
            m_CurrentDepth = depth;
            ApplyVisibility();
        }

        private void ApplyVisibility()
        {
            if (m_CurrentDepth <= 0)
            {
                m_PeekFadeTween?.Kill();
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);
            m_CachedCanvas.overrideSorting = true;
            m_CachedCanvas.sortingOrder = m_CurrentDepth;
            UpdateMaskAlpha();
        }

        private void UpdateMaskAlpha()
        {
            m_PeekFadeTween?.Kill();
            float targetAlpha = m_PeekSuppressed ? 0f : MaskAlpha;
            m_CachedImage.raycastTarget = !m_PeekSuppressed;
            m_PeekFadeTween = m_CachedImage.DOFade(targetAlpha, PeekFadeDuration);
        }
    }
}