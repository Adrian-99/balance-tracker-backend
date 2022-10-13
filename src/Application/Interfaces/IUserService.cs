using Application.Dtos.Ingoing;
using Application.Utilities;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserService
    {
        Task<User> GetAuthorizedUserAsync(HttpContext httpContext);
        Task<User> RegisterAsync(User user);
        Task<JwtTokens> VerifyEmailAsync(User user, string emailVerificationCode);
        Task<User> ResetEmailVerificationCodeAsync(User user);
        Task<JwtTokens> AuthenticateAsync(string usernameOrEmail, string password);
        Task<JwtTokens> RefreshTokenAsync(string refreshToken);
        Task GenerateResetPasswordCodeAsync(string usernameOrEmail);
        Task ResetPasswordAsync(string resetPasswordCode, string newPassword);
        Task<User> ChangePasswordAsync(User user, string currentPassword, string newPassword);
        Task<JwtTokens> ChangeUserDataAsync(User user,
                                            string newUsername,
                                            string newEmail,
                                            string? newFirstName,
                                            string? newLastName);
    }
}
