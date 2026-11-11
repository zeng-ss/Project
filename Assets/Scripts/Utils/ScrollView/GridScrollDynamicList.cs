
//------------------------------------------------------------
// File : GridScrollDynamicList.cs
// Email: mailto:yang.li@kingboat.io
// Desc : 
//------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine.Events;

public interface IListDataItem
{
    public virtual int GetPrefabIndex() => 0; // 获取预制体索引

    public int IndexInGrid { get; set; }

    public void AppearDisplay(GridScrollChildBase cellComponent)
    {
        cellComponent.AppearDisplay(this);
    }

    public void RefreshWhenDrag(GridScrollChildBase cellComponent)
    {
        cellComponent.RefreshWhenDrag();
    }

    public void UpdateDisplay(GridScrollChildBase cellComponent)
    {
        cellComponent.OnSpawnRefresh(this);
    }

    public void DespawnDisplay(GridScrollChildBase cellComponent)
    {
        cellComponent.OnDeSpawnRefresh();
    }

    public virtual float GetSpecialHeight()
    {
        return 0f;
    }

    public virtual float GetSpecialWidth()
    {
        return 0f;
    }
}

[System.Serializable]
public class GridScrollConfig
{
    [Header("是否是水平滚动")] public bool isHorizontal = false;
    [Header("上下或者左右边距配置")] public Vector2 Margin = new Vector2(10f, 10f);
    [Header("间距配置")] public Vector2 cellSpacing = new Vector2(10f, 10f);
    [Header("元素配置")] public Vector2 cellSize = new Vector2(100f, 180f);
    [Header("最小列或者行数限制")] public int minColumnOrRowCount = 1;
    public UnityEvent<Vector2> onContentSizeChange;
    public UnityEvent<Vector2> onContentDrag;
    public UnityEvent<float> onScrollProgress;
}

[RequireComponent(typeof(ScrollRect))]
public class GridScrollDynamicList : MonoBehaviour
{
    [SerializeField] private AttachParentPrefabPool pool;
    [SerializeField] private GridScrollConfig config = new GridScrollConfig();
    public GridScrollConfig Config => config;
    public RectTransform Content => content;
    private ScrollRect scrollRect;
    private RectTransform content;
    private List<IListDataItem> dataSource = new List<IListDataItem>();
    public IReadOnlyList<IListDataItem> DataSource => dataSource;
    public int DataCount => dataSource?.Count ?? 0;

    public IListDataItem GetData(int index)
    {
        return dataSource[index];
    }

    // 当前显示的卡牌
    private Dictionary<int, GridScrollChildBase> activeCells = new Dictionary<int, GridScrollChildBase>();
    public IReadOnlyDictionary<int, GridScrollChildBase> ActiveCells => activeCells;

    // 卡牌尺寸
    private Vector2 cellSize;
    private bool isInitialized = false;

    // 可视区域相关
    private int firstVisibleIndex = -1;
    private int lastVisibleIndex = -1;

    public Vector2 GetCellAnchoredPosition(int index)
    {
        return GetCellPosition(index);
    }

    private void Awake()
    {
        Initialize();
    }

    public void Initialize(List<IListDataItem> newDataSource)
    {
        Initialize();

        if (content != null)
        {
            if (config.isHorizontal)
            {
                content.anchorMin = new Vector2(0f, 0f);
                content.anchorMax = new Vector2(0f, 1f);
                content.pivot = new Vector2(0f, 1f);
            }
            else
            {
                content.anchorMin = new Vector2(0f, 1f);
                content.anchorMax = new Vector2(1f, 1f);
                content.pivot = new Vector2(0f, 1f);
            }
            content.offsetMin = Vector2.zero;
            content.offsetMax = Vector2.zero;
        }

        SetDataSource(newDataSource);
    }

    private void Initialize()
    {
        if (isInitialized) return;

        isInitialized = true;
        scrollRect = GetComponent<ScrollRect>();
        content = scrollRect.content;

        scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
        InitializeCellSize();
    }

