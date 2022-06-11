using Application.Settings;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Encryption
{
    internal abstract class EncryptionConverter<T> : ValueConverter<T, string>
    {
        public EncryptionConverter(Func<T, string> toString, Func<string, T> fromString, EncryptionSettings encryptionSettings) : 
            base(from => Encrypt(from, toString, encryptionSettings), to => Decrypt(to, fromString, encryptionSettings))
        { }

        private static T Decrypt(string value, Func<string, T> fromString, EncryptionSettings encryptionSettings)
        {
            byte[] valueBytes = Convert.FromBase64String(value);
            string path = Path.Combine(encryptionSettings.PrivateKeyPath); 
            var collection = new X509Certificate2Collection();
            collection.Import(
                File.ReadAllBytes(path),
                null,
                X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet
                );
            var certificate = collection[0];
            var csp = certificate.GetRSAPrivateKey();
            if (csp == null)
            {
                throw new CryptographicException("Something went wrong during encryption");
            }
            var keys = Encoding.UTF8.GetString(csp.Decrypt(valueBytes, RSAEncryptionPadding.OaepSHA1));
            return fromString.Invoke(keys);
        }

        private static string Encrypt(T value, Func<T, string> toString, EncryptionSettings encryptionSettings)
        {
            byte[] valueBytes = Encoding.UTF8.GetBytes(toString.Invoke(value));
            string path = Path.GetFullPath(encryptionSettings.PublicKeyPath);
            var collection = new X509Certificate2Collection();
            collection.Import(path);
            var certificate = collection[0];
            var output = "";
            var csp = certificate.GetRSAPublicKey();
            if (csp == null)
            {
                throw new CryptographicException("Something went wrong during decryption");
            }
            byte[] bytesEncrypted = csp.Encrypt(valueBytes, RSAEncryptionPadding.OaepSHA1);
            output = Convert.ToBase64String(bytesEncrypted);
            return output;
        }
    }
}
