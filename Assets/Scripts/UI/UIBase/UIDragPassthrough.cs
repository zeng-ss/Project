using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIDragPassthrough : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private ScrollRect parentScrollRect;

    private Vector2 _startPos;
    private bool _isDragging;

    [SerializeField]
    public Button.ButtonClickedEvent OnClick = new Button.ButtonClickedEvent();

    void Start()
    {
        // 获取父级 ScrollRect
        parentScrollRect = GetComponentInParent<ScrollRect>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 将拖动事件传递给父级 ScrollRect
        if (parentScrollRect != null)
        {
            parentScrollRect.OnBeginDrag(eventData);
        }

        _startPos = eventData.position;
        _isDragging = false;
    }

    public void OnDrag(PointerEventData eventData)
    {

        if (Vector2.Distance(_startPos, eventData.position) > 20f)
        {
            _isDragging = true;
        }

        if (parentScrollRect != null)
        {
            parentScrollRect.OnDrag(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (parentScrollRect != null)
        {
            parentScrollRect.OnEndDrag(eventData);
        }
        _isDragging = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_isDragging)
        {
            OnClick.Invoke();
        }
        else
        {
            //禁止触发点击
        }
    }
}