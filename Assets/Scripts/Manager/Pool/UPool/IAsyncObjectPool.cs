using System.Threading;
using Cysharp.Threading.Tasks;

public interface IAsyncObjectPool<T>
{
    UniTask<T> SpawnAsync(CancellationToken cancellationToken);
    void DeSpawn(T instance);
}