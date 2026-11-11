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
    public class DRActivityInfo : DataRowBase
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
        /// 获取UnlockLv。
        /// </summary>
        public int UnlockLv
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取ConditionType。
        /// </summary>
        public int ConditionType
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Duration。
        /// </summary>
        public int Duration
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Interval。
        /// </summary>
        public int Interval
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取SpinePath。
        /// </summary>
        public string SpinePath
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
            UnlockLv = int.Parse(columnStrings[index++]);
            ConditionType = int.Parse(columnStrings[index++]);
            Duration = int.Parse(columnStrings[index++]);
            Interval = int.Parse(columnStrings[index++]);
            SpinePath = columnStrings[index++];

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
                    UnlockLv = binaryReader.Read7BitEncodedInt32();
                    ConditionType = binaryReader.Read7BitEncodedInt32();
                    Duration = binaryReader.Read7BitEncodedInt32();
                    Interval = binaryReader.Read7BitEncodedInt32();
                    SpinePath = binaryReader.ReadString();
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
