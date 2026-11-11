using System;
using System.Collections;
using CasualGame;
using Custom.Activity;
using Tutorial;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace Framework
{
    /// <summary>
    /// 游戏启动与场景切换流程。
    /// </summary>
    public class GameFlow : Singleton<GameFlow>
    {
        /// <summary>当前所在场景名（对应 Constant.Scene）。</summary>
        public static string CurrentSceneName { get; private set; } = string.Empty;

        public static string PreviousSceneName { get; private set; } = string.Empty;

        /// <summary>流程是否正在跑，防止重复启动。</summary>
        public bool IsBooting { get; private set; }

        private MonoBehaviour _runner;

        /// <summary>由 GameRoot 在 Awake 里调用一次。</summary>
        public void Boot(MonoBehaviour runner)
        {
            _runner = runner;

            if (IsBooting) return;
            IsBooting = true;
            _runner.StartCoroutine(BootRoutine());
        }

        private IEnumerator BootRoutine()
        {
            Application.targetFrameRate = 60;

            // 1. 语言、音量
            LocalizationMgr.Instance.Load(SettingMgr.Instance.GetString(Constant.Setting.Language, null));
            GameRoot.Sound.SetVolume("Music", 1f);
            GameRoot.Sound.SetVolume("Sound", 1f);

            // 1.5 TMP 设置：必须在任何界面打开之前注入，否则 TMP 文本初始化不完整（文字不显示）
            TMPHelper.LoadSettingsDefault();

            // 2. 启动页
            LaunchPage.ChangeLaunchState(LaunchStatus.Launch);

            // 3. 预载
            yield return PreloadRoutine();

            // 4. 预载完成，把启动页进度条推到满，等动画播完
            LaunchPage.SetLaunchFullProcess();
            while (!LaunchPage.IsLaunchAnimationFinish())
            {
                yield return null;
            }

            // 5. 进入游戏
            GameManager.Instance.StartGame();
            LaunchPage.CloseUI();
            PlaySoundManager.Instance.Init();
            PlaySoundManager.Instance.PlayBgm();
            DateTimeHelper.Login();
            TutorialManager.Instance.InitTutorial();
            TopcoverForm.InitForm();

            CurrentSceneName = Constant.Scene.Home;

            // 6. 按进度决定进主界面还是直接进对局
            int level = ProxyManager.UserProxy.GameLevel;
            bool straightToGame = level >= GameConfigMgr.Instance.GetInt("EntraceHomeLevel", 12)
                                  && !EnergyManager.CanUseEnergy(1);

            UIMgr.Instance.OpenUIForm(straightToGame ? UIFormType.UIGameForm : UIFormType.UIMainForm);

            IsBooting = false;
        }

        /// <summary>预载配置表、资源 ID 表、预热池与玩家数据。</summary>
        private IEnumerator PreloadRoutine()
        {
            // 配置表（LuBan JSON）
            GameRoot.Config.LoadAll();

            // 全局数值配置
            GameConfigMgr.Instance.Load();

            // 资源 ID 表：音效、界面等都靠它把 id 映射成资源路径
            bool done = false;
            GameRoot.Res.LoadAssetAsync<TextAsset>("Assets/Game/Configs/ResourcesIdentification.bytes", asset =>
            {
                if (asset == null)
                {
                    Debug.LogError("[GameFlow] 资源 ID 表加载失败，音效/界面路径将无法解析。");
                }
                else
                {
                    ResourceIdentificationTool.Instance.InitResourceIdentificationInfos(asset);
                }

                done = true;
            });
            while (!done) yield return null;

            // 预热对象池
            done = false;
            GameRoot.Res.LoadAssetAsync<PrewarmPoolData>("Assets/Game/Configs/PrewarmPoolData.asset", asset =>
            {
                if (asset != null) PrewarmPoolHandler.Instance.InitPrewarmPool(asset);
                done = true;
            });
            while (!done) yield return null;

            // 玩家数据
            done = false;
            ProxyManager.Instance.InitProxy(() => done = true);
            while (!done) yield return null;

            ActivityManager.Instance.Initialize();
            ProxyManager.Instance.OnDataTableLoaded();

            Debug.Log("[GameFlow] 预载完成。");
        }

        /// <summary>切换场景并记录场景名。</summary>
        public void SwitchScene(string sceneName, Action onComplete = null)
        {
            if (CurrentSceneName == sceneName)
            {
                onComplete?.Invoke();
                return;
            }

            _runner.StartCoroutine(SwitchSceneRoutine(sceneName, onComplete));
        }

        private IEnumerator SwitchSceneRoutine(string sceneName, Action onComplete)
        {
            PreviousSceneName = CurrentSceneName;
            var handle = Addressables.LoadSceneAsync(sceneName,
                sceneName == Constant.Scene.MyGame ? LoadSceneMode.Additive : LoadSceneMode.Single);
            yield return handle;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[SceneMgr] 场景加载失败: {sceneName} - {handle.OperationException}");
                onComplete?.Invoke();
                yield break;
            }

            CurrentSceneName = sceneName;
            Resources.UnloadUnusedAssets();
            onComplete?.Invoke();
            onComplete?.Invoke();
        }
    }
}