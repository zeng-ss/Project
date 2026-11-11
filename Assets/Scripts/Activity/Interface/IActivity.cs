using CasualGame;

namespace Custom.Activity
{
	public interface IActivity
	{
		ActivityType ActivityType { get; }

		DRActivityInfo ActivityInfo { get; }

		void Initial();

		void Activate();

		void CheckActivity(string reason = "");

		void NewDay();

		void UpdatePerSecond();

		void Close();

		string GetRemainTimeText();

		bool IsActivated { get; }

		bool IsNewFunction { get; }
	}
}