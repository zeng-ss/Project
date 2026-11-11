//------------------------------------------------------------
// File : ActivatablePoolPrefabBase.cs
// Email: mailto:zhiqiang.yang@kingboat.io
// Desc : 
//------------------------------------------------------------
using UnityEngine;
using System;

namespace CasualGame
{
    [DisallowMultipleComponent]
    public abstract class ActivatablePoolPrefabBase : BaseUnitObject
    {
        public Action OnRecycle;
        private Transform m_CachedTransform = null;
        public Transform CachedTransform { get => m_CachedTransform; }

        public override void OnInit(PrefabPool ppm)//sealed
        {
            gameObject.SetActive(false);
            m_CachedTransform = transform;
        }

        public override void OnSpawn(PrefabPool ppm)
        {
            gameObject.SetActive(true);
        }

        public override void OnDeSpawn(PrefabPool ppm)
        {
            gameObject.SetActive(false);
            DoRecycle();
        }

        public virtual void DoRecycle()
        {
            OnRecycle?.Invoke();
            OnRecycle = null;
        }

    }
}