using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class ActionResultDto
    {
        public bool Success { get => StatusCode >= 200 && StatusCode <= 299; }
        public int StatusCode { get; protected set; }
        public string? Message { get; protected set; }
        public string? TranslationKey { get; protected set; }

        public ActionResultDto(int statusCode, string? message = null, string? translationKey = null)
        {
            StatusCode = statusCode;
            Message = message;
            TranslationKey = translationKey;
        }
    }
}
