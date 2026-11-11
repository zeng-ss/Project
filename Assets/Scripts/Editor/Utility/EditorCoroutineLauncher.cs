using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CasualGame.Editor
{
    public static class EditorCoroutineLauncher
    {
        private static List<IEnumerator> coroutines = new List<IEnumerator>();

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            EditorApplication.update += Update;
        }

        private static void Update()
        {
            for (int i = coroutines.Count - 1; i >= 0; i--)
            {
                if (!coroutines[i].MoveNext())
                {
                    coroutines.RemoveAt(i);
                }
            }
        }

        public static void StartEditorCoroutine(IEnumerator routine)
        {
            coroutines.Add(routine);
        }
    }
}
