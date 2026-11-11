using UnityEngine;

namespace CasualGame.CoffeeAway
{
    /// <summary>
    /// BgCamera / HighlightCamera 跟随 Main Camera 的位置与正交参数。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public class FollowMainCamera : MonoBehaviour
    {
        [SerializeField] private Camera _mainCamera;

        private Camera _self;

        private void Awake()
        {
            _self = GetComponent<Camera>();
            if (_mainCamera == null)
                _mainCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (_mainCamera == null || _self == null) return;

            transform.SetPositionAndRotation(_mainCamera.transform.position, _mainCamera.transform.rotation);
            _self.orthographic = _mainCamera.orthographic;
            _self.orthographicSize = _mainCamera.orthographicSize;
            _self.nearClipPlane = _mainCamera.nearClipPlane;
            _self.farClipPlane = _mainCamera.farClipPlane;
            _self.fieldOfView = _mainCamera.fieldOfView;
        }
    }
}
