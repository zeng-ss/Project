using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using CasualGame;

public enum TimeFormatType
{
    DD_HH_MM_SS,//01:23:59:59
    DD_HH_MM,//01:23:59
    HH_MM_SS,//23:59:59
    MM_SS,//10:59
    HH_MM,//10:59
    DD_HH,
    DD,
    MM,
    HH,
    SS,
}

public class TimeUtils
{
    /// <summary>
    /// 时间格式转换
    /// </summary>
    /// <param name="totalSeconds"> 倒计时 ：s </param>
    /// <param name="formatType"> 需要的时间格式 </param>
    /// <param name="placeholder"> 格式占位符 </param>
    /// <param name="fillzero"> 单个数字 小于0 是否需要补充0 </param>
    /// <returns></returns>
    public static string TimeFormat(int totalSeconds, TimeFormatType formatType, string placeholder, bool fillzero = true, bool rounding = true)
    {
        if (rounding)
        {
            //- 所有的倒计时，24h统一写23h，0h统一写1h
            if (totalSeconds > 0 && totalSeconds < 3600)
            {
                totalSeconds = 3600;
            }
            else if (totalSeconds % 86400 == 0)
            {
                totalSeconds -= 1;
            }
        }

        if (totalSeconds < 0) totalSeconds = 0;


        int days = (totalSeconds / 3600) / 24;
        int hours = (totalSeconds / 3600) - (days * 24);
        int minutes = (totalSeconds - (hours * 3600) - (days * 86400)) / 60;
        int seconds = totalSeconds - (hours * 3600) - (minutes * 60) - (days * 86400);

        switch (formatType)
        {
            case TimeFormatType.DD_HH_MM_SS:
                return string.Format(placeholder,
                    fillzero && days < 10 ? "0" + days : days.ToString(),
                    fillzero && hours < 10 ? "0" + hours : hours.ToString(),
                    fillzero && minutes < 10 ? "0" + minutes : minutes.ToString(),
                    fillzero && seconds < 10 ? "0" + seconds : seconds.ToString());

            case TimeFormatType.DD_HH_MM:
                return string.Format(placeholder,
                    fillzero && days < 10 ? "0" + days : days.ToString(),
                    fillzero && hours < 10 ? "0" + hours : hours.ToString(),
                    fillzero && minutes < 10 ? "0" + minutes : minutes.ToString());

            case TimeFormatType.HH_MM_SS:
                hours = days * 24 + hours;
                return string.Format(placeholder,
                    fillzero && hours < 10 ? "0" + hours : hours.ToString(),
                    fillzero && minutes < 10 ? "0" + minutes : minutes.ToString(),
                    fillzero && seconds < 10 ? "0" + seconds : seconds.ToString());
            case TimeFormatType.HH_MM:
                hours = days * 24 + hours;
                return string.Format(placeholder,
                    fillzero && hours < 10 ? "0" + hours : hours.ToString(),
                    fillzero && minutes < 10 ? "0" + minutes : minutes.ToString());

            case TimeFormatType.MM_SS:
                minutes = (days * 24 + hours) * 60 + minutes;
                return string.Format(placeholder,
                    fillzero && minutes < 10 ? "0" + minutes : minutes.ToString(),
                    fillzero && seconds < 10 ? "0" + seconds : seconds.ToString());

            case TimeFormatType.DD_HH:
                //四舍五入
                // int addHour = (minutes > 30 && rounding) ? 1 : 0;
                // hours += addHour;
                return string.Format(placeholder,
                    fillzero && days < 10 ? "0" + days : days.ToString(),
                    fillzero && hours < 10 ? "0" + hours : hours.ToString());

            case TimeFormatType.DD:
                return string.Format(placeholder,
                    fillzero && days < 10 ? "0" + days : days.ToString());

            case TimeFormatType.MM:
                minutes = (days * 24 + hours) * 60 + minutes;
                return string.Format(placeholder,
                    fillzero && minutes < 10 ? "0" + minutes : minutes.ToString());

            case TimeFormatType.SS:
                return string.Format(placeholder,
                    fillzero && totalSeconds < 10 ? "0" + totalSeconds : totalSeconds.ToString());

            case TimeFormatType.HH:
                hours = (days * 24 + hours);
                int _addHour = minutes > 30 ? 1 : 0;
                hours += _addHour;
                return string.Format(placeholder, hours);

            default:
                break;
        }

        return "format error";
    }

    public static string GetRemainTimeText(int totalSeconds)
    {
        if (totalSeconds < 60 * 60)
            return TimeFormat(totalSeconds, TimeFormatType.MM_SS, "{0}M{1}S", true, false);
        else if (totalSeconds < 24 * 60 * 60)
            return TimeFormat(totalSeconds, TimeFormatType.HH_MM, "{0}H{1}M", true, false);
        else
            return TimeFormat(totalSeconds, TimeFormatType.DD_HH, "{0}D{1}H", false, false);
    }

    // 全用NowTimeStamp去调用！！
    public static int GetTimeStampOfSeconds()
    {
        return (int)NowTimeStamp;
    }

