using System.Collections.Generic;
using UnityEngine;
using CasualGame;
using System;

[Serializable]
public class PrewarmPoolDataItem
{
    public ResourceIdentificationType ResourceIdentificationType;
    public int Count;
}

[CreateAssetMenu(fileName = "PrewarmPoolData", menuName = "PrewarmPoolData/Create Setting")]
public class PrewarmPoolData : ScriptableObject
{
    public List<PrewarmPoolDataItem> PrewarmPoolDataItems = new List<PrewarmPoolDataItem>();
}