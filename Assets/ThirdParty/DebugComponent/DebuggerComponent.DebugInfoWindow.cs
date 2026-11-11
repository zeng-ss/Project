//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

namespace IMGUIDebugger
{
    public class InfoData
    {
        public string Name;
        public Func<string> Info;

        public string GetInfo()
        {
            try
            {
                return $"{Name} : {Info?.Invoke()}";
            }
            catch (Exception e)
            {
                return $"{Name}"; 
            }
        }
    }

    public sealed partial class DebuggerComponent
    {
        private List<InfoData> _infoDatas = new List<InfoData>();

        public static void SetInfoDatas(List<InfoData> infoDatas)
        {
            Instance._infoDatas = infoDatas;
        }

        private sealed class DebugInfoWindow : IDebuggerWindow
        {
            private InfoData m_SelectedNode = null;
            private Vector2 m_LogScrollPosition = Vector2.zero;
            private Vector2 m_StackScrollPosition = Vector2.zero;
            
            public void Initialize(params object[] args)
            {
            }

            public void Shutdown()
            {
            }

            public void OnEnter()
            {
            }

            public void OnLeave()
            {
            }

            public void OnUpdate(float elapseSeconds, float realElapseSeconds)
            {
            }

            public void OnDraw()
            {
                GUILayout.BeginVertical("box");
                {
                    m_LogScrollPosition = GUILayout.BeginScrollView(m_LogScrollPosition);
                    {
                        bool selected = false;
                        foreach (InfoData infoData in Instance._infoDatas)
                        {
                            if (GUILayout.Toggle(m_SelectedNode == infoData, infoData.GetInfo()))
                            {
                                selected = true;
                                if (m_SelectedNode != infoData)
                                {
                                    m_SelectedNode = infoData;
                                    m_StackScrollPosition = Vector2.zero;
                                }
                            }
                        }

                        if (!selected)
                        {
                            m_SelectedNode = null;
                        }
                    }
                    GUILayout.EndScrollView();
                }
                GUILayout.EndVertical();

                if (m_SelectedNode != null)
                {
                    GUILayout.BeginVertical("box");
                    {
                        m_StackScrollPosition = GUILayout.BeginScrollView(m_StackScrollPosition,
                            GUILayout.Height(800f / DefaultWindowScale));
                        {
                            if (m_SelectedNode != null)
                            {
                                if (GUILayout.Button(m_SelectedNode.GetInfo(), "label"))
                                {
                                    CopyToClipboard(m_SelectedNode.GetInfo());
                                }
                            }
                        }
                        GUILayout.EndScrollView();
                    }
                    GUILayout.EndVertical();
                }
            }
        }
    }
}