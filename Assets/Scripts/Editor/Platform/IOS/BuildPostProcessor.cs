//
// Copyright (c) 2017 eppz! mobile, Gergely Borbás (SP)
//
// http://www.twitter.com/_eppz
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE
// OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if UNITY_IOS

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using Unity.Notifications;


public class BuildPostProcessor
{
    private static string[] LANGUAGES = new string[] { "en" };
    private static string[] APP_NAMES = { "Screw Storm" };

    [PostProcessBuildAttribute(1)]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
        if (target == BuildTarget.iOS)
        {
            // 开启推送
            NotificationSettings.iOSSettings.AddRemoteNotificationCapability = true;

            // Read.
            string projectPath = PBXProject.GetPBXProjectPath(path);
            PBXProject project = new PBXProject();
            project.ReadFromString(File.ReadAllText(projectPath));
            string frameworkGuid = project.GetUnityFrameworkTargetGuid() ??
                                   throw new ArgumentNullException("project.GetUnityFrameworkTargetGuid()");
            string mainGuid = project.GetUnityMainTargetGuid();

            string entitlement = Application.dataPath + "/Editor/Platform/IOS/Base.entitlements";
            File.Copy(entitlement, path + "/Unity-iPhone/Unity-iPhone.entitlements");
            project.AddCapability(mainGuid, PBXCapabilityType.InAppPurchase,
                "Unity-iPhone/Unity-iPhone.entitlements", true);
            project.AddCapability(mainGuid, PBXCapabilityType.SignInWithApple,
                "Unity-iPhone/Unity-iPhone.entitlements", true);
            project.AddCapability(mainGuid, PBXCapabilityType.PushNotifications,
                "Unity-iPhone/Unity-iPhone.entitlements", true);
            project.AddCapability(mainGuid, PBXCapabilityType.BackgroundModes,
                "Unity-iPhone/Unity-iPhone.entitlements", true);

            AddFrameworks(project, frameworkGuid);

            // add files
            // AddFile(project, frameworkGuid, path, "Podfile");

            // SDK已设置了部分必须的属性
            // project.SetBuildProperty(mainGuid, "ENABLE_BITCODE", "NO");

            // Write.
            File.WriteAllText(projectPath, project.WriteToString());

            // 推送
            ProjectCapabilityManager projCapability = new ProjectCapabilityManager(
                    projectPath, "Unity-iPhone.entitlements", "Unity-iPhone");
            projCapability.AddSignInWithApple();
            projCapability.AddBackgroundModes(
                BackgroundModesOptions.BackgroundFetch | BackgroundModesOptions.RemoteNotifications);
            projCapability.WriteToFile();

            FixInfoPlist(project, path, frameworkGuid);

            // 应用名多语言
            // AddLanguages(path);
            // AddLocalization(path);
        }
    }

    //修改文件Info.plist
    static void FixInfoPlist(PBXProject project, string path, string targetGuid)
    {
        // Get plist
        string plistPath = path + "/Info.plist";
        PlistDocument plist = new PlistDocument();
        plist.ReadFromString(File.ReadAllText(plistPath));

        string exitsOnSuspendKey = "UIApplicationExitsOnSuspend";
        if (plist.root.values.ContainsKey(exitsOnSuspendKey))
        {
            plist.root.values.Remove(exitsOnSuspendKey);
        }

        plist.root.SetString("NSPhotoLibraryUsageDescription",
            "The APP would like to access your Photo Library for your profile photo upload");
        plist.root.SetString("NSCameraUsageDescription",
            "The APP would like to access your Camera for your profile photo upload");
        plist.root.SetString("NSMicrophoneUsageDescription",
            "The APP would like to access your Microphone for your profile video upload");
        plist.root.SetString("NSUserTrackingUsageDescription",
            "This identifier will be used to deliver personalized ads to you.");
        plist.root.SetString("NSLocationWhenInUseUsageDescription",
            "The APP needs to decide on different privacy policies based on your location information");
        plist.root.SetBoolean("ITSAppUsesNonExemptEncryption", false);

        // Write to plist
        File.WriteAllText(plistPath, plist.WriteToString());
    }

    static void AddFrameworks(PBXProject project, string targetGuid)
    {
        // Frameworks，SDK里已添加一部分必须的
    }

    static void AddFile(PBXProject project, string targetGuid, string path, string fileName)
    {
        var filePath = Path.Combine("Assets/Plugins/iOS", fileName);
        File.Copy(filePath, Path.Combine(path, fileName));
        project.AddFileToBuild(targetGuid, project.AddFile(fileName, fileName, PBXSourceTree.Source));
    }

    static void AddLanguages(string path)
    {
#if UNITY_EDITOR_WIN
#else
        string projectPath = PBXProject.GetPBXProjectPath(path);
        PBXProject project = new PBXProject();
        project.ReadFromFile(projectPath);
        project.ClearKnownRegions();
        PlistDocument plist = new PlistDocument();
        string plistPath = Path.Combine(path, "Info.plist");
        plist.ReadFromFile(plistPath);
        plist.root.SetBoolean("LSHasLocalizedDisplayName", true);
        PlistElementArray languages = plist.root.CreateArray("CFBundleLocalizations");
        foreach (string code in LANGUAGES)
        {
            project.AddKnownRegion(code);
            languages.AddString(code);
            Debug.LogFormat("Language \"{0}\" added to project", code);
        }
        plist.WriteToFile(plistPath);
        project.WriteToFile(projectPath);
#endif
    }

    static void AddLocalization(string path, string infoFile = "InfoPlist.strings")
    {
#if UNITY_EDITOR_WIN
#else
        string projectPath = PBXProject.GetPBXProjectPath(path);
        PBXProject project = new PBXProject();
        project.ReadFromFile(projectPath);
        for (int i = 0; i < LANGUAGES.Length; i++)
        {
            string code = LANGUAGES[i];
            string name = APP_NAMES[i];

            string langDir = string.Format("{0}.lproj", code);
            string directory = Path.Combine(path, langDir);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string filePath = Path.Combine(directory, infoFile);
            string relativePath = Path.Combine(langDir, infoFile);
            string content = "\"CFBundleDisplayName\" = \"" + name + "\";\n";
            File.WriteAllText(filePath, content);
            project.AddLocaleVariantFile(infoFile, code, relativePath);
            Debug.LogFormat("Localization \"{0}\" added to project", langDir);
        }
        project.WriteToFile(projectPath);
#endif
    }
}

#endif