using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MeshTool
{
    [MenuItem("Assets/Remove Collider from Prefabs")]
    public static void RemoveCollider()
    {
        // 获取选中的文件夹或预制体
        foreach (var obj in Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets))
        {
            // 检查是否为预制体
            if (obj is GameObject prefab)
            {
                // 加载预制体
                GameObject prefabInstance = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(prefab));

                // 遍历预制体的所有子对象
                foreach (Transform child in prefabInstance.GetComponentsInChildren<Transform>(true))
                {
                    // 删除 MeshCollider 组件
                    Collider[] colliders = child.GetComponents<Collider>();
                    foreach (var collider in colliders)
                    {
                        Object.DestroyImmediate(collider, true);
                    }
                }

                // 保存修改后的预制体
                PrefabUtility.SaveAsPrefabAsset(prefabInstance, AssetDatabase.GetAssetPath(prefab));
                PrefabUtility.UnloadPrefabContents(prefabInstance);
            }
        }

        // 刷新资源数据库
        AssetDatabase.Refresh();
    }
}
