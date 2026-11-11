using System.Collections.Generic;
using System.Linq;

namespace CasualGame.CoffeeAway
{
    /// <summary>
    /// 容器集体管理器
    /// </summary>
    public class HolderManager
    {
        #region Fields & Properties

        private readonly List<Holder> _holders = new();
        private ExtraHolder _extraHolder;

        public List<Holder> Holders => _holders;

        #endregion

        #region Lifecycle

        public void Setup(List<Holder> holders)
        {
            _holders.AddRange(holders);
            foreach (var h in holders)
            {
                if (h.Info.HolderPrefabType == HolderPrefabType.Extra)
                    _extraHolder = (ExtraHolder)h;
                else
                    h.SetFrontOutLine();
            }
        }

        // 每帧统一驱动各容器的通关检测
        public void Tick()
        {
            foreach (var holder in _holders) holder.ManualTick();
        }

        public void Clear()
        {
            _holders.Clear();
            _extraHolder = null;
        }

        #endregion

        #region Collective Queries

        // 是否有任意容器可以接受该颜色的车
        public bool CanAnyHolderAccept(Vehicle v)
        {
            foreach (var holder in Holders)
            {
                if (holder.Info.Cars == null || !holder.gameObject.activeSelf) continue;
                if (holder == v.OriginHolder && v.IsOriginReentryNoOp(holder)) continue;
                // 颜色容器对应颜色只能进颜色容器，普通容器一律不算可接受
                if (IsColorTargetOfFilteredMachine(v.ColorType) && !holder.HasColorFilteredSpecial()) continue;
                if (holder.CanAcceptColor(v.ColorType)) return true;
            }

            return false;
        }

        // 收集其他容器当前可接受的颜色 只关心来源容器内存在的颜色，供打乱道具交换到前排
        public HashSet<ColorType> GetAcceptableColors(Holder sourceHolder)
        {
            var colors = new HashSet<ColorType>();
            var candidates = sourceHolder?.GetColorsInHolder();
            if (candidates is null || candidates.Count == 0) return colors;

            foreach (var holder in _holders)
            {
                if (holder == sourceHolder) continue;
                if (holder.Info.Cars == null || !holder.gameObject.activeSelf) continue;

                foreach (var color in candidates)
                {
                    if (holder.CanAcceptColor(color)) colors.Add(color);
                }
            }

            return colors;
        }

        public bool TryGetExtraHolder(out ExtraHolder extra)
        {
            extra = _extraHolder;
            return extra is not null;
        }

        // 额外容器已出现且仍有车时，玩家仍需手动整理，不自动完成
        public bool IsExtraHolderActiveAndNotEmpty()
            => _extraHolder is not null && _extraHolder.gameObject.activeSelf && !_extraHolder.IsEmpty();

        #endregion

        #region Collective Judgement

        // 该颜色是否被某个颜色容器（ColorFilteredMachine）锁定，需由颜色容器填满才算完成
        public bool IsColorTargetOfFilteredMachine(ColorType color)
        {
            foreach (var h in Holders)
            {
                if (h is ColorFilteredMachine cm && cm.AcceptColor == color) return true;
            }

            return false;
        }

        // 容器侧：所有含目标颜色的容器都已铺满该颜色
        public bool IsColorFullySettled(ColorType color, Holder exceptSelf)
        {
            foreach (var holder in _holders)
            {
                if (holder == exceptSelf) continue;
                if (!holder.ContainsColor(color)) continue;
                if (!holder.IsFullSameColor) return false;
            }

            return true;
        }

        // 所有容器要么同色满、要么全空
        public bool AllSettled()
        {
            foreach (var holder in _holders)
            {
                if (holder.Info.HolderPrefabType == HolderPrefabType.Extra) continue;
                if (!holder.IsEmpty())
                {
                    if (!holder.IsFullSameColor) return false;
                    // 等待完成序列（如打包动画）播完；空容器没有完成序列，不受此约束
                    if (!holder.IsCompleteCheckEnded) return false;
                }
            }

            return true;
        }

        // 统计每个待补容器还需多少同色车，并列出"空容器" 存在需玩家操作时返回 false
        public bool TryCollectDemand(out List<Holder> emptyHolders, out int emptyCapacity,
            out Dictionary<ColorType, int> demand)
        {
            demand = new Dictionary<ColorType, int>();
            emptyHolders = new List<Holder>();
            emptyCapacity = 0;

            foreach (var holder in _holders)
            {
                if (holder.Info.HolderPrefabType == HolderPrefabType.Extra) continue;
                if (holder.IsFullSameColor) continue;
                if (!holder.CanTap()) return false; // 未解锁的盖子等仍需玩家操作

                var (isWaiting, placed) = holder.IsWaitingVehicleToComplete();
                if (!isWaiting) return false; // 混色容器仍需玩家整理

                int cap = holder.Info.Width * holder.Info.Height;
                if (holder.IsEmpty())
                {
                    emptyHolders.Add(holder);
                    emptyCapacity += cap;
                }
                else
                {
                    ColorType? color = holder.GetFrontColor();
                    if (color is null) return false;
                    demand[color.Value] =
                        demand.TryGetValue(color.Value, out var n) ? n + cap - placed : cap - placed;
                }
            }

            return true;
        }

        // 广播
        public void NotifyContainersCompleted()
        {
            foreach (var h in Holders)
            {
                if (h.SpecialHolders == null) continue;
                for (var i = 0; i < h.SpecialHolders.Count; i++)
                {
                    h.SpecialHolders[i]?.UpdateProgress();
                }
            }
        }

        // 是否有该颜色的目标容器
        public bool HasPendingColorDemand(ColorType color)
        {
            foreach (var h in Holders)
            {
                if (h.Info.HolderPrefabType == HolderPrefabType.Extra) continue;
                if (h.IsFullSameColor || h.IsEmpty()) continue;
                if (h.GetFrontColor() == color) return true;
            }

            return false;
        }

        // 多余车按道路顺序被空容器吸收
        public bool CanFillEmptyHolders(int surplus, List<Holder> emptyHolders, ColorType color)
        {
            if (surplus <= 0 || emptyHolders.Count == 0) return false;

            var accepting = emptyHolders
                .Where(h => h.CanAcceptColor(color))
                .OrderBy(h => h.RoadConnectionDistance)
                .ToList();

            int sum = 0;
            foreach (var h in accepting)
            {
                sum += h.Info.Width * h.Info.Height;
                if (sum == surplus) return true;
                if (sum > surplus) return false; // 途中某个空容器必然被部分填满
            }

            return false;
        }

        #endregion

        #region Rendering & Highlight

        public void RefreshRenderLayers()
        {
            foreach (var holder in _holders) holder.RefreshRenderLayer();
        }

        public bool ShouldShowShuffleHighlight()
        {
            return _holders.Any(holder => holder.ShouldHighLight());
        }

        #endregion
    }
}
