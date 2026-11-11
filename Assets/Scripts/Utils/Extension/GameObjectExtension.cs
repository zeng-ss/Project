using UnityEngine;

/// <summary>
/// GameObject / Component 的扩展方法。替代原 GameFramework.Utility 里的同名扩展。
/// </summary>
public static class GameObjectExtension
{
    /// <summary>取组件，没有就加上。</summary>
    public static T GetOrAddComponent<T>(this GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();
        return component != null ? component : go.AddComponent<T>();
    }

    /// <summary>取组件，没有就加上。</summary>
    public static T GetOrAddComponent<T>(this Component comp) where T : Component
    {
        return comp.gameObject.GetOrAddComponent<T>();
    }

    /// <summary>递归设置自身及所有子节点的 Layer。</summary>
    public static void SetLayerRecursively(this GameObject go, int layer)
    {
        go.layer = layer;

        foreach (Transform child in go.transform)
        {
            child.gameObject.SetLayerRecursively(layer);
        }
    }
}
