using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CasualGame.CoffeeAway
{
    /// <summary>
    /// 车辆管理器
    /// </summary>
    public class VehicleManager
    {
        #region Fields

        private const float FastSpeedMultiplier = 1.8f;
        private readonly List<RoadSurfaceMove> _roadSurfaces = new();
        private RoadNavigator _roadNav;
        private ReceiptPrinter _receiptPrinter;
        private float _carRoadSpeed;
        private bool _isCloseConveyor;
        private float _minCarGap; // 路上车辆最小安全跟车距离
        private float _curMinCarGap; // 用于咖啡之间的间距检测
        private int _showCarsOnRoad; // 显示用路上车数 排队未发车不计入显示
        private float _exitDuration; // 出场动画时长，仅决定跳跃动画速度
        private float _exitFrontClearance; // 出场时前车最小净空间
        private float _frontClearanceDuration; // 走过 _exitFrontClearance 需要的时间

        #endregion

        #region Properties

        public List<Vehicle> Vehicles { get; } = new();

        public int ConveyorCapacity { get; private set; }

        // 已点出但尚未进容器的车数 含排队未发车，用于容量检查
        public int CarsOnRoad { get; private set; }

        public bool IsFastForward { get; private set; }

        public bool IsSelectingFrozen { get; private set; }

        // 前车让出出口区所需时间：排队待命动画的节奏单位
        public float FrontClearanceDuration => _frontClearanceDuration;

        // 出场动画总时长：动画期间前车恰好前进一个最小间距，落地时道路间距相等
        public float ExitAnimDuration => _exitDuration + _frontClearanceDuration;

        #endregion

        #region Lifecycle

        // 关卡初始化：注入车辆、道路数据
        public void Setup(List<Vehicle> vehicles, List<RoadSurfaceMove> roadSurfaces,
            ReceiptPrinter receiptPrinter, RoadNavigator roadNav, int capacity,
            float carRoadSpeed, bool isCloseConveyor)
        {
            Vehicles.AddRange(vehicles);
            _roadSurfaces.AddRange(roadSurfaces);
            _roadNav = roadNav;
            _receiptPrinter = receiptPrinter;
            ConveyorCapacity = capacity;
            _carRoadSpeed = carRoadSpeed;
            _isCloseConveyor = isCloseConveyor;
            _minCarGap = roadNav.TotalLength / capacity;
            _curMinCarGap = _minCarGap;
            _receiptPrinter.UpdateText($"{_showCarsOnRoad}/{ConveyorCapacity}");
            UpdateExitParams();
        }

        // 清空逻辑状态
        public void Clear()
        {
            _roadNav = null;
            _receiptPrinter = null;
            Vehicles.Clear();
            _roadSurfaces.Clear();
            CarsOnRoad = 0;
            _showCarsOnRoad = 0;
            IsFastForward = false;
            IsSelectingFrozen = false;
        }

        public void Tick(float deltaTime)
        {
            UpdateCarFollowing();
            foreach (var v in Vehicles) v.ManualTick(deltaTime);
            foreach (var v in Vehicles.Where(v => v.State == VehicleState.OnRoad))
            {
                CheckPortal(v);
            }
        }

        #endregion

        #region Car Following & Exit Timing

        // 出场动画时长：仅决定跳跃动画速度，不参与发车间距判断
        private void UpdateExitParams()
        {
            if (_minCarGap >= 5) _carRoadSpeed = 22;
            _exitDuration = (_minCarGap - LevelBuilder.HalfCarDepthSpacing) / _carRoadSpeed;
            _exitFrontClearance = Mathf.Max(0f, _minCarGap - _exitDuration * _carRoadSpeed);
            _frontClearanceDuration = _exitFrontClearance / _carRoadSpeed;
        }

        // 硬跟车
        private void UpdateCarFollowing()
        {
            var onRoadCars = Vehicles.Where(v =>
                v.State is VehicleState.OnRoad or VehicleState.Exiting or VehicleState.WaitingExit).ToList();

            float baseSpeed = IsFastForward ? _carRoadSpeed * FastSpeedMultiplier : _carRoadSpeed;

            if (onRoadCars.Count <= 1)
            {
                foreach (var v in Vehicles)
                {
                    v.IsBlocked = false;
                    if (!IsSelectingFrozen && v.State == VehicleState.OnRoad)
                        v.SetMoveSpeed(baseSpeed);
                }

                return;
            }

            onRoadCars.Sort((a, b) =>
            {
                float distanceA = a.OccupyDistance ?? a.RoadDistance;
                float distanceB = b.OccupyDistance ?? b.RoadDistance;
                return distanceA.CompareTo(distanceB);
            });

            for (int i = 0; i < onRoadCars.Count; i++)
            {
                var curCar = onRoadCars[i];
                if (curCar.State != VehicleState.OnRoad) continue;

                float curDist = curCar.OccupyDistance ?? curCar.RoadDistance;
                float bestGap = float.MaxValue;

                // 向前扫描找第一辆真车
                for (int j = 1; j < onRoadCars.Count; j++)
                {
                    var ahead = onRoadCars[(i + j) % onRoadCars.Count];
                    float aheadDist = ahead.OccupyDistance ?? ahead.RoadDistance;
                    float gap = aheadDist - curDist;
                    if (gap < 0f) gap += _roadNav.TotalLength;

                    // WaitingExit 占位车仍在容器内等待出场，尚未进入道路，不参与跟车阻挡；
                    // 出场安全间距由 CanStartExitAt 单独保证，避免占位车挡停路上车造成互相等待
                    if (ahead.State == VehicleState.WaitingExit) continue;

                    bestGap = gap;
                    break;
                }

                // 距离一到最小值就立即停下，否则满速
                bool blocked = bestGap < _curMinCarGap;
                curCar.IsBlocked = blocked;
                if (!IsSelectingFrozen)
                    curCar.SetMoveSpeed(blocked ? 0f : baseSpeed);
            }
        }

        // entryDist 处前后间距是否满足安全条件
        public bool CanStartExitAt(float entryDist)
        {
            foreach (var v in Vehicles)
            {
                if (v.State != VehicleState.OnRoad && v.State != VehicleState.Exiting) continue;

                // 前向：移动中的车落地时已被前车位移拉开（出场动画时长恰好 = 前车前进一个最小间距的时间），
                // 因此只要前车在移动即可放行，不必等出空隙；被阻塞/正在跳出的车位置固定，要求车身净空。
                float gap = v.RoadDistance - entryDist;
                if (gap < 0f) gap += _roadNav.TotalLength;

                if (v.State == VehicleState.OnRoad)
                {
                    float clearance = v.IsBlocked ? LevelBuilder.CarWidthSpacing : 0f;
                    if (gap < clearance) return false;
                }
                else if (gap < LevelBuilder.CarWidthSpacing) // 其他容器正在跳出的车位置固定
                {
                    return false;
                }

                // 后向：出口点后方半车身范围内有真实车辆则等待其先通过
                float gapBehind = entryDist - v.RoadDistance;
                if (gapBehind < 0f) gapBehind += _roadNav.TotalLength;
                if (gapBehind < LevelBuilder.HalfCarDepthSpacing) return false;
            }

            return true;
        }

        public void StartSingleExit(Vehicle car, float entryDist, Action<Vehicle> onLanded)
        {
            _receiptPrinter.UpdateText($"{++_showCarsOnRoad}/{ConveyorCapacity}");

            float startDist = entryDist;
            Vector3 startPos = _roadNav.GetPosition(startDist);
            Vector3 roadDir = _roadNav.GetDirection(startDist);
            float moveSpeed = IsFastForward ? _carRoadSpeed * FastSpeedMultiplier : _carRoadSpeed;
            car.ExitHolder(_roadNav, startDist, startPos, roadDir, ExitAnimDuration, moveSpeed, () =>
            {
                onLanded?.Invoke(car); // 落地之后 Holder Grid 再移除 Vehicle
            });
        }

        #endregion

        #region Road Events

        // 车辆进入容器后扣除道路计数
        public void OnCarEnteredHolder()
        {
            CarsOnRoad--;
            _curMinCarGap = _minCarGap;
            _receiptPrinter.UpdateText($"{--_showCarsOnRoad}/{ConveyorCapacity}");
        }

        // 传送门检测
        private void CheckPortal(Vehicle v)
        {
            if (_isCloseConveyor) return; // 闭合路径无缝环绕，不需要传送门
            if (_roadNav.CrossedDistance(v.PrevRoadDistance, v.RoadDistance, 0f))
            {
                v.StartTeleport(v.RoadDistance, _roadNav.GetPosition(v.RoadDistance),
                    _roadNav.GetDirection(v.RoadDistance));
            }
        }

        // 马路上指定车辆连续同色车辆数，isHead 表示连续段头部
        public (int count, bool isHead) CountSameColorBehind(Vehicle vehicle)
        {
            if (vehicle.State != VehicleState.OnRoad && vehicle.State != VehicleState.Exiting)
                return (0, false);

            var onRoadCars = Vehicles
                .Where(v => v.State is VehicleState.OnRoad or VehicleState.Exiting)
                .OrderBy(v => v.RoadDistance).ToList();

            if (onRoadCars.Count == 0) return (0, false);

            int idx = onRoadCars.IndexOf(vehicle);
            if (idx < 0) return (0, false);

            // 前方紧邻车辆（环形），判断是否为连续段头部
            int aheadIdx = (idx + 1) % onRoadCars.Count;
            bool isHead = onRoadCars.Count == 1 || onRoadCars[aheadIdx].ColorType != vehicle.ColorType;

            // 环形向后扫描，遇不同色停止
            int count = 1;
            for (int i = 1; i < onRoadCars.Count; i++)
            {
                int behindIdx = (idx - i + onRoadCars.Count) % onRoadCars.Count;
                if (onRoadCars[behindIdx].ColorType != vehicle.ColorType) break;
                count++;
            }

            return (count, isHead);
        }

        #endregion

        #region Public API

        public void AddCapacity(int count)
        {
            ConveyorCapacity += count;
            _receiptPrinter.UpdateText($"{_showCarsOnRoad}/{ConveyorCapacity}");
            _minCarGap = _roadNav.TotalLength / ConveyorCapacity;
            _curMinCarGap = _minCarGap;
            UpdateExitParams();
        }

        public void AddCarsOnRoad(int count) => CarsOnRoad += count;

        // 道具/撤销：从道路上移除车辆，同步容量计数、显示计数与小票机。
        // capacityCount 为占用容量的车数（含排队未发车）；showCount 为已在道路上显示的车数（不含排队未发车）
        public void RemoveCarsOnRoad(int capacityCount, int showCount)
        {
            CarsOnRoad = Mathf.Max(0, CarsOnRoad - capacityCount);
            _showCarsOnRoad = Mathf.Max(0, _showCarsOnRoad - showCount);
            _curMinCarGap = _minCarGap;
            _receiptPrinter.UpdateText($"{_showCarsOnRoad}/{ConveyorCapacity}");
        }

        public bool IsShouldRoadRed() => CarsOnRoad >= ConveyorCapacity - 2;

        public bool IsCapacityFull() => CarsOnRoad >= ConveyorCapacity;

        public void ReduceCarMinCarGap() => _curMinCarGap -= 0.5f;

        public bool HasPendingOrExitingCar()
        {
            foreach (var v in Vehicles)
                if (v.State is VehicleState.WaitingExit or VehicleState.Exiting)
                    return true;
            return false;
        }

        // 加速/恢复：必胜局面下加速路上车辆
        public void SetFastForward(bool on)
        {
            IsFastForward = on;
            foreach (var v in Vehicles)
            {
                if (v.State is VehicleState.OnRoad or VehicleState.Exiting or VehicleState.WaitingExit)
                    v.SetMoveSpeed(on ? _carRoadSpeed * FastSpeedMultiplier : _carRoadSpeed);
            }
        }

        // 冻结：道具选目标/失败时停住所有车辆与出场调度
        public void FreezeAll()
        {
            IsSelectingFrozen = true;
            foreach (var v in Vehicles)
            {
                v.SetMoveSpeed(0f);
                v.PauseAnim();
            }

            foreach (var roadSurface in _roadSurfaces) roadSurface.Stop();
        }

        // 恢复：解冻所有车辆与路面
        public void UnfreezeAll()
        {
            IsSelectingFrozen = false;
            foreach (var v in Vehicles)
            {
                if (v.State is VehicleState.OnRoad or VehicleState.Exiting or VehicleState.WaitingExit)
                    v.SetMoveSpeed(_carRoadSpeed);
                v.ResumeAnim();
            }

            foreach (var roadSurface in _roadSurfaces) roadSurface.Move();
        }

        #endregion
    }
}