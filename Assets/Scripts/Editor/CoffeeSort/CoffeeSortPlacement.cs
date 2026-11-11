using System.Collections.Generic;
using CasualGame.CoffeeAway;
using UnityEngine;

namespace CasualGame.EditorTools
{
    public enum PlaceKind
    {
        None,
        Container,
    }

    /// <summary>
    /// 放置状态机 + 与运行时一致的网格/车位置数学。
    /// 常量与 LevelBuilder 保持一致；车位置算法复刻 CalcCarWorldPos（前对齐，不含 OrigionPos）。
    /// </summary>
    public static class CoffeeSortPlacement
    {
        public const float CarWidthSpacing = 2.1f;
        public const float CarDepthSpacing = 2.75f;
        public const float HalfCarDepthSpacing = CarDepthSpacing / 2f;
        public const float HalfCarWidthSpacing = CarWidthSpacing / 2f;
        public const int ReferenceHolderDepth = 4;
        public const float FrontDepthOffset =
            ReferenceHolderDepth * CarDepthSpacing / 2f - HalfCarDepthSpacing; // 4.125

        public static PlaceKind Kind { get; private set; } = PlaceKind.None;
        public static int ContainerType { get; private set; } = (int)HolderPrefabType.Standard;
        public static float Rotation { get; private set; }

        // 路段端点设置模式：true 时下次 Cmd+点击 写入当前路段的起点/终点（再点已设置的端点则移除）
        public static bool SettingStart { get; private set; }
        public static bool SettingEnd { get; private set; }
        public static bool IsSettingEndpoint => SettingStart || SettingEnd;

        public static bool IsPlacing => Kind != PlaceKind.None;

        // 删除容器模式：开启后 Scene 视图点击已放置的容器即删除
        public static bool DeletingHolder { get; private set; }
        public static bool IsDeletingHolder => DeletingHolder;

        // 路径点选中状态：Scene 视图左键选中后可用 PositionHandle 或窗口 X/Y/Z 输入框微调
        public static PathPointData SelectedPoint { get; private set; }
        public static int SelectedSegmentIndex { get; private set; } = -1;
        public static bool IsPointSelected => SelectedPoint != null;

        public static void SelectPoint(int segmentIndex, PathPointData point)
        {
            SelectedSegmentIndex = segmentIndex;
            SelectedPoint = point;
        }

        public static void ClearPointSelection()
        {
            SelectedPoint = null;
            SelectedSegmentIndex = -1;
        }

        /// <summary>
        /// 用世界坐标（网格坐标，不含 OrigionPos）更新路径点，按半格吸附到 0.5 网格：
        /// X/Z 落在 .0/.5，Y 按 0.5 阶梯。吸附后反解回 Gx/Gy + Offset，
        /// base = 世界坐标 - 0.5，取整为 G、余数为 Offset。
        /// </summary>
        public static void ApplyWorldPos(PathPointData p, float wx, float wy, float wz)
        {
            float sx = SnapHalf(wx);
            float sy = SnapHalf(wy);
            float sz = SnapHalf(wz);

            p.Y = sy;
            float tx = sx - 0.5f;
            float tz = sz - 0.5f;
            p.Gx = Mathf.RoundToInt(tx);
            p.Gy = Mathf.RoundToInt(tz);
            p.OffsetX = tx - p.Gx;
            p.OffsetZ = tz - p.Gy;
        }

        public static void StartContainer(int holderType)
        {
            Kind = PlaceKind.Container;
            ContainerType = holderType;
            Rotation = 0f;
            ClearEndpointMode();
            DeletingHolder = false;
        }

        // mode: 0=关闭，1=起点，2=终点（再点一次同一按钮切换关闭）
        public static void ToggleEndpointMode(int mode)
        {
            if (mode == 1)
            {
                SettingStart = !SettingStart;
                if (SettingStart) SettingEnd = false;
            }
            else if (mode == 2)
            {
                SettingEnd = !SettingEnd;
                if (SettingEnd) SettingStart = false;
            }
            else
            {
                ClearEndpointMode();
            }

            if (SettingStart || SettingEnd) DeletingHolder = false;
        }

        public static void ClearEndpointMode() => SettingStart = SettingEnd = false;

        // 删除容器模式开关：开启时取消放置/端点模式
        public static void ToggleDeleteHolderMode()
        {
            DeletingHolder = !DeletingHolder;
            if (DeletingHolder)
            {
                Kind = PlaceKind.None;
                ClearEndpointMode();
            }
        }

        public static void Cancel()
        {
            Kind = PlaceKind.None;
            ClearEndpointMode();
            DeletingHolder = false;
        }

        public static void Rotate() => Rotation = (Rotation + 90f) % 360f;

        // 方向映射：0°=Up(2)，90°=Right(1)，180°=Down(3)，270°=Left(0)
        public static int DirectionFromValue(float dirValue)
        {
            int deg = NormalizeDeg(dirValue);
            switch (deg)
            {
                case 90: return (int)LevelBuilder.HolderDirection.Right;
                case 180: return (int)LevelBuilder.HolderDirection.Down;
                case 270: return (int)LevelBuilder.HolderDirection.Left;
                default: return (int)LevelBuilder.HolderDirection.Up;
            }
        }

