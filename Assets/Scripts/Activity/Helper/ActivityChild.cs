//------------------------------------------------------------
// File : ActivityChild.cs
// Email: mailto:zhiqiang.yang
// Desc : 活动子面板信息
//------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CasualGame;

namespace Custom.Activity
{
    public abstract class ActivityChild<T> : MonoBehaviour, IUGUIChild where T : class, IActivity
    {
        public UGuiForm Owner { get; set; }
        public T Activity => ActivityManager.GetActivity<T>();

        public virtual void OnInit(UGuiForm owner)
        {
            this.Owner = owner;
        }

        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
       
        }

        public virtual void OnClose()
        {
        }

        public void OnInit()
        {
          
        }

        public virtual void OnOpen()
        {
        }

        public virtual void OnSecondUpdate(float time)
        {
        }
    }
}
