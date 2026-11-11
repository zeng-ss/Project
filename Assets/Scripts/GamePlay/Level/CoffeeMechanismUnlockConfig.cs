using System;
using System.Collections.Generic;
using UnityEngine;

namespace CasualGame.CoffeeAway
{
    /// <summary>
    /// 由编辑器扫描关卡 JSON 生成的机制首次出现配置。
    /// </summary>
    [CreateAssetMenu(
        fileName = "CoffeeMechanismUnlockConfig",
        menuName = "CoffeeSort/Mechanism Unlock Config")]
    public sealed class CoffeeMechanismUnlockConfig : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            [SerializeField] private ItemType m_Mechanism;
            [SerializeField, Min(1)] private int m_FirstProgressLevel;

            public ItemType Mechanism => m_Mechanism;
            public int FirstProgressLevel => m_FirstProgressLevel;

            public Entry(ItemType mechanism, int firstProgressLevel, int contentId)
            {
                m_Mechanism = mechanism;
                m_FirstProgressLevel = firstProgressLevel;
            }
        }

        [SerializeField] private List<Entry> m_Entries = new();

        public IReadOnlyList<Entry> Entries => m_Entries;

        public bool TryGetFirstProgressLevel(ItemType mechanism, out int level)
        {
            for (int i = 0; i < m_Entries.Count; i++)
            {
                if (m_Entries[i].Mechanism != mechanism)
                {
                    continue;
                }

                level = m_Entries[i].FirstProgressLevel;
                return true;
            }

            level = 0;
            return false;
        }

#if UNITY_EDITOR
        public void ReplaceGeneratedEntries(IReadOnlyList<Entry> entries)
        {
            m_Entries.Clear();
            if (entries != null)
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    m_Entries.Add(entries[i]);
                }
            }

            m_Entries.Sort((left, right) =>
            {
                int levelCompare = left.FirstProgressLevel.CompareTo(right.FirstProgressLevel);
                return levelCompare != 0
                    ? levelCompare
                    : left.Mechanism.CompareTo(right.Mechanism);
            });
        }
#endif
    }
}
