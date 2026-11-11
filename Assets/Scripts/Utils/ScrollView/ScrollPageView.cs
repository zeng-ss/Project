using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using System.Linq;

[RequireComponent(typeof(ScrollRect))]
public class ScrollPageView : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IPointerDownHandler
{
    private enum DragState
    {
        None,       // 没有拖拽
        Dragging,   // 正在拖拽
        EndDrag     // 拖拽结束，正在平滑移动
    }
    [Tooltip("平滑时间")] public float duration = 0.1f;
    [Tooltip("划过多少就算成功")][Range(0.1f, 1f)] public float limit = 0.3f;
    [Tooltip("為 true 時子頁會拉伸為 viewport 大小；為 false 時保留換到 content 前的尺寸並置於每頁中央（捲動仍以一個 viewport 寬為一頁）。")]
    public bool fillPage = true;
    private ScrollRect _scrollRect;
    private RectTransform _content;
    private int totalPage = 0; //当前已创建的页面数量
    private DragState dragState = DragState.None;  //当前拖拽状态
    private float targetHorizontalPosition;  //当前目标水平位置
    private Vector2 rectSize = Vector2.zero; //窗口大小
    private Vector3 velocity = Vector3.zero;
    private Vector3 startPosition = Vector3.zero;
    private int selectPage = -1;
    private HashSet<int> currentVisiblePages = new HashSet<int>();
    public Action<int, HashSet<int>> OnPageChanged;
    public Action<float> OnDragDistance;
    /// <summary>按下或拖曳結束時觸發，用於輪播等重置計時</summary>
    public Action OnUserInteraction;
    public int CurrentSelectPage => selectPage;
    public Vector2 CurrentRectSize => rectSize;
    public ScrollRect ScrollRect => _scrollRect;

    public void Init(int pageCount, int selectPage)
    {
        this.selectPage = -1;
        this.currentVisiblePages.Clear();
        this.totalPage = pageCount;

        // scrollRect
        _scrollRect = GetComponent<ScrollRect>();
        _scrollRect.movementType = ScrollRect.MovementType.Clamped;
        _scrollRect.inertia = false;
        _scrollRect.vertical = false;
        _scrollRect.horizontal = true;

        // viewport
        _scrollRect.viewport.pivot = new Vector2(0f, 1f);
        _scrollRect.viewport.anchorMin = new Vector2(0f, 0f);
        _scrollRect.viewport.anchorMax = new Vector2(1f, 1f);
        rectSize = _scrollRect.viewport.rect.size;

        // content
        _content = _scrollRect.content;
        _content.anchorMin = new Vector2(0.5f, 0f);
        _content.anchorMax = new Vector2(0.5f, 1f);
        _content.pivot = new Vector2(0f, 1f);
        _content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rectSize.x * totalPage);

        ForceJumpToPage(selectPage);
    }

    public void InitItem(Transform item, int index)
    {
        RectTransform itemRect = item as RectTransform;
        Vector2 sizeBeforeAnchors = itemRect.rect.size;

        item.SetParent(_content);
        item.localScale = Vector3.one;
        item.localEulerAngles = Vector3.zero;

        Vector2 center = new Vector2(0.5f, 0.5f);
        itemRect.pivot = center;
        itemRect.anchorMin = center;
        itemRect.anchorMax = center;

        if (fillPage)
            itemRect.sizeDelta = new Vector2(rectSize.x, rectSize.y);
        else
            itemRect.sizeDelta = sizeBeforeAnchors;

        float slotX = index * rectSize.x + rectSize.x * 0.5f;
        float slotY = fillPage ? -rectSize.y * 0.5f : -itemRect.sizeDelta.y * 0.5f;
        itemRect.localPosition = new Vector2(slotX, slotY);
    }

    private void Update()
    {
        if (dragState == DragState.Dragging)
        {
            UpdateDragDistance();
        }

        if (dragState == DragState.EndDrag && totalPage > 0)
        {
            float currentPosition = _content.localPosition.x;
            float targetPosition = targetHorizontalPosition;
            if (Mathf.Abs(currentPosition - targetPosition) > 0.01f)
            {
                _content.localPosition = new Vector3(
                            Mathf.SmoothDamp(currentPosition, targetPosition, ref velocity.x, duration),
                            _content.localPosition.y,
                            _content.localPosition.z
                        );
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
        var offsetWidth = -selectPage * rectSize.x - _content.localPosition.x;
        if (Mathf.Abs(offsetWidth) > rectSize.x * limit)
        {
            int newSelectPage = selectPage;
            //向右滑动
            if (offsetWidth < 0)
            {
                newSelectPage -= 1;
            }
            //向左滑动
            else
            {
                newSelectPage += 1;
            }

            newSelectPage = Mathf.Clamp(newSelectPage, 0, totalPage - 1);
            SetSelectPage(newSelectPage);
            targetHorizontalPosition = -selectPage * rectSize.x;
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

    /// <summary>
    /// 获取需要展示的页面id列表（当前页+邻近页，共3个）
    /// </summary>
    /// <returns>展示页面id列表</returns>
    private List<int> GetAdjacentPageIds(int page)
    {
        List<int> pages = new List<int>();

        if (totalPage <= 0) return pages;

        int start = Mathf.Max(0, page - 1);
        int end = Mathf.Min(totalPage - 1, start + 2);

        if (end - start < 2)
        {
            start = Mathf.Max(0, end - 2);
        }

        for (int i = start; i <= end; i++)
        {
            pages.Add(i);
        }

        return pages;
    }

    /// <summary>
    /// 强制跳转到指定页面
    /// </summary>
    /// <param name="pageIndex">页面索引</param>
    public void ForceJumpToPage(int pageIndex)
    {
        pageIndex = Mathf.Clamp(pageIndex, 0, totalPage - 1);
        SetSelectPage(pageIndex);
        targetHorizontalPosition = -selectPage * rectSize.x;
        _content.localPosition = new Vector3(targetHorizontalPosition, _content.localPosition.y, _content.localPosition.z);
        dragState = DragState.None;
    }

    /// <summary>
    /// 手动跳转到指定页面
    /// </summary>
    /// <param name="pageIndex">页面索引</param>
    public void JumpToPage(int pageIndex)
    {
        pageIndex = Mathf.Clamp(pageIndex, 0, totalPage - 1);
        SetSelectPage(pageIndex);
        targetHorizontalPosition = -selectPage * rectSize.x;
        startPosition = _content.localPosition;
        dragState = DragState.EndDrag;
    }
}