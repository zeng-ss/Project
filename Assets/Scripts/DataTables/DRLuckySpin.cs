//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------
// 此文件由工具自动生成，请勿直接修改。
//------------------------------------------------------------

using GameFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace CasualGame
{
    /// <summary>
    /// Id。
    /// </summary>
    [OPS.Obfuscator.Attribute.DoNotObfuscateClass]
    public class DRLuckySpin : DataRowBase
    {
        private int m_Id = 0;

        /// <summary>
        /// 获取Id。
        /// </summary>
        public override int Id
        {
            get
            {
                return m_Id;
            }
        }

        /// <summary>
        /// 获取Index。
        /// </summary>
        public int Index
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取ItemId。
        /// </summary>
        public int ItemId
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取ItemNum。
        /// </summary>
        public int ItemNum
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Probability。
        /// </summary>
        public int Probability
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Type。
        /// </summary>
        public int Type
        {
            get;
            private set;
        }

        public override bool ParseDataRow(string dataRowString, object userData)
        {
            string[] columnStrings = dataRowString.Split(DataTableExtension.DataSplitSeparators);
            for (int i = 0; i < columnStrings.Length; i++)
            {
                columnStrings[i] = columnStrings[i].Trim(DataTableExtension.DataTrimSeparators);
            }

            int index = 0;
            index++;
            m_Id = int.Parse(columnStrings[index++]);
            Index = int.Parse(columnStrings[index++]);
            ItemId = int.Parse(columnStrings[index++]);
            ItemNum = int.Parse(columnStrings[index++]);
            Probability = int.Parse(columnStrings[index++]);
            Type = int.Parse(columnStrings[index++]);

            GeneratePropertyArray();
            return true;
        }

        public override bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)
        {
            using (MemoryStream memoryStream = new MemoryStream(dataRowBytes, startIndex, length, false))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
                {
                    m_Id = binaryReader.Read7BitEncodedInt32();
                    Index = binaryReader.Read7BitEncodedInt32();
                    ItemId = binaryReader.Read7BitEncodedInt32();
                    ItemNum = binaryReader.Read7BitEncodedInt32();
                    Probability = binaryReader.Read7BitEncodedInt32();
                    Type = binaryReader.Read7BitEncodedInt32();
                }
            }

            GeneratePropertyArray();
            return true;
        }

        private void GeneratePropertyArray()
        {

        }
    }
}
