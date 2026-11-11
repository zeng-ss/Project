using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace CasualGame.CoffeeAway
{
    /// <summary>
    /// 容器网格数据与纯逻辑：车辆存取、前排匹配、进出弹出、道具算法（打乱/压实/消除）、通关判定。
    /// </summary>
    public class HolderGrid
    {
        #region Data & Helpers

        // 同色连通块：Cars 为块内车辆，Offsets 为相对块左上角的偏移
        private class BlockShape
        {
            public List<(Vehicle v, int c, int r)> Cars = new();
            public List<(int dc, int dr)> Offsets = new();
        }

        private static readonly (int, int)[] s_Dirs = { (1, 0), (-1, 0), (0, 1), (0, -1) };

        #endregion

        #region Properties & Constructor

        public LevelBuilder.HolderInfo Info { get; }
        public Vehicle[,] Cells { get; }
        public int Width => Info.Width;
        public int Height => Info.Height;

        public HolderGrid(LevelBuilder.HolderInfo info)
        {
            Info = info;
            Cells = new Vehicle[info.Width, info.Height];
        }

        #endregion

        #region Vehicle Placement

        // 加入 Vehicle
        public void PlaceVehicle(Vehicle v, int col, int row, bool isInit = false)
        {
            Cells[col, row] = v;
            v.HolderColIndex = col;
            v.HolderRowIndex = row;
            if (!isInit) SetFrontOutLine();
        }

        public bool ContainsVehicle(Vehicle v)
        {
            if (v is null) return false;
            for (int r = 0; r < Height; r++)
            {
                for (int c = 0; c < Width; c++)
                {
                    if (Cells[c, r] == v) return true;
                }
            }

            return false;
        }

        // 找到第一个空位放入车辆并返回其世界坐标；无空位返回 Vector3.zero
        public Vector3 PlaceToFirstEmpty(Vehicle vehicle)
        {
            for (int r = 0; r < Height; r++)
            {
                for (int c = 0; c < Width; c++)
                {
                    if (Cells[c, r] is null)
                    {
                        PlaceVehicle(vehicle, c, r);
                        return GetCellWorldPos(c, r);
                    }
                }
            }

            return Vector3.zero;
        }

        #endregion

        #region Hidden Vehicle Display

        // 有车出去后：从出口往后排扫前排同色块，？车揭开为 Normal
        public void RefreshHiddenVehicleDisplay()
        {
            ColorType? firstColor = null;
            for (int r = Height - 1; r >= 0; r--)
            {
                ColorType? rowColor = GetRowColor(r, out _);
                if (rowColor == null) continue;
                firstColor ??= rowColor;
                if (rowColor != firstColor) break;

                for (int c = 0; c < Width; c++)
                {
                    Vehicle v = Cells[c, r];
                    v?.SetVehicleType(VehicleType.Normal);
                }
            }

            SetFrontOutLine();
        }

        #endregion

        #region Grid Query

        // 获取网格指定格子的世界坐标
        public Vector3 GetCellWorldPos(int col, int row)
        {
            float cx = Info.AnchorWorldPos.x - LevelBuilder.OrigionPos.x;
            float cz = Info.AnchorWorldPos.z - LevelBuilder.OrigionPos.z;
            var dir = Info.Direction ?? LevelBuilder.HolderDirection.Left;
            return LevelBuilder.CalcCarWorldPos(row * Width + col, cx, cz, Width, Height, dir,
                LevelBuilder.IsFrontAlignedHolder(Info.HolderPrefabType));
        }

        // 空位数量
        public int EmptySlotCount()
        {
            int count = 0;
            for (int c = 0; c < Width; c++)
            for (int r = 0; r < Height; r++)
                if (Cells[c, r] is null)
                    count++;
            return count;
        }

        private bool HasEmptySlot()
        {
            for (int c = 0; c < Width; c++)
            for (int r = 0; r < Height; r++)
                if (Cells[c, r] is null)
                    return true;
            return false;
        }

        // 容器内是否包含指定颜色的车辆
        public bool ContainsColor(ColorType color)
        {
            for (int c = 0; c < Width; c++)
            for (int r = 0; r < Height; r++)
                if (Cells[c, r] is not null && Cells[c, r].ColorType == color)
                    return true;
            return false;
        }

        public bool IsEmpty()
        {
            for (int c = 0; c < Width; c++)
            for (int r = 0; r < Height; r++)
                if (Cells[c, r] != null)
                    return false;
            return true;
        }

        #endregion

        #region Front Match

        // 前排是否可接受该颜色
        public bool CanAcceptAtFront(ColorType colorType)
        {
            if (!HasEmptySlot()) return false;
            var front = GetFrontState();
            return (front.Item2 == null || front.Item2.Value == colorType) && !HasNeedMoveVehicle();
        }

        private ColorType? GetRowColor(int row, out Vehicle vehicle)
        {
            for (int c = Width - 1; c >= 0; c--)
            {
                if (Cells[c, row])
                {
                    vehicle = Cells[c, row];
                    return Cells[c, row].ColorType;
                }
            }

            vehicle = null;
            return null;
        }

        // 获取前排的行和颜色
        private (int?, ColorType?) GetFrontState()
        {
            for (int r = Height - 1; r >= 0; r--)
            {
                ColorType? color = GetRowColor(r, out _);
                if (color != null) return (r, color);
            }

            return (null, null);
        }

        // 获取前排相同颜色的车数量
        public int GetFrontSameColorVehicleCount()
        {
            ColorType? firstColor = null;
            int count = 0;
            for (int r = Height - 1; r >= 0; r--)
            {
                ColorType? rowColor = GetRowColor(r, out _);
                if (rowColor == null) continue;
                firstColor ??= rowColor;
                if (rowColor != firstColor) break;
                count++;
            }

            return count;
        }

        // 前排颜色 空容器返回 null
        public ColorType? GetFrontColor() => GetFrontState().Item2;

        // 设置容器的边缘显示：只给前排连续同色段加边缘光，其余一律关掉
        public void SetFrontOutLine()
        {
            // 已填满同色（完成）的容器不再高亮/呼吸：前排车不可点击，避免误导；SetFront(false) 也会停掉正在播的呼吸
            bool completed = IsFullSameColor();
            bool inFront = true;
            ColorType? frontColor = null;
            for (int r = Height - 1; r >= 0; r--)
            {
                if (inFront)
                {
                    ColorType? rowColor = GetRowColor(r, out _);
                    if (rowColor == null) continue;
                    frontColor ??= rowColor;
                    if (rowColor != frontColor) inFront = false;
                }

                bool rowFront = inFront && !completed;
                for (int c = 0; c < Width; c++)
                {
                    Vehicle v = Cells[c, r];
                    if (v is null) continue;
                    // 与 PeekExitVehicleCells 一致：问号车及其右侧不再算前排
                    if (rowFront && v.VehicleType == VehicleType.Hidden) rowFront = false;
                    v.SetFront(rowFront);
                }
            }
        }

        // 预览即将弹出的车辆及其格子
        public List<(Vehicle vehicle, int col, int row)> PeekExitVehicleCells()
        {
            var result = new List<(Vehicle, int, int)>();
            var state = GetFrontState();
            if (state == (null, null) || !state.Item1.HasValue || !state.Item2.HasValue)
                return result;

            for (int r = state.Item1.Value; r >= 0; r--)
            {
                ColorType? rowColor = GetRowColor(r, out _);
                if (rowColor == null) continue;
                if (rowColor != state.Item2.Value) break;
                for (int c = 0; c < Width; c++)
                {
                    Vehicle v = Cells[c, r];
                    if (v is not null)
                    {
                        // 遇到问好车立马跳过
                        if (v.VehicleType == VehicleType.Hidden) break;
                        result.Add((v, c, r));
                    }
                }
            }

            return result;
        }

        public Vehicle GetVehicleAt(int col, int row) => Cells[col, row];

        // 从 Cell 移除 Vehicle
        public void RemoveVehicle(Vehicle v)
        {
            if (v is null) return;
            v.SetFront(false); // 离开容器后不再显示前排边缘光
            if (v.HolderColIndex >= 0 && v.HolderColIndex < Width &&
                v.HolderRowIndex >= 0 && v.HolderRowIndex < Height && Cells[v.HolderColIndex, v.HolderRowIndex] == v)
                Cells[v.HolderColIndex, v.HolderRowIndex] = null;
            SetFrontOutLine();
        }

        #endregion

        #region Prop Algorithm

        // 随机打乱容器内车辆：同色连通块作为整体移动不拆散
        // preferredColors：优先把其他容器可接受的颜色交换到前排，保证打乱后有解；无任何可接受颜色时退化为纯随机
        public List<Vehicle> Shuffle(Random rng, out string hint, HashSet<ColorType> preferredColors = null)
        {
            hint = null;
            int w = Width, h = Height;

            // 1. 检测同色连通块
            var visited = new bool[w][];
            for (int index = 0; index < w; index++)
            {
                visited[index] = new bool[h];
            }

            var blocks = new List<BlockShape>();
            for (int c = 0; c < w; c++)
            {
                for (int r = 0; r < h; r++)
                {
                    if (Cells[c, r] is null || visited[c][r]) continue;
                    var color = Cells[c, r].ColorType;
                    var cells = new List<(Vehicle v, int c, int r)>();
                    var bfs = new Queue<(int c, int r)>();
                    bfs.Enqueue((c, r));
                    visited[c][r] = true;

                    while (bfs.Count > 0)
                    {
                        var (bc, br) = bfs.Dequeue();
                        cells.Add((Cells[bc, br], bc, br));
                        foreach (var (dc, dr) in s_Dirs)
                        {
                            int nc = bc + dc, nr = br + dr;
                            if (nc < 0 || nr < 0 || nc >= w || nr >= h) continue;
                            if (visited[nc][nr] || Cells[nc, nr] == null) continue;
                            if (Cells[nc, nr].ColorType != color) continue;
                            visited[nc][nr] = true;
                            bfs.Enqueue((nc, nr));
                        }
                    }

                    int minC = int.MaxValue, minR = int.MaxValue;
                    foreach (var cell in cells)
                    {
                        minC = Mathf.Min(minC, cell.c);
                        minR = Mathf.Min(minR, cell.r);
                    }

                    var offsets = new List<(int dc, int dr)>();
                    foreach (var cell in cells) offsets.Add((cell.c - minC, cell.r - minR));

                    blocks.Add(new BlockShape { Cars = cells, Offsets = offsets });
                }
            }

            if (blocks.Count <= 1)
            {
                hint = "容器内车辆为单一整体，无法打乱";
                return null;
            }

            // 是否存在可被其他容器接受的颜色块
            bool hasPreferred = preferredColors != null && preferredColors.Count > 0 &&
                                blocks.Any(b => preferredColors.Contains(b.Cars[0].v.ColorType));

            // 2. 多次随机块顺序 + first-fit 重排，直到得到与原始布局不同的结果
            //    有可接受色时：先严格要求前排为可接受色，放宽后再试，确保尽量有解
            int maxPhase = hasPreferred ? 2 : 1;
            for (int phase = 0; phase < maxPhase; phase++)
            {
                bool requirePreferredFront = hasPreferred && phase == 0;
                for (int attempt = 0; attempt < 60; attempt++)
                {
                    var order = BuildBlockOrder(rng, blocks, preferredColors);

                    var grid = new Vehicle[w][]; // 占用标记
                    for (int index = 0; index < w; index++)
                    {
                        grid[index] = new Vehicle[h];
                    }

                    var placements = new List<(Vehicle v, int c, int r)>();
                    bool ok = true;

                    foreach (var block in order)
                    {
                        bool placed = false;
                        for (int ar = 0; ar < h && !placed; ar++)
                        {
                            for (int ac = 0; ac < w && !placed; ac++)
                            {
                                bool fits = true;
                                for (int i = 0; i < block.Offsets.Count; i++)
                                {
                                    int nc = ac + block.Offsets[i].dc;
                                    int nr = ar + block.Offsets[i].dr;
                                    if (nc < 0 || nr < 0 || nc >= w || nr >= h)
                                    {
                                        fits = false;
                                        break;
                                    }

                                    if (grid[nc][nr] is not null)
                                    {
                                        fits = false;
                                        break;
                                    }
                                }

                                if (!fits) continue;

                                for (int i = 0; i < block.Cars.Count; i++)
                                {
                                    int nc = ac + block.Offsets[i].dc;
                                    int nr = ar + block.Offsets[i].dr;
                                    grid[nc][nr] = block.Cars[i].v;
                                    placements.Add((block.Cars[i].v, nc, nr));
                                }

                                placed = true;
                            }
                        }

                        if (!placed)
                        {
                            ok = false;
                            break;
                        }
                    }

                    if (!ok) continue;

                    // 有可接受色时前排必须为可接受色
                    if (requirePreferredFront && !IsFrontColorPreferred(grid, w, h, preferredColors)) continue;

                    // 3. 布局必须与原始不同，否则换一批顺序重试
                    var moved = new List<Vehicle>();
                    bool changed = false;
                    foreach (var (v, nc, nr) in placements)
                    {
                        if (v.HolderColIndex != nc || v.HolderRowIndex != nr)
                        {
                            changed = true;
                            moved.Add(v);
                        }
                    }

                    if (!changed) continue;

                    // 4. 应用新布局
                    for (int c = 0; c < w; c++)
                    for (int r = 0; r < h; r++)
                        Cells[c, r] = null;

                    foreach (var (v, nc, nr) in placements)
                        PlaceVehicle(v, nc, nr);

                    return moved;
                }
            }

            hint = "打乱失败，请重试";
            return null;
        }

        // 生成打乱顺序：有可接受色时把可接受色块排到队尾，让其落在前排
        private static List<BlockShape> BuildBlockOrder(Random rng, List<BlockShape> blocks,
            HashSet<ColorType> preferredColors)
        {
            var order = new List<BlockShape>(blocks);
            if (preferredColors is null || preferredColors.Count == 0)
            {
                ShuffleList(rng, order);
                return order;
            }

            var preferred = order.Where(b => preferredColors.Contains(b.Cars[0].v.ColorType)).ToList();
            var rest = order.Where(b => !preferredColors.Contains(b.Cars[0].v.ColorType)).ToList();
            ShuffleList(rng, preferred);
            ShuffleList(rng, rest);
            rest.AddRange(preferred);
            return rest;
        }

        private static void ShuffleList(Random rng, List<BlockShape> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        // 候选布局的前排（最高非空行最右）是否为可接受色
        private static bool IsFrontColorPreferred(Vehicle[][] grid, int w, int h, HashSet<ColorType> preferred)
        {
            for (int r = h - 1; r >= 0; r--)
            {
                for (int c = w - 1; c >= 0; c--)
                {
                    if (grid[c][r] is null) continue;
                    return preferred.Contains(grid[c][r].ColorType);
                }
            }

            return false;
        }

        // 拿到指定颜色需要移除的车辆并且更新 数组 信息
        public List<Vehicle> GetRemoveCarsAndUpdateGrid(ColorType color)
        {
            var removed = new List<Vehicle>();
            var remaining = new List<Vehicle>();
            for (int r = 0; r < Height; r++)
            for (int c = 0; c < Width; c++)
            {
                var v = Cells[c, r];
                if (v is null) continue;
                if (v.ColorType == color) removed.Add(v);
                else remaining.Add(v);
            }

            if (removed.Count == 0) return removed;

            // 前 N 格放剩余车，其余格子清空，一步完成压实
            for (int i = 0; i < Width * Height; i++)
            {
                int c = i % Width, r = i / Width;
                if (i < remaining.Count) PlaceVehicle(remaining[i], c, r);
                else Cells[c, r] = null;
            }

            return removed;
        }

        // 将剩余的车子移进容器后部；全部落位后回调 onComplete
        public void MoveRemainVehicleToBack(Action onComplete = null)
        {
            int remain = 0;
            for (int r = 0; r < Height; r++)
            for (int c = 0; c < Width; c++)
            {
                var v = Cells[c, r];
                if (v is null) continue;
                remain++;
                var pos = GetCellWorldPos(c, r);
                var rot = Quaternion.Euler(0f, Info.DirectionValue ?? 0f, 0f);
                v.MoveTo(pos, rot, 0.5f, () =>
                {
                    if (--remain <= 0) onComplete?.Invoke();
                });
            }

            if (remain == 0) onComplete?.Invoke();
        }

        // 拿到容器内的所有的颜色类型
        public List<ColorType> GetColorsInHolder()
        {
            var colors = new List<ColorType>();
            for (int r = 0; r < Height; r++)
            for (int c = 0; c < Width; c++)
            {
                if (Cells[c, r] is null) continue;
                if (!colors.Contains(Cells[c, r].ColorType)) colors.Add(Cells[c, r].ColorType);
            }

            return colors;
        }

        #endregion

        #region Completion & State

        // 是否所有格子都填满且同色
        public bool IsFullSameColor()
        {
            ColorType? firstColor = null;
            for (int r = 0; r < Height; r++)
            {
                for (int c = 0; c < Width; c++)
                {
                    if (Cells[c, r] is null) return false;
                    firstColor ??= Cells[c, r].ColorType;
                    if (Cells[c, r].ColorType != firstColor) return false;
                }
            }

            return true;
        }

        // 是否容器内的都是同色
        public (bool, int) IsPlaceSameColor()
        {
            int count = 0;
            ColorType? firstColor = null;
            for (int r = 0; r < Height; r++)
            {
                for (int c = 0; c < Width; c++)
                {
                    if (Cells[c, r] is null) continue;
                    firstColor ??= Cells[c, r].ColorType;
                    if (Cells[c, r].ColorType != firstColor) return (false, 0);
                    count++;
                }
            }

            return (true, count);
        }

        public bool HasHiddenVehicle()
        {
            for (int r = 0; r < Height; r++)
            {
                for (int c = 0; c < Width; c++)
                {
                    if (Cells[c, r]?.VehicleType == VehicleType.Hidden)
                        return true;
                }
            }

            return false;
        }

        // 是否有车辆正处于进出场动画/排队状态
        public bool HasMovingVehicle()
        {
            for (int r = 0; r < Height; r++)
            {
                for (int c = 0; c < Width; c++)
                {
                    if (Cells[c, r] is null) continue;
                    if (Cells[c, r].State is VehicleState.Entering
                        or VehicleState.Exiting
                        or VehicleState.WaitingExit)
                        return true;
                }
            }

            return false;
        }

        // 是否有车辆正处于出场/排队状态
        public bool HasNeedMoveVehicle()
        {
            for (int r = 0; r < Height; r++)
            {
                for (int c = 0; c < Width; c++)
                {
                    if (Cells[c, r] is null) continue;
                    if (Cells[c, r].State is VehicleState.Exiting or VehicleState.WaitingExit) return true;
                }
            }

            return false;
        }

        #endregion
    }
}
