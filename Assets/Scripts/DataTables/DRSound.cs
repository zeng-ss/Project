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
    public class DRSound : DataRowBase
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
        /// 获取AssetId。
        /// </summary>
        public int AssetId
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取SoundGroup。
        /// </summary>
        public string SoundGroup
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取SpatialBlend。
        /// </summary>
        public float SpatialBlend
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Time。
        /// </summary>
        public float Time
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Volume。
        /// </summary>
        public float Volume
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Priority。
        /// </summary>
        public int Priority
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取MaxDistance。
        /// </summary>
        public float MaxDistance
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取DopplerLevel。
        /// </summary>
        public float DopplerLevel
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Pitch。
        /// </summary>
        public float Pitch
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Loop。
        /// </summary>
        public bool Loop
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
            AssetId = int.Parse(columnStrings[index++]);
            SoundGroup = columnStrings[index++];
            SpatialBlend = float.Parse(columnStrings[index++]);
            Time = float.Parse(columnStrings[index++]);
            Volume = float.Parse(columnStrings[index++]);
            Priority = int.Parse(columnStrings[index++]);
            MaxDistance = float.Parse(columnStrings[index++]);
            DopplerLevel = float.Parse(columnStrings[index++]);
            Pitch = float.Parse(columnStrings[index++]);
            Loop = bool.Parse(columnStrings[index++]);

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
                    AssetId = binaryReader.Read7BitEncodedInt32();
                    SoundGroup = binaryReader.ReadString();
                    SpatialBlend = binaryReader.ReadSingle();
                    Time = binaryReader.ReadSingle();
                    Volume = binaryReader.ReadSingle();
                    Priority = binaryReader.Read7BitEncodedInt32();
                    MaxDistance = binaryReader.ReadSingle();
                    DopplerLevel = binaryReader.ReadSingle();
                    Pitch = binaryReader.ReadSingle();
                    Loop = binaryReader.ReadBoolean();
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
