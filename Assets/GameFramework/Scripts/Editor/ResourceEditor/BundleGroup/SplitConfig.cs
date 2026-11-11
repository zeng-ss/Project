//------------------------------------------------------------
// File : SplitConfig.cs
// Email: mailto:zhiqiang.yang@kingboat.io
// Desc : 
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityGameFramework.Editor.ResourceTools
{
    public class SplitConfig : ScriptableObject
    {
        /// <summary>
        ///     分包模式，默认是白名单，资源组中配置了的包含，黑名单则刚好相反，配置了的不采集
        /// </summary>
        public SplitMode splitMode = SplitMode.SplitByAssetsWithDependencies;

        /// <summary>
        ///     资源组
        /// </summary>

        public IEnumerable<string> GetAssets()
        {
            var hashset = new HashSet<string>(Array.ConvertAll(assets, AssetDatabase.GetAssetPath));
            hashset.RemoveWhere(string.IsNullOrEmpty);
            return hashset;
        }

        public Object[] assets = new Object[0];
    }


    /// <summary>
    ///     分包模式
    /// </summary>
    public enum SplitMode
    {
        /// <summary>
        ///     不分包
        /// </summary>
        SplitNone,

        /// <summary>
        ///     包含
        /// </summary>
        SplitByAssetsWithDependencies,

        /// <summary>
        ///     反包含
        /// </summary>
        SplitByExcludedAssetsWithDependencies
    }
}