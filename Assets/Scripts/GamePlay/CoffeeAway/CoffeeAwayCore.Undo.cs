using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CasualGame.CoffeeAway
{
    public partial class CoffeeAwayCore
    {
        /// <summary>
        /// 撤销有效性：原座位未被其他车占用，且去向 holder 未完成。
        /// </summary>
        public bool CanApplyUndo(UndoRecord record)
        {
            foreach (var entry in record.Cars)
            {
                if (entry.Vehicle is null) return false;

                var occupant = record.SourceHolder.GetVehicleAt(entry.SourceCol, entry.SourceRow);
                if (occupant != null) return false;  // 被自己占也不能undo

                var destHolder = GetVehicleDestinationHolder(entry.Vehicle);
                if (destHolder != null && destHolder.IsFullSameColor)
                    return false;
            }

            return true;
        }

        public void ApplyUndo(UndoRecord record)
        {
            var affectedHolders = new HashSet<Holder> { record.SourceHolder };
            int remaining = record.Cars.Count;
            if (remaining == 0) return;

            void OnOneCarDone()
            {
                remaining--;
                if (remaining > 0) return;

                foreach (var holder in affectedHolders)
                    holder.RefreshHiddenVehicleDisplay();

                if (IsHighlightActive)
                    RefreshAllRenderLayers();
            }

            foreach (var entry in record.Cars)
                AnimateRestoreVehicle(record.SourceHolder, entry, affectedHolders, OnOneCarDone);
        }
        private void AnimateRestoreVehicle(Holder sourceHolder, CarUndoEntry entry, HashSet<Holder> affected, Action onComplete)
        {
            var vehicle = entry.Vehicle;
            var destHolder = GetVehicleDestinationHolder(vehicle);
            if (destHolder != null)
                affected.Add(destHolder);

            DetachVehicleLogicForUndo(vehicle);

            sourceHolder.PlaceVehicle(vehicle, entry.SourceCol, entry.SourceRow);
            var pos = sourceHolder.GetCellWorldPos(entry.SourceCol, entry.SourceRow);
            vehicle.PrepareForUndoEnter();
            vehicle.PlayUndoEnterHolder(pos, GetHolderRotation(sourceHolder), onComplete);
        }

        private Holder GetVehicleDestinationHolder(Vehicle vehicle)
        {
            if (vehicle.State is not (VehicleState.InHolder or VehicleState.Entering))
                return null;

            return _holderMgr.Holders.FirstOrDefault(holder => holder.ContainsVehicle(vehicle));
        }

        private void DetachVehicleLogicForUndo(Vehicle vehicle)
        {
            RemoveFromPendingExits(vehicle);

            foreach (var holder in _holderMgr.Holders) holder.RemoveVehicleFromGrid(vehicle);

            vehicle.PrepareForUndoExit();

            switch (vehicle.State)
            {
                // WaitingExit 排队车点击时已计入容量未计入显示；OnRoad/Exiting 两者都计入，撤销按实际扣回
                case VehicleState.OnRoad or VehicleState.Exiting:
                    _vehicleMgr.RemoveCarsOnRoad(1, 1);
                    break;
                case VehicleState.WaitingExit:
                    _vehicleMgr.RemoveCarsOnRoad(1, 0);
                    break;
            }

            RefreshRoadColor();
        }

        private static Quaternion GetHolderRotation(Holder holder)
        {
            var yaw = holder.Info.DirectionValue ?? 0f;
            return Quaternion.Euler(0f, yaw, 0f);
        }
    }
}
