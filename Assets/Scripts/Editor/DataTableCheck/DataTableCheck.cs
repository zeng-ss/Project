using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GameFramework;
using GameFramework.DataTable;
using UnityEditor;
using UnityEngine;
using UnityGameFramework.Runtime;
using System.Reflection;
using GameFramework.Localization;

namespace CasualGame
{
    public interface IDataTableChecker
    {
        public void Check();
    }

    public static class DataTableCheck
    {
        private static readonly Dictionary<string, DataTableBase> _dataTableBases =
            new Dictionary<string, DataTableBase>();

        private static readonly Dictionary<string, string> m_Dictionary = new Dictionary<string, string>();

        [UnityEditor.MenuItem("Tools/Excel/检查表格配置")]
        public static void Check()
        {
            _dataTableBases.Clear();
            m_Dictionary.Clear();

            var assembly = Assembly.GetExecutingAssembly();
            var baseType = typeof(IDataTableChecker);

            var subTypes = assembly.GetTypes()
                .Where(t => baseType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var type in subTypes)
            {
                IDataTableChecker iDataTableChecker = (IDataTableChecker)Activator.CreateInstance(type);
                iDataTableChecker.Check();
            }

            m_Dictionary.Clear();
            _dataTableBases.Clear();
        }

        public static DataTableBase GetDataTable(Type type)
        {
            string key = type.Name;
            if (!_dataTableBases.TryGetValue(key, out DataTableBase dataTableBase))
            {
                dataTableBase = CreateDataTable(key.Replace("DR", ""));
            }

            return dataTableBase;
        }

        private static bool AddRawString(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new GameFrameworkException("Key is invalid.");
            }

            if (m_Dictionary.ContainsKey(key))
            {
                return false;
            }

            m_Dictionary.Add(key, value ?? string.Empty);
            return true;
        }

        public static string GetLocalizationString(string key)
        {
            if (key == "-1" || key == "-1.0") return string.Empty;

            if (m_Dictionary.Count == 0)
            {
                string path = $"Assets/Game/Localization/uiText_{Language.English.ToString()}.csv";
                TextAsset textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                ParseData(textAsset.text, null);
            }

            return m_Dictionary[key];
        }

        public static IDataTable<T> GetDataTable<T>() where T : IDataRow
        {
            return (IDataTable<T>)GetDataTable(typeof(T));
        }

        private static void AddDataTable(string dataTableName, DataTableBase dataTable)
        {
            dataTableName = $"DR{dataTableName}";
            _dataTableBases[dataTableName] = dataTable;
        }

        // 表格生成
        private static DataTableBase CreateDataTable(string dataTableName)
        {
            string binaryPath = DataTableGenerator.GetBinaryDataFileName(dataTableName);
            byte[] binaryBytes = File.ReadAllBytes(binaryPath);

            if (string.IsNullOrEmpty(dataTableName))
            {
                Log.Error("Data table name is invalid.");
                return null;
            }

            string[] splitedNames = dataTableName.Split('_');
            if (splitedNames.Length > 2)
            {
                Log.Error("Data table name is invalid.");
                return null;
            }

            string dataRowClassName = "DR" + splitedNames[0];

            Assembly[] s_Assemblies = AppDomain.CurrentDomain.GetAssemblies();

            Type dataRowType = null;
            foreach (var assembly in s_Assemblies)
            {
                if (dataRowType != null) break;

                var subTypes = assembly.GetTypes()
                    .Where(t => t.Name == dataRowClassName && !t.IsInterface && !t.IsAbstract);

                foreach (var type in subTypes)
                {
                    dataRowType = type;
                    break;
                }
            }

            string name = splitedNames.Length > 1 ? splitedNames[1] : null;
            DataTableBase dataTable = CreateDataTable(dataRowType, name);
            ParseData(dataTable, binaryBytes, 0, binaryBytes.Length, null);

            AddDataTable(dataTableName, dataTable);

            return dataTable;
        }

        private static DataTableBase CreateDataTable(Type dataRowType, string name)
        {
            if (dataRowType == null)
            {
                throw new GameFrameworkException("Data row type is invalid.");
            }

            if (!typeof(IDataRow).IsAssignableFrom(dataRowType))
            {
                throw new GameFrameworkException(Utility.Text.Format("Data row type '{0}' is invalid.",
                    dataRowType.FullName));
            }

            Type dataTableType = GetCustomDataTable(dataRowType);
            DataTableBase dataTable = (DataTableBase)Activator.CreateInstance(dataTableType, name);

            return dataTable;
        }

        public static Type GetCustomDataTable(Type dataRowType)
        {
            Type dataTableType = typeof(CustomDataTable<>).MakeGenericType(dataRowType);

            return dataTableType;
        }

        private static bool ParseData(DataTableBase dataTable, byte[] dataTableBytes, int startIndex, int length,
            object userData)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream(dataTableBytes, startIndex, length, false))
                {
                    using (BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
                    {
                        while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
                        {
                            int dataRowBytesLength = binaryReader.Read7BitEncodedInt32();
                            if (!dataTable.AddDataRow(dataTableBytes, (int)binaryReader.BaseStream.Position,
                                    dataRowBytesLength, userData))
                            {
                                Log.Warning("Can not parse data row bytes.");
                                return false;
                            }

                            binaryReader.BaseStream.Position += dataRowBytesLength;
                        }
                    }
                }

                return true;
            }
            catch (Exception exception)
            {
                Log.Warning("Can not parse dictionary bytes with exception '{0}'.", exception.ToString());
                return false;
            }
        }

        public static void ParseData(string dictionaryString, object userData)
        {
            try
            {
                byte[] byteArray = System.Text.Encoding.Default.GetBytes(dictionaryString);

                MemoryStream stream = new MemoryStream(byteArray);
                StreamReader reader = null;
                reader = new StreamReader(stream, System.Text.Encoding.UTF8);
                reader.ReadLine();

                for (string str2 = reader.ReadLine(); str2 != null; str2 = reader.ReadLine())
                {
                    string[] strArray = str2.Trim().Split('|');
                    if (strArray == null)
                    {
                        continue;
                    }

                    if (strArray.Length < 2)
                    {
                        Log.Error(Utility.Text.Format(
                            "CsvLocalizationHelper.ParseDictionary() ERROR, why length < 2, length = {0}",
                            strArray.Length));
                        continue;
                    }

                    int key = 0;
                    if (!int.TryParse(strArray[0], out key))
                    {
                        Log.Error(Utility.Text.Format(
                            "CsvLocalizationHelper.ParseDictionary() ERROR, why key <=0,key={0}", key));
                        continue;
                    }

                    string text = strArray[1];
                    text = text.Trim().Replace("\\n", "\n");

                    if (!AddRawString(key + "", text))
                    {
                        Log.Warning("Can not add raw string with key '{0}' which may be invalid or duplicate.", key);
                        return;
                    }
                }

                reader.Dispose();
            }
            catch (Exception exception)
            {
                Log.Warning("Can not parse dictionary data with exception '{0}'.", exception.ToString());
            }
        }
    }
}