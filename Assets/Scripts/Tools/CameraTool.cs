using UnityEngine;

public class CameraTool
{
    // 检测是否在相机内
    public static bool IsInScreenByWorldPosition(Camera camera, Vector3 worldPosition, float safeOffset = 0f)
    {
        Vector3 viewportPoint = camera.WorldToViewportPoint(worldPosition);

        // 检查视口坐标的 x 和 y 是否在 [0, 1] 范围内
        bool isInView = viewportPoint.x >= 0 - safeOffset && viewportPoint.x <= 1 + safeOffset &&
                        viewportPoint.y >= 0 - safeOffset && viewportPoint.y <= 1 + safeOffset &&
                        viewportPoint.z > camera.nearClipPlane && viewportPoint.z < camera.farClipPlane;

        return isInView;
    }

    public static bool IsContainCullingMask(int cullingMask , int layer)
    {
        int targetLayerMask = 1 << layer;

        // 判断是否包含该Layer
        return (cullingMask & targetLayerMask) != 0;
    }

    public static Camera GetTargetTransformCamera(Transform targetTransform)
    {
        if (!targetTransform)
        {
            return null;
        }

        foreach (Camera camera in Camera.allCameras)
        {
            if (!camera.isActiveAndEnabled) continue;
   
            if(!IsInScreenByWorldPosition(camera, targetTransform.position)) continue;
            
            if(!IsContainCullingMask(camera.cullingMask, targetTransform.gameObject.layer)) continue;

            return camera;
        }

        return null;
    }
}
