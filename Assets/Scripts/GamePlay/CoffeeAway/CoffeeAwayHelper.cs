using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CasualGame.CoffeeAway
{
    public static class CoffeeAwayHelper
    {
        public static async UniTask<LevelData> LoadLevel(int level,
            Action<LevelData> onSuccess = null,
            Action onFailure = null)
        {
            string path = LevelData.GetLevelAssetPath(level);
            if (path == null)
            {
                onFailure?.Invoke();
                Log.Error($"关卡文件不存在: Assets/Game/level/Level-{level}.json");
                return null;
            }

            var textAssets = await Framework.GameRoot.Res.LoadAsset<TextAsset>(path, new CancellationTokenSource().Token);
            var data = LevelData.FromJson(textAssets.text);
            onSuccess?.Invoke(data);
            return data;
        }

        #region BuildScene

        public static List<Holder> SpawnHolders(List<LevelBuilder.HolderInfo> holderInfos, RoadNavigator roadNav,
            Transform parent)
        {
            var holders = new List<Holder>();
            var exits = new List<(Vector3 frontCenter, Vector3 exitDir)>();

            foreach (var info in holderInfos)
            {
                var ctrl = SpawnFactory.SpawnHolderByType(info, parent);
                if (ctrl is null) return null;

                holders.Add(ctrl);
                exits.Add((ctrl.FrontFaceCenter, ctrl.ExitDirection));
            }

            roadNav.ProbeAssign(holders, exits);
            return holders;
        }

        // 创建 Vehicle
        public static List<Vehicle> SpawnVehicles(List<LevelBuilder.HolderInfo> holderInfos, List<Holder> holders)
        {
            var allVehicles = new List<Vehicle>();
            for (int h = 0; h < holderInfos.Count; h++)
            {
                var holderCtrl = holders[h];
                var info = holderInfos[h];

                for (int c = 0; c < info.Cars.Count; c++)
                {
                    var carData = info.Cars[c];

                    int col = carData.IndexInHolder % info.Width;
                    int row = carData.IndexInHolder / info.Width;

                    string carName = $"Coffee_{h}_{c}_C{carData.UnitData?.Id?.Color ?? 0}";
                    Vehicle v = null;
                    switch (carData.UnitData?.GetVehicleType())
                    {
                        case VehicleType.Normal:
                            v = SpawnFactory.SpawnVehicle<NormalCoffee>(carData, info.DirectionValue, carName,
                                holderCtrl.transform);
                            break;
                        case VehicleType.Hidden:
                            v = SpawnFactory.SpawnVehicle<HiddenCoffee>(carData, info.DirectionValue, carName,
                                holderCtrl.transform);
                            break;
                    }

                    if (v is null) return null;
                    holderCtrl.PlaceVehicle(v, col, row, true);
                    allVehicles.Add(v);
                }
            }

            return allVehicles;
        }

        public static ReceiptPrinter SpawnReceiptPrinter(LevelData levelData, Transform parent,
            IReadOnlyList<Vector3> roadCenterline = null, IReadOnlyList<Holder> holders = null,
            IReadOnlyList<Entrance> entrances = null)
        {
            if (levelData?.CapacityTextPosition == null) return null;

            var pos = levelData.CapacityTextPosition;
            var anchorPos = new Vector3(pos.X, 0f, pos.Y) + LevelBuilder.OrigionPos;
            var printer = SpawnFactory.SpawnReceiptPrinter(anchorPos, parent);
            if (printer is null) return null;

            if (ProxyManager.UserProxy.GameLevel == 24)
            {
                var vector3 = printer.transform.localPosition;
                vector3.x = 0;
                printer.transform.localPosition = vector3;
                return printer;
            }

            // 小票机比计数机大很多，配置的 capacityTextPosition 可能与道路/容器/额外槽位/传送门重叠。
            // 以配置点为锚，沿水平面外扩搜索最近的无碰撞位置，避免按关卡逐个硬编码偏移。
            float radius = GetPrinterFootprintRadius(printer);
            printer.transform.position =
                ResolveReceiptPrinterPosition(anchorPos, roadCenterline, holders, entrances, radius);
            return printer;
        }

        // 小票机在 XZ 平面上的足迹半径（渲染网格 + 碰撞体都算，取最大水平半长）
        private static float GetPrinterFootprintRadius(ReceiptPrinter printer)
        {
            float radius = 1.5f;
            var renderers = printer.GetComponentsInChildren<Renderer>(true);
            foreach (var t in renderers)
            {
                if (t is null) continue;
                radius = Mathf.Max(radius, Mathf.Max(t.bounds.extents.x, t.bounds.extents.z) + 0.4f);
            }

            var colliders = printer.GetComponentsInChildren<Collider>(true);
            foreach (var c in colliders)
            {
                if (c is null) continue;
                radius = Mathf.Max(radius, Mathf.Max(c.bounds.extents.x, c.bounds.extents.z) + 0.4f);
            }

            return radius;
        }

        /// <summary>
        /// 以锚点为原点向外螺旋搜索最优位置，分数越低越优：
        /// 1. 路上惩罚（10000 + 越深×1000）压过一切其它项，保证任何不上路的点都优于任何上路的点
        /// 2. 避开容器/传送门（重叠深度×100）
        /// 3. 距离锚点越近越好（基础项）
        /// 即使整圈没有完全干净的点也返回"偏离最小"的点，绝不退回锚点（锚点可能就在路上）。
        /// </summary>
        private static Vector3 ResolveReceiptPrinterPosition(Vector3 anchorPos,
            IReadOnlyList<Vector3> roadCenterline, IReadOnlyList<Holder> holders,
            IReadOnlyList<Entrance> entrances, float radius)
        {
            if (!OverlapsAny(anchorPos, roadCenterline, holders, entrances, radius)) return anchorPos;

            const int maxRing = 14; // 最多外扩 14 格（世界单位）
            Vector3 best = anchorPos;
            float bestScore = float.MaxValue;

            for (int ring = 1; ring <= maxRing; ring++)
            {
                for (int i = -ring; i <= ring; i++)
                {
                    TryCandidate(anchorPos + new Vector3(i, 0f, ring));
                    TryCandidate(anchorPos + new Vector3(i, 0f, -ring));
                    TryCandidate(anchorPos + new Vector3(ring, 0f, i));
                    TryCandidate(anchorPos + new Vector3(-ring, 0f, i));
                }
            }

            return best;

            void TryCandidate(Vector3 pos)
            {
                float score = ScoreCandidate(pos);
                if (score < bestScore)
                {
                    bestScore = score;
                    best = pos;
                }
            }

            float ScoreCandidate(Vector3 pos)
            {
                float score = (pos - anchorPos).magnitude;

                float roadDist = DistanceToRoad(pos, roadCenterline);
                const float roadHalfWidth = 1f; // RoadMeshBuilder.DefaultRoadWidth / 2
                if (roadDist < roadHalfWidth + radius)
                {
                    score += 10000f + (roadHalfWidth + radius - roadDist) * 1000f;
                }

                if (holders != null)
                {
                    foreach (var holder in holders)
                    {
                        if (holder is not null) score += OverlapDepth(pos, holder, radius) * 100f;
                    }
                }

                if (entrances != null)
                {
                    foreach (var entrance in entrances)
                    {
                        if (entrance is not null) score += OverlapDepth(pos, entrance, radius) * 100f;
                    }
                }

                return score;
            }
        }

        // 锚点或候选点是否与道路/容器/传送门重叠
        private static bool OverlapsAny(Vector3 pos, IReadOnlyList<Vector3> roadCenterline,
            IReadOnlyList<Holder> holders, IReadOnlyList<Entrance> entrances, float radius)
        {
            const float roadHalfWidth = 1f; // RoadMeshBuilder.DefaultRoadWidth / 2
            if (roadCenterline != null && DistanceToRoad(pos, roadCenterline) < roadHalfWidth + radius) return true;

            // 容器 + 额外槽位：未激活的 ExtraHolder 也要让开（它稍后会出现）
            if (holders != null)
            {
                foreach (var holder in holders)
                {
                    if (holder is not null && OverlapDepth(pos, holder, radius) > 0f) return true;
                }
            }

            if (entrances != null)
            {
                foreach (var entrance in entrances)
                {
                    if (entrance is not null && OverlapDepth(pos, entrance, radius) > 0f) return true;
                }
            }

            return false;
        }

        // 点与 root 任一渲染网格/碰撞体水平包围盒的重叠深度，无重叠返回 0（含未激活物体）
        private static float OverlapDepth(Vector3 pos, Component root, float radius)
        {
            float depth = 0f;
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            foreach (var t in renderers)
            {
                if (t is null) continue;
                depth = Mathf.Max(depth, BoundsOverlapDepth(pos, t.bounds, radius));
            }

            var colliders = root.GetComponentsInChildren<Collider>(true);
            foreach (var c in colliders)
            {
                if (c is null) continue;
                depth = Mathf.Max(depth, BoundsOverlapDepth(pos, c.bounds, radius));
            }

            return depth;
        }

        private static float BoundsOverlapDepth(Vector3 pos, Bounds bounds, float radius)
        {
            float depthX = bounds.extents.x + radius - Mathf.Abs(pos.x - bounds.center.x);
            float depthZ = bounds.extents.z + radius - Mathf.Abs(pos.z - bounds.center.z);
            if (depthX <= 0f || depthZ <= 0f) return 0f;
            return Mathf.Min(depthX, depthZ);
        }

        /// <summary>点到道路中心线折线（RoadMeshBuilder 采样结果，与渲染道路一致）在 XZ 平面的最近距离。</summary>
        private static float DistanceToRoad(Vector3 pos, IReadOnlyList<Vector3> roadCenterline)
        {
            if (roadCenterline == null || roadCenterline.Count < 2) return float.MaxValue;

            float minSq = float.MaxValue;
            for (int i = 1; i < roadCenterline.Count; i++)
            {
                float dSq = SqrDistPointSegment(pos, roadCenterline[i - 1], roadCenterline[i]);
                if (dSq < minSq) minSq = dSq;
            }

            return Mathf.Sqrt(minSq);
        }

        private static float SqrDistPointSegment(Vector3 point, Vector3 a, Vector3 b)
        {
            Vector2 pa = new Vector2(point.x - a.x, point.z - a.z);
            Vector2 ba = new Vector2(b.x - a.x, b.z - a.z);
            float lenSq = ba.sqrMagnitude;
            if (lenSq <= 1e-6f) return pa.sqrMagnitude;

            float h = Mathf.Clamp(Vector2.Dot(pa, ba) / lenSq, 0f, 1f);
            Vector2 projected = pa - ba * h;
            return projected.sqrMagnitude;
        }

        // 传送门标记：开环道路在中心线首尾各放一个，车从终点端进入、起点端出来；闭环传送带不需要
        public static List<Entrance> SpawnEntrances(LevelData levelData, RoadNavigator roadNav, Transform parent)
        {
            var entrances = new List<Entrance>();
            if (roadNav == null || levelData.IsCloseConveyor == true) return entrances;

            var endEntrance = SpawnFactory.SpawnEntrance(roadNav.EndPoint, roadNav.EndDirection, parent, false);
            if (endEntrance is not null) entrances.Add(endEntrance);

            var startEntrance = SpawnFactory.SpawnEntrance(roadNav.StartPoint, roadNav.StartDirection, parent, true);
            if (startEntrance is not null) entrances.Add(startEntrance);

            return entrances;
        }

        // 组装运行时上下文：注入车辆/容器/道路到核心并刷新渲染层
        public static void SetupCore(CoffeeAwayCore core, List<Holder> holders, List<Vehicle> vehicles,
            List<RoadSurfaceMove> roadSurfaces, ReceiptPrinter receiptPrinter, RoadNavigator roadNav,
            int capacity, float carRoadSpeed, bool isCloseConveyor)
        {
            core.Setup(holders, vehicles, roadSurfaces, receiptPrinter, roadNav, capacity, carRoadSpeed,
                isCloseConveyor);
            core.RefreshAllRenderLayers();
        }

        #endregion

        #region SetFinger

        public static void SetFinger(bool isShow, Holder holder)
        {
            // 优先走 UIGameForm 自带的 2D 手指；预制体未绑定 finger 时回退到项目通用 FingerGuide（3D 手指）
            UIGameForm form = Framework.UIMgr.Instance.GetUIForm<UIGameForm>(UIFormType.UIGameForm);
            var screenPos = Camera.main.WorldToScreenPoint(holder.transform.position);
            form.SetFingerShow(isShow, screenPos);
        }

        #endregion
    }
}