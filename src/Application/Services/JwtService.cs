﻿using Application.Interfaces;
using Application.Settings;
using Application.Utilities;
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
    internal class JwtService : IJwtService
    {
        private readonly JwtSettings jwtSettings;

        private static Dictionary<string, JwtTokens> validTokens = new Dictionary<string, JwtTokens>();

        public JwtService(IConfiguration configuration)
        {
            jwtSettings = JwtSettings.Get(configuration);
        }

        public JwtTokens GenerateTokens(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var securityKey = new SymmetricSecurityKey(jwtSettings.KeyBytes);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var accessTokenClaims = new List<Claim>(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.AuthorizationDecision, user.IsEmailVerified ? "true" : "false")
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
                jwtSettings.Issuer,
                jwtSettings.Audience,
                accessTokenClaims,
                DateTime.Now,
                DateTime.Now.AddMinutes(jwtSettings.AccessTokenValidMinutes),
                credentials
                );

            var refreshTokenClaims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Username)
            };
            var refreshJwtToken = new JwtSecurityToken(
                jwtSettings.Issuer,
                jwtSettings.Audience,
                refreshTokenClaims,
                DateTime.Now,
                DateTime.Now.AddMinutes(jwtSettings.RefreshTokenValidMinutes),
                credentials
                );

            if (validTokens.ContainsKey(user.Username))
            {
                validTokens.Remove(user.Username);
            }

            var tokens = new JwtTokens(tokenHandler.WriteToken(accessJwtToken), tokenHandler.WriteToken(refreshJwtToken));
            validTokens.Add(user.Username, tokens);
            return tokens;
        }

        public string? ValidateAccessToken(string accessToken)
        {
            return ValidateAccessToken(accessToken, out _);
        }

        public string? ValidateAccessToken(string accessToken, out bool isEmailVerified)
        {
            var claims = ValidateToken(accessToken);
            if (claims != null)
            {
                var username = claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
                if (username != null && validTokens.ContainsKey(username))
                {
                    if (validTokens[username].AccessToken.Equals(accessToken))
                    {
                        isEmailVerified = claims.First(c => c.Type == ClaimTypes.AuthorizationDecision).Value.Equals("true");
                        return username;
                    }

                    validTokens.Remove(username);
                }
            }

            isEmailVerified = false;
            return null;
        }

        public string? ValidateRefreshToken(string refreshToken)
        {
            var claims = ValidateToken(refreshToken);
            if (claims != null)
            {
                var username = claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
                if (validTokens.ContainsKey(username))
                {
                    if (validTokens[username].RefreshToken.Equals(refreshToken))
                    {
                        return username;
                    }

                    validTokens.Remove(username);
                }
            }

            return null;
        }

        public void RevokeTokens(string username)
        {
            if (validTokens.ContainsKey(username))
            {
                validTokens.Remove(username);
            }
        }

        private IEnumerable<Claim>? ValidateToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(jwtSettings.KeyBytes),
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                return jwtToken.Claims;
            }
            catch
            {
                return null;
            }
        }
    }
}
