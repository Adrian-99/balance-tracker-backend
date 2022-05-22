using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Outgoing
{
    public class ActionResultDto
    {
        [Required]
        public bool Success { get => StatusCode >= 200 && StatusCode <= 299; }

        [Required]
        public int StatusCode { get; }

        public string? Message { get; }

        public string? TranslationKey { get; }

        public ActionResultDto(int statusCode, string? message = null, string? translationKey = null)
        {
            StatusCode = statusCode;
            Message = message;
            TranslationKey = translationKey;
        }
    }
}
