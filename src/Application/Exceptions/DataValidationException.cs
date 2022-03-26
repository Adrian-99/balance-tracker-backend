using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Exceptions
{
    public class DataValidationException : Exception
    {
        public string? ErrorTranslationKey { get; set; }

        public DataValidationException(string? message = null, string? errorTranslationKey = null) :
            base(message)
        {
            ErrorTranslationKey = errorTranslationKey;
        }
    }
}
