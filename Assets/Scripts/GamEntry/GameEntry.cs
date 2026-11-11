using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework;
using GameFramework.Resource;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace CasualGame
{
    /// <summary>
    /// 游戏入口。
    /// </summary>
    public partial class GameEntry : MonoBehaviour
    {
        [SerializeField] private bool useYooAsset;

        private void Start()
        {
            InitBuiltinComponents();

            InitResourceManager();

            IMGUIDebugger.DebuggerComponent.SetCommandDatas(DebugHandler.Instance.CommandDatas);
            IMGUIDebugger.DebuggerComponent.SetInfoDatas(DebugHandler.Instance.InfoDatas);
            IMGUIDebugger.DebuggerComponent.SetFullWindowAction(() =>
            {
                EventSystemTool.DisableEventSystem();
            }, () =>
            {
                EventSystemTool.EnableEventSystem();
            });
            DebugHandler.SetGMButton(DebugHandler.EnableDebug);
        }

        private void InitResourceManager()
        {
            BaseComponent baseComponent = Base;

            if (useYooAsset)
            {
                baseComponent.SetResourceManager(new YooAssetManager());
            }
            else
            {
                if (baseComponent.EditorResourceMode)
                {
                    Log.Info("During this run, Game Framework will use editor resource files, which you should validate first.");

                    baseComponent.SetResourceManager(new EditorResourceManager()
                    {
                        m_EnableCachedAssets = true,
                        m_LoadAssetCountPerFrame = 1,
                        m_MinLoadAssetRandomDelaySeconds = 0.05f,
                        m_MaxLoadAssetRandomDelaySeconds = 0.3f,
                    });
                }
                else
                {
                    baseComponent.SetResourceManager(GameFrameworkEntry.GetModule<IResourceManager>());
                }
            }
        }
    }
}