    private void InitializeCellSize()
    {
        cellSize = config.cellSize;
    }

    public int GetColumnOrRowCount(RectTransform customContent)
    {
        if (config.isHorizontal)
        {
            customContent.anchorMin = new Vector2(0f, 0f);
            customContent.anchorMax = new Vector2(0f, 1f);
            customContent.pivot = new Vector2(0f, 0.5f);
        }
        else
        {
            customContent.anchorMin = new Vector2(0f, 1f);
            customContent.anchorMax = new Vector2(1f, 1f);
            customContent.pivot = new Vector2(0.5f, 1f);
        }

        if (config.isHorizontal)
        {
            float contentHeight = customContent.rect.height;
            float cellHeight = cellSize.y;
            float spacing = config.cellSpacing.y;
            int rowCount = Mathf.FloorToInt((contentHeight + spacing) / (cellHeight + spacing));
            // 确保至少有最小行数
            return Mathf.Max(rowCount, config.minColumnOrRowCount);
        }
        else
        {
            float contentWidth = customContent.rect.width;
            float cellWidth = cellSize.x;
            float spacing = config.cellSpacing.x;

            int columnCount = Mathf.FloorToInt((contentWidth + spacing) / (cellWidth + spacing));
            // 确保至少有最小列数
            return Mathf.Max(columnCount, config.minColumnOrRowCount);
        }
    }

    private int CalculateColumnOrRowCount()
    {
        if (!isInitialized || content == null)
            return config.minColumnOrRowCount;
        if (config.isHorizontal)
        {
            float contentHeight = content.rect.height;
            float cellHeight = cellSize.y;
            float spacing = config.cellSpacing.y;
            int rowCount = Mathf.FloorToInt((contentHeight + spacing) / (cellHeight + spacing));
            // 确保至少有最小行数
            return Mathf.Max(rowCount, config.minColumnOrRowCount);
        }
        else
        {
            float contentWidth = content.rect.width;
            float cellWidth = cellSize.x;
            float spacing = config.cellSpacing.x;

            int columnCount = Mathf.FloorToInt((contentWidth + spacing) / (cellWidth + spacing));
            // 确保至少有最小列数
            return Mathf.Max(columnCount, config.minColumnOrRowCount);
        }
    }

    /// <summary>
    /// 设置数据源
    /// </summary>
    /// <param name="newDataSource">新的数据源</param>
    private void SetDataSource(List<IListDataItem> newDataSource)
    {
        ClearAllCells();

        dataSource = new List<IListDataItem>(newDataSource);

        if (isInitialized)
        {
            for (int i = 0; i < dataSource.Count; i++)
            {
                dataSource[i].IndexInGrid = i;
            }

            UpdateContentSize();
            RefreshVisibleCells(true);
        }
    }

    /// <summary>
    /// 更新Content尺寸
    /// </summary>
    private void UpdateContentSize()
    {
        if (!isInitialized || dataSource.Count == 0)
        {
            if (config.isHorizontal)
            {
                content.sizeDelta = new Vector2(0, content.sizeDelta.y);
            }
            else
            {
                content.sizeDelta = new Vector2(content.sizeDelta.x, 0);
            }

            return;
        }


        if (config.isHorizontal)
        {
            // 计算列数和行数
            int rowCount = CalculateColumnOrRowCount();
            int columnCount = Mathf.CeilToInt((float)dataSource.Count / rowCount);

            // 计算总高度
            float totalWidth = config.Margin.y + config.Margin.x;
            int col = 0;
            for (int i = 0; i < dataSource.Count; i++)
            {
                if (i % rowCount == 0)
                {
                    col++;
                    float specialWidth = dataSource[i].GetSpecialWidth();
                    if (specialWidth > 0f)
                    {
                        totalWidth += specialWidth;
                    }
                    else
                    {
                        totalWidth += cellSize.x;
                    }

                    if (columnCount != col)
                    {
                        totalWidth += config.cellSpacing.x;
                    }
                }
            }


            content.sizeDelta = new Vector2(totalWidth, content.sizeDelta.y);
        }
        else
        {
            // 计算列数和行数
            int columnCount = CalculateColumnOrRowCount();
            int rowCount = Mathf.CeilToInt((float)dataSource.Count / columnCount);
            int row = 0;
            // 计算总高度
            float totalHeight = config.Margin.x + config.Margin.y;
            for (int i = 0; i < dataSource.Count; i++)
            {
                if (i % columnCount == 0)
                {
                    row++;
                    float specialHeight = dataSource[i].GetSpecialHeight();
                    if (specialHeight > 0f)
                    {
                        totalHeight += specialHeight;
                    }
                    else
                    {
                        totalHeight += cellSize.y;
                    }

                    if (rowCount != row)
                    {
                        totalHeight += config.cellSpacing.y;
                    }
                }
            }

            content.sizeDelta = new Vector2(content.sizeDelta.x, totalHeight);
        }

        config.onContentSizeChange?.Invoke(content.sizeDelta);
    }

