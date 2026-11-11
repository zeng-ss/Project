namespace CasualGame.CoffeeAway
{
    public enum VehicleState
    {
        InHolder,
        WaitingExit,
        Exiting,
        OnRoad,
        Entering,
        Teleporting // 传送
    }

    /// <summary>
    /// 车辆门面：负责数据、生命周期与状态管理
    /// </summary>
    public partial class Vehicle : AttachPoolPrefabBase
    {
        private VehicleType _vehicleType;

        #region public

        // ---- 数据 ----
        public VehicleType VehicleType => _vehicleType;
        public ColorType ColorType { get; private set; }
        public int HolderRowIndex { get; set; }
        public int HolderColIndex { get; set; }
        public VehicleState State => CurrentState;

        // ---- 移动数据 ----
        public float RoadDistance => _roadDistance;
        public float PrevRoadDistance => _prevRoadDistance;
        public float? OccupyDistance => _occupyDistance;

        public Holder ExitedFromHolder { get; set; }

        // 与 ExitedFromHolder 的区别：后者在车跨过任意容器入口即失效 此标记保留到真正进容器
        public Holder OriginHolder { get; set; }

        public int OriginEmptyAtPop { get; set; } = -1; // 弹出瞬间来源容器的空位数
        public int OriginPopCount { get; set; } // 弹出瞬间本次弹出组大小

        public bool IsOriginReentryNoOp(Holder holder)
        {
            // 回到来源容器：若容器当前唯一的变化就是本组车被移走 空位 = 弹出时空位 + 组大小
            // 则重进等于回到原状，是无意义循环，返回 true 拒收
            if (OriginHolder is null || holder != OriginHolder || OriginEmptyAtPop < 0) return false;
            return holder.EmptySlotCount() == OriginEmptyAtPop + OriginPopCount;
        }

        public bool IsBlocked { get; set; }

        #endregion

        public void Init(LevelBuilder.CarInfo carInfo)
        {
            ColorType = (ColorType)(carInfo.UnitData?.Id?.Color ?? 0);
            InitInternal(carInfo.UnitData?.GetVehicleType() ?? VehicleType.Normal);
        }

        // 问号杯揭开时以真实颜色预制体重新初始化
        public void InitAsReplacement(ColorType color)
        {
            ColorType = color;
            InitInternal(VehicleType.Normal);
        }

        private void InitInternal(VehicleType vehicleType)
        {
            _vehicleType = vehicleType;
            ChangeState(VehicleState.InHolder);
            m_HideUnlock?.gameObject.SetActive(false);
        }

        // 换类型入口
        public virtual void SetVehicleType(VehicleType type)
        {
            _vehicleType = type;
        }

        public override void DeSpawn()
        {
            _vehicleType = VehicleType.Normal;
            ResetView();
            Reset();
            RefreshRenderLayer();
        }
    }
}