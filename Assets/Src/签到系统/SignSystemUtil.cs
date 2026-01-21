using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;

/// <summary>
/// 签到系统核心工具类
/// </summary>
public static class SignSystemUtil
{
    #region 常量定义
    /// <summary>
    /// 东八区偏移秒数（UTC+8）
    /// </summary>
    private const int East8OffsetSeconds = 8 * 3600;
    
    /// <summary>
    /// 一天的秒数
    /// </summary>
    private const int OneDaySeconds = 86400;
    
    /// <summary>
    /// Unix基准时间（1970-01-01 UTC）
    /// </summary>
    private static readonly DateTime UnixBaseTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    #endregion

    #region 核心方法1：获取服务器时间对应的年月日整数（20260121）
    /// <summary>
    /// 异步获取服务器时间对应的东八区年月日整数（忽略时分秒）
    /// </summary>
    /// <returns>年月日整数（如20260121），失败返回-1</returns>
    public static async Task<int> GetServerDateIntAsync()
    {
        try
        {
            // 1. 获取服务器UTC时间戳（秒）
            long serverTimeStamp = await LWGSDK.GetServerTime();
            
            // 2. 转换为东八区DateTime
            DateTime east8Time = UnixBaseTime.AddSeconds(serverTimeStamp).AddSeconds(East8OffsetSeconds);
            
            // 3. 转换为年月日整数（20260121）
            int dateInt = east8Time.Year * 10000 + east8Time.Month * 100 + east8Time.Day;
            return dateInt;
        }
        catch (Exception e)
        {
            Debug.LogError($"获取服务器日期失败：{e.Message}");
            return -1;
        }
    }

    /// <summary>
    /// 时间戳转年月日整数（工具方法）
    /// </summary>
    /// <param name="timeStamp">服务器UTC时间戳（秒）</param>
    /// <returns>年月日整数</returns>
    public static int ConvertTimeStampToDateInt(long timeStamp)
    {
        DateTime east8Time = UnixBaseTime.AddSeconds(timeStamp).AddSeconds(East8OffsetSeconds);
        return east8Time.Year * 10000 + east8Time.Month * 100 + east8Time.Day;
    }

    /// <summary>
    /// 年月日整数转DateTime（工具方法）
    /// </summary>
    /// <param name="dateInt">20260121</param>
    /// <returns>东八区DateTime（时分秒为0）</returns>
    public static DateTime ConvertDateIntToDateTime(int dateInt)
    {
        if (dateInt < 1000000) return DateTime.MinValue;
        
        int year = dateInt / 10000;
        int month = (dateInt % 10000) / 100;
        int day = dateInt % 100;
        
        try
        {
            return new DateTime(year, month, day);
        }
        catch
        {
            return DateTime.MinValue;
        }
    }
    #endregion

    #region 核心方法2：判断今日是否可签到
    /// <summary>
    /// 异步判断今日是否可签到
    /// </summary>
    /// <param name="signData">玩家签到数据</param>
    /// <returns>true=可签到；false=不可签到；null=获取时间失败</returns>
    public static async Task<bool?> CheckCanSignTodayAsync(PlayerSignData signData)
    {
        // 1. 基础校验
        if (signData == null)
        {
            Debug.LogError("签到数据为空");
            return false;
        }
        
        // 2. 获取今日日期整数
        int todayDate = await GetServerDateIntAsync();
        if (todayDate == -1) return null;
        
        // 3. 判断是否达到签到周期上限（比如7天）
        if (signData.daySignList.Count >= signData.signCycleLimit)
        {
            Debug.Log($"已达到签到周期上限（{signData.signCycleLimit}天），无法签到");
            return false;
        }
        
        // 4. 判断今日是否已签到
        if (signData.daySignList.Contains(todayDate))
        {
            Debug.Log("今日已签到，无法重复签到");
            return false;
        }
        
        // 5. 满足条件：可签到
        return true;
    }
    #endregion

    #region 核心方法3：计算今日是第几天签到
    /// <summary>
    /// 异步计算今日是第几天签到
    /// </summary>
    /// <param name="signData">玩家签到数据</param>
    /// <returns>签到天数（如1/2/3）；0=不可签到；-1=获取时间失败</returns>
    public static async Task<int> CalculateSignDayIndexAsync(PlayerSignData signData)
    {
        // 1. 基础校验
        if (signData == null) return 0;
        
        // 2. 获取今日日期
        int todayDate = await GetServerDateIntAsync();
        if (todayDate == -1) return -1;
        
        // 3. 判断是否达到周期上限
        if (signData.daySignList.Count >= signData.signCycleLimit) return 0;
        
        // 4. 判断今日是否已签到
        if (signData.daySignList.Contains(todayDate))
        {
            // 已签到：返回当前已签天数
            return signData.daySignList.Count;
        }
        else
        {
            // 未签到：返回已签天数+1
            return signData.daySignList.Count + 1;
        }
    }
    #endregion