    private void OnScrollValueChanged(Vector2 value)
    {
        RefreshVisibleCells();
        config.onContentDrag?.Invoke(content.anchoredPosition);
        config.onScrollProgress?.Invoke(config.isHorizontal ? scrollRect.horizontalNormalizedPosition : scrollRect.verticalNormalizedPosition);
    }

    /// <summary>
    /// 刷新可见卡牌
    /// </summary>
    private void RefreshVisibleCells(bool isInit = false)
    {
        if (!isInitialized || dataSource.Count == 0) return;

        // 计算可视区域
        Rect viewportRect = GetViewportRect();


        // 计算可见的数据索引范围
        int newFirstVisible, newLastVisible;
        CalculateVisibleRange(viewportRect, out newFirstVisible, out newLastVisible);

        // 回收不在可见范围内的卡牌
        RecycleInvisibleCells(newFirstVisible, newLastVisible);

        foreach (var items in activeCells)
        {
            UpdateCellWhenDrag(items.Value, items.Key);
        }

        // 生成新的可见卡牌
        SpawnVisibleCells(newFirstVisible, newLastVisible, isInit);

        firstVisibleIndex = newFirstVisible;
        lastVisibleIndex = newLastVisible;
    }

    /// <summary>
    /// 获取视口矩形（在Content坐标系中）
    /// </summary>
    /// <returns>视口矩形</returns>
    private Rect GetViewportRect()
    {
        RectTransform viewportRect = scrollRect.viewport;
        Vector3[] viewportCorners = new Vector3[4];
        viewportRect.GetWorldCorners(viewportCorners);

        // 转换到Content本地坐标系
        Vector2 viewportMin = content.InverseTransformPoint(viewportCorners[0]);
        Vector2 viewportMax = content.InverseTransformPoint(viewportCorners[2]);

        return new Rect(viewportMin.x, viewportMin.y,
            viewportMax.x - viewportMin.x, viewportMax.y - viewportMin.y);
    }

    /// <summary>
    /// 计算可见范围
    /// </summary>
    /// <param name="viewportRect">视口矩形</param>
    /// <param name="firstVisible">第一个可见索引</param>
    /// <param name="lastVisible">最后一个可见索引</param>
    private void CalculateVisibleRange(Rect viewportRect, out int firstVisible, out int lastVisible)
    {
        firstVisible = -1;
        lastVisible = -1;

        // 添加更大的缓冲区，确保边界单元格能被正确显示和预加载
        float buffer = Mathf.Max(cellSize.x, cellSize.y) * 0.5f;
        Rect expandedViewport = new Rect(
            viewportRect.xMin,
            viewportRect.yMin,
            viewportRect.width,
            viewportRect.height
        );

        for (int i = 0; i < dataSource.Count; i++)
        {
            Rect cellRect = GetCellRect(i);

            if (cellRect.Overlaps(expandedViewport))
            {
                if (firstVisible == -1) firstVisible = i;
                lastVisible = i;
            }
        }

        // 确保有效范围
        firstVisible = Mathf.Max(0, firstVisible);
        lastVisible = Mathf.Min(dataSource.Count - 1, lastVisible);
    }

