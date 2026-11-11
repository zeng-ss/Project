using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CasualGame.EditorTools
{
    /// <summary>编辑器内存放的容器数据，对应关卡 JSON 里 holderDataList 的单条</summary>
    [Serializable]
    public class PlacedHolderData
    {
        public int HolderType; // HolderPrefabType
        public float X; // 中心点网格坐标（不含 OrigionPos）
        public float Y;
        public int Width;
        public int Height;
        public float DirectionValue; // 旋转角度 0/90/180/270
        public List<int> CarColors = new();
    }

    /// <summary>
    /// 路径点：格子的交点（Gx/Gy 网格整数坐标），并支持高度 Y 与精细偏移 OffsetX/OffsetZ，
    /// 用于搭建有上下层次的道路，避免同一平面上的路径重叠。
    /// </summary>
    [Serializable]
    public class PathPointData
    {
        public int Gx;
        public int Gy;
        public float Y; // 高度（世界 Y）
        public float OffsetX; // 网格交点基础上的精细 X 偏移
        public float OffsetZ; // 网格交点基础上的精细 Z 偏移

        public float WorldX => Gx + 0.5f + OffsetX;
        public float WorldZ => Gy + 0.5f + OffsetZ;
        public Vector3 WorldPos => new Vector3(WorldX, Y, WorldZ);
    }

    /// <summary>
    /// 一条道路路段：起点（传送门入口）+ 中间路径点 + 终点（传送门出口）。
    /// 保存时按 [Start, ...PathPoints, End] 展平成连续 pathIndex 的道路格，首尾格写 isStart/isEnd 标记。
    /// </summary>
    [Serializable]
    public class RoadSegmentData
    {
        public PathPointData Start; // 起点（可为 null，未设置）
        public PathPointData End; // 终点（可为 null，未设置）
        public List<PathPointData> PathPoints = new();
    }

    /// <summary>撤销栈条目</summary>
    [Serializable]
    public class UndoEntry
    {
        public enum Op
        {
            None,
            AddHolder, // 新增容器
            RemoveHolder, // 删除容器（撤销时恢复被删容器）
            AddPathPoint, // 新增路径点
            AddSegment, // 新增路段
            SetStart, // 设置/移除起点
            SetEnd, // 设置/移除终点
            Mirror, // 对称生成：撤销时恢复操作前快照
            Merge, // 合并路段：撤销时恢复操作前路段快照
            MoveSegment, // 整体平移路段：撤销时恢复操作前路段快照
            RescaleHeight, // 重排层间距：撤销时恢复操作前路段快照与旧间距
            RoundSpiral, // 螺旋圆化：撤销时恢复操作前路段快照
            RemoveSymmetrySide, // 删除对称轴对侧点：撤销时恢复操作前路段快照
        }

        public Op OpType;
        public int Index; // 容器/路段在列表中的下标
        public PathPointData Point; // SetStart/SetEnd：被设置/移除的点（撤销时移除/恢复用）
        public PlacedHolderData RemovedHolder; // RemoveHolder：被删除的容器（撤销时按 Index 恢复）
        public List<RoadSegmentData> SegSnapshot; // Mirror/Merge/MoveSegment/RescaleHeight/RoundSpiral/RemoveSymmetrySide：操作前路段快照
        public List<PlacedHolderData> HolderSnapshot; // Mirror：对称生成前容器快照
        public int CurrentSegmentSnapshot; // Mirror/Merge/MoveSegment/RescaleHeight/RoundSpiral/RemoveSymmetrySide：操作前当前路段下标
        public float SpacingSnapshot; // RescaleHeight：操作前层间距
    }

    /// <summary>关卡编辑器的持久化数据</summary>
    public class CoffeeSortEditData : ScriptableObject
    {
        public const string AssetPath = "Assets/Scripts/Editor/CoffeeSort/CoffeeSortEditData.asset";

        public bool IsActive;
        public string EditorSceneName;

        public int GridSizeX = 19;
        public int GridSizeY = 19;
        public int Capacity = 16;
        public bool IsCloseConveyor = true;
        public bool IsChangeCapacity = true;
        public float CapTextX = 9.5f;
        public float CapTextY = 22f;

        // 对称生成配置
        public bool SymmetryAxisIsX = true; // true=关于 x=? 左右对称；false=关于 y=? 上下对称
        public float SymmetryValue = 0f; // 对称轴坐标（建议整数或 x.5）

        // 螺旋道路层间距：各层之间竖直间距，重排高度时作为基准比例
        public float RoadLayerSpacing = 5f;

        // 道路预览资源：路段 Mesh 与材质，新建场景时创建 SplineComputer + SplineMesh 并赋值
        public Mesh RoadMesh;
        public Material RoadMaterial;

        public List<RoadSegmentData> RoadSegments = new();
        public int CurrentSegmentIndex = -1;
        public List<PlacedHolderData> Holders = new();
        public List<UndoEntry> UndoStack = new();

        public static CoffeeSortEditData GetOrCreate()
        {
            var data = AssetDatabase.LoadAssetAtPath<CoffeeSortEditData>(AssetPath);
            if (data == null)
            {
                data = CreateInstance<CoffeeSortEditData>();
                AssetDatabase.CreateAsset(data, AssetPath);
                AssetDatabase.SaveAssets();
            }

            return data;
        }

        public void ClearContent()
        {
            RoadSegments.Clear();
            CurrentSegmentIndex = -1;
            Holders.Clear();
            UndoStack.Clear();
        }

        public void MarkDirty()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        // ---- 路段辅助 ----

        public RoadSegmentData CurrentSegment =>
            CurrentSegmentIndex >= 0 && CurrentSegmentIndex < RoadSegments.Count
                ? RoadSegments[CurrentSegmentIndex]
                : null;

        // 无当前路段时自动补一个空路段
        public RoadSegmentData EnsureCurrentSegment()
        {
            if (CurrentSegmentIndex < 0 || CurrentSegmentIndex >= RoadSegments.Count)
            {
                RoadSegments.Add(new RoadSegmentData());
                CurrentSegmentIndex = RoadSegments.Count - 1;
            }

            return RoadSegments[CurrentSegmentIndex];
        }

        public int TotalPathPointCount()
        {
            int count = 0;
            foreach (var seg in RoadSegments)
            {
                count += seg.PathPoints.Count;
                if (seg.Start != null) count++;
                if (seg.End != null) count++;
            }

            return count;
        }

        // 展平所有路段的 [Start, ...PathPoints, End]，返回每个点及所在路段下标
        public List<(PathPointData point, int segmentIndex)> GetAllRoadPoints()
        {
            var all = new List<(PathPointData, int)>();
            for (int s = 0; s < RoadSegments.Count; s++)
            {
                var seg = RoadSegments[s];
                if (seg.Start != null) all.Add((seg.Start, s));
                foreach (var p in seg.PathPoints) all.Add((p, s));
                if (seg.End != null) all.Add((seg.End, s));
            }

            return all;
        }
    }
}
