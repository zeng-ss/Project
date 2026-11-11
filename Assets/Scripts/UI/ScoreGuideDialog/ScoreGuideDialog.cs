//------------------------------------------------------------
// File : ScoreGuideDialog.cs
// Desc : 
//------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace CasualGame
{
    public class ScoreGuideDialog : ScaleDialog
    {
        // AutoBindFieldInfo
        [Header("PrefabBinderTool")]
        [SerializeField] private UnityEngine.RectTransform m_StarParent;

        [SerializeField] private AttachParentPrefabPool m_Pool;

        [SerializeField] private UnityEngine.UI.Button m_CloseBtn;

        // AutoBindFieldInfo

        private List<ScoreStarCell> _starCells = new List<ScoreStarCell>();
        private readonly int _starCount = 5;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            m_CloseBtn.onClick.RemoveAllListeners();
            m_CloseBtn.onClick.AddListener(OnClick_Close);

            for (int i = 0; i < _starCount; i++)
            {
                ScoreStarCell starCell = m_Pool.Spawn<ScoreStarCell>(0, m_StarParent);
                starCell.Init(i, OnStarClick, AtlasLoader);
                _starCells.Add(starCell);
            }
            _starCells.Horizontal(TextAnchor.MiddleCenter, 8f);
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
            foreach (var starCell in _starCells)
            {
                m_Pool.DeSpawn(starCell);
            }
            _starCells.Clear();
        }

        private void OnClick_Close()
        {
            Close();
        }

        private void OnStarClick(int index)
        {
            for (int i = 0; i < _starCells.Count; i++)
            {
                _starCells[i].ChangeState(i <= index);
            }

            if (index > 2)
            {
                LogHandler.Info("==> [Rate]：进入评分界面");
                HasScore = true;
            }

            Close();
        }

        public static bool HasScore
        {
            get
            {
                return ProxyManager.UserProxy.userModel.hasScore;
            }
            set
            {
                ProxyManager.UserProxy.userModel.hasScore = value;
                ProxyManager.UserProxy.Save();
            }
        }

        public static void Show(int level, Action onClose)
        {
            if (HasScore)
            {
                onClose?.Invoke();
                return;
            }

            int unlockLevel = 5;
            if (level >= unlockLevel && (level - unlockLevel) % 5 == 0)
            {
                UGUIParams uGUIParams = UGUIParams.Create(() => onClose?.Invoke());
                Framework.UIMgr.Instance.OpenUIForm(UIFormType.ScoreGuideDialog, uGUIParams);
            }
            else
            {
                onClose?.Invoke();
            }
        }
    }
}
