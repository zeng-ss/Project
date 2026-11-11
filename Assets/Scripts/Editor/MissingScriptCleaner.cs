#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace CasualGame.Editor
{
    /// <summary>
    /// 清理预制体/场景里「脚本已删除」的空组件槽（Missing Mono Script）。
    /// </summary>
    public static class MissingScriptCleaner
    {
        [MenuItem("Tools/资源/4. 清理缺失脚本", false, 103)]
        public static void CleanPrefabs()
        {
            string[] folders = { "Assets/Game", "Assets/Resources" };
            string[] guids = AssetDatabase.FindAssets("t:Prefab", folders);

            int prefabCount = 0;
            int removedCount = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                int missing = CountMissing(prefab);
                if (missing == 0) continue;

                RemoveMissing(prefab);
                EditorUtility.SetDirty(prefab);
                prefabCount++;
                removedCount += missing;
                Debug.Log($"[MissingScriptCleaner] {path}: 移除 {missing} 个失效组件");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[MissingScriptCleaner] 完成：{prefabCount} 个预制体，共移除 {removedCount} 个失效组件。");
        }

        private static int CountMissing(GameObject root)
        {
            int count = 0;
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                count += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
            }

            return count;
        }

        private static void RemoveMissing(GameObject root)
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
            }
        }
    }
}
#endif
