//------------------------------------------------------------
// File : NewBehaviourScript.cs
// Email: mailto:zhiqiang.yang
// Desc : 
//------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CasualGame
{
    public interface ICurrencyGroup
    {
        public Transform GetItemTransform(ItemType type);
    }
}
