using Application.Exceptions;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private const string ALLOWED_CHARS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890-_";

        private IConfiguration configuration;
        private IUserRepository userRepository;
        private IMailService mailService;
        private IPasswordService passwordService;

        public UserService(IConfiguration configuration,
            IUserRepository userRepository,
            IMailService mailService,
            IPasswordService passwordService)
        {
            this.configuration = configuration;
            this.userRepository = userRepository;
            this.mailService = mailService;
            this.passwordService = passwordService;
        }

        public async Task ValidateUsernameAndEmailAsync(string username, string email)
        {
            if (username.Any(c => !ALLOWED_CHARS.Contains(c)))
            {
                throw new DataValidationException(
                    "Username may contain only letters, digits, minus (-) and underscore (_)"
                    // TODO: Add translation key
                    );
            }
            else if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase))
            {
                throw new DataValidationException(
                    "Invalid email address"
                    // TODO: Add translation key
                    );
            }
            else if (await userRepository.GetByUsernameAsync(username) != null)
            {
                throw new DataValidationException(
                    "Username is already taken",
                    "error.user.register.usernameTaken"
                    );
            }
            else if (await userRepository.GetByEmailAsync(email) != null)
            {
                throw new DataValidationException(
                    "Email is already taken",
                    "error.user.register.emailTaken"
                    );
            }
        }

        public async Task<User> RegisterAsync(User user)
        {
            var usedEmailVerificationCodes = userRepository.GetAll()
                .Where(user => user.EmailVerificationCode != null)
                .Select(user => user.EmailVerificationCode);
            int emailVerificationCodeLength = Convert.ToInt32(configuration["UserSettings:EmailVerificationCode:Length"]);
            string newEmailVerificationCode;
            do
            {
                newEmailVerificationCode = GenerateRandomString(emailVerificationCodeLength);
            }
            while (usedEmailVerificationCodes.Contains(newEmailVerificationCode));
            user.EmailVerificationCode = newEmailVerificationCode;
            user.EmailVerificationCodeCreatedAt = DateTime.UtcNow;
            var addedUser = await userRepository.AddAsync(user);

            _ = mailService.SendEmailVerificationEmailAsync(addedUser).ConfigureAwait(false);

            return addedUser;
        }

        public async Task<bool> VerifyEmailAsync(string username, string emailVerificationCode)
        {
            var user = await userRepository.GetByUsernameAsync(username);
            
            if (user == null || user.EmailVerificationCode == null || !emailVerificationCode.Equals(user.EmailVerificationCode))
            {
                return false;
            }

            int emailVerificationCodeValidMinutes = Convert.ToInt32(configuration["UserSettings:EmailVerificationCode:ValidMinutes"]);

            if (user.EmailVerificationCodeCreatedAt == null ||
                DateTime.UtcNow.Subtract((DateTime)user.EmailVerificationCodeCreatedAt).TotalMinutes > emailVerificationCodeValidMinutes)
            {
                return false;
            }

            user.EmailVerificationCode = null;
            user.EmailVerificationCodeCreatedAt = null;
            await userRepository.UpdateAsync(user);

            return true;
        }

        public async Task<User?> AuthenticateAsync(string usernameOrEmail, string password)
        {
            var user = await GetUserByUsernameOrEmailAsync(usernameOrEmail);

            if (user != null && passwordService.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            {
                return user;
            }
            else
            {
                return null;
            }
        }

        public Task<User?> GetUserByUsernameIgnoreCaseAsync(string username)
        {
            return userRepository.GetByUsernameAsync(username);
        }

        public async Task GenerateResetPasswordCodeAsync(string usernameOrEmail)
        {
            var user = await GetUserByUsernameOrEmailAsync(usernameOrEmail);

            if (user != null)
            {
                var usedResetPasswordCodes = userRepository.GetAll()
                    .Where(user => user.ResetPasswordCode != null)
                    .Select(user => user.ResetPasswordCode);
                int resetPasswordCodeLength = Convert.ToInt32(configuration["UserSettings:ResetPasswordCode:Length"]);
                string resetPasswordCode;
                do
                {
                    resetPasswordCode = GenerateRandomString(resetPasswordCodeLength);
                }
                while (usedResetPasswordCodes.Contains(resetPasswordCode));

                user.ResetPasswordCode = resetPasswordCode;
                user.ResetPasswordCodeCreatedAt = DateTime.UtcNow;
                var updatedUser = await userRepository.UpdateAsync(user);

                _ = mailService.SendResetPasswordEmailAsync(updatedUser).ConfigureAwait(false);
            }
        }

        public async Task<User?> ValidateResetPasswordCodeAsync(string resetPasswordCode)
        {
            var user = await userRepository.GetByResetPasswordCodeAsync(resetPasswordCode);
            if (user == null)
            {
                return null;
            }

            int resetPasswordCodeValidMinutes = Convert.ToInt32(configuration["UserSettings:ResetPasswordCode:ValidMinutes"]);
            if (user.ResetPasswordCodeCreatedAt == null ||
                DateTime.UtcNow.Subtract((DateTime)user.ResetPasswordCodeCreatedAt).TotalMinutes > resetPasswordCodeValidMinutes)
            {
                return null;
            }

            return user;
        }

        public async Task ChangePasswordAsync(User user, string newPassword)
        {
            byte[] passwordHash, passwordSalt;
            passwordService.CreatePasswordHash(newPassword, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.ResetPasswordCode = null;
            user.ResetPasswordCodeCreatedAt = null;

            await userRepository.UpdateAsync(user);
        }

        private string GenerateRandomString(int length)
        {
            var randomBytes = RandomNumberGenerator.GetBytes(length);
            var randomStringBuilder = new StringBuilder();
            randomBytes.ToList()
                .ForEach(randomByte => randomStringBuilder.Append(ALLOWED_CHARS[randomByte % ALLOWED_CHARS.Length]));

            return randomStringBuilder.ToString();
        }

        private Task<User?> GetUserByUsernameOrEmailAsync(string usernameOrEmail)
        {
            if (usernameOrEmail.Contains('@'))
            {
                return userRepository.GetByEmailAsync(usernameOrEmail);
            }
            else
            {
                return userRepository.GetByUsernameAsync(usernameOrEmail);
            }
        }
    }
}
