using System;
using System.Collections.Generic;
using UnityEngine;

namespace CasualGame.CoffeeAway
{
    /// <summary>
    /// 根据 LevelData 生成容器和车子的位置/尺寸数据。HolderData.x/y 是容器中心点的网格坐标，容器以中心为基准向两侧均匀铺满车辆
    /// </summary>
    public static class LevelBuilder
    {
        #region const

        private const float GridUnit = 1f; // CELL_WORLD_SIZE，单格 = 1 世界单位
        public const float CarWidthSpacing = 2.1f; // HOLDER_CELL_WIDTH_OFFSET，垂直朝向的车间距（比车宽略大，避免并排重叠）
        public const float CarDepthSpacing = 2.75f; // HOLDER_CELL_HEIGHT_OFFSET，沿朝向的车间距
        public const float HalfCarDepthSpacing = CarDepthSpacing / 2f;
        public const float HalfCarWidthSpacing = CarWidthSpacing / 2f;

        // 标准容器深度（行数）：Standard/Extra 容器共享同一前脸(+6.15)，
        // 车块按"前对齐"排布，最前排对齐到标准 2×4 槽位的前排，矮容器（2×2 ExtraHolder）的车才会停在槽位前两行
        private const int ReferenceHolderDepth = 4;
        public static readonly Vector3 OrigionPos = new(100, 100, 100);

        #endregion

        #region Path

        // 单条路径关键点
        public struct PathNode
        {
            public int PathIndex;
            public int GridX, GridY;
            public Vector3 WorldPos; // 格子中心世界坐标（已加 OrigionPos）
        }

        // 相邻两个 key node 之间的直线路段
        public struct PathSegment
        {
            public int FromIndex, ToIndex;
            public Vector3 From, To;
        }

        // 路径转弯处的贝塞尔弧线数据
        public struct PathTurnArc
        {
            public int AtNodeIndex;
            public Vector3 P0; // 进入边中点
            public Vector3 P1; // 外侧角点
            public Vector3 P2; // 离开边中点
        }

        public struct PathInfo
        {
            public List<PathNode> Nodes;
            public List<PathSegment> StraightSegments;
            public List<PathTurnArc> Turns;
            public bool? IsCloseConveyor; // 关卡数据标记：传送带是否闭合
        }

        // 从 LevelData 构建完整的 PathInfo
        public static PathInfo BuildPathInfo(LevelData levelData)
        {
            var info = new PathInfo
            {
                Nodes = new List<PathNode>(),
                StraightSegments = new List<PathSegment>(),
                Turns = new List<PathTurnArc>(),
                IsCloseConveyor = levelData.IsCloseConveyor
            };

            if (levelData?.GridData == null) return info;

            // 1. 收集所有道路格子，按 PathIndex 排序
            // pathIndex 从 1 开始（编辑器写入约定）。个别关卡首格缺 pathIndex 字段，反序列化为 0，
            // 若用 > 0 过滤会把道路起点整个丢掉，导致首段路被截断、容器出口点错位，这里必须 >= 0。
            var nodeCells = new List<GridCellData>();
            foreach (var c in levelData.GridData)
                if (c.IsRoad && c.PathIndex >= 0)
                    nodeCells.Add(c);
            nodeCells.Sort((a, b) => a.PathIndex.CompareTo(b.PathIndex));

            if (nodeCells.Count < 2) return info;

            // 2. 构建 PathNode 列表
            foreach (var c in nodeCells)
            {
                // 编辑器微调过单点（pX/pY/pZ）时用自定义坐标，否则回退到格子中心
                float cx = c.PX ?? (c.X + 0.5f) * GridUnit;
                float cz = c.PZ ?? (c.Y + 0.5f) * GridUnit;
                float cy = c.PY ?? 0f;
                info.Nodes.Add(new PathNode
                {
                    PathIndex = c.PathIndex,
                    GridX = c.X,
                    GridY = c.Y,
                    WorldPos = new Vector3(cx, cy, cz) + OrigionPos
                });
            }

            // 3. 遍历相邻三个节点，识别转弯，生成直线段
            for (int i = 0; i < info.Nodes.Count - 1; i++)
            {
                var cur = info.Nodes[i];
                var next = info.Nodes[i + 1];

                int dx1 = next.GridX - cur.GridX;
                int dy1 = next.GridY - cur.GridY;

                info.StraightSegments.Add(new PathSegment
                {
                    FromIndex = cur.PathIndex,
                    ToIndex = next.PathIndex,
                    From = cur.WorldPos,
                    To = next.WorldPos
                });

                // 检查是否在 next 处转弯（需要 i+2 存在）
                if (i + 2 < info.Nodes.Count)
                {
                    var after = info.Nodes[i + 2];
                    int dx2 = after.GridX - next.GridX;
                    int dy2 = after.GridY - next.GridY;

                    // 方向变化即为转弯
                    bool isTurn = (dx1 != dx2) || (dy1 != dy2);
                    if (isTurn)
                    {
                        info.Turns.Add(BuildTurnArc(next, dx1, dy1, dx2, dy2));
                    }
                }
            }

            return info;
        }

