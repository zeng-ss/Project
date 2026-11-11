using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Framework
{
    /// <summary>
    /// UI 面板管理器：Canvas 搭建、面板加载/显隐/销毁。
    /// 面板资源通过 Addressables 加载（地址由 PanelPathAttribute 决定）。
    /// </summary>
    public partial class UIMgr
    {
        private static UIMgr _instance;
        public static UIMgr Instance => _instance ??= new UIMgr();

        private readonly Dictionary<string, UIFormBase> _panels = new();
        public RectTransform Canvas { get; private set; }

        /// <summary>主 Canvas，替代原 Framework.UIMgr.Instance.MainCanvas。</summary>
        public Canvas MainCanvas { get; private set; }

        /// <summary>UI 相机，替代原 Framework.UIMgr.Instance.UICamera（Overlay 模式下回退主相机）。</summary>
        public Camera UICamera => MainCanvas != null && MainCanvas.worldCamera != null
            ? MainCanvas.worldCamera
            : Camera.main;

        private GameObject _panelRoot;
        private bool _initialized;

        public void Init(Transform canvasParent = null)
        {
            if (_initialized && Canvas != null) return;

            var canvasObj = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            MainCanvas = canvas;
            Canvas = canvasObj.transform as RectTransform;

            if (canvasParent != null) canvasObj.transform.SetParent(canvasParent, false);
            else UnityEngine.Object.DontDestroyOnLoad(canvasObj);

            var root = new GameObject("Panels", typeof(RectTransform));
            var rect = root.GetComponent<RectTransform>();
            rect.SetParent(Canvas, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            _panelRoot = root;

            _initialized = true;
        }

        public void OpenPanel<T>(Action<T> onComplete = null) where T : UIFormBase
        {
            if (!_initialized)
            {
                Debug.LogError("[UIMgr] 未初始化，无法打开面板。");
                onComplete?.Invoke(null);
                return;
            }

            string path = PanelPathResolver.Resolve<T>();

            if (_panels.TryGetValue(path, out var exist))
            {
                exist.Show();
                onComplete?.Invoke(exist as T);
                return;
            }

            GameRoot.Res.LoadAssetAsync<GameObject>(path, prefab =>
            {
                if (prefab == null)
                {
                    onComplete?.Invoke(null);
                    return;
                }

                var obj = UnityEngine.Object.Instantiate(prefab, _panelRoot.transform, false);
                obj.name = path;

                var panel = obj.GetComponent<T>();
                if (panel == null)
                {
                    Debug.LogError($"[UIMgr] 面板 {path} 上找不到组件 {typeof(T).Name}");
                    UnityEngine.Object.Destroy(obj);
                    onComplete?.Invoke(null);
                    return;
                }

                _panels[path] = panel;
                panel.Show();
                onComplete?.Invoke(panel);
            });
        }

        public void ClosePanel<T>() where T : UIFormBase
        {
            string path = PanelPathResolver.Resolve<T>();
            if (_panels.TryGetValue(path, out var panel)) panel.Hide();
        }

        public void ClosePanel(UIFormBase panel)
        {
            if (panel == null) return;
            foreach (var kv in _panels)
            {
                if (kv.Value != panel) continue;
                panel.Hide();
                return;
            }

            panel.Hide();
        }

        public void DestroyPanel<T>() where T : UIFormBase
        {
            string path = PanelPathResolver.Resolve<T>();
            if (!_panels.TryGetValue(path, out var panel)) return;

            UnityEngine.Object.Destroy(panel.gameObject);
            _panels.Remove(path);
            GameRoot.Res.ReleaseAsset(path);
        }

        public T GetPanel<T>() where T : UIFormBase
        {
            string path = PanelPathResolver.Resolve<T>();
            return _panels.TryGetValue(path, out var panel) ? panel as T : null;
        }

        public void DestroyAll()
        {
            foreach (var panel in _panels.Values)
            {
                if (panel != null) UnityEngine.Object.Destroy(panel.gameObject);
            }

            _panels.Clear();
        }
    }
}