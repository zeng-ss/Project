using UnityEngine;

namespace CasualGame.CoffeeAway
{
    public class RoadSurfaceMove : AttachPoolPrefabBase
    {
        private RoadNavigator _navigator;
        public float _roadDistance; // 当前中心线距离
        private const float RoadSurfaceSpeed = 5f; // 路面移动速度
        private float _curspeed;
        private float _surfaceY;

        public void Init(RoadNavigator navigator, float startDist, float surfaceY)
        {
            _curspeed = RoadSurfaceSpeed;
            _navigator = navigator;
            _roadDistance = startDist;
            _surfaceY = surfaceY;
            transform.localScale = Vector3.one * 0.2f;
        }

        public void Stop() => _curspeed = 0;
        public void Move() => _curspeed = RoadSurfaceSpeed;

        private void Update()
        {
            if (_navigator == null) return;
            _roadDistance = _navigator.AdvanceDistance(_roadDistance, _curspeed * Time.deltaTime);
            transform.position = _navigator.GetPosition(_roadDistance) + Vector3.up * _surfaceY;
            var dir = _navigator.GetDirection(_roadDistance);
            // 面片预制体：+X=箭头(道路长方向)，+Z=法线(上)。
            // LookRotation 的 forward 用世界 +Y，upwards 用垂直于道路的水平向量，
            // 结果 = 面片 +Z 朝世界 +Y、+X 箭头朝道路方向 dir。
            if (dir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(Vector3.up, Vector3.Cross(Vector3.up, dir));
        }

        public override void DeSpawn()
        {
            _navigator = null;
            _curspeed = 0f;
            _surfaceY = 0f;
        }
    }
}