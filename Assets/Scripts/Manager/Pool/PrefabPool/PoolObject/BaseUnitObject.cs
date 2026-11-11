//------------------------------------------------------------
// File : PoolPrefabBase.cs
// Email: mailto:zhiqiang.yang@kingboat.io
// Desc : 
//------------------------------------------------------------
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CasualGame
{

    public abstract class BaseUnitObject : MonoBehaviour
    {
        [HideInInspector] public int AssetId;

        public int PoolCount = 0;

        public virtual void OnInit(PrefabPool ppm)
        {
        }

        public virtual void OnSpawn(PrefabPool ppm)
        {
        }

        public virtual void OnDeSpawn(PrefabPool ppm)
        {
        }

        public int GetAssetIdByName()
        {
            var splits = name.Split("#");
            if (splits.Length == 2)
                return int.Parse(splits[1]);
            return 0;
        }

#if UNITY_EDITOR
        [ContextMenu("获取一个没用过的资源ID")]
        public void GetNotSameAssetID()
        {
            string numberString = ResourceIdentificationTool.GetNumbersFromString(name);

            int startIndex = 20000;
            if (int.TryParse(numberString, out int result))
            {
                startIndex = result;
            }

            int assetID = ResourceIdentificationTool.GetNotSameAssetIDByRange(startIndex);

            name = name.Replace(numberString, assetID.ToString());
        }
#endif
    }
}