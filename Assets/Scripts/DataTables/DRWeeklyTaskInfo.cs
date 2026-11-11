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
    public class DRWeeklyTaskInfo : DataRowBase
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
        /// 获取IconName。
        /// </summary>
        public string IconName
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取TargetValue。
        /// </summary>
        public int TargetValue
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取RewardStar。
        /// </summary>
        public int RewardStar
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取DescriptionKey。
        /// </summary>
        public string DescriptionKey
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
            IconName = columnStrings[index++];
            TargetValue = int.Parse(columnStrings[index++]);
            RewardStar = int.Parse(columnStrings[index++]);
            DescriptionKey = columnStrings[index++];

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
                    IconName = binaryReader.ReadString();
                    TargetValue = binaryReader.Read7BitEncodedInt32();
                    RewardStar = binaryReader.Read7BitEncodedInt32();
                    DescriptionKey = binaryReader.ReadString();
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
