using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class NestedScrollRect : ScrollRect
{
    [Tooltip("判定方向的阈值（像素）。小于这个值继续等待更明显的移动方向。")]
    [SerializeField] private float directionThreshold = 5f;

    private ScrollRect _parentScrollRect;
    private ScrollRect parentScrollRect
    {
        get
        {
            if (_parentScrollRect == null)
            {
                var arr = GetComponentsInParent<ScrollRect>(true);
                foreach (var s in arr)
                {
                    if (s != this)
                    {
                        _parentScrollRect = s;
                        break;
                    }
                }
            }
            return _parentScrollRect;
        }
    }

    bool routeToParent; // 当前拖拽是否转给父处理
    bool decided;       // 是否已经决定路由方向

    protected override void OnTransformParentChanged()
    {
        base.OnTransformParentChanged();
        _parentScrollRect = null;
    }

    public override void OnInitializePotentialDrag(PointerEventData eventData)
    {
        base.OnInitializePotentialDrag(eventData);
        if (parentScrollRect != null)
            ExecuteEvents.ExecuteHierarchy(parentScrollRect.gameObject, eventData, ExecuteEvents.initializePotentialDrag);
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        routeToParent = false;
        decided = false;
        base.OnBeginDrag(eventData);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if (!decided)
        {
            Vector2 delta = eventData.delta;
            float absX = Mathf.Abs(delta.x);
            float absY = Mathf.Abs(delta.y);

            // 子竖父横：横向主导 → 父；子横父竖：纵向主导 → 父（与内层滚动轴正交）
            bool routeToParentGesture;
            bool routeToSelfGesture;
            if (!horizontal)
            {
                routeToParentGesture = absX > absY && absX > directionThreshold;
                routeToSelfGesture = absY > absX && absY > directionThreshold;
            }
            else
            {
                routeToParentGesture = absY > absX && absY > directionThreshold;
                routeToSelfGesture = absX > absY && absX > directionThreshold;
            }

            if (routeToParentGesture)
            {
                decided = true;
                if (parentScrollRect != null)
                {
                    routeToParent = true;
                    base.OnEndDrag(eventData);
                    eventData.pointerDrag = parentScrollRect.gameObject;
                    ExecuteEvents.ExecuteHierarchy(parentScrollRect.gameObject, eventData, ExecuteEvents.beginDragHandler);
                }
                else
                {
                    routeToParent = false;
                }
            }
            else if (routeToSelfGesture)
            {
                routeToParent = false;
                decided = true;
            }
        }

        if (routeToParent && parentScrollRect != null)
            ExecuteEvents.ExecuteHierarchy(parentScrollRect.gameObject, eventData, ExecuteEvents.dragHandler);
        else
            base.OnDrag(eventData);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        if (routeToParent && parentScrollRect != null)
            ExecuteEvents.ExecuteHierarchy(parentScrollRect.gameObject, eventData, ExecuteEvents.endDragHandler);
        else
            base.OnEndDrag(eventData);

        routeToParent = false;
        decided = false;
    }
}
