using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CasualGame.Editor;
using GameFramework;
using GameFramework.DataTable;
using UnityEditor;
using UnityEngine;

namespace CasualGame
{
    public sealed class DataTableGeneratorMenu
    {
        public static string WorkingDirectory_Excel => $"{Application.dataPath}/../../Tool/ExportExcelTool";
        public static string WorkingDirectory_LanguageTool_Java => $"{Application.dataPath}/../../Tool/ExportLanguageTool_Java";
        public static string WorkingDirectory_LanguageTool_Python => $"{Application.dataPath}/../../Tool/ExportLanguageTool";
        public static string OutputPath => $"{WorkingDirectory_Excel}/csv";
        public static string FolderPath => $"{Application.dataPath}/../../GameData";

        [UnityEditor.MenuItem("Tools/Excel/跳转Excel配置文件")]
        public static void RevealInFinder_Excel()
        {
            // 检查文件夹是否存在
            if (System.IO.Directory.Exists(FolderPath))
            {
                // 使用 Process.Start 方法打开文件夹
                System.Diagnostics.Process.Start("open", FolderPath);
            }
        }

        [MenuItem("Tools/Excel/Generate DataTables")]
        private static void GenerateDataTables()
        {
            GenerateDataTables(IsMac());

            CreateDataTableType();
            CreateConfigFieldInfo();
            CreateUIFormType();
            CreateUIPrefabAndScript();
            DataTableCheck.Check();
        }

        [MenuItem("Tools/Excel/Generate Language")]
        private static void GenerateLanguage()
        {
            GenerateLanguage(IsMac());
        }

        [MenuItem("Tools/Excel/DownLoad And Export Language")]
        private static void DownLoadAndExportLanguage()
        {
            if (IsMac())
            {
                ProcessCommandUtility.ProcessCommand("/bin/sh", "ExportLanguage.sh", WorkingDirectory_LanguageTool_Python);
            }
            else
            {
                // ProcessCommand("uiText.bat", "", Application.dataPath + "/../Doc/TranslationTool/");
            }

            AssetDatabase.Refresh();
        }