        /// <summary>
        /// 为转弯节点构建贝塞尔弧线，保证入口/出口切线连续。
        /// </summary>
        private static PathTurnArc BuildTurnArc(PathNode node, int dxIn, int dyIn, int dxOut, int dyOut)
        {
            int sdxIn = Math.Sign(dxIn);
            int sdyIn = Math.Sign(dyIn);
            int sdxOut = Math.Sign(dxOut);
            int sdyOut = Math.Sign(dyOut);

            // P0/P2 从格子边缘再往外延伸 spread，给弯道更大的曲率半径
            float spread = 0.5f;
            float ext = GridUnit / 2f + spread; // 0.5 + 0.5 = 1.0
            float yOff = node.WorldPos.y + 0.025f;

            float inX = node.WorldPos.x - sdxIn * ext;
            float inZ = node.WorldPos.z - sdyIn * ext;
            float outX = node.WorldPos.x + sdxOut * ext;
            float outZ = node.WorldPos.z + sdyOut * ext;

            // P1 = 格子中心，保证切线连续
            float p1X = node.WorldPos.x;
            float p1Z = node.WorldPos.z;

            return new PathTurnArc
            {
                AtNodeIndex = node.PathIndex,
                P0 = new Vector3(inX, yOff, inZ),
                P1 = new Vector3(p1X, yOff, p1Z),
                P2 = new Vector3(outX, yOff, outZ)
            };
        }

        #endregion

        #region Holder && Car

        public enum HolderDirection
        {
            Left = 0,
            Right = 1,
            Up = 2,
            Down = 3
        }

        // 容器生成数据
        public struct HolderInfo
        {
            // 容器中心点世界坐标
            public Vector3 AnchorWorldPos;

            public int Width; // 垂直朝向方向的车辆数
            public int Height; // 沿朝向方向（深度）的车辆数
            public HolderDirection? Direction;
            public float? DirectionValue;

            // 容器在 XZ 平面的世界尺寸
            public Vector3 WorldSize => Direction is HolderDirection.Right or HolderDirection.Left
                ? new Vector3(Height * CarDepthSpacing, 0, Width * CarWidthSpacing)
                : new Vector3(Width * CarWidthSpacing, 0, Height * CarDepthSpacing);

            public List<CarInfo> Cars;
            public HolderPrefabType HolderPrefabType; // 基础外壳类型：决定 spawn 哪个 Holder 子类
            public List<GameUnitStateData> HolderStates; // 特殊玩法 states：决定挂载哪些 HolderSpecialCtrl
        }

        // 单个车子的生成数据
        public struct CarInfo
        {
            public Vector3 WorldPos;
            public int IndexInHolder;
            public GameUnitData UnitData;
        }

        public static List<HolderInfo> Build(LevelData levelData)
        {
            var holders = new List<HolderInfo>();
            if (levelData?.HolderDataList == null) return holders;

            // 把关卡用到的相近颜色统一重映射为差异明显的色板色，只改颜色字段
            RemapLevelColors(levelData);

            foreach (var hd in levelData.HolderDataList)
            {
                var info = BuildHolder(hd);
                holders.Add(info);
            }

            return holders;
        }

        // 构建单个容器的布局数据
        private static HolderInfo BuildHolder(HolderData hd)
        {
            // hd.X / hd.Y = 容器中心点的网格坐标，乘 GridUnit 转世界坐标
            float cx = hd.X * GridUnit;
            float cz = hd.Y * GridUnit;
            Vector3 center = new Vector3(cx, 0, cz);

            HolderDirection dir = hd.Direction.HasValue ? (HolderDirection)hd.Direction.Value : HolderDirection.Left;
            List<CarInfo> cars = new List<CarInfo>();
            // HolderType=0（或未定义值）按 Standard 处理，与 SpawnFactory 的默认分支一致
            var prefabType = (HolderPrefabType)(hd.PrefabType ?? 0);

            // 带 Colored 特殊玩法的容器统一按颜色容器外壳处理（由 ColorHolderPool 生成专用预制体）
            if (hd.States is { Count: > 0 })
            {
                foreach (var state in hd.States)
                {
                    if ((HolderSpecialType)state.Type == HolderSpecialType.Colored && state.ColorType != null)
                    {
                        prefabType = HolderPrefabType.Colored;
                        break;
                    }
                }
            }

            if (hd.GameUnits is { Count: > 0 })
            {
                bool frontAligned = IsFrontAlignedHolder(prefabType);
                for (int i = 0; i < hd.GameUnits.Count; i++)
                {
                    Vector3 carWorld = CalcCarWorldPos(i, cx, cz, hd.Width, hd.Height, dir, frontAligned);
                    cars.Add(new CarInfo
                    {
                        WorldPos = carWorld,
                        IndexInHolder = i,
                        UnitData = hd.GameUnits[i]
                    });
                }
            }

            return new HolderInfo
            {
                AnchorWorldPos = center + OrigionPos,
                Width = hd.Width,
                Height = hd.Height,
                Direction = dir,
                DirectionValue = hd.DirectionValue,
                HolderPrefabType = prefabType,
                Cars = cars,
                HolderStates = hd.States
            };
        }

