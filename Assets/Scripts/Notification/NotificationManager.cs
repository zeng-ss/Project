using System.Diagnostics;
using System;
//------------------------------------------------------------
// File : NotificationManager.cs
// Email: mailto:zhuofeng.cai@kingboat.io
// Desc : 
//------------------------------------------------------------
using System.Collections;
using GameFramework.DataTable;
using UnityGameFramework.Runtime;
#if UNITY_ANDROID
using Unity.Notifications.Android;
#elif UNITY_IOS
using Unity.Notifications.iOS;
#endif

namespace CasualGame
{

    public class NotificationBaseClass : MonoSingleton<NotificationManager>
    {
        protected DRNotification[] notifications;

        public virtual void Init()
        {
            notifications = GameEntry.DataTable.GetDataTable<DRNotification>().GetAllDataRows();
            PushLog();
        }

        protected virtual void PushLog()
        {

        }
    }

#if UNITY_ANDROID
    public class AndroidNotificationManager : NotificationBaseClass
    {
        private const string androidChannel = "wb_local_push";

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                ReSendNotification();
                PushLog();
            }
        }

        protected override void PushLog()
        {
            var notificationIntentData = AndroidNotificationCenter.GetLastNotificationIntent();
            if (notificationIntentData != null)
            {
                var notification = notificationIntentData.Notification.Title;
                int id = 0;
                int.TryParse(notificationIntentData.Notification.IntentData, out id);
            }
        }

        override public void Init()
        {
            base.Init();
            ReSendNotification();
            AndroidNotificationCenter.NotificationReceivedCallback receivedNotificationHandler = delegate (AndroidNotificationIntentData data)
            {
                int id = 0;
                int.TryParse(data.Notification.IntentData, out id);
            };

            AndroidNotificationCenter.OnNotificationReceived += receivedNotificationHandler;
        }

        protected void ReSendNotification()
        {
            AndroidNotificationCenter.CancelAllNotifications();//清除上次注册的通知
            var channel = new AndroidNotificationChannel()
            {
                Id = androidChannel,
                Name = "Default Channel",
                Importance = Importance.Default,
                Description = "Generic notifications",
                EnableVibration = true,
                CanShowBadge = true,
                EnableLights = true,
            };
            AndroidNotificationCenter.RegisterNotificationChannel(channel);

            StopAllCoroutines();
            StartCoroutine(ScheduleNotificationsIE());
        }

        private IEnumerator ScheduleNotificationsIE()
        {
            DateTime now = DateTime.Now;
            /// 注册未来30天的推送
            for (int i = 0; i < 30; i++)
            {
                int day = now.Day - 1;
                DRNotification drNotificationMorning = notifications[day * 3];
                DRNotification drNotificationAfternoon = notifications[day * 3 + 1];
                DRNotification drNotificationNight = notifications[day * 3 + 2];
                DateTime scheduleTimeMorning = new DateTime(now.Year, now.Month, now.Day, 11, 54, 0);
                DateTime scheduleTimeAfternoon = new DateTime(now.Year, now.Month, now.Day, 16, 54, 0);
                DateTime scheduleTimeNight = new DateTime(now.Year, now.Month, now.Day, 21, 54, 0);
                yield return ScheduleANotification(drNotificationMorning, scheduleTimeMorning);
                yield return ScheduleANotification(drNotificationAfternoon, scheduleTimeAfternoon);
                yield return ScheduleANotification(drNotificationNight, scheduleTimeNight);
                // Log.Debug("schedule month" + now.Month + " day" + now.Day);
                now = now.AddDays(1);
            }
        }

        private IEnumerator ScheduleANotification(DRNotification dRNotification, DateTime dt)
        {
            DateTime now = DateTime.Now;
            if (now.CompareTo(dt) > 0)
            {
                yield break;
            }
            var notification = new AndroidNotification();
            notification.Title = dRNotification.Title;
            notification.Text = dRNotification.Description;
            notification.FireTime = dt;
            notification.IntentData = dRNotification.Id.ToString();
            // notification.LargeIcon = "icon_0";
            AndroidNotificationCenter.SendNotification(notification, androidChannel);
            yield return YieldHepler.WaitForEndOfFrame;
        }
    }

