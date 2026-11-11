using System.Collections.Generic;
using UnityEngine;

namespace CasualGame.CoffeeAway
{
    public partial class CoffeeAwayCore
    {
        // 逐容器放行待出场车辆：按出口点道路空间决定何时发车
        private void ProcessPendingExits()
        {
            if (_vehicleMgr.IsSelectingFrozen || _pendingExits.Count == 0) return;

            var done = new List<Holder>();
            foreach (var (holder, queue) in _pendingExits)
            {
                if (queue.Count == 0)
                {
                    done.Add(holder);
                    continue;
                }

                // 只按空间判断发车：出口点前方 _minCarGap 内无车即发，落地间距由前车离开出口点的距离自然保证
                if (_vehicleMgr.CanStartExitAt(holder.RoadConnectionDistance))
                {
                    var car = queue[0];
                    queue.RemoveAt(0);
                    car.ExitedFromHolder = holder;
                    car.OriginHolder = holder;
                    _vehicleMgr.StartSingleExit(car, holder.RoadConnectionDistance,
                        c => holder.OnCarExited(c, queue.Count == 0));
                }
            }

            foreach (var h in done) _pendingExits.Remove(h);
        }

        // 车辆行驶中跨越容器入口：能进则进，回到来源容器则清除来源标记
        // 注意：多个容器可能共享同一个道路交点 因此一个容器拒收后不能直接结束判断，要继续检查同交点的其他容器
        private void CheckHolderEntry(Vehicle v)
        {
            foreach (var holder in _holderMgr.Holders)
            {
                if (holder.Info.Cars == null || !holder.gameObject.activeSelf) continue;
                if (!_roadNav.CrossedDistance(v.PrevRoadDistance, v.RoadDistance, holder.RoadConnectionDistance))
                    continue;

                // 绕了一圈又回到来源容器，清除标记允许重新进入，本次穿越消费掉
                if (holder == v.ExitedFromHolder)
                {
                    v.ExitedFromHolder = null;
                    break;
                }

                v.ExitedFromHolder = null; // 经过了其他容器，来源标记失效

                // 颜色容器对应的咖啡杯不能进普通容器；同一交点可能还有目标颜色容器，跳过继续检查
                if (_holderMgr.IsColorTargetOfFilteredMachine(v.ColorType) && !holder.HasColorFilteredSpecial())
                    continue;

                var worldPos = SkipHolderEntry(holder, v)
                    ? Vector3.zero
                    : holder.AcceptVehicle(v, _vehicleMgr.CountSameColorBehind(v));
                if (worldPos == Vector3.zero) continue; // 被拒 跳过：同交点其他容器仍有机会接收

                v.OriginHolder = null; // 已进入新容器，来源标记失效
                _vehicleMgr.OnCarEnteredHolder();
                RefreshRoadColor();
                var targetRot = holder.Info.DirectionValue.HasValue
                    ? Quaternion.Euler(0f, holder.Info.DirectionValue.Value, 0f)
                    : Quaternion.identity;

                v.EnterHolder(worldPos, targetRot);
                break; // 成功进入，消费本次穿越
            }
        }

        // 是否跳过该容器
        private bool SkipHolderEntry(Holder holder, Vehicle v)
        {
            return IsAutoCompleting && holder.IsEmpty() && _holderMgr.HasPendingColorDemand(v.ColorType);
        }

        // 从所有出口队列中移除指定车辆 撤销用
        public void RemoveFromPendingExits(Vehicle car)
        {
            foreach (var queue in _pendingExits.Values) queue.Remove(car);
        }

        // 从出口队列反查车辆归属容器
        private Holder GetPendingExitHolder(Vehicle vehicle)
        {
            foreach (var kv in _pendingExits)
                if (kv.Value.Contains(vehicle))
                    return kv.Key;
            return null;
        }
    }
}