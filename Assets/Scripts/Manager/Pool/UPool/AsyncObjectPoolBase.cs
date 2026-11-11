using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

public abstract class AsyncObjectPoolBase<T> : IAsyncObjectPool<T> where T : class
{
    protected readonly Stack<T> stack = new(32);
    protected bool isDisposed;
    protected abstract UniTask<T> CreateInstanceAsync(CancellationToken cancellationToken, bool isPreWarm = false);
    protected virtual void OnDisposed() { }
    protected virtual void OnDestroy(T instance) { }
    protected virtual void OnSpawn(T instance) { }
    protected virtual void OnDeSpawn(T instance) { }
    protected virtual void OnPreWarmDeSpawn(T instance) { }
    public int Count => stack.Count;
    public bool IsDisposed => isDisposed;

    /// <summary>
    /// 获取对象
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns></returns>
    public UniTask<T> SpawnAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        if (stack.TryPop(out var obj))
        {
            OnSpawn(obj);
            return new UniTask<T>(obj);
        }

        return CreateInstanceAsync(cancellationToken);
    }

    /// <summary>
    /// 回收对象
    /// </summary>
    /// <param name="obj">对象</param>
    /// <exception cref="ArgumentNullException">对象为空</exception>
    public void DeSpawn(T obj)
    {
        ThrowIfDisposed();
        if (obj == null) throw new ArgumentNullException(nameof(obj));

        OnDeSpawn(obj);
        stack.Push(obj);
    }

    /// <summary>
    /// 清空对象池
    /// </summary>
    private void Clear()
    {
        ThrowIfDisposed();
        while (stack.TryPop(out var obj))
        {
            OnDestroy(obj);
        }
        OnDisposed();
    }

    /// <summary>
    /// 预回收对象
    /// </summary>
    /// <param name="obj"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public void PreWarmDeSpawn(T obj)
    {
        ThrowIfDisposed();
        if (obj == null) throw new ArgumentNullException(nameof(obj));

        OnPreWarmDeSpawn(obj);
        stack.Push(obj);
    }

    /// <summary>
    /// 预加载
    /// </summary>
    /// <param name="count">预加载数量</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns></returns>
    public async UniTask PrewarmAsync(int count, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        for (int i = 0; i < count; i++)
        {
            var instance = await CreateInstanceAsync(cancellationToken, true);
            PreWarmDeSpawn(instance);
        }
    }

    /// <summary>
    /// 释放对象池
    /// </summary>
    public virtual void Dispose()
    {
        ThrowIfDisposed();
        Clear();
        isDisposed = true;
    }

    /// <summary>
    /// 抛出异常
    /// </summary>
    /// <exception cref="ObjectDisposedException"></exception>
    protected void ThrowIfDisposed()
    {
        if (isDisposed) throw new ObjectDisposedException(GetType().Name);
    }
}