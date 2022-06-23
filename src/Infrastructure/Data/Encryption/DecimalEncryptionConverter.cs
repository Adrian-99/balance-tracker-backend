using Application.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Encryption
{
    public class DecimalEncryptionConverter : EncryptionConverter<decimal>
    {
        public DecimalEncryptionConverter(EncryptionSettings encryptionSettings) :
            base(d => d.ToString(), s => decimal.Parse(s), encryptionSettings)
        { }
    }
}
