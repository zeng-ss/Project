using System.Collections.Generic;
using CasualGame;
using UnityEngine;

namespace Framework
{
    /// <summary>被互斥挡下的界面请求，等当前互斥界面关闭后再打开。</summary>
    public class UIFormRequest
    {
        public int FormId;
        public object UserData;
    }

    /// <summary>
    /// UIMgr 的「界面流程」部分：按 UIFormType 打开 / 关闭 / 互斥排队。
    /// 配置来自 tbuiform.json（DRUIForm），资源走 Addressables（地址由资源ID表查得）。
    /// </summary>
    public partial class UIMgr
    {
        private readonly Dictionary<int, UIFormBase> _forms = new();
        private readonly List<UIFormRequest> _queue = new();

        public void OpenUIForm(UIFormType formType, object userData = null, bool enqueue = true)
            => OpenUIForm((int)formType, userData, enqueue);

        /// <summary>按界面ID打开。已打开则忽略；互斥且当前有互斥界面时入队。</summary>
        private void OpenUIForm(int formId, object userData = null, bool enqueue = true)
        {
            if (!_initialized)
            {
                Debug.LogError("[UIMgr] 未初始化，无法打开界面。");
                return;
            }

            if (_forms.ContainsKey(formId)) return;

            DRUIForm drUIForm = GameRoot.Config?.GetTable<DRUIForm>()?.GetDataRow(formId);
            if (drUIForm == null)
            {
                Debug.LogError($"[UIMgr] 配置表里找不到界面 {formId}");
                return;
            }

            // 互斥：当前已有互斥界面在显示，排队等待
            if (drUIForm.Mutex && enqueue && HasMutexUI())
            {
                if (!_queue.Exists(q => q.FormId == formId))
                {
                    _queue.Add(new UIFormRequest { FormId = formId, UserData = userData });
                    Debug.Log($"[UIMgr] 界面 {formId} 互斥，已入队。");
                }

                return;
            }

            string address = ResourceIdentificationTool.Instance.GetAssetPathById(formId);
            if (string.IsNullOrEmpty(address))
            {
                Debug.LogError($"[UIMgr] 资源ID表里找不到界面 {formId} 的资源路径");
                return;
            }

            GameRoot.Res.LoadAssetAsync<GameObject>(address, prefab =>
            {
                if (prefab == null) return;
                if (_forms.ContainsKey(formId)) return;

                var obj = Object.Instantiate(prefab, _panelRoot.transform, false);
                obj.name = prefab.name;

                var panel = obj.GetComponent<UIFormBase>();
                if (panel == null)
                {
                    Debug.LogError($"[UIMgr] 预制体 {address} 上没有挂 BasePanel 派生脚本");
                    Object.Destroy(obj);
                    return;
                }

                _forms[formId] = panel;
                obj.transform.SetAsLastSibling();

                if (panel is UGuiForm guiForm) guiForm.SetParams(userData);
                panel.Show();
            });
        }

        public void CloseUIForm(UIFormType formType) => CloseUIForm((int)formType);

        private void CloseUIForm(int formId)
        {
            if (!_forms.Remove(formId, out UIFormBase panel)) return;

            if (panel != null)
            {
                panel.Hide();
                Object.Destroy(panel.gameObject);
            }

            GameRoot.Res.ReleaseAsset(ResourceIdentificationTool.Instance.GetAssetPathById(formId));
            OpenNextQueued();
        }

        public void CloseUIForm(UIFormBase panel)
        {
            if (panel == null) return;

            foreach (var pair in _forms)
            {
                if (pair.Value != panel) continue;
                CloseUIForm(pair.Key);
                return;
            }
        }

        private UIFormBase GetUIForm(int formId) => _forms.GetValueOrDefault(formId);

        public UIFormBase GetUIForm(UIFormType formType) => GetUIForm((int)formType);

        public T GetUIForm<T>(int formId) where T : UIFormBase => GetUIForm(formId) as T;

