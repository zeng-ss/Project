using DG.Tweening;
using UnityEngine;

namespace CasualGame.CoffeeAway
{
    public class RoadView : MonoBehaviour
    {
        [SerializeField] private Renderer roadRenderer;

        private Material _instanceMaterial;
        private Color _normalRoadColor;
        private Sequence m_Seq;
        private bool _isInRed;
        private readonly Color _redColor = new(0.8f, 0.2f, 0.3f);

        private void Awake()
        {
            _instanceMaterial = roadRenderer.materials[1];
            _normalRoadColor = _instanceMaterial.color;
        }

        // 让容器与道路的连接路段的路面材质槽共用道路的材质实例
        public void RegisterConnectionRenderer(Renderer renderer)
        {
            if (renderer is null) return;
            // 获取共享材质数组（不会创建副本）
            var sharedMats = renderer.sharedMaterials;
    
            // 替换索引 1 的材质
            if (sharedMats.Length > 1 && sharedMats[1] != _instanceMaterial)
            {
                sharedMats[1] = _instanceMaterial;
                renderer.sharedMaterials = sharedMats;
            }
        }

        public void SetRoadColor(bool isRed = false)
        {
            if (isRed && _isInRed) return;
            m_Seq?.Kill();
            _isInRed = isRed;
            _instanceMaterial.color = _normalRoadColor;
            if (isRed)
            {
                m_Seq = DOTween.Sequence();
                m_Seq.Append(_instanceMaterial.DOColor(_redColor, 0.7f)).SetLoops(-1, LoopType.Yoyo);
            }
        }

        private void OnDestroy()
        {
            // 销毁材质实例，避免内存泄漏
            if (_instanceMaterial != null)
            {
                Destroy(_instanceMaterial);
            }
        }
    }
}