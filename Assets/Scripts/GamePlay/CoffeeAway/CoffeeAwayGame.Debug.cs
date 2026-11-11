using UnityEngine;

namespace CasualGame.CoffeeAway
{
    public partial class CoffeeAwayGame
    {
        // 游戏主循环
        private void Update()
        {
            HandleDebugInput();
            if (_roadNav == null || IsLevelComplete || _isLoading) return;

            HandleInput();
            IsLevelComplete = _core.Tick();
            if (IsLevelComplete) Win();
        }

        private void HandleDebugInput()
        {
            if (Input.GetKeyDown(KeyCode.Q)) RefitCamera();
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (ProxyManager.UserProxy.GameLevel < 1) return;
                ProxyManager.UserProxy.SetGameLevel(ProxyManager.UserProxy.GameLevel - 1);
                Restart();
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                ProxyManager.UserProxy.AddGameLevel();
                Restart();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Win();
            }
        }

#if UNITY_EDITOR
        // Scene 视图调试：画出道路中心线、路径关键点、每个容器的前脸/出口方向/实际出口落点
        private void OnDrawGizmos()
        {
            // 道路中心线
            if (_roadCenterline is { Count: > 1 })
            {
                Gizmos.color = new Color(1f, 0.92f, 0.25f, 0.9f);
                for (int i = 1; i < _roadCenterline.Count; i++)
                    Gizmos.DrawLine(_roadCenterline[i - 1], _roadCenterline[i]);
            }

            // 路径关键节点
            if (_pathInfo.Nodes is { Count: > 0 })
            {
                Gizmos.color = new Color(0.2f, 0.4f, 1f, 1f);
                foreach (var node in _pathInfo.Nodes)
                    Gizmos.DrawWireSphere(node.WorldPos, 0.4f);
            }

            // 每个容器的前脸中心(青)、出口方向(青射线)、道路出口落点(绿球+连线)、
            // 未分配到落点的容器(红球) 一眼看出是道路几何问题还是容器朝向问题
            if (_core?.Holders is not null)
            {
                foreach (var holder in _core.Holders)
                {
                    if (holder == null) continue;

                    var fc = holder.FrontFaceCenter;
                    var dir = holder.ExitDirection;

                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireSphere(fc, 0.5f);
                    /*if (dir.sqrMagnitude > 0.001f)
                        Gizmos.DrawRay(fc, dir * 8f);*/

                    if (holder.RoadEntryPoint != Vector3.zero)
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawWireSphere(holder.RoadEntryPoint, 0.45f);
                        Gizmos.DrawLine(fc, holder.RoadEntryPoint);
                    }
                    else
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawWireSphere(fc, 0.7f);
                    }
                }
            }
        }
#endif
    }
}