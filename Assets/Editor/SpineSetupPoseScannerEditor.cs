using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Spine;
using Spine.Unity;
using UnityEditor;
using UnityEngine;

namespace KBGame
{
    public sealed class SpineSetupPoseScannerEditor : EditorWindow
    {
        private const string MenuRoot = "轻舟工具集/Spine Setup状态扫描";

        private string _assetPathsText = string.Empty;
        private string _slotFilterText = string.Empty;
        private string _skinFilterText = string.Empty;
        private bool _includeSkinAttachments = true;
        private bool _includeDefaultSkinEntries = false;
        private bool _copyReportToClipboard = true;
        private Vector2 _reportScrollPosition;
        private string _lastReport = string.Empty;

        [MenuItem(MenuRoot)]
        private static void ShowWindow()
        {
            GetWindow<SpineSetupPoseScannerEditor>("Spine Setup扫描");
        }

        [MenuItem("Assets/Spine/扫描 Setup状态", false, 2000)]
        private static void ScanSelectedFromAssetMenu()
        {
            RunSelectionScan(
                Selection.objects,
                ParseTokens(string.Empty),
                ParseTokens(string.Empty),
                true,
                false,
                true);
        }

        [MenuItem("Assets/Spine/扫描 Setup状态", true)]
        private static bool ValidateScanSelectedFromAssetMenu()
        {
            return Selection.objects != null && Selection.objects.Length > 0;
        }

        public static void DumpFromCommandLine()
        {
            string[] commandLineArgs = Environment.GetCommandLineArgs();
            List<string> assetPaths = ParseCommandLinePaths(GetCommandLineValue(commandLineArgs, "-spineSetupPaths"));
            HashSet<string> slotFilters = ParseTokens(GetCommandLineValue(commandLineArgs, "-spineSetupSlots"));
            HashSet<string> skinFilters = ParseTokens(GetCommandLineValue(commandLineArgs, "-spineSetupSkins"));
            bool includeSkinAttachments = ParseCommandLineBool(
                GetCommandLineValue(commandLineArgs, "-spineSetupIncludeSkins"),
                true);
            bool includeDefaultSkinEntries = ParseCommandLineBool(
                GetCommandLineValue(commandLineArgs, "-spineSetupIncludeDefaultSkin"),
                false);

            List<SkeletonDataAsset> assets = CollectSkeletonDataAssets(assetPaths);
            string report = BuildReport(assets, slotFilters, skinFilters, includeSkinAttachments, includeDefaultSkinEntries);

            if (!string.IsNullOrEmpty(report))
            {
                Debug.Log(report);
            }
        }

        private void OnGUI()
        {
            GUILayout.Label("Spine Setup状态扫描", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "支持扫描当前选中的 SkeletonDataAsset / .skel.bytes / 文件夹，也支持直接粘贴资源路径。输出格式为 setup slot 和 skin attachment 列表。",
                MessageType.Info);

            EditorGUILayout.Space();
            GUILayout.Label("路径扫描（每行或分号分隔一个路径）", EditorStyles.boldLabel);
            _assetPathsText = EditorGUILayout.TextArea(_assetPathsText, GUILayout.MinHeight(80f));

            EditorGUILayout.Space();
            _slotFilterText = EditorGUILayout.TextField("Slot过滤", _slotFilterText);
            _skinFilterText = EditorGUILayout.TextField("Skin过滤", _skinFilterText);
            _includeSkinAttachments = EditorGUILayout.Toggle("输出Skin条目", _includeSkinAttachments);
            _includeDefaultSkinEntries = EditorGUILayout.Toggle("包含Default Skin", _includeDefaultSkinEntries);
            _copyReportToClipboard = EditorGUILayout.Toggle("扫描后复制到剪贴板", _copyReportToClipboard);

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("扫描当前选中资源", GUILayout.Height(28f)))
            {
                RunSelectionScan(
                    Selection.objects,
                    ParseTokens(_slotFilterText),
                    ParseTokens(_skinFilterText),
                    _includeSkinAttachments,
                    _includeDefaultSkinEntries,
                    _copyReportToClipboard,
                    this);
            }

