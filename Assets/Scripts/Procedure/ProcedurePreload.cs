using System;
using System.Threading;
using Custom.Activity;
using Cysharp.Threading.Tasks;
using GameFramework.Event;
using GameFramework.Resource;
using Tutorial;
using UnityEngine;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;
using PuzzleTrack;

namespace CasualGame
{
    [OPS.Obfuscator.Attribute.DoNotObfuscateClass]
    public class ProcedurePreload : ProcedureBase
    {
        private int _totalResourceCount;
        private int _currentResourceCount;
        private bool isInitializedTA = false;
        private bool isReadyPreload = false;
        private bool IsFirebaseFetchCompleted => RemoteConfigManager.Instance.IsFirebaseFetchCompleted;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            GameEntry.Event.Subscribe(LoadConfigSuccessEventArgs.EventId, OnLoadConfigSuccess);
            GameEntry.Event.Subscribe(LoadConfigFailureEventArgs.EventId, OnLoadConfigFailure);
            GameEntry.Event.Subscribe(LoadDataTableSuccessEventArgs.EventId, OnLoadDataTableSuccess);
            GameEntry.Event.Subscribe(LoadDataTableFailureEventArgs.EventId, OnLoadDataTableFailure);
            GameEntry.Event.Subscribe(LoadDictionarySuccessEventArgs.EventId, OnLoadDictionarySuccess);
            GameEntry.Event.Subscribe(LoadDictionaryFailureEventArgs.EventId, OnLoadDictionaryFailure);

            _totalResourceCount = 0;
            _currentResourceCount = 0;

            InitializeTA();

            LoadServerTime();

            LoadDataTable();

            LoadGlobalConfig();

            LoadLocalization();

            LoadResourceIdentification();

            LoadTMPSetting();

            InitializeProxy();
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);

            GameEntry.Event.Unsubscribe(LoadConfigSuccessEventArgs.EventId, OnLoadConfigSuccess);
            GameEntry.Event.Unsubscribe(LoadConfigFailureEventArgs.EventId, OnLoadConfigFailure);
            GameEntry.Event.Unsubscribe(LoadDataTableSuccessEventArgs.EventId, OnLoadDataTableSuccess);
            GameEntry.Event.Unsubscribe(LoadDataTableFailureEventArgs.EventId, OnLoadDataTableFailure);
            GameEntry.Event.Unsubscribe(LoadDictionarySuccessEventArgs.EventId, OnLoadDictionarySuccess);
            GameEntry.Event.Unsubscribe(LoadDictionaryFailureEventArgs.EventId, OnLoadDictionaryFailure);

            // 初始化
            HorstRillSdk.InitSDK();
            // 开启全局广告加载优化
            var failToast = LocalizationUtils.GetString(
                276, "Video is currently unavailable. Please try again later.");
            HorstRillSdk.Ads.EnableWaitForRetry(true, 8f,
                showLoading: () => UIManager.ShowLoading(),
                closeLoading: () => UIManager.CloseLoading(),
                showFailToast: () => UIManager.Toast(failToast));
#if ENABLE_SDK
            string countryCode = McSdk.GetSdkConfiguration().CountryCode;
            if (!string.IsNullOrEmpty(countryCode))
                ProxyManager.UserProxy.SetCountryCode(countryCode);
#endif

            IAPManager.Instance.Initilize();
            PlaySoundManager.Instance.Init();
            DateTimeHelper.Login();
            TutorialManager.Instance.InitTutorial();
#if UNITY_ANDROID
            // NotificationManager.Instance.Init();
#endif
            TopcoverForm.InitForm();
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (_totalResourceCount != -1 && _currentResourceCount >= _totalResourceCount)
            {
                ActivityManager.Instance.Initialize();
                ProxyManager.Instance.OnDataTableLoaded();
                _totalResourceCount = -1;
                isReadyPreload = true;
            }

            if (isReadyPreload && (IsFirebaseFetchCompleted || LaunchPage.IsLaunchTimedOut()))
            {
                isReadyPreload = false;
                LaunchPage.SetLaunchFullProcess();
            }

