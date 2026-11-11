//------------------------------------------------------------
// File : CasualCurrencyGroup.cs
// Email: mailto:zhiqiang.yang
// Desc : 
//------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace CasualGame
{
    public class CasualCurrencyGroup : ActivatablePoolPrefabBase, ICurrencyGroup
    {
        private static int CasualAssetId = 12005;
        private static CasualCurrencyGroup m_casualCurrencyGroup;
        public static CasualCurrencyGroup GetCasualCurrencyGroup() => m_casualCurrencyGroup;

        public static async void OnDepthChanged(IShowMaskInterface showMaskInterface)
        {
            if (m_casualCurrencyGroup == null)
            {
                m_casualCurrencyGroup = await PrefabPool.Instance.Spawn<CasualCurrencyGroup>(CasualAssetId, new CancellationTokenSource().Token);
                m_casualCurrencyGroup?.InitialCurrency(GameEntry.UI.MainCanvas.transform, new List<ItemType> { ItemType.Energy, ItemType.Coin });
            }
            if (!showMaskInterface.ShowCurrency)
                return;
            m_casualCurrencyGroup.AddInterface(showMaskInterface);
        }

        public static void CloseUI(IShowMaskInterface showMaskInterface)
        {
            if (!showMaskInterface.ShowCurrency)
                return;
            m_casualCurrencyGroup.RemoveInterface(showMaskInterface);
        }

        [SerializeField] private CurrencyCell m_Coin, m_Heart;
        private Dictionary<int, CurrencyCell> m_CurrencyCellMap = new Dictionary<int, CurrencyCell>();
        private RectTransform rectTransform => gameObject.GetComponent<RectTransform>();
        private AtlasLoader _atlasLoader = new AtlasLoader();
        private Canvas m_CachedCanvas = null;

        private void OnDestroy()
        {
            _atlasLoader.Dispose();
        }

        public override void OnSpawn(PrefabPool ppm)
        {
            base.OnSpawn(ppm);
            gameObject.name = "CurrencyGroup";
            m_CurrencyCellMap = new Dictionary<int, CurrencyCell>();
            m_Coin.SetData((int)ItemType.Coin, _atlasLoader);
            m_Heart.SetData((int)ItemType.Energy, _atlasLoader);
            m_CurrencyCellMap[(int)ItemType.Energy] = m_Heart;
            m_CurrencyCellMap[(int)ItemType.Coin] = m_Coin;
        }

        public override void OnDeSpawn(PrefabPool ppm)
        {
            base.OnDeSpawn(ppm);
            foreach (var item in m_CurrencyCellMap)
            {
                item.Value.ClearData();
            }
            m_CurrencyCellMap.Clear();
            _atlasLoader.Cancel();
        }

        public void InitialCurrency(Transform canvas, List<ItemType> itemsType, Vector2 anchor = default)
        {
            List<int> itemsId = new List<int>();
            itemsType.ForEach((item) => { itemsId.Add((int)item); });
            InitialCurrency(canvas, itemsId, anchor);
        }

        private void RefreshSafeArea()
        {
            Rect safeArea = Screen.safeArea;
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            Vector2 anchorMin = new Vector2(safeArea.xMin / screenWidth, safeArea.yMin / screenHeight);
            Vector2 anchorMax = new Vector2(safeArea.xMax / screenWidth, safeArea.yMax / screenHeight);
            anchorMax.y = anchorMax.y >= 0.98f ? 0.98f : anchorMax.y;
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
        }

        /// <summary>
        /// 初始化货币信息
        /// </summary>
        /// <param name="itemsId"> 要展示的货币数量 </param>
        /// <param name="usingSystemValue"> 是否使用系统值  false: 使用局部变量 </param>
        private void InitialCurrency(Transform parent, List<int> itemsId, Vector2 anchor = default)
        {
            CachedTransform.SetParent(parent);
            CachedTransform.localScale = Vector3.one;
            CachedTransform.localPosition = Vector3.zero;
            RefreshSafeArea();

            m_CachedCanvas = gameObject.GetOrAddComponent<Canvas>();
            gameObject.GetOrAddComponent<GraphicRaycaster>();
            gameObject.layer = LayerMask.NameToLayer("UI");
            SetDepth(0);
        }

        public CurrencyCell GetCurrencyCell(int itemId)
        {
            if (m_CurrencyCellMap.ContainsKey(itemId))
            {
                return m_CurrencyCellMap[itemId];
            }

            foreach (var item in m_CurrencyCellMap)
            {
                if (item.Value.CurrencyType == itemId)
                {
                    return item.Value;
                }
            }

            return null;
        }

        public Transform GetItemTransform(ItemType type)
        {
            var currencyCell = GetCurrencyCell((int)type);
            if (currencyCell != null)
                return currencyCell.m_icon.transform;
            return null;
        }


        private List<IShowMaskInterface> m_ShowMaskInterfaces = new List<IShowMaskInterface>();

        private void AddInterface(IShowMaskInterface showMaskInterface)
        {
            if (!m_ShowMaskInterfaces.Contains(showMaskInterface))
            {
                m_ShowMaskInterfaces.Add(showMaskInterface);
            }

            CheckDepth();
        }

        private void RemoveInterface(IShowMaskInterface showMaskInterface)
        {
            if (!m_ShowMaskInterfaces.Contains(showMaskInterface))
            {
                return;
            }

            m_ShowMaskInterfaces.Remove(showMaskInterface);
            CheckDepth();
        }

        private void CheckDepth()
        {
            if (m_ShowMaskInterfaces.Count <= 0)
            {
                SetDepth(0);
                return;
            }

            int maxDepth = 0;
            for (int i = m_ShowMaskInterfaces.Count - 1; i >= 0; i--)
            {
                int currentDepath = m_ShowMaskInterfaces[i].MaskDepth + 1;
                if (currentDepath > maxDepth)
                {
                    maxDepth = currentDepath;
                }
            }

            SetDepth(maxDepth);
        }

        private void SetDepth(int depth)
        {
            gameObject.SetActive(depth > 0);
            m_CachedCanvas.overrideSorting = true;
            m_CachedCanvas.sortingOrder = depth;
        }
    }
}