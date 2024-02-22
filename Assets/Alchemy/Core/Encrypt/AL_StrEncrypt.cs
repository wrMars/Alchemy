using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

/*----------------------------
// Alchemy framework
// @author wr
// a tool for dn developer
//--------------------------*/
namespace Alchemy
{
    public static class AL_StrEncrypt
    {
        private static string priKey = "";
        private static string pubKey = "";

        public static string DecryptRsa(string encryptString)
        {
            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(priKey);
            var PlainTextBArray = Convert.FromBase64String(encryptString);
            var DypherTextBArray = rsa.Decrypt(PlainTextBArray, false);
            var re = (new UnicodeEncoding()).GetString(DypherTextBArray);
            rsa.Clear();
            return re;
        }

        public static string EncryptRsa(string originalStr)
        {
            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(pubKey);
            var PlainTextBArray = (new UnicodeEncoding()).GetBytes(originalStr);
            var CypherTextBArray = rsa.Encrypt(PlainTextBArray, false);
            var re = Convert.ToBase64String(CypherTextBArray);
            rsa.Clear();
            return re;
        }
        
        private static List<string> _fakeKey = new List<string>(){"p,m==lk#k*libl", "k,m0dikmao^dyg2*=", "u,mxmqxu@dub4&kk="};

        public static string GetOneFakeKey()
        {
            System.Random random = new System.Random();
            var index = random.Next(2);
            return _fakeKey[index];
        }

        // [MenuItem("Alchemy/TestStrEncrypt")]
        private static void TestStrEncrypt()
        {
            UpdateAes("Alchemy", "ssafs");
            var en = EncryptAes("TestStrEncrypt");
            var de = DecryptAes(en);
            Debug.LogError($"en:{en}, de:{de}");

        }

        private static byte[] _aeskey;
        private static byte[] _aesIv;
        public static void UpdateAes(string key, string iv)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] ivBytes = Encoding.UTF8.GetBytes(iv);
 
            byte[] useKeyBytes = new byte[16];
            byte[] useIvBytes = new byte[16];
 
            if (keyBytes.Length > useKeyBytes.Length)
                Array.Copy(keyBytes, useKeyBytes, useKeyBytes.Length);
            else
                Array.Copy(keyBytes, useKeyBytes, keyBytes.Length);

            if (ivBytes.Length > useIvBytes.Length)
                Array.Copy(ivBytes, useIvBytes, useIvBytes.Length);
            else
                Array.Copy(ivBytes, useIvBytes, ivBytes.Length);

            _aeskey = useKeyBytes;
            _aesIv = useIvBytes;
        }

        public static string EncryptAes(string originalStr)
        {
            if (_aeskey == null)
            {
                Debug.LogError("未有key时候尝试加密字符串，失败");
                return "";
            }
            using (Aes aes = Aes.Create())
            {
                byte[] data = Encoding.UTF8.GetBytes(originalStr);
                aes.Key = _aeskey;
                aes.IV = _aesIv;

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(data, 0, data.Length);
                        cryptoStream.FlushFinalBlock();
                        return Convert.ToBase64String(memoryStream.ToArray());
                    }
                }
            }
        }

        public static string DecryptAes(string encryptString)
        {
            if (string.IsNullOrEmpty(encryptString)) return "";
            if (_aeskey == null)
            {
                Debug.LogError("未有key时候尝试解密字符串，失败");
                return "";
            }
            using (Aes aes = Aes.Create())
            {
                aes.Key = _aeskey;
                aes.IV = _aesIv;
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        byte[] encryptedBytes = Convert.FromBase64String(encryptString);
                        cryptoStream.Write(encryptedBytes, 0, encryptedBytes.Length);
                        cryptoStream.FlushFinalBlock();
                        byte[] decryptedBytes = memoryStream.ToArray();
                        return Encoding.UTF8.GetString(decryptedBytes);
                    }
                }
            }
        }
    }
}