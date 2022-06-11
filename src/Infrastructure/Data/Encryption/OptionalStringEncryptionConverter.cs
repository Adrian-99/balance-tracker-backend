using Application.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Encryption
{
    internal class OptionalStringEncryptionConverter : EncryptionConverter<string?>
    {
        public OptionalStringEncryptionConverter(EncryptionSettings encryptionSettings) :
            base(s => s != null ? s : "", s => s, encryptionSettings)
        { }
    }
}
