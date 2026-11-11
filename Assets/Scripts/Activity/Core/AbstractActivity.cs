using System.Collections.Generic;
using CasualGame;
using GameFramework;
using Newtonsoft.Json;

namespace Custom.Activity
{
    public abstract class AbstractActivity<T> : IActivity where T : ActivityData
    {
        /// <summary>
        /// 活动模型
        /// </summary>
        private ActivityModel _activityModel => ProxyManager.UserProxy.userModel.ActivityModel;

        /// <summary>
        /// 活动信息
        /// </summary>
        private DRActivityInfo _activityInfo;

        /// <summary>
        /// 活动条件类型
        /// </summary>
        protected ActivityConditionType _activityConditionType => ActivityInfo.GetActivityConditionType();

        /// <summary>
        /// 创建活动数据
        /// </summary>
        protected abstract T CreateActivityData();

        /// <summary>
        /// 活动类型
        /// </summary>
        public abstract ActivityType ActivityType { get; }

        /// <summary>
        /// 初始化
        /// </summary>
        public abstract void Initial();

        /// <summary>
        /// 激活
        /// </summary>
        public abstract void Activate();

        /// <summary>
        /// 关闭
        /// </summary>
        public abstract void Close();

        /// <summary>
        /// 新的一天
        /// </summary>
        public virtual void NewDay()
        {
            if (_data == null) return;
            for (int i = 28; i < 32; i++)
            {
                Utility.Bitwise.SetBit(ref _data.popBit, 1 << i, false);
            }
            Save();
        }

        /// <summary>
        /// 是否激活
        /// </summary>
        public bool IsActivated => Data?.isEnabled ?? false;

        /// <summary>
        /// 游戏等级
        /// </summary>
        public int CurLevel => ProxyManager.UserProxy.GameLevel;

        /// <summary>
        /// 活动数据
        /// </summary>
        public T Data => GetActivityData();

        protected T _data;

        /// <summary>
        /// 获取活动信息
        /// </summary>
        public DRActivityInfo ActivityInfo => GetActivityInfo();

        public AbstractActivity()
        {
        }

        /// <summary>
        /// 获取活动数据
        /// </summary>
        /// <returns>活动数据</returns>
        protected T GetActivityData()
        {
            if (_activityModel == null) ProxyManager.UserProxy.userModel.ActivityModel = new ActivityModel();

            if (_data == null)
            {
                int activityTypeId = (int)ActivityType;
                if (_activityModel.ActivityData.TryGetValue(activityTypeId, out string dataStr))
                {
                    _data = JsonConvert.DeserializeObject<T>(dataStr);
                }
                else
                {
                    _data = CreateActivityData();
                }
            }

            return _data;
        }

        /// <summary>
        /// 获取活动信息
        /// </summary>
        /// <returns>活动信息</returns>
        public DRActivityInfo GetActivityInfo()
        {
            if (_activityInfo == null)
            {
                _activityInfo = ActivityManager.Instance.tableActivityInfo.GetDataRow(Data.activityId);
            }
            return _activityInfo;
        }

        /// <summary>
        /// 保存活动数据
        /// </summary>
        public virtual void Save()
        {
            _activityModel.ActivityData[(int)ActivityType] = JsonConvert.SerializeObject(Data);
            ProxyManager.UserProxy.Save();
        }

        /// <summary>
        /// 立即保存活动数据
        /// </summary>
        public virtual void SaveImmediately()
        {
            _activityModel.ActivityData[(int)ActivityType] = JsonConvert.SerializeObject(Data);
            ProxyManager.UserProxy.SaveImmediately();
        }

        /// <summary>
        /// 检查活动
        /// </summary>
        public virtual void CheckActivity(string reason = "")
        {
            if (IsActivated) return;

            switch (_activityConditionType)
            {
                case ActivityConditionType.ByCustomize:
                    if (CustomizeCondition())
                        Activate();
                    break;
                case ActivityConditionType.ByTime:
                    if (ActivityInfo.IsLevelAndTimeCondition(CurLevel))
                        Activate();
                    break;
            }
        }

