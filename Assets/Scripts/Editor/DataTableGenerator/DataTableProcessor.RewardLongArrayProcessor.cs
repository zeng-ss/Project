using System.IO;

namespace CasualGame
{
    public sealed partial class DataTableProcessor
    {
        private sealed class LongArrayProcessor : GenericDataProcessor<long[]>
        {
            public override bool IsSystem
            {
                get
                {
                    return false;
                }
            }

            public override string LanguageKeyword
            {
                get
                {
                    return "long[]";
                }
            }

            public override string[] GetTypeStrings()
            {
                return new string[]
                {
                    "long[]",
                    "System.Int64[]"
                };
            }

            public override long[] Parse(string value)
            {
                string[] splitValue = value.Split(',');
                long[] result = new long[splitValue.Length];
                for (int i = 0; i < splitValue.Length; i++)
                {
                    result[i] = long.Parse(splitValue[i]);
                }

                return result;
            }

            public override void WriteToStream(DataTableProcessor dataTableProcessor, BinaryWriter binaryWriter, string value)
            {
                long[] intArray = Parse(value);
                binaryWriter.Write(intArray.Length);
                foreach (var elementValue in intArray)
                {
                    binaryWriter.Write(elementValue);
                }
            }
        }
    }
}