    /// <summary>
    /// 获取指定索引卡牌的矩形区域
    /// </summary>
    /// <param name="index">数据索引</param>
    /// <returns>卡牌矩形</returns>
    private Rect GetCellRect(int index)
    {
        Vector2 position = GetCellPosition(index);
        float x = cellSize.x;
        float y = cellSize.y;
        if (config.isHorizontal)
        {
            x = dataSource[index].GetSpecialWidth();
            if (x <= 0)
            {
                x = cellSize.x;
            }
        }
        else
        {
            y = dataSource[index].GetSpecialHeight();
            if (y <= 0)
            {
                y = cellSize.y;
            }
        }

        return new Rect(position.x - x * 0.5f, position.y - y * 0.5f, x, y);
    }

    public bool IsDataIndexVisible(int index)
    {
        if (!isInitialized || dataSource == null || index < 0 || index >= dataSource.Count)
            return false;
        Rect viewportRect = GetViewportRect();
        Rect cellRect = GetCellRect(index);
        return cellRect.Overlaps(viewportRect);
    }

    public Vector2 GetPosition(int index)
    {
        return GetCellPosition(index);
    }

    public Vector2 GetContentSize()
    {
        return content.sizeDelta;
    }

    /// <summary>
    /// 获取指定索引卡牌的位置
    /// </summary>
    /// <param name="index">数据索引</param>
    /// <returns>卡牌位置</returns>
    private Vector2 GetCellPosition(int index)
    {
        int columnOrRowCount = CalculateColumnOrRowCount();
        int other = Mathf.CeilToInt((float)dataSource.Count / columnOrRowCount);
        if (config.isHorizontal)
        {
            int col = 0;
            int colDesign = other;
            int colCurrent = index / columnOrRowCount;
            int row = index % columnOrRowCount;

            // 计算居中位置
            float contentHeight = content.rect.height;
            float totalCellsHeight = columnOrRowCount * cellSize.y + (columnOrRowCount - 1) * config.cellSpacing.y;
            float startY = (contentHeight - totalCellsHeight) * 0.5f + cellSize.y * 0.5f;
            float selfWidth = dataSource[index].GetSpecialWidth();
            if (selfWidth <= 0)
            {
                selfWidth = cellSize.x;
            }

            float x = config.Margin.x + selfWidth * 0.5f;
            for (int i = 0; i < index; i++)
            {
                if (i % columnOrRowCount == 0)
                {
                    if (col == colCurrent)
                    {
                        break;
                    }

                    col++;


                    float otherWidth = dataSource[i].GetSpecialWidth();
                    if (otherWidth <= 0)
                    {
                        otherWidth = cellSize.x;
                    }

                    if (col != colDesign)
                    {
                        x += (otherWidth + config.cellSpacing.x);
                    }
                    else
                    {
                        x += otherWidth;
                    }
                }
            }


            float y = -(startY + row * (cellSize.y + config.cellSpacing.y));

            return new Vector2(x, y);
        }
        else
        {
            int row = 0;
            int rowDesign = other;
            int rowCurrent = index / columnOrRowCount;
            int col = index % columnOrRowCount;

            // 计算居中位置
            float contentWidth = content.rect.width;

            float totalCellsWidth = columnOrRowCount * cellSize.x + (columnOrRowCount - 1) * config.cellSpacing.x;

            float startX = (contentWidth - totalCellsWidth) * 0.5f + cellSize.x * 0.5f;

            float x = startX + col * (cellSize.x + config.cellSpacing.x);

            float selHeight = dataSource[index].GetSpecialHeight();
            if (selHeight <= 0)
            {
                selHeight = cellSize.y;
            }

            float temp = 0f;

            for (int i = 0; i < index; i++)
            {
                if (i % columnOrRowCount == 0)
                {
                    if (row == rowCurrent)
                    {
                        break;
                    }

                    row++;


                    float otherHeight = dataSource[i].GetSpecialHeight();
                    if (otherHeight <= 0)
                    {
                        otherHeight = cellSize.y;
                    }

                    if (row != rowDesign)
                    {
                        temp += (otherHeight + config.cellSpacing.y);
                    }
                    else
                    {
                        temp += otherHeight;
                    }
                }
            }

            float y = -(config.Margin.x + selHeight * 0.5f + temp);

            return new Vector2(x, y);
        }
    }

