//------------------------------------------------------------
// File : CurrencyCell.cs
// Email: mailto:zhiqiang.yang
// Desc : 
//------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Threading;
using Coffee.UIExtensions;

namespace CasualGame
{
    public class RefreshItemEventArgs : CustomEventArgs
    {
        public int itemId;
    }

    public class CurrencyCell : AttachPoolPrefabBase, IEffectTarget
    {
        [SerializeField] private TextMeshProUGUI m_currenty, m_centerTxt;
        [SerializeField] public Image m_icon;
        [SerializeField] public Button m_addBtn;
        private Tweener tweener;
        private int m_type = (int)ItemType.None;
        private int currentCurrencyCount = 0;
        private AtlasLoader _atlasLoader;
        private float lastParticlePlayTime = -1f;
        public int CurrencyType => m_type;

        private void Awake()
        {
            EventManager.AddListener(Constant.Event.RefreshItem, RefreshItem);
        }

        private void OnDestroy()
        {
            EventManager.RemoveListener(Constant.Event.RefreshItem, RefreshItem);
        }

        public override void Spawn()
        {
            base.Spawn();
            m_addBtn.onClick.RemoveAllListeners();
            m_addBtn.onClick.AddListener(OnClickAddBtn);
        }

        public override void DeSpawn()
        {
            base.DeSpawn();
        }

        //货币变化
        private void RefreshItem(CustomEventArgs args)
        {
            if (args != null && args is RefreshItemEventArgs refreshItemEventArgs)
            {
                int itemId = refreshItemEventArgs.itemId;

                if (itemId == m_type)
                {
                    OnCurrencyChange();
                }
            }
        }

        private void OnClickAddBtn()
        {
            GameEntry.UI.OpenUIForm(UIFormType.GlobalToast, UGUIParams.Create().AddValue("Toast", GameEntry.Localization.GetString("57")));
        }

        private void RefreshAddBtn()
        {
            m_addBtn.gameObject.SetActive(false);
            if (m_type == (int)ItemType.Coin)
            {
                m_addBtn.gameObject.SetActive(currentCurrencyCount <= 0);
            }
        }

        private void Update()
        {
            switch (m_type)
            {
                case (int)ItemType.Energy:
                {
                    int timeStamp = EnergyManager.GetNextEnergyRecoverTime(out bool showFull);
                    if (timeStamp >= 0)
                        m_currenty.text = TimeUtils.TimeFormat(timeStamp, TimeFormatType.MM_SS, "{0}:{1}", true, false);
                    if (showFull)
                        m_currenty.text = "FULL";
                    break;
                }
            }

        }

        private void RefreshUI()
        {
            currentCurrencyCount = ProxyManager.UserProxy.ItemCount(m_type);
            ItemData itemData = new ItemData(m_type);
            m_centerTxt.gameObject.SetActive(m_type == (int)ItemType.Energy);
            if (m_type == (int)ItemType.Energy)
                m_centerTxt.text = currentCurrencyCount.ToString();
            else
                m_currenty.text = currentCurrencyCount.ToString();
            _atlasLoader.LoadSprite(m_icon, itemData.GetSpritePath());
        }

        private void OnCurrencyChange()
        {
            if (tweener != null && tweener.IsActive())
            {
                tweener.Kill(false);
                tweener = null;
            }

            int oldValue = currentCurrencyCount;
            int newValue = ProxyManager.UserProxy.ItemCount(m_type);

            if (!gameObject.activeSelf)
            {
                m_currenty.text = newValue.ToString("F0");
                currentCurrencyCount = newValue;
                return;
            }

            if (oldValue != newValue)
            {
                tweener = DOTween.To(() => oldValue, (int curValue) =>
                {
                    if (m_type == (int)ItemType.Energy)
                        m_centerTxt.text = curValue.ToString("F0");
                    else
                        m_currenty.text = curValue.ToString("F0");
                    currentCurrencyCount = curValue;
                    RefreshAddBtn();
                }, newValue, 0.45f).SetEase(Ease.Linear);
            }
        }

        public void SetData(int itemId, AtlasLoader atlasLoader)
        {
            _atlasLoader = atlasLoader;
            m_type = itemId;
            RefreshUI();
            RefreshAddBtn();
        }

        public void RefreshData()
        {
            if (m_type <= (int)ItemType.None) return;
            RefreshUI();
            RefreshAddBtn();
        }

        public void ClearData()
        {
            m_type = (int)ItemType.None;
            if (tweener.IsActive())
            {
                tweener.Kill(false);
                tweener = null;
            }
        }

        public void PlayParticle()
        {
            if (Time.time - lastParticlePlayTime < 0.1f)
            {
                return;
            }

            m_icon.transform.localScale = Vector3.one;
            m_icon.transform.DOKill();
            m_icon.transform.DOScale(1.2f, 0.1f).OnComplete(() =>
            {
                m_icon.transform.DOScale(1f, 0.1f);
            });
            lastParticlePlayTime = Time.time;
            EffectManager.PlayCurrencySound(m_type);
        }

        public Transform GetTransform()
        {
            return m_icon.transform;
        }
    }
}