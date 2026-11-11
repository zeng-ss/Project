//------------------------------------------------------------
// File : EasyInput.cs
// Email: mailto:zhiqiang.yang@cootek.cn
// Desc : 
//------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using GameFramework.UI;
using CasualGame;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public interface InputListener
{
    //参数需要时候加
    void OnMouseDown(Vector2 mousePos);

    void OnMouseUp();

    void OnMouseClick(Vector2 mousePos);

    //void OnMouseStay();

    void OnMouseMove(Vector2 mousePos);

    void OnFingerPinchZoom(float deltaDistance);

}


public class EasyInput : MonoSingleton<EasyInput>
{
    /// <summary>
    /// 可穿透UI
    /// </summary>
    private Dictionary<GameObject, bool> raycastableUI = new Dictionary<GameObject, bool>();

    private List<InputListener> inputListerners = new List<InputListener>();

    private bool isClick = false;
    private bool isZoomPinch = false;

    private float lastDistance = 0;

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            if (!EventSystem.current.IsPointerOverGameObject()
                || (EventSystem.current.IsPointerOverGameObject() && CanRaycase()))
            {
                isClick = true;
                ExecuteMouseDown(Input.mousePosition);
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (CanRaycase())
            {
                if (isClick && !isZoomPinch)
                {
                    ExecuteMouseClick(Input.mousePosition);
                }
            }
            isClick = false;
            if (!isZoomPinch)
                ExecuteMouseUp();
            isZoomPinch = false;
        }
        else if (isClick && !isZoomPinch)
        {
            ExecuteMouseMove(Input.mousePosition);
        }

        return;
#endif

        // #elif UNITY_ANDROID || UNITY_IPHONE
        if (Input.touchCount == 1)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                // IUIGroup group = GameEntry.UI.GetUIGroup("Default-0");

                if (!EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)
                    || (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId) && CanRaycase()))
                {
                    isClick = true;
                    ExecuteMouseDown(Input.GetTouch(0).position);
                }
            }
            else if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                // IUIGroup group = GameEntry.UI.GetUIGroup("Default-0");
                if (CanRaycase())
                {
                    if (isClick && !isZoomPinch)
                    {
                        ExecuteMouseClick(Input.GetTouch(0).position);
                    }
                }
                isClick = false;
                if (!isZoomPinch)
                    ExecuteMouseUp();
                isZoomPinch = false;
            }
            else if (isClick && !isZoomPinch)
            {
                ExecuteMouseMove(Input.GetTouch(0).position);
            }
        }
        else if (Input.touchCount >= 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);
            if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
            {
                lastDistance = Vector2.Distance(touch0.position, touch1.position);
                isZoomPinch = true;
            }
            else if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
            {
                float distance = Vector2.Distance(touch0.position, touch1.position);
                ExecuteFingerPinchZoom(distance - lastDistance);
                lastDistance = distance;
            }
            else if (touch0.phase == TouchPhase.Ended && touch1.phase == TouchPhase.Ended)
            {
                isZoomPinch = false;
            }
        }
        // #endif
    }


    #region private
    private void ExecuteMouseDown(Vector2 mousePos)
    {
        for (int i = 0; i < inputListerners.Count; i++)
        {
            inputListerners[i].OnMouseDown(mousePos);
        }
    }

    private void ExecuteMouseClick(Vector2 mousePos)
    {
        for (int i = 0; i < inputListerners.Count; i++)
        {
            inputListerners[i].OnMouseClick(mousePos);
        }
    }

    private void ExecuteMouseUp()
    {
        for (int i = 0; i < inputListerners.Count; i++)
        {
            inputListerners[i].OnMouseUp();
        }
    }

    private void ExecuteMouseMove(Vector2 mousePos)
    {
        for (int i = 0; i < inputListerners.Count; i++)
        {
            inputListerners[i].OnMouseMove(mousePos);
        }
    }

    private void ExecuteFingerPinchZoom(float distance)
    {
        for (int i = 0; i < inputListerners.Count; i++)
        {
            inputListerners[i].OnFingerPinchZoom(distance);
        }
    }

    public bool CanRaycase()
    {
        if (EventSystem.current == null)
        {
            return false;
        }
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        //设置鼠标位置
        pointerEventData.position = Input.mousePosition;
        //射线检测返回结果
        var results = ListPool<RaycastResult>.Get();
        //检测UI
        //graphicRaycaster.Raycast(pointerEventData, results);
        EventSystem.current.RaycastAll(pointerEventData, results);

        if (results.Count <= 0)
        {
            ListPool<RaycastResult>.Release(results);
            return true;
        }

        if (results.Count >= 1)
        {
            if (raycastableUI.ContainsKey(results[0].gameObject))
            {
                ListPool<RaycastResult>.Release(results);
                return true;
            }
        }

        ListPool<RaycastResult>.Release(results);
        return false;
    }

    #endregion

    #region public
    public void AddIgnoreUI(GameObject click)
    {
        if (raycastableUI.ContainsKey(click))
        {
            return;
        }
        raycastableUI.Add(click, true);
    }

    public void RemoveIgnoreUI(GameObject click)
    {
        if (!raycastableUI.ContainsKey(click))
        {
            return;
        }
        raycastableUI.Remove(click);
    }

    public void Register(InputListener listener)
    {
        if (inputListerners.Contains(listener))
        {
            return;
        }
        inputListerners.Add(listener);
    }

    public void UnRegister(InputListener listener)
    {
        if (!inputListerners.Contains(listener))
        {
            return;
        }
        inputListerners.Remove(listener);
    }

    #endregion
}
