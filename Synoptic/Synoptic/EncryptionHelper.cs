using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Synoptic
{
    public class EncryptionHelper
    {

        public static string Hash(string password)
        {
            using (var md5Hahser = MD5.Create())
            {
                var data = md5Hahser.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(data).Replace("-", "").Substring(0, 16);
            }
        }
        public static string Encrypt(string key, string text)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using(Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using(MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream((Stream)ms, encryptor, CryptoStreamMode.Write))
                    {
                        using(StreamWriter sw = new StreamWriter((Stream)cs))
                        {
                            sw.Write(text);
                        }

                        array = ms.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        public static string Decrypt(string key, string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
