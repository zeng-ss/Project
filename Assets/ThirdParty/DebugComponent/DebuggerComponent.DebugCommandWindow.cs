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
    public class CommandData
    {
        public string Name;
        public Action<string> Action;
        public Func<bool> State;
        public Func<string> Info;
        public int DebugType;
    }
    
    public sealed partial class DebuggerComponent
    {
        private List<CommandData> _commandDatas = new List<CommandData>();

        public static void SetCommandDatas(List<CommandData> commandDatas)
        {
            Instance._commandDatas = commandDatas;
        }
        
        private sealed class DebugCommandWindow : IDebuggerWindow
        {
            private const int columns = 4; // 每行列数
            private Vector2 m_ScrollPosition = Vector2.zero;
            private string _textContent = String.Empty;
            
            // 在类顶部添加样式定义

            public void Initialize(params object[] args)
            {

            }
            
            // 辅助方法创建纯色纹理
            private Texture2D CreateColorTexture(Color color)
            {
                Texture2D tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, color);
                tex.Apply();
                return tex;
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
                _textContent = GUILayout.TextField(_textContent, 30);
                
                m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition);
                {
                    OnDrawScrollableWindow();
                }
                GUILayout.EndScrollView();
            }
            
            private void OnDrawScrollableWindow()
            {
                GUIStyle normalButtonStyle = new GUIStyle(GUI.skin.button);

                GUIStyle activeButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    normal = { background = CreateColorTexture(new Color(0.2f, 0.8f, 0.2f)) }, // 绿色背景
                    hover = { background = CreateColorTexture(new Color(0.2f, 0.8f, 0.2f)) },
                    active = { textColor = Color.white }
                };
                
                GUILayout.Label("<b>System Information</b>");
                GUILayout.BeginVertical("box");
                {
                    // 计算每个按钮宽度
                    float buttonWidth = Screen.width / DefaultWindowScale / columns - 14;

                    int index = 0;

                    int lastDebugType = -1;
                    bool showTitle = false;
                    do
                    {
                        GUILayout.BeginHorizontal();
                        {
                            for (int j = 0; j < columns; j++)
                            {
                                if (index >= Instance._commandDatas.Count) break;
                                
                                CommandData commandData = Instance._commandDatas[index];

                                if (lastDebugType != commandData.DebugType)
                                {
                                    if (lastDebugType != -1)
                                    {
                                        showTitle = true;
                                    }

                                    lastDebugType = commandData.DebugType;
                                    break;
                                }

                                index++;
                                string buttonName = string.Empty;
                                if (commandData.Info != null)
                                {
                                    try
                                    {
                                        buttonName += $"<color=#70a1ff>{commandData.Info.Invoke()}</color>";
                                        buttonName += "\n";
                                    }
                                    catch (Exception e)
                                    {
                                        // ignore
                                    }
                                }
                                buttonName += commandData.Name;

                                GUIStyle currentStyle = normalButtonStyle;
                                try
                                {
                                    if (commandData.State != null)
                                    {
                                        bool state = commandData.State.Invoke();
                                        currentStyle = state ? activeButtonStyle : normalButtonStyle;
                                    }
                                }
                                catch (Exception e)
                                {
                                    // ignore
                                }
                                
                                if (GUILayout.Button(buttonName, currentStyle, GUILayout.Width(buttonWidth), GUILayout.Height(40f)))
                                {
                                    commandData.Action?.Invoke(_textContent);
                                }
                            }
                        }
                        GUILayout.EndHorizontal();

                        if (showTitle)
                        {
                            showTitle = false;
                            GUILayout.Space(10f);
                        }
                    } while (index < Instance._commandDatas.Count);
                }
                GUILayout.EndVertical();
            }
        }
    }
}
