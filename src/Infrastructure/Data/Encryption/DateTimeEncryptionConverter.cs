using Application.Settings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Encryption
{
    public class DateTimeEncryptionConverter : EncryptionConverter<DateTime>
    {
        public DateTimeEncryptionConverter(EncryptionSettings encryptionSettings) :
            base(
                d => d.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture),
                s => DateTime.Parse(s, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal),
                encryptionSettings
                )
        {
        }
    }
}
