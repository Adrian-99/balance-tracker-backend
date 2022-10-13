using Application.Utilities;
using Newtonsoft.Json;
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

        [JsonConstructor]
        public TokensDto(string accessToken, string refreshToken)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }

        public TokensDto(JwtTokens jwtTokens)
        {
            AccessToken = jwtTokens.AccessToken;
            RefreshToken = jwtTokens.RefreshToken;
        }
    }
}
