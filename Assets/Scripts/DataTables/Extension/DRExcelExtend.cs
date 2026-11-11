//------------------------------------------------------------
// File : DRExcelExtend.cs
// Desc : 配置表的数据扩展方法
//------------------------------------------------------------

namespace CasualGame
{
    public enum ItemShowStyle
    {
        Add = 1,  // +100
        Mult = 2, // x100
        Time = 3, // 2h3min
        Empty = 4,
    }

    public static class DRExcelExtend
    {
        public static bool IsTableEmpty(string str)
        {
            if (string.IsNullOrEmpty(str)) return true;
            if (str == "-1" || str == "-1.0") return true;
            return false;
        }

        public static ItemShowStyle GetItemShowStyle(this DRItem item)
        {
            return (ItemShowStyle)item.Style;
        }

        public static string GetPrize(this DRItemIAP iap)
        {
            return $"${iap.PrizeValue}";
        }

        public static string GetOriginalPrize(this DRItemIAP iap)
        {
            return $"${iap.OriginalPrize}";
        }

        public static string GetPrizeWithRate(this DRItemIAP iap, float rate)
        {
            return $"${(iap.PrizeValue / rate):F2}";
        }
    }
}
