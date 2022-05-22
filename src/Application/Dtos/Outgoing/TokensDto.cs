using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Outgoing
{
    public class TokensDto
    {
        [Required]
        public string AccessToken { get; }

        [Required]
        public string RefreshToken { get; }

        public string? TranslationKey { get; }

        public TokensDto(string accessToken, string refreshToken, string? translationKey = null)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            TranslationKey = translationKey;
        }
    }
}
