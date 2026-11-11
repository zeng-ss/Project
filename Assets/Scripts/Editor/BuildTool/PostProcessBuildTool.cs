using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class PostProcessBuildTool
{
    public const string VersionKey = "Version_PostProcessBuildTool";
    
    [PostProcessBuild(1)]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
        switch (target)
        {
            case BuildTarget.iOS:
            {
                PostProcessBuild_iOS(path);
                break;
            }
        }
    }

    private static void PostProcessBuild_iOS(string path)
    {
// #if UNITY_IOS
//         ModifyInfoPlist(path);
// #endif
    }

#if UNITY_IOS
    private static void ModifyInfoPlist(string path)
    {
        // 修改Info.plist文件
        string plistPath = path + "/Info.plist";
        UnityEditor.iOS.Xcode.PlistDocument plist = new UnityEditor.iOS.Xcode.PlistDocument();
        plist.ReadFromString(File.ReadAllText(plistPath));
        UnityEditor.iOS.Xcode.PlistElementDict infoDict = plist.root;

        string version = EditorPrefs.GetString(VersionKey);
        infoDict.SetString("CFBundleShortVersionString", version); //version 

        File.WriteAllText(plistPath, plist.WriteToString());
    } 
#endif

}