        public T GetUIForm<T>(UIFormType formType) where T : UIFormBase => GetUIForm((int)formType) as T;

        public bool HasUIForm(int formId) => _forms.ContainsKey(formId);

        public bool HasUIForm(UIFormType formType) => HasUIForm((int)formType);

        /// <summary>是否打开着任意「弹窗组」界面。</summary>
        public bool HasDialogUI() => CurrentPopUpCount() > 0;

        /// <summary>关闭所有弹窗组界面。</summary>
        public void CloseDialogUI()
        {
            ConfigTable<DRUIForm> table = GameRoot.Config?.GetTable<DRUIForm>();
            if (table == null) return;

            var ids = new List<int>();
            foreach (int formId in _forms.Keys)
            {
                DRUIForm drUIForm = table.GetDataRow(formId);
                if (drUIForm != null && drUIForm.UIGroupName == "Dialog") ids.Add(formId);
            }

            foreach (int formId in ids) CloseUIForm(formId);
        }

        /// <summary>正在显示的弹窗数量（Dialog 分组）。</summary>
        public int CurrentPopUpCount() => CountByGroup("Dialog");

        /// <summary>正在显示的默认组界面数量。</summary>
        public int CurrentDefaultUpCount() => CountByGroup("Default");

        private int CountByGroup(string groupName)
        {
            ConfigTable<DRUIForm> table = GameRoot.Config?.GetTable<DRUIForm>();
            if (table == null) return 0;

            int count = 0;
            foreach (int formId in _forms.Keys)
            {
                DRUIForm drUIForm = table.GetDataRow(formId);
                if (drUIForm != null && drUIForm.UIGroupName == groupName) count++;
            }

            return count;
        }

        /// <summary>
        /// 从 Resources 直接打开一个界面（不经过配置表和 Addressables）。
        /// 仅用于启动页这类不在配置表里的界面。
        /// </summary>
        public T OpenUIInResources<T>(string resourcesPath) where T : UIFormBase
        {
            if (!_initialized)
            {
                Debug.LogError("[UIMgr] 未初始化。");
                return null;
            }

            GameObject prefab = Resources.Load<GameObject>(resourcesPath);
            if (prefab == null)
            {
                Debug.LogError($"[UIMgr] Resources 里找不到 {resourcesPath}");
                return null;
            }

            var obj = Object.Instantiate(prefab, _panelRoot.transform, false);
            obj.name = prefab.name;
            obj.transform.SetAsLastSibling();

            var panel = obj.GetComponent<T>();
            if (panel == null)
            {
                Debug.LogError($"[UIMgr] {resourcesPath} 上没有挂 {typeof(T).Name}");
                Object.Destroy(obj);
                return null;
            }

            panel.Show();
            return panel;
        }

        /// <summary>当前是否有互斥界面在显示。</summary>
        public bool HasMutexUI()
        {
            ConfigTable<DRUIForm> table = GameRoot.Config?.GetTable<DRUIForm>();
            if (table == null) return false;

            foreach (int formId in _forms.Keys)
            {
                DRUIForm drUIForm = table.GetDataRow(formId);
                if (drUIForm != null && drUIForm.Mutex) return true;
            }

            return false;
        }

        /// <summary>打开队首的等待界面。</summary>
        private void OpenNextQueued()
        {
            if (_queue.Count == 0) return;

            UIFormRequest request = _queue[0];
            _queue.RemoveAt(0);
            OpenUIForm(request.FormId, request.UserData, false);
        }

        public void CloseAllForms()
        {
            var ids = new List<int>(_forms.Keys);
            foreach (int formId in ids)
            {
                if (_forms.TryGetValue(formId, out UIFormBase panel) && panel != null)
                {
                    Object.Destroy(panel.gameObject);
                }

                _forms.Remove(formId);
            }

            _queue.Clear();
        }
    }
}