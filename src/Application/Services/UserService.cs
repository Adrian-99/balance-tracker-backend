using Application.Dtos.Ingoing;
using Application.Exceptions;
using Application.Interfaces;
using Application.Settings;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private const string ALLOWED_CHARS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890-_";

        private readonly UserSettings userSettings;
        private IUserRepository userRepository;
        private IMailService mailService;
        private IPasswordService passwordService;

        public UserService(IConfiguration configuration,
            IUserRepository userRepository,
            IMailService mailService,
            IPasswordService passwordService)
        {
            userSettings = UserSettings.Get(configuration);
            this.userRepository = userRepository;
            this.mailService = mailService;
            this.passwordService = passwordService;
        }

        public Task<User> GetAuthorizedUserAsync(HttpContext httpContext)
        {
            return userRepository.GetByUsernameAsync(httpContext.Items[Constants.AUTHORIZED_USERNAME].ToString());
        }

        public async Task ValidateUserDetailsAsync(UserRegisterDto userDetails)
        {
            if (userDetails.Username.Any(c => !ALLOWED_CHARS.Contains(c)))
            {
                throw new DataValidationException(
                    "Username may contain only letters, digits, minus (-) and underscore (_)"
                    // TODO: Add translation key
                    );
            }
            else if (userDetails.Username.Length > userSettings.Username.MaxLength)
            {
                throw new DataValidationException(
                    $"Username must not be longer than {userSettings.Username.MaxLength} characters"
                    // TODO: Add translation key
                    );
            }
            else if (!Regex.IsMatch(userDetails.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase))
            {
                throw new DataValidationException(
                    "Invalid email address"
                    // TODO: Add translation key
                    );
            }
            else if (await userRepository.GetByUsernameAsync(userDetails.Username) != null)
            {
                throw new DataValidationException(
                    "Username is already taken",
                    "error.user.register.usernameTaken"
                    );
            }
            else if (await userRepository.GetByEmailAsync(userDetails.Email) != null)
            {
                throw new DataValidationException(
                    "Email is already taken",
                    "error.user.register.emailTaken"
                    );
            }
            else if (userDetails.FirstName?.Length > userSettings.FirstName.MaxLength)
            {
                throw new DataValidationException(
                    $"First name must not be longer than {userSettings.FirstName.MaxLength} characters"
                    // TODO: Add translation key
                    );
            }
            else if (userDetails.LastName?.Length > userSettings.LastName.MaxLength)
            {
                throw new DataValidationException(
                    $"Last name must not be longer than {userSettings.LastName.MaxLength} characters"
                    // TODO: Add translation key
                    );
            }
        }

        public async Task<User> RegisterAsync(User user)
        {
            var usedEmailVerificationCodes = userRepository.GetAll()
                .Where(user => user.EmailVerificationCode != null)
                .Select(user => user.EmailVerificationCode);
            string newEmailVerificationCode;
            do
            {
                newEmailVerificationCode = GenerateRandomString(userSettings.EmailVerificationCode.Length);
            }
            while (usedEmailVerificationCodes.Contains(newEmailVerificationCode));
            user.EmailVerificationCode = newEmailVerificationCode;
            user.EmailVerificationCodeCreatedAt = DateTime.UtcNow;
            var addedUser = await userRepository.AddAsync(user);

            _ = mailService.SendEmailVerificationEmailAsync(addedUser).ConfigureAwait(false);

            return addedUser;
        }

        public async Task<bool> VerifyEmailAsync(User user, string emailVerificationCode)
        {            
            if (user.EmailVerificationCode == null || !emailVerificationCode.Equals(user.EmailVerificationCode))
            {
                return false;
            }

            if (user.EmailVerificationCodeCreatedAt == null ||
                DateTime.UtcNow.Subtract((DateTime)user.EmailVerificationCodeCreatedAt).TotalMinutes > userSettings.EmailVerificationCode.ValidMinutes)
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

                string resetPasswordCode;
                do
                {
                    resetPasswordCode = GenerateRandomString(userSettings.ResetPasswordCode.Length);
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

            if (user.ResetPasswordCodeCreatedAt == null ||
                DateTime.UtcNow.Subtract((DateTime)user.ResetPasswordCodeCreatedAt).TotalMinutes > userSettings.ResetPasswordCode.ValidMinutes)
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