#elif UNITY_IOS
    public class IOSNotificationManager : NotificationBaseClass
    {
        private const string iosNotificationId = "wb_local_noti";

        private const string kiOSScheduleNotificationKey = "kiOSScheduleNotificationKey";

        private void OnApplicationFocus(bool focusStatus)
        {
            if (focusStatus)
            {
                StartCoroutine(ReSendNotification());
                PushLog();
            }
        }

        protected override void PushLog()
        {
            base.PushLog();
            var notification = iOSNotificationCenter.GetLastRespondedNotification();
            if (notification != null)
            {
                int id = 0;
                int.TryParse(notification.Data, out id);
                GLog.notification_open(notification.Title, id);
            }
        }

        override public void Init()
        {
            base.Init();
            StartCoroutine(ReSendNotification());
            iOSNotificationCenter.OnNotificationReceived += notification =>
            {
                int id = 0;
                int.TryParse(notification.Data, out id);
                GLog.notification_rec(notification.Title, id);
            };
        }

        protected IEnumerator ReSendNotification()
        {
            iOSNotificationCenter.ApplicationBadge = 0;
            iOSNotificationCenter.RemoveAllScheduledNotifications();

            // if (GameEntry.Setting.GetBool(kiOSScheduleNotificationKey, false))
            // {
            //     return;
            // }

            DateTime now = DateTime.Now;
            int day = now.Day - 1;
            for (int i = 0; i < 20; i++)
            {
                int dayIndex = day * 3;
                for (int j = 0; j < 3; j++)
                {
                    DRNotification rNotification = notifications[dayIndex + j];
                    int hour;
                    int min;
                    switch (j)
                    {
                        case 0:
                            hour = 11;
                            min = 54;
                            break;
                        case 1:
                            hour = 16;
                            min = 54;
                            break;
                        case 2:
                            hour = 21;
                            min = 54;
                            break;
                        default:
                            hour = 21;
                            min = 54;
                            break;
                    }

                    var calendarTrigger = new iOSNotificationCalendarTrigger()
                    {
                        // Year = 2020,
                        // Month = 6,
                        Day = day + 1,
                        Hour = hour,
                        Minute = min,
                        Second = 0,
                        Repeats = true
                    };

                    var notification = new iOSNotification()
                    {
                        // You can specify a custom identifier which can be used to manage the notification later.
                        // If you don't provide one, a unique string will be generated automatically.
                        Identifier = string.Format("{0}_{1}", iosNotificationId, rNotification.Id),
                        Title = rNotification.Title,
                        Body = rNotification.Description,
                        // Subtitle = "This is a subtitle, something, something important...",
                        ShowInForeground = true,
                        ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound | PresentationOption.Badge),
                        CategoryIdentifier = "category_a",
                        ThreadIdentifier = "thread1",
                        Trigger = calendarTrigger,
                        Badge = 1,
                        Data = rNotification.Id.ToString()
                    };
                    // UnityEngine.Debug.Log(rNotification.Id + " scheduled");
                    iOSNotificationCenter.ScheduleNotification(notification);
                    yield return YieldHepler.WaitForEndOfFrame;
                }
                day = (day + 1) % 31;
            }

            GameEntry.Setting.SetBool(kiOSScheduleNotificationKey, true);
        }
    }
#endif

    public class NotificationManager
#if UNITY_ANDROID
    : AndroidNotificationManager
#elif UNITY_IOS
    : IOSNotificationManager
#else
    : NotificationBaseClass
#endif
    {
        // private void OnApplicationFocus(bool hasFocus)
        // {
        //     if (hasFocus)
        //         ReSendNotification();
        // }
    }
}