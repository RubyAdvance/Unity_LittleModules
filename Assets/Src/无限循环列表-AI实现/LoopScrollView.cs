using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 通用无限循环滚动列表管理器（支持垂直/横向滚动，兼容Unity 2021.3.32） 
/// 注意：切换横向或者纵向的时候需要指定不同的预设，且itemSize要对应的啥，目前都是100不需要另外调整！！！！！！
/// 挂载在ScrollRect对象上使用
/// </summary>
[RequireComponent(typeof(ScrollRect))]
public class LoopScrollView : MonoBehaviour
{
    // 自定义滚动方向枚举 
    public enum ScrollDirection
    {
        Vertical,   // 垂直滚动
        Horizontal  // 横向滚动
    }

    [Header("基础配置")]
    [Tooltip("滚动方向（Vertical=垂直，Horizontal=横向）")]
    public ScrollDirection scrollDirection = ScrollDirection.Vertical;
    public GameObject itemPrefab;
    [Tooltip("单个列表项的尺寸（垂直=高度，横向=宽度）")]
    public float itemSize = 100f;
    [Tooltip("列表项之间的间距")]
    public float itemSpacing = 10f;
    [Tooltip("缓冲Item数量（默认2个，避免滚动时出现空白）")]
    public int bufferCount = 2;

    // 私有变量
    private ScrollRect _scrollRect;          // 滚动组件
    private RectTransform _contentRect;      // Content的RectTransform
    private List<GameObject> _itemPool = new List<GameObject>(); // Item对象池
    private List<object> _dataList = new List<object>(); // 数据源
    private int _totalDataCount;             // 总数据量
    private int _visibleItemCount;           // 可视区域能显示的Item数量
    private float _contentTotalSize;         // Content总尺寸（垂直=高度，横向=宽度）
    private float _viewPortSize;             // 视口尺寸（垂直=高度，横向=宽度）

    // 滚动偏移阈值（避免频繁计算）
    private float _lastScrollPos = -1f;
    private const float _offsetThreshold = 0.001f;

    void Awake()
    {
        // 初始化组件引用
        _scrollRect = GetComponent<ScrollRect>();
        _contentRect = _scrollRect.content;
        if (_contentRect == null)
        {
            Debug.LogError("ScrollRect未配置Content！");
            return;
        }

        // 强制移除Content上的冲突组件
        RemoveConflictComponents();

        // 配置ScrollRect的滚动方向（核心修正：用自定义枚举控制布尔值）
        _scrollRect.vertical = scrollDirection == ScrollDirection.Vertical;
        _scrollRect.horizontal = scrollDirection == ScrollDirection.Horizontal;

        // 强制设置Content锚点和Pivot（适配横竖方向）
        SetRectAnchorByDirection();

        // 获取视口尺寸（适配分辨率）
        RectTransform viewPortRect = _scrollRect.viewport as RectTransform;
        if (viewPortRect == null)
        {
            Debug.LogError("ScrollRect未配置Viewport！");
            return;
        }
        _viewPortSize = scrollDirection == ScrollDirection.Vertical 
            ? viewPortRect.rect.height 
            : viewPortRect.rect.width;

        // 监听滚动事件
        _scrollRect.onValueChanged.AddListener(OnScrollChanged);

        // 检查并配置预制体
        CheckAndConfigItemPrefab();
    }

    /// <summary>
    /// 移除Content上的冲突组件（ContentSizeFitter/LayoutGroup等）
    /// </summary>
    private void RemoveConflictComponents()
    {
        // 移除ContentSizeFitter
        ContentSizeFitter csf = _contentRect.GetComponent<ContentSizeFitter>();
        if (csf != null)
        {
            Debug.LogWarning("已自动移除Content上的ContentSizeFitter（与循环列表逻辑冲突）！");
            Destroy(csf);
        }

        // 移除布局组（Horizontal/VerticalLayoutGroup）
        LayoutGroup lg = _contentRect.GetComponent<LayoutGroup>();
        if (lg != null)
        {
            Debug.LogWarning("已自动移除Content上的LayoutGroup（与循环列表逻辑冲突）！");
            Destroy(lg);
        }
    }

