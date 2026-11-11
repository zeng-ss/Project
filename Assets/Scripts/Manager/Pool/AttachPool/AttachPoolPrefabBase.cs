//------------------------------------------------------------
// File : ActivatablePoolPrefabBase.cs
// Desc : 
//------------------------------------------------------------
using UnityEngine;
using System;

namespace CasualGame
{
    [DisallowMultipleComponent]
    public abstract class AttachPoolPrefabBase : MonoBehaviour, IObjectPrefab
    {
        public int Index { get; set; }

        public virtual void Spawn()
        {
        }

        public virtual void DeSpawn()
        {
        }
    }
}