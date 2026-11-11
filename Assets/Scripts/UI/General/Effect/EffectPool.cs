using UnityEngine;
using UnityEngine.UI;
using Coffee.UIExtensions;

namespace CasualGame
{
    public class EffectPool : BaseUnitObject
    {
        [SerializeField] private Image m_icon;
        [SerializeField] private UIParticle m_Trail, m_Bao;
        private ItemData _itemData;
        public Image Icon => m_icon;

        private AtlasLoader _atlasLoader = new AtlasLoader();

        private void OnDestroy()
        {
            _atlasLoader.Dispose();
        }

        public override void OnDeSpawn(PrefabPool ppm)
        {
            base.OnDeSpawn(ppm);
            _atlasLoader.Cancel();
        }

        public void ChangeSize(float size)
        {
            m_icon.GetComponent<RectTransform>().sizeDelta = new Vector2(size, size);
        }

        public void Play()
        {
            var self = GetComponent<ParticleSystem>();
            if (self != null)
                self.Play();
        }

        public void SetData(ItemData itemData)
        {
            _itemData = itemData;
            m_icon.gameObject.SetActive(true);
            m_icon.color = new Color(m_icon.color.r, m_icon.color.g, m_icon.color.b, 1f);
            // m_Trail.gameObject.SetActive(false);
            // m_Bao.gameObject.SetActive(false);
            _atlasLoader.LoadSprite(m_icon, _itemData.GetSpritePath());
        }

        public void PlayTrailParticle()
        {
            // m_Trail.gameObject.SetActive(true);
            // m_Trail.Play();
        }

        public void PlayBaoParticle()
        {
            // m_Trail.gameObject.SetActive(false);
            // m_Bao.gameObject.SetActive(true);
            // m_Bao.Play();
            m_icon.gameObject.SetActive(false);
        }
    }
}
