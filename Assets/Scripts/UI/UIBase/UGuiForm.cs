using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;
using GameFramework;
using System.Threading;

namespace CasualGame
{
    [RequireComponent(typeof(UIConfigMono))]
    public abstract class UGuiForm : UIFormLogic, IShowMaskInterface
    {
        private readonly List<IUGUIChild> _uguiChildren = new List<IUGUIChild>();
        public const int DepthFactor = 100;
        private UIFormType m_formType = UIFormType.Undefined;

        protected Canvas m_CachedCanvas = null;
        protected CanvasGroup m_CanvasGroup = null;
        private List<Canvas> m_CachedCanvasContainer = new List<Canvas>();
        private RectTransform m_rect;

        public RectTransform Rect => m_rect;

        protected UIConfigMono m_Config;
        protected UGUIParams m_Params;
        protected DRUIForm drUIForm;
        protected Transform panel;
        /// <summary>
        /// 用于取消异步任务，onclose时自动判定取消
        /// </summary>
        protected CancellationTokenSource _cancellationTokenSource;
        protected AtlasLoader _atlasLoader = new AtlasLoader();

        [Header("展示黑色遮罩")][SerializeField] public bool showMask = true;
        [Header("展示资源栏")][SerializeField] public bool showCurrency = false;
        public bool ShowMask => showMask;
        public bool ShowCurrency => showCurrency;
        public int MaskDepth => m_CachedCanvas.sortingOrder;

        public UIFormType UIType
        {
            get
            {
                if (m_formType == UIFormType.Undefined)
                {
                    int lastIndexOf = UIForm.UIFormAssetName.LastIndexOf('#');
                    int lastIndexOfDot = UIForm.UIFormAssetName.LastIndexOf('.');
                    if (lastIndexOfDot == -1)
                    {
                        lastIndexOfDot = UIForm.UIFormAssetName.Length;
                    }

                    int assetId = Utility.Parse.parseInt(
                        UIForm.UIFormAssetName.Substring(lastIndexOf + 1, lastIndexOfDot - lastIndexOf - 1), 0);
                    m_formType = (UIFormType)assetId;
                }

                return m_formType;
            }
        }

        public virtual bool UIAnimationFinish { get; set; }

        public int OriginalDepth { get; private set; }

        public int Depth
        {
            get { return m_CachedCanvas.sortingOrder; }
        }

        private void OnDestroy()
        {
            _atlasLoader?.Dispose();
        }

        public virtual void Close()
        {
            GameEntry.UI.CloseUIForm(this);
        }

        public void PlayUISound(int uiSoundId)
        {
            PlaySoundManager.Instance.PlaySound(uiSoundId);
        }

