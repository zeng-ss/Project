//------------------------------------------------------------
// 配置表：对话引导配置表
// 数据来源：Assets/StreamingAssets/tbdialogguide.json（由 LuBan 生成）
// 读取方式：GameManager.Instance.Config.GetTable<DRDialogGuide>()
//------------------------------------------------------------

namespace CasualGame
{
    /// <summary>
    /// 对话引导配置表。
    /// </summary>
    public class DRDialogGuide
    {
        public int Id { get; set; }
        public int AnchorID { get; set; }
        public int ClothID { get; set; }
        public string Content { get; set; }
        public int AudioAssetID { get; set; }
    }
}
