using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Settings
{
    public class EncryptionSettings
    {
        public string Key { get; set; }
        public string IV { get; set; }

        public byte[] KeyBytes { get => Encoding.UTF8.GetBytes(Key); }
        public byte[] IVBytes { get => Encoding.UTF8.GetBytes(IV); }

        public static EncryptionSettings Get(IConfiguration configuration)
        {
            return configuration.GetSection("Encryption")
                .Get<EncryptionSettings>();
        }
    }
}
