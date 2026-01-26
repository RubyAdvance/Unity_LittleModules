using UnityEngine;
using UnityEngine.UI;

// 示例：任务列表项（根据你的实际需求修改）
public class TaskItem : MonoBehaviour, IListItem
{
    public Text taskNameText;
    public Text taskIndexText;

    /// <summary>
    /// 绑定任务数据（数据类型可自定义，比如TaskData类）
    /// </summary>
    public void SetData(object data, int index)
    {
        // 转换数据类型（根据你的实际数据源调整）
        string taskData = data as string;
        if (taskData == null) return;

        // 绑定数据到UI
        taskIndexText.text = $"第{index+1}个任务";
        taskNameText.text = taskData;
    }
}