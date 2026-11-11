//------------------------------------------------------------
// File : Settings.cs
// Email: mailto:zhiqiang.yang@kingboat.io
// Desc : 
//------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;
using System;
using System.IO;
using System.Linq;
using UnityEditor;

namespace UnityGameFramework.Editor.ResourceTools
{
    public class Settings
    {
        public static bool ReplaceBundleNameWithHash = false;
        public static string BundleExtension = ".dat";
        public static List<string> ExcludeFiles = new List<string> { ".spriteatlas" };
        public static ReplaceBundleName[] ReplaceBundleNames = new ReplaceBundleName[1] { new ReplaceBundleName { key = "Assets/", enabled = true, value = "" } };
        public static bool ForceAllShadersPackTogether = true;
        public const string ShadersAssetBundleName = "shaders";
        // public static bool EncryptionEnabled { get; private set; }


        public static bool IsExcluded(string path)
        {
            return ExcludeFiles.Exists(path.EndsWith) || path.EndsWith(".cs") || path.EndsWith(".dll");
        }

        public static IEnumerable<string> GetDependencies(string path)
        {
            var set = new HashSet<string>(AssetDatabase.GetDependencies(path, true));
            set.Remove(path);
            set.RemoveWhere(IsExcluded);
            return set.ToArray();
        }
    }


    [Serializable]
    public class ReplaceBundleName
    {
        public string key;
        public string value;
        public bool enabled = true;
    }
}