        public static int NormalizeDeg(float v)
        {
            int deg = (int)Mathf.Round(v) % 360;
            if (deg < 0) deg += 360;
            return deg;
        }

        // 容器朝向（车头/出口）的单位向量
        public static Vector3 Forward(float dirValue)
        {
            switch (DirectionFromValue(dirValue))
            {
                case (int)LevelBuilder.HolderDirection.Right: return Vector3.right;
                case (int)LevelBuilder.HolderDirection.Down: return Vector3.back;
                case (int)LevelBuilder.HolderDirection.Left: return Vector3.left;
                default: return Vector3.forward;
            }
        }

        // 容器横向（车辆并排）的单位向量
        public static Vector3 Perp(float dirValue)
        {
            switch (DirectionFromValue(dirValue))
            {
                case (int)LevelBuilder.HolderDirection.Right:
                case (int)LevelBuilder.HolderDirection.Left:
                    return Vector3.forward;
                default:
                    return Vector3.right;
            }
        }

        /// <summary>复刻 LevelBuilder.CalcCarWorldPos（默认 frontAligned=true），返回网格坐标（不含 OrigionPos）</summary>
        public static Vector3 CalcCarWorldPos(int index, float cx, float cz, int width, int height, float dirValue)
            => CalcCarWorldPos(index, cx, cz, width, height, dirValue, true);

        public static Vector3 CalcCarWorldPos(int index, float cx, float cz, int width, int height, float dirValue,
            bool frontAligned)
        {
            int depthIdx = index / width;
            int perpIdx = index % width;
            float offsetIdx = index % 2 == 0 ? -0.05f : 0.05f;

            float halfPerp = width * CarWidthSpacing / 2f;
            float halfDepth = height * CarDepthSpacing / 2f;
            float depthOffset = frontAligned
                ? FrontDepthOffset - (height - 1 - depthIdx) * CarDepthSpacing
                : -halfDepth + depthIdx * CarDepthSpacing + HalfCarDepthSpacing;

            float wx, wz;
            switch (DirectionFromValue(dirValue))
            {
                case (int)LevelBuilder.HolderDirection.Right:
                    wx = cx + depthOffset;
                    wz = cz + halfPerp - perpIdx * CarWidthSpacing - HalfCarWidthSpacing + offsetIdx;
                    break;

                case (int)LevelBuilder.HolderDirection.Left:
                    wx = cx - depthOffset;
                    wz = cz + halfPerp - perpIdx * CarWidthSpacing - HalfCarWidthSpacing + offsetIdx;
                    break;

                case (int)LevelBuilder.HolderDirection.Down:
                    wx = cx - halfPerp + perpIdx * CarWidthSpacing + HalfCarWidthSpacing + offsetIdx;
                    wz = cz - depthOffset;
                    break;

                default: // Up
                    wx = cx - halfPerp + perpIdx * CarWidthSpacing + HalfCarWidthSpacing + offsetIdx;
                    wz = cz + depthOffset;
                    break;
            }

            return new Vector3(wx, 0f, wz);
        }

        // Standard/Extra 前对齐到标准 2×4 前排，Expandable/Tunnel 居中（与 LevelBuilder.IsFrontAlignedHolder 一致）
        public static bool IsFrontAligned(int holderType) =>
            holderType is (int)HolderPrefabType.Standard or (int)HolderPrefabType.Extra;

        /// <summary>各容器类型的默认尺寸</summary>
        public static (int width, int height) ContainerSize(int holderType)
        {
            switch ((HolderPrefabType)holderType)
            {
                case HolderPrefabType.Extra: return (2, 2);
                case HolderPrefabType.Expandable: return (2, 6);
                case HolderPrefabType.Tunnel: return (2, 10);
                default: return (2, 4);
            }
        }

        // ---- 放置 / 碰撞 ----

        public static bool TryPlaceAt(Vector3 worldPos, CoffeeSortEditData data)
        {
            if (Kind != PlaceKind.Container) return false;
            float cx = SnapHalf(worldPos.x);
            float cz = SnapHalf(worldPos.z);
            return TryPlaceContainer(data, cx, cz);
        }

        public static float SnapHalf(float v) => Mathf.Round(v / 0.5f) * 0.5f;

