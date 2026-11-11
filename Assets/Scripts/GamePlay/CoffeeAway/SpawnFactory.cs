using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace CasualGame.CoffeeAway
{
    /// <summary>
    /// 创建工厂
    /// </summary>
    public static class SpawnFactory
    {
        private static AttachParentPrefabPool[] m_Pools;
        public static AttachParentPrefabPool HolderPool;
        public static AttachParentPrefabPool ColorHolderPool;
        public static AttachParentPrefabPool VehiclePool;

        #region all_pool_idx

        private const int HOLDER_POOL_IDX = 0;
        private const int VEHICLE_POOL_IDX = 1;
        private const int COLOR_HOLDER__POOL_IDX = 2;

        #endregion

        #region holder_pool_idx

        private const int STANDARD_HOLDER_IDX = 1;
        private const int EXPANDABLE_HOLDER_IDX = 2;
        private const int EXTRA_HOLDER_IDX = 3;
        private const int TUNNEL_IDX = 4;
        private const int OPEN_TUNNEL_IDX = 5;
        private const int CONTAINER_OBJ_IDX = 6;
        private const int GLASS_OBJ_IDX = 7;
        private const int RECEIPT_PRINTER_IDX = 8;
        private const int ROAD_SURFACE_IDX = 9;
        private const int Entrance_IDX = 10;

        #endregion

        #region vehicle_pool_idx

        private const int BLACK_INX = 0;
        private const int BLUE_INX = 1;
        private const int BROWN_INX = 2;
        private const int CYAN_INX = 3;
        private const int GREEN_INX = 4;
        private const int DARK_GREEN_INX = 5;
        private const int ORANGE_INX = 7;
        private const int PINK_INX = 8;
        private const int PURPLE_INX = 9;
        private const int RED_INX = 10;
        private const int MAGENTA_INX = 12;
        private const int LILAC_INX = 13;
        private const int YELLOW_INX = 14;
        private const int CHERRY_INX = 15;
        private const int CARAMEL_BROWN_INX = 16;
        private const int DARK_BLUE_INX = 17;
        private const int SKY_BLUE_INX = 18;

        #endregion

        #region color_holder_pool_idx

        private const int BLUE_COLOR_HOLDER_INX = 1;
        private const int BROWN_COLOR_HOLDER_INX = 2;
        private const int CYAN_COLOR_HOLDER_INX = 3;
        private const int GREEN_COLOR_HOLDER_INX = 4;
        private const int DARK_COLOR_HOLDER_GREEN_INX = 5;
        private const int ORANGE_COLOR_HOLDER_INX = 7;
        private const int PINK_COLOR_HOLDER_INX = 8;
        private const int PURPLE_COLOR_HOLDER_INX = 9;
        private const int RED_COLOR_HOLDER_INX = 10;
        private const int MAGENTA_COLOR_HOLDER_INX = 12;
        private const int LILAC_COLOR_HOLDER_INX = 13;
        private const int YELLOW_COLOR_HOLDER_INX = 14;
        private const int CHERRY_COLOR_HOLDER_INX = 15;
        private const int CARAMEL_BROWN_COLOR_HOLDER_INX = 16;
        private const int DARK_BLUE_COLOR_HOLDER_INX = 17;
        private const int SKY_BLUE_COLOR_HOLDER_INX = 18;

        #endregion

        public static void Init(AttachParentPrefabPool[] pools)
        {
            m_Pools = pools;
            HolderPool = m_Pools[HOLDER_POOL_IDX];
            VehiclePool = m_Pools[VEHICLE_POOL_IDX];
            // 颜色容器池可能未配置（场景未添加第三个池），缺失时走标准容器+覆盖物兜底
            ColorHolderPool = m_Pools.Length > COLOR_HOLDER__POOL_IDX ? m_Pools[COLOR_HOLDER__POOL_IDX] : null;
        }

        #region Spawn Coffee

        /// <summary>
        /// 按类型从池创建 Vehicle；隐藏杯使用问号专用预制体（index 0），普通杯使用对应颜色预制体
        /// </summary>
        public static T SpawnVehicle<T>(LevelBuilder.CarInfo carInfo, float? yaw, string carName, Transform parent)
            where T : Vehicle
        {
            // 隐藏杯：用问号专用预制体（index 0）遮盖真实颜色，揭开时再替换为对应颜色预制体
            if (carInfo.UnitData?.GetVehicleType() == VehicleType.Hidden)
            {
                var hidden = VehiclePool.Spawn<HiddenCoffee>(BLACK_INX, parent);
                if (hidden is null)
                {
                    Log.Error("[SpawnFactory] Failed to spawn hidden coffee from pool.");
                    return null;
                }

                hidden.name = carName;
                hidden.transform.position = carInfo.WorldPos;
                if (yaw.HasValue) hidden.transform.rotation = Quaternion.Euler(0f, yaw.Value, 0f);
                hidden.Init(carInfo);
                return hidden as T;
            }

            var v = VehiclePool.Spawn<T>(carInfo.UnitData.Id?.Color ?? 0, parent);
            return SetupVehicle(v, carInfo, yaw, carName) as T;
        }

        // 问号杯揭开：用真实颜色的专用预制体替换
        public static Vehicle SpawnColorVehicleReplacement(Vehicle hidden, ColorType color, Transform parent)
        {
            var v = VehiclePool.Spawn<NormalCoffee>((int)color, parent);
            if (v is null)
            {
                Log.Error($"[SpawnFactory] Failed to spawn color vehicle for hidden reveal, color={color}.");
                return null;
            }

            v.name = hidden.name;
            v.transform.position = hidden.transform.position;
            v.transform.rotation = hidden.transform.rotation;
            v.InitAsReplacement(color);
            return v;
        }

        // 设置 Vehicle 属性
        private static Vehicle SetupVehicle(Vehicle v, LevelBuilder.CarInfo carInfo, float? yaw, string carName)
        {
            if (v is null)
            {
                Log.Error("[SpawnFactory] Failed to spawn vehicle from pool.");
                return null;
            }

            v.name = carName;
            v.transform.position = carInfo.WorldPos;
            if (yaw.HasValue) v.transform.rotation = Quaternion.Euler(0f, yaw.Value, 0f);

            v.Init(carInfo);
            return v;
        }

        public static Color GetColor(ColorType type)
        {
            // 色值取自 Assets/Game/Sprites/CoffeeColor 对应贴图主色
            return type switch
            {
                ColorType.Blue => new Color(0x62 / 255f, 0xA7 / 255f, 0xDC / 255f),
                ColorType.Brown => new Color(0xCC / 255f, 0x8B / 255f, 0x69 / 255f),
                ColorType.Cyan => new Color(0x66 / 255f, 0xCC / 255f, 0xDB / 255f),
                ColorType.Green => new Color(0x77 / 255f, 0xC4 / 255f, 0x6F / 255f),
                ColorType.DarkGreen => new Color(0x70 / 255f, 0xAD / 255f, 0x80 / 255f),
                ColorType.Orange => new Color(0xEC / 255f, 0xA0 / 255f, 0x73 / 255f),
                ColorType.Pink => new Color(0xF1 / 255f, 0xA5 / 255f, 0xBF / 255f),
                ColorType.Purple => new Color(0xB5 / 255f, 0x84 / 255f, 0xE3 / 255f),
                ColorType.Red => new Color(0xE5 / 255f, 0x6E / 255f, 0x70 / 255f),
                ColorType.Lilac => new Color(0xCE / 255f, 0x91 / 255f, 0xD7 / 255f),
                ColorType.Yellow => new Color(0xF1 / 255f, 0xC6 / 255f, 0x5D / 255f),
                ColorType.Cherry => new Color(0xD0 / 255f, 0x71 / 255f, 0xA0 / 255f),
                ColorType.Magenta => new Color(0xEC / 255f, 0x8D / 255f, 0xC3 / 255f),
                ColorType.CaramelBrown => new Color(0xDF / 255f, 0x9C / 255f, 0x78 / 255f),
                ColorType.DarkBlue => new Color(0x76 / 255f, 0x78 / 255f, 0xCB / 255f),
                ColorType.SkyBlue => new Color(0x7A / 255f, 0xAC / 255f, 0xE7 / 255f),
                ColorType.Black => new Color(0x4D / 255f, 0x4D / 255f, 0x4D / 255f),
                _ => Color.gray
            };
        }

        #endregion

        #region Spawn Holders

        /// <summary>
        /// 根据 HolderInfo.HolderPrefabType 从对象池创建容器，并按 states 挂载特殊玩法组件
        /// </summary>
        public static Holder SpawnHolderByType(LevelBuilder.HolderInfo info, Transform parent)
        {
            return info.HolderPrefabType switch
            {
                HolderPrefabType.Standard => SpawnHolder<StandardHolder>(info, parent, STANDARD_HOLDER_IDX),
                //HolderPrefabType.Expandable => SpawnHolder<ExpandableHolder>(info, parent, EXPANDABLE_HOLDER_IDX),
                HolderPrefabType.Extra => SpawnHolder<ExtraHolder>(info, parent, EXTRA_HOLDER_IDX),
                //HolderPrefabType.Tunnel => SpawnHolder<Tunnel>(info, parent, TUNNEL_IDX),
                //HolderPrefabType.OpenTunnel => SpawnHolder<OpenTunnel>(info, parent, OPEN_TUNNEL_IDX),
                HolderPrefabType.Colored => SpawnColorHolder(info, parent),
                _ => SpawnHolder<StandardHolder>(info, parent, STANDARD_HOLDER_IDX)
            };
        }

        // 颜色容器专用路径：从 ColorHolderPool 按颜色索引生成 ColorFilteredMachine 容器预制体
        private static Holder SpawnColorHolder(LevelBuilder.HolderInfo info, Transform parent)
        {
            // 从 states 里取 Colored 状态的颜色索引
            int colorIdx = 0;
            if (info.HolderStates != null)
            {
                foreach (var state in info.HolderStates)
                {
                    if ((HolderSpecialType)state.Type != HolderSpecialType.Colored || state.ColorType == null) continue;
                    colorIdx = (int)state.ColorType;
                    break;
                }
            }

            var holder = ColorHolderPool.Spawn<ColorFilteredMachine>(colorIdx, parent);
            if (holder is null)
            {
                Log.Warning($"[SpawnFactory] 颜色容器池缺少颜色 {colorIdx} 的专用预制体，回退到标准容器。");
                return SpawnHolder<StandardHolder>(info, parent, STANDARD_HOLDER_IDX);
            }

            holder.transform.position = info.AnchorWorldPos;
            holder.transform.localRotation = Quaternion.Euler(0, info.DirectionValue ?? 0, 0);
            holder.SourcePool = ColorHolderPool;
            holder.Init(info, AttachHolderSpecial(holder, info.HolderStates));
            holder.gameObject.SetLayerRecursively(Constant.Layer.HolderLayerId);
            return holder;
        }

        private static T SpawnHolder<T>(LevelBuilder.HolderInfo info, Transform parent, int poolIndex) where T : Holder
        {
            var holder = HolderPool.Spawn<T>(poolIndex, parent);
            if (holder is null)
            {
                Log.Error($"[SpawnFactory] Failed to spawn {typeof(T).Name} from pool index {poolIndex}.");
                return null;
            }

            holder.transform.position = info.AnchorWorldPos;
            holder.transform.localRotation = Quaternion.Euler(0, info.DirectionValue ?? 0, 0);
            holder.SourcePool = HolderPool;
            holder.Init(info, AttachHolderSpecial(holder, info.HolderStates));
            holder.gameObject.SetLayerRecursively(Constant.Layer.HolderLayerId);
            if (typeof(T) == typeof(ExtraHolder)) holder.gameObject.SetActive(false);
            return holder;
        }

        // 根据关卡 states 数组挂载容器特殊玩法覆盖物（玻璃/盖子等）；Colored 由专用颜色容器预制体承载
        private static List<ISpecialMachine> AttachHolderSpecial(Holder holder, List<GameUnitStateData> states)
        {
            if (states == null) return null;

            var specialHolders = new List<ISpecialMachine>();
            foreach (var state in states)
            {
                ISpecialMachine special = null;
                switch ((HolderSpecialType)state.Type)
                {
                    case HolderSpecialType.Glass:
                        special = SpawnSpecial<GlassHolderCtrl>(holder, GLASS_OBJ_IDX);
                        break;
                    case HolderSpecialType.Container:
                        special = SpawnSpecial<ColorLockedHolderCtrl>(holder, CONTAINER_OBJ_IDX);
                        break;
                    case HolderSpecialType.Colored:
                        // 颜色容器由 ColorHolderPool 的专用容器预制体承载，不在此挂覆盖物
                        break;
                    case HolderSpecialType.None:
                    default:
                        Log.Warning($"[SpawnFactory] 未处理的容器特殊玩法 type={state.Type}");
                        break;
                }

                if (special is null) continue;
                special.Init(holder, state);
                specialHolders.Add(special);
            }

            return specialHolders;
        }

        // 从对象池生成特殊玩法覆盖物（玻璃/盖子），统一从 HolderPool 生成
        private static T SpawnSpecial<T>(Holder holder, int poolIndex) where T : HolderSpecialCtrl
            => HolderPool.Spawn<T>(poolIndex, holder.transform);

        #endregion

        #region Spawn RoadSurfaceMove

        /// <summary>
        /// 沿中心线每隔 spacing 距离放一个 RoadSurfaceMove，让它们沿道路循环移动。
        /// </summary>
        public static List<RoadSurfaceMove> SpawnRoadSurfaces(
            RoadNavigator navigator, Transform parent, float spacing, float surfaceY)
        {
            var list = new List<RoadSurfaceMove>();

            // 闭环道路：用 Floor 避免最后一个恰好落在首尾接缝(0)处与第一个重叠
            int count = Mathf.Max(1, Mathf.FloorToInt(navigator.TotalLength / spacing));
            for (int i = 0; i < count; i++)
            {
                float dist = i * spacing % navigator.TotalLength;
                var surface = HolderPool.Spawn<RoadSurfaceMove>(ROAD_SURFACE_IDX, parent);
                if (surface is null) continue;

                var pos = navigator.GetPosition(dist);
                var dir = navigator.GetDirection(dist);
                surface.transform.position = pos + Vector3.up * surfaceY;
                if (dir != Vector3.zero) surface.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
                surface.Init(navigator, dist, surfaceY);
                list.Add(surface);
            }

            return list;
        }

        #endregion

        #region Spawn ReceiptPrinter

        /// <summary>
        /// 从对象池创建小票机，坐标来自关卡 capacityTextPosition
        /// </summary>
        public static ReceiptPrinter SpawnReceiptPrinter(Vector3 worldPos, Transform parent)
        {
            var printer = HolderPool.Spawn<ReceiptPrinter>(RECEIPT_PRINTER_IDX, parent);
            if (printer is null)
            {
                Log.Error($"[SpawnFactory] Failed to spawn ReceiptPrinter from pool index {RECEIPT_PRINTER_IDX}.");
                return null;
            }

            // 位置碰撞处理由 CoffeeAwayHelper.SpawnReceiptPrinter 统一负责
            printer.transform.position = worldPos;
            return printer;
        }

        #endregion

        #region Spawn Entrance

        /// <summary>
        /// 从对象池创建传送门（出入口）标记，roadDir 为道路行进方向，用于让门洞横跨道路
        /// </summary>
        public static Entrance SpawnEntrance(Vector3 worldPos, Vector3 roadDir, Transform parent, bool isStart)
        {
            var entrance = HolderPool.Spawn<Entrance>(Entrance_IDX, parent);
            if (entrance is null)
            {
                Log.Error($"[SpawnFactory] Failed to spawn Entrance from pool index {Entrance_IDX}.");
                return null;
            }

            entrance.SetPos(worldPos, roadDir, isStart);
            return entrance;
        }

        #endregion
    }
}