        /// <summary>
        /// 每秒更新
        /// </summary>
        public void UpdatePerSecond()
        {
            if (_activityConditionType == ActivityConditionType.ByTime)
            {
                if (IsActivated)
                {
                    long currentEndTimestamp = GetCurrentEndTimestamp();
                    if (currentEndTimestamp < 0)
                    {
                        Close();
                    }
                }
                else
                {
                    if (ActivityInfo.IsLevelAndTimeCondition(CurLevel))
                    {
                        Activate();
                    }
                }
            }
        }

        /// <summary>
        /// 获取当前周期数
        /// </summary>
        /// <returns>当前周期数</returns>
        public int GetCurrentCycleNumber()
        {
            return ActivityInfo.GetCurrentCycleNumber();
        }

        /// <summary>
        /// 获取当前活动剩余时间（距离结束的秒数）
        /// </summary>
        /// <returns>剩余时间</returns>
        public long GetCurrentEndTimestamp()
        {
            return ActivityInfo.GetCurrentEndTimestamp();
        }

        /// <summary>
        /// 获取距离下次活动开启的剩余时间（秒数）
        /// </summary>
        /// <returns>下次活动开启时间戳</returns>
        public long GetNextStartTimestamp()
        {
            return ActivityInfo.GetNextStartTimestamp();
        }

        /// <summary>
        /// 自定义条件判断（用于活动是否开启）
        /// </summary>
        /// <returns>true表示活动开启，false表示活动关闭</returns>
        public virtual bool CustomizeCondition()
        {
            return ActivityInfo.IsLevelCondition(CurLevel);
        }

        /// <summary>
        /// 获取剩余时间文本
        /// </summary>
        /// <returns>剩余时间文本</returns>
        public string GetRemainTimeText()
        {
            string text = "";
            int remainTime = (int)GetCurrentEndTimestamp();
            if (remainTime < 24 * 60 * 60)
                text = TimeUtils.TimeFormat(remainTime, TimeFormatType.HH_MM, "{0}H{1}M", true, false);
            else
                text = TimeUtils.TimeFormat(remainTime, TimeFormatType.DD_HH, "{0}D{1}H", false, false);
            return text;
        }

        /// <summary>
        /// 获取下次活动开启的剩余时间文本
        /// </summary>
        /// <returns></returns>
        public string GetNextRoundRemainTimeText()
        {
            long nextStart = GetNextStartTimestamp();
            string text = "";
            int remainTime = (int)nextStart - GameManager.Instance.GetTimeStampOfSeconds();
            if (remainTime < 24 * 60 * 60)
                text = TimeUtils.TimeFormat(remainTime, TimeFormatType.HH_MM, "{0}H{1}M", true, false);
            else
                text = TimeUtils.TimeFormat(remainTime, TimeFormatType.DD_HH, "{0}D{1}H", false, false);
            return text;
        }

        /// <summary>
        /// 检查指定状态UI是否已经弹出过
        /// </summary>
        /// <param name="state">状态</param>
        /// <returns>true表示已弹出过，false表示未弹出过</returns>
        public bool HasShownStateUI(int flag)
        {
            return (Data.popBit & 1 << flag) != 0;
        }

        /// <summary>
        /// 标记指定状态UI已经弹出过
        /// </summary>
        /// <param name="state">状态</param>
        public void MarkStateUIShown(int flag)
        {
            Data.popBit |= 1 << flag;
            Save();
        }

        /// <summary>
        /// 重置UI弹出状态标记
        /// </summary>
        /// <param name="state">状态</param>
        public void ResetStateUIFlag(int flag = -1)
        {
            if (flag == -1)
            {
                Data.popBit = 0; // 重置所有标记
            }
            else
            {
                Data.popBit &= ~(1 << flag); // 重置指定标记
            }
            Save();
        }


        public bool IsNewFunction { get; protected set; } = false; //是否为新功能
        public virtual void FinishNewFunc()
        {
            IsNewFunction = false;
        }
        public virtual void HandleNewFunc() { }

        public bool IsPopupToday
        {
            get => _data.IsBit(ActivityData.BIT_POPUP_TODAY);
            set
            {
                _data.SetBit(ActivityData.BIT_POPUP_TODAY, value);
                Save();
            }
        }

        public bool HasPopForm()
        {
            int popup = GameEntry.UI.CurrentPopUpCount() + GameEntry.UI.CurrentDefaultUpCount();
            return popup >= 2;
        }
    }
}