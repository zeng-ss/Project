using System;
using System.Collections;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// 游戏组合根：初始化资源、UI、场景、音频等管理器。
    /// 场景里挂一个 GameRoot 即可启动新框架（替代原 GameEntry + Procedure）。
    /// </summary>
    public class GameRoot : UnitySingleTonMono<GameRoot>
    {
        public static ResMgr Res { get; private set; }
        public static SoundManager Sound { get; private set; }
        public static ConfigMgr Config { get; private set; }

        /// <summary>游戏速度（替代原 GameEntry.Base.GameSpeed）。</summary>
        public float GameSpeed
        {
            get => Time.timeScale;
            set => Time.timeScale = value;
        }

        protected override void Awake()
        {
            base.Awake();
            if (IsDuplicateInstance) return;

            Res = new ResMgr();
            Sound = new SoundManager(transform);
            Config = new ConfigMgr();

            UIMgr.Instance.Init(transform);

            Debug.Log("[GameRoot] 框架初始化完成。");
        }

        /// <summary>启动游戏流程（由 ProcedureBoot 调用）。</summary>
        public void Boot()
        {
            GameFlow.Instance.Boot(this);
        }

        /// <summary>按固定间隔重复执行（替代原 GameEntry.Base.AttachTimer）。</summary>
        public void Repeat(float intervalSeconds, Action action)
        {
            StartCoroutine(RepeatRoutine(intervalSeconds, action));
        }

        private static IEnumerator RepeatRoutine(float intervalSeconds, Action action)
        {
            var wait = new WaitForSecondsRealtime(intervalSeconds);

            while (true)
            {
                yield return wait;
                action?.Invoke();
            }
        }

        private void OnDestroy()
        {
            if (IsDuplicateInstance) return;
            UIMgr.Instance.DestroyAll();
            Res?.ReleaseAll();
        }
    }
}