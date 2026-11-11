using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CasualGame;

public class GridScrollChildBase : AttachPoolPrefabBase
{
    public IListDataItem ListDataItem { get; set; }

    public virtual void OnDeSpawnRefresh()
    {
    }


    public virtual void OnSpawnRefresh(IListDataItem data)
    {
        ListDataItem = data;
    }

    public virtual void RefreshWhenDrag()
    {
    }

    public virtual void AppearDisplay(IListDataItem data)
    {
        ListDataItem = data;
    }
}