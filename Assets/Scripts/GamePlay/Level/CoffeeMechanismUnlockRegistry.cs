using System.Collections.Generic;

namespace CasualGame.CoffeeAway
{
    /// <summary>
    /// 特殊玩法机制与关卡 JSON 字段的映射，以及运行时查询辅助。
    /// </summary>
    public static class CoffeeMechanismUnlockRegistry
    {
        private static CoffeeMechanismUnlockConfig s_Config;
        public static CoffeeMechanismUnlockConfig Config => s_Config;

        public static void Initialize(CoffeeMechanismUnlockConfig configs)
        {
            CoffeeMechanismUnlockConfig config = configs;
            s_Config = config;
        }

        public static bool IsMechanismFeature(ItemType mechanism)
        {
            return mechanism is ItemType.Hidden
                or ItemType.Glass
                or ItemType.Container
                or ItemType.Colored;
        }

        public static bool TryGetMechanismFromHolderState(int type, out ItemType mechanism)
        {
            mechanism = type switch
            {
                (int)HolderSpecialType.Glass => ItemType.Glass,
                (int)HolderSpecialType.Container => ItemType.Container,
                (int)HolderSpecialType.Colored => ItemType.Colored,
                _ => ItemType.None
            };

            return mechanism != ItemType.None;
        }

        public static bool TryGetMechanismFromVehicleState(int type, int? stateType, out ItemType mechanism)
        {
            if (type == (int)VehicleType.Hidden || stateType == (int)VehicleType.Hidden)
            {
                mechanism = ItemType.Hidden;
                return true;
            }

            mechanism = ItemType.None;
            return false;
        }

        /// <summary>
        /// 拿到最近的玩法预告的 Item 类型（首次出现关卡大于当前 level）
        /// </summary>
        public static void GetNextFutureTypeFromLevel(int level, out ItemType unlockType)
        {
            unlockType = ItemType.None;
            if (s_Config == null)
            {
                return;
            }

            IReadOnlyList<CoffeeMechanismUnlockConfig.Entry> entries = s_Config.Entries;
            for (int i = 0; i < entries.Count; i++)
            {
                CoffeeMechanismUnlockConfig.Entry entry = entries[i];
                if (!IsMechanismFeature(entry.Mechanism))
                {
                    continue;
                }

                if (entry.FirstProgressLevel > level)
                {
                    unlockType = entry.Mechanism;
                    return;
                }
            }
        }

        /// <summary>
        /// 读取机制首次出现关卡
        /// </summary>
        public static int GetUnlockLevelForItem(ItemType type)
        {
            if (s_Config != null && s_Config.TryGetFirstProgressLevel(type, out int level))
            {
                return level;
            }

            return 0;
        }

        /// <summary>
        /// 找到上一个解锁关卡用作进度条段起点
        /// </summary>
        public static int GetPreviousUnlockLevel(int level)
        {
            if (s_Config == null)
            {
                return 1;
            }

            IReadOnlyList<CoffeeMechanismUnlockConfig.Entry> entries = s_Config.Entries;
            for (int i = 0; i < entries.Count - 1; i++)
            {
                int preUnlockLevel = entries[i].FirstProgressLevel;
                int nextUnlockLevel = entries[i + 1].FirstProgressLevel;
                if (level >= preUnlockLevel && level < nextUnlockLevel)
                {
                    return preUnlockLevel;
                }
            }

            return 1;
        }
    }
}
