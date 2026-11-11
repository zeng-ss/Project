//------------------------------------------------------------
// 配置表：音效配置表
// 数据来源：Assets/StreamingAssets/tbsound.json（由 LuBan 生成）
// 读取方式：GameManager.Instance.Config.GetTable<DRSound>()
//------------------------------------------------------------

namespace CasualGame
{
    /// <summary>
    /// 音效配置表。
    /// </summary>
    public class DRSound
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
        public string SoundGroup { get; set; }
        public float SpatialBlend { get; set; }
        public float Time { get; set; }
        public float Volume { get; set; }
        public int Priority { get; set; }
        public float MaxDistance { get; set; }
        public float DopplerLevel { get; set; }
        public float Pitch { get; set; }
        public bool Loop { get; set; }
    }
}
