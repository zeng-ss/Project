using System.Collections.Generic;
using CasualGame.CoffeeAway;
using Dreamteck.Splines;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CasualGame.EditorTools
{
    /// <summary>
    /// Scene 视图渲染与输入：网格、多路段道路、容器、放置幽灵；
    /// Cmd+点击放路径点，端点赋值模式下点击设置/替换/移除端点，左键放置容器，Q 旋转，ESC/右键取消。
    /// 道路预览：Data 里赋值 RoadMesh/RoadMaterial 后，用场景中的 SplineComputer+SplineMesh 实时渲染当前路段。
    /// </summary>
    [InitializeOnLoad]
    public static class CoffeeSortSceneView
    {
        private const float PathPointDiscRadius = 0.22f;
        private const string RoadPreviewObjectName = "CoffeeSortRoadPreview";

        private static GUIStyle _coordStyle;
        private static GUIStyle CoordStyle => _coordStyle ??= new GUIStyle(EditorStyles.label)
        {
            fontSize = 10,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(1f, 1f, 1f, 0.7f) },
        };

        private static CoffeeSortEditData _data;
        private static SplineComputer _roadPreviewComputer;
        private static SplineMesh _roadPreviewMesh;
        private static Mesh _configuredRoadMesh;
        private static string _roadPreviewSignature;

        /// <summary>编辑窗口是否打开；仅在窗口打开时才允许创建/加载编辑数据</summary>
        public static bool WindowOpen { get; set; }

        static CoffeeSortSceneView()
        {
            SceneView.duringSceneGui += OnSceneGUI;
            EditorApplication.update += OnEditorUpdate;
            EditorApplication.delayCall += () =>
            {
                if (UnityEditor.AssetDatabase.LoadAssetAtPath<CoffeeSortEditData>(CoffeeSortEditData.AssetPath) != null)
                    _data = CoffeeSortEditData.GetOrCreate();
            };
        }

        private static void OnEditorUpdate()
        {
            // 放置/删除模式下持续刷新 Scene 视图，让幽灵/悬停高亮跟随鼠标保持流畅
            if (CoffeeSortPlacement.IsPlacing || CoffeeSortPlacement.IsSettingEndpoint || CoffeeSortPlacement.IsDeletingHolder)
                SceneView.RepaintAll();
        }

        private static void OnSceneGUI(SceneView sv)
        {
            // _data 仅在窗口打开（或资源已存在）时加载，避免无关场景里自动创建编辑数据
            if (_data == null)
            {
                if (!WindowOpen) return;
                _data = CoffeeSortEditData.GetOrCreate();
            }

            if (!_data.IsActive) return;
            if (string.IsNullOrEmpty(_data.EditorSceneName)) return;
            if (SceneManager.GetActiveScene().name != _data.EditorSceneName) return;

            var data = _data;

            // 选中的路径点先渲染 PositionHandle：拖动手柄时手柄会吃掉鼠标事件，
            // HandleInput 就不会在同一帧误判为“选择/放置”。
            DrawPointEditHandle(data);

            HandleInput(sv, data);

            if (Event.current.type == EventType.Repaint)
            {
                DrawGrid(data);
                DrawRoad(data);
                DrawHolders(data);
                DrawGhost(data);
            }
        }

        /// <summary>销毁预览 Spline 对象（窗口关闭时调用）</summary>
        public static void DisposePreview()
        {
            if (_roadPreviewComputer != null)
            {
                if (_roadPreviewComputer.gameObject != null)
                    Object.DestroyImmediate(_roadPreviewComputer.gameObject);
                _roadPreviewComputer = null;
            }

            _roadPreviewMesh = null;
            _configuredRoadMesh = null;
            _roadPreviewSignature = null;
        }

        /// <summary>路段切换/路径点变更后刷新道路预览</summary>
        public static void UpdateRoadPreview(CoffeeSortEditData data)
        {
            _roadPreviewSignature = null; // 强制下次 Repaint 重建
            SceneView.RepaintAll();
        }

        /// <summary>强制清空道路预览（新建场景时调用，防止残留旧点/旧网格）</summary>
        public static void ResetRoadPreview()
        {
            _roadPreviewSignature = null;
            _configuredRoadMesh = null;
            if (_roadPreviewComputer != null && _roadPreviewComputer.gameObject != null)
            {
                _roadPreviewComputer.SetPoints(new SplinePoint[0]);
                _roadPreviewComputer.gameObject.SetActive(false);
            }
        }

        /// <summary>在场景中创建道路预览组件（SplineComputer + SplineMesh），供新建场景时调用</summary>
        public static void EnsureRoadPreviewInScene(CoffeeSortEditData data)
        {
            EnsureRoadPreview();
            ReconfigureRoadChannel(data);
        }

        public static void FrameView(CoffeeSortEditData data)
        {
            var sv = SceneView.lastActiveSceneView;
            if (sv == null) return;

            Vector3 center = new Vector3(data.GridSizeX / 2f, 0f, data.GridSizeY / 2f);
            sv.LookAt(center, Quaternion.Euler(90f, 0f, 0f), Mathf.Max(data.GridSizeX, data.GridSizeY) * 1.1f);
            sv.Repaint();
        }

        // ---- 输入 ----

        private static void HandleInput(SceneView sv, CoffeeSortEditData data)
        {
            var ev = Event.current;

            if (ev.type == EventType.MouseDown)
            {
                if (ev.button == 0)
                {
                    // 删除模式优先：左键点击命中的容器即删除（吞掉 Cmd+点击，避免误加路径点）
                    if (CoffeeSortPlacement.IsDeletingHolder)
                    {
                        bool deleted = CoffeeSortPlacement.TryDeleteHolderAt(data, GetMouseWorldRaw());
                        if (deleted) data.MarkDirty();
                        ev.Use();
                        sv.Repaint();
                        return;
                    }

                    // 端点赋值模式：左键点击即把该点设为当前路段端点（再点已设端点则移除）
                    if (CoffeeSortPlacement.IsSettingEndpoint)
                    {
                        TryAddPathPoint(data);
                        ev.Use();
                        sv.Repaint();
                        return;
                    }

                    // Cmd/Ctrl + 左键：放置路径点（吸附到格子交点）
                    if (ev.command || ev.control)
                    {
                        TryAddPathPoint(data);
                        ev.Use();
                        sv.Repaint();
                        return;
                    }

                    // 放置模式：左键落下当前幽灵
                    if (CoffeeSortPlacement.IsPlacing)
                    {
                        Vector3 snap = GetMouseWorld(true);
                        bool ok = CoffeeSortPlacement.TryPlaceAt(snap, data);
                        if (!ok)
                            Debug.LogWarning("[CoffeeSort] 该位置不可放置（重叠/越界），请换位置");
                        else
                            data.MarkDirty();

                        ev.Use();
                        sv.Repaint();
                        return;
                    }

                    // 空闲模式：左键选择路径点（命中即选中，未命中清空选中），
                    // 选中后可用 Scene 手柄拖动或窗口 X/Y/Z 输入框微调
                    TrySelectPathPoint(data);
                    ev.Use();
                    sv.Repaint();
                    return;
                }
                else if (ev.button == 1)
                {
                    bool changed = CoffeeSortPlacement.IsPlacing
                        || CoffeeSortPlacement.IsSettingEndpoint
                        || CoffeeSortPlacement.IsDeletingHolder;
                    if (changed) CoffeeSortPlacement.Cancel();
                    if (changed)
                    {
                        ev.Use();
                        sv.Repaint();
                    }
                }
            }
            else if (ev.type == EventType.KeyDown)
            {
                if (CoffeeSortPlacement.IsPlacing && ev.keyCode == KeyCode.Q)
                {
                    CoffeeSortPlacement.Rotate();
                    ev.Use();
                    sv.Repaint();
                }
                else if ((CoffeeSortPlacement.IsPlacing || CoffeeSortPlacement.IsSettingEndpoint || CoffeeSortPlacement.IsDeletingHolder)
                         && ev.keyCode == KeyCode.Escape)
                {
                    CoffeeSortPlacement.Cancel();
                    ev.Use();
                    sv.Repaint();
                }
            }
        }

        private static void TryAddPathPoint(CoffeeSortEditData data)
        {
            Vector3 snapped = GetMouseWorld(false);
            int gx = Mathf.RoundToInt(snapped.x);
            int gy = Mathf.RoundToInt(snapped.z);
            gx = Mathf.Clamp(gx, 0, data.GridSizeX - 1);
            gy = Mathf.Clamp(gy, 0, data.GridSizeY - 1);

            if (CoffeeSortPlacement.SettingStart)
            {
                SetEndpoint(data, true, gx, gy);
                return;
            }

            if (CoffeeSortPlacement.SettingEnd)
            {
                SetEndpoint(data, false, gx, gy);
                return;
            }

            var seg = EnsureSegment(data);
            if (seg.Start != null && seg.Start.Gx == gx && seg.Start.Gy == gy ||
                seg.End != null && seg.End.Gx == gx && seg.End.Gy == gy)
            {
                Debug.Log($"[CoffeeSort] ({gx},{gy}) 已被起点/终点占用");
                return;
            }

            foreach (var p in seg.PathPoints)
            {
                if (p.Gx == gx && p.Gy == gy)
                {
                    Debug.Log($"[CoffeeSort] 路径点 ({gx},{gy}) 已存在");
                    return;
                }
            }

            seg.PathPoints.Add(new PathPointData { Gx = gx, Gy = gy });
            data.UndoStack.Add(new UndoEntry
            {
                OpType = UndoEntry.Op.AddPathPoint,
                Index = data.CurrentSegmentIndex,
                Point = new PathPointData { Gx = gx, Gy = gy },
            });
            data.MarkDirty();
            UpdateRoadPreview(data);
            LogSegmentState(data, $"添加路径点({gx},{gy})");
        }

        /// <summary>
        /// 设置/替换/移除当前路段的起点或终点：
        /// 点击已设端点 → 移除；点击其他点 → 重新赋值（替换旧端点）。
        /// 多路段请用「新建路段」按钮创建新段后再赋值，这里不会隐式开新段。
        /// </summary>
        private static void SetEndpoint(CoffeeSortEditData data, bool isStart, int gx, int gy)
        {
            var seg = EnsureSegment(data);
            int segIndex = data.CurrentSegmentIndex;
            var point = new PathPointData { Gx = gx, Gy = gy };

            if (isStart)
            {
                var old = seg.Start;
                bool removing = old != null && old.Gx == gx && old.Gy == gy;
                seg.Start = removing ? null : point;
                data.UndoStack.Add(new UndoEntry
                {
                    OpType = UndoEntry.Op.SetStart,
                    Index = segIndex,
                    Point = old, // 撤销时恢复操作前的起点（null=原本无起点，或原起点）
                });
                Debug.Log(removing ? $"[CoffeeSort] 移除起点 ({gx},{gy})" : $"[CoffeeSort] 设置起点 ({gx},{gy})");
            }
            else
            {
                var old = seg.End;
                bool removing = old != null && old.Gx == gx && old.Gy == gy;
                seg.End = removing ? null : point;
                data.UndoStack.Add(new UndoEntry
                {
                    OpType = UndoEntry.Op.SetEnd,
                    Index = segIndex,
                    Point = old,
                });
                Debug.Log(removing ? $"[CoffeeSort] 移除终点 ({gx},{gy})" : $"[CoffeeSort] 设置终点 ({gx},{gy})");
            }

            // 一次性端点模式：赋值/移除完成后自动关闭，下一次点击恢复为添加路径点
            CoffeeSortPlacement.ClearEndpointMode();

            data.MarkDirty();
            UpdateRoadPreview(data);
            LogSegmentState(data, isStart ? "设置起点" : "设置终点");
        }

        /// <summary>显式清除当前路段的起点/终点（带撤销）</summary>
        public static void ClearEndpoint(CoffeeSortEditData data, bool isStart)
        {
            var seg = data.CurrentSegment;
            if (seg == null) return;

            if (isStart)
            {
                if (seg.Start == null) return;
                data.UndoStack.Add(new UndoEntry
                {
                    OpType = UndoEntry.Op.SetStart,
                    Index = data.CurrentSegmentIndex,
                    Point = seg.Start, // 撤销时恢复被清除的起点
                });
                seg.Start = null;
            }
            else
            {
                if (seg.End == null) return;
                data.UndoStack.Add(new UndoEntry
                {
                    OpType = UndoEntry.Op.SetEnd,
                    Index = data.CurrentSegmentIndex,
                    Point = seg.End,
                });
                seg.End = null;
            }

            data.MarkDirty();
            UpdateRoadPreview(data);
            LogSegmentState(data, isStart ? "清除起点" : "清除终点");
        }

        private static void LogSegmentState(CoffeeSortEditData data, string action)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append($"[CoffeeSort] {action}：共 {data.RoadSegments.Count} 条路段");
            for (int s = 0; s < data.RoadSegments.Count; s++)
            {
                var seg = data.RoadSegments[s];
                sb.Append($" | 段{s} 起点={(seg.Start != null ? $"({seg.Start.Gx},{seg.Start.Gy})" : "无")}");
                sb.Append($" 终点={(seg.End != null ? $"({seg.End.Gx},{seg.End.Gy})" : "无")}");
                sb.Append($" 路径点({seg.PathPoints.Count})=");
                foreach (var p in seg.PathPoints)
                    sb.Append($"({p.Gx},{p.Gy})");
            }

            Debug.Log(sb.ToString());
        }

        private static RoadSegmentData EnsureSegment(CoffeeSortEditData data)
        {
            var seg = data.CurrentSegment;
            if (seg != null) return seg;

            seg = data.EnsureCurrentSegment();
            data.UndoStack.Add(new UndoEntry
            {
                OpType = UndoEntry.Op.AddSegment,
                Index = data.RoadSegments.Count - 1,
            });
            return seg;
        }

        // 鼠标射线与 y=0 平面求交；halfGrid=true 吸附 0.5（容器），false 吸附 1（路径点交点）
        private static Vector3 GetMouseWorld(bool halfGrid)
        {
            var ev = Event.current;
            Ray ray = HandleUtility.GUIPointToWorldRay(ev.mousePosition);
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            if (plane.Raycast(ray, out float enter))
            {
                Vector3 p = ray.GetPoint(enter);
                float step = halfGrid ? 0.5f : 1f;
                return new Vector3(Mathf.Round(p.x / step) * step, 0f, Mathf.Round(p.z / step) * step);
            }

            return Vector3.zero;
        }

        // 未吸附的原始鼠标位置：删除容器命中检测用，避免整数吸附在容器边缘漏点
        private static Vector3 GetMouseWorldRaw()
        {
            var ev = Event.current;
            Ray ray = HandleUtility.GUIPointToWorldRay(ev.mousePosition);
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            if (plane.Raycast(ray, out float enter))
                return ray.GetPoint(enter);

            return Vector3.zero;
        }

        // ---- 路径点选中 / 微调 ----

        // 空闲模式下左键点击路径点：命中最近的点则选中，否则清空选中
        private static void TrySelectPathPoint(CoffeeSortEditData data)
        {
            Vector2 mouse = Event.current.mousePosition;
            PathPointData best = null;
            int bestSeg = -1;
            float bestSqr = 15f * 15f; // 屏幕像素阈值

            for (int s = 0; s < data.RoadSegments.Count; s++)
            {
                foreach (var p in GetSegmentPoints(data.RoadSegments[s]))
                {
                    Vector2 screen = HandleUtility.WorldToGUIPoint(p.WorldPos);
                    float d = (screen - mouse).sqrMagnitude;
                    if (d < bestSqr)
                    {
                        bestSqr = d;
                        best = p;
                        bestSeg = s;
                    }
                }
            }

            CoffeeSortPlacement.SelectPoint(bestSeg, best);
            if (best != null)
                Debug.Log($"[CoffeeSort] 选中路径点 段{bestSeg + 1} ({best.Gx},{best.Gy}) " +
                          $"X={best.WorldX:F2} Y={best.Y:F2} Z={best.WorldZ:F2}");
        }

        // 选中路径点后显示移动手柄：拖动按半格吸附（手柄 snap + ApplyWorldPos 兜底吸附），
        // 反解回 Gx/Gy + Offset + Y
        private static void DrawPointEditHandle(CoffeeSortEditData data)
        {
            var sel = CoffeeSortPlacement.SelectedPoint;
            if (sel == null) return;

            EditorGUI.BeginChangeCheck();
            // 本版 Unity 的 PositionHandle 无 snap 参数；半格吸附由 ApplyWorldPos 完成，
            // 每帧把手柄位置更新为吸附后的值，拖动即按 0.5 步进。
            Vector3 newPos = Handles.PositionHandle(sel.WorldPos, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                CoffeeSortPlacement.ApplyWorldPos(sel, newPos.x, newPos.y, newPos.z);
                data.MarkDirty();
                UpdateRoadPreview(data);
            }
        }

        // ---- 路段辅助 ----

        // 展平路段为 [Start, ...PathPoints, End]，缺端点时自然省略
        private static List<PathPointData> GetSegmentPoints(RoadSegmentData seg)
        {
            var list = new List<PathPointData>(seg.PathPoints.Count + 2);
            if (seg.Start != null) list.Add(seg.Start);
            list.AddRange(seg.PathPoints);
            if (seg.End != null) list.Add(seg.End);
            return list;
        }

        // ---- 绘制 ----

        private static void DrawGrid(CoffeeSortEditData data)
        {
            Handles.color = new Color(0.6f, 0.6f, 0.6f, 0.45f);
            for (int i = 0; i <= data.GridSizeX; i++)
                Handles.DrawLine(new Vector3(i, 0f, 0f), new Vector3(i, 0f, data.GridSizeY));
            for (int j = 0; j <= data.GridSizeY; j++)
                Handles.DrawLine(new Vector3(0f, 0f, j), new Vector3(data.GridSizeX, 0f, j));

            Handles.color = new Color(1f, 1f, 1f, 0.8f);
            Handles.DrawLine(new Vector3(0f, 0.001f, 0f), new Vector3(data.GridSizeX, 0.001f, 0f));
            Handles.DrawLine(new Vector3(0f, 0.001f, data.GridSizeY), new Vector3(data.GridSizeX, 0.001f, data.GridSizeY));
            Handles.DrawLine(new Vector3(0f, 0.001f, 0f), new Vector3(0f, 0.001f, data.GridSizeY));
            Handles.DrawLine(new Vector3(data.GridSizeX, 0.001f, 0f), new Vector3(data.GridSizeX, 0.001f, data.GridSizeY));

            DrawCoordLabels(data);
        }

        // 格子边缘坐标标注：底部标注 X（列），左侧标注 Z（行），方便数格子放路径点
        private static void DrawCoordLabels(CoffeeSortEditData data)
        {
            var style = CoordStyle;
            for (int i = 0; i <= data.GridSizeX; i++)
                Handles.Label(new Vector3(i, 0.02f, -0.7f), i.ToString(), style);
            for (int j = 0; j <= data.GridSizeY; j++)
                Handles.Label(new Vector3(-0.7f, 0.02f, j), j.ToString(), style);
        }

        private static void DrawRoad(CoffeeSortEditData data)
        {
            // 每段道路 [Start, ...PathPoints, End] 覆盖的格子填充
            Color fill = new Color(0.2f, 0.72f, 0.35f, 0.32f);
            foreach (var seg in data.RoadSegments)
            {
                var pts = GetSegmentPoints(seg);
                if (pts.Count < 2) continue;
                for (int i = 0; i < pts.Count - 1; i++)
                    FillCellsBetween(pts[i], pts[i + 1], fill);
            }

            // 道路中心线 + 当前路段插件预览
            DrawRoadSpline(data);

            // 路径点（绿点）、起点（绿色球）、终点（红色球）
            foreach (var seg in data.RoadSegments)
            {
                foreach (var p in seg.PathPoints)
                    DrawPathPointDisc(p);
                if (seg.Start != null) DrawEndpointSphere(seg.Start, Color.green);
                if (seg.End != null) DrawEndpointSphere(seg.End, Color.red);
            }

            DrawSelectedPointHighlight();
        }

        // 当前路段：RoadMesh 已赋值时用 SplineComputer+SplineMesh 渲染真实道路；全部路段画折线中心线
        private static void DrawRoadSpline(CoffeeSortEditData data)
        {
            DrawRoadPolyline(data);

            var seg = data.CurrentSegment;
            var points = seg == null ? null : GetSegmentPoints(seg);
            if (points == null || points.Count < 2 || data.RoadMesh == null)
            {
                HideRoadPreview();
                return;
            }

            EnsureRoadPreview();
            ReconfigureRoadChannel(data);

            string sig = $"{data.CurrentSegmentIndex}";
            for (int i = 0; i < points.Count; i++)
                sig += $"|{points[i].Gx},{points[i].Gy}:{points[i].Y:F3},{points[i].OffsetX:F3},{points[i].OffsetZ:F3}";

            if (sig != _roadPreviewSignature)
            {
                _roadPreviewSignature = sig;
                ApplyRoadPreviewPoints(points, data.IsCloseConveyor);
            }
        }

        private static void DrawRoadPolyline(CoffeeSortEditData data)
        {
            Handles.color = new Color(1f, 0.8f, 0.25f, 0.9f);
            foreach (var seg in data.RoadSegments)
            {
                var pts = GetSegmentPoints(seg);
                if (pts.Count < 2) continue;
                var line = new Vector3[pts.Count];
                for (int i = 0; i < pts.Count; i++)
                    line[i] = new Vector3(pts[i].WorldX, pts[i].Y + 0.02f, pts[i].WorldZ);
                Handles.DrawPolyLine(line);
            }
        }

        private static void DrawPathPointDisc(PathPointData p)
        {
            var pos = new Vector3(p.WorldX, p.Y + 0.015f, p.WorldZ);
            Handles.color = new Color(0f, 1f, 0f, 0.35f);
            Handles.DrawSolidDisc(pos, Vector3.up, PathPointDiscRadius + 0.08f);
            Handles.color = Color.green;
            Handles.DrawSolidDisc(pos, Vector3.up, PathPointDiscRadius);
        }

        private static void DrawEndpointSphere(PathPointData p, Color color)
        {
            Vector3 pos = new Vector3(p.WorldX, p.Y + 0.2f, p.WorldZ);
            Handles.color = color;
            Handles.DrawWireCube(pos, Vector3.one * 0.45f);
        }

        // 选中的路径点高亮：黄色线框 + 亮色底盘，与普通绿点区分
        private static void DrawSelectedPointHighlight()
        {
            var sel = CoffeeSortPlacement.SelectedPoint;
            if (sel == null) return;

            var pos = sel.WorldPos;
            Handles.color = new Color(1f, 0.85f, 0f, 0.4f);
            Handles.DrawSolidDisc(pos, Vector3.up, PathPointDiscRadius + 0.2f);
            Handles.color = new Color(1f, 0.9f, 0f, 1f);
            Handles.DrawWireCube(pos + Vector3.up * 0.05f, Vector3.one * 0.7f);
        }

        // ---- 插件道路预览（SplineComputer + SplineMesh） ----

        private static void EnsureRoadPreview()
        {
            if (_roadPreviewComputer != null && _roadPreviewComputer.gameObject != null) return;

            // 重建/新建对象时清空缓存，强制重新配置通道
            _configuredRoadMesh = null;
            _roadPreviewSignature = null;

            // 清理历史残留的预览对象（可能含未激活/重复对象，Dreamteck 编辑器会干扰绘制）
            foreach (var sc in Resources.FindObjectsOfTypeAll<SplineComputer>())
            {
                if (sc == null || sc.gameObject == null) continue;
                if (!sc.gameObject.scene.IsValid()) continue; // 跳过 prefab 资源
                if (sc.gameObject.name == RoadPreviewObjectName)
                    Object.DestroyImmediate(sc.gameObject);
            }

            // 全新创建：隐藏于层级面板（不可选中，避免 Dreamteck gizmo/Inspector 干扰），
            // 且不随场景保存（防止跨会话残留旧点）。
            var go = new GameObject(RoadPreviewObjectName);
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            go.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave;

            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            _roadPreviewComputer = go.AddComponent<SplineComputer>();
            _roadPreviewComputer.type = Spline.Type.BSpline;
            _roadPreviewComputer.space = SplineComputer.Space.World;
            _roadPreviewComputer.sampleMode = SplineComputer.SampleMode.Uniform;
            // 初始 0 点，道路不会从预览对象原点拉出
            _roadPreviewComputer.SetPoints(new SplinePoint[0]);
            _roadPreviewComputer.editorAlwaysDraw = false; // 只保留我们自己的 Handles 绘制

            _roadPreviewMesh = go.AddComponent<SplineMesh>();
            _roadPreviewMesh.spline = _roadPreviewComputer;

            // 默认隐藏，直到 ApplyRoadPreviewPoints 填入有效点序才激活渲染
            go.SetActive(false);
        }

        // RoadMesh 变化时重建通道并刷新材质
        private static void ReconfigureRoadChannel(CoffeeSortEditData data)
        {
            if (_roadPreviewMesh == null) return;
            if (_configuredRoadMesh == data.RoadMesh) return;
            _configuredRoadMesh = data.RoadMesh;

            while (_roadPreviewMesh.GetChannelCount() > 0)
                _roadPreviewMesh.RemoveChannel(0);

            if (data.RoadMesh != null)
            {
                var ch = _roadPreviewMesh.AddChannel(data.RoadMesh, "Road");
                ch.autoCount = true; // 按路段长度自动分段，避免整条路拉成一个拉伸的格子
                var mr = _roadPreviewMesh.GetComponent<MeshRenderer>();
                if (mr != null && data.RoadMaterial != null)
                    mr.sharedMaterial = data.RoadMaterial;
            }

            // 不在换 Mesh 时立即重建：此时 SplineComputer 可能仍持有旧点/空点，
            // 会在目标位置生成错误的道路；点序变更统一由 ApplyRoadPreviewPoints 重建。
        }

        // 与 RoadMeshBuilder.BuildRoadFromExistingSpline 一致：SetPoints → Close/Break → RebuildImmediate
        private static void ApplyRoadPreviewPoints(List<PathPointData> points, bool close)
        {
            int n = points.Count;
            var splinePoints = new SplinePoint[n];
            for (int i = 0; i < n; i++)
            {
                var p = points[i];
                splinePoints[i] = new SplinePoint(new Vector3(p.WorldX, p.Y + 0.02f, p.WorldZ));
            }

            _roadPreviewComputer.gameObject.SetActive(true);
            _roadPreviewComputer.SetPoints(splinePoints);
            if (close && n >= 3) _roadPreviewComputer.Close();
            else if (_roadPreviewComputer.isClosed) _roadPreviewComputer.Break();
            _roadPreviewComputer.RebuildImmediate();
            _roadPreviewMesh.RebuildImmediate();
        }

        private static void HideRoadPreview()
        {
            _roadPreviewSignature = null;
            if (_roadPreviewComputer != null && _roadPreviewComputer.gameObject != null)
                _roadPreviewComputer.gameObject.SetActive(false);
        }

        private static void FillCellsBetween(PathPointData a, PathPointData b, Color color)
        {
            int x0 = a.Gx, y0 = a.Gy, x1 = b.Gx, y1 = b.Gy;
            int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx + dy;

            while (true)
            {
                DrawCellQuad(x0, y0, color);
                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 >= dy)
                {
                    err += dy;
                    x0 += sx;
                }

                if (e2 <= dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }

        private static void DrawCellQuad(int gx, int gy, Color color)
        {
            Vector3 p0 = new Vector3(gx, 0.005f, gy);
            Vector3 p1 = new Vector3(gx + 1f, 0.005f, gy);
            Vector3 p2 = new Vector3(gx + 1f, 0.005f, gy + 1f);
            Vector3 p3 = new Vector3(gx, 0.005f, gy + 1f);
            Handles.DrawSolidRectangleWithOutline(new[] { p0, p1, p2, p3 }, color, Color.clear);
        }

        private static void DrawHolders(CoffeeSortEditData data)
        {
            for (int i = 0; i < data.Holders.Count; i++)
            {
                var h = data.Holders[i];

                if (h.CarColors.Count > 0)
                {
                    for (int j = 0; j < h.CarColors.Count; j++)
                    {
                        Vector3 pos = CoffeeSortPlacement.CalcCarWorldPos(j, h.X, h.Y, h.Width, h.Height, h.DirectionValue,
                            CoffeeSortPlacement.IsFrontAligned(h.HolderType));
                        DrawCarBox(pos, SpawnFactory.GetColor((ColorType)h.CarColors[j]), h.DirectionValue, false);
                    }
                }
                else
                {
                    DrawContainerBox(h.X, h.Y, h.Width, h.Height, h.DirectionValue, false);
                }
            }
        }

        private static void DrawGhost(CoffeeSortEditData data)
        {
            if (CoffeeSortPlacement.IsDeletingHolder)
            {
                DrawDeleteHover(data);
                return;
            }

            if (!CoffeeSortPlacement.IsPlacing) return;

            Vector3 snap = GetMouseWorld(true);
            var (w, h) = CoffeeSortPlacement.ContainerSize(CoffeeSortPlacement.ContainerType);
            DrawContainerBox(snap.x, snap.z, w, h, CoffeeSortPlacement.Rotation, true);
        }

        // 删除模式下鼠标悬停的容器红色描边，提示点击将删除它
        private static void DrawDeleteHover(CoffeeSortEditData data)
        {
            Vector3 mouse = GetMouseWorldRaw();
            for (int i = data.Holders.Count - 1; i >= 0; i--)
            {
                var h = data.Holders[i];
                if (!CoffeeSortPlacement.PointInBox(mouse, h.X, h.Y, h.Width, h.Height, h.DirectionValue))
                    continue;

                Quaternion rot = Quaternion.Euler(0f, h.DirectionValue, 0f);
                Vector3 center = new Vector3(h.X, 0f, h.Y);
                float halfW = h.Width * CoffeeSortPlacement.CarWidthSpacing / 2f;
                float halfD = h.Height * CoffeeSortPlacement.CarDepthSpacing / 2f;
                var corners = new[]
                {
                    center + rot * new Vector3(-halfW, 0.02f, -halfD),
                    center + rot * new Vector3(halfW, 0.02f, -halfD),
                    center + rot * new Vector3(halfW, 0.02f, halfD),
                    center + rot * new Vector3(-halfW, 0.02f, halfD),
                };
                Handles.color = new Color(1f, 0.25f, 0.25f, 0.95f);
                Handles.DrawSolidRectangleWithOutline(corners, new Color(1f, 0.25f, 0.25f, 0.25f), Handles.color);
                break;
            }
        }

        private static void DrawCarBox(Vector3 center, Color color, float dirValue, bool ghost, bool mergeHint = false)
        {
            const float w = 1.85f, d = 2.4f;
            Quaternion rot = Quaternion.Euler(0f, dirValue, 0f);
            var corners = new[]
            {
                center + rot * new Vector3(-w / 2f, 0.01f, -d / 2f),
                center + rot * new Vector3(w / 2f, 0.01f, -d / 2f),
                center + rot * new Vector3(w / 2f, 0.01f, d / 2f),
                center + rot * new Vector3(-w / 2f, 0.01f, d / 2f),
            };

            Color fill = color;
            fill.a = ghost ? 0.55f : 0.92f;
            Color outline = ghost
                ? (mergeHint ? Color.green : Color.white)
                : new Color(0f, 0f, 0f, 0.6f);
            Handles.DrawSolidRectangleWithOutline(corners, fill, outline);
        }

        private static void DrawContainerBox(float x, float z, int w, int h, float dirValue, bool ghost)
        {
            Quaternion rot = Quaternion.Euler(0f, dirValue, 0f);
            Vector3 center = new Vector3(x, 0f, z);
            float halfW = w * CoffeeSortPlacement.CarWidthSpacing / 2f;
            float halfD = h * CoffeeSortPlacement.CarDepthSpacing / 2f;

            var corners = new[]
            {
                center + rot * new Vector3(-halfW, 0.01f, -halfD),
                center + rot * new Vector3(halfW, 0.01f, -halfD),
                center + rot * new Vector3(halfW, 0.01f, halfD),
                center + rot * new Vector3(-halfW, 0.01f, halfD),
            };

            Color fill = new Color(0.3f, 0.55f, 0.95f, ghost ? 0.22f : 0.28f);
            Color outline = ghost ? Color.green : new Color(0.65f, 0.9f, 1f, 0.9f);
            Handles.DrawSolidRectangleWithOutline(corners, fill, outline);

            // 前脸标记（车出口方向）
            Vector3 front = center + rot * Vector3.forward * halfD;
            Vector3 right = rot * Vector3.right;
            Handles.color = Color.yellow;
            Handles.DrawLine(front + right * 0.5f, front - right * 0.5f);
            Handles.DrawLine(front, front + rot * Vector3.forward * 0.6f);
        }
    }
}
