//------------------------------------------------------------
// 配置表：手指引导配置表
// 数据来源：Assets/StreamingAssets/tbfingerguide.json（由 LuBan 生成）
// 读取方式：GameManager.Instance.Config.GetTable<DRFingerGuide>()
//------------------------------------------------------------

using UnityEngine;

namespace CasualGame
{
    /// <summary>
    /// 手指引导配置表。
    /// </summary>
    public class DRFingerGuide
    {
        public int Id { get; set; }
        public string SceneName { get; set; }
        public string RootName { get; set; }
        public string HigtLightTargetPath { get; set; }
        public string ButtonPath { get; set; }
        public Vector2 Pivot { get; set; }
    }
}
