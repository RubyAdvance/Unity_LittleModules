using System.Collections.Generic;
using UnityEngine;

public class TestLoopScroll : MonoBehaviour
{
    public LoopScrollView loopScrollView;

    void Start()
    {
        // 模拟100个任务数据
        List<object> taskList = new List<object>();
        for (int i = 0; i < 100; i++)
        {
            taskList.Add($"任务{i+1}：完成每日打卡");
        }

        // 一行代码设置数据源，启动无限循环列表
        loopScrollView.SetDataList(taskList);

        // 可选：滚动到指定索引（比如第20个任务）
        // loopScrollView.ScrollToIndex(19);
    }
}