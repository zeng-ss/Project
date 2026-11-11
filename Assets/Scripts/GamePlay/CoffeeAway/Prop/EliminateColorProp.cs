using System.Collections.Generic;
using System.Linq;
using UnityGameFramework.Runtime;

namespace CasualGame.CoffeeAway
{
    /// <summary>
    /// 消除颜色道具：随机消除场上一种颜色的所有车辆（所有容器 + 道路）。
    /// </summary>
    public class EliminateColorProp : PropBase
    {
        public override ItemType ItemType => ItemType.EliminateColor;

        public EliminateColorProp(CoffeeAwayCore core) : base(core)
        {
        }

        public override bool Execute(Holder targetHolder = null, ColorType targetColor = default)
        {
            // 未指定颜色时随机消除场上存在的一种颜色
            if (targetColor == default)
            {
                var colors = GetRemainingColors();
                if (colors.Count == 0)
                {
                    Log.Warning("[PowerUp:EliminateColor] 消除失败：场上没有车辆");
                    return false;
                }

                targetColor = colors[Core.Rng.Next(colors.Count)];
            }

            var removed = Eliminate(targetColor);
            if (removed.Count == 0)
            {
                Log.Warning($"[PowerUp:EliminateColor] 消除失败：场上没有颜色 {targetColor} 的车辆");
                return false;
            }

            // 被消除车辆直接回收
            foreach (var car in removed) car.DoScaleAni();
            return true;
        }

        // 当前可被消除的颜色列表：跳过未解锁/已完成排序容器内的车辆
        private List<ColorType> GetRemainingColors()
        {
            var protectedCars = CollectProtectedCars();
            var colors = new List<ColorType>();
            foreach (var v in Core.Vehicles)
            {
                if (protectedCars.Contains(v.ColorType)) continue;
                if (!colors.Contains(v.ColorType)) colors.Add(v.ColorType);
            }

            return colors;
        }

        // 收集受保护容器（未解锁特殊玩法 / 已完成排序）内的车辆颜色
        private HashSet<ColorType> CollectProtectedCars()
        {
            var set = new HashSet<ColorType>();
            foreach (var holder in Core.Holders)
            {
                if (!holder.IsEliminationBlocked()) continue;

                foreach (var colorType in holder.GetColorsInHolder())
                {
                    set.Add(colorType);
                }
            }

            return set;
        }

        private List<Vehicle> Eliminate(ColorType color)
        {
            var toRemove = new HashSet<Vehicle>();
            var affectedHolders = new List<Holder>();

            // 逐容器移除目标色并压实网格数据
            foreach (var holder in Core.Holders)
            {
                var removed = holder.GetRemoveCarsAndUpdateGrid(color);
                if (removed.Count == 0) continue;

                foreach (var car in removed) toRemove.Add(car);
                affectedHolders.Add(holder);
            }

            // 道路上 / 排队中的该颜色车辆
            // 注意：WaitingExit/Exiting 车仍占用容器网格，会先被上面的容器循环加入 toRemove；
            // 但它们同时占用道路计数（CarsOnRoad/_showCarsOnRoad），必须继续走这里同步计数并清理出场状态，
            // 因此不能因 toRemove.Contains 跳过。
            foreach (var car in new List<Vehicle>(Core.Vehicles))
            {
                if (car.ColorType != color) continue;
                if (car.State == VehicleState.InHolder) continue;

                Core.RemoveFromPendingExits(car);
                toRemove.Add(car);
                switch (car.State)
                {
                    // WaitingExit 排队车点击时已计入容量未计入显示；OnRoad/Exiting 两者都计入，撤销按实际扣回
                    case VehicleState.Exiting or VehicleState.OnRoad:
                        Core.RemoveCarsOnRoad(1, 1);
                        break;
                    case VehicleState.WaitingExit:
                        Core.RemoveCarsOnRoad(1, 0);
                        break;
                }
                car.CancelRoadState();
            }

            // 从主列表移除
            Core.Vehicles.RemoveAll(toRemove.Contains);

            // 压实动画全部落位后再统一刷新前排问号杯揭开：中途揭开会把替换车留在旧位置，导致问号杯"看起来没被正确Replace"
            if (affectedHolders.Count > 0)
            {
                int remain = affectedHolders.Count;
                foreach (var holder in affectedHolders)
                {
                    holder.MoveRemainVehicleToBack(() =>
                    {
                        if (--remain <= 0)
                        {
                            foreach (var h in affectedHolders) h.RefreshHiddenVehicleDisplay();
                            Core.NotifyContainersCompleted();
                        }
                    });
                }
            }

            return new List<Vehicle>(toRemove);
        }
    }
}