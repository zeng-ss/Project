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
    public class DRPetInfo : DataRowBase
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
        /// 获取DefaultCloth。
        /// </summary>
        public bool DefaultCloth
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取MainTabCategory。
        /// </summary>
        public int MainTabCategory
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取PetSkinType。
        /// </summary>
        public int PetSkinType
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Cost。
        /// </summary>
        public int Cost
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Attribute。
        /// </summary>
        public int Attribute
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Desc。
        /// </summary>
        public string Desc
        {
            get;
            private set;
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
        /// 获取SpinePath。
        /// </summary>
        public string SpinePath
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取SkinName。
        /// </summary>
        public string SkinName
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取PetSpineAnimation。
        /// </summary>
        public string PetSpineAnimation
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
            DefaultCloth = bool.Parse(columnStrings[index++]);
            MainTabCategory = int.Parse(columnStrings[index++]);
            PetSkinType = int.Parse(columnStrings[index++]);
            Cost = int.Parse(columnStrings[index++]);
            Attribute = int.Parse(columnStrings[index++]);
            Desc = columnStrings[index++];
            IconName = columnStrings[index++];
            SpinePath = columnStrings[index++];
            SkinName = columnStrings[index++];
            PetSpineAnimation = columnStrings[index++];

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
                    DefaultCloth = binaryReader.ReadBoolean();
                    MainTabCategory = binaryReader.Read7BitEncodedInt32();
                    PetSkinType = binaryReader.Read7BitEncodedInt32();
                    Cost = binaryReader.Read7BitEncodedInt32();
                    Attribute = binaryReader.Read7BitEncodedInt32();
                    Desc = binaryReader.ReadString();
                    IconName = binaryReader.ReadString();
                    SpinePath = binaryReader.ReadString();
                    SkinName = binaryReader.ReadString();
                    PetSpineAnimation = binaryReader.ReadString();
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
