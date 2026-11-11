using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuideHelper : MonoBehaviour, ICanvasRaycastFilter
{
    private RectTransform m_target;

    public void SetTargetArea(RectTransform area)
    {
        m_target = area;
    }

    public void ClearTargetArea()
    {
        m_target = null;
    }

    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        if (m_target == null) { return true; }
        return !RectTransformUtility.RectangleContainsScreenPoint(m_target, sp, eventCamera);
    }
}