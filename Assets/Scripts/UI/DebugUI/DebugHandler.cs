using System;
using UnityEngine;

namespace CasualGame
{
    public partial class DebugHandler
    {
        public static bool EnableDebug
        {
            get
            {
#if UNITY_EDITOR
                return true;
#endif
                return false;
            }
            set
            {
            }
        }

        private static int _click;
        private static DateTime _lastClickTime;
        public static void TriggerDebugger()
        {
            DateTime now = DateTime.Now;
            if ((now - _lastClickTime).TotalMilliseconds <= 300)
            {
                // appendDebug = true;
                _click++;
                if (_click >= 8)
                {
                    _click = 0;

                    if (GUIUtility.systemCopyBuffer == "WordScrew2026")
                    {
                        UIManager.Toast("Debug Now");
                        EnableDebug = true;
                        SetGMButton(true);
                    }
                }
            }
            else
            {
                _click = 0; // 不是连续点击时，清0
            }

            _lastClickTime = now;
        }

        public static void SetGMButton(bool state)
        {
            IMGUIDebugger.DebuggerComponent.ActiveWindow = state;
        }

        public static void CloseDebuggerUI()
        {
            IMGUIDebugger.DebuggerComponent.ShowFullWindow = false;
        }
    }
}