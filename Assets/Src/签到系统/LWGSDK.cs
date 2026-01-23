using System;
using System.Threading.Tasks;

/// <summary>
/// 模拟LWGSDK（测试时用，实际替换为真实SDK）
/// </summary>
public static class LWGSDK
{
    #region 最简单的返回当前时间戳方法用于测试
    /// <summary>
    /// 模拟获取服务器UTC时间戳（秒）
    /// </summary>
    /// <returns>当前本地时间对应的UTC时间戳</returns>
    public static Task<long> GetServerTime()
    {
        // return GetServerTime3();//可以用来测试时间偏移，调整TimeOffsetSeconds即可
        DateTime utcNow = DateTime.UtcNow;
        DateTime unixBase = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        long timeStamp = (long)(utcNow - unixBase).TotalSeconds;
        return Task.FromResult(timeStamp);
    }
    #endregion



    #region 手动配置固定时间戳用来测试
    /// <summary>
    /// 手动配置的固定时间戳（默认值：2026-01-21 12:00:00 UTC → 东八区2026-01-21 20:00:00）
    /// 可在Inspector/代码中修改，测试不同日期
    /// </summary>
    public static long CustomServerTimeStamp = 1737422400;
    public static Task<long> GetServerTime2()
    {
        // 直接返回手动配置的时间戳
        return Task.FromResult(CustomServerTimeStamp);
    }

    /// <summary>
    /// 辅助方法：年月日时分秒 → UTC时间戳（方便配置）
    /// </summary>
    /// <param name="year">年</param>
    /// <param name="month">月</param>
    /// <param name="day">日</param>
    /// <param name="hour">时（UTC）</param>
    /// <param name="minute">分</param>
    /// <param name="second">秒</param>
    /// <returns>UTC时间戳（秒）</returns>
    public static long ConvertDateTimeToTimeStamp(int year, int month, int day, int hour = 0, int minute = 0, int second = 0)
    {
        DateTime utcTime = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
        DateTime unixBaseTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return (long)(utcTime - unixBaseTime).TotalSeconds;
    }
    #endregion

    #region 模拟在服务器时间上加减偏移量用来测试
    /// <summary>
    /// 时间偏移量（秒）：正数=服务器时间比本地UTC快，负数=慢
    /// 比如：3600 = 快1小时，-1800 = 慢30分钟
    /// </summary>
    public static int TimeOffsetSeconds = 3600;//需要偏移多少个小时就乘以多少，测试方便

    public static Task<long> GetServerTime3()
    {
        // 1. 本地UTC时间戳
        DateTime utcNow = DateTime.UtcNow;
        DateTime unixBaseTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        long localTimeStamp = (long)(utcNow - unixBaseTime).TotalSeconds;

        // 2. 加上偏移量，模拟服务器时间
        long serverTimeStamp = localTimeStamp + TimeOffsetSeconds;

        return Task.FromResult(serverTimeStamp);
    }
    #endregion
}