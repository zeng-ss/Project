//------------------------------------------------------------
// File : UIButtonScale.cs
// Email: mailto:zhiqiang.yang
// Desc : 
//------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;

namespace CasualGame
{
    public class UIButtonScale : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] float minScaler = 0.97f;
        [SerializeField] bool useOneScale = false;

        [HideInInspector] public bool IsReturn;

        Vector3 initScale;
        public float MinScaler { set => minScaler = value; }

        private void Awake()
        {
            initScale = transform.localScale;
            IsReturn = false;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (IsReturn) return;
            Vector3 scale = useOneScale ? Vector3.one : initScale;
            transform.DOScale(scale, 0.05f);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (IsReturn) return;
            Vector3 scale = useOneScale ? Vector3.one : initScale;
            transform.DOScale(scale * minScaler, 0.05f);
        }

    }
}


