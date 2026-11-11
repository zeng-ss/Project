using System;
using DG.Tweening;
using UnityEngine;

namespace CasualGame.CoffeeAway
{
    public class ExtraHolder : Holder
    {
        CoffeeAwayCore _core;
        bool canAccept;

        protected override void CheckComplete()
        {
            // Empty method to ensure clickable
        }

        public void DoShowAni(CoffeeAwayCore core, Action onComplete)
        {
            _core = core;
            //_core.AddCapacity(4);
            transform.DOKill(true);
            gameObject.SetActive(true);
            transform.localScale = Vector3.zero;
            transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).OnComplete(() => { onComplete?.Invoke(); });
        }

        protected override bool CanAcceptVehicle(ColorType colorType)
        {
            return !_core.IsAutoCompleting && base.CanAcceptVehicle(colorType);
        }

        /*// 道路满载时主动吸最近一辆 OnRoad 车进来，避免全路堵死导致额外容器永远收不到车
        private bool TryPullCar(Vehicle vehicle)
        {
            if (!gameObject.activeSelf || IsBusy) return false;
            if (AnySpecialBlocks(s => !s.CanAcceptVehicle())) return false;

            Quaternion targetRot = Info.DirectionValue.HasValue
                ? Quaternion.Euler(0f, Info.DirectionValue.Value, 0f)
                : Quaternion.identity;
            Vector3 worldPos = Grid.PlaceToFirstEmpty(vehicle);
            vehicle.EnterHolder(worldPos, targetRot);
            return true;
        }*/
    }
}