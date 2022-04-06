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
        void GenerateTokens(User user, out string accessToken, out string refreshToken);
        Task<string?> ValidateAccessToken(string accessToken);
        Task<string?> ValidateRefreshToken(string refreshToken);
    }
}
