using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityGameFramework.Runtime;
using Random = System.Random;

namespace CasualGame.CoffeeAway
{
    /// <summary>
    /// 容器基类：组合 HolderGrid、进出场编排、特殊玩法拦截、通关检测与生命周期。
    /// </summary>
    public abstract class Holder : AttachPoolPrefabBase
    {
        #region private

        private BoxCollider _collider;
        private int _remainEnteringCount;

        #endregion

        #region public

        public LevelBuilder.HolderInfo Info { get; private set; }
        protected HolderGrid Grid { get; private set; }
        public Vector3 FrontFaceCenter { get; private set; }
        public Vector3 ExitDirection { get; private set; }
        public float RoadConnectionDistance { get; set; }
        public Vector3 RoadEntryPoint { get; set; }
        public bool IsBusy => _remainEnteringCount > 0 || (Grid != null && Grid.HasMovingVehicle());
        public List<ISpecialMachine> SpecialHolders { get; private set; }
        public bool IsFullSameColor { get; private set; }

        // 首关指引期间强制高亮
        public bool ForceHighlight { get; set; }
        public bool IsCompleteCheckEnded { get; protected set; }
        public Renderer RoadView => _roadView;

        // 容器来自哪个对象池，回收时按来源池归还（颜色容器专用预制体来自 ColorHolderPool）
        public AttachParentPrefabPool SourcePool { get; set; }
        public void DeSpawnToSourcePool() => (SourcePool ?? SpawnFactory.HolderPool).DeSpawn(this);

        #endregion

        private Transform _frontPoint;
        private Renderer _roadView;

        public virtual void Init(LevelBuilder.HolderInfo info, List<ISpecialMachine> specials = null)
        {
            Info = info;
            Grid = new HolderGrid(info);
            _remainEnteringCount = 0;
            _collider ??= GetComponent<BoxCollider>();
            _collider.enabled = true;
            IsFullSameColor = false;
            ForceHighlight = false;
            IsCompleteCheckEnded = false;
            SpecialHolders = specials ?? new List<ISpecialMachine>();

            _frontPoint = transform.Find("FrontPoint");
            _roadView = transform.Find("Road_All")?.GetComponent<Renderer>();
            if (_frontPoint is null)
            {
                Log.Error("Front Point is null.");
                return;
            }

            FrontFaceCenter = _frontPoint.position;
            var dir = _frontPoint.position - transform.position;
            dir.y = 0f;
            ExitDirection = dir.normalized;
        }

        #region Grid 透传

        // 加入 Vehicle
        public void PlaceVehicle(Vehicle v, int col, int row, bool isInit = false) =>
            Grid.PlaceVehicle(v, col, row, isInit);

        // 获取网格指定格子的世界坐标
        public Vector3 GetCellWorldPos(int col, int row) => Grid.GetCellWorldPos(col, row);

        // 容器内是否包含指定颜色的车辆
        public bool ContainsColor(ColorType color) => Grid.ContainsColor(color);

        public bool IsEmpty() => Grid.IsEmpty();

        // 容器内空位数量
        public int EmptySlotCount() => Grid.EmptySlotCount();

        // 随机打乱：前置校验容器状态后委托给网格算法
        public List<Vehicle> Shuffle(Random rng, out string hint, HashSet<ColorType> preferredColors = null)
        {
            if (IsBusy)
            {
                hint = "容器正在进出车辆，无法打乱";
                return null;
            }

            if (IsFullSameColor)
            {
                hint = "容器已完成排序，无需打乱";
                return null;
            }

            return Grid.Shuffle(rng, out hint, preferredColors);
        }

        // 拿到移除指定颜色的所有车辆
        public List<Vehicle> GetRemoveCarsAndUpdateGrid(ColorType color) => Grid.GetRemoveCarsAndUpdateGrid(color);

        public List<ColorType> GetColorsInHolder() => Grid.GetColorsInHolder();

        // 前排颜色 空容器返回 null
        public ColorType? GetFrontColor() => Grid.GetFrontColor();

        // 将剩余的车子移进容器后部
        public void MoveRemainVehicleToBack(Action onComplete = null) => Grid.MoveRemainVehicleToBack(onComplete);

        // 获取前排相同颜色的车数量
        public int GetFrontSameColorVehicleCount()
        {
            if (!CanTap()) return 0;
            return Grid.GetFrontSameColorVehicleCount();
        }

        public void SetFrontOutLine()
        {
            if (AnySpecialBlocks(s => !s.CanPopExit()))
            {
                return;
            }

            Grid.SetFrontOutLine();
        }

        // 拿到需要弹出的 Vehicle 集合
        public List<(Vehicle vehicle, int col, int row)> PeekExitVehicleCells() =>
            AnySpecialBlocks(s => !s.CanPopExit()) ? new List<(Vehicle, int, int)>() : Grid.PeekExitVehicleCells();

        public Vehicle GetVehicleAt(int col, int row) => Grid?.GetVehicleAt(col, row);

        public void RemoveVehicleFromGrid(Vehicle v) => Grid.RemoveVehicle(v);

