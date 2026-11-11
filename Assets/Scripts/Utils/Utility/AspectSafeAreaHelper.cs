//------------------------------------------------------------
// File : AspectSafeAreaHelper.cs
// Email: yang.li@kingboat.io
// Desc : 
//------------------------------------------------------------
using UnityEngine;
using System;
using CasualGame;

public enum AspectSafeAreaType
{
    Top = 0,
    Bottom = 1,
    Both = 2,
    Banner = 3,
}

public class AspectSafeAreaHelper : MonoBehaviour
{
    public AspectSafeAreaType safeAreaType = AspectSafeAreaType.Both;
    public Canvas useCanvas;

    /// <summary>
    /// 如果不是ugui的panel，需要手动刷新
    /// </summary>
    public void Refresh()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector4 safeArea = GetSafeArea(safeAreaType);

        var canvas = useCanvas == null ? GameEntry.UI.MainCanvas : useCanvas;
        float fullScreenHeight = canvas.GetComponent<RectTransform>().rect.height;
        float currentHeight = rectTransform.parent.GetComponent<RectTransform>().rect.height;
        LogHandler.Info($"==> Refresh fullScreenHeight: {fullScreenHeight}, currentHeight: {currentHeight}");

        float fullMinY = safeArea.y * fullScreenHeight;
        safeArea.y = fullMinY / currentHeight;

        float fullMaxY = (1f - safeArea.w) * fullScreenHeight;
        safeArea.w = 1f - (fullMaxY / currentHeight);

        rectTransform.anchorMin = new Vector2(safeArea.x, safeArea.y);
        rectTransform.anchorMax = new Vector2(safeArea.z, safeArea.w);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        LogHandler.Info($"==> Refresh safeAreaType: {safeAreaType}, anchorMin: {rectTransform.anchorMin}, anchorMax: {rectTransform.anchorMax}");
    }

    public static Vector4 GetSafeArea(AspectSafeAreaType safeAreaType = AspectSafeAreaType.Both)
    {
        Rect safeArea = Screen.safeArea;
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        Vector2 anchorMin = Vector2.zero;
        Vector2 anchorMax = Vector2.one;

        anchorMin.x = safeArea.xMin / screenWidth;
        anchorMax.x = safeArea.xMax / screenWidth;

        if (safeAreaType == AspectSafeAreaType.Bottom || safeAreaType == AspectSafeAreaType.Both)
        {
            anchorMin.y = safeArea.yMin / screenHeight;
        }

        bool aspectBanner = safeAreaType == AspectSafeAreaType.Banner || safeAreaType == AspectSafeAreaType.Both;
        if (aspectBanner && !AdManager.Instance.IsVip() && SceneHelper.IsGamePlayScene())
        {
            float _realHeight = AdManager.Instance.GetAdaptiveBannerHeight();
            var heightDp = _realHeight <= 0 ? 60f : _realHeight;
            var density = AdManager.Instance.GetScreenDensity();
            var bannerHeight = heightDp * density;
            float bannerHeightNormalized = bannerHeight / screenHeight;
            anchorMin.y = anchorMin.y + bannerHeightNormalized;
            LogHandler.Info($"==> Banner GetSafeArea heightDp: {heightDp}, density: {density}, bannerHeight: {bannerHeight}, bannerHeightNormalized: {bannerHeightNormalized}");
        }

        if (safeAreaType == AspectSafeAreaType.Top || safeAreaType == AspectSafeAreaType.Both)
        {
            anchorMax.y = safeArea.yMax / screenHeight;
            anchorMax.y = anchorMax.y >= 0.98f ? 0.98f : anchorMax.y;
        }

        anchorMin.y = anchorMin.y <= 0.02f ? 0.02f : anchorMin.y;

        LogHandler.Info($"==> Banner GetSafeArea safeAreaType: {safeAreaType}, anchorMin: {anchorMin}, anchorMax: {anchorMax}");
        return new Vector4(anchorMin.x, anchorMin.y, anchorMax.x, anchorMax.y);
    }

#if UNITY_EDITOR
    [ContextMenu("Test")]
    private void Test()
    {
        Refresh();
    }
#endif
}