        /// <summary>
        /// 根据车辆在容器内的 index 计算世界坐标
        /// depthIdx=0 = 离朝向最远的列（最后排），depthIdx=height-1 = 最前排，
        /// perpIdx=0 = 上（左/右朝向）或 左（上/下朝向）
        /// frontAligned=true：车块最前排对齐到标准深度(ReferenceHolderDepth)的最前排，
        /// 对标准 2×4 容器与原来居中等价，对矮容器则把车推进到槽位前排；
        /// frontAligned=false：车块以容器中心为基准居中排布（Expandable/Tunnel 等长容器）。
        /// </summary>
        public static Vector3 CalcCarWorldPos(int index, float cx, float cz, int width, int height,
            HolderDirection dir, bool frontAligned = false)
        {
            int depthIdx = index / width;
            int perpIdx = index % width;
            float offsetIdx = index % 2 == 0 ? -0.05f : 0.05f;

            float halfPerp = width * CarWidthSpacing / 2f;
            float halfDepth = height * CarDepthSpacing / 2f;
            float frontDepthOffset = ReferenceHolderDepth * CarDepthSpacing / 2f - HalfCarDepthSpacing;

            // 沿容器朝向方向（正 = 朝前/路侧）的偏移
            float depthOffset = frontAligned
                ? frontDepthOffset - (height - 1 - depthIdx) * CarDepthSpacing
                : -halfDepth + depthIdx * CarDepthSpacing + HalfCarDepthSpacing;

            float wx, wz;

            switch (dir)
            {
                case HolderDirection.Right:
                    wx = cx + depthOffset;
                    wz = cz + halfPerp - perpIdx * CarWidthSpacing - HalfCarWidthSpacing + offsetIdx;
                    break;

                case HolderDirection.Left:
                    wx = cx - depthOffset;
                    wz = cz + halfPerp - perpIdx * CarWidthSpacing - HalfCarWidthSpacing + offsetIdx;
                    break;

                case HolderDirection.Up:
                    wx = cx - halfPerp + perpIdx * CarWidthSpacing + HalfCarWidthSpacing + offsetIdx;
                    wz = cz + depthOffset;
                    break;

                case HolderDirection.Down:
                    wx = cx - halfPerp + perpIdx * CarWidthSpacing + HalfCarWidthSpacing + offsetIdx;
                    wz = cz - depthOffset;
                    break;

                default:
                    wx = cx - depthOffset;
                    wz = cz + halfPerp - perpIdx * CarWidthSpacing - HalfCarWidthSpacing;
                    break;
            }

            return new Vector3(wx, 0, wz) + OrigionPos;
        }

        // Standard/Extra 共享标准前脸(+6.15)，车块需前对齐到标准 2×4 前排；Expandable/Tunnel 等前脸不同，保持居中
        public static bool IsFrontAlignedHolder(HolderPrefabType type) =>
            type is HolderPrefabType.Standard or HolderPrefabType.Extra or HolderPrefabType.Colored;

        #endregion

        #region Color Remap

        // 全部可用色板 = 16 个杯子色。排除 Black：VehiclePool 的 index 0 是问号杯(HiddenCoffee)专用槽位，
        // 不是黑色普通杯，NormalCar 用 Black(0) 会 spawn 失败。原关卡若出现 Black(null) 也会被映射到色板色。
        private static readonly ColorType[] s_ColorPalette =
        {
            ColorType.Blue, ColorType.Brown, ColorType.Cyan, ColorType.Green, ColorType.DarkGreen,
            ColorType.Orange, ColorType.Pink, ColorType.Purple, ColorType.Red, ColorType.Magenta,
            ColorType.Lilac, ColorType.Yellow, ColorType.Cherry, ColorType.CaramelBrown,
            ColorType.DarkBlue, ColorType.SkyBlue
        };

