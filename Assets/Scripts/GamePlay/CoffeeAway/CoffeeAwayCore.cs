using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace CasualGame.CoffeeAway
{
    /// <summary>
    /// CarSort 规则编排器
    /// </summary>
    public partial class CoffeeAwayCore
    {
        #region Fields

        private readonly VehicleManager _vehicleMgr = new();
        private readonly HolderManager _holderMgr = new();
        private readonly Dictionary<Holder, List<Vehicle>> _pendingExits = new();
        private RoadNavigator _roadNav;
        private bool _hasContinue;
        private bool _isShowingExtraHolder;
        private bool _isRoadFullPending; // 满载等待死局判定
        private Action<bool> _onRoadColorChanged; // 道路变红回调：进出车/扩容时同步刷新

        #endregion

        #region Properties

        public int ConveyorCapacity => _vehicleMgr.ConveyorCapacity;
        public int CarsOnRoad => _vehicleMgr.CarsOnRoad;
        public List<Holder> Holders => _holderMgr.Holders;
        public List<Vehicle> Vehicles => _vehicleMgr.Vehicles;
        public Random Rng { get; } = new();

        public static bool IsHighlightActive =>
            HolderHighlightController.Instance != null && HolderHighlightController.Instance.IsActive;

        // 玩家已无需操作、等待车辆驶入容器即可获胜：加速路上车辆并禁止点击容器
        public bool IsAutoCompleting => _vehicleMgr.IsFastForward;
        public bool IsShowingExtraHolder => _isShowingExtraHolder;
        public UndoHistory UndoHistory { get; } = new();

        #endregion

        #region Lifecycle

        public void Setup(List<Holder> holders, List<Vehicle> vehicles, List<RoadSurfaceMove> roadSurfaces,
            ReceiptPrinter receiptPrinter, RoadNavigator roadNav, int capacity,
            float carRoadSpeed, bool isCloseConveyor)
        {
            Reset();
            _roadNav = roadNav;
            _holderMgr.Setup(holders);
            _vehicleMgr.Setup(vehicles, roadSurfaces, receiptPrinter, roadNav, capacity, carRoadSpeed,
                isCloseConveyor);

            foreach (var h in holders)
            {
                if (h.SpecialHolders == null) continue;
                foreach (var s in h.SpecialHolders) s?.OnSetup(this);
            }
        }

        // 清空逻辑状态
        public void Reset()
        {
            _pendingExits.Clear();
            _holderMgr.Clear();
            _vehicleMgr.Clear();
            UndoHistory.Clear();
            _hasContinue = false;
            _isRoadFullPending = false;
        }

        #endregion

        #region Core Logic

        public bool Tick()
        {
            _vehicleMgr.Tick(Time.deltaTime);
            ProcessPendingExits();

            // 先判定加速：让空容器在本帧起对"有归属颜色"拒收，防止车被空容器截胡
            CheckImminentWin();

            foreach (var v in _vehicleMgr.Vehicles)
            {
                if (v.State != VehicleState.OnRoad) continue;
                CheckHolderEntry(v);
            }

            _holderMgr.Tick();
            TickRoadFullFail();
            return CheckWin();
        }

        public void OnHolderTapped(Holder holder, Action<bool> setRoadColor)
        {
            if (setRoadColor != null) _onRoadColorChanged = setRoadColor;
            int rowCount = holder.GetFrontSameColorVehicleCount();
            if (rowCount == 0) return;
            if (holder.IsBusy)
            {
                Log.Info("Busy!");
                return;
            }

            if (_pendingExits.ContainsKey(holder)) return;

            var exitVehicles = holder.PeekExitVehicleCells();
            if (exitVehicles.Count == 0) return;

            var available = _vehicleMgr.ConveyorCapacity - _vehicleMgr.CarsOnRoad;
            if (available <= 0)
            {
                // 道路已满
                _isRoadFullPending = true;
                return;
            }

            int exitCount = Mathf.Min(exitVehicles.Count, available);

            // 记录弹出瞬间来源容器的空位数与弹出组大小，供"回到来源容器是否无意义循环"判定
            int emptyAtPop = holder.EmptySlotCount();

            // 只对实际放出的 exitCount 辆生成撤销记录
            var undoRecord = new UndoRecord { SourceHolder = holder };
            var vehicleCells = new List<Vehicle>(exitCount);
            for (int i = 0; i < exitCount; i++)
            {
                var (v, c, r) = exitVehicles[i];
                v.OriginEmptyAtPop = emptyAtPop;
                v.OriginPopCount = exitCount;
                undoRecord.Cars.Add(new CarUndoEntry(v, c, r));
                vehicleCells.Add(v);
            }

            UndoHistory.PushMove(undoRecord);
            _vehicleMgr.AddCarsOnRoad(exitCount);
            RefreshRoadColor();

            // 登记为待出场队列，由 ProcessPendingExits 逐辆放行
            _pendingExits[holder] = vehicleCells;
            foreach (var vehicle in vehicleCells) vehicle.ReserveRoadPosition(holder.RoadConnectionDistance);
            CoffeeAwayGame.Instance.ShouldBackReduceEnergy();

            if (_vehicleMgr.IsCapacityFull()) _isRoadFullPending = true;
        }

        // 刷新道路红/正常色
        private void RefreshRoadColor() => _onRoadColorChanged?.Invoke(_vehicleMgr.IsShouldRoadRed());

        private void CheckImminentWin()
        {
            if (_vehicleMgr.IsFastForward) return;
            if (!IsShouldUpdateMoveSpeed()) return;
            _vehicleMgr.SetFastForward(true);
        }

        // 判定是否无需玩家操作即可获胜
        // 注意：auto-complete 模式下空容器会拒收"有未满容器待补"的颜色（见 SkipHolderEntry）
        private bool IsShouldUpdateMoveSpeed()
        {
            var roadCars = _vehicleMgr.Vehicles
                .Where(v => v.State is VehicleState.OnRoad or VehicleState.Exiting or VehicleState.WaitingExit)
                .ToList();
            if (roadCars.Count == 0) return false;

            // 额外容器已出现且仍有车时，玩家仍需手动整理额外容器，不自动完成，避免锁定点击
            if (_holderMgr.IsExtraHolderActiveAndNotEmpty()) return false;

            // 统计每个待补容器还需多少同色车；空容器作为"可选吸收槽"：可被填满，也可保持空
            if (!_holderMgr.TryCollectDemand(out var emptyHolders, out int emptyCapacity, out var demand))
                return false;

            var supply = new Dictionary<ColorType, int>();
            foreach (var v in roadCars)
                supply[v.ColorType] = supply.TryGetValue(v.ColorType, out var n) ? n + 1 : 1;

            // 每种待补颜色的路上车数必须足够填满对应容器
            foreach (var kv in demand)
            {
                int s = supply.GetValueOrDefault(kv.Key, 0);
                if (s < kv.Value) return false;
            }

            // 未被同色容器吸收的多余车必须全部同色
            ColorType? surplusColor = null;
            int surplus = 0;
            foreach (var kv in supply)
            {
                int d = demand.GetValueOrDefault(kv.Key, 0);
                if (kv.Value <= d) continue; // 恰好填满同色容器，无多余

                surplus += kv.Value - d;
                if (surplusColor.HasValue && surplusColor.Value != kv.Key) return false;
                surplusColor = kv.Key;
            }

            if (emptyCapacity == 0) return surplus == 0;

            // 无多余车：路上车恰好补全所有部分填充容器
            if (surplus == 0) return true;

            // 多余车同色 空容器填满
            return surplusColor.HasValue && _holderMgr.CanFillEmptyHolders(surplus, emptyHolders, surplusColor.Value);
        }

        private bool CheckWin()
        {
            if (_holderMgr.Holders.Count == 0) return false;
            if (_vehicleMgr.Vehicles.Count == 0) return true;

            // 所有车辆都必须已停进容器
            if (_vehicleMgr.Vehicles.Any(v => v.State != VehicleState.InHolder))
            {
                return false;
            }

            // 所有容器要么同色满，要么全空
            if (!_holderMgr.AllSettled()) return false;

            Log.Info("胜利！");
            return true;
        }

        #endregion

        #region Road Full & Dead End

        // 满载等待判定：道路被填满后，等所有车落地，再检测是否死局
        private void TickRoadFullFail()
        {
            if (!_isRoadFullPending) return;
            if (_vehicleMgr.IsSelectingFrozen) return;
            if (_vehicleMgr.HasPendingOrExitingCar()) return;
            if (_vehicleMgr.IsCapacityFull())
            {
                if (IsDeadEnd())
                {
                    StopAllVehicles();
                    _isRoadFullPending = false; // 已触发弹窗，防止每帧重复
                    if (!_hasContinue)
                    {
                        UGUIParams uiParams = UGUIParams.Create();
                        uiParams.AddDelegage("OnContinue", (_params) =>
                        {
                            _hasContinue = true;
                            _isRoadFullPending = true; // 购买继续后，额外容器出现，重新进入满载判定
                        });
                        Framework.UIMgr.Instance.OpenUIForm(UIFormType.ContinueUIForm, uiParams);
                    }
                    else
                    {
                        Framework.UIMgr.Instance.OpenUIForm(UIFormType.FailUIForm);
                    }
                }
                else _vehicleMgr.ReduceCarMinCarGap(); // 满就减少间距防止走走停停

                return;
            }

            // 道路已有空余容量，解除满载等待
            _isRoadFullPending = false;
        }

        // 死局：道路上所有车辆没有一辆能进入任意非来源容器
        private bool IsDeadEnd()
        {
            bool anyRoadCar = false;
            foreach (var v in _vehicleMgr.Vehicles.Where(v => v.State == VehicleState.OnRoad))
            {
                anyRoadCar = true;
                if (_holderMgr.CanAnyHolderAccept(v)) return false;
            }

            return anyRoadCar; // 路上没有车不算死局
        }

        #endregion

        #region Prop API

        public void AddCapacity(int count)
        {
            _vehicleMgr.AddCapacity(count);
            RefreshRoadColor();
        }

        // 消除/撤销路上车辆：同步容量计数、显示计数与小票机
        public void RemoveCarsOnRoad(int capacityCount, int showCount)
        {
            _vehicleMgr.RemoveCarsOnRoad(capacityCount, showCount);
            RefreshRoadColor();
        }

        private Holder FindHolderForVehicle(Vehicle vehicle)
        {
            if (vehicle is null) return null;

            // InHolder / Entering：AcceptVehicle 已 Place 进 Grid，可直接查归属
            if (vehicle.State is VehicleState.InHolder or VehicleState.Entering)
            {
                foreach (var holder in _holderMgr.Holders)
                {
                    if (holder.ContainsVehicle(vehicle))
                        return holder;
                }
            }

            if (vehicle.State == VehicleState.WaitingExit)
            {
                return GetPendingExitHolder(vehicle);
            }

            return vehicle.ExitedFromHolder;
        }

        public int GetVehicleRenderLayer(Vehicle vehicle)
        {
            if (vehicle is null || vehicle.State is VehicleState.OnRoad) return Constant.Layer.BgLayerId;

            if (vehicle.State is VehicleState.InHolder or VehicleState.WaitingExit or VehicleState.Entering)
            {
                var holder = FindHolderForVehicle(vehicle);
                if (holder is not null && (holder.ShouldHighlightForShuffle() || holder.ForceHighlight))
                    return Constant.Layer.HolderLayerId;
                return Constant.Layer.BgLayerId;
            }

            return Constant.Layer.HolderLayerId;
        }

        public void RefreshAllRenderLayers()
        {
            _holderMgr.RefreshRenderLayers();
            foreach (var vehicle in _vehicleMgr.Vehicles) vehicle.RefreshRenderLayer();
        }

        public bool ShouldShowShuffleHighlight() => _holderMgr.ShouldShowShuffleHighlight();

        // 供打乱道具查询其他容器当前可接受的颜色，优先交换到前排保证有解
        public HashSet<ColorType> GetAcceptableColorsForShuffle(Holder sourceHolder) => _holderMgr.GetAcceptableColors(sourceHolder);

        #endregion

        #region Special Mechanics API

        // 盖子解锁条件：目标颜色车辆必须全部安稳停在容器内，且所在容器都已铺满该颜色
        public bool IsColorCompleted(ColorType color, Holder exceptSelf)
        {
            foreach (var v in _vehicleMgr.Vehicles)
            {
                if (v.ColorType != color) continue;
                if (v.State != VehicleState.InHolder) return false;
            }

            // 所有含目标颜色的容器都必须已铺满
            return _holderMgr.IsColorFullySettled(color, exceptSelf);
        }

        // 广播：任意容器完成时，通知所有容器的特殊玩法组件
        public void NotifyContainersCompleted() => _holderMgr.NotifyContainersCompleted();

        #endregion

        #region UI API

        public void ShowExtraHolder()
        {
            if (!_holderMgr.TryGetExtraHolder(out var extra)) return;
            _vehicleMgr.SetFastForward(false); // 额外容器出现后回到手动阶段，否则必胜加速会拒收所有进额外容器的车
            _isShowingExtraHolder = true;
            extra.DoShowAni(this, () => _isShowingExtraHolder = false);
        }

        public void StopAllVehicles() => _vehicleMgr.FreezeAll();

        public void ResetAllVehiclesMoveSpeed() => _vehicleMgr.UnfreezeAll();

        #endregion
    }
}
