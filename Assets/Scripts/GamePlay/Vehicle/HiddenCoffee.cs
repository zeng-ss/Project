using System.Linq;

namespace CasualGame.CoffeeAway
{
    /// <summary>
    /// 问号杯：专用预制体遮盖真实颜色。揭开时播放特效，并替换为对应颜色的专用预制体。
    /// </summary>
    public class HiddenCoffee : Vehicle
    {
        private bool _revealed;

        // 问号杯揭开为 Normal 时替换真实颜色预制体；成功即回收自身，失败保留问号杯供下次重试
        public override void SetVehicleType(VehicleType type)
        {
            if (VehicleType == type) return;
            if (type != VehicleType.Normal) return;
            base.SetVehicleType(type);
            if (_revealed) return;
            _revealed = true;
            PlaySoundManager.Instance.PlaySound(Constant.Audio.HiddenDisappear);
            if (!ReplaceWithColorVehicle()) _revealed = false;
        }

        // 用真实颜色的专用预制体替换自己，并同步容器网格与核心车辆列表；成功返回 true
        private bool ReplaceWithColorVehicle()
        {
            var core = CoffeeAwayGame.Instance?.Core;
            var holder = core?.Holders.FirstOrDefault(h => h.ContainsVehicle(this));
            if (holder is null) return false;

            var colorV = SpawnFactory.SpawnColorVehicleReplacement(this, ColorType, transform.parent);
            if (colorV is null) return false;

            holder.PlaceVehicle(colorV, HolderColIndex, HolderRowIndex);

            if (core != null)
            {
                var vehicles = core.Vehicles;
                int idx = vehicles.IndexOf(this);
                if (idx >= 0) vehicles[idx] = colorV;
            }

            colorV.ShowUnlock();
            SpawnFactory.VehiclePool.DeSpawn(this);
            return true;
        }

        public override void DeSpawn()
        {
            _revealed = false;
            base.DeSpawn();
        }
    }
}