using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace CasualGame
{
    /// <summary>
    /// Transform / ScrollRect 的通用小工具。
    /// </summary>
    public static class TransformExtend
    {
        /// <summary>把 ScrollRect 的内容直接吸附到指定坐标。</summary>
        public static void SnapToFixedPosition(this ScrollRect scroller, float fixValue, bool isVertical)
        {
            var contentPos = (Vector2)scroller.transform.InverseTransformPoint(scroller.content.position);

            if (isVertical) contentPos.y = fixValue;
            else contentPos.x = fixValue;

            scroller.content.anchoredPosition = contentPos;
        }

        /// <summary>呼吸缩放动画（循环）。</summary>
        public static Tween BreathAnim(this Transform item, float duration, float scale = 1.1f,
            Action onComplete = null)
        {
            item.DOKill();
            item.localScale = Vector3.one;

            Tween tween = item.DOScale(Vector3.one * scale, duration);
            tween.SetLoops(-1, LoopType.Yoyo);
            return tween;
        }
    }
}
