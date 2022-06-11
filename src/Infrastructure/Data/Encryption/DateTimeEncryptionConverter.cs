using Application.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Encryption
{
    internal class DateTimeEncryptionConverter : EncryptionConverter<DateTime>
    {
        public DateTimeEncryptionConverter(EncryptionSettings encryptionSettings) :
            base(d => d.ToLongDateString(), s => DateTime.Parse(s), encryptionSettings)
        {
        }
    }
}
