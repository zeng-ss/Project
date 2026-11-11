using System.Collections.Generic;
using UnityEngine;
using Dreamteck.Splines;

namespace CasualGame.CoffeeAway
{
    public static class RoadMeshBuilder
    {
        private const float DefaultRoadWidth = 2f;
        private const float DefaultRoadY = 0.1f;

        /// <summary>
        /// 生成平面道路，已弃用
        /// </summary>
        public static (GameObject , List<Vector3>) BuildPlaneRoad(
            LevelBuilder.PathInfo pathInfo, Material material,Transform parent,
            float roadY = DefaultRoadY, float roadWidth = DefaultRoadWidth,
            float textureRepeatSpacing = 0f)
        {
            var nodes = pathInfo.Nodes;
            int n = nodes.Count;
            bool isClosed = (pathInfo.IsCloseConveyor ?? false) && n > 2;

            var points = new SplinePoint[n];
            for (int i = 0; i < n; i++) points[i] = new SplinePoint(nodes[i].WorldPos);

            // 创建 Computer（用于 PathGenerator + 中心线采样）
            var go = new GameObject("RoadSpline", typeof(SplineComputer));
            go.transform.SetParent(parent);
            go.SetLayerRecursively(Constant.Layer.BgLayerId);
            var computer = go.GetComponent<SplineComputer>();
            computer.type = Spline.Type.BSpline;
            computer.space = SplineComputer.Space.World;
            computer.sampleMode = SplineComputer.SampleMode.Uniform;
            // 设置 SplineComputer
            computer.SetPoints(points);
            if (isClosed) computer.Close();
            else if (computer.isClosed) computer.Break();
            computer.RebuildImmediate();

            // 中心线采样
            // 注意必须用 Cached：Calculate 会对 World 空间点再套一次 TransformPoint，
            // 当 Computer 挂在非原点父级（如 Game 在 (118,101,137)）时中心线会被整体偏移。
            // Cached 与 SplineMesh/PathGenerator 渲染走同一套采样，位置一致。
            int sampleCount = Mathf.Max(n * 15, 500);
            var centerline = new List<Vector3>(sampleCount + 1);
            for (int i = 0; i < sampleCount; i++)
            {
                double pct = (double)i / sampleCount;
                centerline.Add(computer.EvaluatePosition(pct, SplineComputer.EvaluateMode.Cached));
            }
            if (isClosed) centerline.Add(centerline[0]);

            // 设置 PathGenerator
            var roadGO = new GameObject("RoadMesh", typeof(PathGenerator));
            roadGO.transform.SetParent(parent);
            roadGO.SetLayerRecursively(Constant.Layer.BgLayerId);
            var pathGen = roadGO.GetComponent<PathGenerator>();
            pathGen.spline = computer;
            pathGen.slices = 1;

            pathGen.size = roadWidth;
            pathGen.offset = new Vector3(0, roadY, 0);

            // 沿路循环铺贴图：默认 uvScale=(1,1) 会把整条路摊成一个 0~1 UV，贴图被整体拉伸。
            // PathGenerator 的 UV 是 U=宽度方向、V=沿路径方向，所以沿路重复要设 uvScale.y。
            // tiles 取整数保证闭环首尾无缝，烘焙进 Mesh UV
            if (textureRepeatSpacing > 0f)
            {
                float length = 0f;
                for (int i = 1; i < centerline.Count; i++)
                    length += Vector3.Distance(centerline[i - 1], centerline[i]);
                float tiles = Mathf.Max(1f, Mathf.Round(length / textureRepeatSpacing));
                pathGen.uvScale = new Vector2(1f, tiles);
            }

            pathGen.GetComponent<MeshRenderer>().material = material;
            pathGen.RebuildImmediate();

            return (roadGO, centerline);
        }

