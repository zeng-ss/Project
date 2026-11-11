//------------------------------------------------------------
// File : UIComponentExtend.cs

// Desc : 
//------------------------------------------------------------

using GameFramework.DataTable;
using GameFramework.UI;
using UnityGameFramework.Runtime;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace CasualGame
{
    public static class UIComponentExtend
    {
        public static IDataTable<DRUIForm> dtUIForm = null;


        public static bool HasUIForm(this UIComponent uiComponent, UIFormType formId)
        {
            var uiForm = uiComponent.GetUIForm<UGuiForm>(formId);
            return uiForm != null;
        }

        public static T GetUIForm<T>(this UIComponent uiComponent, UIFormType formId, string uiGroupName = null) where T : UIFormLogic
        {
            string assetPath = ResourceIdentificationTool.Instance.GetAssetPathById((int)formId);

            if (string.IsNullOrEmpty(assetPath))
            {
                return null;
            }

            UIForm uiForm = null;
            if (string.IsNullOrEmpty(uiGroupName))
            {
                uiForm = uiComponent.GetUIForm(assetPath);
                if (uiForm == null)
                {
                    return null;
                }

                return (T)uiForm.Logic;
            }

            IUIGroup uiGroup = uiComponent.GetUIGroup(uiGroupName);
            if (uiGroup == null)
            {
                return null;
            }

            uiForm = (UIForm)uiGroup.GetUIForm(assetPath);
            if (uiForm == null)
            {
                return null;
            }

            return (T)uiForm.Logic;
        }

        public static void CloseUIForm(this UIComponent uiComponent, UGuiForm uiForm)
        {
            uiComponent.RemoveUIInfo((int)uiForm.UIType);
            uiComponent.CloseUIForm(uiForm.UIForm);
        }

        /// <summary>
        ///  通过UIForm close form
        /// </summary>
        /// <param name="uiComponent"></param>
        /// <param name="uiFormId"></param>
        public static void CloseUIForm(this UIComponent uiComponent, UIFormType uiFormId)
        {
            UGuiForm uiform = uiComponent.GetUIForm<UGuiForm>(uiFormId);
            if (uiform)
            {
                uiComponent.CloseUIForm(uiform);
            }

            string assetPath = ResourceIdentificationTool.Instance.GetAssetPathById((int)uiFormId);

            if (string.IsNullOrEmpty(assetPath)) return;

            if (uiComponent.IsLoadingUIForm(assetPath))
            {
                uiComponent.CloseLoadingUIForm(assetPath);
            }
        }

        public static void OpenUIForm(this UIComponent uiComponent, UIFormType formId, object userData = null, bool isEnQueue = true)
        {
            int uiFormId = (int)formId;
            if (dtUIForm == null) dtUIForm = GameEntry.DataTable.GetDataTable<DRUIForm>();
            DRUIForm drUIForm = dtUIForm.GetDataRow(uiFormId);
            uiComponent.OpenUI(formId, userData, isEnQueue);
        }

        public static int? OpenUI(this UIComponent uiComponent, int uiFormId, object userData = null,
            bool isEnQueue = true)
        {
            UIFormType formId = (UIFormType)uiFormId;

            if (dtUIForm == null)
                dtUIForm = GameEntry.DataTable.GetDataTable<DRUIForm>();
            DRUIForm drUIForm = dtUIForm.GetDataRow(uiFormId);
            if (drUIForm == null)
            {
                Log.Error("Can not load UI form '{0}' from data table.", uiFormId.ToString());
                return null;
            }

            //弹窗互斥 且当前有互斥弹窗正在显示
            if (drUIForm.Mutex && isEnQueue && uiComponent.HasMutexUI())
            {
                if (uiComponent.GetUIInfoFromCache((int)formId) == null)
                {
                    uiComponent.CacheUIInfo.Add(new UIComponent.SingleUIInfo() { uiType = uiFormId, userData = userData });
                }
                Debug.LogWarning("弹窗互斥 且当前有互斥弹窗正在显示");
                return -1;
            }

            string assetName = ResourceIdentificationTool.Instance.GetAssetPathById(uiFormId);

            if (string.IsNullOrEmpty(assetName))
            {
                Debug.LogWarning("assetName IsNullOrEmpty ");
                return null;
            }
            //string assetName = AssetUtility.GetUIFormAsset(drUIForm.AssetName);
            if (!drUIForm.AllowMultiInstance)
            {
                if (uiComponent.IsLoadingUIForm(assetName))
                {
                    Debug.LogWarning("uiComponent.IsLoadingUIForm " + assetName);
                    return null;
                }

                if (uiComponent.HasUIForm(assetName))
                {
                    Debug.LogWarning("uiComponent.HasUIForm " + assetName);
                    return null;
                }
            }

            string uiGroupName = drUIForm.UIGroupName;

            return OpenUI(uiComponent, uiFormId, assetName, uiGroupName, Constant.AssetPriority.UIFormAsset,
                drUIForm.PauseCoveredUIForm, userData);
        }

        public static int? OpenUI(this UIComponent uiComponent, UIFormType formId, string assetName, string uiGroupName,
            int priority, bool pauseCoveredUIForm = true, object userData = null)
        {
            return OpenUI(uiComponent, (int)formId, assetName, uiGroupName, priority, pauseCoveredUIForm, userData);
        }

        public static int? OpenUI(this UIComponent uiComponent, int uiFormId, string assetName, string uiGroupName,
            int priority, bool pauseCoveredUIForm = true, object userData = null)
        {
            uiComponent.AddUIInfo(uiFormId, userData);
            return uiComponent.OpenUIForm(assetName, uiGroupName, priority, pauseCoveredUIForm, userData);
        }

        public static int? OpenUI(this UIComponent uiComponent, UIFormType formId, object userData = null, bool isEnQueue = true)
        {
            int uiFormId = (int)formId;

            return OpenUI(uiComponent, uiFormId, userData, isEnQueue);
        }

        public static bool HasMutexUI(this UIComponent uiComponent)
        {
            IDataTable<DRUIForm> dtUIForm = GameEntry.DataTable.GetDataTable<DRUIForm>();
            DRUIForm drUIForm = default;
            for (int i = 0; i < uiComponent.ShowedUIInfo.Count; i++)
            {
                UIComponent.SingleUIInfo info = uiComponent.ShowedUIInfo[i];
                drUIForm = dtUIForm.GetDataRow(info.uiType);
                if (drUIForm.Mutex)
                {
                    return true;
                }
            }

            return false;

        }

        public static T OpenUIInResources<T>(this UIComponent uiComponent, string uiAssetPath, string uiGroupName,
            object userData = null) where T : MonoBehaviour
        {
            IUIGroup group = GameEntry.UI.GetUIGroup(uiGroupName);
            Object resourceObject = Resources.Load(uiAssetPath);
            GameObject gameObject = (GameObject)Object.Instantiate(resourceObject);
            Transform transform = gameObject.transform;

            transform.SetParent(((UGuiGroupHelper)group.Helper).transform);
            transform.localScale = Vector3.one;

            UIForm uiForm = gameObject.GetOrAddComponent<UIForm>();
            uiForm.OnInit(0, uiAssetPath, group, false, true, userData);
            uiForm.OnOpen(userData);

            Canvas canvas = transform.gameObject.GetComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = 30000 + transform.parent.childCount;

            return gameObject.GetComponent<T>();
        }

        public static void SnapToFixedPosition(this ScrollRect scroller, float fixValue, bool isVertical)
        {
            // Canvas.ForceUpdateCanvases();
            var contentPos = (Vector2)scroller.transform.InverseTransformPoint(scroller.content.position);

            Vector2 endPos = contentPos;
            if (isVertical)
            {
                endPos.y = fixValue;
            }
            else
            {
                endPos.x = fixValue;
            }
            scroller.content.anchoredPosition = endPos;
        }

        public static void CloseDialogUI(this UIComponent uiComponent)
        {
            var group = GameEntry.UI.GetUIGroup("Dialog");
            IUIForm[] ui = group.GetAllUIForms();
            foreach (IUIForm item in ui)
            {
                GameEntry.UI.CloseUIForm(item.SerialId);
            }
        }

        public static bool HasDialogUI(this UIComponent uiComponent)
        {
            var group = GameEntry.UI.GetUIGroup("Dialog");
            IUIForm[] ui = group.GetAllUIForms();
            return ui.Length > 0;
        }

        public static Tween BreathAnim(this Transform item, float duration, float scale = 1.1f, System.Action onComplete = null)
        {
            item.DOKill();
            item.transform.localScale = Vector3.one;
            Tween tween = item.DOScale(Vector3.one * scale, duration);
            tween.SetLoops(-1, LoopType.Yoyo);
            return tween;
        }

        public static int CurrentPopUpCount(this UIComponent uiComponent)
        {
            var loadingNum = GameEntry.UI.GetAllLoadingUIFormSerialIds().Length;
            var group = GameEntry.UI.GetUIGroup("Dialog");
            return group.GetAllUIForms().Length + loadingNum;
        }

        public static int CurrentDefaultUpCount(this UIComponent uiComponent)
        {
            var loadingNum = GameEntry.UI.GetAllLoadingUIFormSerialIds().Length;
            var group = GameEntry.UI.GetUIGroup("Default");
            return group.GetAllUIForms().Length + loadingNum;
        }
    }
}