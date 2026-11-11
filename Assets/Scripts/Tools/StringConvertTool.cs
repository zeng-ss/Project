//------------------------------------------------------------
// File : StringConvertTool.cs
// Desc : 
//------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CasualGame
{
    public class StringConvertTool : MonoBehaviour
    {
        public static string StringToBase64(string text)
        {
            // 将JSON字符串转换为字节数组
            byte[] textBytes = Encoding.UTF8.GetBytes(text);

            // 将字节数组转换为Base64编码的字符串
            string base64String = Convert.ToBase64String(textBytes);

            return base64String;
        }

        public static string Base64ToString(string base64)
        {
            // 将Base64编码的字符串转换回字节数组
            byte[] decodedBytes = Convert.FromBase64String(base64);

            // 将字节数组转换回JSON字符串
            string decodedString = Encoding.UTF8.GetString(decodedBytes);
            return decodedString;
        }
    }
}
