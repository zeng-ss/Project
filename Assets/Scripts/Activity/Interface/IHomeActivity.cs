using CasualGame;

namespace Custom.Activity
{
	public interface IHomeActivity
	{
		/// <summary>
		/// 活動配置資訊
		/// </summary>
		DRActivityInfo ActivityInfo { get; }

		/// <summary>
		/// 是否展示入口
		/// </summary>
		bool ShowEntry { get; }

		/// <summary>
		/// 排序位置
		/// </summary>
		ActivityPos ActivityPos { get; }

		/// <summary>
		/// 排序优先级
		/// </summary>
		int ShowPriority { get; }

		/// <summary>
		/// 红点数量
		/// </summary>
		/// <returns></returns>
		int RedPointCount();

		/// <summary>
		/// 获取主页icon
		/// </summary>
		/// <returns></returns>
		string GetIconName();

		/// <summary>
		/// 获取多语言key
		/// </summary>
		/// <returns></returns>
		int GetTextKey();

		/// <summary>
		/// 点击活动UI
		/// </summary>
		void ClickActivityUI();

		/// <summary>
		/// 创建入口
		/// </summary>
		void OnSpawnEntrance(HomePanelEntrance entrance);

		/// <summary>
		/// 销毁入口
		/// </summary>
		void OnDespawnEntrance(HomePanelEntrance entrance);

		//获取主界面名称
		string GetEntranceName();

		/// <summary>
		/// 检查自动弹窗
		/// </summary>
		/// <param name="backHome"></param>
		void CheckAutoPopup(BackHomeType backHome);

		/// <summary>
		/// 完成新功能
		/// </summary>
		void FinishNewFunc();

		/// <summary>
		/// 处理新功能
		/// </summary>
		void HandleNewFunc();

		/// <summary>
		/// 是否需要刷新时间
		/// </summary>
		/// <returns></returns>
		bool ShouldRefreshTime();
	}
}