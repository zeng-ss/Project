using UnityEngine;
using UnityEngine.UI;

namespace Framework
{
    /// <summary>
    /// 面板基类。预制体根节点上挂载具体面板脚本（继承本类）。
    /// 提供显隐、关闭按钮、简单缩放动画。
    /// </summary>
    public class UIFormBase : MonoBehaviour
    {
        public bool IsClosing { get; private set; }

        protected virtual void Awake()
        {
            var closeBtn = transform.Find("CloseBtn")?.GetComponent<Button>();
            if (closeBtn != null) closeBtn.onClick.AddListener(() => UIMgr.Instance.ClosePanel(this));
        }

        public virtual void Show()
        {
            IsClosing = false;
            gameObject.SetActive(true);
            transform.localScale = Vector3.one;
        }

        public virtual void Hide()
        {
            if (IsClosing) return;
            IsClosing = true;
            gameObject.SetActive(false);
            IsClosing = false;
        }

        public void HideImmediate()
        {
            IsClosing = false;
            gameObject.SetActive(false);
        }
    }
}