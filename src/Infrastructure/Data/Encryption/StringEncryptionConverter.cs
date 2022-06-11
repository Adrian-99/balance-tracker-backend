using Application.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Encryption
{
    internal class StringEncryptionConverter : EncryptionConverter<string>
    {
        public StringEncryptionConverter(EncryptionSettings encryptionSettings) :
            base(s => s, s => s, encryptionSettings)
        { }
    }
}
