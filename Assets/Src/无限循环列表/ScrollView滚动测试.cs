using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ScrollView滚动测试 : MonoBehaviour
{
    [Range(0, 1)]//0表示底部，1表示顶部
    public float testValue = 0.2f;
    public ScrollRect testScrollRect;

    // 基础滑动速度（可在Inspector面板调整）
    [Header("缓动滑动参数")]
    [Range(0.3f, 1f)]
    public float scrollSpeed = 0.3f;

    // 后1/3阶段的减速系数（值越小减速越明显，建议0.1~0.5）
    [Range(0.1f, 0.8f)]
    public float slowDownRatio = 0.3f;

    // 用于标记是否正在滑动，避免重复触发
    private bool isScrolling = false;

    void Start()
    {
        if (testScrollRect == null)
        {
            Debug.LogWarning("请为testScrollRect赋值！");
        }
        //调用滚动
        SmoothScrollToTarget(testValue, 1);
    }

    void Update() { }

    [ContextMenu("测试（瞬间定位）")]
    public void TestFun()
    {
        testScrollRect.verticalNormalizedPosition = testValue;
    }

    [ContextMenu("测试缓动滑动（先快后慢）")]
    public void TestSmoothScroll()
    {
        // 示例：从顶部（startValue=1）滑动到testValue设置的目标位置
        SmoothScrollToTarget(testValue, 1);
    }

    /// <summary>
    /// 先快后慢的缓动滑动方法
    /// </summary>
    /// <param name="targetValue">目标位置（0=底部，1=顶部）</param>
    /// <param name="startValue">初始位置（仅支持0或1）</param>
    public void SmoothScrollToTarget(float targetValue, float startValue)
    {
        if (testScrollRect == null)
        {
            Debug.LogError("testScrollRect未赋值，无法执行滑动！");
            return;
        }

        // 修正初始位置和目标位置（限制在0-1之间）
        float correctedStartValue = startValue == 0 ? 0 : 1;
        float correctedTargetValue = Mathf.Clamp01(targetValue);

        // 避免重复触发滑动
        if (isScrolling)
        {
            StopCoroutine("DoSmoothScroll_EaseOut");
        }

        // 启动先快后慢的滑动协程
        StartCoroutine(DoSmoothScroll_EaseOut(correctedStartValue, correctedTargetValue));
    }

    /// <summary>
    /// 先快后慢的核心协程（前2/3快，后1/3减速）
    /// </summary>
    private IEnumerator DoSmoothScroll_EaseOut(float startValue, float targetValue)
    {
        isScrolling = true;
        float currentPos = startValue;
        testScrollRect.verticalNormalizedPosition = currentPos;

        // 计算总滑动距离
        float totalDistance = Mathf.Abs(targetValue - startValue);
        // 定义减速阈值（滑动剩余距离小于总距离的1/3时开始减速）
        float slowDownThreshold = totalDistance / 3f;

        while (Mathf.Abs(currentPos - targetValue) > 0.001f)
        {
            // 计算当前剩余滑动距离
            float remainingDistance = Mathf.Abs(targetValue - currentPos);
            // 动态调整速度：剩余距离>1/3总距离时用基础速度，否则减速
            float currentSpeed = remainingDistance > slowDownThreshold
                ? scrollSpeed * 2f  // 前2/3阶段：基础速度×2（更快）
                : scrollSpeed * slowDownRatio; // 后1/3阶段：基础速度×减速系数（变慢）

            // 向目标位置移动（每一帧的移动距离由动态速度决定）
            currentPos = Mathf.MoveTowards(currentPos, targetValue, currentSpeed * Time.deltaTime);
            testScrollRect.verticalNormalizedPosition = currentPos;

            yield return null;
        }

        // 滑动结束后精准定位
        testScrollRect.verticalNormalizedPosition = targetValue;
        isScrolling = false;
    }
}