    /// <summary>
    /// 回收不可见的卡牌
    /// </summary>
    /// <param name="newFirstVisible">新的第一个可见索引</param>
    /// <param name="newLastVisible">新的最后一个可见索引</param>
    private void RecycleInvisibleCells(int newFirstVisible, int newLastVisible)
    {
        List<int> toRecycle = new List<int>();

        foreach (var kvp in activeCells)
        {
            int index = kvp.Key;
            if (index < newFirstVisible || index > newLastVisible)
            {
                toRecycle.Add(index);
            }
        }

        foreach (int index in toRecycle)
        {
            RecycleCell(index);
        }
    }

    /// <summary>
    /// 生成可见的卡牌
    /// </summary>
    /// <param name="firstVisible">第一个可见索引</param>
    /// <param name="lastVisible">最后一个可见索引</param>
    private void SpawnVisibleCells(int firstVisible, int lastVisible, bool isInit = false)
    {
        for (int i = firstVisible; i <= lastVisible; i++)
        {
            if (!activeCells.ContainsKey(i))
            {
                SpawnCell(i, isInit);
            }
        }
    }

    /// <summary>
    /// 回收指定索引的卡牌
    /// </summary>
    /// <param name="index">数据索引</param>
    private void UpdateCellWhenDrag(GridScrollChildBase kbPoolObjectBase, int index)
    {
        if (index >= 0 && index < dataSource.Count)
        {
            dataSource[index].RefreshWhenDrag(kbPoolObjectBase);
        }
    }

    private void SpawnCell(int index, bool isInit = false)
    {
        if (index < 0 || index >= dataSource.Count) return;

        IListDataItem dataItem = dataSource[index];
        int prefabIndex = dataItem.GetPrefabIndex();
        GridScrollChildBase gridScrollChildBase = pool.Spawn<GridScrollChildBase>(prefabIndex, content);
        if (gridScrollChildBase != null)
        {
            // 设置位置和锚点
            RectTransform cellTransform = gridScrollChildBase.GetComponent<RectTransform>();
            if (cellTransform != null)
            {
                cellTransform.anchorMin = new Vector2(0f, 1f); //左上角 
                cellTransform.anchorMax = new Vector2(0f, 1f);
                cellTransform.pivot = new Vector2(0.5f, 0.5f);
                cellTransform.transform.localScale = Vector3.one;

                Vector2 position = GetCellPosition(index);
                cellTransform.anchoredPosition = position;
            }

            // 更新数据显示
            dataItem.UpdateDisplay(gridScrollChildBase);
            if (isInit)
            {
                dataItem.AppearDisplay(gridScrollChildBase);
            }
        }
        activeCells[index] = gridScrollChildBase;
    }

    /// <summary>
    /// 回收指定索引的卡牌
    /// </summary>
    /// <param name="index">数据索引</param>
    private void RecycleCell(int index)
    {
        if (activeCells.TryGetValue(index, out GridScrollChildBase cellComponent))
        {
            cellComponent.OnDeSpawnRefresh();
            pool.DeSpawn(cellComponent);
            activeCells.Remove(index);
        }
    }

    /// <summary>
    /// 回收所有卡牌
    /// </summary>
    private void RecycleAllCell()
    {
        if (!isInitialized)
        {
            return;
        }

        foreach (var data in activeCells)
        {
            data.Value?.OnDeSpawnRefresh();
            pool.DeSpawn(data.Value);
        }

        activeCells.Clear();
    }

