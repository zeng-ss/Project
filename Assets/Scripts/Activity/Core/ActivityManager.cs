using System;
using System.Collections.Generic;
using UnityEngine;
using CasualGame;
using System.Linq;

namespace Custom.Activity
{
    public class ActivityManager : MonoBehaviour
    {
        private static ActivityManager _instance;
        public static ActivityManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("ActivityManager");
                    _instance = go.AddComponent<ActivityManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        private Dictionary<Type, IActivity> _activities;
        private float _updatePerSecond = 0;
        public GameFramework.DataTable.IDataTable<DRActivityInfo> tableActivityInfo;
        public bool isInited = false;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            float realTime = Time.unscaledDeltaTime;

            _updatePerSecond += realTime;
            if (_updatePerSecond >= 1)
            {
                _updatePerSecond = 0;
                foreach (var activity in _activities.Values)
                {
                    try
                    {
                        activity.UpdatePerSecond();
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogError($"更新活动 {activity.ActivityType} 失败: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            _activities = new Dictionary<Type, IActivity>();
            tableActivityInfo = GameEntry.DataTable.GetDataTable<DRActivityInfo>();
            RegisterActivity(new HomePanelActivity());

#if UNITY_EDITOR
            var checkActivityTypes = System.Reflection.Assembly.GetExecutingAssembly()
                                .GetTypes()
                                .Where(t => t.IsClass && !t.IsAbstract && typeof(IActivity).IsAssignableFrom(t) && t != typeof(IActivity))
                                .ToList();

            foreach (var activityType in checkActivityTypes)
            {
                var checkActivity = Activator.CreateInstance(activityType) as IActivity;
                Type type = checkActivity.GetType();
                if (!_activities.ContainsKey(type))
                {
                    UnityEngine.Debug.LogError($"==> lyly 活动类型 {type} 未注册");
                }
            }
#endif

            InitializeActivities();
            isInited = true;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void InitializeActivities()
        {
            foreach (var activity in _activities.Values)
            {
                try
                {
                    activity.Initial();
                    activity.CheckActivity();
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError($"初始化活动 {activity.ActivityType} 失败: {e.Message}");
                }
            }
        }

        /// <summary>
        /// 新的一天
        /// </summary>
        public void NewDay()
        {
            foreach (var activity in _activities.Values)
            {
                try
                {
                    activity.NewDay();
                    activity.CheckActivity();
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError($"新的一天 {activity.ActivityType} 失败: {e.Message}");
                }
            }
        }

        /// <summary>
        /// 手动注册活动
        /// </summary>
        public void RegisterActivity(IActivity activity)
        {
            if (activity == null) return;

            Type type = activity.GetType();
            if (_activities.ContainsKey(type))
            {
                UnityEngine.Debug.LogWarning($"活动类型 {activity.ActivityType} 已存在，将被替换");
            }

            _activities[type] = activity;
        }

        /// <summary>
        /// 获取指定类型的活动
        /// </summary>
        public T TryGetActivity<T>() where T : class, IActivity
        {
            if (_activities.TryGetValue(typeof(T), out var activity))
            {
                return activity as T;
            }
            UnityEngine.Debug.LogError($"获取活动 {typeof(T)} 失败");
            return null;
        }

        /// <summary>
        /// 获取指定类型的活动
        /// </summary>
        public IActivity TryGetActivityByType(ActivityType activityType)
        {
            foreach (var item in _activities)
            {
                if (item.Value.ActivityType == activityType)
                {
                    return item.Value;
                }
            }
            UnityEngine.Debug.LogError($"获取活动 {activityType} 失败");
            return null;
        }

        /// <summary>
        /// 活动检查
        /// </summary>
        public void CheckActivity(string reason = "")
        {
            foreach (var activity in _activities.Values)
            {
                activity.CheckActivity(reason);
            }
            EventManager.NotifyEvent(Constant.Event.UpdateBtnActive);
        }

        /// <summary>
        /// 获取显示入口的活动
        /// </summary>
        /// <returns></returns>
        public List<IHomeActivity> GetShowEntryActivities()
        {
            List<IHomeActivity> activities = new List<IHomeActivity>();
            foreach (var activity in _activities.Values)
            {
                if (activity is IHomeActivity homeActivity && homeActivity.ShowEntry && homeActivity.ActivityPos != ActivityPos.None)
                {
                    activities.Add(homeActivity);
                }
            }
            return activities;
        }

        /// <summary>
        /// 获取活动
        /// </summary>
        public static T GetActivity<T>() where T : class, IActivity
        {
            return Instance.TryGetActivity<T>();
        }

        /// <summary>
        /// 检查新功能
        /// </summary>
        /// <returns></returns>
        public bool CheckNewFunction()
        {
            bool hasNewFunc = false;
            foreach (var activity in _activities.Values)
            {
                if (activity.IsNewFunction && activity is IHomeActivity homeActivity)
                {
                    homeActivity.HandleNewFunc();
                    hasNewFunc = true;
                }
            }
            return hasNewFunc;
        }

        /// <summary>
        /// 检查自动弹窗
        /// </summary>
        /// <param name="backHome"></param>
        public void CheckAutoPopup(BackHomeType backHome)
        {
            foreach (var activity in _activities.Values)
            {
                if (activity is IHomeActivity homeActivity)
                {
                    homeActivity.CheckAutoPopup(backHome);
                }
            }
        }
    }
}
