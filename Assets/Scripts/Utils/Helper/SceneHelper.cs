//------------------------------------------------------------
// File : SceneHelper.cs
// Desc : 主界面 / 对局界面的切换
//------------------------------------------------------------

using System;
using CasualGame.CoffeeAway;
using Cysharp.Threading.Tasks;
using Framework;
using UnityEngine;

namespace CasualGame
{
    public static class SceneHelper
    {
        /// <summary>回主界面（Home 没有独立场景，只切界面）。</summary>
        public static void LoadHomeScene(int hard, Action onCompleteAction = null)
        {
            if (IsHomeScene())
            {
                onCompleteAction?.Invoke();
                return;
            }

            TransitionForm.Open(hard, async () =>
            {
                UIMgr.Instance.CloseUIForm(UIFormType.UIGameForm);
                UIMgr.Instance.OpenUIForm(UIFormType.UIMainForm);

                await UniTask.WaitUntil(() => UIMgr.Instance.GetUIForm<UIMainForm>(UIFormType.UIMainForm) != null);

                onCompleteAction?.Invoke();
            });
        }

        /// <summary>进对局界面：加载 MyGame 场景并打开对局 UI。</summary>
        public static void LoadGameScene(Action onCompleteAction = null)
        {
            if (IsGamePlayScene())
            {
                onCompleteAction?.Invoke();
                return;
            }

            TransitionForm.Open(0, async () =>
            {
                UIMgr.Instance.CloseUIForm(UIFormType.UIMainForm);
                GameFlow.Instance.SwitchScene(Constant.Scene.MyGame, () =>
                {
                    UIMgr.Instance.OpenUIForm(UIFormType.UIGameForm);
                });

                await UniTask.WaitUntil(() => CoffeeAwayGame.Instance != null);

                onCompleteAction?.Invoke();
            });
        }

        public static bool IsHomeScene()
        {
            return UIMgr.Instance.HasUIForm(UIFormType.UIMainForm);
        }

        public static bool IsGamePlayScene()
        {
            return UIMgr.Instance.HasUIForm(UIFormType.UIGameForm);
        }
    }
}
