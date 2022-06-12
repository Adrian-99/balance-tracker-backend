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
        public string PrivateKeyPath { get; set; }
        public string? PrivateKeyPassword { get; set; }
        public string PublicKeyPath { get; set; }

        public static EncryptionSettings Get(IConfiguration configuration)
        {
            return configuration.GetSection("Encryption")
                .Get<EncryptionSettings>();
        }
    }
}
