// //------------------------------------------------------------
// // File : CasualCurrencyGroup.cs
// // Email: mailto:zhiqiang.yang
// // Desc : 
// //------------------------------------------------------------

// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Threading;
// using UnityEngine;
// using UnityEngine.UI;

// namespace CasualGame
// {
//     public class CasualCurrencyGroup : ActivatablePoolPrefabBase, ICurrencyGroup
//     {
//         private static int CasualAssetId = 12005;
//         private static CasualCurrencyGroup m_casualCurrencyGroup;
//         public static CasualCurrencyGroup GetCasualCurrencyGroup() => m_casualCurrencyGroup;

//         public static async void Spawn()
//         {
//             if (m_casualCurrencyGroup != null) return;

//             m_casualCurrencyGroup = await PrefabPool.Instance.Spawn<CasualCurrencyGroup>(CasualAssetId, new CancellationTokenSource().Token);
//             var group = GameEntry.UI.GetUIGroup("Loading");
//             m_casualCurrencyGroup?.InitialCurrency(((UGuiGroupHelper)group.Helper).transform, new List<ItemType> { ItemType.Energy, ItemType.Coin });//, ItemType.BuildingResource 
//         }

//         public static void CheckCurrencyGroup()
//         {
//             if (m_casualCurrencyGroup == null) return;

//             if (SceneHelper.IsGamePlayScene() || SceneHelper.IsHomeScene())
//             {
//                 bool hasDialogUI = GameEntry.UI.HasDialogUI();
//                 m_casualCurrencyGroup.SetCasualCurrencyGroup(hasDialogUI);
//             }
//         }

//         public static void ForceSetCurrencyGroupActive(bool active)
//         {
//             if (m_casualCurrencyGroup == null) return;

//             m_casualCurrencyGroup.SetCasualCurrencyGroup(active);
//         }

//         public static CurrencyCell GetGlobalCurrencyCell(int itemId)
//         {
//             if (m_casualCurrencyGroup == null) return null;

//             return m_casualCurrencyGroup.GetCurrencyCell(itemId);
//         }

//         [SerializeField] private AttachParentPrefabPool m_AttachParentPrefabPool;
//         [SerializeField] private Transform m_TempRewardLayout;

//         private readonly Vector2 spaing = new Vector2(34.1f, -40f);
//         private readonly float paddingLeft = 50f;

//         private Dictionary<int, CurrencyCell> m_CurrencyCellMap = new Dictionary<int, CurrencyCell>();
//         private RectTransform rectTransform => gameObject.GetComponent<RectTransform>();

//         private List<CurrencyCell> m_CurrencyCellList = new List<CurrencyCell>();
//         private AtlasLoader _atlasLoader = new AtlasLoader();

//         private void OnDestroy()
//         {
//             _atlasLoader.Dispose();
//         }

//         public override void OnSpawn(PrefabPool ppm)
//         {
//             base.OnSpawn(ppm);
//             gameObject.name = "CurrencyGroup";
//             m_CurrencyCellMap.Clear();
//         }

//         public override void OnDeSpawn(PrefabPool ppm)
//         {
//             base.OnDeSpawn(ppm);
//             foreach (var item in m_CurrencyCellMap)
//             {
//                 item.Value.ClearData();
//                 m_AttachParentPrefabPool.DeSpawn(item.Value);
//             }
//             m_CurrencyCellMap.Clear();
//             _atlasLoader.Cancel();
//         }

//         public void InitialCurrency(Transform canvas, List<ItemType> itemsType, Vector2 anchor = default)
//         {
//             List<int> itemsId = new List<int>();
//             itemsType.ForEach((item) => { itemsId.Add((int)item); });
//             InitialCurrency(canvas, itemsId, anchor);
//         }

//         private void RefreshSafeArea()
//         {
//             Rect safeArea = Screen.safeArea;
//             float screenWidth = Screen.width;
//             float screenHeight = Screen.height;
//             Vector2 anchorMin = new Vector2(safeArea.xMin / screenWidth, safeArea.yMin / screenHeight);
//             Vector2 anchorMax = new Vector2(safeArea.xMax / screenWidth, safeArea.yMax / screenHeight);
//             anchorMax.y = anchorMax.y >= 0.98f ? 0.98f : anchorMax.y;
//             rectTransform.anchorMin = anchorMin;
//             rectTransform.anchorMax = anchorMax;
//             rectTransform.sizeDelta = Vector2.zero;
//             rectTransform.anchoredPosition = Vector2.zero;
//         }

//         /// <summary>
//         /// 初始化货币信息
//         /// </summary>
//         /// <param name="itemsId"> 要展示的货币数量 </param>
//         /// <param name="usingSystemValue"> 是否使用系统值  false: 使用局部变量 </param>
//         private void InitialCurrency(Transform parent, List<int> itemsId, Vector2 anchor = default)
//         {
//             CachedTransform.SetParent(parent);
//             CachedTransform.localScale = Vector3.one;
//             CachedTransform.localPosition = Vector3.zero;
//             RefreshSafeArea();

//             for (int i = 0; i < itemsId.Count; i++)
//             {
//                 var slot = m_AttachParentPrefabPool.Spawn<CurrencyCell>(0, transform);
//                 slot.SetData(itemsId[i], _atlasLoader);
//                 slot.name = string.Format("Currency_{0}", itemsId[i]);

//                 RectTransform cellRect = slot.GetComponent<RectTransform>();
//                 int index = i;
//                 float x = cellRect.sizeDelta.x / 2 + paddingLeft + spaing.x * index + index * cellRect.sizeDelta.x;
//                 cellRect.anchorMin = cellRect.anchorMax = new Vector2(0, 1);
//                 cellRect.anchoredPosition = new Vector2(x, spaing.y) + anchor;
//                 m_CurrencyCellMap.Add(itemsId[i], slot);
//             }
//         }

//         public CurrencyCell GetCurrencyCell(int itemId)
//         {
//             if (m_CurrencyCellMap.ContainsKey(itemId))
//             {
//                 return m_CurrencyCellMap[itemId];
//             }

//             foreach (var item in m_CurrencyCellMap)
//             {
//                 if (item.Value.CurrencyType == itemId)
//                 {
//                     return item.Value;
//                 }
//             }

//             return null;
//         }

//         public Transform GetItemTransform(ItemType type)
//         {
//             var currencyCell = GetCurrencyCell((int)type);
//             if (currencyCell != null)
//                 return currencyCell.m_icon.transform;
//             return null;
//         }

//         private void SetCasualCurrencyGroup(bool active)
//         {
//             if (gameObject.activeSelf != active)
//             {
//                 gameObject.SetActive(active);
//             }
//         }
//     }
// }