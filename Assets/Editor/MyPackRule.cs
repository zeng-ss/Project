using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using YooAsset;
using YooAsset.Editor;



[DisplayName("资源包名: 单个文件-记录依赖")]
public class PackSeparately2 : IPackRule
{
    PackRuleResult IPackRule.GetPackRuleResult(PackRuleData data)
    {
        List<string> deps = GetDep(data.AssetPath);
        foreach (var dep in deps)
        {
            if (!DefaultPackRule.s_Deps.ContainsKey(dep))
            {
                // UnityEngine.Debug.Log($"依赖  {data.AssetPath} -> {dep}");
                DefaultPackRule.s_Deps.Add(dep, data.AssetPath);
            }
        }
        string bundleName = PathUtility.RemoveExtension(data.AssetPath);
        PackRuleResult result = new PackRuleResult(bundleName, DefaultPackRule.AssetBundleFileExtension);
        return result;
    }

    public static List<string> GetDep(string assetPath)
    {
        var set = new List<string>();
        foreach (var dependency in AssetDatabase.GetDependencies(assetPath))
        {
            if (dependency == assetPath
                || set.Contains(dependency)
                || dependency.Contains("Font")
                || dependency.StartsWith("Packages/")
                || dependency.Contains("/Plugins/")
                || dependency.EndsWith(".shader")
                || dependency.EndsWith("Editor")
                || dependency.EndsWith(".otf")
                || dependency.EndsWith(".ttf"))
            {
                continue;
            }

            // if (!dependency.StartsWith("Assets/GameMain/Art"))
            // {
            //     // Debug.LogError(" 注意了，这个资源不是GameMain下的资源，请检查 " + dependency);
            //     continue;
            // }
            set.Add(dependency);
        }

        return set;
    }
}


[DisplayName("资源包名: 被其他人引用")]
public class PackDepence : IPackRule
{
    PackRuleResult IPackRule.GetPackRuleResult(PackRuleData data)
    {
        PackRuleResult result;
        string bundleName = string.Empty;
        if (DefaultPackRule.s_Deps.ContainsKey(data.AssetPath))
        {
            bundleName = PathUtility.RemoveExtension(DefaultPackRule.s_Deps[data.AssetPath]);
            result = new PackRuleResult(bundleName, DefaultPackRule.AssetBundleFileExtension);
            // UnityEngine.Debug.Log($"打包依赖资源 {data.AssetPath} -> {bundleName}");
            return result;
        }
        else
        {
            bundleName = $"Assets/dependency_asset";
            result = new PackRuleResult(bundleName, DefaultPackRule.AssetBundleFileExtension);
            UnityEngine.Debug.Log($"打包依赖资源 {data.AssetPath} -> {bundleName}");
            return result;
        }
    }
}