using System.Threading;
using Cysharp.Threading.Tasks;
using Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CasualGame
{
    /// <summary>
    /// ResMgr 的便捷重载：直接按资源ID加载（ID → 路径由 ResourceIdentificationTool 提供）。
    /// </summary>
    public static class ResMgrExtension
    {
        public static UniTask<T> LoadAsset<T>(this ResMgr res, int assetId,
            CancellationToken cancellationToken = default) where T : Object
        {
            return res.LoadAssetById<T>(assetId, cancellationToken);
        }

        public static UniTask<T> LoadAndInstance<T>(this ResMgr res, int assetId,
            CancellationToken cancellationToken = default) where T : Component
        {
            return res.LoadAndInstanceById<T>(assetId, null, cancellationToken);
        }
    }
}
