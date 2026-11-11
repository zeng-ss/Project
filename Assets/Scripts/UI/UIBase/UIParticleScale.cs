//------------------------------------------------------------
// File : UIParticleScale.cs
// Email: mailto:zhiqiang.yang
// Desc : 
//------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CasualGame
{
    public class UIParticleScale : MonoBehaviour
    {
        private CanvasScaler m_scaler;
        private Canvas m_cancas;

        public bool AutoStart = true;
        [HideInInspector]
        public List<ScaleData> scaleDatas = null;


        void Awake()
        {
            scaleDatas = new List<ScaleData>();
            foreach (ParticleSystem p in transform.GetComponentsInChildren<ParticleSystem>(true))
            {
                scaleDatas.Add(new ScaleData() { transform = p.transform, beginScale = p.transform.localScale });
            }
        }

        private void Start()
        {
            if (AutoStart) Resize();
        }

        public void Resize()
        {
            if (m_scaler == null)
            {
                m_scaler = CUtility.Mono.GetParentFirstComponent<CanvasScaler>(gameObject);
                if (m_scaler != null)
                {
                    m_cancas = m_scaler.GetComponent<Canvas>();
                }
                else
                {
                    return;
                }
            }

            float designWidth = m_scaler.referenceResolution.x;//开发时分辨率宽
            float designHeight = m_scaler.referenceResolution.y;//开发时分辨率高
            float designScale = designWidth / designHeight;
            float scaleRate = (float)Screen.width / (float)Screen.height;

            foreach (ScaleData scale in scaleDatas)
            {
                if (scale.transform != null)
                {
                    float resultScale = designHeight / ((float)m_cancas.pixelRect.height / m_cancas.scaleFactor);
                    scale.transform.localScale = scale.beginScale * resultScale;
                }
            }
        }

        // #if UNITY_EDITOR
        //         void Update()
        //         {
        //             Resize(); //Editor下修改屏幕的大小实时预览缩放效果
        //         }
        // #endif
        public class ScaleData
        {
            public Transform transform;
            public Vector3 beginScale = Vector3.one;
        }

    }
}


