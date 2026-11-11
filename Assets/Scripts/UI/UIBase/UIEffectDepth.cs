//------------------------------------------------------------
// File : UIEffectDepth.cs
// Email: mailto:zhiqiang.yang@kingboat.io
// Desc : 特效层级管理

/// UI 特效做好会有很多层级
/// 例如：-1，-9，3，4，10 这5个Particle System
///      现在需求是 <=-1 的Particle System 需要显示在UI 的后面，>-1的 显示在UI 的前面
///      则这个m_ManualDepth 就应该设置为-1
//------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CasualGame
{
    public class UIEffectDepth : MonoBehaviour
    {

        [Header("手动设置的层级，> manualDepth 在父UI上级，< manualDepth 在父UI下一级,Particle System -> SortingLayer ID")]
        [SerializeField] private int m_ManualDepth = 0;//特效手动层级

        private List<Renderer> renderers = new List<Renderer>();
        private int baseDepth = 0;

        private void Start()
        {
            Refresh();
        }

        public void Refresh()
        {
            renderers.Clear();
            GetComponentsInChildren(renderers);

            Canvas parentCanvas = CUtility.Mono.GetParentFirstComponent<Canvas>(gameObject);
            if (parentCanvas)
            {
                baseDepth = parentCanvas.sortingOrder;
            }

            AutoSorting();
        }

        public void AutoSorting()
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                renderers[i].sortingOrder -= m_ManualDepth;
            }

            renderers.Sort((Renderer r1, Renderer r2) =>
            {
                return r1.sortingOrder - r2.sortingOrder;
            });


            //>0 在父UI上层级，  <0 在父UI下层   depth -> List<Renderer>
            SortedDictionary<int, List<Renderer>> rendererMap = new SortedDictionary<int, List<Renderer>>();

            int GTDepthCnt = 0;
            int LTDepthCnt = 0;
            for (int i = 0; i < renderers.Count; i++)
            {
                int depth = renderers[i].sortingOrder;

                List<Renderer> rs;
                if (rendererMap.ContainsKey(depth))
                {
                    rs = rendererMap[depth];
                }
                else
                {
                    rs = new List<Renderer>();
                    rendererMap.Add(depth, rs);

                    if (depth >= 0)
                    {
                        GTDepthCnt++;
                    }
                    else
                    {
                        LTDepthCnt++;
                    }
                }
                rs.Add(renderers[i]);
            }

            int GTDepthFirst = 1;
            foreach (var item in rendererMap)
            {
                List<Renderer> renderer = item.Value;

                if (renderer.Count > 0)
                {
                    int curDepth = renderer[0].sortingOrder;

                    int deltaDepth = 0;
                    if (curDepth >= 0)
                    {
                        deltaDepth = GTDepthFirst;
                        GTDepthFirst++;
                    }
                    else
                    {
                        deltaDepth = -LTDepthCnt;
                        LTDepthCnt--;
                    }


                    for (int k = 0; k < renderer.Count; k++)
                    {
                        renderer[k].sortingOrder = baseDepth + deltaDepth;
                    }
                }
            }

        }


        public void ManualDepth(int manualDepth)
        {
            m_ManualDepth = manualDepth;

            AutoSorting();
        }


#if UNITY_EDITOR

        [ContextMenu("Reset")]
        private void Reset()
        {
            AutoSorting();
        }

        private void OnValidate()
        {
            AutoSorting();
        }

#endif
    }
}


