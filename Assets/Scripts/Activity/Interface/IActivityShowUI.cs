using CasualGame;

namespace Custom.Activity
{
    public interface IActivityShowUI
    {
        /// <summary>
        /// 是否自动显示UI
        /// </summary>
        /// <returns></returns>
        bool ShowUICondition(out int flag);

        /// <summary>
        /// 标记UI已显示
        /// </summary>
        void MarkUIShown(int flag);

        /// <summary>
        /// 获取状态UI标识
        /// </summary>
        /// <returns></returns>
        int GetStateUIFlag();

        /// <summary>
        /// 获取当前状态UI
        /// </summary>
        /// <returns></returns>
        UIFormType GetCurrentStateUI();
    }
}