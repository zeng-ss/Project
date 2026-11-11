//------------------------------------------------------------
// File : LaunchPage.cs
// Email: yang.li@kingboat.io
// Desc : 
//------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameFramework;
using UnityEngine.Video;

namespace CasualGame
{
    public enum LaunchStatus
    {
        None,
        Launch,
        CheckVersion,
        HotUpdatable,
    }
    public class LaunchPage : UGuiForm
    {
        public static LaunchPage launchPage;
        public const string AssetPath = "Assets/Game/Prefabs/LaunchPage/LaunchPage#10000.prefab";
        [SerializeField] private RectMask2D m_Process;
        [SerializeField] private Image background;
        [SerializeField] private RectTransform m_loadingIcon;
        [SerializeField] private Button m_DebugButton;
        [SerializeField] private TextMeshProUGUI m_LoadingText;
        [HideInInspector] private bool m_IsFullProcess = false;
        public bool IsAnimationFinish => m_process >= maxProcess;
        public bool IsLaunchTimeout => timer >= launchTimeoutDuration;
        public bool IsPause;
        private float m_process = 0;
        private const float maxProcess = 1f;
        private const float stopProcess = 0.8f;
        private const float launchProgressDuration = 4f;
#if UNITY_EDITOR
        private const float launchTimeoutDuration = 0.6f;
#else
        private const float launchTimeoutDuration = 6f;
#endif
        private float timer = 0f;
        private LaunchStatus status;
        private float updatingProcess = 0f;

        void Awake()
        {
            background.gameObject.SetActive(true);
        }

        private float MaskWidth => (m_Process.rectTransform.parent as RectTransform).rect.width;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            m_loadingIcon.anchoredPosition = new Vector3(0f, 0f);

            m_DebugButton.onClick.RemoveAllListeners();
            m_DebugButton.onClick.AddListener(() =>
            {
                DebugHandler.TriggerDebugger();
            });
        }

        private void Update()
        {
            if (IsPause) return;

            if (m_IsFullProcess)
            {
                if (m_process >= maxProcess)
                {
                    return;
                }
                float delta = Time.deltaTime * 0.8f;
                SetProcess(delta);
            }
            else
            {
                switch (status)
                {
                    case LaunchStatus.Launch:
                        UpdateLaunch();
                        break;
                    case LaunchStatus.CheckVersion:
                        UpdateCheckVersion();
                        break;
                    case LaunchStatus.HotUpdatable:
                        UpdateDownloadModule();
                        break;
                }
            }
        }

        private void UpdateLaunch()
        {
            timer += Time.deltaTime;
            if (m_process > stopProcess)
            {
                return;
            }
            float delta = (stopProcess / launchProgressDuration) * Time.deltaTime;
            SetProcess(delta);
        }

        private void UpdateCheckVersion()
        {
            if (m_process > stopProcess)
            {
                return;
            }
            float delta = (stopProcess / launchProgressDuration) * Time.deltaTime;
            SetProcess(delta);
        }

        private void UpdateDownloadModule()
        {
        }

        private void ChangeState(LaunchStatus status)
        {
            IsPause = false;
            this.status = status;
            SetMaskRight(MaskWidth);
            m_loadingIcon.anchoredPosition = new Vector3(0f, 0f);
            m_process = timer = 0f;
            m_IsFullProcess = false;
            // loadingTxt.text = string.Empty;
            updatingProcess = 0f;
            switch (status)
            {
                case LaunchStatus.Launch:
                    // loadingTxt.text = "Loading...";
                    break;
                case LaunchStatus.CheckVersion:
                    // loadingTxt.text = "Checking...";
                    break;
                case LaunchStatus.HotUpdatable:
                    // loadingTxt.text = "";
                    break;
            }
        }

        private void ResetProcess()
        {
            IsPause = false;
            SetMaskRight(MaskWidth);
            m_process = timer = 0f;
            m_loadingIcon.anchoredPosition = new Vector3(0f, 0f);
        }

        private void SetProcess(float delta)
        {
            m_process += delta;
            m_process = Mathf.Clamp01(m_process);
            SetMaskRight(MaskWidth * (1f - m_process));
            m_loadingIcon.anchoredPosition = new Vector3(m_process * MaskWidth - 40f, 0f);
        }

        private void SetMaskRight(float right)
        {
            var p = m_Process.padding;
            m_Process.padding = new Vector4(p.x, p.y, right, p.w);
        }

        private void UpdateProcess(float delta, string desc)
        {
            m_process = Mathf.Clamp01(delta);
            SetMaskRight(MaskWidth * (1f - m_process));
            m_loadingIcon.anchoredPosition = new Vector3(m_process * MaskWidth - 40f, 0f);
            // loadingTxt.text = desc;
        }

        public static void ChangeLaunchState(LaunchStatus status)
        {
            if (launchPage == null)
            {
                launchPage = GameEntry.UI.OpenUIInResources<LaunchPage>("Prefabs/LaunchPage", Constant.UIGroup.Loading, null);
                launchPage.ChangeState(status);
            }
            else
            {
                launchPage.ChangeState(status);
            }
        }

        public static void CloseUI()
        {
            if (launchPage != null)
            {
                UnityEngine.Object.Destroy(launchPage.gameObject);
                launchPage = null;
            }
        }

        public static void SetLaunchFullProcess()
        {
            if (launchPage != null)
            {
                launchPage.m_IsFullProcess = true;
            }
        }

        public static void UpdateResourceProcess(float progress, string desc)
        {
            if (launchPage != null)
            {
                launchPage.UpdateProcess(progress, desc);
            }
        }

        public static void PauseLaunchProcess()
        {
            if (launchPage != null)
            {
                launchPage.IsPause = true;
            }
        }

        public static void ResetLaunchProcess()
        {
            if (launchPage != null)
            {
                launchPage.ResetProcess();
            }
        }

        public static bool IsLaunchAnimationFinish()
        {
            if (launchPage != null)
            {
                return launchPage.IsAnimationFinish;
            }
            return false;
        }

        public static bool IsLaunchTimedOut()
        {
            return launchPage != null && launchPage.IsLaunchTimeout;
        }

        public static LaunchStatus GetLaunchStatus()
        {
            if (launchPage != null)
            {
                return launchPage.status;
            }
            return LaunchStatus.None;
        }
    }
}