        private static bool TryPlaceContainer(CoffeeSortEditData data, float cx, float cz)
        {
            var (w, h) = ContainerSize(ContainerType);

            foreach (var other in data.Holders)
            {
                if (BoxesOverlap(cx, cz, w, h, Rotation,
                        other.X, other.Y, other.Width, other.Height, other.DirectionValue))
                {
                    Debug.LogWarning($"[CoffeeSort] 与已有容器重叠，无法放置 ({cx:F1},{cz:F1})");
                    return false;
                }

                // 容器占据的格子也不允许叠放车
                for (int j = 0; j < other.CarColors.Count; j++)
                {
                    Vector3 carPos = CalcCarWorldPos(j, other.X, other.Y, other.Width, other.Height, other.DirectionValue,
                        IsFrontAligned(other.HolderType));
                    if (PointInBox(carPos, cx, cz, w, h, Rotation))
                    {
                        Debug.LogWarning("[CoffeeSort] 与已有车辆重叠，无法放置");
                        return false;
                    }
                }
            }

            // 新容器 footprint 与已有车的格点冲突检查
            for (int j = 0; j < w * h; j++)
            {
                Vector3 slot = CalcCarWorldPos(j, cx, cz, w, h, Rotation, IsFrontAligned(ContainerType));
                foreach (var other in data.Holders)
                {
                    for (int k = 0; k < other.CarColors.Count; k++)
                    {
                        Vector3 otherPos = CalcCarWorldPos(k, other.X, other.Y, other.Width, other.Height, other.DirectionValue,
                            IsFrontAligned(other.HolderType));
                        if (Vector3.Distance(slot, otherPos) < 0.9f)
                        {
                            Debug.LogWarning("[CoffeeSort] 与已有车辆冲突，无法放置");
                            return false;
                        }
                    }
                }
            }

            var holder = new PlacedHolderData
            {
                HolderType = ContainerType,
                X = cx,
                Y = cz,
                Width = w,
                Height = h,
                DirectionValue = Rotation,
                CarColors = new List<int>(),
            };
            data.Holders.Add(holder);
            data.UndoStack.Add(new UndoEntry
            {
                OpType = UndoEntry.Op.AddHolder,
                Index = data.Holders.Count - 1,
            });
            return true;
        }

