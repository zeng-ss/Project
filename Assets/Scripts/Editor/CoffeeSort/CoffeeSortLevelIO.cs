using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CasualGame.CoffeeAway;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CasualGame.EditorTools
{
    /// <summary>关卡 JSON 的生成与解析（格式与 Assets/Game/Level/Level-*.json 完全一致）</summary>
    public static class CoffeeSortLevelIO
    {
        public static string BuildJson(CoffeeSortEditData data)
        {
            var root = new JObject
            {
                ["updateDate"] = DateTime.UtcNow.ToString("o"),
                ["isChangeCapacity"] = data.IsChangeCapacity,
                ["capacity"] = data.Capacity,
                ["capacityTextPosition"] = new JObject
                {
                    ["x"] = data.CapTextX,
                    ["y"] = data.CapTextY,
                },
                ["gridSizeX"] = data.GridSizeX,
                ["gridSizeY"] = data.GridSizeY,
                ["IsCloseConveyor"] = data.IsCloseConveyor,
            };

            // gridData：按路段展平 [Start, ...PathPoints, End]，顺序即 pathIndex；
            // 路段首格写 isStart、末格写 isEnd 标记，供回读时切分路段。
            var gridArr = new JArray();
            int pathIdx = 0;
            foreach (var seg in data.RoadSegments)
            {
                var pts = new List<PathPointData>(seg.PathPoints.Count + 2);
                if (seg.Start != null) pts.Add(seg.Start);
                pts.AddRange(seg.PathPoints);
                if (seg.End != null) pts.Add(seg.End);

                if (pts.Count < 2) continue; // 不构成路段的跳过

                for (int i = 0; i < pts.Count; i++)
                {
                    pathIdx++;
                    var cell = new JObject
                    {
                        ["x"] = pts[i].Gx,
                        ["y"] = pts[i].Gy,
                        ["state"] = 1,
                        ["pathIndex"] = pathIdx,
                    };
                    if (i == 0) cell["isStart"] = true;
                    if (i == pts.Count - 1) cell["isEnd"] = true;

                    // 做过 xyz 微调（Y 或 Offset 非零）才写 pX/pY/pZ，保持未微调关卡 JSON 不变
                    if (!Mathf.Approximately(pts[i].Y, 0f)
                        || !Mathf.Approximately(pts[i].OffsetX, 0f)
                        || !Mathf.Approximately(pts[i].OffsetZ, 0f))
                    {
                        cell["pX"] = pts[i].WorldX;
                        cell["pY"] = pts[i].Y;
                        cell["pZ"] = pts[i].WorldZ;
                    }

                    gridArr.Add(cell);
                }
            }

            root["gridData"] = gridArr;

            var holders = new JArray();
            foreach (var h in data.Holders)
            {
                var units = new JArray();
                foreach (var c in h.CarColors)
                {
                    units.Add(new JObject
                    {
                        ["ID"] = new JObject { ["Color"] = c },
                        ["States"] = new JArray(),
                    });
                }

                holders.Add(new JObject
                {
                    ["x"] = h.X,
                    ["y"] = h.Y,
                    ["width"] = h.Width,
                    ["height"] = h.Height,
                    ["direction"] = CoffeeSortPlacement.DirectionFromValue(h.DirectionValue),
                    ["directionValue"] = h.DirectionValue,
                    ["gameUnits"] = units,
                    ["states"] = new JArray(),
                    ["HolderType"] = h.HolderType,
                });
            }

            root["holderDataList"] = holders;
            root["VisibleSizeX"] = data.GridSizeX;
            root["VisibleSizeY"] = data.GridSizeY;

            return root.ToString(Formatting.Indented);
        }

        public static bool Save(CoffeeSortEditData data, int levelNumber, out string message)
        {
            if (data.TotalPathPointCount() < 2)
            {
                message = "至少需要 2 个路径点才能形成道路";
                return false;
            }

            string path = LevelData.GetLevelAssetPath(levelNumber);
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), path);
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                File.WriteAllText(fullPath, BuildJson(data));
                AssetDatabase.ImportAsset(path);
                AssetDatabase.SaveAssets();
                message = $"已保存：{path}（路径点 {data.TotalPathPointCount()}，路段 {data.RoadSegments.Count}，容器 {data.Holders.Count}）";
                return true;
            }
            catch (Exception ex)
            {
                message = $"保存失败：{ex.Message}";
                return false;
            }
        }

        public static bool Load(int levelNumber, CoffeeSortEditData data, out string message)
        {
            string path = LevelData.GetLevelAssetPath(levelNumber);
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), path);
            if (!File.Exists(fullPath))
            {
                message = $"文件不存在：{path}";
                return false;
            }

            LevelData levelData;
            try
            {
                levelData = LevelData.FromJson(File.ReadAllText(fullPath));
            }
            catch (Exception ex)
            {
                message = $"解析失败：{ex.Message}";
                return false;
            }

            if (levelData == null)
            {
                message = "解析失败：JSON 内容无效";
                return false;
            }

            data.ClearContent();
            CoffeeSortPlacement.ClearPointSelection();
            data.GridSizeX = levelData.GridSizeX;
            data.GridSizeY = levelData.GridSizeY;
            data.Capacity = levelData.Capacity;
            data.IsCloseConveyor = levelData.IsCloseConveyor ?? true;
            data.IsChangeCapacity = levelData.IsChangeCapacity;
            if (levelData.CapacityTextPosition != null)
            {
                data.CapTextX = levelData.CapacityTextPosition.X;
                data.CapTextY = levelData.CapacityTextPosition.Y;
            }

            data.EditorSceneName = SceneManager.GetActiveScene().name;
            data.IsActive = true;

            // 道路格子：按 pathIndex 排序（与 LevelBuilder.BuildPathInfo 一致，PathIndex >= 0 才保留）
            var roads = new List<GridCellData>();
            if (levelData.GridData != null)
            {
                foreach (var c in levelData.GridData)
                    if (c.IsRoad && c.PathIndex >= 0)
                        roads.Add(c);
            }

            roads.Sort((a, b) => a.PathIndex.CompareTo(b.PathIndex));
            data.RoadSegments.AddRange(RebuildSegments(roads));
            if (data.RoadSegments.Count > 0) data.CurrentSegmentIndex = 0;

            // 容器
            if (levelData.HolderDataList != null)
            {
                foreach (var hd in levelData.HolderDataList)
                {
                    var h = new PlacedHolderData
                    {
                        HolderType = hd.PrefabType ?? (int)HolderPrefabType.Standard,
                        X = hd.X,
                        Y = hd.Y,
                        Width = hd.Width,
                        Height = hd.Height,
                        DirectionValue = hd.DirectionValue ?? DirValueFromDirection(hd.Direction),
                        CarColors = new List<int>(),
                    };

                    if (hd.GameUnits != null)
                        foreach (var g in hd.GameUnits)
                            h.CarColors.Add(g.Id?.Color ?? 0);

                    data.Holders.Add(h);
                }
            }

            message = $"已载入：{path}（路径点 {data.TotalPathPointCount()}，路段 {data.RoadSegments.Count}，容器 {data.Holders.Count}）";
            return true;
        }

        /// <summary>
        /// 将排好序的道路格子重建为路段列表。
        /// 带 isStart/isEnd 标记：isStart 开启新路段，isEnd 收尾；无标记的旧数据整条归为一个路段的路径点。
        /// </summary>
        private static List<RoadSegmentData> RebuildSegments(List<GridCellData> roads)
        {
            var segments = new List<RoadSegmentData>();
            if (roads.Count == 0) return segments;

            bool hasMarker = roads.Any(c => c.IsStart || c.IsEnd);

            if (!hasMarker)
            {
                var seg = new RoadSegmentData();
                foreach (var c in roads)
                    seg.PathPoints.Add(FromGridCell(c));
                segments.Add(seg);
                return segments;
            }

            RoadSegmentData cur = null;
            foreach (var c in roads)
            {
                if (c.IsStart)
                {
                    cur = new RoadSegmentData
                    {
                        Start = FromGridCell(c),
                    };
                    segments.Add(cur);
                }
                else if (c.IsEnd)
                {
                    if (cur == null)
                    {
                        cur = new RoadSegmentData();
                        segments.Add(cur);
                    }

                    cur.End = FromGridCell(c);
                }
                else
                {
                    if (cur == null)
                    {
                        cur = new RoadSegmentData();
                        segments.Add(cur);
                    }

                    cur.PathPoints.Add(FromGridCell(c));
                }
            }

            return segments;
        }

        // 由道路格子重建路径点：有 pX/pY/pZ（编辑器微调过）时反解 Gx/Gy + Offset + Y，
        // 缺省时以格子中心 (X, Y) 为基点、Y=0
        private static PathPointData FromGridCell(GridCellData c)
        {
            var p = new PathPointData { Gx = c.X, Gy = c.Y };
            if (c.PX.HasValue)
            {
                float wx = c.PX.Value - 0.5f;
                p.Gx = Mathf.RoundToInt(wx);
                p.OffsetX = wx - p.Gx;
            }

            if (c.PY.HasValue) p.Y = c.PY.Value;
            if (c.PZ.HasValue)
            {
                float wz = c.PZ.Value - 0.5f;
                p.Gy = Mathf.RoundToInt(wz);
                p.OffsetZ = wz - p.Gy;
            }

            return p;
        }

        private static float DirValueFromDirection(int? dir)
        {
            switch (dir)
            {
                case (int)LevelBuilder.HolderDirection.Right: return 90f;
                case (int)LevelBuilder.HolderDirection.Down: return 180f;
                case (int)LevelBuilder.HolderDirection.Left: return 270f;
                default: return 0f;
            }
        }
    }
}
