//------------------------------------------------------------
// 配置表：UI 界面配置表
// 数据来源：Assets/StreamingAssets/tbuiform.json（由 LuBan 生成）
// 读取方式：GameManager.Instance.Config.GetTable<DRUIForm>()
//------------------------------------------------------------

namespace CasualGame
{
    /// <summary>
    /// UI 界面配置表。
    /// </summary>
    public class DRUIForm
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DirectoryName { get; set; }
        public string UIGroupName { get; set; }
        public bool AllowMultiInstance { get; set; }
        public bool PauseCoveredUIForm { get; set; }
        public bool Mutex { get; set; }
    }
}
