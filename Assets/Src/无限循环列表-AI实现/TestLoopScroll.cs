using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLoopScroll : MonoBehaviour
{
    public LoopScrollView loopScrollView;
    public int targetIndex = 99; // 可以在Inspector中调整测试

    void Start()
    {
        // 模拟100个任务数据
        List<object> taskList = new List<object>();
        for (int i = 0; i < 100; i++)
        {
            taskList.Add($"任务{i + 1}：完成每日打卡");
        }

        // 一行代码设置数据源，启动无限循环列表
        loopScrollView.SetDataList(taskList);
        
        // 添加UI按钮测试
        StartCoroutine(TestAfterFrame());
    }
    
    IEnumerator TestAfterFrame()
    {
        yield return null; // 等待一帧确保布局完成
        
        // 测试滚动到不同位置
        Debug.Log("开始测试滚动到指定位置...");
        
        // 测试滚动到中间
        // loopScrollView.ScrollToIndexCenter(49); // 任务50
        
        // 测试滚动到接近底部
        // loopScrollView.ScrollToIndexCenter(79); // 任务80
        
        // 测试滚动到底部
        // loopScrollView.ScrollToIndexCenter(9); // 任务100
    }
    
    // 可以添加一个UI按钮来测试不同位置
    public void TestScrollToIndex(int index)
    {
        if (loopScrollView != null)
        {
            loopScrollView.ScrollToIndexCenter(index);
        }
    }
}