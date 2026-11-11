//------------------------------------------------------------
// 配置表：道具配置表
// 数据来源：Assets/StreamingAssets/tbitem.json（由 LuBan 生成）
// 读取方式：GameManager.Instance.Config.GetTable<DRItem>()
//------------------------------------------------------------

namespace CasualGame
{
    /// <summary>
    /// 道具配置表。
    /// </summary>
    public class DRItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Icons { get; set; }
        public int Style { get; set; }
    }
}
