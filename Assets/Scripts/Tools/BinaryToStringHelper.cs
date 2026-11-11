using System;
using System.Text;
using UnityEngine;

public static class BinaryToStringHelper
{
    /// <summary>
    /// 二进制数据转UTF-8字符串
    /// </summary>
    public static string BytesToUTF8String(byte[] bytes)
    {
        return Encoding.UTF8.GetString(bytes);
    }

    /// <summary>
    /// 二进制数据转Base64字符串
    /// </summary>
    public static string BytesToBase64String(byte[] bytes)
    {
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// 二进制数据转十六进制字符串
    /// </summary>
    public static string BytesToHexString(byte[] bytes)
    {
        StringBuilder hex = new StringBuilder(bytes.Length * 2);
        foreach (byte b in bytes)
        {
            hex.AppendFormat("{0:x2}", b);
        }
        return hex.ToString();
    }

    /// <summary>
    /// 二进制数据转ASCII字符串
    /// </summary>
    public static string BytesToASCIIString(byte[] bytes)
    {
        return Encoding.ASCII.GetString(bytes);
    }

    /// <summary>
    /// 字符串转二进制数据 (UTF-8)
    /// </summary>
    public static byte[] UTF8StringToBytes(string str)
    {
        return Encoding.UTF8.GetBytes(str);
    }

    /// <summary>
    /// Base64字符串转二进制数据
    /// </summary>
    public static byte[] Base64StringToBytes(string base64String)
    {
        return Convert.FromBase64String(base64String);
    }

    /// <summary>
    /// 十六进制字符串转二进制数据
    /// </summary>
    public static byte[] HexStringToBytes(string hex)
    {
        int numberChars = hex.Length;
        byte[] bytes = new byte[numberChars / 2];
        for (int i = 0; i < numberChars; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }
        return bytes;
    }
}