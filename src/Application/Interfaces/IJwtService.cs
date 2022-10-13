using Application.Utilities;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IJwtService
    {
        JwtTokens GenerateTokens(User user);
        string? ValidateAccessToken(string accessToken);
        string? ValidateAccessToken(string accessToken, out bool isEmailVerified);
        string? ValidateRefreshToken(string refreshToken);
        void RevokeTokens(string username);
    }
}
