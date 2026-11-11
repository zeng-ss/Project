using System.Collections.Generic;
using System.Threading;
using Framework;
using UnityEngine;
using UnityEngine.UI;

namespace CasualGame
{
    /// <summary>
    /// 所有 UGUI 界面的基类。
    /// 由 Framework.BasePanel 派生，保留原 UIFormLogic 的生命周期方法名，
    /// 让各界面脚本的 override 不需要改动。
    /// </summary>
    [RequireComponent(typeof(UIConfigMono))]
    public abstract class UGuiForm : UIFormBase, IShowMaskInterface
    {
        private readonly List<IUGUIChild> _uguiChildren = new List<IUGUIChild>();
        private UIFormType _mFormType = UIFormType.Undefined;

        protected Canvas MCachedCanvas;
        protected CanvasGroup MCanvasGroup;
        protected UIConfigMono MConfig;
        protected UGUIParams MParams;
        protected Transform Panel;
        protected readonly AtlasLoader AtlasLoader = new AtlasLoader();

        /// <summary>用于取消异步任务，界面关闭时自动取消。</summary>
        protected CancellationTokenSource CancellationTokenSource;

        [Header("展示黑色遮罩")] [SerializeField] public bool showMask = true;
        [Header("展示资源栏")] [SerializeField] public bool showCurrency = false;

        public bool ShowMask => showMask;
        public bool ShowCurrency => showCurrency;

        /// <summary>界面是否显示中（替代原 UIFormLogic.Visible）。</summary>
        public bool Visible => gameObject.activeSelf;

        /// <summary>自身 Transform 缓存（替代原 UIFormLogic.CachedTransform）。</summary>
        public Transform CachedTransform => transform;

        public virtual bool UIAnimationFinish { get; set; }

        public int OriginalDepth { get; private set; }

        private RectTransform _mRect;
        public RectTransform Rect => _mRect;

        public int MaskDepth => MCachedCanvas != null ? MCachedCanvas.sortingOrder : 0;

        /// <summary>界面类型，从预制体名字（如 UIMainForm#10001）里解析。</summary>
        public UIFormType UIType
        {
            get
            {
                if (_mFormType != UIFormType.Undefined) return _mFormType;

                string objName = gameObject.name;
                int hash = objName.LastIndexOf('#');
                if (hash < 0) return _mFormType;

                int dot = objName.LastIndexOf('.');
                if (dot < 0) dot = objName.Length;

                if (int.TryParse(objName.Substring(hash + 1, dot - hash - 1), out int assetId))
                {
                    _mFormType = (UIFormType)assetId;
                }

                return _mFormType;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            Panel = transform.Find("Panel");
            MConfig = GetComponent<UIConfigMono>();
            if (MConfig == null)
            {
                Debug.LogError($"[UGuiForm] {gameObject.name} 未挂载 UIConfigMono");
                return;
            }

            MConfig.Init();

            MCachedCanvas = gameObject.GetOrAddComponent<Canvas>();
            MCachedCanvas.overrideSorting = true;
            OriginalDepth = MCachedCanvas.sortingOrder;

            _mRect = GetComponent<RectTransform>();
            _mRect.anchorMin = Vector2.zero;
            _mRect.anchorMax = Vector2.one;
            _mRect.anchoredPosition = Vector2.zero;
            _mRect.sizeDelta = Vector2.zero;

            gameObject.GetOrAddComponent<GraphicRaycaster>();

            GameRoot.Config?.GetTable<DRUIForm>()?.GetDataRow((int)UIType);

            GetComponentsInChildren(true, _uguiChildren);
            foreach (IUGUIChild child in _uguiChildren)
            {
                child.OnInit();
            }

            if (Panel != null)
            {
                MCanvasGroup = Panel.gameObject.GetOrAddComponent<CanvasGroup>();
            }

            OnInit(null);
            OnDepthChanged(0, 0);
        }

        private void Update()
        {
            OnUpdate(Time.deltaTime, Time.unscaledDeltaTime);

            foreach (IUGUIChild child in _uguiChildren)
            {
                child.OnUpdate(Time.deltaTime, Time.unscaledDeltaTime);
            }
        }

        private void OnDestroy()
        {
            AtlasLoader?.Dispose();

            if (CancellationTokenSource == null) return;
            CancellationTokenSource.Cancel();
            CancellationTokenSource.Dispose();
            CancellationTokenSource = null;
        }

        #region 生命周期（签名与原 UIFormLogic 一致，供各界面 override）

        protected virtual void OnInit(object userData)
        {
        }

        protected virtual void OnOpen(object userData)
        {
        }

        protected virtual void OnClose(bool isShutdown, object userData)
        {
        }

        protected virtual void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
        }

