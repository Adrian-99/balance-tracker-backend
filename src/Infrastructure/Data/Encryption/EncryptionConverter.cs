using Application.Settings;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Encryption
{
    internal abstract class EncryptionConverter<T> : ValueConverter<T, string>
    {
        private static RSA privateKeyRSA = RSA.Create();
        private static RSA publicKeyRSA = RSA.Create();

        public EncryptionConverter(Func<T, string> toString, Func<string, T> fromString, EncryptionSettings encryptionSettings) : 
            base(from => Encrypt(from, toString), to => Decrypt(to, fromString))
        {
            var privateKeyString = File.ReadAllText(encryptionSettings.PrivateKeyPath);
            if (encryptionSettings.PrivateKeyPassword != null)
            {
                privateKeyRSA.ImportFromEncryptedPem(privateKeyString, encryptionSettings.PrivateKeyPassword);
            }
            else
            {
                privateKeyRSA.ImportFromPem(privateKeyString);
            }

            var publicKeyString = File.ReadAllText(encryptionSettings.PublicKeyPath);
            publicKeyRSA.ImportFromPem(publicKeyString);
        }

        private static T Decrypt(string value, Func<string, T> fromString)
        {
            byte[] valueBytes = Convert.FromBase64String(value);
            var decryptedString = Encoding.UTF8.GetString(privateKeyRSA.Decrypt(valueBytes, RSAEncryptionPadding.Pkcs1));
            return fromString.Invoke(decryptedString);
        }

        private static string Encrypt(T value, Func<T, string> toString)
        {
            byte[] valueBytes = Encoding.UTF8.GetBytes(toString.Invoke(value));
            byte[] bytesEncrypted = publicKeyRSA.Encrypt(valueBytes, RSAEncryptionPadding.Pkcs1);
            return Convert.ToBase64String(bytesEncrypted);
        }
    }
}
