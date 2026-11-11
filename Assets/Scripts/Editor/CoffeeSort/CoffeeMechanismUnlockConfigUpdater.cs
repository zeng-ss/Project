using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CasualGame.CoffeeAway;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace CasualGame.EditorTools
{
    public static class CoffeeMechanismUnlockConfigUpdater
    {
        private const string ConfigAssetPath = "Assets/Game/CoffeeMechanismUnlockConfig.asset";
        private const string LevelFolder = "Assets/Game/Level";
        private static readonly Regex LevelFilePattern = new(@"^Level-(\d+)\.json$", RegexOptions.IgnoreCase);

        [MenuItem("Tools/CoffeeSort/更新机制首次出现关卡")]
        public static void UpdateConfig()
        {
            List<int> levelIds = ScanLevelIds();
            if (levelIds.Count == 0)
            {
                Debug.LogError($"[CoffeeSort] 未在 {LevelFolder} 下找到任何 Level-{{id}}.json 关卡文件。");
                return;
            }

            var firstAppearance = new Dictionary<ItemType, CoffeeMechanismUnlockConfig.Entry>();

            for (int i = 0; i < levelIds.Count; i++)
            {
                int levelId = levelIds[i];
                string levelPath = LevelData.GetLevelAssetPath(levelId);
                TextAsset levelAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(levelPath);
                if (levelAsset == null)
                {
                    Debug.LogError($"[CoffeeSort] 找不到关卡文件：{levelPath}");
                    return;
                }

                try
                {
                    RecordFirstAppearances(levelAsset.text, levelId, firstAppearance);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[CoffeeSort] 解析关卡失败：{levelPath}\n{ex}");
                    return;
                }
            }

            var generatedEntries = firstAppearance.Values
                .OrderBy(entry => entry.FirstProgressLevel)
                .ThenBy(entry => entry.Mechanism)
                .ToList();
            if (generatedEntries.Count == 0)
            {
                Debug.LogWarning("[CoffeeSort] 未扫描到可识别的特殊玩法机制，配置未更新。");
                return;
            }

            CoffeeMechanismUnlockConfig config =
                AssetDatabase.LoadAssetAtPath<CoffeeMechanismUnlockConfig>(ConfigAssetPath);
            if (config == null)
            {
                EnsureAssetFolder("Assets/Game");
                config = ScriptableObject.CreateInstance<CoffeeMechanismUnlockConfig>();
                AssetDatabase.CreateAsset(config, ConfigAssetPath);
            }

            Undo.RecordObject(config, "Update CoffeeSort mechanism unlock config");
            config.ReplaceGeneratedEntries(generatedEntries);
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();

            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);

            LogResult(generatedEntries);
        }

        private static List<int> ScanLevelIds()
        {
            var ids = new List<int>();
            if (!Directory.Exists(LevelFolder))
            {
                return ids;
            }

            foreach (string file in Directory.GetFiles(LevelFolder, "Level-*.json", SearchOption.TopDirectoryOnly))
            {
                string name = Path.GetFileName(file);
                Match match = LevelFilePattern.Match(name);
                if (match.Success && int.TryParse(match.Groups[1].Value, out int id) && id > 0)
                {
                    ids.Add(id);
                }
            }

            ids.Sort();
            return ids;
        }

        private static void RecordFirstAppearances(
            string levelJson,
            int levelId,
            IDictionary<ItemType, CoffeeMechanismUnlockConfig.Entry> firstAppearance)
        {
            JObject root = JObject.Parse(levelJson);
            if (root["holderDataList"] is not JArray holders)
            {
                return;
            }

            foreach (JToken holder in holders)
            {
                if (holder["states"] is JArray holderStates)
                {
                    foreach (JToken state in holderStates)
                    {
                        int type = state.Value<int?>("type") ?? 0;
                        TryRecordMechanism(levelId, type, null, firstAppearance);
                    }
                }

                if (holder["gameUnits"] is not JArray gameUnits)
                {
                    continue;
                }

                foreach (JToken unit in gameUnits)
                {
                    if (unit["States"] is not JArray unitStates)
                    {
                        continue;
                    }

                    foreach (JToken state in unitStates)
                    {
                        int type = state.Value<int?>("type") ?? 0;
                        int? stateType = state["ID"]?.Value<int?>("stateType");
                        TryRecordMechanism(levelId, type, stateType, firstAppearance);
                    }
                }
            }
        }

        private static void TryRecordMechanism(
            int levelId,
            int type,
            int? stateType,
            IDictionary<ItemType, CoffeeMechanismUnlockConfig.Entry> firstAppearance)
        {
            ItemType mechanism = ItemType.None;
            if (!CoffeeMechanismUnlockRegistry.TryGetMechanismFromHolderState(type, out mechanism)
                && !CoffeeMechanismUnlockRegistry.TryGetMechanismFromVehicleState(type, stateType, out mechanism))
            {
                return;
            }

            if (!CoffeeMechanismUnlockRegistry.IsMechanismFeature(mechanism)
                || firstAppearance.ContainsKey(mechanism))
            {
                return;
            }

            firstAppearance.Add(
                mechanism,
                new CoffeeMechanismUnlockConfig.Entry(mechanism, levelId, levelId));
        }

        private static void EnsureAssetFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            string folderName = Path.GetFileName(folderPath);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureAssetFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, folderName);
        }

        private static void LogResult(IReadOnlyList<CoffeeMechanismUnlockConfig.Entry> entries)
        {
            string rows = entries.Count == 0
                ? "（未发现可识别机制）"
                : string.Join(
                    "\n",
                    entries.Select(entry =>
                        $"- {entry.Mechanism}: progress={entry.FirstProgressLevel}"));

            Debug.Log(
                $"[CoffeeSort] 已更新机制首次出现配置，机制数={entries.Count}\n" +
                rows);
        }
    }
}
