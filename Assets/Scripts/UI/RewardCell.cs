using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CasualGame
{
    public class RewardCell : AttachPoolPrefabBase
    {
        [SerializeField] private Image m_Icon;
        [SerializeField] private TextMeshProUGUI m_Count;

        public void SetData(ItemData data, AtlasLoader atlasLoader)
        {
            if (m_Icon == null)
            {
                m_Icon = GetComponent<Image>();
            }
            if (m_Count == null)
            {
                m_Count = GetComponentInChildren<TextMeshProUGUI>();
            }
            m_Count.text = data.StyleNum();
            if (data.id == (int)ItemType.Coin)
            {
                m_Icon.rectTransform.sizeDelta = new Vector2(270, 150);
                atlasLoader.LoadSprite(m_Icon, data.GetSpritePath(1));
            }
            else
            {
                atlasLoader.LoadSprite(m_Icon, data.GetSpritePath());
            }
        }
    }
}