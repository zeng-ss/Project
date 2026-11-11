using DG.Tweening;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace CasualGame.CoffeeAway
{
    /// <summary>
    /// 车辆表现模块
    /// </summary>
    public partial class Vehicle
    {
        [SerializeField] private ParticleSystem m_HideUnlock;
        [SerializeField] private GameObject m_OutLine;

        private Tween m_IdleTween;
        private bool _wantIdle; // 进场动画期间前置车辆也需呼吸，但呼吸必须等进入容器动画完成后再启动

        public void SetFront(bool active)
        {
            m_OutLine?.SetActive(active);
            _wantIdle = active;
            if (active)
            {
                // 仍在进场动画中（OnRoad/Entering）时只记录意愿，等状态切到 InHolder 再呼吸
                if (IsState(VehicleState.InHolder)) DoIdle();
            }
            else
            {
                StopIdle();
            }
        }

        private void DoIdle()
        {
            if (m_IdleTween != null && m_IdleTween.IsActive()) return;
            m_IdleTween = transform.DOScale(1.05f, 1f).SetLoops(-1, LoopType.Yoyo);
        }

        private void StopIdle()
        {
            if (m_IdleTween == null) return;
            m_IdleTween.Kill();
            m_IdleTween = null;
            // 呼吸动画被中断（如容器完成）时，把缩放平滑还原到 1，避免停在中间值
            if (!Mathf.Approximately(transform.localScale.x, 1f))
            {
                transform.DOScale(Vector3.one, 0.05f).SetEase(Ease.OutQuad);
            }
        }

        // 状态机切到 InHolder 时由 ChangeState 回调：进场动画完成后再启动呼吸
        private void StartIdleIfWanted()
        {
            if (_wantIdle && IsState(VehicleState.InHolder)) DoIdle();
        }

        // 缩放消失动画，播完回调回收
        public void DoScaleAni()
        {
            transform.DOKill(true);
            transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() =>
            {
                SpawnFactory.VehiclePool.DeSpawn(this);
            });
        }

        public void RefreshRenderLayer() => ApplyRenderLayer();

        private void ApplyRenderLayer()
        {
            int layer = CoffeeAwayGame.Instance?.Core?.GetVehicleRenderLayer(this)
                        ?? Constant.Layer.DefaultLayerId;
            gameObject.SetLayerRecursively(layer);
        }

        public void ShowUnlock()
        {
            if (m_HideUnlock == null)
            {
                Log.Error("Hide unlock component is invalid.");
                return;
            }

            m_HideUnlock.gameObject.SetActive(true);
            m_HideUnlock.Play();
        }

        private void ResetView()
        {
            StopIdle();
            _wantIdle = false;
            transform.DOKill(true);
            transform.localScale = Vector3.one;
            m_HideUnlock?.gameObject.SetActive(false);
        }
    }
}