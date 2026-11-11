//------------------------------------------------------------
// File : TransitionForm.cs
// Desc : 
//------------------------------------------------------------
using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

namespace CasualGame
{
    public class TransitionForm : UGuiForm
    {
        // AutoBindFieldInfo
        [Header("PrefabBinderTool")]
        [SerializeField] private UnityEngine.CanvasGroup m_Bg;

        [SerializeField] private UnityEngine.UI.RawImage m_RawImage;

        // AutoBindFieldInfo

        private Func<UniTask> m_OnOpenAction;
        private Sequence m_Sequence;
        private bool m_IsOpen;
        private bool m_LoadComplete;
        private float _time;
        private readonly float fadeDuration = 0.5f;
        private readonly float showTime = 0.8f;
        public static bool IsOpening;
        private Rect m_UvRect;
        private readonly int m_StartDescKey = 1035;
        private readonly int m_EndDescKey = 1064;

        public static void Open(int hard, Func<UniTask> onOpenAction)
        {
            UGUIParams param = UGUIParams.Create();
            param.AddValue("Hard", hard);
            param.AddValue("OnOpenAction", onOpenAction);
            Framework.UIMgr.Instance.OpenUIForm(UIFormType.TransitionForm, param);
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            // PlaySoundManager.Instance.PlaySound(Constant.Audio.Loading);
            m_OnOpenAction = MParams.GetAnyParams<Func<UniTask>>("OnOpenAction");
            m_IsOpen = false;
            m_LoadComplete = false;
            IsOpening = true;

            if (m_OnOpenAction == null)
            {
                Close();

                Debug.LogError("TransitionForm Open Error");
                return;
            }

            //RefreshDescText();

            m_Bg.alpha = 0f;
            m_Sequence = DOTween.Sequence();
            m_Sequence.Append(m_Bg.DOFade(1f, fadeDuration).SetEase(Ease.OutQuad));
            m_Sequence.OnComplete(() =>
            {
                RunOpenAction().Forget();
            });
            _time = 0f;

            m_UvRect = m_RawImage.uvRect;
            m_RawImage.uvRect = new Rect(0f, 0f, 5f, 5f);
        }

        private async UniTaskVoid RunOpenAction()
        {
            m_IsOpen = true;
            _time = 0f;

            try
            {
                await m_OnOpenAction();
            }
            catch (Exception e)
            {
                Debug.LogError($"TransitionForm OnOpenAction failed: {e}");
            }
            finally
            {
                m_LoadComplete = true;
            }
        }

        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            if (m_IsOpen)
            {
                _time += elapseSeconds;
                if (m_LoadComplete && _time > showTime)
                {
                    CloseTransition();
                }
            }

            m_UvRect.x -= 0.2f * elapseSeconds;
            m_RawImage.uvRect = m_UvRect;
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
            IsOpening = false;
        }

        // private void RefreshDescText()
        // {
        //     int lastKey = ProxyManager.UserProxy.LastPlayDescKey;
        //     int currentKey = lastKey == 0 ? m_StartDescKey : lastKey + 1;
        //     if (currentKey > m_EndDescKey)
        //     {
        //         currentKey = m_StartDescKey;
        //     }

        //     ProxyManager.UserProxy.LastPlayDescKey = currentKey;
        //     m_des.text = LocalizationMgr.Instance.GetString(currentKey.ToString());
        // }

        private void CloseTransition()
        {
            PlaySoundManager.Instance.PlaySound(Constant.Audio.LoadingComplete);
            m_IsOpen = false;
            if (m_Sequence.IsActive()) m_Sequence.Kill();

            m_Sequence = DOTween.Sequence();
            m_Bg.alpha = 1f;
            m_Sequence.Append(m_Bg.DOFade(0f, fadeDuration));
            m_Sequence.OnComplete(() =>
            {
                Close();
            });
        }
    }
}