            if (GUILayout.Button("按路径扫描", GUILayout.Height(28f)))
            {
                RunPathScan(
                    ParseAssetPaths(_assetPathsText),
                    ParseTokens(_slotFilterText),
                    ParseTokens(_skinFilterText),
                    _includeSkinAttachments,
                    _includeDefaultSkinEntries,
                    _copyReportToClipboard,
                    this);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            GUILayout.Label("输出结果", EditorStyles.boldLabel);
            _reportScrollPosition = EditorGUILayout.BeginScrollView(_reportScrollPosition);
            EditorGUILayout.TextArea(_lastReport, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            if (!string.IsNullOrEmpty(_lastReport) && GUILayout.Button("复制当前结果"))
            {
                EditorGUIUtility.systemCopyBuffer = _lastReport;
            }
        }

        private static void RunSelectionScan(
            IEnumerable<UnityEngine.Object> selectedObjects,
            HashSet<string> slotFilters,
            HashSet<string> skinFilters,
            bool includeSkinAttachments,
            bool includeDefaultSkinEntries,
            bool copyReportToClipboard,
            SpineSetupPoseScannerEditor window = null)
        {
            List<SkeletonDataAsset> assets = CollectSkeletonDataAssets(selectedObjects);
            PublishReport(
                BuildReport(assets, slotFilters, skinFilters, includeSkinAttachments, includeDefaultSkinEntries),
                copyReportToClipboard,
                window);
        }

        private static void RunPathScan(
            IEnumerable<string> assetPaths,
            HashSet<string> slotFilters,
            HashSet<string> skinFilters,
            bool includeSkinAttachments,
            bool includeDefaultSkinEntries,
            bool copyReportToClipboard,
            SpineSetupPoseScannerEditor window = null)
        {
            List<SkeletonDataAsset> assets = CollectSkeletonDataAssets(assetPaths);
            PublishReport(
                BuildReport(assets, slotFilters, skinFilters, includeSkinAttachments, includeDefaultSkinEntries),
                copyReportToClipboard,
                window);
        }

        private static void PublishReport(string report, bool copyReportToClipboard, SpineSetupPoseScannerEditor window)
        {
            if (window != null)
            {
                window._lastReport = report;
                window.Repaint();
            }

            if (string.IsNullOrEmpty(report))
            {
                Debug.LogWarning("未找到可扫描的 Spine SkeletonDataAsset。");
                return;
            }

            if (!Application.isBatchMode && copyReportToClipboard)
            {
                EditorGUIUtility.systemCopyBuffer = report;
            }

            Debug.Log(report);
        }

        private static string BuildReport(
            List<SkeletonDataAsset> assets,
            HashSet<string> slotFilters,
            HashSet<string> skinFilters,
            bool includeSkinAttachments,
            bool includeDefaultSkinEntries)
        {
            if (assets == null || assets.Count == 0)
            {
                return string.Empty;
            }

            StringBuilder reportBuilder = new StringBuilder(4096);

            for (int i = 0; i < assets.Count; i++)
            {
                SkeletonDataAsset asset = assets[i];
                if (asset == null)
                {
                    continue;
                }

                SkeletonData skeletonData = asset.GetSkeletonData(true);
                if (skeletonData == null)
                {
                    Debug.LogWarning(string.Format("读取 SkeletonData 失败: {0}", AssetDatabase.GetAssetPath(asset)));
                    continue;
                }

                if (reportBuilder.Length > 0)
                {
                    reportBuilder.AppendLine();
                }

                string label = NormalizeSkeletonDisplayName(asset.name);
                AppendSetupSlots(reportBuilder, label, skeletonData, slotFilters);

                if (includeSkinAttachments)
                {
                    AppendSkinEntries(reportBuilder, label, skeletonData, slotFilters, skinFilters, includeDefaultSkinEntries);
                }
            }

            return reportBuilder.ToString().TrimEnd();
        }

        private static void AppendSetupSlots(
            StringBuilder reportBuilder,
            string label,
            SkeletonData skeletonData,
            HashSet<string> slotFilters)
        {
            reportBuilder.Append("== ").Append(label).AppendLine(" Slots ==");

            ExposedList<SlotData> slots = skeletonData.Slots;
            SlotData[] slotItems = slots.Items;

            for (int i = 0; i < slots.Count; i++)
            {
                SlotData slot = slotItems[i];
                string slotName = slot.Name;
                if (!MatchesAnyFilter(slotName, slotFilters))
                {
                    continue;
                }

                reportBuilder
                    .Append("slot=").Append(slotName)
                    .Append(" index=").Append(slot.Index)
                    .Append(" setupAttachment=").Append(FormatSetupAttachmentName(slot.AttachmentName))
                    .AppendLine();
            }
        }

        private static void AppendSkinEntries(
            StringBuilder reportBuilder,
            string label,
            SkeletonData skeletonData,
            HashSet<string> slotFilters,
            HashSet<string> skinFilters,
            bool includeDefaultSkinEntries)
        {
            List<Skin> filteredSkins = new List<Skin>();
            ExposedList<Skin> skins = skeletonData.Skins;
            Skin[] skinItems = skins.Items;

            for (int i = 0; i < skins.Count; i++)
            {
                Skin skin = skinItems[i];
                if (skin == null)
                {
                    continue;
                }

                if (!includeDefaultSkinEntries && IsDefaultSkin(skeletonData, skin))
                {
                    continue;
                }

                if (!MatchesAnyFilter(skin.Name, skinFilters))
                {
                    continue;
                }

                filteredSkins.Add(skin);
            }

            filteredSkins.Sort((left, right) => string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase));

            List<string> renderedSkins = new List<string>();
            StringBuilder skinBuilder = new StringBuilder(1024);

            foreach (Skin skin in filteredSkins)
            {
                List<Skin.SkinEntry> entries = skin.Attachments
                    .Where(entry => MatchesAnyFilter(GetSlotName(skeletonData, entry.SlotIndex), slotFilters))
                    .OrderBy(entry => entry.SlotIndex)
                    .ThenBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (entries.Count == 0)
                {
                    continue;
                }

                renderedSkins.Add(skin.Name);
                skinBuilder.Append("Skin: ").AppendLine(skin.Name);

                foreach (Skin.SkinEntry entry in entries)
                {
                    skinBuilder
                        .Append("  slot=").Append(GetSlotName(skeletonData, entry.SlotIndex))
                        .Append(" key=").Append(entry.Name)
                        .Append(" attachment=").Append(FormatAttachmentName(entry.Attachment))
                        .AppendLine();
                }
            }

            if (renderedSkins.Count == 0)
            {
                return;
            }

            reportBuilder.AppendLine();
            reportBuilder.Append("== ").Append(label).AppendLine(" Skins ==");
            reportBuilder.Append(skinBuilder);
        }