        // 车辆出去之后回调
        public void OnCarExited(Vehicle car, bool isLast)
        {
            RemoveVehicleFromGrid(car);
            if (isLast)
            {
                RefreshHiddenVehicleDisplay();
            }
        }

        public void RefreshHiddenVehicleDisplay() => Grid.RefreshHiddenVehicleDisplay();

        public bool ContainsVehicle(Vehicle v) => Grid?.ContainsVehicle(v) ?? false;

        public bool CanTap() => !AnySpecialBlocks(s => !s.CanTap());

        public bool ShouldHighLight()
        {
            if (!gameObject.activeSelf) return false;
            if (Grid.IsPlaceSameColor().Item1 || IsBusy || IsEmpty()) return false;
            return CanTap();
        }

        // Shuffle 选目标高亮：可点击、非同色整理完毕、无问号车
        public bool ShouldHighlightForShuffle()
        {
            if (!gameObject.activeSelf) return false;
            // 首关指引期间只允许 ForceHighlight 的目标容器高亮
            //if (CoffeeAwayGame.Instance.IsGuideActive) return false;
            if (!CoffeeAwayCore.IsHighlightActive) return false;
            if (Grid.IsPlaceSameColor().Item1 || IsBusy || IsEmpty()) return false;
            return CanTap();
        }

        public void RefreshRenderLayer()
        {
            int layer = ForceHighlight || ShouldHighlightForShuffle()
                ? Constant.Layer.HolderLayerId
                : Constant.Layer.BgLayerId;
            gameObject.SetLayerRecursively(layer);
        }

        #endregion

        #region Special Holder Need

        // 任一特殊玩法拦截则整个容器拦截
        private bool AnySpecialBlocks(System.Func<ISpecialMachine, bool> gate)
        {
            if (SpecialHolders == null) return false;
            foreach (var s in SpecialHolders)
            {
                if (s is null) continue;
                if (gate(s)) return true;
            }

            return false;
        }

        public bool HasColorFilteredSpecial() => this is ColorFilteredMachine;

        // 特殊玩法完成使命后自行移除
        public void RemoveSpecial(ISpecialMachine special) => SpecialHolders?.Remove(special);

        // 特殊玩法未解锁或已完成排序的容器，消除道具不得影响
        public bool IsEliminationBlocked()
        {
            if (IsFullSameColor) return true;
            return AnySpecialBlocks(s => !s.CanTap() || !s.CanPopExit());
        }

        #endregion

        #region Entry

        // 接受 Vehicle
        public Vector3 AcceptVehicle(Vehicle vehicle, (int count, bool isHead) carState)
        {
            if (!CanAcceptVehicle(vehicle.ColorType))
            {
                ReleaseEntryCommit();
                return Vector3.zero;
            }

            if (carState.isHead) _remainEnteringCount = carState.count;

            Vector3 pos = Grid.PlaceToFirstEmpty(vehicle);
            ReleaseEntryCommit();
            return pos;
        }

        // 被拒绝的车不会进入本容器，释放一个进入占位，避免 _remainEnteringCount 卡死导致 IsBusy 永远为 true
        private void ReleaseEntryCommit()
        {
            if (_remainEnteringCount > 0) _remainEnteringCount--;
        }

        protected virtual bool CanAcceptVehicle(ColorType colorType)
        {
            return !AnySpecialBlocks(s => !s.CanAcceptVehicle(colorType)) && Grid.CanAcceptAtFront(colorType);
        }

        // 供全局规则查询容器是否可接受某颜色车辆
        public bool CanAcceptColor(ColorType colorType) => CanAcceptVehicle(colorType);

        #endregion

        #region Complete

        // 是否正在等待一定数量的相同颜色的咖啡
        public (bool, int) IsWaitingVehicleToComplete()
        {
            if (IsFullSameColor) return (false, 0);
            var state = Grid.IsPlaceSameColor();
            return (state.Item1, state.Item2);
        }

        protected virtual void CheckComplete()
        {
            if (IsFullSameColor) return;
            if (!Grid.IsFullSameColor()) return;
            IsFullSameColor = true;
            // 无完成序列的容器完成即视为序列结束；StandardHolder 播放打包动画时会置回 false 等待动画结束
            IsCompleteCheckEnded = true;
            _collider.enabled = false;
            CoffeeAwayGame.Instance?.Core?.RefreshAllRenderLayers();
            CoffeeAwayGame.Instance?.Core?.NotifyContainersCompleted();
        }

        #endregion

        public virtual void ManualTick() => CheckComplete();

        public override void DeSpawn()
        {
            _remainEnteringCount = 0;
            if (SpecialHolders is null) return;
            foreach (var s in SpecialHolders.Where(s => s is not null))
            {
                // 有模型
                if (s is HolderSpecialCtrl ctrl)
                {
                    SpawnFactory.HolderPool.DeSpawn(ctrl);
                }
                else
                {
                    // 无模型
                    s.DeSpawn();
                }
            }

            SpecialHolders = null;
        }
    }
}