using UnityEngine;

namespace CasualGame
{
    public class EffectCell : ActivatablePoolPrefabBase
    {
        [SerializeField] private float delayTime = 1;

        private ParticleSystem[] m_particleSystem;

        public override void OnSpawn(PrefabPool ppm)
        {
            base.OnSpawn(ppm);
            var autoDestory = gameObject.GetOrAddComponent<AutoDestroy>();
            autoDestory.delayTime = delayTime;
            autoDestory.Reset();
        }

        public virtual void Play()
        {
            m_particleSystem = GetComponentsInChildren<ParticleSystem>();
            for (int i = 0; i < m_particleSystem.Length; i++)
            {
                if (m_particleSystem[i] != null)
                {
                    m_particleSystem[i].Play();
                }
            }
        }

    }
}
