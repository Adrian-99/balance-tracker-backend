using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Exceptions
{
    public class DataValidationException : ResponseStatusException
    {
        public DataValidationException(string? message = null, string? errorTranslationKey = null) :
            base(StatusCodes.Status400BadRequest, message, errorTranslationKey)
        { }
    }
}
