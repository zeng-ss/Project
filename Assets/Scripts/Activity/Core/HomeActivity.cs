using CasualGame;

namespace Custom.Activity
{
    /// <summary>
    /// 主頁入口活動基底：實作 <see cref="IHomeActivity"/>，子類覆寫顯示條件、圖示與點擊行為。
    /// </summary>
    public abstract class HomeActivity<T> : AbstractActivity<T>, IHomeActivity where T : ActivityData
    {
        /// <summary>
        /// 是否显示入口
        /// </summary>
        public virtual bool ShowEntry => true;

        /// <summary>
        /// 位置
        /// </summary>
        public abstract ActivityPos ActivityPos { get; }

        /// <summary>
        /// 优先级
        /// </summary>
        public abstract int ShowPriority { get; }

        /// <summary>
        /// 红点数量
        /// </summary>
        public virtual int RedPointCount() => 0;

        /// <summary>
        /// 获取图标名称
        /// </summary>
        public abstract string GetIconName();

        /// <summary>
        /// 获取文本键值
        /// </summary>
        public abstract int GetTextKey();

        /// <summary>
        /// 获取入口名称
        /// </summary>
        public virtual string GetEntranceName() => string.Empty;

        /// <summary>
        /// 生成入口
        /// </summary>
        public virtual void OnSpawnEntrance(HomePanelEntrance entrance) { }

        /// <summary>
        /// 销毁入口
        /// </summary>
        public virtual void OnDespawnEntrance(HomePanelEntrance entrance) { }

        /// <summary>
        /// 默认点击打开的界面
        /// </summary>
        protected abstract UIFormType HomeClickForm { get; }

        /// <summary>
        /// 点击活动UI
        /// </summary>
        public virtual void ClickActivityUI()
        {
            UIFormType form = HomeClickForm;
            if (form != UIFormType.Undefined)
                GameEntry.UI.OpenUIForm(form);
        }

        /// <summary>
        /// 检查自动弹窗
        /// </summary>
        /// <param name="backHome"></param>
        public virtual void CheckAutoPopup(BackHomeType backHome) { }

        /// <summary>
        /// 是否需要刷新时间
        /// </summary>
        /// <returns></returns>
        public virtual bool ShouldRefreshTime() => false;
    }
}
