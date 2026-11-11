//------------------------------------------------------------
// File : TMPHelper.cs
// Email: mailto:yang.li@kingboat.io
// Desc : 
//------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace CasualGame
{
    public class TMPHelper
    {
        private const string TmpSettingsResourcesPath = "TMP/TMP Settings";

        private const string TMPSettingAssetPath = "Assets/Game/Fonts/TMP Settings.asset";

        public static void LoadSettingsDefault()
        {
            TMP_Settings settings = Resources.Load<TMP_Settings>(TmpSettingsResourcesPath);
            if (settings == null)
            {
                Debug.LogError($"[TMPHelper] Missing TMP_Settings. Place asset at Resources/{TmpSettingsResourcesPath} (file: TMP/TMP Settings.asset).");
                return;
            }

            ApplyTmpSettingsInstance(settings);
        }

        public static async UniTaskVoid LoadSetting(Action onCompleteAction = null)
        {
            TMP_Settings resource = await GameEntry.Resource.LoadAsset<TMP_Settings>(TMPSettingAssetPath, default);
            ApplyTmpSettingsInstance(resource);

            onCompleteAction?.Invoke();
        }

        private static void ApplyTmpSettingsInstance(TMP_Settings resource)
        {
            if (resource == null)
            {
                return;
            }

            Type settingsType = resource.GetType();
            FieldInfo settingsInstanceInfo = settingsType.GetField("s_Instance", BindingFlags.Static | BindingFlags.NonPublic);
            if (settingsInstanceInfo == null)
            {
                Debug.LogError("[TMPHelper] TMP_Settings.s_Instance not found; TextMeshPro package version may have changed.");
                return;
            }

            settingsInstanceInfo.SetValue(null, resource);
        }
    }
}
