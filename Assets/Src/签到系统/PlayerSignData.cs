using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家签到数据（JSON序列化/反序列化用）
/// </summary>
[Serializable]
public class PlayerSignData
{
    /// <summary>
    /// 已签到的日期列表（格式：20260121）
    /// </summary>
    public List<int> daySignList = new List<int>();
    
    /// <summary>
    /// 补签的日期列表（格式：20260121）
    /// </summary>
    public List<int> makeUpSignList = new List<int>();
    
    /// <summary>
    /// 签到周期上限（默认7天）
    /// </summary>
    public int signCycleLimit = 7;
    
    /// <summary>
    /// 首次签到的日期（格式：20260121，用于计算间隔天数）
    /// </summary>
    public int firstSignDate;
}