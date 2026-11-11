using System;
using System.Collections.Generic;
using UnityEngine;

namespace CasualGame
{
    public partial class DebugHandler
    {
#if UNITY_EDITOR
        private static Dictionary<KeyCode, Action> _commands = new Dictionary<KeyCode, Action>()
        {
            {
                KeyCode.C, () =>
                {
                }
            },
            {
                KeyCode.D,
                () =>
                {
                }
            },
            {
                KeyCode.F, () => { GameEntry.Base.GameSpeed += 0.5f; }
            },
            {
                KeyCode.S, () => { GameEntry.Base.GameSpeed -= 0.5f; }
            },
            {
                KeyCode.R, () => { GameEntry.Base.GameSpeed = 1f; }
            },
            {
                KeyCode.B, () =>
                {

                }
            },
            {
                KeyCode.Q, () => { }
            },
            {
                KeyCode.Alpha1, () =>
                {

                }
            },
            {
                KeyCode.Alpha2,
                () =>
                {

                }
            },
            {
                KeyCode.Alpha3,
                () =>
                {

                }
            },
            {
                KeyCode.Alpha4, () =>
                {
                }
            },
            {
                KeyCode.Alpha5,
                () =>
                {
                }
            },
            {
                KeyCode.Alpha6,
                () =>
                {
                }
            },
        };

        public static void Update()
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
                {
                    if (!Input.GetKeyDown(keyCode)) continue;

                    if (_commands.TryGetValue(keyCode, out Action action))
                    {
                        action.Invoke();
                    }
                }
            }
        }
#endif
    }
}