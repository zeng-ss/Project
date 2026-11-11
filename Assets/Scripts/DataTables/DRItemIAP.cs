//------------------------------------------------------------
// 配置表：内购商品配置表
// 数据来源：Assets/StreamingAssets/tbitemiap.json（由 LuBan 生成）
// 读取方式：GameManager.Instance.Config.GetTable<DRItemIAP>()
//------------------------------------------------------------

namespace CasualGame
{
    /// <summary>
    /// 内购商品配置表。
    /// </summary>
    public class DRItemIAP
    {
        public int Id { get; set; }
        public string ProductId { get; set; }
        public int ProductType { get; set; }
        public int Platform { get; set; }
        public float PrizeValue { get; set; }
        public int TikTokBeans { get; set; }
        public int RevType { get; set; }
        public string BaseProductId { get; set; }
        public float OriginalPrize { get; set; }
        public string EventName { get; set; }
    }
}
