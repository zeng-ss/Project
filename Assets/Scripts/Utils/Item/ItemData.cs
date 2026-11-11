using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace CasualGame
{
    [Serializable]
    public class ItemData
    {
        public int id;
        public int num;

        public ItemData(int _id)
        {
            id = _id;
            num = 1;
        }

        public ItemData(int _id, int _num)
        {
            id = _id;
            num = _num;
        }

        [NonSerialized] private DRItem _drItem = null;

        public DRItem DRItem()
        {
            if (_drItem == null)
            {
                _drItem = ProxyManager.UserProxy.ItemTables.GetDataRow(id);
            }

            if (_drItem == null)
            {
                UnityGameFramework.Runtime.Log.Error(" cant find dr item id={0}", id);
                id = 1;
                _drItem = ProxyManager.UserProxy.ItemTables.GetDataRow(id);
            }

            return _drItem;
        }

        public string StyleNum()
        {
            return ItemUtils.GetNumByStyle(DRItem(), num);
        }

        public string MultStyleNum()
        {
            return ItemUtils.GetMultNumByStyle(DRItem(), num);
        }

        public string GetSpritePath(int index = 0)
        {
            string assetPath = AtlasLoader.SpritePath + Constant.AtlasId.Item + "/" + GetIconName(index) + ".png";
            return assetPath;
        }

        public string GetIconName(int index = 0)
        {
            string[] iconNames = DRItem().Icons.Split(",");
            return iconNames[index];
        }
    }
}