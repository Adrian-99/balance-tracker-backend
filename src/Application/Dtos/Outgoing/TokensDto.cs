using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Outgoing
{
    public class TokensDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public string? TranslationKey { get; set; }

        public TokensDto(string accessToken, string refreshToken, string? translationKey = null)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            TranslationKey = translationKey;
        }
    }
}
