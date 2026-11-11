using Cysharp.Threading.Tasks;
using Dreamteck.Splines;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace CasualGame.CoffeeAway
{
    /// <summary>
    /// CarSort 游戏主管理器
    /// </summary>
    public partial class CoffeeAwayGame : MonoBehaviour
    {
        #region SerializeField

        [SerializeField] private LevelCameraFitter m_CameraFitter;
        [SerializeField] private AttachParentPrefabPool[] m_Pools;
        [SerializeField] private SplineComputer m_Computer;
        [SerializeField] private SplineMesh m_SplineMesh;
        [SerializeField] private CoffeeMechanismUnlockConfig m_CoffeeMechanismUnlockConfig;

        [SerializeField] private float carRoadSpeed = 18f;
        [SerializeField] private float roadSurfaceY = 0.15f; // 贴在路面上方的高度

        #endregion

        #region private

        private const float roadSurfaceSpacing = 20f; // 中心线上每隔多少距离放一个
        private LevelData _levelData;
        private LevelBuilder.PathInfo _pathInfo;
        private RoadNavigator _roadNav;
        private List<Vector3> _roadCenterline; // 道路中心线采样点，仅用于 Scene 视图调试可视化
        private readonly CoffeeAwayCore _core = new();
        private RoadView _roadView;
        private PropMgr _propMgr;
        private ReceiptPrinter _receiptPrinter;
        private readonly List<RoadSurfaceMove> _roadSurfaces = new();
        private readonly List<Entrance> _entrances = new();

        // 调试用：打乱容器索引
        private int _debugHolderIndex;

        private bool _isLoading;

        #endregion

        #region public

        public bool IsLevelComplete { get; private set; }
        public CoffeeAwayCore Core => _core;
        public PropMgr PropMgr => _propMgr;
        public bool IsBackShouldReduceEnergy { get; private set; }
        public static CoffeeAwayGame Instance { get; private set; }

        #endregion

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            EnergyManager.UseEnergy(1);
            _roadView = m_SplineMesh.gameObject.GetComponent<RoadView>();
            SpawnFactory.Init(m_Pools);
            _propMgr = new PropMgr(_core);
            CoffeeMechanismUnlockRegistry.Initialize(m_CoffeeMechanismUnlockConfig);
            InitLevel(ProxyManager.UserProxy.GameLevel + 1).Forget();
        }

        private void Win()
        {
            Framework.UIMgr.Instance.OpenUIForm(UIFormType.WinUIForm);
            if (ProxyManager.UserProxy.GameLevel >= 89) // 循环
            {
                int level = Random.Range(10, 91);
                while (level == 70)
                {
                    level = Random.Range(10, 91);
                }

                ProxyManager.UserProxy.SetGameLevel(level);
            }
            else ProxyManager.UserProxy.AddGameLevel();
        }

        // 输入处理
        private void HandleInput()
        {
            if (_core.IsAutoCompleting || _core.IsShowingExtraHolder) return;
            if (Input.GetMouseButtonDown(0) && !Framework.UIMgr.Instance.HasDialogUI())
            {
                if (EventSystem.current is not null && EventSystem.current.IsPointerOverGameObject()) return;
                Ray ray = Camera.main!.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out var hit, 200f))
                {
                    var holder = hit.collider.GetComponent<Holder>();
                    if (holder is null) return;
                    // 首关指引期间只有高亮的目标容器可点击，其它容器一律忽略
                    if (ProxyManager.UserProxy.GameLevel == 0 && _guideTarget != holder) return;
                    TryCompleteFirstLevelGuide(holder);
                    if (_propMgr.TrySelectHolder(holder)) return;

                    _core.OnHolderTapped(holder, red => _roadView.SetRoadColor(red));
                }
            }
        }

        public void ShouldBackReduceEnergy() => IsBackShouldReduceEnergy = true;

        public void RefitCamera() => m_CameraFitter?.Refit(_pathInfo, _core.Holders, _receiptPrinter);

        #region Settlement

        // Level initialize
        private async UniTask InitLevel(int level)
        {
            IsBackShouldReduceEnergy = false;
            _isLoading = true;
            IsLevelComplete = false;
            _roadView?.SetRoadColor(false);
            FindFirstObjectByType<UGuiScreenMaskHelper>()?.gameObject.SetActive(false);
            _levelData = await CoffeeAwayHelper.LoadLevel(level);
            if (_levelData == null)
            {
                Log.Error($"[CarSortGame] Failed to load level {level}");
                _isLoading = false;
                return;
            }

            _pathInfo = LevelBuilder.BuildPathInfo(_levelData);
            var holderInfos = LevelBuilder.Build(_levelData);

            // Build Road
            _roadCenterline = RoadMeshBuilder.BuildRoadFromExistingSpline(_pathInfo, m_Computer, m_SplineMesh);

            if (_roadNav == null) _roadNav = new RoadNavigator(_roadCenterline, _levelData.Capacity);
            else _roadNav.Rebuild(_roadCenterline, _levelData.Capacity);

            _roadSurfaces.AddRange(
                SpawnFactory.SpawnRoadSurfaces(_roadNav, transform, roadSurfaceSpacing, roadSurfaceY));

            // Build Holder && Vehicle
            var holders = CoffeeAwayHelper.SpawnHolders(holderInfos, _roadNav, transform);
            var vehicles = CoffeeAwayHelper.SpawnVehicles(holderInfos, holders);
            var entrances = CoffeeAwayHelper.SpawnEntrances(_levelData, _roadNav, transform);
            _entrances.AddRange(entrances);
            _receiptPrinter =
                CoffeeAwayHelper.SpawnReceiptPrinter(_levelData, transform, _roadCenterline, holders, entrances);
            CoffeeAwayHelper.SetupCore(_core, holders, vehicles, _roadSurfaces, _receiptPrinter, _roadNav,
                _levelData.Capacity, carRoadSpeed, _levelData.IsCloseConveyor ?? false);
            // 容器与道路的连接路段共用道路材质实例，颜色同步变化
            foreach (var holder in holders) _roadView?.RegisterConnectionRenderer(holder.RoadView);

            m_CameraFitter?.Fit(_pathInfo, holders, _receiptPrinter);
            _isLoading = false;

            if (TryShowSpecialFeatureUnlock(level, out ItemType unlockType))
            {
                UGUIParams uiParams = UGUIParams.Create();
                uiParams.AddValue("unlockType", unlockType);
                Framework.UIMgr.Instance.OpenUIForm(UIFormType.NewItemDialog, uiParams);
            }

            await UniTask.WaitUntil(() => Framework.UIMgr.Instance.GetUIForm<UIGameForm>(UIFormType.UIGameForm) != null);
            var form = Framework.UIMgr.Instance.GetUIForm<UIGameForm>(UIFormType.UIGameForm);
            form.UpdateVisual();
            form.SetBtnShow(level != 1);
            TryStartFirstLevelGuide();

            Log.Info(
                $"[CarSortGame] Level {level} initialized. Holders: {holders.Count}, Vehicles: {vehicles.Count}, Road: {_roadNav.TotalLength:F1}u");
        }

        private bool TryShowSpecialFeatureUnlock(int level, out ItemType unlockType)
        {
            unlockType = ItemType.None;
            if (CoffeeMechanismUnlockRegistry.Config == null)
            {
                Log.Error("[CoffeeMechanismUnlockRegistry] CoffeeMechanismUnlockConfig 未绑定，无法读取机制首次出现关卡。");
                return false;
            }

            IReadOnlyList<CoffeeMechanismUnlockConfig.Entry> entries = CoffeeMechanismUnlockRegistry.Config.Entries;
            foreach (var entry in entries)
            {
                if (entry.FirstProgressLevel != level) continue;
                ItemType candidate = entry.Mechanism;
                if (ProxyManager.UserProxy.IsBoosterUnlocked((int)candidate)) continue;
                unlockType = candidate;
                return true;
            }

            return false;
        }

        private void Clear()
        {
            ResetGuide();
            _propMgr?.Reset();
            foreach (var surface in _roadSurfaces) SpawnFactory.HolderPool.DeSpawn(surface);
            _roadSurfaces.Clear();
            foreach (var entrance in _entrances) SpawnFactory.HolderPool.DeSpawn(entrance);
            _entrances.Clear();
            foreach (var vehicle in _core.Vehicles) SpawnFactory.VehiclePool.DeSpawn(vehicle);
            foreach (var holder in _core.Holders) holder.DeSpawnToSourcePool();
            if (_receiptPrinter is not null)
            {
                SpawnFactory.HolderPool.DeSpawn(_receiptPrinter);
                _receiptPrinter = null;
            }

            _core.Reset();
        }

        public void Restart()
        {
            Clear();
            InitLevel(ProxyManager.UserProxy.GameLevel + 1).Forget();
        }

        #endregion
    }
}