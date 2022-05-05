using Application.Dtos.Ingoing;
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
        Task ValidateUserDetailsAsync(string username,
                                      string email,
                                      string? firstName,
                                      string? lastName,
                                      bool checkIfUsernameTaken = true,
                                      bool checkIfEmailTaken = true);
        Task<User> RegisterAsync(User user);
        Task<User?> VerifyEmailAsync(User user, string emailVerificationCode);
        Task<User> ResetEmailVerificationCodeAsync(User user);
        Task<User?> AuthenticateAsync(string usernameOrEmail, string password);
        Task<User?> GetUserByUsernameIgnoreCaseAsync(string username);
        Task GenerateResetPasswordCodeAsync(string usernameOrEmail);
        Task<User?> ValidateResetPasswordCodeAsync(string resetPasswordCode);
        Task<User> ChangePasswordAsync(User user, string newPassword);
        Task<User> ChangeUserDataAsync(User user, ChangeUserDataDto newData);
    }
}
