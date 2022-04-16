using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserService
    {
        Task ValidateUsernameAndEmailAsync(string username, string email);
        Task<User> RegisterAsync(User user);
        Task<bool> VerifyEmailAsync(string username, string emailVerificationCode);
        Task<User?> AuthenticateAsync(string usernameOrEmail, string password);
        Task<User?> GetUserByUsernameIgnoreCaseAsync(string username);
        Task GenerateResetPasswordCodeAsync(string usernameOrEmail);
    }
}
