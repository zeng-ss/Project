using UnityEngine;
using UnityGameFramework.Runtime;

namespace CasualGame.CoffeeAway
{
    /// <summary>
    /// 随机打乱道具：调用容器的 Shuffle 在内部重排车辆，再逐辆播放移动动画。
    /// </summary>
    public class ShuffleProp : PropBase
    {
        public override ItemType ItemType => ItemType.Shuffle;

        public ShuffleProp(CoffeeAwayCore core) : base(core)
        {
        }

        public override bool Execute(Holder targetHolder = null, ColorType targetColor = default)
        {
            if (targetHolder is null)
            {
                Log.Error("[PowerUp:Shuffle] 打乱失败：未选中容器");
                return false;
            }

            // 优先把其他容器可接受的颜色交换到前排，保证打乱后有解
            var preferredColors = Core.GetAcceptableColorsForShuffle(targetHolder);
            var moved = targetHolder.Shuffle(Core.Rng, out var hint, preferredColors);
            if (moved == null)
            {
                Log.Error($"[PowerUp:Shuffle] 打乱失败：{hint}");
                return false;
            }

            // 没有车需要移动也要恢复冻结的车辆，否则会一直静止；布局未变仍可能已有前排问号杯待揭开
            if (moved.Count == 0)
            {
                Core.ResetAllVehiclesMoveSpeed();
                targetHolder.RefreshHiddenVehicleDisplay();
                return true;
            }

            // 等所有车落到新位置后再刷新前排：动画中途揭开会把替换车留在旧位置，导致问号杯"看起来没被正确Replace"
            float duration = 0.5f;
            int remain = moved.Count;
            foreach (var car in moved)
            {
                var pos = targetHolder.GetCellWorldPos(car.HolderColIndex, car.HolderRowIndex);
                var rot = Quaternion.Euler(0f, targetHolder.Info.DirectionValue ?? 0f, 0f);
                car.MoveTo(pos, rot, duration, () =>
                {
                    Core.ResetAllVehiclesMoveSpeed();
                    if (--remain <= 0) targetHolder.RefreshHiddenVehicleDisplay();
                });
            }

            return true;
        }
    }
}