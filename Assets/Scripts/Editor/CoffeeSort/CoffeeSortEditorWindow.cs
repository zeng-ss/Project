using CasualGame.CoffeeAway;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CasualGame.EditorTools
{
    /// <summary>
    /// Tool/Coffee Sort 关卡编辑器。
    /// 工具栏：新建 / 载入 / 保存 / 撤销 / 视角。
    /// 容器：长按图片进入放置，Q 旋转，Scene 内点击落下。
    /// 路段：新建/切换/删除；端点赋值模式下点击设置/替换起点/终点（再点已设端点移除），可用清除按钮删除。
    /// 路径点：Scene 内按住 Cmd（Win 为 Ctrl）+ 左键点击格子交点。
    /// 道路资源：在 Data 文件（或本窗口）赋值 RoadMesh/RoadMaterial，Scene 中自动生成 SplineComputer+SplineMesh 实时预览。
    /// </summary>
    public class CoffeeSortEditorWindow : EditorWindow
    {
        private const string PrefabRoot = "Assets/RawAsset/Prefab/";
        private const float LongPressSeconds = 0.35f;

        private static readonly string[] ContainerPrefabs =
        {
            "StandardHolder.prefab", "ExpandableHolder.prefab", "ExtraHolder.prefab",
            "TunnelHolder.prefab", "OpenTunnelHolder.prefab",
        };

        private static readonly int[] ContainerTypes =
        {
            (int)HolderPrefabType.Standard, (int)HolderPrefabType.Expandable, (int)HolderPrefabType.Extra,
            (int)HolderPrefabType.Tunnel, (int)HolderPrefabType.OpenTunnel,
        };

        private static readonly string[] ContainerNames =
        {
            "标准", "扩展", "额外", "隧道", "开放隧道",
        };

        private CoffeeSortEditData _data;
        private int _levelNumber = 1;
        private int _moveOffsetX;
        private int _moveOffsetZ;
        private float _newLayerSpacing = 8f;

        // 长按状态：1..5=容器，-1=无
        private bool _pressedOnItem;
        private int _pressedIndex = -1;
        private double _pressStart;
        private bool _activated;

        private readonly Texture2D[] _containerTexs = new Texture2D[ContainerPrefabs.Length];

        [MenuItem("Tools/Coffee Sort")]
        public static void Open()
        {
            var win = GetWindow<CoffeeSortEditorWindow>("Coffee Sort");
            win.minSize = new Vector2(330f, 640f);
            win.Show();
        }

        private void OnEnable()
        {
            CoffeeSortSceneView.WindowOpen = true;
            _data = CoffeeSortEditData.GetOrCreate();
            _data.IsActive = true;
            _data.EditorSceneName = SceneManager.GetActiveScene().name;
            _data.MarkDirty();
        }

        private void OnDisable()
        {
            CoffeeSortSceneView.WindowOpen = false;
            if (_data != null)
            {
                _data.IsActive = false;
                _data.MarkDirty();
            }

            CoffeeSortPlacement.Cancel();
            CoffeeSortSceneView.DisposePreview();
        }

        private void Update()
        {
            if (_pressedOnItem && !_activated &&
                EditorApplication.timeSinceStartup - _pressStart >= LongPressSeconds)
            {
                ActivatePlacement(_pressedIndex);
            }

            // 预览图异步加载，持续刷新直到可用
            if (AnyContainerTexNull())
                Repaint();
        }

        private bool AnyContainerTexNull()
        {
            foreach (var t in _containerTexs) if (t == null) return true;
            return false;
        }

        // ---- 工具栏 ----

        private void OnGUI()
        {
            DrawToolbar();
            DrawSettings();
            DrawSegmentBar();
            DrawDeleteToggle();
            DrawSymmetryBar();
            DrawPointEditBar();
            DrawContainerPalette();
            DrawStatus();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("新建")) OnNew();
            if (GUILayout.Button("载入")) OnLoad();
            if (GUILayout.Button("保存")) OnSave();
            if (GUILayout.Button("撤销"))
            {
                if (CoffeeSortPlacement.Undo(_data))
                {
                    _data.MarkDirty();
                    CoffeeSortSceneView.UpdateRoadPreview(_data);
                }
                else
                {
                    Debug.Log("[CoffeeSort] 没有可撤销的操作");
                }
            }

            if (GUILayout.Button("视角")) CoffeeSortSceneView.FrameView(_data);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSettings()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("关卡", GUILayout.Width(30));
            _levelNumber = Mathf.Max(1, EditorGUILayout.IntField(_levelNumber, GUILayout.Width(38)));
            EditorGUILayout.LabelField("网格", GUILayout.Width(30));
            _data.GridSizeX = Mathf.Max(2, EditorGUILayout.IntField(_data.GridSizeX, GUILayout.Width(38)));
            EditorGUILayout.LabelField("x", GUILayout.Width(10));
            _data.GridSizeY = Mathf.Max(2, EditorGUILayout.IntField(_data.GridSizeY, GUILayout.Width(38)));
            EditorGUILayout.LabelField("容量", GUILayout.Width(30));
            _data.Capacity = Mathf.Max(1, EditorGUILayout.IntField(_data.Capacity, GUILayout.Width(38)));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _data.IsCloseConveyor = EditorGUILayout.Toggle("传送带", _data.IsCloseConveyor);
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(
                $"路径点 {_data.TotalPathPointCount()}  容器 {_data.Holders.Count}", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4f);
        }

        // ---- 路段 ----

        private void DrawSegmentBar()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("新建路段", GUILayout.Width(64f))) NewSegment();
            if (GUILayout.Button("◀", GUILayout.Width(28f))) PrevSegment();
            if (GUILayout.Button("▶", GUILayout.Width(28f))) NextSegment();
            if (GUILayout.Button("删除", GUILayout.Width(48f))) DeleteSegment();
            EditorGUILayout.EndHorizontal();

            int total = _data.RoadSegments.Count;
            var seg = _data.CurrentSegment;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(
                $"路段 {(total == 0 ? "-" : (_data.CurrentSegmentIndex + 1).ToString())} / {total}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                seg == null
                    ? "（空）"
                    : $"起点 {(seg.Start != null ? $"({seg.Start.Gx},{seg.Start.Gy})" : "未设")}  " +
                      $"终点 {(seg.End != null ? $"({seg.End.Gx},{seg.End.Gy})" : "未设")}  " +
                      $"路径点 {seg.PathPoints.Count}",
                EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();

            // 端点赋值模式：开启后点击场景中的点即设为当前路段起点/终点（再点已设端点移除）
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("端点赋值：", GUILayout.Width(66f));
            bool startOn = GUILayout.Toggle(CoffeeSortPlacement.SettingStart, "重新赋值起点", GUILayout.Width(104f));
            if (startOn != CoffeeSortPlacement.SettingStart) ToggleEndpoint(1);
            bool endOn = GUILayout.Toggle(CoffeeSortPlacement.SettingEnd, "重新赋值终点", GUILayout.Width(104f));
            if (endOn != CoffeeSortPlacement.SettingEnd) ToggleEndpoint(2);
            EditorGUILayout.EndHorizontal();

            // 显式删除端点
            EditorGUILayout.BeginHorizontal();
            bool hasStart = seg != null && seg.Start != null;
            bool hasEnd = seg != null && seg.End != null;
            GUI.enabled = hasStart;
            if (GUILayout.Button("清除起点", GUILayout.Width(76f)))
                CoffeeSortSceneView.ClearEndpoint(_data, true);
            GUI.enabled = hasEnd;
            if (GUILayout.Button("清除终点", GUILayout.Width(76f)))
                CoffeeSortSceneView.ClearEndpoint(_data, false);
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            // 合并当前路段与下一路段（对称生成后把两半并成一条闭环：起点接起点、终点接终点）
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("合并到下一段", GUILayout.Width(96f)))
            {
                if (CoffeeSortPlacement.MergeSegment(_data))
                {
                    _data.MarkDirty();
                    CoffeeSortSceneView.UpdateRoadPreview(_data);
                }
                else
                {
                    Debug.Log("[CoffeeSort] 路段不足两条，无法合并");
                }
            }

            EditorGUILayout.LabelField("起点接起点/终点接终点", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();

            // 整体平移当前路段：所有点往同一方向移动指定格数
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("整体移动", GUILayout.Width(56f));
            EditorGUILayout.LabelField("X", GUILayout.Width(14f));
            _moveOffsetX = EditorGUILayout.IntField(_moveOffsetX, GUILayout.Width(40f));
            EditorGUILayout.LabelField("Z", GUILayout.Width(14f));
            _moveOffsetZ = EditorGUILayout.IntField(_moveOffsetZ, GUILayout.Width(40f));
            if (GUILayout.Button("移动", GUILayout.Width(48f)))
            {
                if (CoffeeSortPlacement.MoveSegment(_data, _moveOffsetX, _moveOffsetZ))
                {
                    _data.MarkDirty();
                    CoffeeSortSceneView.UpdateRoadPreview(_data);
                }
                else
                {
                    Debug.Log("[CoffeeSort] 无当前路段或偏移为 0");
                }
            }

            EditorGUILayout.EndHorizontal();

            // 层间距重排：输入新层间距后整体竖直拉伸螺旋道路（y=0 的点锚定不动）
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("层间距", GUILayout.Width(52f));
            EditorGUILayout.LabelField($"当前 {_data.RoadLayerSpacing:F1}", EditorStyles.miniLabel, GUILayout.Width(62f));
            _newLayerSpacing = EditorGUILayout.FloatField(_newLayerSpacing, GUILayout.Width(44f));
            if (GUILayout.Button("重排高度", GUILayout.Width(72f)))
            {
                if (CoffeeSortPlacement.RescaleRoadHeight(_data, _newLayerSpacing))
                {
                    _data.MarkDirty();
                    CoffeeSortSceneView.UpdateRoadPreview(_data);
                }
                else
                {
                    Debug.Log("[CoffeeSort] 层间距未变化或无效，无法重排");
                }
            }

            EditorGUILayout.EndHorizontal();

            // 螺旋圆化：当前路段每圈铺成正圆、点均匀、高度均匀下坡/上坡（y=0 的底部环不动）
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("螺旋圆化", GUILayout.Width(72f)))
            {
                if (CoffeeSortPlacement.RoundSpiral(_data))
                {
                    _data.MarkDirty();
                    CoffeeSortSceneView.UpdateRoadPreview(_data);
                }
                else
                {
                    Debug.Log("[CoffeeSort] 当前路段点不足或没有可圆化的螺旋段");
                }
            }

            EditorGUILayout.LabelField("每圈正圆+均匀下坡（y=0不动）", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();

            // 道路资源（也可直接在 Data 文件里赋值）
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("路段Mesh", GUILayout.Width(56f));
            _data.RoadMesh = (Mesh)EditorGUILayout.ObjectField(_data.RoadMesh, typeof(Mesh), false);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("路段材质", GUILayout.Width(56f));
            _data.RoadMaterial = (Material)EditorGUILayout.ObjectField(_data.RoadMaterial, typeof(Material), false);
            EditorGUILayout.EndHorizontal();

            if (GUI.changed)
            {
                _data.MarkDirty();
                CoffeeSortSceneView.UpdateRoadPreview(_data);
            }

            EditorGUILayout.Space(4f);
        }

        private void NewSegment()
        {
            _data.RoadSegments.Add(new RoadSegmentData());
            _data.CurrentSegmentIndex = _data.RoadSegments.Count - 1;
            _data.UndoStack.Add(new UndoEntry
            {
                OpType = UndoEntry.Op.AddSegment,
                Index = _data.CurrentSegmentIndex,
            });
            CoffeeSortPlacement.ClearEndpointMode();
            _data.MarkDirty();
            CoffeeSortSceneView.UpdateRoadPreview(_data);
        }

        private void PrevSegment()
        {
            if (_data.RoadSegments.Count == 0) return;
            _data.CurrentSegmentIndex =
                (_data.CurrentSegmentIndex - 1 + _data.RoadSegments.Count) % _data.RoadSegments.Count;
            _data.MarkDirty();
            CoffeeSortSceneView.UpdateRoadPreview(_data);
        }

        private void NextSegment()
        {
            if (_data.RoadSegments.Count == 0) return;
            _data.CurrentSegmentIndex = (_data.CurrentSegmentIndex + 1) % _data.RoadSegments.Count;
            _data.MarkDirty();
            CoffeeSortSceneView.UpdateRoadPreview(_data);
        }

        private void DeleteSegment()
        {
            if (_data.RoadSegments.Count == 0) return;
            CoffeeSortPlacement.ClearPointSelection();
            _data.RoadSegments.RemoveAt(_data.CurrentSegmentIndex);
            _data.CurrentSegmentIndex = _data.RoadSegments.Count == 0
                ? -1
                : Mathf.Clamp(_data.CurrentSegmentIndex, 0, _data.RoadSegments.Count - 1);
            _data.MarkDirty();
            CoffeeSortSceneView.UpdateRoadPreview(_data);
        }

        private void ToggleEndpoint(int mode)
        {
            if (CoffeeSortPlacement.IsPlacing) CoffeeSortPlacement.Cancel();
            CoffeeSortPlacement.ToggleEndpointMode(mode);
            SceneView.RepaintAll();
        }

        // ---- 删除容器 ----

        private void DrawDeleteToggle()
        {
            bool deleting = GUILayout.Toggle(CoffeeSortPlacement.IsDeletingHolder, "删除容器（Scene 点击即删）",
                GUILayout.Width(170f));
            if (deleting != CoffeeSortPlacement.IsDeletingHolder)
            {
                if (CoffeeSortPlacement.IsPlacing || CoffeeSortPlacement.IsSettingEndpoint)
                    CoffeeSortPlacement.Cancel();
                CoffeeSortPlacement.ToggleDeleteHolderMode();
                SceneView.RepaintAll();
            }

            EditorGUILayout.Space(4f);
        }

        // ---- 对称生成 ----

        private void DrawSymmetryBar()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("对称", GUILayout.Width(36f));
            bool xOn = GUILayout.Toggle(_data.SymmetryAxisIsX, "x=", GUILayout.Width(36f));
            if (xOn != _data.SymmetryAxisIsX) _data.SymmetryAxisIsX = true;
            bool yOn = GUILayout.Toggle(!_data.SymmetryAxisIsX, "y=", GUILayout.Width(36f));
            if (yOn == _data.SymmetryAxisIsX) _data.SymmetryAxisIsX = false;

            _data.SymmetryValue = EditorGUILayout.FloatField(_data.SymmetryValue, GUILayout.Width(52f));
            if (GUILayout.Button("对称生成", GUILayout.Width(72f))) OnApplySymmetry();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("调间距：", GUILayout.Width(52f));
            if (GUILayout.Button("删除另一边", GUILayout.Width(88f))) OnRemoveOppositeSide();
            EditorGUILayout.LabelField("删镜像侧→整体移动→重新对称生成", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();

            if (GUI.changed)
            {
                _data.MarkDirty();
                SceneView.RepaintAll();
            }

            EditorGUILayout.Space(4f);
        }

        private void OnApplySymmetry()
        {
            if (CoffeeSortPlacement.ApplySymmetry(_data))
            {
                _data.MarkDirty();
                CoffeeSortSceneView.UpdateRoadPreview(_data);
            }

            SceneView.RepaintAll();
        }

        private void OnRemoveOppositeSide()
        {
            if (CoffeeSortPlacement.RemoveSymmetrySide(_data))
            {
                _data.MarkDirty();
                CoffeeSortSceneView.UpdateRoadPreview(_data);
            }
            else
            {
                Debug.Log("[CoffeeSort] 对称轴对侧没有可删除的点");
            }

            SceneView.RepaintAll();
        }

        // ---- 路径点微调 ----

        private void DrawPointEditBar()
        {
            var sel = CoffeeSortPlacement.SelectedPoint;
            if (sel == null)
            {
                EditorGUILayout.LabelField("路径点：Scene 视图左键点击路径点，选中后可用手柄拖动或下方输入框微调 xyz",
                    EditorStyles.miniLabel);
                EditorGUILayout.Space(4f);
                return;
            }

            EditorGUILayout.LabelField(
                $"路径点微调（路段 {CoffeeSortPlacement.SelectedSegmentIndex + 1}，格 {sel.Gx},{sel.Gy}）",
                EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            float wx = sel.WorldX;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("X", GUILayout.Width(16f));
            wx = EditorGUILayout.FloatField(wx);
            EditorGUILayout.EndHorizontal();

            float wy = sel.Y;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Y", GUILayout.Width(16f));
            wy = EditorGUILayout.FloatField(wy);
            EditorGUILayout.EndHorizontal();

            float wz = sel.WorldZ;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Z", GUILayout.Width(16f));
            wz = EditorGUILayout.FloatField(wz);
            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                CoffeeSortPlacement.ApplyWorldPos(sel, wx, wy, wz);
                _data.MarkDirty();
                CoffeeSortSceneView.UpdateRoadPreview(_data);
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(
                $"偏移 ({sel.OffsetX:F2},{sel.OffsetZ:F2})", EditorStyles.miniLabel);
            if (GUILayout.Button("取消选择", GUILayout.Width(72f)))
            {
                CoffeeSortPlacement.ClearPointSelection();
                SceneView.RepaintAll();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4f);
        }

        // ---- 容器 ----

        private void DrawContainerPalette()
        {
            GUILayout.Label("容器（长按图片进入放置，Q 旋转 90°）", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < ContainerPrefabs.Length; i++)
            {
                if (_containerTexs[i] == null)
                    _containerTexs[i] = AssetPreview.GetAssetPreview(LoadPrefab(ContainerPrefabs[i]));

                EditorGUILayout.BeginVertical();
                Rect r = GUILayoutUtility.GetRect(56f, 56f, GUILayout.Width(56f), GUILayout.Height(56f));
                DrawImageWithHighlight(r, _containerTexs[i], _pressedOnItem && _pressedIndex == i + 1);
                HandlePalettePress(r, i + 1);
                GUILayout.Label(ContainerNames[i], EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4f);
        }

        // ---- 状态 ----

        private void DrawStatus()
        {
            GUILayout.Space(4f);
            EditorGUILayout.HelpBox(
                "操作：\n" +
                "· 容器：长按图片 → 在 Scene 点击放置，Q 旋转，ESC/右键取消\n" +
                "· 路径点：Cmd(⌘)+点击 Scene 中的格子交点；顺序即行车方向\n" +
                "· 路径点微调：空闲时左键点击路径点选中，拖动手柄或在下方面板改 X/Y/Z\n" +
                "· 起点/终点：开启「重新赋值起点/终点」后，点击场景中的点即设为该路段端点（再点已设端点移除），或用「清除起点/终点」按钮删除；多路段用「新建路段」创建\n" +
                "· 合并路段：把当前路段与下一路段并成一条（起点接起点、终点接终点），适合对称生成后合成闭环\n" +
                "· 整体移动：设置 X/Z 偏移后点「移动」，当前路段所有点整体平移\n" +
                "· 对称：设好轴(x=/y=)与坐标后点「对称生成」镜像复制；「删除另一边」删掉镜像侧的点，配合「整体移动」把源侧拉近/拉远轴后再重新生成\n" +
                "· 层间距：输入新层间距后点「重排高度」，螺旋道路整体竖直拉伸（y=0 的点不动），如 5→8 使层高 0/5/10/15/20 变 0/8/16/24/32\n" +
                "· 螺旋圆化：把当前路段的每圈铺成正圆、点均匀分布、高度均匀下坡/上坡（y=0 的底部环不动；水平位置会调整，容器需重新对齐）\n" +
                "· 撤销：撤回上一步放置的容器/路径点/路段/端点",
                MessageType.Info);

            if (CoffeeSortPlacement.IsPlacing || CoffeeSortPlacement.IsSettingEndpoint || CoffeeSortPlacement.IsDeletingHolder)
            {
                EditorGUILayout.BeginHorizontal();
                string mode;
                if (CoffeeSortPlacement.IsDeletingHolder)
                    mode = "删除容器";
                else if (CoffeeSortPlacement.IsSettingEndpoint)
                    mode = CoffeeSortPlacement.SettingStart ? "设置起点" : "设置终点";
                else
                    mode = $"放置容器（{ContainerNames[CoffeeSortPlacement.ContainerType - 1]}）";

                EditorGUILayout.LabelField($"当前：{mode}  Q={CoffeeSortPlacement.Rotation}°", EditorStyles.boldLabel);
                if (GUILayout.Button("取消", GUILayout.Width(48f)))
                {
                    CoffeeSortPlacement.Cancel();
                    SceneView.RepaintAll();
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        // ---- 长按 ----

        private static void DrawImageWithHighlight(Rect rect, Texture2D tex, bool highlighted)
        {
            if (tex != null)
                GUI.DrawTexture(rect, tex, ScaleMode.ScaleToFit);
            else
                EditorGUI.DrawRect(rect, new Color(0.4f, 0.4f, 0.4f, 0.5f));

            if (highlighted)
            {
                float t = 2f;
                EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, t), Color.yellow);
                EditorGUI.DrawRect(new Rect(rect.x, rect.y + rect.height - t, rect.width, t), Color.yellow);
                EditorGUI.DrawRect(new Rect(rect.x, rect.y, t, rect.height), Color.yellow);
                EditorGUI.DrawRect(new Rect(rect.x + rect.width - t, rect.y, t, rect.height), Color.yellow);
            }
        }

        private void HandlePalettePress(Rect rect, int paletteIndex)
        {
            var ev = Event.current;
            if (ev.type == EventType.MouseDown && ev.button == 0 && rect.Contains(ev.mousePosition))
            {
                _pressedOnItem = true;
                _pressedIndex = paletteIndex;
                _pressStart = EditorApplication.timeSinceStartup;
                _activated = false;
                ev.Use();
            }
            else if (ev.type == EventType.MouseUp && ev.button == 0 && _pressedOnItem)
            {
                if (!_activated)
                    ActivatePlacement(_pressedIndex); // 短按同样进入放置，更顺手
                _pressedOnItem = false;
                _pressedIndex = -1;
                ev.Use();
            }
        }

        private void ActivatePlacement(int paletteIndex)
        {
            if (paletteIndex >= 1 && paletteIndex <= ContainerPrefabs.Length)
                CoffeeSortPlacement.StartContainer(ContainerTypes[paletteIndex - 1]);
            else
                return;

            _activated = true;
            _pressedOnItem = false;
            _pressedIndex = -1;
            SceneView.RepaintAll();
        }

        // ---- 动作 ----

        private void OnNew()
        {
            // 无条件新建独立的关卡编辑场景，避免停留在游戏场景（MyGame）里编辑
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            newScene.name = "CoffeeSortEditor";
            _data.EditorSceneName = "CoffeeSortEditor";

            _data.ClearContent();
            _data.IsActive = true;
            _data.MarkDirty();
            CoffeeSortPlacement.Cancel();
            CoffeeSortPlacement.ClearPointSelection();
            // 先清空旧的预览点序/网格，再在新建场景里创建道路预览组件（SplineComputer + SplineMesh）
            CoffeeSortSceneView.ResetRoadPreview();
            CoffeeSortSceneView.EnsureRoadPreviewInScene(_data);
            SceneView.RepaintAll();
            CoffeeSortSceneView.FrameView(_data);
            Debug.Log("[CoffeeSort] 已新建场景并清空内容，保留网格");
        }

        private void OnLoad()
        {
            if (CoffeeSortLevelIO.Load(_levelNumber, _data, out string msg))
            {
                _data.MarkDirty();
                SceneView.RepaintAll();
            }

            EditorUtility.DisplayDialog("Coffee Sort", msg, "OK");
        }

        private void OnSave()
        {
            CoffeeSortLevelIO.Save(_data, _levelNumber, out string msg);
            EditorUtility.DisplayDialog("Coffee Sort", msg, "OK");
        }

        private GameObject LoadPrefab(string prefabName) =>
            AssetDatabase.LoadAssetAtPath<GameObject>(PrefabRoot + prefabName);
    }
}
