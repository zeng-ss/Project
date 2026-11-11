//------------------------------------------------------------
// File : MainController.cs
// Email: mailto:zhiqiang.yang@kingboat.io
// Desc : 
//------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace CasualGame
{
    public class UIConfigMono : MonoBehaviour
    {
        public const string prefix = "AB_";

        #region 序列化数据(让外部配置)

        /// <summary>
        /// 绑定的ui控件列表，只有被绑定的控件，才可以被逻辑控制
        /// 外部不要直接使用这个，可以使用UIHandler.GetBindObject(String tag);
        /// </summary>

        public List<UIBindObject> bindObjectList = new List<UIBindObject>();

        /// <summary>
        /// ui配置参数
        /// </summary>

        // public List<UIConfigElement> configList = new List<UIConfigElement>();

        /// <summary>
        /// ui配置参数
        /// 背景音效ID
        /// </summary>

        public int bgSEId = 0;

        /// <summary>
        /// 是否启用背景蒙板
        /// </summary>

        public bool isSetMaskCollider = false;
        #endregion

        private UGuiForm mOwnerUIHandler = null;
        public UGuiForm GetUIHandler()
        {
            //已经创建，直接返回
            if (mOwnerUIHandler != null)
            {
                return mOwnerUIHandler;
            }

            return gameObject.GetComponent<UGuiForm>();
        }

        private Dictionary<string, UIBindObject> comBindObjectHash = new Dictionary<string, UIBindObject>();


        public void Init()
        {
            if (bindObjectList == null)
            {
                return;
            }

            if (GetUIHandler() == null) return;

            for (int i = 0; i < bindObjectList.Count; i++)
            {
                UIBindObject bindObject = bindObjectList[i];
                if (bindObject == null)
                {
                    continue;
                }

                this.AddBindObject(bindObject.tag, bindObject);

                //注册事件
                GetUIHandler().RegisterObjectEvent(bindObject.tag, bindObject.obj);
            }
        }



        /// <summary>
        /// 添加一个ui绑定gameObject
        /// </summary>
        /// <param name="isDynamic">true:动态（游戏运行过程添加）；false：UIConfigMono静态配置</param>
        private void AddBindObject(string tag, UIBindObject bindObject)
        {
            if (bindObject.obj == null)
            {
                Log.Error(" {0}/{1}对象为空", mOwnerUIHandler.name, tag);
                return;
            }
            if (!comBindObjectHash.ContainsKey(tag))
                comBindObjectHash.Add(tag, bindObject);
            else
            {
                Log.Error(" {0}/{1} 有重复key ", mOwnerUIHandler.name, tag);
            }
        }


        public UIBindObject GetBindEntityByTag(string tag)
        {
            UIBindObject bindEntity = null;
            if (comBindObjectHash.TryGetValue(tag, out bindEntity))
            {
                return bindEntity;
            }
            return null;
        }




    }

    /// <summary>
    /// ui控件绑定
    /// </summary>
    [System.Serializable]
    public class UIBindObject
    {
        /// <summary>
        /// 控件的唯一标记，用于定位控件，以及Lua的事件传递等
        /// </summary>
        public string tag = string.Empty;

        /// <summary>
        /// tag对应的节点object
        /// </summary>
        public GameObject obj = null;
    }

    /// <summary>
    /// ui配置
    /// </summary>
    [System.Serializable]
    public class UIConfigElement
    {
        public string cfgKey = string.Empty;
        public string cfgValue = string.Empty;
    }

}


