//------------------------------------------------------------
// File : UIComponent.SingleUIInfo.cs
// Email: mailto:zhiqiang.yang@kingboat.io
// Desc : 
//------------------------------------------------------------
using System;
using UnityEngine;
using System.Collections.Generic;

namespace UnityGameFramework.Runtime
{
    public sealed partial class UIComponent : GameFrameworkComponent
    {
        public RectTransform InstanceRoot { get => (RectTransform)m_InstanceRoot; }
        private Camera _uiCamera = null;
        public Camera UICamera { get => _uiCamera; }
        
        #region 处理弹窗重叠问题
        public class SingleUIInfo
        {
            public int uiType;
            public object userData;
        }

        public List<SingleUIInfo> CacheUIInfo = new List<SingleUIInfo>();
        public List<SingleUIInfo> ShowedUIInfo = new List<SingleUIInfo>();



        public SingleUIInfo GetUIInfoFromCache(int uiType)
        {
            for (int i = 0; i < CacheUIInfo.Count; i++)
            {
                if (CacheUIInfo[i].uiType == uiType)
                {
                    return CacheUIInfo[i];
                }
            }
            return null;
        }

        public SingleUIInfo GetUIInfo(int uiType)
        {
            for (int i = 0; i < ShowedUIInfo.Count; i++)
            {
                if (ShowedUIInfo[i].uiType == uiType)
                {
                    return ShowedUIInfo[i];
                }
            }
            return null;
        }

        public void AddUIInfo(int uiType, object userData)
        {
            SingleUIInfo uiInfo = GetUIInfo(uiType);

            if (uiInfo == null)
            {
                uiInfo = new SingleUIInfo() { uiType = uiType, userData = userData };
                ShowedUIInfo.Add(uiInfo);
            }
            // else
            // {
            //     Log.Info("AddUIInfo already exist ui  uitype = " + uiType);
            // }
        }

        public void RemoveUIInfo(int uiType)
        {
            SingleUIInfo uiInfo = GetUIInfo(uiType);
            if (uiInfo != null)
            {
                ShowedUIInfo.Remove(uiInfo);
            }
            // else
            // {
            //     Log.Error("RemoveUIInfo  not exist ui  uitype = " + uiType);
            // }
        }

        public bool IsExist(int uiType)
        {
            return GetUIInfo(uiType) != null;
        }


        #endregion
    }
}


