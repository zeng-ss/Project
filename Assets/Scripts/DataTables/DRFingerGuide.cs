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
    public class DRFingerGuide : DataRowBase
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
        /// 获取SceneName。
        /// </summary>
        public string SceneName
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取RootName。
        /// </summary>
        public string RootName
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取HigtLightTargetPath。
        /// </summary>
        public string HigtLightTargetPath
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取ButtonPath。
        /// </summary>
        public string ButtonPath
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Pivot。
        /// </summary>
        public Vector2 Pivot
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
            SceneName = columnStrings[index++];
            RootName = columnStrings[index++];
            HigtLightTargetPath = columnStrings[index++];
            ButtonPath = columnStrings[index++];
            Pivot = DataTableExtension.ParseVector2(columnStrings[index++]);

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
                    SceneName = binaryReader.ReadString();
                    RootName = binaryReader.ReadString();
                    HigtLightTargetPath = binaryReader.ReadString();
                    ButtonPath = binaryReader.ReadString();
                    Pivot = binaryReader.ReadVector2();
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