    /// <summary>
    /// 根据滚动方向设置Content锚点
    /// </summary>
    private void SetRectAnchorByDirection()
    {
        if (scrollDirection == ScrollDirection.Vertical)
        {
            // 垂直：Top Stretch（Anchor Min/Max=(0,1)/(1,1)）
            _contentRect.anchorMin = new Vector2(0, 1);
            _contentRect.anchorMax = new Vector2(1, 1);
            _contentRect.pivot = new Vector2(0.5f, 1f); // 顶部中心
            _contentRect.anchoredPosition = Vector2.zero;
            _contentRect.sizeDelta = new Vector2(0, _contentRect.sizeDelta.y);
        }
        else
        {
            // 横向：Left Stretch（Anchor Min/Max=(0,0)/(0,1)）
            _contentRect.anchorMin = new Vector2(0, 0);
            _contentRect.anchorMax = new Vector2(0, 1);
            _contentRect.pivot = new Vector2(0, 0.5f); // 左侧中心
            _contentRect.anchoredPosition = Vector2.zero;
            _contentRect.sizeDelta = new Vector2(_contentRect.sizeDelta.x, 0);
        }
    }

    /// <summary>
    /// 检查并配置Item预制体
    /// </summary>
    private void CheckAndConfigItemPrefab()
    {
        if (itemPrefab == null) return;

        RectTransform itemRect = itemPrefab.GetComponent<RectTransform>();
        if (itemRect != null)
        {
            // 强制设置Item锚点和尺寸
            if (scrollDirection == ScrollDirection.Vertical)
            {
                itemRect.anchorMin = new Vector2(0, 1);
                itemRect.anchorMax = new Vector2(1, 1);
                itemRect.pivot = new Vector2(0.5f, 1f);
                itemRect.sizeDelta = new Vector2(0, itemSize); // 宽度拉伸，高度固定
            }
            else
            {
                itemRect.anchorMin = new Vector2(0, 0);
                itemRect.anchorMax = new Vector2(0, 1);
                itemRect.pivot = new Vector2(0, 0.5f);
                itemRect.sizeDelta = new Vector2(itemSize, 0); // 高度拉伸，宽度固定
            }
        }

        // 检查是否实现IListItem接口
        if (itemPrefab.GetComponent<IListItem>() == null)
        {
            Debug.LogError("Item预制体未实现IListItem接口！");
        }
    }

    /// <summary>
    /// 设置列表数据源（外部调用的核心API，横竖通用）
    /// </summary>
    /// <param name="dataList">任意类型的数据源列表</param>
    public void SetDataList(List<object> dataList)
    {
        if (dataList == null)
        {
            Debug.LogWarning("数据源为空！");
            return;
        }

        // 清空旧数据
        _dataList.Clear();
        _dataList.AddRange(dataList);
        _totalDataCount = _dataList.Count;

        // 计算Content总尺寸（垂直=高度，横向=宽度）
        _contentTotalSize = _totalDataCount * (itemSize + itemSpacing) - itemSpacing;
        if (scrollDirection == ScrollDirection.Vertical)
        {
            _contentRect.sizeDelta = new Vector2(0, _contentTotalSize);
        }
        else
        {
            _contentRect.sizeDelta = new Vector2(_contentTotalSize, 0);
        }

        // 计算可视区域Item数量（适配分辨率，保底3个）
        float visibleSize = _viewPortSize + (itemSize + itemSpacing) * bufferCount;
        _visibleItemCount = Mathf.CeilToInt(visibleSize / (itemSize + itemSpacing));
        _visibleItemCount = Mathf.Max(_visibleItemCount, 3);

        // 初始化Item对象池
        InitItemPool();

        // 初始刷新Item位置和数据
        RefreshAllVisibleItems();
    }

    /// <summary>
    /// 初始化Item对象池（创建最少需要的Item数量）
    /// </summary>
    private void InitItemPool()
    {
        // 销毁旧Item
        foreach (var item in _itemPool)
        {
            Destroy(item);
        }
        _itemPool.Clear();

        // 创建新Item
        for (int i = 0; i < _visibleItemCount; i++)
        {
            if (itemPrefab == null)
            {
                Debug.LogError("未配置Item预制体！");
                return;
            }

            GameObject item = Instantiate(itemPrefab, _contentRect);
            item.SetActive(true);
            RectTransform itemRect = item.GetComponent<RectTransform>();
            // 强制设置Item尺寸
            if (scrollDirection == ScrollDirection.Vertical)
            {
                itemRect.sizeDelta = new Vector2(0, itemSize);
            }
            else
            {
                itemRect.sizeDelta = new Vector2(itemSize, 0);
            }
            _itemPool.Add(item);
        }
    }

