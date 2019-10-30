﻿using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MiraiNotes.Shared.Utils
{
    public class EncryptUtil
    {
        //Encrypt
        public static string EncryptString(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;
            byte[] initVectorBytes = Encoding.UTF8.GetBytes(Secrets.InitVector);
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            PasswordDeriveBytes password = new PasswordDeriveBytes(Secrets.Password, null);
            byte[] keyBytes = password.GetBytes(Secrets.KeySize / 8);
            RijndaelManaged symmetricKey = new RijndaelManaged
            {
                Mode = CipherMode.CBC
            };
            ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);
            using (var memoryStream = new MemoryStream())
            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
            {
                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                cryptoStream.FlushFinalBlock();
                byte[] cipherTextBytes = memoryStream.ToArray();
                return Convert.ToBase64String(cipherTextBytes);
            }
        }

        //Decrypt
        public static string DecryptString(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;
            byte[] initVectorBytes = Encoding.UTF8.GetBytes(Secrets.InitVector);
            byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
            PasswordDeriveBytes password = new PasswordDeriveBytes(Secrets.Password, null);
            byte[] keyBytes = password.GetBytes(Secrets.KeySize / 8);
            RijndaelManaged symmetricKey = new RijndaelManaged
            {
                Mode = CipherMode.CBC
            };
            ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);
            using (var memoryStream = new MemoryStream(cipherTextBytes))
            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
            {
                byte[] plainTextBytes = new byte[cipherTextBytes.Length];
                int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);

                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
            }
        }
    }
}
