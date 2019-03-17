using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

//https://www.selamigungor.com/post/7/encrypt-decrypt-a-string-in-csharp

public class LB_CryptoManager
{
    private const string keyString = "enterYourKeyHere";

    public string Encrypt(string plainText)
    {
        if (plainText == null)
        {
            return null;
        }

        var bytesToBeEncrypted = Encoding.UTF8.GetBytes(plainText);
        var passwordBytes = Encoding.UTF8.GetBytes(keyString);

        passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

        var bytesEncrypted = Encrypt(bytesToBeEncrypted, passwordBytes);

        return Convert.ToBase64String(bytesEncrypted);
    }

    public string Decrypt(string encryptedText)
    {
        if (encryptedText == null)
        {
            return null;
        }

        var bytesToBeDecrypted = Convert.FromBase64String(encryptedText);
        var passwordBytes = Encoding.UTF8.GetBytes(keyString);

        passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

        var bytesDecrypted = Decrypt(bytesToBeDecrypted, passwordBytes);

        return Encoding.UTF8.GetString(bytesDecrypted);
    }

    private byte[] Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
    {
        byte[] encryptedBytes = null;
        var saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

        using (MemoryStream ms = new MemoryStream())
        {
            using (RijndaelManaged AES = new RijndaelManaged())
            {
                var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);

                AES.KeySize = 256;
                AES.BlockSize = 128;
                AES.Key = key.GetBytes(AES.KeySize / 8);
                AES.IV = key.GetBytes(AES.BlockSize / 8);

                AES.Mode = CipherMode.CBC;

                using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                    cs.Close();
                }

                encryptedBytes = ms.ToArray();
            }
        }

        return encryptedBytes;
    }

    private byte[] Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
    {
        byte[] decryptedBytes = null;
        var saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

        using (MemoryStream ms = new MemoryStream())
        {
            using (RijndaelManaged AES = new RijndaelManaged())
            {
                var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);

                AES.KeySize = 256;
                AES.BlockSize = 128;
                AES.Key = key.GetBytes(AES.KeySize / 8);
                AES.IV = key.GetBytes(AES.BlockSize / 8);
                AES.Mode = CipherMode.CBC;

                using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                    cs.Close();
                }

                decryptedBytes = ms.ToArray();
            }
        }

        return decryptedBytes;
    }
}
