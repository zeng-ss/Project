using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityGameFramework.Runtime;
using UnityEngine.UI;

namespace CasualGame
{
    public class GuideFingerCell : MonoBehaviour, IUGUIChild
    {
        // AutoBindFieldInfo
        [Header("PrefabBinderTool")]
        [SerializeField] private UnityEngine.Camera GuideCamera_Camera;

        [SerializeField] private UnityEngine.UI.Button GuideButton_Button;

        // AutoBindFieldInfo

        private FingerGuide _fingerGuide;

        private Button m_TargetBtn;
        private Camera _targetCamera;
        private Button _targetButton;
        private Vector2 _targetGuidePosition;
        private Action _clickAction;
        private DRFingerGuide _drFingerGuide;

        private CancellationTokenSource _cancellationTokenSource;

        private void Awake()
        {
            GuideButton_Button.onClick.AddListener(Click_Button);
        }

        private void Click_Button()
        {
            //PlaySoundManager.Instance.PlaySound(Constant.Audio.click_start);
            _clickAction?.Invoke();

            PointerEventData eventData = new PointerEventData(EventSystem.current);

            if (_targetButton)
            {
                _targetButton.OnPointerDown(eventData);
                _targetButton.OnPointerUp(eventData);
                _targetButton.OnPointerClick(eventData);
            }

            GuideManager.FinishGuide();
        }

        public async UniTaskVoid Guide(DRFingerGuide drFingerGuide, Action clickAction)
        {
            _drFingerGuide = drFingerGuide;
            _clickAction = clickAction;

            // 第一步获取指定对象
            Transform rootTransform = null;
            if (drFingerGuide.SceneName == "UI" && int.TryParse(drFingerGuide.RootName, out int formType))
            {
                UGuiForm form = GameEntry.UI.GetUIForm<UGuiForm>((UIFormType)formType);
                if (form == null)
                {
                    Log.Error("Cant find form formtype = {0}  guide Step={1} ", formType,
                        drFingerGuide.Id);
                    GameEntry.UI.CloseUIForm(UIFormType.GuideUI);
                    return;
                }

                rootTransform = form.CachedTransform;
            }
            else
            {
                rootTransform = SceneTool.GetSceneTransformRoot(drFingerGuide.SceneName, drFingerGuide.RootName);
            }

            if (rootTransform == null)
            {
                Log.Error("Cant find targetTrans path = {0}  guide Step={1} ", drFingerGuide.RootName, drFingerGuide.Id);
                GameEntry.UI.CloseUIForm(UIFormType.GuideUI);
                return;
            }

            List<Transform> targetTransformList = new List<Transform>();

            // 分割路径
            string[] highLightPathArray = drFingerGuide.HigtLightTargetPath.Replace(" ", "").Split(",");

            foreach (var highLightPath in highLightPathArray)
            {
                Transform tempTransform = rootTransform.Find(highLightPath);
                if (tempTransform == null)
                {
                    Log.Error("Cant find targetTrans path = {0}  guide Step={1} ", highLightPath, drFingerGuide.Id);
                    GameEntry.UI.CloseUIForm(UIFormType.GuideUI);
                    return;
                }

                targetTransformList.Add(tempTransform);
            }

            foreach (var targetTransform in targetTransformList)
            {
                await UniTask.WaitUntil(() => targetTransform.gameObject.activeInHierarchy, cancellationToken: _cancellationTokenSource.Token);
            }

            if (_cancellationTokenSource == null || _cancellationTokenSource.Token.IsCancellationRequested)
                return;

            Transform guideClickTransform = targetTransformList[0];

            if (drFingerGuide.ButtonPath == "-1.0" || drFingerGuide.ButtonPath == "-1")
            {
                _targetButton = guideClickTransform.GetComponentInChildren<Button>();
            }
            else
            {
                _targetButton = guideClickTransform.Find(drFingerGuide.ButtonPath).GetComponentInChildren<Button>();
            }

            // 第二步 将指定对象层级调上来
            // 先寻找到原始拍摄该物体的相机
            Camera targetCamera = CameraTool.GetTargetTransformCamera(guideClickTransform);

            _targetCamera = targetCamera;
            bool isUICamera = _targetCamera == GameEntry.UI.MainCanvas.worldCamera;
            GuideCamera_Camera.gameObject.SetActive(!isUICamera);

            if (!isUICamera)
            {
                // 同步位置
                GuideCamera_Camera.transform.position = targetCamera.transform.position;
                GuideCamera_Camera.transform.rotation = targetCamera.transform.rotation;
                // 同步相机属性
                GuideCamera_Camera.orthographicSize = targetCamera.orthographicSize;
                GuideCamera_Camera.nearClipPlane = targetCamera.nearClipPlane;
                GuideCamera_Camera.farClipPlane = targetCamera.farClipPlane;

                foreach (var targetTransform in targetTransformList)
                {
                    // 设置对象 Layer
                    int originLayer = targetTransform.gameObject.layer;
                    targetTransform.gameObject.SetLayerRecursively(LayerMask.NameToLayer("FingerGuide"));
                    _clickAction += () =>
                    {
                        targetTransform.gameObject.SetLayerRecursively(originLayer);
                    };
                }
            }

            foreach (var targetTransform in targetTransformList)
            {
                // 如果是 Canvas 管理则另外添加 canvas
                if (targetTransform.GetComponentInParent<Canvas>() != null)
                {
                    Canvas canvas = targetTransform.gameObject.GetOrAddComponent<Canvas>();
                    GraphicRaycaster graphicRaycaster = targetTransform.gameObject.GetOrAddComponent<GraphicRaycaster>();
                    graphicRaycaster.enabled = false;

                    _clickAction += () =>
                    {
                        graphicRaycaster.enabled = true;
                    };

                    if (isUICamera)
                    {
                        canvas.overrideSorting = true;
                        canvas.sortingOrder = 32000;

                        _clickAction += () =>
                        {
                            canvas.overrideSorting = false;
                        };
                    }
                }
            }

            // 第三步 在引导物体范围上盖一层按钮
            // 获取目标对象在引导 UI 下的包围盒大小，用于设置引导按钮位置和大小
            Bounds bounds = BoundTool.GetBounds(guideClickTransform, transform, out int result);
            Rect screenRect = BoundTool.BoundsToScreenRect(bounds, targetCamera);
            Vector2 screenCenter = screenRect.center;
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                GameEntry.UI.MainCanvas.GetComponent<RectTransform>(),
                screenCenter,
                GameEntry.UI.MainCanvas.worldCamera,
                out localPoint
            );
            GuideButton_Button.GetComponent<RectTransform>().localPosition = localPoint;
            GuideButton_Button.GetComponent<RectTransform>().sizeDelta = new Vector2(screenRect.width, screenRect.height);

