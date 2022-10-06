using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Application.Utilities
{
    public static class EncryptionUtils
    {
        public static string? EncryptWithAES(string? dataToEncrypt, out string? key, out string? IV)
        {
            if (dataToEncrypt != null && dataToEncrypt != "")
            {
                using (var aes = Aes.Create())
                {
                    key = Convert.ToBase64String(aes.Key);
                    IV = Convert.ToBase64String(aes.IV);

                    var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                    using (var memoryStream = new MemoryStream())
                    {
                        using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                        {
                            using (var streamWriter = new StreamWriter(cryptoStream))
                            {
                                streamWriter.Write(dataToEncrypt);
                            }
                            return Convert.ToBase64String(memoryStream.ToArray());
                        }
                    }
                }
            }
            else
            {
                key = null;
                IV = null;
                return null;
            }
        }

        public static string? DecryptWithAES(string? encryptedData, string? key, string? IV)
        {
            if (encryptedData != null && key != null && IV != null)
            {
                using (var aes = Aes.Create())
                {
                    aes.Key = Convert.FromBase64String(key);
                    aes.IV = Convert.FromBase64String(IV);

                    var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    using (var memoryStream = new MemoryStream(Convert.FromBase64String(encryptedData)))
                    {
                        using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            using (var streamReader = new StreamReader(cryptoStream))
                            {
                                return streamReader.ReadToEnd();
                            }
                        }
                    }
                }
            } 
            else
            {
                return null;
            }
        }
    }
}