            if (LaunchPage.IsLaunchAnimationFinish())
            {
                // 进入主场景
                GameManager.Instance.StartGame();
                VarAction<ProcedureOwner> varAction = new VarAction<ProcedureOwner>();
                varAction.Value = (owner) => { };
                procedureOwner.SetData(Constant.ProcedureData.SwitchProcedureSuccessAction, varAction);
                procedureOwner.SetData<VarBoolean>(Constant.ProcedureData.ColdBoot, true);

                int curLevel = ProxyManager.UserProxy.GameLevel;
                if (curLevel >= GameEntry.Config.GetInt("EntraceHomeLevel", 12) && !EnergyManager.CanUseEnergy(1))
                {
                    SwitchProcedure<ProcedureMainGame>();
                }
                else
                {
                    SwitchProcedure<ProcedureCoffeeGame>();
                }
            }
        }

        private async void LoadServerTime()
        {
            GameManager.Instance.SetServerTime(0);
            // var timeData = await WebManager.Instance.GetServerTime();
            // if (timeData != null)
            //     GameManager.Instance.SetServerTime(timeData.timestamp);
        }

        private void LoadGlobalConfig()
        {
            _totalResourceCount++;

            string globalConfigAssetPath = $"Assets/Game/Configs/GlobalConfig.csv";
            GameEntry.Config.ReadData(globalConfigAssetPath, this);
        }

        private void LoadDataTable()
        {
            // 输出结果
            foreach (DataTableType dataTableType in Enum.GetValues(typeof(DataTableType)))
            {
                string dataTableName = dataTableType.ToString();
                string dataTableAssetPath = $"Assets/Game/DataTables/{dataTableName}.bytes";

                HasAssetResult hasAssetResult = GameEntry.Resource.HasAsset(dataTableAssetPath);

                if (hasAssetResult != HasAssetResult.NotExist)
                {
                    _totalResourceCount++;
                    GameEntry.DataTable.LoadDataTable(dataTableName, dataTableAssetPath, this);
                }
            }
        }

        private void LoadLocalization()
        {
            _totalResourceCount++;

            string locationAssetPath = $"Assets/Game/Localization/uiText_{GameEntry.Localization.Language.ToString()}.csv";
            GameEntry.Localization.ReadData(locationAssetPath, this);
        }

        private void LoadResourceIdentification()
        {
            _totalResourceCount += 2;

            GameEntry.Resource.LoadAsset("Assets/Game/Configs/ResourcesIdentification.bytes", typeof(TextAsset),
                new LoadAssetCallbacks(LoadAssetSuccessCallback));
        }

        private void LoadAssetSuccessCallback(string assetName, object asset, float duration, object userData)
        {
            ResourceIdentificationTool.Instance.InitResourceIdentificationInfos((TextAsset)asset);

            AddCurrentResourceCount();

            LoadPrewarmPool();

            GameEntry.Resource.UnloadAsset(asset);
        }

        private void LoadPrewarmPool()
        {
            GameEntry.Resource.LoadAsset("Assets/Game/Configs/PrewarmPoolData.asset", typeof(PrewarmPoolData),
                new LoadAssetCallbacks(LoadPrewarmPoolSuccessCallback));
        }

        private void LoadPrewarmPoolSuccessCallback(string assetName, object asset, float duration, object userData)
        {
            PrewarmPoolHandler.Instance.InitPrewarmPool((PrewarmPoolData)asset);

            AddCurrentResourceCount();

            GameEntry.Resource.UnloadAsset(asset);
        }

        private void LoadTMPSetting()
        {
            _totalResourceCount++;
            TMPHelper.LoadSetting(() => AddCurrentResourceCount()).Forget();
        }

        private void AddCurrentResourceCount(int count = 1)
        {
            _currentResourceCount += count;
        }

        private void OnLoadConfigSuccess(object sender, GameEventArgs e)
        {
            LoadConfigSuccessEventArgs ne = (LoadConfigSuccessEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            AddCurrentResourceCount();
        }

        private void OnLoadConfigFailure(object sender, GameEventArgs e)
        {
            LoadConfigFailureEventArgs ne = (LoadConfigFailureEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            Log.Error("Can not load config '{0}' from '{1}' with error message '{2}'.", ne.ConfigAssetName,
                ne.ConfigAssetName, ne.ErrorMessage);
        }

        private void OnLoadDataTableSuccess(object sender, GameEventArgs e)
        {
            LoadDataTableSuccessEventArgs ne = (LoadDataTableSuccessEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            AddCurrentResourceCount();
        }

        private void OnLoadDataTableFailure(object sender, GameEventArgs e)
        {
            LoadDataTableFailureEventArgs ne = (LoadDataTableFailureEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            Log.Error("Can not load data table '{0}' from '{1}' with error message '{2}'.", ne.DataTableAssetName,
                ne.DataTableAssetName, ne.ErrorMessage);
        }

        private void OnLoadDictionarySuccess(object sender, GameEventArgs e)
        {
            LoadDictionarySuccessEventArgs ne = (LoadDictionarySuccessEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            AddCurrentResourceCount();
        }

        private void OnLoadDictionaryFailure(object sender, GameEventArgs e)
        {
            LoadDictionaryFailureEventArgs ne = (LoadDictionaryFailureEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            Log.Error("Can not load dictionary '{0}' from '{1}' with error message '{2}'.", ne.DictionaryAssetName,
                ne.DictionaryAssetName, ne.ErrorMessage);
        }

        private void InitializeProxy()
        {
            _totalResourceCount += 1;
            ProxyManager.Instance.InitProxy(() =>
            {
                _totalResourceCount--;
                // WebManager.Instance.Activate();
            });
        }

        private void InitializeTA()
        {
            if (isInitializedTA) return;
            isInitializedTA = true;

            HorstRillSdk.InitAnalytics((props) =>
            {
            });
        }
    }
}
