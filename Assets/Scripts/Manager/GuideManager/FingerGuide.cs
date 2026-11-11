using System;
using UnityEngine;
using Spine.Unity;
using DG.Tweening;

namespace CasualGame
{
    public class FingerGuide : ActivatablePoolPrefabBase
    {
        [SerializeField] private GameObject skeletonGraphic;
        private Tweener _tweener;
        private bool _isForce;
        private Vector2 _targetGuidePosition;
        private Transform _targetTransform;
        public Action DisableAction;

        private Bounds _debugBounds;

        private void OnDisable()
        {
            gameObject.SetActive(false);

            DisableAction?.Invoke();
            DisableAction = null;

            _targetTransform = null;
        }

        public void SetTarget(Transform targetTransform, Vector2? pivot = null, bool force = false,
            Camera targetCamera = null)
        {
            if (_targetTransform == targetTransform) return;
            _targetTransform = targetTransform;

            _isForce = force;

            Bounds bounds = BoundTool.GetBounds(targetTransform, targetTransform, out int result);
            _debugBounds = bounds;
            Vector2 worldMin = targetTransform.TransformPoint(bounds.min);
            Vector2 worldSize = bounds.size;
            Vector2 lossyScale = targetTransform.lossyScale;
            worldSize.x *= lossyScale.x;
            worldSize.y *= lossyScale.y;

            if (pivot != null && pivot.Value != Vector2.one * -1)
            {
                _targetGuidePosition = worldMin + worldSize * pivot.Value;
            }
            else
            {
                _targetGuidePosition = targetTransform.position;
            }

            if (targetCamera == null)
            {
                targetCamera = GameEntry.UI.MainCanvas.worldCamera;
            }

            Vector2 screenPoint =
                TransformUtility.WorldToScreenPoint(targetCamera, _targetGuidePosition);

            CachedTransform.SetParent(targetTransform);
            CUtility.Mono.SetIdentity(CachedTransform);
            CachedTransform.position = _targetGuidePosition;
            CachedTransform.eulerAngles =
                Vector3.up * (180 * (screenPoint.x >= GameEntry.UI.MainCanvas.pixelRect.width - 120f ? 1 : 0));

            skeletonGraphic.GetComponent<IAnimationStateComponent>().AnimationState.SetAnimation(0, "click", true);
        }

        public void SetTarget3D(Transform targetTransform, bool force = false, Camera targetCamera = null)
        {
            if (_targetTransform == targetTransform) return;
            _targetTransform = targetTransform;

            _isForce = force;

            Bounds bounds = BoundTool.GetBounds(targetTransform, targetTransform, out int result);
            _debugBounds = bounds;
            CachedTransform.SetParent(targetTransform, true);
            CUtility.Mono.SetIdentity(CachedTransform);
            CachedTransform.eulerAngles = targetCamera.transform.eulerAngles;
            CachedTransform.position = _targetTransform.position + CachedTransform.forward * -3f;
            skeletonGraphic.GetComponent<IAnimationStateComponent>().AnimationState.SetAnimation(0, "click", true);
        }


#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_targetTransform == null) return;

            Gizmos.color = Color.red;
            Gizmos.matrix = _targetTransform.localToWorldMatrix;
            Gizmos.DrawWireCube(_debugBounds.center, _debugBounds.size);
        }
#endif

        private void Update()
        {
            if (_isForce)
            {
                if (!skeletonGraphic.gameObject.activeInHierarchy)
                {
                    skeletonGraphic.gameObject.SetActive(true);
                }
            }
        }

        public override void OnDeSpawn(PrefabPool ppm)
        {
            base.OnDeSpawn(ppm);

            if (_tweener.IsActive())
            {
                _tweener.Kill(false);
            }
        }
    }
}