        /// <summary>
        /// 统计关卡实际用到的所有颜色（杯子基础色 + 特殊玩法目标色），
        /// 再从色板用最远点采样挑同样数量、彼此 RGB 距离最大的不相近颜色，按枚举序一一对应做重映射。
        /// 只改颜色字段（Id.Color / 各 state 的 ColorType），不影响位置/类型/玩法等任何其它数据；
        /// 同一原色在杯子与特殊玩法（颜色托盘、颜色盖子）中映射到同一个新色，保证门禁逻辑一致。
        /// </summary>
        private static void RemapLevelColors(LevelData levelData)
        {
            if (levelData?.HolderDataList == null || levelData.HolderDataList.Count == 0) return;

            // 1. 收集关卡用到的所有颜色，null 视为 Black(0)（与 SpawnFactory 的 ?? 0 默认一致）
            var used = new SortedSet<int>();
            foreach (var hd in levelData.HolderDataList)
            {
                if (hd?.GameUnits != null)
                {
                    foreach (var g in hd.GameUnits)
                    {
                        if (g is null) continue;
                        used.Add(g.Id?.Color ?? 0);
                        CollectStateColors(g.States, used);
                    }
                }

                CollectStateColors(hd?.States, used);
            }

            int count = used.Count;
            if (count == 0 || count > s_ColorPalette.Length) return; // 超出色板数量时无法保证不相近，保持原样

            // 2. 挑 count 个互不相近的颜色
            var selected = PickDistinctColors(count);

            // 3. 原色与新色都按枚举序升序一一对应，保证同一个原色永远映射到同一个新色（确定性）
            var originals = new int[count];
            int idx = 0;
            foreach (var c in used) originals[idx++] = c;

            var map = new Dictionary<int, int>(count);
            for (int i = 0; i < count; i++) map[originals[i]] = (int)selected[i];

            // 4. 应用到所有杯子基础色与特殊玩法目标色
            foreach (var hd in levelData.HolderDataList)
            {
                if (hd?.GameUnits != null)
                {
                    foreach (var g in hd.GameUnits)
                    {
                        if (g?.Id is null) continue;
                        if (map.TryGetValue(g.Id.Color ?? 0, out int newColor)) g.Id.Color = newColor;
                        RemapStateColors(g.States, map);
                    }
                }

                RemapStateColors(hd?.States, map);
            }
        }

        private static void CollectStateColors(List<GameUnitStateData> states, SortedSet<int> used)
        {
            if (states == null) return;
            foreach (var s in states)
                if (s?.ColorType is int c) used.Add(c);
        }

        private static void RemapStateColors(List<GameUnitStateData> states, Dictionary<int, int> map)
        {
            if (states == null) return;
            foreach (var s in states)
                if (s?.ColorType is int c && map.TryGetValue(c, out int newColor)) s.ColorType = newColor;
        }

        // 最远点采样：每次选到已选集合最小距离最大的颜色，让挑出的颜色在 RGB 空间尽量散开；结果确定性，同 N 恒同结果
        private static ColorType[] PickDistinctColors(int count)
        {
            if (count >= s_ColorPalette.Length) return (ColorType[])s_ColorPalette.Clone();

            var chosen = new List<ColorType>(count);

            // 种子：离色板均值色最远的“角落色”
            Vector3 centroid = Vector3.zero;
            foreach (var c in s_ColorPalette) centroid += ToRgb(c);
            centroid /= s_ColorPalette.Length;

            ColorType seed = s_ColorPalette[0];
            float seedScore = float.MinValue;
            foreach (var c in s_ColorPalette)
            {
                float d = RgbDistance(ToRgb(c), centroid);
                if (d > seedScore)
                {
                    seedScore = d;
                    seed = c;
                }
            }

            chosen.Add(seed);

            while (chosen.Count < count)
            {
                ColorType best = chosen[0];
                float bestMin = float.MinValue;
                foreach (var c in s_ColorPalette)
                {
                    if (chosen.Contains(c)) continue;
                    float minDist = float.MaxValue;
                    Vector3 cRgb = ToRgb(c);
                    foreach (var k in chosen) minDist = Mathf.Min(minDist, RgbDistance(cRgb, ToRgb(k)));
                    if (minDist > bestMin)
                    {
                        bestMin = minDist;
                        best = c;
                    }
                }

                chosen.Add(best);
            }

            return chosen.ToArray();
        }

        private static Vector3 ToRgb(ColorType color)
        {
            Color c = SpawnFactory.GetColor(color);
            return new Vector3(c.r, c.g, c.b);
        }

        // 人眼对绿色更敏感，加权使感知距离更接近真实
        private static float RgbDistance(Vector3 a, Vector3 b)
        {
            float dr = a.x - b.x;
            float dg = a.y - b.y;
            float db = a.z - b.z;
            return Mathf.Sqrt(2f * dr * dr + 4f * dg * dg + 3f * db * db);
        }

        #endregion
    }
}