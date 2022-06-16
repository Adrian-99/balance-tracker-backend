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
    internal class UserService : IUserService
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
            return userRepository.GetByUsernameIgnoreCaseAsync(httpContext.Items[Constants.AUTHORIZED_USERNAME].ToString());
        }

        public async Task ValidateUserDetailsAsync(string username,
                                      string email,
                                      string? firstName,
                                      string? lastName,
                                      bool checkIfUsernameTaken = true,
                                      bool checkIfEmailTaken = true)
        {
            if (username.Any(c => !ALLOWED_CHARS.Contains(c)))
            {
                throw new DataValidationException(
                    "Username may contain only letters, digits, minus (-) and underscore (_)"
                    // TODO: Add translation key
                    );
            }
            else if (username.Length > userSettings.Username.MaxLength)
            {
                throw new DataValidationException(
                    $"Username must not be longer than {userSettings.Username.MaxLength} characters"
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
            else if (checkIfUsernameTaken && await userRepository.GetByUsernameIgnoreCaseAsync(username) != null)
            {
                throw new DataValidationException(
                    "Username is already taken",
                    "error.user.register.usernameTaken"
                    );
            }
            else if (checkIfEmailTaken && await userRepository.GetByEmailIgnoreCaseAsync(email) != null)
            {
                throw new DataValidationException(
                    "Email is already taken",
                    "error.user.register.emailTaken"
                    );
            }
            else if (firstName?.Length > userSettings.FirstName.MaxLength)
            {
                throw new DataValidationException(
                    $"First name must not be longer than {userSettings.FirstName.MaxLength} characters"
                    // TODO: Add translation key
                    );
            }
            else if (lastName?.Length > userSettings.LastName.MaxLength)
            {
                throw new DataValidationException(
                    $"Last name must not be longer than {userSettings.LastName.MaxLength} characters"
                    // TODO: Add translation key
                    );
            }
        }

        public async Task<User> RegisterAsync(User user)
        {
            user.LastUsernameChangeAt = DateTime.UtcNow;
            user.IsEmailVerified = false;
            user.EmailVerificationCode = await GenerateEmailVerificationCodeAsync();
            user.EmailVerificationCodeCreatedAt = DateTime.UtcNow;
            user.ResetPasswordCode = null;
            user.ResetPasswordCodeCreatedAt = null;
            var addedUser = await userRepository.AddAsync(user);

            _ = mailService.SendEmailVerificationEmailAsync(addedUser).ConfigureAwait(false);

            return addedUser;
        }

        public async Task<User?> VerifyEmailAsync(User user, string emailVerificationCode)
        {            
            if (user.IsEmailVerified ||
                user.EmailVerificationCode == null ||
                !emailVerificationCode.Equals(user.EmailVerificationCode) ||
                user.EmailVerificationCodeCreatedAt == null ||
                !Utils.IsWithinTimeframe(
                    (DateTime)user.EmailVerificationCodeCreatedAt,
                    userSettings.EmailVerificationCode.ValidMinutes,
                    Utils.DateTimeUnit.MINTUES
                    ))
            {
                return null;
            }

            user.IsEmailVerified = true;
            user.EmailVerificationCode = null;
            user.EmailVerificationCodeCreatedAt = null;
            var updatedUser = await userRepository.UpdateAsync(user);

            return updatedUser;
        }

        public async Task<User> ResetEmailVerificationCodeAsync(User user)
        {
            user.EmailVerificationCode = await GenerateEmailVerificationCodeAsync();
            user.EmailVerificationCodeCreatedAt = DateTime.UtcNow;
            var updatedUser = await userRepository.UpdateAsync(user);

            _ = mailService.SendEmailVerificationEmailAsync(updatedUser).ConfigureAwait(false);

            return updatedUser;
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
            return userRepository.GetByUsernameIgnoreCaseAsync(username);
        }

        public async Task GenerateResetPasswordCodeAsync(string usernameOrEmail)
        {
            var user = await GetUserByUsernameOrEmailAsync(usernameOrEmail);

            if (user != null)
            {
                var users = await userRepository.GetAllAsync();
                var usedResetPasswordCodes = users.Where(user => user.ResetPasswordCode != null)
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
            if (user == null ||
                user.ResetPasswordCodeCreatedAt == null ||
                !Utils.IsWithinTimeframe(
                    (DateTime)user.ResetPasswordCodeCreatedAt,
                    userSettings.ResetPasswordCode.ValidMinutes,
                    Utils.DateTimeUnit.MINTUES
                    ))
            {
                return null;
            }

            return user;
        }

        public Task<User> ChangePasswordAsync(User user, string newPassword)
        {
            byte[] passwordHash, passwordSalt;
            passwordService.CreatePasswordHash(newPassword, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.ResetPasswordCode = null;
            user.ResetPasswordCodeCreatedAt = null;

            return userRepository.UpdateAsync(user);
        }

        public async Task<User> ChangeUserDataAsync(User user, ChangeUserDataDto newData)
        {
            var wasEmailChanged = !user.Email.ToLower().Equals(newData.Email.ToLower());

            if (!user.Username.Equals(newData.Username))
            {
                user.LastUsernameChangeAt = DateTime.UtcNow;
            }

            user.Username = newData.Username;
            user.Email = newData.Email;
            user.FirstName = newData.FirstName;
            user.LastName = newData.LastName;

            if (wasEmailChanged)
            {
                user.IsEmailVerified = false;
                user.EmailVerificationCode = await GenerateEmailVerificationCodeAsync();
                user.EmailVerificationCodeCreatedAt = DateTime.UtcNow;
            }

            var updatedUser = await userRepository.UpdateAsync(user);

            if (wasEmailChanged)
            {
                _ = mailService.SendEmailVerificationEmailAsync(updatedUser).ConfigureAwait(false);
            }

            return updatedUser;
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
                return userRepository.GetByEmailIgnoreCaseAsync(usernameOrEmail);
            }
            else
            {
                return userRepository.GetByUsernameIgnoreCaseAsync(usernameOrEmail);
            }
        }

        private async Task<string> GenerateEmailVerificationCodeAsync()
        {
            var users = await userRepository.GetAllAsync();
            var usedEmailVerificationCodes = users.Where(user => user.EmailVerificationCode != null)
                .Select(user => user.EmailVerificationCode);
            string newEmailVerificationCode;
            do
            {
                newEmailVerificationCode = GenerateRandomString(userSettings.EmailVerificationCode.Length);
            }
            while (usedEmailVerificationCodes.Contains(newEmailVerificationCode));
            return newEmailVerificationCode;
        }
    }
}
