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
    public class DRItemIAP : DataRowBase
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
        /// 获取ProductId。
        /// </summary>
        public string ProductId
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取ProductType。
        /// </summary>
        public int ProductType
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Platform。
        /// </summary>
        public int Platform
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取PrizeValue。
        /// </summary>
        public float PrizeValue
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取TikTokBeans。
        /// </summary>
        public int TikTokBeans
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取RevType。
        /// </summary>
        public int RevType
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取BaseProductId。
        /// </summary>
        public string BaseProductId
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取OriginalPrize。
        /// </summary>
        public float OriginalPrize
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取EventName。
        /// </summary>
        public string EventName
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
            ProductId = columnStrings[index++];
            ProductType = int.Parse(columnStrings[index++]);
            Platform = int.Parse(columnStrings[index++]);
            PrizeValue = float.Parse(columnStrings[index++]);
            TikTokBeans = int.Parse(columnStrings[index++]);
            RevType = int.Parse(columnStrings[index++]);
            BaseProductId = columnStrings[index++];
            OriginalPrize = float.Parse(columnStrings[index++]);
            EventName = columnStrings[index++];

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
                    ProductId = binaryReader.ReadString();
                    ProductType = binaryReader.Read7BitEncodedInt32();
                    Platform = binaryReader.Read7BitEncodedInt32();
                    PrizeValue = binaryReader.ReadSingle();
                    TikTokBeans = binaryReader.Read7BitEncodedInt32();
                    RevType = binaryReader.Read7BitEncodedInt32();
                    BaseProductId = binaryReader.ReadString();
                    OriginalPrize = binaryReader.ReadSingle();
                    EventName = binaryReader.ReadString();
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
