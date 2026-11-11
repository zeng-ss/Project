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
    public class DRDialogGuide : DataRowBase
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
        /// 获取AnchorID。
        /// </summary>
        public int AnchorID
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取ClothID。
        /// </summary>
        public int ClothID
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Content。
        /// </summary>
        public string Content
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取AudioAssetID。
        /// </summary>
        public int AudioAssetID
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
            AnchorID = int.Parse(columnStrings[index++]);
            ClothID = int.Parse(columnStrings[index++]);
            Content = columnStrings[index++];
            AudioAssetID = int.Parse(columnStrings[index++]);

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
                    AnchorID = binaryReader.Read7BitEncodedInt32();
                    ClothID = binaryReader.Read7BitEncodedInt32();
                    Content = binaryReader.ReadString();
                    AudioAssetID = binaryReader.Read7BitEncodedInt32();
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