        public void OpenUIForm(UIFormType formType)
        {
            GameEntry.UI.OpenUIForm(formType);
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnInit(object userData)
#else
        protected internal override void OnInit(object userData)
#endif
        {
            base.OnInit(userData);

            m_Config = gameObject.GetComponent<UIConfigMono>();
            if (m_Config == null)
            {
                Log.Error("UI组建未挂载UIConfigMono，请检查");
                return;
            }

            m_Config.Init();

            m_CachedCanvas = gameObject.GetOrAddComponent<Canvas>();
            m_CachedCanvas.overrideSorting = true;
            OriginalDepth = m_CachedCanvas.sortingOrder;

            RectTransform transform = GetComponent<RectTransform>();
            transform.anchorMin = Vector2.zero;
            transform.anchorMax = Vector2.one;
            transform.anchoredPosition = Vector2.zero;
            transform.sizeDelta = Vector2.zero;
            m_rect = transform;

            gameObject.GetOrAddComponent<GraphicRaycaster>();

            if (UIComponentExtend.dtUIForm != null)
                drUIForm = UIComponentExtend.dtUIForm.GetDataRow((int)UIType);

            GetComponentsInChildren(true, _uguiChildren);

            foreach (var iChild in _uguiChildren)
            {
                iChild.OnInit();
            }

            if (panel == null)
            {
                AspectSafeAreaHelper aspectSafeAreaHelper = transform.GetComponentInChildren<AspectSafeAreaHelper>();
                if (aspectSafeAreaHelper)
                    panel = aspectSafeAreaHelper.transform;
            }

            if (panel != null)
                m_CanvasGroup = panel.gameObject.GetOrAddComponent<CanvasGroup>();
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnRecycle()
#else
        protected internal override void OnRecycle()
#endif
        {
            base.OnRecycle();
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnOpen(object userData)
#else
        protected internal override void OnOpen(object userData)
#endif
        {
            base.OnOpen(userData);
            _cancellationTokenSource = new CancellationTokenSource();
            UIAnimationFinish = true;

            if (userData is UGUIParams) m_Params = (UGUIParams)userData;
            StopAllCoroutines();

            OnOpenCallback();

            foreach (var iChild in _uguiChildren)
            {
                iChild.OnOpen();
            }

            AspectSafeAreaHelper[] aspectSafeAreaHelper = transform.GetComponentsInChildren<AspectSafeAreaHelper>();
            foreach (var item in aspectSafeAreaHelper)
            {
                item.Refresh();
            }
        }

        protected virtual void OnOpenCallback()
        {
            if (m_Params != null)
                m_Params.OnOpenCallback?.Invoke(this);
        }


#if UNITY_2017_3_OR_NEWER
        protected override void OnClose(bool isShutdown, object userData)
#else
        protected internal override void OnClose(bool isShutdown, object userData)
#endif
        {
            base.OnClose(isShutdown, userData);
            UIAnimationFinish = false;

            if (m_Params != null)
            {
                m_Params.OnCloseCallback?.Invoke();
                ReferencePool.Release(m_Params);
                m_Params = null;
            }

            if (GameEntry.UI.CacheUIInfo.Count > 0)
            {
                UIComponent.SingleUIInfo info = GameEntry.UI.CacheUIInfo[0];
                if (GameEntry.UI.HasMutexUI())
                {
                    return;
                }

                GameEntry.UI.OpenUIForm((UIFormType)info.uiType, info.userData, false);
                GameEntry.UI.CacheUIInfo.RemoveAt(0);
            }

            foreach (var iChild in _uguiChildren)
            {
                iChild.OnClose();
            }

            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }

            _atlasLoader?.Cancel();
            UGuiScreenMaskHelper.CloseUI(this);
            CasualCurrencyGroup.CloseUI(this);
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnPause()
#else
        protected internal override void OnPause()
#endif
        {
            base.OnPause();
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnResume()
#else
        protected internal override void OnResume()
#endif
        {
            base.OnResume();

        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnCover()
#else
        protected internal override void OnCover()
#endif
        {
            base.OnCover();
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnReveal()
#else
        protected internal override void OnReveal()
#endif
        {
            base.OnReveal();
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnRefocus(object userData)
#else
        protected internal override void OnRefocus(object userData)
#endif
        {
            base.OnRefocus(userData);
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
#else
        protected internal override void OnUpdate(float elapseSeconds, float realElapseSeconds)
#endif
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            foreach (var iChild in _uguiChildren)
            {
                iChild.OnUpdate(elapseSeconds, realElapseSeconds);
            }
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
#else
        protected internal override void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
#endif
        {
            int oldDepth = Depth;
            base.OnDepthChanged(uiGroupDepth, depthInUIGroup);
            int deltaDepth = UGuiGroupHelper.DepthFactor * uiGroupDepth + DepthFactor * depthInUIGroup - oldDepth +
                             OriginalDepth;
            GetComponentsInChildren(true, m_CachedCanvasContainer);
            for (int i = 0; i < m_CachedCanvasContainer.Count; i++)
            {
                m_CachedCanvasContainer[i].sortingOrder += deltaDepth;
            }

            foreach (var uidepth in GetComponentsInChildren<UIDepth>())
            {
                uidepth.Refresh();
            }

            foreach (var uidepth in GetComponentsInChildren<UIEffectDepth>())
            {
                uidepth.Refresh();
            }

            m_CachedCanvasContainer.Clear();

            UGuiScreenMaskHelper.OnDepthChanged(this);
            CasualCurrencyGroup.OnDepthChanged(this);
        }

        #region UIConfigMono

        public T GetRef<T>(string key) where T : UnityEngine.Object
        {
            if (m_Config == null)
            {
                Log.Error("{0} {1} null point", gameObject.name, key);
                return null;
            }

            var bind = m_Config.GetBindEntityByTag(key);
            if (bind == null)
            {
                Log.Error("{0} {1} key not find", gameObject.name, key);
                return null;
            }

            if (typeof(T) == typeof(GameObject))
            {
                return (bind.obj) as T;
            }
            else if (typeof(T) == typeof(Transform))
            {
                return (bind.obj.transform) as T;
            }

            return bind.obj.GetComponent<T>();
        }


        public void RegisterObjectEvent(string key, GameObject obj)
        {
            if (obj.tag == "Unclick") return;
            var btn = obj.GetComponent<Button>();
            if (btn != null)
            {
                btn.gameObject.GetOrAddComponent<UIButtonScale>();
                btn.onClick.AddListener(() =>
                {
                    if (m_Params != null) m_Params.OnClickCallback?.Invoke(key);
                    gameObject.SendMessage(Utility.Text.Format("OnClick_{0}", key), SendMessageOptions.DontRequireReceiver);

                    Log.Info("按钮 {0}.{1}被点击", gameObject.name, key);
                });
                return;
            }

            var toggle = obj.GetComponent<Toggle>();
            if (toggle != null)
            {
                toggle.onValueChanged.AddListener((isOn) =>
                {
                    gameObject.SendMessage(Utility.Text.Format("OnToggle_{0}", key), isOn,
                        SendMessageOptions.DontRequireReceiver);
                });
                return;
            }


            var slider = obj.GetComponent<Slider>();
            if (slider != null)
            {
                slider.onValueChanged.AddListener((precent) =>
                {
                    gameObject.SendMessage(Utility.Text.Format("OnSlider_{0}", key), precent,
                        SendMessageOptions.DontRequireReceiver);
                });
                return;
            }

            var img = obj.GetComponent<Image>();
            if (img != null && img.raycastTarget)
            {
                UGUIEventListener.Get(obj).onClick = (go) =>
                {
                    if (m_Params != null) m_Params.OnClickCallback?.Invoke(key);
                    gameObject.SendMessage(Utility.Text.Format("OnClick_{0}", key), go,
                        SendMessageOptions.DontRequireReceiver);
                };
                return;
            }

            //TODO 需要框架Auto注册事件的请在这里写
        }

        #endregion
    }
}