    /// <summary>
    /// 删除索引的卡牌和数据
    /// </summary>
    /// <param name="index">数据索引</param>
    private void DestoryCellAndData(int index)
    {
        IListDataItem dataItem = dataSource[index];
        int needChangeIndex = index + 1;
        List<(int, GridScrollChildBase)> needMoveCells = new List<(int, GridScrollChildBase)>();
        for (; needChangeIndex < dataSource.Count; needChangeIndex++)
        {
            if (activeCells.TryGetValue(needChangeIndex, out GridScrollChildBase data))
            {
                needMoveCells.Add((needChangeIndex, data));
            }
        }

        if (activeCells.TryGetValue(index, out GridScrollChildBase cellComponent))
        {
            dataItem.DespawnDisplay(cellComponent);
            pool.DeSpawn(cellComponent);
            activeCells.Remove(index);
        }
        foreach (var data in needMoveCells)
        {
            activeCells.Remove(data.Item1);
        }

        foreach (var data in needMoveCells)
        {
            activeCells[data.Item1 - 1] = data.Item2;
        }

        dataSource.RemoveAt(index);
    }

    /// <summary>
    /// 清理所有卡牌
    /// </summary>
    private void ClearAllCells()
    {
        RecycleAllCell();
    }

    /// <summary>
    /// 刷新指定索引的卡牌数据
    /// </summary>
    /// <param name="index">数据索引</param>
    public void RefreshCell(int index)
    {
        if (activeCells.TryGetValue(index, out GridScrollChildBase cellComponent))
        {
            if (index >= 0 && index < dataSource.Count)
            {
                dataSource[index].UpdateDisplay(cellComponent);
            }
        }
    }

    /// <summary>
    /// 刷新所有可见卡牌的数据
    /// </summary>
    public void RefreshAllVisibleCells()
    {
        foreach (var kvp in activeCells)
        {
            int index = kvp.Key;
            if (index >= 0 && index < dataSource.Count)
            {
                dataSource[index].UpdateDisplay(kvp.Value);
            }
        }
    }