        protected virtual void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
        {
        }

        protected virtual void OnOpenCallback()
        {
            MParams?.OnOpenCallback?.Invoke(this);
        }

        #endregion

        public override void Show()
        {
            base.Show();

            CancellationTokenSource = new CancellationTokenSource();
            UIAnimationFinish = true;

            StopAllCoroutines();
            OnOpen(MParams);

            foreach (IUGUIChild child in _uguiChildren)
            {
                child.OnOpen();
            }

            OnOpenCallback();
        }

        public override void Hide()
        {
            if (IsClosing) return;

            OnClose(false, MParams);

            if (MParams != null)
            {
                MParams.OnCloseCallback?.Invoke();
                MParams = null;
            }

            foreach (IUGUIChild child in _uguiChildren)
            {
                child.OnClose();
            }

            AtlasLoader?.Cancel();

            CancellationTokenSource?.Cancel();
            CancellationTokenSource?.Dispose();
            CancellationTokenSource = null;

            UIAnimationFinish = false;
            base.Hide();
        }

        /// <summary>打开时由 UIManager 塞入参数。</summary>
        public void SetParams(object userData)
        {
            MParams = userData as UGUIParams;
        }

        public virtual void Close()
        {
            UIMgr.Instance.CloseUIForm(this);
        }

        public void PlayUISound(int uiSoundId)
        {
            PlaySoundManager.Instance.PlaySound(uiSoundId);
        }

        public void OpenUIForm(UIFormType formType)
        {
            UIMgr.Instance.OpenUIForm(formType);
        }

        #region UIConfigMono 绑定

        public T GetRef<T>(string key) where T : Object
        {
            if (MConfig == null)
            {
                Debug.LogError($"[{gameObject.name}] UIConfigMono 为空，取不到 {key}");
                return null;
            }

            var bind = MConfig.GetBindEntityByTag(key);
            if (bind == null)
            {
                Debug.LogError($"[{gameObject.name}] 找不到绑定 key: {key}");
                return null;
            }

            if (typeof(T) == typeof(GameObject)) return bind.obj as T;
            if (typeof(T) == typeof(Transform)) return bind.obj.transform as T;

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
                    MParams?.OnClickCallback?.Invoke(key);
                    gameObject.SendMessage($"OnClick_{key}", SendMessageOptions.DontRequireReceiver);
                    Debug.Log($"按钮 {gameObject.name}.{key} 被点击");
                });
                return;
            }

            var toggle = obj.GetComponent<Toggle>();
            if (toggle != null)
            {
                toggle.onValueChanged.AddListener(isOn =>
                {
                    gameObject.SendMessage($"OnToggle_{key}", isOn, SendMessageOptions.DontRequireReceiver);
                });
                return;
            }

            var slider = obj.GetComponent<Slider>();
            if (slider != null)
            {
                slider.onValueChanged.AddListener(percent =>
                {
                    gameObject.SendMessage($"OnSlider_{key}", percent, SendMessageOptions.DontRequireReceiver);
                });
                return;
            }

            var img = obj.GetComponent<Image>();
            if (img != null && img.raycastTarget)
            {
                UGUIEventListener.Get(obj).onClick = go =>
                {
                    MParams?.OnClickCallback?.Invoke(key);
                    gameObject.SendMessage($"OnClick_{key}", go, SendMessageOptions.DontRequireReceiver);
                };
            }
        }

        #endregion
    }
}