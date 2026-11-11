using UnityEngine;

namespace Framework
{
    /// <summary>
    /// 游戏启动入口。
    /// 不再依赖场景里手摆的 GameFramework.prefab，运行后自动创建 GameRoot 并拉起流程。
    /// </summary>
    public static class Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Launch()
        {
            GameRoot.Instance.Boot();
        }
    }
}