        private static bool IsMac()
        {
            string os = SystemInfo.operatingSystem;

            if (os.Contains("Windows"))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 生成数据表格
        /// </summary>
        /// <param name="isMac"></param>
        private static void GenerateDataTables(bool isMac)
        {
            if (isMac)
            {
                ProcessCommandUtility.ProcessCommand("/bin/sh", "ExcelToCsv.sh", WorkingDirectory_Excel);
            }
            else
            {
                ProcessCommandUtility.ProcessCommand("ExcelToCsv.bat", "", WorkingDirectory_Excel);
            }


            string dataTablePath = Utility.Path.GetRegularPath($"{Application.dataPath}/{DataTableGenerator.DataTablePath.Replace("Assets/", "")}");
            if (Directory.Exists(dataTablePath))
            {
                string[] fileNames = Directory.GetFiles(dataTablePath, "*", SearchOption.AllDirectories);
                foreach (string fileName in fileNames)
                {
                    File.Delete(fileName);
                }
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(OutputPath);
            foreach (var fileInfo in directoryInfo.GetFiles())
            {
                if (fileInfo.Extension == ".csv")
                {
                    string dataTableName = fileInfo.Name.Replace(fileInfo.Extension, "");

                    if (dataTableName == "GlobalConfig")
                        continue;

                    DataTableProcessor dataTableProcessor = DataTableGenerator.CreateDataTableProcessor(fileInfo.FullName);
                    if (!DataTableGenerator.CheckRawData(dataTableProcessor, dataTableName))
                    {
                        Debug.LogError(Utility.Text.Format("Check raw data failure. DataTableName='{0}'", dataTableName));
                        break;
                    }

                    DataTableGenerator.GenerateDataFile(dataTableProcessor, dataTableName);
                    DataTableGenerator.GenerateCodeFile(dataTableProcessor, dataTableName);
                }
            }

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 生成数据表格
        /// </summary>
        /// <param name="isMac"></param>
        // private static void GenerateDataTables(bool isMac)
        // {
        //     // string workingDirectory = $"{Application.dataPath}/../../Doc/Excel/tool";
        //     // string outputPath = $"{Application.dataPath}/../../Doc/Excel/csv";

        //     if (isMac)
        //     {
        //         EditorCoroutineLauncher.StartEditorCoroutine(ProcessCommandUtility.ProcessCommandAsync("/bin/sh", "ExcelToCsv.sh", WorkingDirectory_Excel, () =>
        //         {
        //             Csv2Bytes(OutputPath);
        //         }));
        //     }
        //     else
        //     {
        //         ProcessCommandUtility.ProcessCommand("ExcelToCsv.bat", "", WorkingDirectory_Excel);
        //         Csv2Bytes(OutputPath);
        //     }

        // }

        // private static void Csv2Bytes(string outputPath)
        // {
        //     string dataTablePath = Utility.Path.GetRegularPath($"{Application.dataPath}/{DataTableGenerator.DataTablePath.Replace("Assets/", "")}");
        //     if (Directory.Exists(dataTablePath))
        //     {
        //         string[] fileNames = Directory.GetFiles(dataTablePath, "*", SearchOption.AllDirectories);
        //         foreach (string fileName in fileNames)
        //         {
        //             File.Delete(fileName);
        //         }
        //     }


        //     DirectoryInfo directoryInfo = new DirectoryInfo(outputPath);
        //     foreach (var fileInfo in directoryInfo.GetFiles())
        //     {
        //         if (fileInfo.Extension == ".csv")
        //         {
        //             string dataTableName = fileInfo.Name.Replace(fileInfo.Extension, "");

        //             if (dataTableName == "GlobalConfig")
        //                 continue;

        //             DataTableProcessor dataTableProcessor = DataTableGenerator.CreateDataTableProcessor(fileInfo.FullName);
        //             if (!DataTableGenerator.CheckRawData(dataTableProcessor, dataTableName))
        //             {
        //                 Debug.LogError(Utility.Text.Format("Check raw data failure. DataTableName='{0}'", dataTableName));
        //                 break;
        //             }

        //             DataTableGenerator.GenerateDataFile(dataTableProcessor, dataTableName);
        //             DataTableGenerator.GenerateCodeFile(dataTableProcessor, dataTableName);
        //         }
        //     }

        //     AssetDatabase.Refresh();
        // }

        /// <summary>
        /// 生成多语言配置
        /// </summary>
        /// <param name="isMac"></param>
        private static void GenerateLanguage(bool isMac)
        {
            if (isMac)
            {
                ProcessCommandUtility.ProcessCommand("/bin/sh", "uiText.sh", WorkingDirectory_LanguageTool_Java);
            }
            else
            {
                // ProcessCommand("uiText.bat", "", Application.dataPath + "/../Doc/TranslationTool/");
            }

            AssetDatabase.Refresh();
        }

        private static void CreateDataTableType()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo("Assets/Game/DataTables");
            FileInfo[] assetsFile = directoryInfo.GetFiles("*", SearchOption.AllDirectories).ToArray();

            List<string> fileNames = new List<string>();

            // 遍历所有资源并检查名称中是否包含"#"符号
            foreach (FileInfo fileInfo in assetsFile)
            {
                if (fileInfo.Name.Contains(".meta")) continue;

                string fileName = fileInfo.Name;

                fileName = fileName.Replace(fileInfo.Extension, "");

                fileNames.Add(fileName);
            }

            string enumType = string.Empty;

            int index = 0;
            foreach (var fileName in fileNames)
            {
                enumType += $"    {fileName},";

                index++;
                if (index < fileNames.Count)
                {
                    enumType += "\n";
                }
            }

            string temScript = @"
public enum DataTableType
{
**Content**
}  ";

            temScript = temScript.Replace("**Content**", enumType);
            File.WriteAllText("Assets/Scripts/Definition/DataTableType.cs", temScript,
                Encoding.UTF8);

            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
        }

        private static void CreateConfigFieldInfo()
        {
            string configPath = "Assets/Game/Configs/GlobalConfig.csv";

            TextAsset textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(configPath);

            // 遍历所有节点获取其名字
            string[] stringLines = textAsset.text.Split('\n');

            List<string> configNames = new List<string>();
            foreach (var stringLine in stringLines)
            {
                if (stringLine.Contains("#")) continue;

                string[] values = stringLine.Split('|');

                configNames.Add(values[2]);
            }

            string temScript = @"
namespace CasualGame
{
    public static partial class Constant
    {
        public static class Config
        {
**Content**
        }
    }
}
";

            string configContent = string.Empty;

            int index = 0;
            foreach (var configName in configNames)
            {
                configContent += $"            public const string {configName.Replace('.', '_')} = \"{configName}\";";

                index++;
                if (index < configNames.Count)
                {
                    configContent += "\n";
                }
            }

            temScript = temScript.Replace("**Content**", configContent);
            File.WriteAllText("Assets/Scripts/Definition/Constant/Constant.Config.cs", temScript,
                Encoding.UTF8);

            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
        }

        private static void CreateUIFormType()
        {
            IDataTable<DRUIForm> druiFormDtDataTable = DataTableCheck.GetDataTable<DRUIForm>();

            string enumType = string.Empty;

            int index = 0;
            foreach (var drUIForm in druiFormDtDataTable)
            {
                enumType += $"        {drUIForm.Name} = {drUIForm.Id},";

                index++;
                if (index < druiFormDtDataTable.Count)
                {
                    enumType += "\n";
                }
            }

            string temScript = @"
namespace CasualGame
{
    public enum UIFormType
    {
        Undefined = 0,
**Content**
    }  
}";

            temScript = temScript.Replace("**Content**", enumType);

            string scriptPath = "Assets/Scripts/Definition/UIFormType.cs";
            File.WriteAllText(scriptPath, temScript, Encoding.UTF8);

            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
        }


        private static void CreateUIPrefabAndScript()
        {
            IDataTable<DRUIForm> druiFormDtDataTable = DataTableCheck.GetDataTable<DRUIForm>();
            const string DefaultPrefabPath = "Assets/Game/Prefabs/";
            const string DefaultScriptPath = "Assets/Scripts/UI/";
            const string TempUIPath = "Assets/Editor/Tools/TempUI.prefab";
            const string TemplateScript =
                @"//------------------------------------------------------------
// File : TemplateScript.cs
// Email: mailto:TemplateAuthor@kingboat.io
// Desc : 
//------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CasualGame
{
    public class TemplateScript : UGuiForm
    {
        
    } 
}
";

            ResourceIdentificationTool.InitInEditor();

            foreach (var drUIForm in druiFormDtDataTable)
            {
                int id = drUIForm.Id;
                string directoryName = drUIForm.DirectoryName;
                if (directoryName == "None")
                {
                    directoryName = string.Empty;
                }
                string uiName = drUIForm.Name;

                // 已经生成了，跳过自动生成
                if (ResourceIdentificationTool.Instance.GetAssetPathById(id) != String.Empty) continue;

                // 生成预制体
                string prefabPath = $"{DefaultPrefabPath}{directoryName}/{uiName}";
                Directory.CreateDirectory(prefabPath);
                string newPrefabPath = $"{prefabPath}/{uiName}#{id}.prefab";
                if (AssetDatabase.CopyAsset(TempUIPath, newPrefabPath))
                {
                    //打开对应预制体
                    GameObject newUIPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(newPrefabPath);
                    AssetDatabase.OpenAsset(newUIPrefab);
                    //聚焦Scene窗口
                    EditorWindow.FocusWindowIfItsOpen<SceneView>();
                    // 在Project窗口高亮显示
                    EditorGUIUtility.PingObject(newUIPrefab);
                    // 在Inspector中显示预制体
                    Selection.activeObject = newUIPrefab;
                }

                // 生成脚本
                string scriptDirectory = $"{DefaultScriptPath}/{directoryName}/{uiName}";
                Directory.CreateDirectory(scriptDirectory);
                string newScriptPath = $"{scriptDirectory}/{uiName}.cs";

                if (!File.Exists(newScriptPath))
                {
                    string temScript = TemplateScript.Replace("TemplateScript", uiName);
                    File.WriteAllText(newScriptPath, temScript, Encoding.UTF8);
                }
            }

            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
        }
    }
}
