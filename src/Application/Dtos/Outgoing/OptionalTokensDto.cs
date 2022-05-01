using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Outgoing
{
    public class OptionalTokensDto
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? TranslationKey { get; set; }

        public OptionalTokensDto(string? accessToken, string? refreshToken, string? translationKey)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            TranslationKey = translationKey;
        }
    }
}
