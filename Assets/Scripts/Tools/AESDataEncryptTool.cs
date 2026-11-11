//------------------------------------------------------------
// File : AESDataEncryptTool.cs
// Email: mailto:zewei.zhuang@kingboat.io
// Desc : 
//------------------------------------------------------------

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;


public class AESDataEncryptTool
{
    private static AesManaged _encryptAesManaged;

    // 加密
    public static byte[] EncryptString(string originString, string keyString = null, string ivString = null)
    {
        if (_encryptAesManaged == null)
        {
            _encryptAesManaged = new AesManaged();
            _encryptAesManaged.Mode = CipherMode.CBC;
        }

        if (keyString != null)
        {
            _encryptAesManaged.Key = BinaryToStringHelper.Base64StringToBytes(keyString);
        }
        else
        {
            _encryptAesManaged.GenerateKey();
        }

        if (ivString != null)
        {
            _encryptAesManaged.IV = BinaryToStringHelper.Base64StringToBytes(ivString);
        }
        else
        {
            _encryptAesManaged.GenerateIV();
        }

        byte[] encryptedData;
        using (MemoryStream memoryStream = new MemoryStream())
        {
            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, _encryptAesManaged.CreateEncryptor(),
                       CryptoStreamMode.Write))
            {
                using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                {
                    streamWriter.Write(originString);
                }

                encryptedData = memoryStream.ToArray();
            }
        }

        return encryptedData;
    }

    // 解密
    public static string DecryptString(byte[] encryptedBytes, string keyString, string ivString)
    {
        byte[] key = BinaryToStringHelper.Base64StringToBytes(keyString);
        byte[] iv = BinaryToStringHelper.Base64StringToBytes(ivString);

        using (AesManaged aesManaged = new AesManaged())
        {
            aesManaged.Key = key;
            aesManaged.IV = iv;

            byte[] decryptedData;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aesManaged.CreateDecryptor(),
                           CryptoStreamMode.Write))
                {
                    cryptoStream.Write(encryptedBytes, 0, encryptedBytes.Length);
                    cryptoStream.FlushFinalBlock();
                    decryptedData = memoryStream.ToArray();
                }
            }

            return BinaryToStringHelper.BytesToUTF8String(decryptedData);
        }
    }
}