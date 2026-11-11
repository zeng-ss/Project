//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System.IO;

namespace CasualGame
{
    public sealed partial class DataTableProcessor
    {
        private sealed class StringArrayProcessor : GenericDataProcessor<string[]>
        {
            public override bool IsSystem
            {
                get { return true; }
            }

            public override string LanguageKeyword
            {
                get { return "string[]"; }
            }

            public override string[] GetTypeStrings()
            {
                return new string[]
                {
                    "string[]",
                    "system.string[]"
                };
            }

            public override string[] Parse(string value)
            {
                return value.Split(',');
            }

            public override void WriteToStream(DataTableProcessor dataTableProcessor, BinaryWriter binaryWriter,
                string value)
            {
                string[] p = value.Split(',');
                binaryWriter.Write(p.Length);
                foreach (var str in p)
                {
                    binaryWriter.Write(str);
                }
            }
        }
    }
}