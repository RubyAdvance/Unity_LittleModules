using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SignSystemTest : MonoBehaviour
{
    // UI引用（需在Inspector赋值）
    public Text canSignText;
    public Text signDayIndexText;
    public Text makeUpDatesText;
    public SignCountdownManager countdownManager;
    public Button signBtn;
    public Button makeUpBtn;

    // 玩家签到数据（模拟存储）
    private PlayerSignData _testSignData;

    private void Start()
    {
        // 初始化测试数据
        _testSignData = new PlayerSignData();
        _testSignData.signCycleLimit = 7; // 7天周期
        
        // 绑定按钮事件
        signBtn.onClick.AddListener(OnSignBtnClick);
        makeUpBtn.onClick.AddListener(OnMakeUpBtnClick);
        
        // 初始化签到界面
        InitSignUI();
    }

    /// <summary>
    /// 初始化签到界面
    /// </summary>
    private async void InitSignUI()
    {
        // 1. 判断今日是否可签到
        bool? canSign = await SignSystemUtil.CheckCanSignTodayAsync(_testSignData);
        canSignText.text = canSign == null ? "获取时间失败" : (canSign.Value ? "今日可签到" : "今日不可签到");
        
        // 2. 计算今日签到天数
        int signDayIndex = await SignSystemUtil.CalculateSignDayIndexAsync(_testSignData);
        signDayIndexText.text = signDayIndex == -1 ? "获取时间失败" : $"今日是第{signDayIndex}天签到";
        
        // 3. 计算补签日期
        List<int> makeUpDates = await SignSystemUtil.CalculateMakeUpSignDatesAsync(_testSignData);
        makeUpDatesText.text = makeUpDates == null ? "获取时间失败" : 
            (makeUpDates.Count > 0 ? $"需补签：{string.Join(",", makeUpDates)}" : "无需补签");
        
        // 4. 今日已签到则显示倒计时
        if (canSign == false && _testSignData.daySignList.Count > 0)
        {
            float remainingSec = await SignSystemUtil.CalculateTodayRemainingSecondsAsync();
            if (remainingSec > 0)
            {
                countdownManager.StartCountdown(remainingSec);
            }
        }
        else
        {
            countdownManager.StopCountdown();
        }
    }

    /// <summary>
    /// 签到按钮点击
    /// </summary>
    private async void OnSignBtnClick()
    {
        bool? canSign = await SignSystemUtil.CheckCanSignTodayAsync(_testSignData);
        if (canSign != true)
        {
            Debug.Log("今日不可签到");
            return;
        }
        
        // 1. 获取今日日期
        int todayDate = await SignSystemUtil.GetServerDateIntAsync();
        if (todayDate == -1) return;
        
        // 2. 记录签到日期
        _testSignData.daySignList.Add(todayDate);
        
        // 3. 首次签到则记录首次日期
        if (_testSignData.firstSignDate == 0)
        {
            _testSignData.firstSignDate = todayDate;
        }
        
        Debug.Log($"签到成功：{todayDate}");
        
        // 4. 刷新UI
        InitSignUI();
    }

    /// <summary>
    /// 补签按钮点击
    /// </summary>
    private async void OnMakeUpBtnClick()
    {
        List<int> makeUpDates = await SignSystemUtil.CalculateMakeUpSignDatesAsync(_testSignData);
        if (makeUpDates == null || makeUpDates.Count == 0)
        {
            Debug.Log("无可用补签日期");
            return;
        }
        
        // 补签第一个缺失日期
        int targetDate = makeUpDates[0];
        bool success = SignSystemUtil.DoMakeUpSign(_testSignData, targetDate);
        if (success)
        {
            Debug.Log($"补签{targetDate}成功");
            InitSignUI();
        }
    }
}