        /// <summary>
        /// 复用场景中已配置好的 SplineComputer SplineMesh 生成道路
        /// </summary>
        public static List<Vector3> BuildRoadFromExistingSpline(
            LevelBuilder.PathInfo pathInfo, SplineComputer computer, SplineMesh splineMesh)
        {
            var nodes = pathInfo.Nodes;
            int n = nodes.Count;
            bool isClosed = (pathInfo.IsCloseConveyor ?? false) && n > 2;

            var points = new SplinePoint[n];
            for (int i = 0; i < n; i++) points[i] = new SplinePoint(nodes[i].WorldPos);

            computer.SetPoints(points);
            if (isClosed) computer.Close();
            else if (computer.isClosed) computer.Break();
            computer.RebuildImmediate();

            int sampleCount = Mathf.Max(n * 15, 500);
            var centerline = new List<Vector3>(sampleCount + 1);
            for (int i = 0; i < sampleCount; i++)
            {
                // 必须用 Cached：Calculate 会对 World 空间点再套一次 TransformPoint，
                // 当 Computer 挂在非原点父级（如 Game 在 (118,101,137)）时中心线会被整体偏移。
                // Cached 与 SplineMesh 渲染走同一套采样，位置一致。
                centerline.Add(computer.EvaluatePosition((double)i / sampleCount, SplineComputer.EvaluateMode.Cached));
            }
            if (isClosed) centerline.Add(centerline[0]);

            float totalLen = 0f;
            for (int i = 1; i < centerline.Count; i++) totalLen += Vector3.Distance(centerline[i - 1], centerline[i]);

            for (int c = 0; c < splineMesh.GetChannelCount(); c++)
            {
                var ch = splineMesh.GetChannel(c);
                if (ch.GetMeshCount() == 0) continue;

                float Len = 0f;
                for (int m = 0; m < ch.GetMeshCount(); m++)
                    Len += Mathf.Max(0.01f, ch.GetMesh(m).bounds.size.z);
                Len /= ch.GetMeshCount();
                ch.count = Mathf.RoundToInt(totalLen / Len) * 5;
                /*if (ch.autoCount) continue;
                if (ch.type == SplineMesh.Channel.Type.Extrude)
                {
                    // 路面通道：autoCount 只是按 mesh 自身长度估段数（段数≈总长/段长），
                    // 每段路面太长，拐弯处段数太少会出棱角。这里关闭 autoCount，
                    // 改按更细的目标段长 smoothSegLen 算段数，保证拐弯圆滑。
                    // 每局都重算，换关卡长度不同也正确。
                    ch.autoCount = false;
                    float meshLen = 0f;
                    for (int m = 0; m < ch.GetMeshCount(); m++)
                        meshLen += Mathf.Max(0.01f, ch.GetMesh(m).bounds.size.z);
                    meshLen /= ch.GetMeshCount();

                    float step = Mathf.Max(0.02f, Mathf.Min(meshLen, smoothSegLen));
                    ch.count = isClosed
                        ? Mathf.Max(2, Mathf.RoundToInt(totalLen / step))
                        : Mathf.Max(1, Mathf.RoundToInt(totalLen / step));
                }
                else if (!ch.autoCount)
                {
                    // 摆件通道：保留原有逻辑，autoCount 关闭时尊重手动 Count
                    float segLen = 0f;
                    for (int m = 0; m < ch.GetMeshCount(); m++)
                        segLen += Mathf.Max(0.01f, ch.GetMesh(m).bounds.size.z);
                    segLen /= ch.GetMeshCount();

                    ch.count = isClosed
                        ? Mathf.Max(2, Mathf.RoundToInt(totalLen / segLen) + 1)
                        : Mathf.Max(1, Mathf.RoundToInt(totalLen / segLen));
                }
                // else：autoCount 打开的摆件通道由 Dreamteck 自行计算*/
            }

            splineMesh.RebuildImmediate();
            return centerline;
        }

