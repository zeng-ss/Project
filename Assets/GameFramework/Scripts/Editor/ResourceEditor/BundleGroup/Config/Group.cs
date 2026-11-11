using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Collections;
using System.Collections.Generic;
using UnityGameFramework.Runtime;
using UnityEditor.Animations;

namespace UnityGameFramework.Editor.ResourceTools
{
    /// <summary>
    ///     资源的打包分组。
    /// </summary>
    [CreateAssetMenu(menuName = "xasset/Group", fileName = "Group", order = 0)]
    public class Group : ScriptableObject
    {
        /// <summary>
        ///     需要采集的资源节点
        /// </summary>
        [Tooltip("需要采集的节点")] public Object[] entries;

        /// <summary>
        ///     过滤器，例如t:Scene 表示场景，t:Texture 表示纹理
        /// </summary>
        [Tooltip("过滤器，例如t:Scene 表示场景，t:Texture 表示纹理")]
        public string filter;

        [Tooltip("过滤掉文件夹")] public Object[] filterFolder;

        /// <summary>
        ///     打包模式，控制资源的打包粒度，PackTogether 表示打包到一起，PackByFile 表示每个文件单独打包，PackByDirectory 则按目录打包
        /// </summary>
        public BundleMode bundleMode = BundleMode.PackByFile;

        public string build;

        private static string GetDirectoryName(string path)
        {
            var dir = Path.GetDirectoryName(path);
            return !string.IsNullOrEmpty(dir) ? dir.Replace("\\", "/") : string.Empty;
        }

        public static string PackAsset(string assetPath, string entry, BundleMode bundleMode, string group,
            string build)
        {
            var bundle = string.Empty;
            switch (bundleMode)
            {
                case BundleMode.PackTogether:
                    bundle = group;
                    break;
                case BundleMode.PackByFolder:
                    bundle = GetDirectoryName(assetPath);
                    break;
                case BundleMode.PackByFile:
                    bundle = assetPath;
                    break;
                case BundleMode.PackByTopSubFolder:
                    bundle = PackAssetByTopDirectory(assetPath, entry, bundle);
                    break;
                case BundleMode.PackByEntry:
                    bundle = Path.GetFileNameWithoutExtension(entry);
                    break;
            }

            return PackAsset(assetPath, build, bundle);
        }

        private static string PackAsset(string assetPath, string build, string bundle)
        {
            if (Settings.ForceAllShadersPackTogether && assetPath.EndsWith(".shader"))
            {
                bundle = Settings.ShadersAssetBundleName;
            }

            var extension = Settings.BundleExtension;

            bundle = bundle.Replace(" ", "").Replace("-", "_").Replace(".", "_");
            return $"{ApplyReplaceBundleNames(bundle)}{extension}";
        }

        private static string PackAssetByTopDirectory(string assetPath, string entryPath, string bundle)
        {
            if (!string.IsNullOrEmpty(entryPath))
            {
                int pos = assetPath.IndexOf("/", entryPath.Length + 1, StringComparison.Ordinal);
                bundle = pos != -1 ? assetPath.Substring(0, pos) : entryPath;
            }
            else
            {
                Debug.LogError($"invalid rootPath {assetPath}");
            }

            return bundle;
        }

        private static string ApplyReplaceBundleNames(string bundle)
        {
            foreach (var replace in Settings.ReplaceBundleNames)
            {
                if (!replace.enabled) continue;

                if (!bundle.Contains(replace.key)) continue;

                bundle = bundle.Replace(replace.key, replace.value);
                break;
            }

            return bundle;
        }

        public static void CollectAssets(Group group, Action<string, string> collectAction)
        {
            if (group == null || group.entries == null) return;

            foreach (var asset in group.entries)
            {
                if (asset == null) continue;

                string path = AssetDatabase.GetAssetPath(asset);
                
                // 不是文件夹，所以是文件
                if (!Directory.Exists(path))
                {
                    collectAction?.Invoke(path, path);
                }
                else
                {
                    string[] guids = AssetDatabase.FindAssets(group.filter, new[] { path });
                    foreach (var guild in guids)
                    {
                        string childAssetPath = AssetDatabase.GUIDToAssetPath(guild);
                        if (group.FilterByPath(childAssetPath)
                            || string.IsNullOrEmpty(childAssetPath)
                            || Directory.Exists(childAssetPath)
                            || Settings.IsExcluded(childAssetPath))
                            continue;

                        collectAction?.Invoke(childAssetPath, path);
                    } 
                }
            }
        }

        public GroupAsset CreateAsset(string path, string entry)
        {
            System.Type type = AssetDatabase.GetMainAssetTypeAtPath(path);
            
            return new GroupAsset
            {
                path = path,
                entry = entry,
                type = type.Name,
                group = this,
                guid = AssetDatabase.AssetPathToGUID(path)
            };
        }

        private bool FilterByPath(string filePath)
        {
            if (filterFolder == null || filterFolder.Length == 0)
                return false;

            for (int i = 0; i < filterFolder.Length; i++)
            {
                var path = AssetDatabase.GetAssetPath(filterFolder[i]);
                if (filePath.StartsWith(path))
                    return true;
            }

            return false;
        }

        public static string PackAsset(GroupAsset asset)
        {
            return PackAsset(asset.path, asset.entry, asset.group.bundleMode, asset.group.name, asset.group.build);
        }

        public static List<string> GetDependencyInRawAsset(string assetPath)
        {
            List<string> set = new List<string>();

            foreach (var dependencyPath in AssetDatabase.GetDependencies(assetPath))
            {
                // 剔除所有非原始资源的依赖
                if(!dependencyPath.StartsWith("Assets/RawAsset")) continue;
                
                set.Add(dependencyPath);
            }

            return set; 
        }

        /// <summary>
        /// 获取引用
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        /// 
        //OriginFont  Font
        public static List<string> GetDep(string assetPath)
        {
            var set = new List<string>();

            foreach (var dependency in AssetDatabase.GetDependencies(assetPath))
            {
                if (dependency == assetPath
                    || Settings.ExcludeFiles.Exists(dependency.Contains)
                    || Settings.IsExcluded(dependency)
                    || set.Contains(dependency)
                    || dependency.EndsWith(".shader")
                    || dependency.Contains("Font")
                    || dependency.StartsWith("Packages/")
                    || dependency.Contains("/Plugins/"))
                {
                    continue;
                }

                if (!dependency.StartsWith("Assets/GameMain/GameAsset"))
                {
                    Debug.LogError(" 注意了，这个资源不是GameMain下的资源，请检查 " + dependency);
                }

                set.Add(dependency);
            }

            return set;
        }
    }
}