    public int GetIndex(IListDataItem gridScrollChildBase)
    {
        for (int i = 0; i < dataSource.Count; i++)
        {
            if (gridScrollChildBase == dataSource[i])
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// 滚动到指定索引
    /// </summary>
    /// <param name="index">目标索引</param>
    /// <param name="immediately">是否立即滚动</param>
    public Sequence ScrollToIndex(int index, bool immediately = false, float lerpTime = 0.4f)
    {
        if (index < 0 || index >= dataSource.Count || !isInitialized) return null;

        Vector2 targetPosition = GetCellPosition(index);

        if (config.isHorizontal)
        {
            float contentWidth = content.rect.width;
            float viewportWidth = scrollRect.viewport.rect.width;
            float targetWidth = dataSource[index].GetSpecialWidth();
            if (targetWidth <= 0)
            {
                targetWidth = cellSize.x;
            }

            // 计算滚动位置（0-1之间）
            float normalizedX =
                Mathf.Clamp01(Mathf.Abs(targetPosition.x + targetWidth / 2f) / (contentWidth - viewportWidth));

            if (immediately)
            {
                scrollRect.horizontalNormalizedPosition = normalizedX;
                OnScrollValueChanged(Vector2.zero);
                return null;
            }
            else
            {
                // 这里可以添加平滑滚动的协程
                return DOTween.Sequence().Join(DOTween.To(() => scrollRect.horizontalNormalizedPosition,
                    (s) => { scrollRect.horizontalNormalizedPosition = s; }, normalizedX, lerpTime)).OnComplete(() =>
                {
                    scrollRect.horizontalNormalizedPosition = normalizedX;
                });
            }
        }
        else
        {
            float contentHeight = content.rect.height;
            float viewportHeight = scrollRect.viewport.rect.height;
            float targetHeight = dataSource[index].GetSpecialHeight();
            if (targetHeight <= 0)
            {
                targetHeight = cellSize.y;
            }

            // 计算滚动位置（0-1之间）
            float normalizedY =
                Mathf.Clamp01((-targetPosition.y - targetHeight / 2f) / (contentHeight - viewportHeight));

            if (immediately)
            {
                scrollRect.verticalNormalizedPosition = 1f - normalizedY;
                OnScrollValueChanged(Vector2.zero);
                return null;
            }
            else
            {
                // 这里可以添加平滑滚动的协程
                return DOTween.Sequence().Join(DOTween.To(() => scrollRect.verticalNormalizedPosition,
                        (s) => { scrollRect.verticalNormalizedPosition = s; }, 1f - normalizedY, lerpTime))
                    .OnComplete(() => { scrollRect.verticalNormalizedPosition = 1f - normalizedY; });
            }
        }
    }

    /// <summary>
    /// 添加单个数据项
    /// </summary>
    /// <param name="dataItem">要添加的数据项</param>
    /// <param name="refreshImmediately">是否立即刷新显示</param>
    public void AddDataItem(IListDataItem dataItem, bool refreshImmediately = true)
    {
        if (dataItem == null) return;

        dataSource.Add(dataItem);
        dataItem.IndexInGrid = dataSource.Count - 1;
        if (refreshImmediately && isInitialized)
        {
            UpdateContentSize();
            RefreshVisibleCells();
        }
    }

    /// <summary>
    /// 插入数据项到指定位置
    /// </summary>
    /// <param name="index">插入位置</param>
    /// <param name="dataItem">要插入的数据项</param>
    /// <param name="refreshImmediately">是否立即刷新显示</param>
    public void InsertDataItem(int index, IListDataItem dataItem, bool refreshImmediately = true)
    {
        if (dataItem == null || index < 0 || index > dataSource.Count) return;

        dataSource.Insert(index, dataItem);
        int id = 0;
        foreach (var data in dataSource)
        {
            data.IndexInGrid = id;
            id++;
        }

        if (refreshImmediately && isInitialized)
        {
            // 清理所有卡牌重新生成，因为索引发生了变化
            ClearAllCells();
            UpdateContentSize();
            RefreshVisibleCells();
        }
    }

    /// <summary>
    /// 移除指定索引的数据项
    /// </summary>
    /// <param name="index">要移除的索引</param>
    /// <param name="refreshImmediately">是否立即刷新显示</param>
    public bool RemoveDataItem(int index, bool refreshImmediately = true)
    {
        if (index < 0 || index >= dataSource.Count) return false;

        DestoryCellAndData(index);

        int id = 0;
        foreach (var data in dataSource)
        {
            data.IndexInGrid = id;
            id++;
        }

        // 清理所有卡牌重新生成，因为索引发生了变化
        if (refreshImmediately && isInitialized)
        {
            ClearAllCells();
            UpdateContentSize();
            RefreshVisibleCells();
        }

        return true;
    }

    /// <summary>
    /// 更新配置
    /// </summary>
    /// <param name="newConfig">新的配置</param>
    /// <param name="refreshImmediately">是否立即刷新显示</param>
    public void UpdateConfig(GridScrollConfig newConfig, bool refreshImmediately = true)
    {
        config = newConfig;
        cellSize = config.cellSize;
        if (!isInitialized)
        {
            scrollRect = GetComponent<ScrollRect>();
            content = scrollRect.content;
        }

        if (refreshImmediately)
        {
            ClearAllCells();
            UpdateContentSize();
            RefreshVisibleCells();
        }
    }

    /// <summary>
    /// 滚动到顶部
    /// </summary>
    /// <param name="immediately">是否立即滚动</param>
    public void ScrollToTop(bool immediately = false)
    {
        if (config.isHorizontal)
        {
            if (immediately)
            {
                scrollRect.horizontalNormalizedPosition = 0f;
            }
            else
            {
                scrollRect.horizontalNormalizedPosition = 0f;
            }
        }
        else
        {
            if (immediately)
            {
                scrollRect.verticalNormalizedPosition = 1f;
            }
            else
            {
                scrollRect.verticalNormalizedPosition = 1f;
            }
        }
    }

    /// <summary>
    /// 滚动到底部
    /// </summary>
    /// <param name="immediately">是否立即滚动</param>
    public void ScrollToBottom(bool immediately = false)
    {
        if (config.isHorizontal)
        {
            if (immediately)
            {
                scrollRect.horizontalNormalizedPosition = 1f;
            }
            else
            {
                scrollRect.horizontalNormalizedPosition = 1f;
            }
        }
        else
        {
            if (immediately)
            {
                scrollRect.verticalNormalizedPosition = 0f;
            }
            else
            {
                scrollRect.verticalNormalizedPosition = 0f;
            }
        }
    }

    /// <summary>
    /// 将指定Index滑动到视口中心
    /// </summary>
    /// <param name="index"></param>
    /// <param name="immediately"></param>
    /// <param name="lerpTime"></param>
    /// <returns></returns>
    public Sequence ScrollTo(int index, bool immediately = false, float lerpTime = 0.4f)
    {
        var cell = dataSource[index];
        if (cell == null) return null;

        Vector2 targetInContentPos = GetCellPosition(index);

        if (!config.isHorizontal)
        {
            float contentHeight = content.rect.height;
            float viewportHeight = scrollRect.viewport.rect.height;
            float targetHeight = cellSize.y;
            float targetTop = targetInContentPos.y;
            float targetCenterY = targetTop - (targetHeight * 0.5f);
            float viewportCenterY = -viewportHeight * 0.5f;
            float scrollDistance = targetCenterY - viewportCenterY;
            float scrollableHeight = Mathf.Max(1f, contentHeight - viewportHeight);
            float normalizedPosition = Mathf.Clamp01((scrollableHeight + scrollDistance) / scrollableHeight);

            if (immediately)
            {
                scrollRect.verticalNormalizedPosition = normalizedPosition;
                return null;
            }

            return DOTween.Sequence().Join(DOTween.To(
                () => scrollRect.verticalNormalizedPosition,
                value => scrollRect.verticalNormalizedPosition = value,
                normalizedPosition,
                lerpTime
            ).SetEase(Ease.OutCubic));
        }
        else
        {
            float contentWidth = content.rect.width;
            float viewportWidth = scrollRect.viewport.rect.width;
            float targetWidth = cellSize.x;
            float targetLeft = targetInContentPos.x;
            float targetCenterX = targetLeft + (targetWidth * 0.5f);
            float viewportCenterX = viewportWidth * 0.5f;
            float scrollDistance = targetCenterX - viewportCenterX;
            float scrollableWidth = Mathf.Max(1f, contentWidth - viewportWidth);
            float normalizedPosition = Mathf.Clamp01((scrollDistance) / scrollableWidth);

            if (immediately)
            {
                scrollRect.horizontalNormalizedPosition = normalizedPosition;
                return null;
            }

            return DOTween.Sequence().Join(DOTween.To(
                () => scrollRect.horizontalNormalizedPosition,
                value => scrollRect.horizontalNormalizedPosition = value,
                normalizedPosition,
                lerpTime
            ).SetEase(Ease.OutCubic));
        }

    }

    /// <summary>
    /// 节点是否在显示
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public bool IsNodeActive(int index)
    {
        return index >= firstVisibleIndex && index <= lastVisibleIndex;
    }

    public GridScrollChildBase GetFirstActiveCell()
    {
        if (activeCells.Count > 0)
        {
            return activeCells.Values.FirstOrDefault();
        }

        return null;
    }

    /// <summary>
    /// 清理所有数据
    /// </summary>
    public void Clear()
    {
        ClearAllCells();
        dataSource.Clear();
    }
}