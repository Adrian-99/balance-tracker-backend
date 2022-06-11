using Application.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Encryption
{
    internal class DecimalEncryptionConverter : EncryptionConverter<decimal>
    {
        public DecimalEncryptionConverter(EncryptionSettings encryptionSettings) :
            base(d => d.ToString(), s => decimal.Parse(s), encryptionSettings)
        { }
    }
}
