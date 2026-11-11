using System;
using DG.Tweening;
using UnityEngine;

namespace CasualGame.CoffeeAway
{
    /// <summary>
    /// 咖啡移动模块
    /// </summary>
    public partial class Vehicle
    {
        private RoadNavigator _navigator;
        private float _speed;
        private Sequence m_Seq;

        private float _roadDistance; // 当前中心线距离
        private float _prevRoadDistance; // 上一步中心线距离
        private float? _occupyDistance;


        // 预约道路位置：排队等待移出
        public void ReserveRoadPosition(float distance)
        {
            _occupyDistance = distance;
            ChangeState(VehicleState.WaitingExit);
        }

        #region 移出/进入容器

        // 移出容器
        public void ExitHolder(RoadNavigator navigator, float startDistance, Vector3 roadPos, Vector3 roadDir,
            float exitDuration, float moveSpeed, Action onComplete)
        {
            if (!IsState(VehicleState.WaitingExit)) return;

            _navigator = navigator;
            _roadDistance = startDistance;
            _prevRoadDistance = startDistance;
            _speed = moveSpeed;
            _occupyDistance = null;
            ChangeState(VehicleState.Exiting);
            PlaySoundManager.Instance.PlaySound(Constant.Audio.CoffeeGoOut);

            m_Seq?.Kill(true);
            m_Seq = DOTween.Sequence();
            var targetRot = Quaternion.LookRotation(roadDir, Vector3.up);
            PlayJump(roadPos, targetRot, exitDuration);
            m_Seq.AppendCallback(() =>
            {
                ChangeState(VehicleState.OnRoad);
                PlaySoundManager.Instance.PlaySound(Constant.Audio.Land);
                onComplete?.Invoke(); // 落地就从 Holder Grid 移除
            });
            PlayLand();
        }

        // 进入容器
        public void EnterHolder(Vector3 targetPos, Quaternion targetRot)
        {
            if (!IsState(VehicleState.OnRoad)) return;
            ChangeState(VehicleState.Entering);
            PlaySoundManager.Instance.PlaySound(Constant.Audio.CoffeeGoIn);
            m_Seq?.Kill(true);
            m_Seq = DOTween.Sequence();
            PlayJump(targetPos, targetRot, 0.15f);
            PlayLand();
            m_Seq.OnComplete(() => ChangeState(VehicleState.InHolder));
        }

        private void PlayJump(Vector3 targetPos, Quaternion targetRot, float duration)
        {
            m_Seq.Append(transform.DOJump(targetPos, 5f, 1, duration).SetEase(Ease.OutQuad));
            m_Seq.Join(transform.DORotateQuaternion(targetRot, duration).SetEase(Ease.OutCubic));
            m_Seq.Join(transform.DOScaleY(0.7f, duration).SetEase(Ease.OutQuad));
            /*
             if (isEnter)
            {
                m_Seq.Join(transform.DORotateQuaternion(targetRot, duration).SetEase(Ease.OutCubic));
            }
            else
                m_Seq.Join(transform.DOLocalRotate(new Vector3(360f, 0, 0), duration, RotateMode.LocalAxisAdd));
            */
        }

        private void PlayLand()
        {
            m_Seq.Append(transform.DOScale(new Vector3(1.3f, 0.7f, 1.3f), 0.1f).SetEase(Ease.InQuad));
            m_Seq.Append(transform.DOScale(new Vector3(0.985f, 1.03f, 0.985f), 0.1f).SetEase(Ease.OutQuad));
            m_Seq.Append(transform.DOScale(Vector3.one, 0.1f).SetEase(Ease.OutBack));
        }

        #endregion

        // 传送
        public void StartTeleport(float newDistance, Vector3 newPos, Vector3 newDir)
        {
            _roadDistance = newDistance;
            // 保留 _prevRoadDistance：传送门把车从终点跳到起点时，本帧 CheckHolderEntry 仍需用原环绕段
            // 检测"传送门起点位置"的容器入口穿越；若这里重置为 newDistance，那段入口会永久丢失，车永远进不去
            transform.position = newPos;
            transform.rotation = Quaternion.LookRotation(newDir, Vector3.up);
            ApplyRenderLayer();
        }

        public void MoveTo(Vector3 position, Quaternion rotation, float duration, Action onComplete = null)
        {
            m_Seq?.Kill(true);
            m_Seq = DOTween.Sequence();

            float half = duration * 0.5f;
            m_Seq.Append(transform.DOScale(Vector3.zero, half).SetEase(Ease.InBack));
            m_Seq.AppendCallback(() => transform.SetPositionAndRotation(position, rotation));
            m_Seq.Append(transform.DOScale(Vector3.one, half).SetEase(Ease.OutBack));
            m_Seq.OnComplete(() => onComplete?.Invoke());
        }

        public void SetMoveSpeed(float speed) => _speed = speed;

        // 冻结/恢复动画：道具选目标期间暂停所有进出场/移动序列，恢复后继续执行
        public void PauseAnim()
        {
            if (m_Seq != null && m_Seq.IsActive()) m_Seq.Pause();
        }

        public void ResumeAnim()
        {
            if (m_Seq != null && m_Seq.IsActive()) m_Seq.Play();
        }

        // 道路驱动：仅 OnRoad；跟车减速由 _speed 控制，IsBlocked 不拦截移动。由 VehicleManager 统一调用
        public void ManualTick(float deltaTime)
        {
            if (!IsState(VehicleState.OnRoad) || _navigator == null) return;

            _prevRoadDistance = _roadDistance;
            _roadDistance = _navigator.AdvanceDistance(_roadDistance, _speed * deltaTime);
            transform.position = _navigator.GetPosition(_roadDistance);
            var dir = _navigator.GetDirection(_roadDistance);
            if (dir != Vector3.zero) transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }

        // 道具使用：终止道路行驶或排队状态
        public void CancelRoadState()
        {
            transform.DOKill(true);
            _navigator = null;
            _occupyDistance = null;
            ExitedFromHolder = null;
            OriginHolder = null;
            IsBlocked = false;
        }

        // 对象回收时复位移动数据
        public void Reset()
        {
            m_Seq?.Kill(true);
            transform.DOKill(true);
            _navigator = null;
            _occupyDistance = null;
            ExitedFromHolder = null;
            OriginHolder = null;
            IsBlocked = false;
        }

        #region Uodo

        /// <summary>Undo：清道路/动画状态，保持当前位置。</summary>
        public void PrepareForUndoExit()
        {
            m_Seq?.Kill(true);
            transform.DOKill(true);
            _navigator = null;
            _occupyDistance = null;
            ExitedFromHolder = null;
            OriginHolder = null;
            IsBlocked = false;
        }

        /// <summary>Undo：进入源 holder 前切到 OnRoad，从当前位置播进场动画。</summary>
        public void PrepareForUndoEnter()
        {
            _navigator = null;
            _occupyDistance = null;
            ExitedFromHolder = null;
            OriginHolder = null;
            IsBlocked = false;
            ChangeState(VehicleState.OnRoad);
        }

        /// <summary>Undo：从当前位置跳进 holder（进场动画）。</summary>
        public void PlayUndoEnterHolder(Vector3 targetPos, Quaternion targetRot, Action onComplete)
        {
            m_Seq?.Kill(true);
            ChangeState(VehicleState.Entering);
            m_Seq = DOTween.Sequence();
            PlayJump(targetPos, targetRot, 0.15f);
            PlayLand();
            m_Seq.OnComplete(() =>
            {
                ChangeState(VehicleState.InHolder);
                onComplete?.Invoke();
            });
        }

        #endregion
    }
}