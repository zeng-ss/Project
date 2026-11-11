using System.IO;
using System;
using System.Collections.Generic;
using GameFramework.DataTable;
using GameFramework.Event;
using UnityEngine;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace CasualGame
{
    public class ProcedureChangeScene : ProcedureBase
    {
        private string[] _loadedSceneAssetNames;
        private Action<ProcedureOwner> _loadSceneSuccessAction;
        private bool _canChangeScene;
        private const string SceneAssetPath = "Assets/Game/GameScenes/";

        public static string GetScenePath(string scene)
        {
            return SceneAssetPath + scene + ".unity";
        }

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            GameEntry.Event.Subscribe(LoadSceneSuccessEventArgs.EventId, OnLoadSceneSuccess);
            GameEntry.Event.Subscribe(LoadSceneFailureEventArgs.EventId, OnLoadSceneFailure);
            GameEntry.Event.Subscribe(LoadSceneUpdateEventArgs.EventId, OnLoadSceneUpdate);
            GameEntry.Event.Subscribe(LoadSceneDependencyAssetEventArgs.EventId, OnLoadSceneDependencyAsset);
            GameEntry.Event.Subscribe(UnloadSceneSuccessEventArgs.EventId, OnUnloadSceneSuccess);

            _canChangeScene = false;

            // 停止所有声音
            //GameEntry.Sound.StopAllLoadingSounds();
            //GameEntry.Sound.StopAllLoadedSounds();

            // 隐藏所有实体
            GameEntry.Entity.HideAllLoadingEntities();
            GameEntry.Entity.HideAllLoadedEntities();

            // 还原游戏速度
#if !UNITY_EDITOR
            GameEntry.Base.ResetNormalGameSpeed();
#endif

            string scenePath = procedureOwner.GetData<VarString>(Constant.ProcedureData.ScenePath).Value;
            _loadedSceneAssetNames = GameEntry.Scene.GetLoadedSceneAssetNames();
            _loadSceneSuccessAction = procedureOwner.GetData<VarAction<ProcedureOwner>>(Constant.ProcedureData.SceneLoadSuccessAction).Value;

            // if (scenePath.Equals(Constant.Scene.Home))
            // {
            //     UnloadUnusedAssets();
            // }
            // else
            // {
            //     scenePath = GetScenePath(scenePath);
            //     GameEntry.Scene.LoadScene(scenePath, Constant.AssetPriority.SceneAsset, this);
            // }

            UnloadUnusedAssets();
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);

            GameEntry.Event.Unsubscribe(LoadSceneSuccessEventArgs.EventId, OnLoadSceneSuccess);
            GameEntry.Event.Unsubscribe(LoadSceneFailureEventArgs.EventId, OnLoadSceneFailure);
            GameEntry.Event.Unsubscribe(LoadSceneUpdateEventArgs.EventId, OnLoadSceneUpdate);
            GameEntry.Event.Unsubscribe(LoadSceneDependencyAssetEventArgs.EventId, OnLoadSceneDependencyAsset);
            GameEntry.Event.Unsubscribe(UnloadSceneSuccessEventArgs.EventId, OnUnloadSceneSuccess);
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (_canChangeScene)
            {
                _loadSceneSuccessAction?.Invoke(procedureOwner);
                _canChangeScene = false;
            }
        }

        private void OnLoadSceneSuccess(object sender, GameEventArgs e)
        {
            LoadSceneSuccessEventArgs ne = (LoadSceneSuccessEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            Log.Info("Load scene '{0}' OK.", ne.SceneAssetName);

            UnloadUnusedAssets();
        }

        private void OnLoadSceneFailure(object sender, GameEventArgs e)
        {
            LoadSceneFailureEventArgs ne = (LoadSceneFailureEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            Log.Error("Load scene '{0}' failure, error message '{1}'.", ne.SceneAssetName, ne.ErrorMessage);
        }

        private void OnLoadSceneUpdate(object sender, GameEventArgs e)
        {
            LoadSceneUpdateEventArgs ne = (LoadSceneUpdateEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            Log.Info("Load scene '{0}' update, progress '{1}'.", ne.SceneAssetName, ne.Progress.ToString("P2"));
        }

        private void OnLoadSceneDependencyAsset(object sender, GameEventArgs e)
        {
            LoadSceneDependencyAssetEventArgs ne = (LoadSceneDependencyAssetEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            Log.Info("Load scene '{0}' dependency asset '{1}', count '{2}/{3}'.", ne.SceneAssetName, ne.DependencyAssetName, ne.LoadedCount.ToString(), ne.TotalCount.ToString());
        }

        private void OnUnloadSceneSuccess(object sender, GameEventArgs e)
        {
            UnloadSceneSuccessEventArgs ne = (UnloadSceneSuccessEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            Log.Info("UnLoad scene '{0}' OK.", ne.SceneAssetName);
        }

        private void UnloadUnusedAssets()
        {
            for (int i = 0; i < _loadedSceneAssetNames.Length; i++)
            {
                GameEntry.Scene.UnloadScene(_loadedSceneAssetNames[i]);
            }
            UnityEngine.Resources.UnloadUnusedAssets();
            _canChangeScene = true;
        }

        // private IEnumerator<float> UnloadUnusedAssets()
        // {
        //     for (int i = 0; i < _loadedSceneAssetNames.Length; i++)
        //     {
        //         GameEntry.Scene.UnloadScene(_loadedSceneAssetNames[i]);
        //     }
        //     AsyncOperation asyncOperation = UnityEngine.Resources.UnloadUnusedAssets();

        //     yield return MEC.Timing.WaitUntilTrue(() => asyncOperation.isDone);

        //     System.GC.Collect();
        //     _canChangeScene = true;
        // }
    }
}
