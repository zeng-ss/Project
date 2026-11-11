using UnityEngine;

namespace CasualGame.CoffeeAway
{
    public class StandardHolder : Holder
    {
        [SerializeField] private Animator Animator;
        [SerializeField] private ParticleSystem completeCheck;

        private bool _animJustStarted; // 动画播放首帧 Animator 尚未初始化，需隔一帧再读状态

        public override void Spawn()
        {
            base.Spawn();
            completeCheck.gameObject.SetActive(false);
            Animator.gameObject.SetActive(false);
            _animJustStarted = false;
        }

        public override void ManualTick()
        {
            base.ManualTick();
            // 容器未完成时 Animator GameObject 是关闭的，对未初始化的 Animator 读状态会抛异常，必须先跳过；
            // 动画刚播放的那一帧 Animator 还没初始化，也要隔一帧再读
            if (!Animator.isActiveAndEnabled) return;
            if (_animJustStarted)
            {
                _animJustStarted = false;
                return;
            }

            AnimatorStateInfo state = Animator.GetCurrentAnimatorStateInfo(0);
            if (state.IsName("PackingTape_Close") && state.normalizedTime >= 0.95f)
            {
                IsCompleteCheckEnded = true;
            }
        }

        protected override void CheckComplete()
        {
            base.CheckComplete();
            if (IsFullSameColor && !IsBusy && !Animator.gameObject.activeSelf)
            {
                // 进入打包动画：先置回 false 阻塞通关，等动画播完再由 ManualTick 置回 true
                IsCompleteCheckEnded = false;
                _animJustStarted = true;
                CoffeeAwayGame.Instance?.Core?.RefreshAllRenderLayers();
                Animator.gameObject.SetActive(true);
                if (Info.Direction == LevelBuilder.HolderDirection.Down)
                {
                    Animator.transform.localRotation = Quaternion.Euler(0, 180, 0);
                }
                Animator.Play("PackingTape_Close");
                completeCheck.gameObject.SetActive(true);
                PlaySoundManager.Instance.PlaySound(Constant.Audio.CompleteHolder);
                VibrateManager.Instance.PlayOnce();
            }
        }
    }
}