    public static long GetTimeStampOfMilliseconds()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        return Convert.ToInt64(ts.TotalMilliseconds);
    }

    public static DateTime GetToday()
    {
        return GetDateTime(GetTimeStampOfSeconds());
    }

    public static DateTime GetDateTime(long totalSeconds)
    {
        DateTime utcStartTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime utcDateTime = utcStartTime.AddSeconds(totalSeconds);
        return utcDateTime;
    }

    /// <summary>
    /// 返回当前时间的秒值
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public static long ConvertDateTimeLong(System.DateTime time)
    {
        System.DateTime utcStartTime = new System.DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return (long)(time.ToUniversalTime() - utcStartTime).TotalSeconds;
    }

    /// <summary>
    /// 当前系统时间戳：单位：s
    /// </summary>
    public static long NowTimeStamp => GameManager.Instance.GetTimeStampOfSeconds();

    public static int DateDiff(long startTS, long endTS)
    {
        DateTime startDate = Convert.ToDateTime(GetDateTime(startTS).ToShortDateString());
        DateTime endDate = Convert.ToDateTime(GetDateTime(endTS).ToShortDateString());
        TimeSpan sp = endDate.Subtract(startDate);
        return sp.Days;
    }

    public static DateTime GetNextDay()
    {
        DateTime d = GetDateTime(NowTimeStamp);
        d = new DateTime(d.Year, d.Month, d.Day, 0, 0, 0, DateTimeKind.Utc);
        d = d.AddSeconds(24 * 3600 * 1);
        return d;
    }

    public static bool IsSameDay(DateTime dateTime)
    {
        return IsSameDayUseServerTime(dateTime);
    }

    public static bool IsToday(DateTime dateTime)
    {
        return IsSameDayUseServerTime(dateTime);
    }

    //是否为传入时间下个自然日 以及以后的日子
    public static bool IsNextNDay(int timestamp)
    {
        DateTime theDate = GetDateTime(timestamp);
        if (IsSameDayUseServerTime(theDate))
            return false;
        return true;
    }

    public static bool IsSameDayUseServerTime(DateTime dateTime, bool IsNextNDay = false)
    {
        DateTime today = GetDateTime(GetTimeStampOfSeconds());
        if (today.Year == dateTime.Year
            && today.Month == dateTime.Month
            && today.Day == dateTime.Day)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static int GetSubtractDay(int time)
    {
        DateTime now = DateTime.UtcNow;
        DateTime d = GetDateTime(time);
        d = new DateTime(d.Year, d.Month, d.Day, 0, 0, 0, DateTimeKind.Utc);
        TimeSpan t = now.Subtract(d);
        return Mathf.Abs(t.Days);
    }

    #region GMT-4 时区相关

    private const int GMT4_OFFSET_HOURS = -4;

    /// <summary>
    /// 获取当前时间对应的 GMT-4 时区下周一 00:00:00 的时间戳（秒）
    /// </summary>
    /// <returns>下周一00:00:00的时间戳（秒）</returns>
    public static int GetGMT4NextMondayTimeStampOfSecond()
    {
        DateTime utcNow = GetDateTime(NowTimeStamp);
        DateTime gmt4Now = utcNow.AddHours(GMT4_OFFSET_HOURS);

        int daysUntilMonday = ((int)DayOfWeek.Monday - (int)gmt4Now.DayOfWeek + 7) % 7;
        if (daysUntilMonday == 0)
        {
            daysUntilMonday = 7;
        }

        DateTime gmt4Monday = new DateTime(gmt4Now.Year, gmt4Now.Month, gmt4Now.Day, 0, 0, 0, DateTimeKind.Unspecified);
        gmt4Monday = gmt4Monday.AddDays(daysUntilMonday);

        DateTime utcMonday = DateTime.SpecifyKind(gmt4Monday.AddHours(-GMT4_OFFSET_HOURS), DateTimeKind.Utc);
        int utcMondayTimeStamp = (int)new DateTimeOffset(utcMonday).ToUnixTimeSeconds();
        return utcMondayTimeStamp;
    }

    /// <summary>
    /// 获取当前时间对应的 GMT-4 时区本周一 00:00:00 的时间戳（秒）
    /// </summary>
    /// <returns>本周一00:00:00的时间戳（秒）</returns>
    public static int GetGMT4ThisMondayTimeStampOfSecond()
    {
        DateTime utcNow = GetDateTime(NowTimeStamp);
        DateTime gmt4Now = utcNow.AddHours(GMT4_OFFSET_HOURS);

        // 计算今天是本周第几天（周一=0，周二=1，...周日=6）
        int daysSinceMonday = ((int)gmt4Now.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;

        DateTime gmt4ThisMonday = new DateTime(gmt4Now.Year, gmt4Now.Month, gmt4Now.Day, 0, 0, 0, DateTimeKind.Unspecified);
        gmt4ThisMonday = gmt4ThisMonday.AddDays(-daysSinceMonday);

        DateTime utcThisMonday = DateTime.SpecifyKind(gmt4ThisMonday.AddHours(-GMT4_OFFSET_HOURS), DateTimeKind.Utc);
        int utcThisMondayTimeStamp = (int)new DateTimeOffset(utcThisMonday).ToUnixTimeSeconds();
        return utcThisMondayTimeStamp;
    }

    #endregion

    #region 本地时区

    /// <summary>
    /// 获取用户本地时区下周一 00:00:00 对应的 UTC 时间戳（秒）
    /// </summary>
    /// <returns>本地下周一 00:00:00 时刻的 Unix 时间戳（秒）</returns>
    public static int GetLocalNextMondayTimeStampOfSecond()
    {
        DateTime utcNow = GetDateTime(NowTimeStamp);
        DateTime localNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, TimeZoneInfo.Local);

        int daysUntilMonday = ((int)DayOfWeek.Monday - (int)localNow.DayOfWeek + 7) % 7;
        if (daysUntilMonday == 0)
        {
            daysUntilMonday = 7;
        }

        DateTime localNextMonday = new DateTime(localNow.Year, localNow.Month, localNow.Day, 0, 0, 0, DateTimeKind.Unspecified);
        localNextMonday = localNextMonday.AddDays(daysUntilMonday);

        DateTime utcNextMonday = TimeZoneInfo.ConvertTimeToUtc(localNextMonday, TimeZoneInfo.Local);
        return (int)new DateTimeOffset(utcNextMonday).ToUnixTimeSeconds();
    }

    #endregion
}
