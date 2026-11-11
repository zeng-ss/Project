using System;
using System.Collections.Generic;

/// <summary>可从对象池取用的对象。</summary>
public interface IReference
{
    void Clear();
}

/// <summary>
/// 极简引用池。替代原 GameFramework.ReferencePool，接口保持一致。
/// </summary>
public static class ReferencePool
{
    private static readonly Dictionary<Type, Stack<IReference>> Pools = new Dictionary<Type, Stack<IReference>>();

    public static T Acquire<T>() where T : class, IReference, new()
    {
        Stack<IReference> pool = GetPool(typeof(T));

        if (pool.Count > 0) return (T)pool.Pop();

        return new T();
    }

    public static void Release(IReference reference)
    {
        if (reference == null) return;

        reference.Clear();
        GetPool(reference.GetType()).Push(reference);
    }

    public static void ClearAll()
    {
        Pools.Clear();
    }

    private static Stack<IReference> GetPool(Type type)
    {
        if (Pools.TryGetValue(type, out Stack<IReference> pool)) return pool;

        pool = new Stack<IReference>();
        Pools[type] = pool;
        return pool;
    }
}
