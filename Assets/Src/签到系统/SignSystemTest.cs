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
    public Button clearDataBtn; // 新增：清空数据按钮
    // 新增：周数显示文本
    public Text weekOfYearText;

    // 玩家签到数据（从PlayerPrefs读取）
    private PlayerSignData _testSignData;

    private void Start()
    {
        // 1. 从PlayerPrefs读取签到数据（持久化）
        _testSignData = SignDataPersistence.LoadSignData();
        // 初始化周期上限（首次读取时设置）
        if (_testSignData.signCycleLimit == 0)
        {
            _testSignData.signCycleLimit = 7;
            SignDataPersistence.SaveSignData(_testSignData); // 保存初始配置
        }

        // 2. 绑定按钮事件
        signBtn.onClick.AddListener(OnSignBtnClick);
        makeUpBtn.onClick.AddListener(OnMakeUpBtnClick);
        clearDataBtn.onClick.AddListener(OnClearDataBtnClick); // 绑定清空按钮

        // 3. 初始化签到界面
        InitSignUI();
    }

    /// <summary>
    /// 初始化签到界面
    /// </summary>
    private async void InitSignUI()
    {
        // 原有逻辑不变...
        bool? canSign = await SignSystemUtil.CheckCanSignTodayAsync(_testSignData);
        canSignText.text = canSign == null ? "获取时间失败" : (canSign.Value ? "今日可签到" : "今日不可签到");

        int signDayIndex = await SignSystemUtil.CalculateSignDayIndexAsync(_testSignData);
        signDayIndexText.text = signDayIndex == -1 ? "获取时间失败" : $"今日是第{signDayIndex}天签到";

        List<int> makeUpDates = await SignSystemUtil.CalculateMakeUpSignDatesAsync(_testSignData);
        makeUpDatesText.text = makeUpDates == null ? "首次签到日期为空" :
            (makeUpDates.Count > 0 ? $"需补签：{string.Join(",", makeUpDates)}" : "无需补签");
        // 修正：获取服务器时间对应的年份+周数（核心修改）
        var yearAndWeek = await SignSystemUtil.GetServerYearAndWeekAsync();
        int serverYear = yearAndWeek.Item1;
        int serverWeek = yearAndWeek.Item2;
        // 显示修正：年份用服务器时间的年份，而非本地DateTime.Now.Year
        weekOfYearText.text = (serverYear == -1 || serverWeek == -1)
            ? "获取周数失败"
            : $"当前是{serverYear}年第{serverWeek}周";

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
    /// 签到按钮点击（新增存储逻辑）
    /// </summary>
    private async void OnSignBtnClick()
    {
        bool? canSign = await SignSystemUtil.CheckCanSignTodayAsync(_testSignData);
        if (canSign != true)
        {
            Debug.Log("今日不可签到");
            return;
        }

        int todayDate = await SignSystemUtil.GetServerDateIntAsync();
        if (todayDate == -1) return;

        // 记录签到日期
        _testSignData.daySignList.Add(todayDate);
        if (_testSignData.firstSignDate == 0)
        {
            _testSignData.firstSignDate = todayDate;
        }

        // 核心：签到后立即存储数据到PlayerPrefs
        SignDataPersistence.SaveSignData(_testSignData);

        Debug.Log($"签到成功：{todayDate}");
        InitSignUI();
    }

    /// <summary>
    /// 补签按钮点击（新增存储逻辑）
    /// </summary>
    private async void OnMakeUpBtnClick()
    {
        List<int> makeUpDates = await SignSystemUtil.CalculateMakeUpSignDatesAsync(_testSignData);
        if (makeUpDates == null || makeUpDates.Count == 0)
        {
            Debug.Log("无可用补签日期");
            return;
        }

        int targetDate = makeUpDates[0];
        bool success = SignSystemUtil.DoMakeUpSign(_testSignData, targetDate);
        if (success)
        {
            // 核心：补签后立即存储数据
            SignDataPersistence.SaveSignData(_testSignData);
            Debug.Log($"补签{targetDate}成功");
            InitSignUI();
        }
    }

    /// <summary>
    /// 新增：清空数据按钮点击
    /// </summary>
    private void OnClearDataBtnClick()
    {
        SignDataPersistence.ClearSignData();
        // 重置本地数据并刷新UI
        _testSignData = new PlayerSignData();
        _testSignData.signCycleLimit = 7;
        InitSignUI();
        Debug.Log("已清空所有签到数据，重置为初始状态");
    }
}