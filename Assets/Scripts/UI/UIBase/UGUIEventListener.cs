//------------------------------------------------------------
// File : UGUIEventListener.cs
// Desc : 动态监听
//------------------------------------------------------------
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace CasualGame
{
    public class UGUIEventListener : MonoBehaviour, IPointerClickHandler
    {
        public UnityAction<GameObject> onClick;

        public void OnPointerClick(PointerEventData eventData)
        {
            onClick?.Invoke(gameObject);
        }

        static public UGUIEventListener Get(GameObject go)
        {
            UGUIEventListener listener = go.GetOrAddComponent<UGUIEventListener>();
            return listener;
        }

    }
}


