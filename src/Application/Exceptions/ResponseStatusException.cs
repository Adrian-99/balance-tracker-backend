using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Exceptions
{
    public class ResponseStatusException : Exception
    {
        public int StatusCode { get; set; }
        public string? ErrorTranslationKey { get; set; }

        public ResponseStatusException(int statusCode, string? message = null, string? errorTranslationKey = null)
            : base(message)
        {
            StatusCode = statusCode;
            ErrorTranslationKey = errorTranslationKey;
        }
    }
}
