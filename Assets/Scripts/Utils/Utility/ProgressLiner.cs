//------------------------------------------------------------
// File : ProgressLiner.cs
// Desc : 
//------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CasualGame
{
    public class ProgressLiner : MonoBehaviour
    {
        private enum Direction
        {
            Horizontal,
            Vertical,
        }

        [SerializeField] private RectTransform progressBg;
        [SerializeField] private RectTransform progress;
        [SerializeField] private Direction direction;
        [SerializeField] private float borderOffset;
        [SerializeField] private float startOffset;
        [SerializeField] private float endOffset;
        [NonSerialized] public float Percent;

        public void SetProgress(float percent)
        {
            Percent = Mathf.Clamp01(percent);
            progress.sizeDelta =
                new Vector2(
                    startOffset + (progressBg.rect.width - (borderOffset * 2) - startOffset - endOffset) * percent,
                    progress.sizeDelta.y);
        }
    }
}