    /// <summary>
    /// 滚动事件回调（横竖通用）
    /// </summary>
    /// <param name="scrollPos">滚动归一化位置</param>
    private void OnScrollChanged(Vector2 scrollPos)
    {
        // 获取当前滚动轴的位置（垂直=Y，横向=X）
        float currentPos = scrollDirection == ScrollDirection.Vertical 
            ? scrollPos.y 
            : scrollPos.x;

        // 避免频繁计算
        if (Mathf.Abs(currentPos - _lastScrollPos) < _offsetThreshold) return;
        _lastScrollPos = currentPos;

        RefreshAllVisibleItems();
    }

    /// <summary>
    /// 刷新所有可视区域的Item（核心复用逻辑，横竖通用）
    /// </summary>
    private void RefreshAllVisibleItems()
    {
        if (_totalDataCount == 0 || _itemPool.Count == 0) return;

        // 计算Content的偏移量（垂直=Y轴偏移，横向=X轴偏移）
        float contentOffset = 0;
        if (scrollDirection == ScrollDirection.Vertical)
        {
            contentOffset = Mathf.Abs(_contentRect.anchoredPosition.y); // 垂直：Top锚点下的向下偏移
        }
        else
        {
            contentOffset = Mathf.Abs(_contentRect.anchoredPosition.x); // 横向：Left锚点下的向右偏移
        }

        // 计算第一个可视Item的索引（容错：最小为0）
        int firstVisibleIndex = Mathf.Max(0, Mathf.FloorToInt(contentOffset / (itemSize + itemSpacing)) - bufferCount);

        // 遍历Item池，更新位置和数据
        for (int i = 0; i < _itemPool.Count; i++)
        {
            int targetIndex = firstVisibleIndex + i;
            // 超出数据范围则隐藏
            if (targetIndex < 0 || targetIndex >= _totalDataCount)
            {
                _itemPool[i].SetActive(false);
                continue;
            }
            _itemPool[i].SetActive(true);

            // 计算Item的目标位置（横竖区分）
            RectTransform itemRect = _itemPool[i].GetComponent<RectTransform>();
            float itemOffset = targetIndex * (itemSize + itemSpacing);
            if (scrollDirection == ScrollDirection.Vertical)
            {
                // 垂直：Y轴为负偏移（Top锚点）
                itemRect.anchoredPosition = new Vector2(0, -itemOffset);
            }
            else
            {
                // 横向：X轴为正偏移（Left锚点）
                itemRect.anchoredPosition = new Vector2(itemOffset, 0);
            }

            // 绑定数据
            IListItem listItem = _itemPool[i].GetComponent<IListItem>();
            listItem?.SetData(_dataList[targetIndex], targetIndex);
        }
    }

    /// <summary>
    /// 滚动到指定索引的Item（横竖通用）
    /// </summary>
    /// <param name="targetIndex">目标索引</param>
    public void ScrollToIndex(int targetIndex)
    {
        if (targetIndex < 0 || targetIndex >= _totalDataCount)
        {
            Debug.LogWarning("目标索引超出范围！");
            return;
        }

        // 计算目标偏移
        float targetOffset = targetIndex * (itemSize + itemSpacing);
        if (scrollDirection == ScrollDirection.Vertical)
        {
            // 垂直：Content的Y轴偏移为负
            _contentRect.anchoredPosition = new Vector2(_contentRect.anchoredPosition.x, -targetOffset);
        }
        else
        {
            // 横向：Content的X轴偏移为正
            _contentRect.anchoredPosition = new Vector2(targetOffset, _contentRect.anchoredPosition.y);
        }
        // 强制刷新
        RefreshAllVisibleItems();
    }

    /// <summary>
    /// 清空列表数据
    /// </summary>
    public void ClearData()
    {
        _dataList.Clear();
        _totalDataCount = 0;
        if (scrollDirection == ScrollDirection.Vertical)
        {
            _contentRect.sizeDelta = new Vector2(0, 0);
        }
        else
        {
            _contentRect.sizeDelta = new Vector2(0, 0);
        }
        foreach (var item in _itemPool)
        {
            item.SetActive(false);
        }
    }
}

/// <summary>
/// 列表项通用接口（所有循环列表的Item必须实现此接口）
/// </summary>
public interface IListItem
{
    /// <summary>
    /// 绑定数据到列表项
    /// </summary>
    /// <param name="data">任意类型的数据源（任务、商品等）</param>
    /// <param name="index">数据在列表中的索引</param>
    void SetData(object data, int index);
}