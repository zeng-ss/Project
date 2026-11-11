//------------------------------------------------------------
// 配置表：引导配置表
// 数据来源：Assets/StreamingAssets/tbguide.json（由 LuBan 生成）
// 读取方式：GameManager.Instance.Config.GetTable<DRGuide>()
//------------------------------------------------------------

namespace CasualGame
{
    /// <summary>
    /// 引导配置表。
    /// </summary>
    public class DRGuide
    {
        public int Id { get; set; }
        public int WaitUIOpenType { get; set; }
        public int GuideType { get; set; }
        public int NextId { get; set; }
    }
}