        // 删除模式：点击位置落在某容器 footprint 内则删除该容器（返回是否删除）
        public static bool TryDeleteHolderAt(CoffeeSortEditData data, Vector3 worldPos)
        {
            for (int i = data.Holders.Count - 1; i >= 0; i--)
            {
                var h = data.Holders[i];
                if (PointInBox(worldPos, h.X, h.Y, h.Width, h.Height, h.DirectionValue))
                {
                    data.UndoStack.Add(new UndoEntry
                    {
                        OpType = UndoEntry.Op.RemoveHolder,
                        Index = i,
                        RemovedHolder = CloneHolder(h),
                    });
                    data.Holders.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public static bool PointInBox(Vector3 point, float hx, float hz, int w, int h, float dirValue)
        {
            Vector3 f = Forward(dirValue);
            Vector3 p = Perp(dirValue);
            Vector3 rel = new Vector3(point.x - hx, 0f, point.z - hz);
            float df = Vector3.Dot(rel, f);
            float dp = Vector3.Dot(rel, p);
            return Mathf.Abs(df) <= h * CarDepthSpacing / 2f && Mathf.Abs(dp) <= w * CarWidthSpacing / 2f;
        }

        /// <summary>两个容器 footprint（旋转为 0/90/180/270，即世界轴对齐）的 AABB 相交检测</summary>
        public static bool BoxesOverlap(float ax, float az, int aw, int ah, float ad,
            float bx, float bz, int bw, int bh, float bd)
        {
            BoxAABB(ax, az, aw, ah, ad, out float aminX, out float aminZ, out float amaxX, out float amaxZ);
            BoxAABB(bx, bz, bw, bh, bd, out float bminX, out float bminZ, out float bmaxX, out float bmaxZ);
            return aminX < bmaxX && amaxX > bminX && aminZ < bmaxZ && amaxZ > bminZ;
        }

        private static void BoxAABB(float x, float z, int w, int h, float dir,
            out float minX, out float minZ, out float maxX, out float maxZ)
        {
            bool swapped = DirectionFromValue(dir) is (int)LevelBuilder.HolderDirection.Right
                or (int)LevelBuilder.HolderDirection.Left;
            float spanX = swapped ? h * CarDepthSpacing : w * CarWidthSpacing;
            float spanZ = swapped ? w * CarWidthSpacing : h * CarDepthSpacing;
            minX = x - spanX / 2f;
            maxX = x + spanX / 2f;
            minZ = z - spanZ / 2f;
            maxZ = z + spanZ / 2f;
        }

        // ---- 撤销 ----

        public static bool Undo(CoffeeSortEditData data)
        {
            if (data.UndoStack.Count == 0) return false;

            var entry = data.UndoStack[^1];
            data.UndoStack.RemoveAt(data.UndoStack.Count - 1);

            // 撤销会移除/替换路段里的点对象，选中引用会失效，统一清空
            ClearPointSelection();

            switch (entry.OpType)
            {
                case UndoEntry.Op.AddHolder:
                    if (entry.Index >= 0 && entry.Index < data.Holders.Count)
                        data.Holders.RemoveAt(entry.Index);
                    break;

                case UndoEntry.Op.RemoveHolder:
                    // 撤销删除：把被删容器插回原下标
                    if (entry.RemovedHolder != null && entry.Index >= 0)
                    {
                        int idx = Mathf.Clamp(entry.Index, 0, data.Holders.Count);
                        data.Holders.Insert(idx, entry.RemovedHolder);
                    }

                    break;

                case UndoEntry.Op.AddPathPoint:
                    if (entry.Index >= 0 && entry.Index < data.RoadSegments.Count && entry.Point != null)
                        data.RoadSegments[entry.Index].PathPoints.RemoveAll(p =>
                            p.Gx == entry.Point.Gx && p.Gy == entry.Point.Gy);
                    break;

                case UndoEntry.Op.AddSegment:
                    if (entry.Index >= 0 && entry.Index < data.RoadSegments.Count)
                    {
                        data.RoadSegments.RemoveAt(entry.Index);
                        if (data.CurrentSegmentIndex >= data.RoadSegments.Count)
                            data.CurrentSegmentIndex = data.RoadSegments.Count - 1;
                    }

                    break;

                case UndoEntry.Op.SetStart:
                    // Point 记录的是"被设置前的原值"：设新起点时存 null（撤销=清空），移除已有点时存原值（撤销=恢复）
                    if (entry.Index >= 0 && entry.Index < data.RoadSegments.Count)
                        data.RoadSegments[entry.Index].Start = entry.Point;
                    break;

                case UndoEntry.Op.SetEnd:
                    if (entry.Index >= 0 && entry.Index < data.RoadSegments.Count)
                        data.RoadSegments[entry.Index].End = entry.Point;
                    break;

                case UndoEntry.Op.Mirror:
                    // 恢复对称生成前的完整快照
                    if (entry.SegSnapshot != null)
                    {
                        data.RoadSegments.Clear();
                        data.RoadSegments.AddRange(CloneSegments(entry.SegSnapshot));
                    }

                    if (entry.HolderSnapshot != null)
                    {
                        data.Holders.Clear();
                        data.Holders.AddRange(CloneHolders(entry.HolderSnapshot));
                    }

                    data.CurrentSegmentIndex = entry.CurrentSegmentSnapshot;
                    break;

                case UndoEntry.Op.Merge:
                case UndoEntry.Op.MoveSegment:
                    // 合并/整体平移：恢复操作前的整表路段快照
                    if (entry.SegSnapshot != null)
                    {
                        data.RoadSegments.Clear();
                        data.RoadSegments.AddRange(CloneSegments(entry.SegSnapshot));
                    }

                    data.CurrentSegmentIndex = entry.CurrentSegmentSnapshot;
                    break;

                case UndoEntry.Op.RescaleHeight:
                    // 重排高度：恢复操作前的整表路段快照与旧层间距
                    if (entry.SegSnapshot != null)
                    {
                        data.RoadSegments.Clear();
                        data.RoadSegments.AddRange(CloneSegments(entry.SegSnapshot));
                    }

                    data.CurrentSegmentIndex = entry.CurrentSegmentSnapshot;
                    data.RoadLayerSpacing = entry.SpacingSnapshot;
                    break;

                case UndoEntry.Op.RoundSpiral:
                case UndoEntry.Op.RemoveSymmetrySide:
                    // 螺旋圆化 / 删除对称轴对侧点：恢复操作前的整表路段快照
                    if (entry.SegSnapshot != null)
                    {
                        data.RoadSegments.Clear();
                        data.RoadSegments.AddRange(CloneSegments(entry.SegSnapshot));
                    }

                    data.CurrentSegmentIndex = entry.CurrentSegmentSnapshot;
                    break;
            }

            return true;
        }

        // ---- 对称生成 ----

        public static bool ApplySymmetry(CoffeeSortEditData data)
        {
            // 对称生成会删/加路段，选中的路径点引用可能失效，先清空
            ClearPointSelection();

            bool axisIsX = data.SymmetryAxisIsX;
            float axis = data.SymmetryValue;

            // 固定操作侧：x=? 轴 → 左侧（Gx 小于轴）为操作区；y=? 轴 → 下方（Gy 小于轴）为操作区。
            // 统一以"坐标小于轴"的一侧（Side.Below）为源侧，镜像到"坐标大于轴"一侧（Side.Above）。
            Side source = Side.Below;
            Side opposite = Side.Above;

            // 操作区为空则不做任何事，避免误删对侧内容
            if (!HasSourceContent(data, axisIsX, axis))
            {
                Debug.LogWarning("[CoffeeSort] 操作区（x=? 轴左侧 / y=? 轴下方）没有内容，无法对称生成");
                return false;
            }

            // 2. 记录撤销快照（整个对称操作一步撤销）
            data.UndoStack.Add(new UndoEntry
            {
                OpType = UndoEntry.Op.Mirror,
                SegSnapshot = CloneSegments(data.RoadSegments),
                HolderSnapshot = CloneHolders(data.Holders),
                CurrentSegmentSnapshot = data.CurrentSegmentIndex,
            });

            // 3. 删除对侧内容（完全在对侧的路段 / 中心在对侧的容器；跨轴或轴上内容保留）
            for (int i = data.RoadSegments.Count - 1; i >= 0; i--)
                if (SegmentSide(data.RoadSegments[i], axisIsX, axis) == opposite)
                    data.RoadSegments.RemoveAt(i);
            for (int i = data.Holders.Count - 1; i >= 0; i--)
            {
                float c = axisIsX ? data.Holders[i].X : data.Holders[i].Y;
                if (opposite == Side.Below ? c < axis : c > axis)
                    data.Holders.RemoveAt(i);
            }

            // 4. 源侧内容镜像追加到对侧（类型/宽高/颜色/起点终点不变，仅位置镜像 + 朝向翻转）。
            // 路段按整段镜像：不是"完全落在对称轴上"（自对称）就整体镜像，即使跨轴也照镜像。
            // 幂等：若对侧已存在完全相同的位置镜像副本则跳过，重复点击不会产生重复路段。
            var mirrorSegs = new List<RoadSegmentData>();
            foreach (var seg in data.RoadSegments)
            {
                if (SegmentEntirelyOnAxis(seg, axisIsX, axis)) continue;
                var m = MirrorSegment(seg, axisIsX, axis);
                if (ContainsSegment(data.RoadSegments, m)) continue;
                mirrorSegs.Add(m);
            }

            data.RoadSegments.AddRange(mirrorSegs);

            var mirrorHolders = new List<PlacedHolderData>();
            foreach (var h in data.Holders)
            {
                float c = axisIsX ? h.X : h.Y;
                if (source == Side.Below ? c < axis : c > axis)
                    mirrorHolders.Add(MirrorHolder(h, axisIsX, axis));
            }

            data.Holders.AddRange(mirrorHolders);

            // 5. 修正当前路段下标（删除对侧路段后可能越界）
            if (data.CurrentSegmentIndex >= data.RoadSegments.Count)
                data.CurrentSegmentIndex = data.RoadSegments.Count - 1;

            return true;
        }

        // 操作区（坐标小于轴的一侧）是否存在内容
        private static bool HasSourceContent(CoffeeSortEditData data, bool axisIsX, float axis)
        {
            foreach (var seg in data.RoadSegments)
            {
                foreach (var p in SegPoints(seg))
                {
                    float c = axisIsX ? p.Gx : p.Gy;
                    if (c < axis) return true;
                }
            }

            foreach (var h in data.Holders)
            {
                float c = axisIsX ? h.X : h.Y;
                if (c < axis) return true;
            }

            return false;
        }

        // 列表中是否已存在与 target 完全相同的路段（用于幂等判断）
        private static bool ContainsSegment(List<RoadSegmentData> list, RoadSegmentData target)
        {
            foreach (var s in list)
                if (SegmentsEqual(s, target)) return true;
            return false;
        }

        // 两个路段完整点序（Start + PathPoints + End）是否一致
        private static bool SegmentsEqual(RoadSegmentData a, RoadSegmentData b)
        {
            var pa = SegPoints(a);
            var pb = SegPoints(b);
            if (pa.Count != pb.Count) return false;
            for (int i = 0; i < pa.Count; i++)
            {
                if (pa[i].Gx != pb[i].Gx || pa[i].Gy != pb[i].Gy) return false;
                if (!Mathf.Approximately(pa[i].Y, pb[i].Y)) return false;
                if (!Mathf.Approximately(pa[i].OffsetX, pb[i].OffsetX)) return false;
                if (!Mathf.Approximately(pa[i].OffsetZ, pb[i].OffsetZ)) return false;
            }

            return true;
        }

        private enum Side { Below, Above, Mixed }

        // 路段所在侧：全在轴一侧归该侧，跨轴或全在轴上归 Mixed（不删不镜像）
        private static Side SegmentSide(RoadSegmentData seg, bool axisIsX, float axis)
        {
            bool hasBelow = false, hasAbove = false;
            foreach (var p in SegPoints(seg))
            {
                float c = axisIsX ? p.Gx : p.Gy;
                if (c < axis) hasBelow = true;
                else if (c > axis) hasAbove = true;
            }

            if (hasBelow && hasAbove) return Side.Mixed;
            if (hasBelow) return Side.Below;
            if (hasAbove) return Side.Above;
            return Side.Mixed;
        }

        private static List<PathPointData> SegPoints(RoadSegmentData seg)
        {
            var pts = new List<PathPointData>(seg.PathPoints.Count + 2);
            if (seg.Start != null) pts.Add(seg.Start);
            pts.AddRange(seg.PathPoints);
            if (seg.End != null) pts.Add(seg.End);
            return pts;
        }

        // 路段所有点是否都落在对称轴上（自对称路段）：是则跳过镜像，避免生成重叠的重复路段
        private static bool SegmentEntirelyOnAxis(RoadSegmentData seg, bool axisIsX, float axis)
        {
            var pts = SegPoints(seg);
            if (pts.Count == 0) return true; // 空路段不镜像
            foreach (var p in pts)
            {
                float c = axisIsX ? p.Gx : p.Gy;
                if (c != axis) return false;
            }

            return true;
        }

        private static RoadSegmentData MirrorSegment(RoadSegmentData seg, bool axisIsX, float axis)
        {
            var m = new RoadSegmentData();
            m.Start = seg.Start == null ? null : MirrorPoint(seg.Start, axisIsX, axis);
            m.End = seg.End == null ? null : MirrorPoint(seg.End, axisIsX, axis);
            foreach (var p in seg.PathPoints) m.PathPoints.Add(MirrorPoint(p, axisIsX, axis));
            return m;
        }

        private static PathPointData MirrorPoint(PathPointData p, bool axisIsX, float axis)
        {
            return new PathPointData
            {
                Gx = axisIsX ? Mathf.RoundToInt(2f * axis - p.Gx) : p.Gx,
                Gy = axisIsX ? p.Gy : Mathf.RoundToInt(2f * axis - p.Gy),
                Y = p.Y,
                // 镜像方向上的精细偏移取反，保持相对对称轴的左右/上下镜像位置
                OffsetX = axisIsX ? -p.OffsetX : p.OffsetX,
                OffsetZ = axisIsX ? p.OffsetZ : -p.OffsetZ,
            };
        }

        private static PlacedHolderData MirrorHolder(PlacedHolderData h, bool axisIsX, float axis)
        {
            var m = new PlacedHolderData
            {
                HolderType = h.HolderType,
                Width = h.Width,
                Height = h.Height,
                DirectionValue = MirrorDirection(h.DirectionValue, axisIsX),
                CarColors = new List<int>(h.CarColors),
            };

            if (axisIsX)
            {
                m.X = 2f * axis - h.X;
                m.Y = h.Y;
            }
            else
            {
                m.X = h.X;
                m.Y = 2f * axis - h.Y;
            }

            return m;
        }

        // 关于 x=c 左右镜像：Right↔Left（90°↔270°），Up/Down 不变 → newDir=(360-dir)%360
        // 关于 y=c 上下镜像：Up↔Down（0°↔180°），Left/Right 不变 → newDir=(180-dir)%360
        private static float MirrorDirection(float dirValue, bool axisIsX)
        {
            int deg = NormalizeDeg(dirValue);
            int newDeg = axisIsX ? (360 - deg) % 360 : (180 - deg + 360) % 360;
            return newDeg;
        }

        /// <summary>
        /// 删除对称轴对侧（镜像侧）的全部道路点，保留源侧（坐标小于轴）与轴上的点。
        /// 用于：保存后想拉近对称轴两边的间距时，先删一边 → 整体移动源侧 → 再重新对称生成。
        /// 跨轴路段内的对侧点会被移除，整段都在对侧的路段整段删除，空路段自动清理。
        /// 撤销走整表路段快照（UndoEntry.Op.RemoveSymmetrySide）。
        /// </summary>
        public static bool RemoveSymmetrySide(CoffeeSortEditData data)
        {
            bool axisIsX = data.SymmetryAxisIsX;
            float axis = data.SymmetryValue;

            bool hasOpposite = false;
            foreach (var seg in data.RoadSegments)
                foreach (var p in SegPoints(seg))
                    if (axisIsX ? p.Gx > axis : p.Gy > axis) { hasOpposite = true; break; }

            if (!hasOpposite) return false;

            ClearPointSelection();
            data.UndoStack.Add(new UndoEntry
            {
                OpType = UndoEntry.Op.RemoveSymmetrySide,
                SegSnapshot = CloneSegments(data.RoadSegments),
                CurrentSegmentSnapshot = data.CurrentSegmentIndex,
            });

            int originalCurrent = data.CurrentSegmentIndex;
            bool currentRemoved = false;
            int removalsBeforeCurrent = 0;

            for (int i = data.RoadSegments.Count - 1; i >= 0; i--)
            {
                var seg = data.RoadSegments[i];
                seg.PathPoints.RemoveAll(p => axisIsX ? p.Gx > axis : p.Gy > axis);
                if (seg.Start != null && (axisIsX ? seg.Start.Gx > axis : seg.Start.Gy > axis)) seg.Start = null;
                if (seg.End != null && (axisIsX ? seg.End.Gx > axis : seg.End.Gy > axis)) seg.End = null;

                if (seg.Start == null && seg.End == null && seg.PathPoints.Count == 0)
                {
                    if (i == originalCurrent) currentRemoved = true;
                    else if (i < originalCurrent) removalsBeforeCurrent++;
                    data.RoadSegments.RemoveAt(i);
                }
            }

            // 修正当前路段下标：当前段被删则就近取有效段，前面有段被删则前移
            data.CurrentSegmentIndex = currentRemoved
                ? Mathf.Clamp(originalCurrent, 0, data.RoadSegments.Count - 1)
                : originalCurrent - removalsBeforeCurrent;

            return true;
        }

        private static PathPointData ClonePoint(PathPointData p) => new PathPointData
        {
            Gx = p.Gx,
            Gy = p.Gy,
            Y = p.Y,
            OffsetX = p.OffsetX,
            OffsetZ = p.OffsetZ,
        };

        private static List<RoadSegmentData> CloneSegments(List<RoadSegmentData> list)
        {
            var copy = new List<RoadSegmentData>(list.Count);
            foreach (var s in list)
            {
                var c = new RoadSegmentData();
                c.Start = s.Start == null ? null : ClonePoint(s.Start);
                c.End = s.End == null ? null : ClonePoint(s.End);
                foreach (var p in s.PathPoints) c.PathPoints.Add(ClonePoint(p));
                copy.Add(c);
            }

            return copy;
        }

        private static PlacedHolderData CloneHolder(PlacedHolderData h)
        {
            return new PlacedHolderData
            {
                HolderType = h.HolderType,
                X = h.X,
                Y = h.Y,
                Width = h.Width,
                Height = h.Height,
                DirectionValue = h.DirectionValue,
                CarColors = new List<int>(h.CarColors),
            };
        }

        private static List<PlacedHolderData> CloneHolders(List<PlacedHolderData> list)
        {
            var copy = new List<PlacedHolderData>(list.Count);
            foreach (var h in list) copy.Add(CloneHolder(h));
            return copy;
        }

        // ---- 路段合并 / 整体平移 ----

        /// <summary>
        /// 合并当前路段与下一路段（首尾相接循环取下一段）为一个路段。
        /// 新路段点序 = A.Start, ...A.PathPoints, A.End, B.End, ...B.PathPoints(倒序), B.Start，
        /// 即终点接终点（E_A→E_B）、起点接起点（S_B→S_A），适合对称生成后把两半并成一条闭环路。
        /// 撤销走整表快照（UndoEntry.Op.Merge）。
        /// </summary>
        public static bool MergeSegment(CoffeeSortEditData data)
        {
            if (data.RoadSegments.Count < 2) return false;
            ClearPointSelection();

            int i = data.CurrentSegmentIndex < 0 ? 0 : data.CurrentSegmentIndex;
            int j = (i + 1) % data.RoadSegments.Count;

            data.UndoStack.Add(new UndoEntry
            {
                OpType = UndoEntry.Op.Merge,
                SegSnapshot = CloneSegments(data.RoadSegments),
                CurrentSegmentSnapshot = data.CurrentSegmentIndex,
            });

            var a = data.RoadSegments[i];
            var b = data.RoadSegments[j];

            var merged = new RoadSegmentData();
            merged.Start = a.Start == null ? null : ClonePoint(a.Start);
            merged.End = b.Start == null ? null : ClonePoint(b.Start);
            foreach (var p in a.PathPoints) merged.PathPoints.Add(ClonePoint(p));
            if (a.End != null) merged.PathPoints.Add(ClonePoint(a.End));
            if (b.End != null) merged.PathPoints.Add(ClonePoint(b.End));
            for (int k = b.PathPoints.Count - 1; k >= 0; k--)
                merged.PathPoints.Add(ClonePoint(b.PathPoints[k]));

            int lo = Mathf.Min(i, j);
            int hi = Mathf.Max(i, j);
            data.RoadSegments.RemoveAt(hi);
            data.RoadSegments.RemoveAt(lo);
            data.RoadSegments.Insert(lo, merged);
            data.CurrentSegmentIndex = lo;

            return true;
        }

        /// <summary>把当前路段的所有点（起点/终点/路径点）整体平移 (dx, dz) 个格子</summary>
        public static bool MoveSegment(CoffeeSortEditData data, int dx, int dz)
        {
            var seg = data.CurrentSegment;
            if (seg == null) return false;
            if (dx == 0 && dz == 0) return false;
            ClearPointSelection();

            data.UndoStack.Add(new UndoEntry
            {
                OpType = UndoEntry.Op.MoveSegment,
                SegSnapshot = CloneSegments(data.RoadSegments),
                CurrentSegmentSnapshot = data.CurrentSegmentIndex,
            });

            if (seg.Start != null)
            {
                seg.Start.Gx += dx;
                seg.Start.Gy += dz;
            }

            if (seg.End != null)
            {
                seg.End.Gx += dx;
                seg.End.Gy += dz;
            }

            foreach (var p in seg.PathPoints)
            {
                p.Gx += dx;
                p.Gy += dz;
            }

            return true;
        }

        // ---- 层间距重排 ----

        /// <summary>
        /// 按新层间距整体竖直拉伸道路：所有路段所有点的 Y 乘以 (newSpacing / oldSpacing)，
        /// y=0 的点乘 0 自然保持不动，螺旋层从 0/5/10/15/20 变为 0/8/16/24/32。
        /// 撤销走整表路段快照 + 旧间距（UndoEntry.Op.RescaleHeight）。
        /// </summary>
        public static bool RescaleRoadHeight(CoffeeSortEditData data, float newSpacing)
        {
            if (newSpacing <= 0f || data.RoadLayerSpacing <= 0f) return false;
            float scale = newSpacing / data.RoadLayerSpacing;
            if (Mathf.Approximately(scale, 1f)) return false;
            ClearPointSelection();

            data.UndoStack.Add(new UndoEntry
            {
                OpType = UndoEntry.Op.RescaleHeight,
                SegSnapshot = CloneSegments(data.RoadSegments),
                CurrentSegmentSnapshot = data.CurrentSegmentIndex,
                SpacingSnapshot = data.RoadLayerSpacing,
            });

            foreach (var seg in data.RoadSegments)
            {
                if (seg.Start != null) seg.Start.Y *= scale;
                if (seg.End != null) seg.End.Y *= scale;
                foreach (var p in seg.PathPoints) p.Y *= scale;
            }

            data.RoadLayerSpacing = newSpacing;
            return true;
        }

        // ---- 螺旋圆化 ----

        /// <summary>
        /// 把当前路段的螺旋圈做成正圆：y=0 的点（底部环）保持不动，
        /// y&gt;0 的点按路径被底部环隔成若干连续段，每段内按累计转角分圈，
        /// 每圈半径取平均并线性平滑，圈内角度、高度均匀重排，实现圆形螺旋 + 均匀下坡/上坡。
        /// 撤销走整表路段快照（UndoEntry.Op.RoundSpiral）。
        /// </summary>
        public static bool RoundSpiral(CoffeeSortEditData data)
        {
            var seg = data.CurrentSegment;
            if (seg == null) return false;

            var pts = new List<PathPointData>();
            if (seg.Start != null) pts.Add(seg.Start);
            pts.AddRange(seg.PathPoints);
            if (seg.End != null) pts.Add(seg.End);
            if (pts.Count < 3) return false;

            bool anyMobile = false;
            foreach (var p in pts)
                if (!Mathf.Approximately(p.Y, 0f)) { anyMobile = true; break; }

            if (!anyMobile) return false;

            ClearPointSelection();
            data.UndoStack.Add(new UndoEntry
            {
                OpType = UndoEntry.Op.RoundSpiral,
                SegSnapshot = CloneSegments(data.RoadSegments),
                CurrentSegmentSnapshot = data.CurrentSegmentIndex,
            });

            // 底部环（y=0）作为分隔符，y>0 的连续点各成一段，段内圆化
            var mobile = new List<PathPointData>();
            foreach (var p in pts)
            {
                if (Mathf.Approximately(p.Y, 0f))
                {
                    if (mobile.Count > 0)
                    {
                        RoundSpiralSub(mobile);
                        mobile.Clear();
                    }
                }
                else
                {
                    mobile.Add(p);
                }
            }

            if (mobile.Count > 0) RoundSpiralSub(mobile);
            return true;
        }

        /// <summary>
        /// 圆化单个移动段：以段内质心为圆心，按累计转角分圈；
        /// 每圈半径取平均并首末线性平滑，段内角度、高度沿路径线性重排。
        /// 段内首末点高度保持，水平位置随圈移动（质心不变，整体不漂移）。
        /// </summary>
        private static void RoundSpiralSub(List<PathPointData> mobile)
        {
            int m = mobile.Count;
            if (m < 3) return;

            Vector2 center = Vector2.zero;
            for (int i = 0; i < m; i++)
                center += new Vector2(mobile[i].WorldX, mobile[i].WorldZ);
            center /= m;

            var angles = new float[m];
            var cum = new float[m];
            var rad = new float[m];
            for (int i = 0; i < m; i++)
            {
                Vector2 v = new Vector2(mobile[i].WorldX, mobile[i].WorldZ) - center;
                rad[i] = v.magnitude;
                angles[i] = Mathf.Atan2(v.y, v.x);
            }

            cum[0] = angles[0];
            for (int i = 1; i < m; i++)
            {
                float d = angles[i] - angles[i - 1];
                while (d > Mathf.PI) d -= Mathf.PI * 2f;
                while (d < -Mathf.PI) d += Mathf.PI * 2f;
                cum[i] = cum[i - 1] + d;
            }

            float totalAngle = cum[m - 1] - cum[0];
            float totalHeight = mobile[m - 1].Y - mobile[0].Y;

            // 转角不足半圈（如直段/弱弧）只均匀高度，不做水平圆化，避免点挤成一团
            if (Mathf.Abs(totalAngle) < Mathf.PI * 0.5f)
            {
                for (int i = 0; i < m; i++)
                    mobile[i].Y = mobile[0].Y + totalHeight * i / (m - 1);
                return;
            }

            int K = Mathf.Max(1, Mathf.RoundToInt(Mathf.Abs(totalAngle) / (Mathf.PI * 2f)));
            float per = totalAngle / K;

            var radius = new float[K];
            var counts = new int[K];
            for (int i = 0; i < m; i++)
            {
                int k = Mathf.Abs(per) > 1e-4f
                    ? Mathf.Clamp(Mathf.FloorToInt((cum[i] - cum[0]) / per), 0, K - 1)
                    : 0;
                radius[k] += rad[i];
                counts[k]++;
            }

            for (int k = 0; k < K; k++)
                if (counts[k] > 0) radius[k] /= counts[k];

            // 圈间半径线性平滑：外圈→内圈均匀渐变
            if (K > 1)
            {
                float r0 = radius[0];
                float r1 = radius[K - 1];
                for (int k = 0; k < K; k++)
                    radius[k] = r0 + (r1 - r0) * k / (K - 1);
            }

            for (int i = 0; i < m; i++)
            {
                float t = (float)i / (m - 1);
                float a = cum[0] + totalAngle * t;
                int k = Mathf.Clamp(Mathf.FloorToInt(t * K), 0, K - 1);
                float r = radius[k];
                float wx = center.x + r * Mathf.Cos(a);
                float wz = center.y + r * Mathf.Sin(a);

                mobile[i].Y = mobile[0].Y + totalHeight * t;
                mobile[i].Gx = Mathf.RoundToInt(wx - 0.5f);
                mobile[i].OffsetX = (wx - 0.5f) - mobile[i].Gx;
                mobile[i].Gy = Mathf.RoundToInt(wz - 0.5f);
                mobile[i].OffsetZ = (wz - 0.5f) - mobile[i].Gy;
            }
        }
    }
}
