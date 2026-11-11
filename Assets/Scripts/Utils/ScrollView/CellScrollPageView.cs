using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;

/// <summary>
/// 橫向分頁捲動，行為對齊 <see cref="ScrollPageView"/>（拖拽、閾值吸附、平滑），
/// 但每頁寬度由 cell 自身尺寸決定，不強制拉滿 viewport。
/// 使用方式：Init → 依序 InitItem(0..n-1)（最後一筆會完成 content 寬度並跳轉到選中頁）。
/// </summary>
[RequireComponent(typeof(ScrollRect))]
public class CellScrollPageView : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IPointerDownHandler
{
    private enum DragState
    {
        None,
        Dragging,
        EndDrag
    }

    [Tooltip("平滑時間")] public float duration = 0.1f;
    [Tooltip("劃過多少寬度算翻頁")][Range(0.1f, 1f)] public float limit = 0.3f;
    [Tooltip("cell 之間水平間距（每個 cell 右側再加此值作為下一個 cell 的起點）")]
    public float cellSpacing = 0f;
    [Tooltip("第一個 cell 左側留白")] public float paddingStart = 0f;
    [Tooltip("最後一個 cell 右側留白（加大可捲動範圍）")] public float paddingEnd = 0f;

    private ScrollRect _scrollRect;
    private RectTransform _content;
    private int totalPage;
    private DragState dragState = DragState.None;
    private float targetHorizontalPosition;
    private Vector2 _viewportSize;
    private Vector3 velocity = Vector3.zero;
    private Vector3 startPosition = Vector3.zero;
    private int selectPage = -1;
    private HashSet<int> currentVisiblePages = new HashSet<int>();

    private readonly List<float> _cellWidths = new List<float>();
    private readonly List<float> _cellLeftEdges = new List<float>();
    private readonly List<float> _snapContentX = new List<float>();
    private int _pendingSelectPage;
    private float _layoutCursorX;

    public Action<int, HashSet<int>> OnPageChanged;
    public Action<float> OnDragDistance;
    public Action OnUserInteraction;
    public int CurrentSelectPage => selectPage;
    public Vector2 ViewportSize => _viewportSize;
    public ScrollRect ScrollRect => _scrollRect;

