//------------------------------------------------------------
// File : GameplayTutorialForm.cs
// Desc : 
//------------------------------------------------------------
using System.Text.RegularExpressions;
using DG.Tweening;
using NodeCanvas.Framework;
using Tutorial;
using Tutorial.Action;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace CasualGame
{
    public class GameplayTutorialForm : UGuiForm
    {
        #region SerializeField
        private GuideController m_GuideMask => GetRef<GuideController>("GuideMask");
        private UnityEngine.RectTransform m_Effect => GetRef<UnityEngine.RectTransform>("Effect");
        private UnityEngine.RectTransform m_Finger => GetRef<UnityEngine.RectTransform>("Finger");
        private UnityEngine.RectTransform m_Description => GetRef<UnityEngine.RectTransform>("Description");
        private Spine.Unity.SkeletonGraphic m_ActorIcon => GetRef<Spine.Unity.SkeletonGraphic>("ActorIcon");
        private UnityEngine.UI.Image m_TipsContent => GetRef<UnityEngine.UI.Image>("TipsContent");
        private UnityEngine.UI.Button m_Continue => GetRef<UnityEngine.UI.Button>("Continue");
        private TMPro.TextMeshProUGUI m_Tips => GetRef<TMPro.TextMeshProUGUI>("Tips");
        private TweenScale m_ContinueTxt => GetRef<TweenScale>("ContinueTxt");
        private UnityEngine.RectTransform m_CloneParent => GetRef<UnityEngine.RectTransform>("CloneParent");
        #endregion


        private Image m_MaskImg => GetRef<Image>("GuideMask");
        private Color m_MaskColor;
        private Action OnClickMasking = null;// 点击蒙版时的监听
        private Action OnClickTargetBtn = null;// 点击对应的按钮

        public Blackboard blackboard { get => GetComponent<Blackboard>(); }

        private RectTransform m_CloneRect; //执行结束要清理
        private Button m_TargetBtn; //和👆是同一个按钮
        private Image m_TargetImg; //和👆是同一个图片 图方式直接添加按钮


        [HideInInspector]
        public bool IsUseing = false;


        #region SerializeField
        //Show Tips 下的按钮
        private void OnClick_Continue()
        {
            Log.Info("OnClick_Continue");
            OnClickTargetBtn?.Invoke();
        }

        private void OnClick_GuideMask()
        {
            Log.Info("OnClick_GuideMask");
            OnClickMasking?.Invoke();
            // if (maskIsSafe && Time.realtimeSinceStartup - startObstructTime >= 5)
            // {
            //     m_MaskImg.gameObject.SetActive(false);
            // }
        }

        private void OnTargetBtnClick(GameObject go)
        {
            Log.Info("OnTargetBtnClick Go");
            OnClickTargetBtn?.Invoke();
        }

        private void OnTargetBtnClick()
        {
            Log.Info("OnTargetBtnClick");
            if (m_CloneRect != null && m_TargetBtn != null)
            {
                m_TargetBtn.onClick.Invoke();
            }
            OnClickTargetBtn?.Invoke();
        }


        #endregion

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            m_MaskColor = m_MaskImg.color;
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            DisableAll();
        }

        private void DisableAll()
        {
            m_Description.gameObject.SetActive(false);
            m_MaskImg.gameObject.SetActive(false);
            m_Finger.gameObject.SetActive(false);
            m_Effect.gameObject.SetActive(false);
        }


        #region Action Methods
        public bool ExecuteAction(IActionData actionData)
        {
            if (actionData == null) return false;
            var result = actionData.ActionType switch
            {
                ActionType.Clean => ExecuteCleanAction(actionData as CleanActionData),
                ActionType.ShowMask => ExecuteMaskAction(actionData as MaskActionData),
                ActionType.ShowTutorialTip => ExecuteShowTutorialTipAction(actionData as ShowTutorialTipActionData),
                _ => true,
            };

            ReferencePool.Release(actionData as IReference);
            return result;
        }

        private bool ExecuteCleanAction(CleanActionData cleanActionData)
        {
            IsUseing = false;

            DisableAll();

            if (m_CloneRect != null)
            {
                CUtility.Mono.Destroy(m_CloneRect);
                m_CloneRect = null;
            }

            if (m_TargetBtn != null)
            {
                m_TargetBtn.onClick.RemoveListener(OnTargetBtnClick);
                m_TargetBtn = null;
            }

            if (m_TargetImg != null)
            {
                var uguiEvent = m_TargetImg.GetComponent<UGUIEventListener>();
                Destroy(uguiEvent);
                m_TargetImg = null;
            }

            m_GuideMask.Clear();

            OnClickMasking = null;
            OnClickTargetBtn = null;
            return true;
        }

        private bool ExecuteMaskAction(MaskActionData maskActionData)
        {
            if (maskActionData == null) return false;

            IsUseing = true;
            m_MaskImg.gameObject.SetActive(maskActionData.IsShow);

            var color = m_MaskImg.color;
            m_MaskImg.DOFade(0, 0);
            m_MaskImg.DOFade(maskActionData.Alpha * 1.0f / 255f, 0.2f);

            if (maskActionData.ClearHole)
            {
                m_GuideMask.Clear();
            }
            return true;
        }

        private bool ExecuteShowTutorialTipAction(ShowTutorialTipActionData tipActionData)
        {
            // TODO: 这里只创建空方法，后续按设计补充（文本、位置、继续按钮、回调等）
            if (tipActionData == null) return false;

            string localizations = tipActionData.Tips;
            if (!string.IsNullOrEmpty(tipActionData.LocalizationKey))
            {
                localizations = LocalizationMgr.Instance.GetString(tipActionData.LocalizationKey);
            }

            string pattern = @"{\d}";
            Match match = Regex.Match(localizations, pattern);
            if (match.Success)
            {
                m_Tips.text = string.Format(localizations, TutorialManager.Instance.GetVariableValue<string>(BlackboardKey.Str_TipPlaceHolder));
            }
            else
            {
                m_Tips.text = localizations;
            }
            IsUseing = true;

            m_Description.anchoredPosition = tipActionData.TipPosition;
            m_Description.gameObject.SetActive(true);

            m_Description.localScale = Vector3.one * 0.3f;
            m_Description.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

            m_Continue.gameObject.SetActive(false);
            m_ContinueTxt.gameObject.SetActive(false);
            if (tipActionData.HasContinue)
            {
                this.OnClickTargetBtn = tipActionData.OnClickTargetBtn;
                this.AttachTimer(0.8f, () =>
                {
                    m_Continue.gameObject.SetActive(tipActionData.HasContinue);
                    m_ContinueTxt.gameObject.SetActive(tipActionData.HasContinue);
                });
            }
            Log.Info("执行提示内容 {0}", localizations);
            return true;
        }

        /// <summary>
        /// 在原来基础上附加事件方式
        /// </summary>
        /// <param name="targetRectTrans"></param>
        private void AttachBtnEvent(RectTransform targetRectTrans)
        {
            m_TargetBtn = targetRectTrans.GetComponent<Button>();
            if (m_TargetBtn != null)
            {
                m_TargetBtn.onClick.AddListener(OnTargetBtnClick);

                return;
            }

            m_TargetImg = targetRectTrans.GetComponent<Image>();
            if (m_TargetImg != null)
            {
                Log.Info("AttachBtnEvent  UGUIEventListener  GoName={0}", targetRectTrans.gameObject.name);
                UGUIEventListener.Get(targetRectTrans.gameObject).onClick = OnTargetBtnClick;
            }
        }

        /// <summary>
        /// 克隆方式附加按钮事件
        /// </summary>
        /// <param name="targetRectTrans"></param>
        /// <param name="original"></param>
        /// <param name="btnRelatePath"> if not null </param>
        private void SetButtonEvent(RectTransform targetRectTrans, RectTransform original, string btnRelatePath)
        {
            if (!string.IsNullOrEmpty(btnRelatePath))
            {
                targetRectTrans = targetRectTrans.Find(btnRelatePath).gameObject.GetComponent<RectTransform>();
                original = original.Find(btnRelatePath).gameObject.GetComponent<RectTransform>();
            }

            Button targetBtn = targetRectTrans.GetComponent<Button>();
            if (targetBtn != null)
            {
                targetBtn.onClick.RemoveAllListeners();
                targetBtn.onClick.AddListener(OnTargetBtnClick);
                m_TargetBtn = original.GetComponent<Button>();
                return;
            }


            Image targetImg = targetRectTrans.GetComponent<Image>();
            if (targetImg != null)
            {
                Log.Info("AttachBtnEvent  UGUIEventListener  GoName={0}", targetRectTrans.gameObject.name);
                UGUIEventListener.Get(targetRectTrans.gameObject).onClick += OnTargetBtnClick;
                return;
            }

            //子节点有Button 
            var targetBtn2 = targetRectTrans.GetComponentInChildren<Button>();
            if (targetBtn == null && targetBtn2 != null)
            {
                targetBtn2.onClick.RemoveAllListeners();
                targetBtn2.onClick.AddListener(OnTargetBtnClick);

                //临时处理特色点击
                var defaultCanvas = Framework.UIMgr.Instance.MainCanvas;
                var anchoredPositionInRect = TransformUtility.WorldToScreenPoints(Framework.UIMgr.Instance.UICamera, Rect, targetBtn2.transform.position);
                m_Finger.anchoredPosition = anchoredPositionInRect;
                m_TargetBtn = original.GetComponentInChildren<Button>();
                return;
            }
        }
        #endregion

    }
}
