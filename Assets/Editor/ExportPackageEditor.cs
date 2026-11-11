//------------------------------------------------------------
// File : ExportPackageEditor.cs
// Email: mailto:zhiqiang.yang
// Desc : 
//------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ExportPackageEditor
{
    [MenuItem("Assets/Export Package By Selected")]
    static void ExportSelectedPackage()
    {
        var path = EditorUtility.SaveFilePanel("Save unitypackage", "", "", "unitypackage");
        if (path == "")
            return;

        var assetPathNames = new string[Selection.objects.Length];
        for (var i = 0; i < assetPathNames.Length; i++)
        {
            assetPathNames[i] = AssetDatabase.GetAssetPath(Selection.objects[i]);
        }

        assetPathNames = AssetDatabase.GetDependencies(assetPathNames);
        AssetDatabase.ExportPackage(assetPathNames, path, ExportPackageOptions.Interactive | ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);
    }

}

