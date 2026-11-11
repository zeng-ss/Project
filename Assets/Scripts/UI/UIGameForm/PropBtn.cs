using CasualGame;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using CasualGame.CoffeeAway;

[RequireComponent(typeof(Button))]
public class PropBtn : AttachPoolPrefabBase
{
    [SerializeField] private Image m_AddIcon;

    [SerializeField] private TextMeshProUGUI m_CountText;
    [SerializeField] private RectTransform m_RedParent;
    [SerializeField] private Image m_Icon, m_LockIcon;

    private ItemType m_ItemType;
    private ItemData m_ItemData;
    private Button m_Button;
    private bool m_IsLocked;
    private int m_UnlockLevel;
    public void Init(UIGameForm uIGameForm, ItemType itemType, AtlasLoader atlasLoader)
    {
        m_ItemType = itemType;
        m_ItemData = new ItemData((int)itemType);
        atlasLoader.LoadSprite(m_Icon, m_ItemData.GetSpritePath());
        RefreshUI();
    }

    private void RefreshUI()
    {
        m_UnlockLevel = m_ItemType switch
        {
            ItemType.Undo => GameConfigMgr.Instance.GetInt(Constant.Config.PropUnlock11, 9),
            ItemType.Shuffle => GameConfigMgr.Instance.GetInt(Constant.Config.PropUnlock12, 12),
            ItemType.ExpandRoad => GameConfigMgr.Instance.GetInt(Constant.Config.PropUnlock13, 15),
            ItemType.EliminateColor => GameConfigMgr.Instance.GetInt(Constant.Config.PropUnlock14, 18),
            _ => 0
        };
        bool hasItem = ProxyManager.UserProxy.ItemCount((int)m_ItemType) > 0;
        m_IsLocked = ProxyManager.UserProxy.GameLevel + 1 < m_UnlockLevel;
        m_LockIcon.GetComponentInChildren<TextMeshProUGUI>().text = "Lv." + m_UnlockLevel;
        m_LockIcon.gameObject.SetActive(m_IsLocked);
        // m_LockIcon.GetComponentInChildren<TextMeshProUGUI>().text = "Lv." + unlockLevel;
        // m_LockIcon.gameObject.SetActive(m_IsLocked);
        m_AddIcon.gameObject.SetActive(!hasItem && !m_IsLocked);
        m_RedParent.gameObject.SetActive(hasItem);
        m_CountText.text = ProxyManager.UserProxy.ItemCount((int)m_ItemType).ToString();
    }

    private void Awake()
    {
        m_Button = GetComponent<Button>();
        m_Button.onClick.RemoveAllListeners();
        m_Button.onClick.AddListener(() => { ItemBtnClick(m_ItemData); });

        EventManager.AddListener(Constant.Event.RefreshItem, RefreshItemCount);
    }

    // 刷新道具的数量显示 外部事件触发调用 参数传对应道具 id
    private void RefreshItemCount(CustomEventArgs eventArgs)
    {
        if (eventArgs is not RefreshItemEventArgs e) return;

        RefreshUI();
    }

    // 道具被点击时执行的方法
    private void ItemBtnClick(ItemData itemData)
    {
        if (m_IsLocked)
        {
            Framework.UIMgr.Instance.OpenUIForm(UIFormType.GlobalToast,
                UGUIParams.Create().AddValue("Toast",
                    String.Format(LocalizationMgr.Instance.GetString("66").ToString(), m_UnlockLevel)));
            return;
        }
        PropMgr propMgr = CoffeeAwayGame.Instance.PropMgr;
        if (propMgr == null) return;

        // 正在选择道具目标时不允许再次点击道具按钮
        if (propMgr.IsSelectingTarget) return;
        if (m_IsLocked) return;

        PlaySoundManager.Instance.PlaySound(Constant.Audio.ClickButton);

        if (ProxyManager.UserProxy.ItemCount(itemData.id) > 0)
        {
            if (propMgr.UsePropByType((ItemType)itemData.id))
            {
                CoffeeAwayGame.Instance.ShouldBackReduceEnergy();
                ProxyManager.UserProxy.CostItem(itemData.id, 1);
                RefreshUI();
            }
        }
        else
        {
            UGUIParams @params = UGUIParams.Create();
            @params.AddValue("ItemType", (ItemType)itemData.id);
            @params.AddValue("TitleText", itemData.id);
            @params.AddValue("Icon", transform);
            Framework.UIMgr.Instance.OpenUIForm(UIFormType.GetItemForm, @params);
        }
    }

    // public void OpenNewItem(Camera camera)
    // {
    //     Vector2 screenPoint = TransformUtility.WorldToScreenPoint(camera, transform.position);
    //     TransformUtility.ScreenPointToWorldPointInRectangle(
    //         Framework.UIMgr.Instance.MainCanvas.GetComponent<RectTransform>(), screenPoint, out Vector3 uiWorldPoint);
    //     UGUIParams uiParams = UGUIParams.Create().AddValue("unlockType", itemType).AddValue("Target", uiWorldPoint)
    //         .AddValue("Icon", transform);
    //     Framework.UIMgr.Instance.OpenUIForm(UIFormType.NewItemDialog, uiParams);
    // }


    void OnDestroy()
    {
        EventManager.RemoveListener(Constant.Event.RefreshItem, RefreshItemCount);
    }

    // public static int GetName(int id)
    // {
    //     switch ((ItemType)id)
    //     {
    //         case ItemType.RemoveUnit:
    //             return 2;
    //         case ItemType.ReverseDirection:
    //             return 3;
    //         case ItemType.ReverseSingleDirection:
    //             return 4;
    //     }

    //     return 611;
    // }


    // public static string GetPropDes(ItemType itemType)
    // {
    //     switch (itemType)
    //     {
    //         case ItemType.RemoveUnit:
    //             return "61";
    //         case ItemType.ReverseDirection:
    //             return "65";
    //         case ItemType.ReverseSingleDirection:
    //             return "62";
    //     }
    //     return "";
    // }
}