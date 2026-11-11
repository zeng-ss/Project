using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotList<T> where T : MonoBehaviour
{
    public T prefab;
    public Transform parent;

    public int Count { get; private set; }

    protected List<T> slots;

    /// <summary>
    /// 初始化列表
    /// </summary>
    /// <param name="hidePrefab"></param>
    /// <param name="useChildCom">子物体必须包含组件T</param>
    public void Init(T prefab, Transform parent, bool hidePrefab = false, bool useChildCom = true)
    {
        this.prefab = prefab;
        this.parent = parent;
        if (hidePrefab) prefab.gameObject.SetActive(false);

        slots = new List<T>();
        if (useChildCom)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform tran = parent.GetChild(i);

                T comp = tran.GetComponent<T>();
                if (comp != null)
                {
                    slots.Add(comp);
                }
            }
        }
    }

    public void SetCount(int count)
    {
        for (int i = 0; i < count; i++)
        {
            T slot = GetSlot(i);
            if (slot == null)
            {
                slot = CreatSlot();
                slots.Add(slot);
            }

            slot.gameObject.SetActive(true);
        }

        for (int i = count; i < slots.Count; i++)
        {
            slots[i].gameObject.SetActive(false);
        }

        Count = count;
    }

    public void SetCountAndFirst(int count)
    {
        for (int i = 0; i < count; i++)
        {
            T slot = GetSlot(i);
            if (slot == null)
            {
                slot = CreatSlot(true);
                slots.Add(slot);
            }

            slot.gameObject.SetActive(true);
        }

        for (int i = count; i < slots.Count; i++)
        {
            slots[i].gameObject.SetActive(false);
        }

        Count = count;
    }

    public void SetCount(int count, bool active)
    {
        for (int i = 0; i < count; i++)
        {
            T slot = GetSlot(i);
            if (slot == null)
            {
                slot = CreatSlot();
                slots.Add(slot);
            }

            slot.gameObject.SetActive(active);
        }

        for (int i = count; i < slots.Count; i++)
        {
            slots[i].gameObject.SetActive(false);
        }

        Count = count;
    }

    public void SetCountWithReset(int count)
    {
        slots.Clear();

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform tran = parent.GetChild(i);

            T comp = tran.GetComponent<T>();
            if (comp != null)
            {
                slots.Add(comp);
            }
        }

        SetCount(count);
    }

    T CreatSlot(bool SetAsFirstSibling = false)
    {
        T slot = Object.Instantiate<T>(prefab, Vector3.zero, Quaternion.identity, parent);
        slot.transform.localPosition = Vector3.zero;
        if (SetAsFirstSibling)
            slot.transform.SetAsFirstSibling();
        return slot;
    }

    public T this[int index]
    {
        get
        {
            if (slots != null && index < slots.Count)
            {
                return slots[index];
            }

            return null;
        }
    }

    public T GetSlot(int index)
    {
        if (slots != null && index < slots.Count)
        {
            return slots[index];
        }

        return null;
    }

    public int GetSlotIndex(T slot)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slot == slots[i])
            {
                return i;
            }
        }

        return -1;
    }

    //移除某个cell后需要重新排序,把移除对象放到最后
    public void RemovePrefab(T prefab)
    {
        slots.Remove(prefab);
        slots.Add(prefab);
    }

    public void RemovePrefabAndSetActive(T prefab)
    {
        slots.Remove(prefab);
        slots.Add(prefab);

        Count--;
        prefab.transform.SetParent(parent);
        prefab.gameObject.SetActive(false);
    }

    public void Clear()
    {
        this.prefab = null;
        this.prefab = null;

        if (slots != null)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                GameObject.Destroy(slots[i].gameObject);
            }

            slots.Clear();
        }
    }

    public void ClearExceptPrefab()
    {
        if (slots != null)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] == this.prefab) continue;
                GameObject.Destroy(slots[i].gameObject);
            }

            slots.Clear();
        }

        this.prefab = null;
    }
}