        private static bool IsDefaultSkin(SkeletonData skeletonData, Skin skin)
        {
            return ReferenceEquals(skeletonData.DefaultSkin, skin)
                   || string.Equals(skin.Name, "default", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetSlotName(SkeletonData skeletonData, int slotIndex)
        {
            ExposedList<SlotData> slots = skeletonData.Slots;
            if (slotIndex < 0 || slotIndex >= slots.Count)
            {
                return "<unknown>";
            }

            return slots.Items[slotIndex].Name;
        }

        private static string FormatAttachmentName(Attachment attachment)
        {
            return attachment == null ? "<null>" : attachment.Name;
        }

        private static string FormatSetupAttachmentName(string attachmentName)
        {
            return string.IsNullOrEmpty(attachmentName) ? "<null>" : attachmentName;
        }

        private static bool MatchesAnyFilter(string value, HashSet<string> filters)
        {
            if (filters == null || filters.Count == 0)
            {
                return true;
            }

            foreach (string filter in filters)
            {
                if (value.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static List<SkeletonDataAsset> CollectSkeletonDataAssets(IEnumerable<UnityEngine.Object> objects)
        {
            HashSet<string> uniqueAssetPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<SkeletonDataAsset> assets = new List<SkeletonDataAsset>();

            if (objects == null)
            {
                return assets;
            }

            foreach (UnityEngine.Object target in objects)
            {
                if (target == null)
                {
                    continue;
                }

                CollectSkeletonDataAssetsFromPath(AssetDatabase.GetAssetPath(target), uniqueAssetPaths, assets);
            }

            assets.Sort((left, right) =>
                string.Compare(
                    AssetDatabase.GetAssetPath(left),
                    AssetDatabase.GetAssetPath(right),
                    StringComparison.OrdinalIgnoreCase));
            return assets;
        }

        private static List<SkeletonDataAsset> CollectSkeletonDataAssets(IEnumerable<string> assetPaths)
        {
            HashSet<string> uniqueAssetPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<SkeletonDataAsset> assets = new List<SkeletonDataAsset>();

            if (assetPaths == null)
            {
                return assets;
            }

            foreach (string assetPath in assetPaths)
            {
                CollectSkeletonDataAssetsFromPath(assetPath, uniqueAssetPaths, assets);
            }

            assets.Sort((left, right) =>
                string.Compare(
                    AssetDatabase.GetAssetPath(left),
                    AssetDatabase.GetAssetPath(right),
                    StringComparison.OrdinalIgnoreCase));
            return assets;
        }

        private static void CollectSkeletonDataAssetsFromPath(
            string rawPath,
            HashSet<string> uniqueAssetPaths,
            List<SkeletonDataAsset> assets)
        {
            string assetPath = NormalizeAssetPath(rawPath);
            if (string.IsNullOrEmpty(assetPath))
            {
                return;
            }

            if (AssetDatabase.IsValidFolder(assetPath))
            {
                string[] guids = AssetDatabase.FindAssets("t:SkeletonDataAsset", new[] { assetPath });
                foreach (string guid in guids)
                {
                    TryAddSkeletonDataAsset(AssetDatabase.GUIDToAssetPath(guid), uniqueAssetPaths, assets);
                }

                return;
            }

            SkeletonDataAsset directAsset = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(assetPath);
            if (directAsset != null)
            {
                TryAddSkeletonDataAsset(assetPath, uniqueAssetPaths, assets);
                return;
            }

            if (IsSkeletonDataFile(assetPath))
            {
                string matchingSkeletonAssetPath = FindMatchingSkeletonDataAssetPath(assetPath);
                if (!string.IsNullOrEmpty(matchingSkeletonAssetPath))
                {
                    TryAddSkeletonDataAsset(matchingSkeletonAssetPath, uniqueAssetPaths, assets);
                }
            }
        }

        private static void TryAddSkeletonDataAsset(
            string assetPath,
            HashSet<string> uniqueAssetPaths,
            List<SkeletonDataAsset> assets)
        {
            if (string.IsNullOrEmpty(assetPath) || !uniqueAssetPaths.Add(assetPath))
            {
                return;
            }

            SkeletonDataAsset asset = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(assetPath);
            if (asset != null)
            {
                assets.Add(asset);
            }
        }

        private static string FindMatchingSkeletonDataAssetPath(string skeletonDataFilePath)
        {
            string directoryPath = Path.GetDirectoryName(skeletonDataFilePath);
            string baseName = GetSkeletonBaseName(skeletonDataFilePath);
            if (string.IsNullOrEmpty(directoryPath) || string.IsNullOrEmpty(baseName))
            {
                return null;
            }

            string directAssetPath = Path.Combine(directoryPath, baseName + "_SkeletonData.asset").Replace("\\", "/");
            if (File.Exists(directAssetPath))
            {
                return directAssetPath;
            }

            string[] localGuids = AssetDatabase.FindAssets(baseName + " t:SkeletonDataAsset", new[] { directoryPath });
            if (localGuids.Length > 0)
            {
                return AssetDatabase.GUIDToAssetPath(localGuids[0]);
            }

            string[] globalGuids = AssetDatabase.FindAssets(baseName + " t:SkeletonDataAsset");
            if (globalGuids.Length > 0)
            {
                return AssetDatabase.GUIDToAssetPath(globalGuids[0]);
            }

            Debug.LogWarning(string.Format("未找到与 {0} 对应的 SkeletonDataAsset。", skeletonDataFilePath));
            return null;
        }

        private static bool IsSkeletonDataFile(string assetPath)
        {
            return assetPath.EndsWith(".skel.bytes", StringComparison.OrdinalIgnoreCase)
                   || assetPath.EndsWith(".skel", StringComparison.OrdinalIgnoreCase)
                   || assetPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
                   || assetPath.EndsWith(".json.txt", StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeAssetPath(string rawPath)
        {
            if (string.IsNullOrWhiteSpace(rawPath))
            {
                return null;
            }

            string normalizedPath = rawPath.Trim().Trim('"').Replace("\\", "/");

            if (normalizedPath.StartsWith(Application.dataPath, StringComparison.OrdinalIgnoreCase))
            {
                normalizedPath = "Assets" + normalizedPath.Substring(Application.dataPath.Length);
            }

            return normalizedPath;
        }

        private static string NormalizeSkeletonDisplayName(string rawName)
        {
            if (string.IsNullOrEmpty(rawName))
            {
                return "<unnamed>";
            }

            string displayName = rawName;

            if (displayName.EndsWith("_SkeletonData", StringComparison.OrdinalIgnoreCase))
            {
                displayName = displayName.Substring(0, displayName.Length - "_SkeletonData".Length);
            }

            if (displayName.EndsWith(".skel", StringComparison.OrdinalIgnoreCase))
            {
                displayName = Path.GetFileNameWithoutExtension(displayName);
            }

            return displayName;
        }

        private static string GetSkeletonBaseName(string assetPath)
        {
            string fileName = Path.GetFileName(assetPath);
            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            if (fileName.EndsWith(".skel.bytes", StringComparison.OrdinalIgnoreCase))
            {
                return fileName.Substring(0, fileName.Length - ".skel.bytes".Length);
            }

            if (fileName.EndsWith(".json.txt", StringComparison.OrdinalIgnoreCase))
            {
                return fileName.Substring(0, fileName.Length - ".json.txt".Length);
            }

            if (fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                return fileName.Substring(0, fileName.Length - ".json".Length);
            }

            if (fileName.EndsWith(".skel", StringComparison.OrdinalIgnoreCase))
            {
                return fileName.Substring(0, fileName.Length - ".skel".Length);
            }

            return Path.GetFileNameWithoutExtension(fileName);
        }

        private static HashSet<string> ParseTokens(string rawText)
        {
            HashSet<string> tokens = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(rawText))
            {
                return tokens;
            }

            string[] parts = rawText.Split(new[] { '\r', '\n', ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts)
            {
                string token = part.Trim();
                if (!string.IsNullOrEmpty(token))
                {
                    tokens.Add(token);
                }
            }

            return tokens;
        }

        private static List<string> ParseAssetPaths(string rawText)
        {
            if (string.IsNullOrWhiteSpace(rawText))
            {
                return new List<string>();
            }

            return rawText
                .Split(new[] { '\r', '\n', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(path => path.Trim())
                .Where(path => !string.IsNullOrEmpty(path))
                .ToList();
        }

        private static List<string> ParseCommandLinePaths(string rawText)
        {
            if (string.IsNullOrWhiteSpace(rawText))
            {
                return new List<string>();
            }

            return rawText
                .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(path => path.Trim())
                .Where(path => !string.IsNullOrEmpty(path))
                .ToList();
        }

        private static string GetCommandLineValue(string[] commandLineArgs, string key)
        {
            for (int i = 0; i < commandLineArgs.Length - 1; i++)
            {
                if (string.Equals(commandLineArgs[i], key, StringComparison.OrdinalIgnoreCase))
                {
                    return commandLineArgs[i + 1];
                }
            }

            return null;
        }

        private static bool ParseCommandLineBool(string rawValue, bool defaultValue)
        {
            bool parsedValue;
            return bool.TryParse(rawValue, out parsedValue) ? parsedValue : defaultValue;
        }
    }
}
