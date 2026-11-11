//------------------------------------------------------------
// File : CsvLocalizationHelper.cs
// Email: mailto:zhiqiang.yang@kingboat.io
// Desc : Csv 格式的本地化辅助器。
//------------------------------------------------------------

using System;
using System.IO;
using GameFramework;
using UnityGameFramework.Runtime;
using GameFramework.Localization;

namespace CasualGame
{
    [OPS.Obfuscator.Attribute.DoNotObfuscateClass]
    public class CsvLocalizationHelper : DefaultLocalizationHelper
    {
        public override bool ParseData(ILocalizationManager localizationManager, string dictionaryString,
            object userData)
        {
            try
            {
                byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(dictionaryString);

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

                    if (!localizationManager.AddRawString(key + "", text))
                    {
                        Log.Warning("Can not add raw string with key '{0}' which may be invalid or duplicate.", key);
                        return false;
                    }
                }

                reader.Dispose();
            }
            catch (Exception exception)
            {
                Log.Warning("Can not parse dictionary data with exception '{0}'.", exception.ToString());
                return false;
            }

            return true;
        }
    }
}