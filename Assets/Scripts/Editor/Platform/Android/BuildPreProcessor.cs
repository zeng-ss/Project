using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace CasualGame
{
    public class AndroidPrebuild : IPreprocessBuildWithReport
    {
        public int callbackOrder
        {
            get => 1;
        }

        public void OnPreprocessBuild(BuildReport report)
        {
#if UNITY_ANDROID
            _OnPreprocessBuild();
#endif
        }

        public static void _OnPreprocessBuild()
        {
            PlayerSettings.SplashScreen.showUnityLogo = false;
        }
    }
}