//------------------------------------------------------------
// 配置表：活动配置表
// 数据来源：Assets/StreamingAssets/tbactivityinfo.json（由 LuBan 生成）
// 读取方式：GameManager.Instance.Config.GetTable<DRActivityInfo>()
//------------------------------------------------------------

namespace CasualGame
{
    /// <summary>
    /// 活动配置表。
    /// </summary>
    public class DRActivityInfo
    {
        public int Id { get; set; }
        public int UnlockLv { get; set; }
        public int ConditionType { get; set; }
        public int Duration { get; set; }
        public int Interval { get; set; }
        public string SpinePath { get; set; }
    }
}