    #region 核心方法4：计算间隔天数 & 补签逻辑
    /// <summary>
    /// 计算“首次签到日→当前日”的所有缺失日期（需补签的日期）
    /// </summary>
    /// <param name="signData">玩家签到数据</param>
    /// <returns>需补签的日期列表（20260121）；null=获取时间失败</returns>
    public static async Task<List<int>> CalculateMakeUpSignDatesAsync(PlayerSignData signData)
    {
        // 1. 基础校验
        if (signData == null || signData.firstSignDate == 0)
        {
            Debug.LogError("首次签到日期为空，无法计算补签");
            return new List<int>();
        }
        
        // 2. 获取今日日期
        int todayDate = await GetServerDateIntAsync();
        if (todayDate == -1) return null;
        
        // 3. 转换为DateTime
        DateTime firstSignDt = ConvertDateIntToDateTime(signData.firstSignDate);
        DateTime todayDt = ConvertDateIntToDateTime(todayDate);
        if (firstSignDt == DateTime.MinValue || todayDt == DateTime.MinValue)
        {
            Debug.LogError("日期转换失败");
            return new List<int>();
        }
        
        // 4. 生成“首次签到日→当前日”的所有日期列表
        List<int> allDates = new List<int>();
        for (DateTime dt = firstSignDt; dt <= todayDt; dt = dt.AddDays(1))
        {
            int dateInt = dt.Year * 10000 + dt.Month * 100 + dt.Day;
            allDates.Add(dateInt);
        }
        
        // 5. 排除已签到+已补签的日期，得到需补签的日期
        HashSet<int> signedDates = new HashSet<int>(signData.daySignList);
        signedDates.UnionWith(signData.makeUpSignList); // 合并已补签的日期
        List<int> makeUpDates = allDates.Where(d => !signedDates.Contains(d)).ToList();
        
        // 6. 过滤超出签到周期的日期
        makeUpDates = makeUpDates.Take(signData.signCycleLimit).ToList();
        
        return makeUpDates;
    }

    /// <summary>
    /// 执行补签操作
    /// </summary>
    /// <param name="signData">玩家签到数据</param>
    /// <param name="makeUpDate">要补签的日期（20260121）</param>
    /// <returns>true=补签成功；false=补签失败</returns>
    public static bool DoMakeUpSign(PlayerSignData signData, int makeUpDate)
    {
        if (signData == null || makeUpDate == -1) return false;
        
        // 1. 校验：补签日期未超出周期
        if (signData.daySignList.Count + signData.makeUpSignList.Count >= signData.signCycleLimit)
        {
            Debug.Log("补签后超出签到周期，无法补签");
            return false;
        }
        
        // 2. 校验：未签到、未补签
        if (signData.daySignList.Contains(makeUpDate) || signData.makeUpSignList.Contains(makeUpDate))
        {
            Debug.Log("该日期已签到/补签，无需重复补签");
            return false;
        }
        
        // 3. 执行补签
        signData.makeUpSignList.Add(makeUpDate);
        Debug.Log($"补签成功：{makeUpDate}");
        return true;
    }
    #endregion

    #region 核心方法5：计算今日剩余秒数（倒计时用）
    /// <summary>
    /// 异步计算今日剩余秒数（到次日0点）
    /// </summary>
    /// <returns>剩余秒数；-1=失败</returns>
    public static async Task<float> CalculateTodayRemainingSecondsAsync()
    {
        try
        {
            long serverTimeStamp = await LWGSDK.GetServerTime();
            DateTime east8Now = UnixBaseTime.AddSeconds(serverTimeStamp).AddSeconds(East8OffsetSeconds);
            DateTime east8NextDay = new DateTime(east8Now.Year, east8Now.Month, east8Now.Day).AddDays(1);
            
            float remainingSec = (float)(east8NextDay - east8Now).TotalSeconds;
            return Mathf.Max(remainingSec, 0);
        }
        catch (Exception e)
        {
            Debug.LogError($"计算剩余秒数失败：{e.Message}");
            return -1;
        }
    }
    #endregion
}