        /// <summary>
        /// 多路段道路生成（临时测试）：按 gridData 的 isStart/isEnd 标记把整条道路切成多个路段，
        /// 每段用模板 computer/splineMesh 复制一份 SplineComputer + SplineMesh，再复用
        /// BuildRoadFromExistingSpline 生成道路网格。素材关卡（多段路）专用；
        /// 正经关卡只有一段，等价于原 BuildRoadFromExistingSpline。后续搬到 playable 项目时再接入导航。
        /// </summary>
        public static List<(SplineComputer computer, SplineMesh splineMesh, List<Vector3> centerline)> BuildRoadBySegments(
            LevelData levelData, SplineComputer templateComputer, SplineMesh templateSplineMesh, Transform parent)
        {
            var result = new List<(SplineComputer, SplineMesh, List<Vector3>)>();
            if (levelData?.GridData == null || templateComputer == null || templateSplineMesh == null) return result;

            // 1. 收集道路格并按 pathIndex 排序（与 LevelBuilder.BuildPathInfo 保持一致）
            var roads = new List<GridCellData>();
            foreach (var c in levelData.GridData)
                if (c.IsRoad && c.PathIndex >= 0)
                    roads.Add(c);
            roads.Sort((a, b) => a.PathIndex.CompareTo(b.PathIndex));

            // 2. 按 isStart/isEnd 切分路段；无标记的旧数据整段归为一个路段
            var segments = SplitRoadSegments(roads);

            // 3. 每段独立复制模板并生成道路网格。
            // IsCloseConveyor=true 时每段各自闭合（终点连回起点成环），多路段也逐段闭合。
            bool isClosedLoop = levelData.IsCloseConveyor ?? false;
            for (int i = 0; i < segments.Count; i++)
            {
                var segLevelData = new LevelData
                {
                    GridData = segments[i],
                    IsCloseConveyor = isClosedLoop,
                };
                var pathInfo = LevelBuilder.BuildPathInfo(segLevelData);
                if (pathInfo.Nodes.Count < 2) continue;

                var go = new GameObject($"RoadSegment{i + 1}");
                go.transform.SetParent(parent);
                go.SetLayerRecursively(Constant.Layer.BgLayerId);
                var computer = go.AddComponent<SplineComputer>();
                var splineMesh = go.AddComponent<SplineMesh>();

                CopyComputer(templateComputer, computer);
                CopySplineMesh(templateSplineMesh, splineMesh);
                splineMesh.spline = computer;

                var centerline = BuildRoadFromExistingSpline(pathInfo, computer, splineMesh);
                result.Add((computer, splineMesh, centerline));
            }

            return result;
        }

        private static void CopyComputer(SplineComputer source, SplineComputer target)
        {
            target.type = source.type;
            target.space = source.space;
            target.sampleMode = source.sampleMode;
        }

        private static void CopySplineMesh(SplineMesh source, SplineMesh target)
        {
            target.size = source.size;
            target.color = source.color;
            target.offset = source.offset;
            target.rotation = source.rotation;
            target.uvScale = source.uvScale;
            target.uvOffset = source.uvOffset;
            target.normalMethod = source.normalMethod;
            target.useSplineSize = source.useSplineSize;
            target.useSplineColor = source.useSplineColor;
            target.calculateTangents = source.calculateTangents;
            target.flipFaces = source.flipFaces;
            target.doubleSided = source.doubleSided;
            target.uvMode = source.uvMode;
            target.uvRotation = source.uvRotation;
            target.meshIndexFormat = source.meshIndexFormat;
            target.markDynamic = source.markDynamic;

            for (int c = 0; c < source.GetChannelCount(); c++)
            {
                var srcCh = source.GetChannel(c);
                var newCh = target.AddChannel(srcCh.name);
                srcCh.CopyTo(newCh);
                newCh.autoCount = srcCh.autoCount;
                newCh.uvScale = srcCh.uvScale;
                newCh.uvOffset = srcCh.uvOffset;
                newCh.randomOrder = srcCh.randomOrder;
            }

            var srcRend = source.GetComponent<MeshRenderer>();
            var rend = target.GetComponent<MeshRenderer>();
            if (srcRend != null && rend != null)
                rend.sharedMaterials = srcRend.sharedMaterials;
        }

        /// <summary>
        /// 把按 pathIndex 排好序的道路格按 isStart/isEnd 标记切成多个路段。
        /// isStart 开启新路段、isEnd 收尾；无标记的旧数据整段归为一个路段。
        /// </summary>
        private static List<List<GridCellData>> SplitRoadSegments(List<GridCellData> roads)
        {
            var segments = new List<List<GridCellData>>();
            if (roads.Count == 0) return segments;

            bool hasMarker = false;
            foreach (var c in roads)
            {
                if (c.IsStart || c.IsEnd)
                {
                    hasMarker = true;
                    break;
                }
            }

            if (!hasMarker)
            {
                segments.Add(roads);
                return segments;
            }

            List<GridCellData> cur = null;
            foreach (var c in roads)
            {
                if (c.IsStart)
                {
                    cur = new List<GridCellData> { c };
                    segments.Add(cur);
                }
                else if (c.IsEnd)
                {
                    if (cur == null)
                    {
                        cur = new List<GridCellData>();
                        segments.Add(cur);
                    }

                    cur.Add(c);
                    cur = null; // 路段在 isEnd 收尾，下一段由 isStart 重新开启
                }
                else
                {
                    if (cur == null)
                    {
                        cur = new List<GridCellData>();
                        segments.Add(cur);
                    }

                    cur.Add(c);
                }
            }

            return segments;
        }
    }
}
