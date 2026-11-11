using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BoundTool
{
    public static Bounds GetBounds(Transform _targetTransform, Transform father, out int state)
    {
        if (_targetTransform == null)
        {
            state = 2;
            return new Bounds(Vector3.zero, Vector3.zero);
        }

        if (_targetTransform is RectTransform rect)
        {
            state = 1;
            return RectTransformUtility.CalculateRelativeRectTransformBounds(father, _targetTransform);
        }
        else
        {
            var target = new Bounds(_targetTransform.position, Vector3.zero);
            MeshRenderer[] mf = _targetTransform.GetComponentsInChildren<MeshRenderer>(true);
            if (mf != null && mf.Length > 0)
            {
                foreach (var m in mf)
                {
                    target.Encapsulate(m.bounds);
                }

                state = 0;
                return target;
            }
            else
            {
                SpriteRenderer[] sf = _targetTransform.GetComponentsInChildren<SpriteRenderer>(true);
                if (sf != null && sf.Length > 0)
                {
                    foreach (var sr in sf)
                    {
                        target.Encapsulate(sr.bounds);
                    }
                    state = 0;
                    return target;
                }
            }
        }

        state = 2;
        return new Bounds(Vector3.zero, Vector3.zero);
    }

    /// <summary>
    /// 将 worldBounds 在 cameraFrom 的世界空间中投影后，转换到 cameraTo 的视口空间 Rect。
    /// </summary>
    public static Rect BoundsToViewportRect(Bounds worldBounds, Camera cameraTo)
    {
        // 1. 计算 8 个角点
        Vector3 center = worldBounds.center;
        Vector3 ext = worldBounds.extents;
        Vector3[] corners = new Vector3[8]
        {
                center + new Vector3( ext.x,  ext.y,  ext.z),
                center + new Vector3( ext.x,  ext.y, -ext.z),
                center + new Vector3( ext.x, -ext.y,  ext.z),
                center + new Vector3( ext.x, -ext.y, -ext.z),
                center + new Vector3(-ext.x,  ext.y,  ext.z),
                center + new Vector3(-ext.x,  ext.y, -ext.z),
                center + new Vector3(-ext.x, -ext.y,  ext.z),
                center + new Vector3(-ext.x, -ext.y, -ext.z)
        };

        // 2. 投影到视口
        Vector2 min = new Vector2(1f, 1f);
        Vector2 max = new Vector2(0f, 0f);
        foreach (var corner in corners)
        {
            Vector3 vp = cameraTo.WorldToViewportPoint(corner);
            min.x = Mathf.Min(min.x, vp.x);
            min.y = Mathf.Min(min.y, vp.y);
            max.x = Mathf.Max(max.x, vp.x);
            max.y = Mathf.Max(max.y, vp.y);
        }

        return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
    }

    /// <summary>
    /// 将 worldBounds 转换到 cameraTo 的屏幕空间像素 Rect。
    /// </summary>
    public static Rect BoundsToScreenRect(Bounds worldBounds, Camera cameraTo)
    {
        Rect vpRect = BoundsToViewportRect(worldBounds, cameraTo);
        float x = vpRect.x * cameraTo.pixelWidth;
        float y = vpRect.y * cameraTo.pixelHeight;
        float width = vpRect.width * cameraTo.pixelWidth;
        float height = vpRect.height * cameraTo.pixelHeight;
        return new Rect(x, y, width, height);
    }
}
