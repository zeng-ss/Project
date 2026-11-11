//------------------------------------------------------------
// File : UIEffectLoader.cs
// Email: mailto:zhiqiang.yang
// Desc : UI 例子特效加载器
//------------------------------------------------------------
using System.Threading;
using UnityEngine;

namespace CasualGame
{
    public class UIEffectLoader : MonoBehaviour
    {

        // public int m_EffectAssetId;

        public GameObject m_EffectTemp;

        public bool m_AutoStart = true;

        public int m_Order = 0;

        public bool m_ScreenAdaptation = false;

        public float m_BaseScale = 1.0f;

        // [CustomLabel("粒子特效的局部欧拉")]
        public Vector3 m_EulerAngles = Vector3.zero;

        public Transform MainPartical { get; private set; }
        public UIDepth EffectDepth { get; private set; }
        public UIParticleScale ParticleScale { get; private set; }
        public bool Loaded { get; private set; }
        private ParticleSystem _particleSystem;

        private void Start()
        {
            if (m_AutoStart)
                LoadEffect();
        }

        private void LoadEffect()
        {
            if (Loaded) return;

            Transform trans = null;
            if (m_EffectTemp != null)
            {
                trans = m_EffectTemp.transform;
            }
            // else
            // {
            //      var collectItem = await PrefabPool.Instance.Spawn<GameObject>(m_EffectAssetId, new CancellationTokenSource().Token);
            //     // trans = ((GameObject)Framework.GameRoot.Res.LoadAssetSync<GameObject>(m_EffectAssetId)).transform;
            // }

            if (trans == null)
            {
                Loaded = false;
                // Log.Error("Load Effect Fail,EffectAssetId {0} Or Reference is null  parent name={1}", m_EffectAssetId, transform.parent.name);
                return;
            }

            MainPartical = CUtility.Mono.Instantiate<Transform>(trans, transform);
            _particleSystem = MainPartical.GetComponent<ParticleSystem>();
            Loaded = true;
            MainPartical.gameObject.SetLayerRecursively(gameObject.layer);
            MainPartical.localEulerAngles = m_EulerAngles;

            EffectDepth = MainPartical.gameObject.GetOrAddComponent<UIDepth>();
            EffectDepth.order = m_Order;
            EffectDepth.isUI = false;

            EffectDepth.Refresh();

            if (m_ScreenAdaptation)
            {
                ParticleScale = MainPartical.gameObject.GetOrAddComponent<UIParticleScale>();
            }
            else
            {
                MainPartical.transform.localScale = MainPartical.transform.localScale * m_BaseScale;
            }
        }

        public void ClearEffect()
        {
            if (MainPartical != null)
            {
                CUtility.Mono.Destroy(MainPartical.gameObject);
            }
            MainPartical = null;
            EffectDepth = null;
            ParticleScale = null;
            Loaded = false;
            _particleSystem = null;
        }

        /// <summary>
        /// 重新切换粒子特效
        /// </summary>
        /// <param name="effectId"></param>
        public void ChangeEffect(int effectId)
        {
            ClearEffect();
            // m_EffectAssetId = effectId;
            LoadEffect();
        }

        public void Play()
        {
            LoadEffect();
            if (_particleSystem != null)
            {
                _particleSystem.Play();
            }
        }

        public void Stop()
        {
            if (_particleSystem != null)
            {
                _particleSystem.Stop();
            }
        }

        public void StopEffect()
        {
            MainPartical.GetComponent<ParticleSystem>().Stop();
        }

        /// <summary>
        /// 重新加载例子特效
        /// </summary>
        [ContextMenu("Reload")]
        public void Reload()
        {
            ClearEffect();
            LoadEffect();
        }
    }
}


