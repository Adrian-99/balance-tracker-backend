using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class ActionInfoDto
    {
        public bool Success { get; protected set; }
        public string? Message { get; protected set; }
        public string? TranslationKey { get; protected set; }

        public ActionInfoDto(bool success, string? message = null, string? translationKey = null)
        {
            Success = success;
            Message = message;
            TranslationKey = translationKey;
        }
    }
}
