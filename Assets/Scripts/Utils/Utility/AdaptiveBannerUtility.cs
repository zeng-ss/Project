using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 静态工具：创建与管理一个 adaptive banner placeholder（底部，撑满宽度）。
/// 用法：
///   GameObject banner = AdaptiveBannerUtility.CreateBanner(myCanvas);
///   // 若要删除： Destroy(banner);
/// </summary>
public static class AdaptiveBannerUtility
{
    private static AdaptiveBannerPlaceholderBehaviour banner;

    public static void DestroyBanner()
    {
        if (banner != null)
        {
            banner.DestroyPlaceholder();
            banner = null;
        }
    }

    public static GameObject CreateBanner(Canvas canvas)
    {
        DestroyBanner();

        Color color = Color.white;

        GameObject go = new GameObject("AdaptiveBanner");
        var group = CasualGame.GameEntry.UI.GetUIGroup("Loading");
        go.transform.SetParent(((CasualGame.UGuiGroupHelper)group.Helper).transform, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        Image img = go.AddComponent<Image>();
        img.raycastTarget = false;
        img.color = color;
        img.type = Image.Type.Simple;

        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;

        banner = go.AddComponent<AdaptiveBannerPlaceholderBehaviour>();
        banner.Initialize(canvas, rt, true);

        banner.UpdateLayoutImmediate();

        return go;
    }
}

/// <summary>
/// MonoBehaviour：负责按 AdManager.Instance.GetAdaptiveBannerHeight + AdManager.Instance.GetScreenDensity 的返回值计算高度，
/// 并将占位放到底部（考虑 safe area）
/// </summary>
public class AdaptiveBannerPlaceholderBehaviour : MonoBehaviour
{
    Canvas uiCanvas;
    RectTransform rectTransform;
    bool autoUpdate = true;
    int lastW, lastH;

    internal void Initialize(Canvas canvas, RectTransform rt, bool autoUpdateOnResolutionChange)
    {
        uiCanvas = canvas;
        rectTransform = rt;
        autoUpdate = autoUpdateOnResolutionChange;
        lastW = Screen.width;
        lastH = Screen.height;
    }

    void Start()
    {
        UpdateLayoutImmediate();
    }

    void Update()
    {
        if (!autoUpdate) return;
        if (Screen.width != lastW || Screen.height != lastH)
        {
            lastW = Screen.width;
            lastH = Screen.height;
            UpdateLayoutImmediate();
        }
    }

    public void UpdateLayoutImmediate()
    {
        if (rectTransform == null || uiCanvas == null) return;

        float density = 1f;
        try
        {
            density = AdManager.Instance.GetScreenDensity();
        }
        catch
        {
            density = (Screen.dpi > 0f ? Screen.dpi / 160f : 1f);
        }
        if (density <= 0f) density = 1f;

        Rect safe = Screen.safeArea;
        float returnedValue = 0f;
        try
        {
            returnedValue = AdManager.Instance.GetAdaptiveBannerHeight();
        }
        catch
        {
            returnedValue = Mathf.Clamp((Screen.height * 0.15f) / density, 50f, 90f);
        }

        float heightPx = returnedValue * density;
        if (heightPx > Screen.height * 0.95f) // 如果超出屏幕高度（接近不合理值），认为SDK返回的是px
        {
            heightPx = returnedValue; // treat as px
            LogHandler.Info("[AdaptiveBanner] Heuristic: SDK returned px directly.");
        }

        float scaleFactor = uiCanvas.scaleFactor;
        scaleFactor = Mathf.Max(1e-6f, scaleFactor);
        float bannerUiHeight = heightPx / scaleFactor;

        float safeInsetBottomPx = safe.y;
        float safeInsetBottomUi = safeInsetBottomPx / scaleFactor;

        // apply
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, safeInsetBottomUi);
        rectTransform.sizeDelta = new Vector2(uiCanvas.GetComponent<RectTransform>().rect.width * 0.65f, bannerUiHeight);

        GameObject txt = new GameObject("Banner");
        RectTransform txtTrans = txt.AddComponent<RectTransform>();
        txtTrans.SetParent(rectTransform);
        txtTrans.localScale = Vector3.one;
        txtTrans.anchorMin = new Vector2(0.5f, 0.5f);
        txtTrans.anchorMax = new Vector2(0.5f, 0.5f);
        txtTrans.pivot = new Vector2(0.5f, 0.5f);
        txtTrans.anchoredPosition = Vector2.zero;
        TMPro.TextMeshProUGUI txtComponent = txtTrans.gameObject.AddComponent<TMPro.TextMeshProUGUI>();
        txtComponent.text = "Banner";
        txtComponent.color = Color.black;
        txtComponent.verticalAlignment = TMPro.VerticalAlignmentOptions.Middle;
        txtComponent.horizontalAlignment = TMPro.HorizontalAlignmentOptions.Center;
        txtComponent.fontSize = 30;

        LogHandler.Info($"[AdaptiveBanner] returned={returnedValue} (dpOrPx), density={density}, heightPx={heightPx}, scaleFactor={scaleFactor}, bannerUiH={bannerUiHeight}, safeInsetBottomUi={safeInsetBottomUi}");
    }


    /// <summary>
    /// 外部可调用：销毁占位物
    /// </summary>
    public void DestroyPlaceholder()
    {
        Destroy(gameObject);
    }
}