    public void Init(int pageCount, int selectPage)
    {
        this.selectPage = -1;
        currentVisiblePages.Clear();
        totalPage = pageCount;
        _cellWidths.Clear();
        _cellLeftEdges.Clear();
        _snapContentX.Clear();
        _pendingSelectPage = selectPage;
        _layoutCursorX = paddingStart;

        _scrollRect = GetComponent<ScrollRect>();
        _scrollRect.movementType = ScrollRect.MovementType.Clamped;
        _scrollRect.inertia = false;
        _scrollRect.vertical = false;
        _scrollRect.horizontal = true;

        _scrollRect.viewport.pivot = new Vector2(0f, 1f);
        _scrollRect.viewport.anchorMin = new Vector2(0f, 0f);
        _scrollRect.viewport.anchorMax = new Vector2(1f, 1f);

        Canvas.ForceUpdateCanvases();
        _viewportSize = _scrollRect.viewport.rect.size;

        _content = _scrollRect.content;
        _content.anchorMin = new Vector2(0.5f, 0f);
        _content.anchorMax = new Vector2(0.5f, 1f);
        _content.pivot = new Vector2(0f, 1f);
        _content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0f);
    }

    /// <summary>
    /// 必須依索引 0..totalPage-1 順序呼叫；最後一個 index 會套用 content 寬度並執行跳頁。
    /// </summary>
    public void InitItem(Transform item, int index)
    {
        if (totalPage <= 0 || index < 0 || index >= totalPage)
            return;
        if (index != _cellWidths.Count)
        {
            Debug.LogWarning("[CellScrollPageView] InitItem 請依序 0..n-1 呼叫。");
            return;
        }

        RectTransform itemRect = item as RectTransform;
        Vector2 rawSize = itemRect.rect.size;

        var layout = item.GetComponent<LayoutElement>();
        if (layout != null)
        {
            if (layout.preferredWidth > 1f) rawSize.x = layout.preferredWidth;
            if (layout.preferredHeight > 1f) rawSize.y = layout.preferredHeight;
        }

        float left = _layoutCursorX;

        item.SetParent(_content);
        item.localScale = Vector3.one;
        item.localEulerAngles = Vector3.zero;

        Vector2 leftCenter = new Vector2(0f, 0.5f);
        itemRect.anchorMin = leftCenter;
        itemRect.anchorMax = leftCenter;
        itemRect.pivot = new Vector2(0f, 0.5f);

        float w = Mathf.Min(rawSize.x, _viewportSize.x);
        float h = Mathf.Min(rawSize.y, _viewportSize.y);
        itemRect.sizeDelta = new Vector2(w, h);

        // anchor 在父節點左側垂直中心 (0,0.5)：錨點已是 content 垂直正中，anchoredPosition.y 應為 0；勿再 -viewportH/2 否則會整段下移
        itemRect.anchoredPosition = new Vector2(left, 0f);

        LayoutRebuilder.ForceRebuildLayoutImmediate(itemRect);
        float measuredW = itemRect.rect.width;
        _cellLeftEdges.Add(left);
        _cellWidths.Add(measuredW);
        _snapContentX.Add(-left);

        _layoutCursorX = left + measuredW + cellSpacing;

        if (index == totalPage - 1)
        {
            float totalW = left + measuredW + paddingEnd;
            _content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Max(totalW, _viewportSize.x));
            ForceJumpToPage(_pendingSelectPage);
        }
    }

    private void Update()
    {
        if (dragState == DragState.Dragging)
            UpdateDragDistance();

        if (dragState == DragState.EndDrag && totalPage > 0 && _snapContentX.Count == totalPage)
        {
            float currentPosition = _content.localPosition.x;
            float targetPosition = targetHorizontalPosition;
            if (Mathf.Abs(currentPosition - targetPosition) > 0.01f)
            {
                _content.localPosition = new Vector3(
                    Mathf.SmoothDamp(currentPosition, targetPosition, ref velocity.x, duration),
                    _content.localPosition.y,
                    _content.localPosition.z);
                UpdateDragDistance();
                return;
            }
            dragState = DragState.None;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnUserInteraction?.Invoke();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragState = DragState.Dragging;
        startPosition = _content.localPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        OnUserInteraction?.Invoke();
        dragState = DragState.EndDrag;
        if (totalPage <= 0 || selectPage < 0 || _snapContentX.Count != totalPage)
            return;

        float targetSnap = _snapContentX[selectPage];
        float offsetWidth = targetSnap - _content.localPosition.x;
        float pageSpan = Mathf.Max(_cellWidths[selectPage], 0.01f);

        if (Mathf.Abs(offsetWidth) > pageSpan * limit)
        {
            int newSelectPage = selectPage;
            if (offsetWidth < 0)
                newSelectPage -= 1;
            else
                newSelectPage += 1;

            newSelectPage = Mathf.Clamp(newSelectPage, 0, totalPage - 1);
            SetSelectPage(newSelectPage);
            targetHorizontalPosition = _snapContentX[selectPage];
        }
    }

    private void UpdateDragDistance()
    {
        float distance = startPosition.x - _content.localPosition.x;
        startPosition = _content.localPosition;
        OnDragDistance?.Invoke(distance);
    }

    private void SetSelectPage(int newSelectPage)
    {
        if (selectPage != newSelectPage)
        {
            int lastSelectPage = selectPage;
            selectPage = newSelectPage;
            UpdateVisiblePages(lastSelectPage);
        }
    }

    private void UpdateVisiblePages(int lastSelectPage)
    {
        List<int> newVisiblePages = GetAdjacentPageIds(selectPage);
        currentVisiblePages = new HashSet<int>(newVisiblePages);
        OnPageChanged?.Invoke(lastSelectPage, currentVisiblePages);
    }

    private List<int> GetAdjacentPageIds(int page)
    {
        List<int> pages = new List<int>();
        if (totalPage <= 0) return pages;

        int start = Mathf.Max(0, page - 1);
        int end = Mathf.Min(totalPage - 1, start + 2);
        if (end - start < 2)
            start = Mathf.Max(0, end - 2);

        for (int i = start; i <= end; i++)
            pages.Add(i);

        return pages;
    }

    public void ForceJumpToPage(int pageIndex)
    {
        if (_snapContentX.Count != totalPage || totalPage <= 0)
            return;

        pageIndex = Mathf.Clamp(pageIndex, 0, totalPage - 1);
        SetSelectPage(pageIndex);
        targetHorizontalPosition = _snapContentX[selectPage];
        _content.localPosition = new Vector3(targetHorizontalPosition, _content.localPosition.y, _content.localPosition.z);
        dragState = DragState.None;
    }

    public void JumpToPage(int pageIndex)
    {
        if (_snapContentX.Count != totalPage || totalPage <= 0)
            return;

        pageIndex = Mathf.Clamp(pageIndex, 0, totalPage - 1);
        SetSelectPage(pageIndex);
        targetHorizontalPosition = _snapContentX[selectPage];
        startPosition = _content.localPosition;
        dragState = DragState.EndDrag;
    }
}
