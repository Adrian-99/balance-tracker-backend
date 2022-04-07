using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class JwtService : IJwtService
    {
        private IConfiguration configuration;

        private static Dictionary<string, Tuple<string, string>> validTokens = new Dictionary<string, Tuple<string, string>>();

        public JwtService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void GenerateTokens(User user, out string accessToken, out string refreshToken)
        {
            var issuer = configuration["JwtSettings:Issuer"];
            var audience = configuration["JwtSettings:Audience"];
            var accessTokenValidMinutes = Convert.ToInt32(configuration["JwtSettings:AccessTokenValidMinutes"]);
            var refreshTokenValidMinutes = Convert.ToInt32(configuration["JwtSettings:RefreshTokenValidMinutes"]);

            var tokenHandler = new JwtSecurityTokenHandler();

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var accessTokenClaims = new List<Claim>(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            });
            if (user.FirstName != null)
            {
                accessTokenClaims.Add(new Claim(ClaimTypes.GivenName, user.FirstName));
            }
            if (user.LastName != null)
            {
                accessTokenClaims.Add(new Claim(ClaimTypes.Surname, user.LastName));
            }

            var accessJwtToken = new JwtSecurityToken(
                issuer,
                audience,
                accessTokenClaims,
                DateTime.Now,
                DateTime.Now.AddMinutes(accessTokenValidMinutes),
                credentials
                );
            accessToken = tokenHandler.WriteToken(accessJwtToken);

            var refreshTokenClaims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Username)
            };
            var refreshJwtToken = new JwtSecurityToken(
                issuer,
                audience,
                refreshTokenClaims,
                DateTime.Now,
                DateTime.Now.AddMinutes(refreshTokenValidMinutes),
                credentials
                );

            refreshToken = tokenHandler.WriteToken(refreshJwtToken);

            if (!validTokens.ContainsKey(user.Username))
            {
                validTokens.Add(user.Username, new Tuple<string, string>(accessToken, refreshToken));
            }
            else
            {
                validTokens[user.Username] = new Tuple<string, string>(accessToken, refreshToken);
            }
        }

        public async Task<string?> ValidateAccessToken(string accessToken)
        {
            var authorizedUsername = await ValidateToken(accessToken);
            if (authorizedUsername != null && validTokens.ContainsKey(authorizedUsername))
            {
                if (validTokens[authorizedUsername].Item1.Equals(accessToken))
                {
                    return authorizedUsername;
                }

                validTokens.Remove(authorizedUsername);
                return null;
            }

            return null;
        }

        public async Task<string?> ValidateRefreshToken(string refreshToken)
        {
            var authorizedUsername = await ValidateToken(refreshToken);
            if (authorizedUsername != null && validTokens.ContainsKey(authorizedUsername))
            {
                if (validTokens[authorizedUsername].Item2.Equals(refreshToken))
                {
                    return authorizedUsername;
                }

                validTokens.Remove(authorizedUsername);
                return null;
            }

            return null;
        }

        private async Task <string?> ValidateToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"]);
            var issuer = configuration["JwtSettings:Issuer"];
            var audience = configuration["JwtSettings:Audience"];
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                return jwtToken.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
            }
            catch
            {
                return null;
            }
        }
    }
}