            // 第四步 添加手指引导
            ShowFinger();
        }

        private async void ShowFinger()
        {
            if (!_targetButton || !_targetCamera) return;

            bool isUI = _targetButton.GetComponentInParent<Canvas>() != null;
            if (isUI)
            {
                _fingerGuide = await PrefabPool.Instance.Spawn<FingerGuide>(996, _cancellationTokenSource.Token);
                if (_fingerGuide == null)
                    return;

                _fingerGuide.SetTarget(_targetButton.transform,
                    _drFingerGuide.Pivot == Vector2.one * -1 ? null : _drFingerGuide.Pivot, true, _targetCamera);
            }
            else
            {
                _fingerGuide = await PrefabPool.Instance.Spawn<FingerGuide>(997, _cancellationTokenSource.Token);
                if (_fingerGuide == null)
                    return;

                _fingerGuide.SetTarget3D(_targetButton.transform, true, _targetCamera);
            }

        }

        public void OnInit()
        {
        }

        public void OnOpen()
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            // 同步相机属性
            if (GuideCamera_Camera.gameObject.activeInHierarchy && _targetCamera)
            {
                GuideCamera_Camera.orthographicSize = _targetCamera.orthographicSize;
            }
        }

        public void OnClose()
        {
            if (_fingerGuide != null)
            {
                PrefabPool.Instance.DeSpawn(_fingerGuide);
                _fingerGuide = null;
            }

            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }

        public void OnSecondUpdate(float time)
        {

        }
    }
}