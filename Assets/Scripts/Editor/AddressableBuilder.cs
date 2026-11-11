#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace CasualGame.Editor
{
    /// <summary>
    /// 一键把游戏资源标记为 Addressable。
    /// 规则：Address 直接用资源路径（Assets/xxx），因为 ResourceIdentificationTool 返回的就是路径。
    /// </summary>
    public static class AddressableBuilder
    {
        /// <summary>要纳入 Addressables 的目录。</summary>
        private static readonly string[] IncludeFolders =
        {
            "Assets/Game",
            "Assets/RawAsset",
            "Assets/Resources",
            "Assets/Sprite",
            "Assets/Texture2D",
            "Assets/Material",
            "Assets/Mesh",
            "Assets/Shader",
            "Assets/AnimationClip",
            "Assets/AnimatorController",
            "Assets/CupMaterial",
            "Assets/CupMesh",
            "Assets/CupTexture",
            "Assets/ShaderVariants",
        };

        /// <summary>不标记的扩展名。</summary>
        private static readonly HashSet<string> SkipExtensions = new HashSet<string>
        {
            ".cs", ".asmdef", ".asmref", ".preset", ".meta",
        };

        [MenuItem("Tools/资源/1. 一键标记 Addressables", false, 100)]
        public static void MarkAll()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("[AddressableBuilder] 没有 Addressables 配置，请先执行 Window > Asset Management > Addressables > Groups > Create Addressables Settings");
                return;
            }

            var groups = new Dictionary<string, AddressableAssetGroup>();
            var addressNames = new Dictionary<string, string>();
            int total = 0;
            int skipped = 0;
            int duplicated = 0;

            foreach (string folder in IncludeFolders)
            {
                if (!AssetDatabase.IsValidFolder(folder)) continue;

                string[] guids = AssetDatabase.FindAssets(string.Empty, new[] { folder });
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (string.IsNullOrEmpty(path) || AssetDatabase.IsValidFolder(path)) continue;

                    string extension = System.IO.Path.GetExtension(path).ToLowerInvariant();
                    if (SkipExtensions.Contains(extension) || path.EndsWith(".unity.meta"))
                    {
                        skipped++;
                        continue;
                    }

                    string groupName = GetGroupName(path);
                    if (!groups.TryGetValue(groupName, out AddressableAssetGroup group))
                    {
                        group = settings.FindGroup(groupName)
                                ?? settings.CreateGroup(groupName, false, false, true, settings.DefaultGroup.Schemas);
                        groups[groupName] = group;
                    }

                    AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group, false, false);

                    // Address 用文件名（不含扩展名），例：UIMainForm#10001
                    string address = System.IO.Path.GetFileNameWithoutExtension(path);
                    if (addressNames.TryGetValue(address, out string existPath) && existPath != path)
                    {
                        Debug.LogWarning($"[AddressableBuilder] 地址重名: {address}\n  已有: {existPath}\n  冲突: {path}");
                        duplicated++;
                    }
                    else
                    {
                        addressNames[address] = path;
                    }

                    entry.address = address;
                    total++;
                }
            }

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true, true);
            AssetDatabase.SaveAssets();

            Debug.Log($"[AddressableBuilder] 标记完成：{total} 个资源进 {groups.Count} 个组（跳过 {skipped} 个脚本/配置文件，地址重名 {duplicated} 个）。");
        }

        [MenuItem("Tools/资源/2. 构建 Addressables 内容", false, 101)]
        public static void BuildContent()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("[AddressableBuilder] 没有 Addressables 配置。");
                return;
            }

            AddressableAssetSettings.BuildPlayerContent();
            Debug.Log("[AddressableBuilder] Addressables 内容构建完成。");
        }

        [MenuItem("Tools/资源/3. 清除所有 Addressables 标记", false, 102)]
        public static void ClearAll()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null) return;

            int count = 0;
            foreach (AddressableAssetGroup group in new List<AddressableAssetGroup>(settings.groups))
            {
                if (group == null || group == settings.DefaultGroup) continue;

                foreach (AddressableAssetEntry entry in new List<AddressableAssetEntry>(group.entries))
                {
                    settings.RemoveAssetEntry(entry.guid, false);
                    count++;
                }
            }

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true, true);
            AssetDatabase.SaveAssets();

            Debug.Log($"[AddressableBuilder] 已清除 {count} 条标记（默认组保留）。");
        }

        /// <summary>按路径分到不同组，方便在 Addressables 面板里查看。</summary>
        private static string GetGroupName(string path)
        {
            if (path.StartsWith("Assets/Game/Prefabs")) return "game_ui";
            if (path.StartsWith("Assets/Game/Audio")) return "game_audio";
            if (path.StartsWith("Assets/Game/GameScenes")) return "game_scene";
            if (path.StartsWith("Assets/Game/Configs")) return "game_config";
            if (path.StartsWith("Assets/Game/Spines")) return "game_spine";
            if (path.StartsWith("Assets/Game/Sprites")) return "game_sprite";
            if (path.StartsWith("Assets/Game/Level")) return "game_level";
            if (path.StartsWith("Assets/Game/Localization")) return "game_localization";
            if (path.StartsWith("Assets/Game/Fonts")) return "game_font";
            return "game_misc";
        }
    }
}
#endif
