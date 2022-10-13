using Application.Dtos.Ingoing;
using Application.Exceptions;
using Application.Interfaces;
using Application.Settings;
using Application.Utilities;
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
        private readonly IUserRepository userRepository;
        private readonly IMailService mailService;
        private readonly IPasswordService passwordService;
        private readonly IJwtService jwtService;

        public UserService(IConfiguration configuration,
            IUserRepository userRepository,
            IMailService mailService,
            IPasswordService passwordService,
            IJwtService jwtService)
        {
            userSettings = UserSettings.Get(configuration);
            this.userRepository = userRepository;
            this.mailService = mailService;
            this.passwordService = passwordService;
            this.jwtService = jwtService;
        }

        public Task<User> GetAuthorizedUserAsync(HttpContext httpContext)
        {
            return userRepository.GetByUsernameIgnoreCaseAsync(httpContext.Items[Constants.AUTHORIZED_USERNAME].ToString());
        }

        public async Task<User> RegisterAsync(User user)
        {
            await ValidateAsync(user.Username, user.Email, user.FirstName, user.LastName);

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

        public async Task<JwtTokens> VerifyEmailAsync(User user, string emailVerificationCode)
        {
            if (user.IsEmailVerified)
            {
                throw new ResponseStatusException(StatusCodes.Status409Conflict, "Email already verified");
            }
            else if (user.EmailVerificationCode == null ||
                !emailVerificationCode.Equals(user.EmailVerificationCode) ||
                user.EmailVerificationCodeCreatedAt == null ||
                !Utils.IsWithinTimeframe(
                    (DateTime)user.EmailVerificationCodeCreatedAt,
                    userSettings.EmailVerificationCode.ValidMinutes,
                    Utils.DateTimeUnit.MINTUES
                    ))
            {
                throw new ResponseStatusException(
                    StatusCodes.Status400BadRequest,
                    "Invalid email verification code",
                    "error.user.verifyEmail.invalidCode"
                    );
            }

            user.IsEmailVerified = true;
            user.EmailVerificationCode = null;
            user.EmailVerificationCodeCreatedAt = null;
            var updatedUser = await userRepository.UpdateAsync(user);

            return jwtService.GenerateTokens(updatedUser);
        }

        public async Task<User> ResetEmailVerificationCodeAsync(User user)
        {
            if (user.IsEmailVerified)
            {
                throw new ResponseStatusException(StatusCodes.Status409Conflict, "Email already verified");
            }

            user.EmailVerificationCode = await GenerateEmailVerificationCodeAsync();
            user.EmailVerificationCodeCreatedAt = DateTime.UtcNow;
            var updatedUser = await userRepository.UpdateAsync(user);

            _ = mailService.SendEmailVerificationEmailAsync(updatedUser).ConfigureAwait(false);

            return updatedUser;
        }

        public async Task<JwtTokens> AuthenticateAsync(string usernameOrEmail, string password)
        {
            var user = await GetUserByUsernameOrEmailAsync(usernameOrEmail);

            if (user != null && passwordService.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            {
                return jwtService.GenerateTokens(user);
            }
            else
            {
                throw new ResponseStatusException(
                    StatusCodes.Status401Unauthorized,
                    "Wrong username or password",
                    "error.user.authenticate.wrongCredentials"
                    );
            }
        }

        public async Task<JwtTokens> RefreshTokenAsync(string refreshToken)
        {
            var username = jwtService.ValidateRefreshToken(refreshToken);
            if (username == null)
            {
                throw new ResponseStatusException(StatusCodes.Status400BadRequest, "Invalid refresh token");
            }
            var user = await GetUserByUsernameIgnoreCaseAsync(username);
            return jwtService.GenerateTokens(user);
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

        public async Task ResetPasswordAsync(string resetPasswordCode, string newPassword)
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
                throw new ResponseStatusException(
                    StatusCodes.Status400BadRequest,
                    "Invalid reset password code",
                    "error.user.resetPassword.invalidCode"
                    );
            }

            await SaveChangedPasswordAsync(user, newPassword);
        }
        public Task<User> ChangePasswordAsync(User user, string currentPassword, string newPassword)
        {
            if (!passwordService.VerifyPasswordHash(currentPassword, user.PasswordHash, user.PasswordSalt))
            {
                throw new ResponseStatusException(
                    StatusCodes.Status400BadRequest,
                    "Wrong current password",
                    "error.user.changePassword.wrongCurrentPassword"
                    );
            }

            return SaveChangedPasswordAsync(user, newPassword);
        }

        public async Task<JwtTokens> ChangeUserDataAsync(User user,
                                                         string newUsername,
                                                         string newEmail,
                                                         string? newFirstName,
                                                         string? newLastName)
        {
            var isUsernameChanged = !user.Username.Equals(newUsername);
            var isEmailChanged = !user.Email.ToLower().Equals(newEmail.ToLower());

            if (isUsernameChanged)
            {
                if (Utils.IsWithinTimeframe(user.LastUsernameChangeAt, userSettings.Username.AllowedChangeFrequencyDays, Utils.DateTimeUnit.DAYS))
                {
                    throw new ResponseStatusException(
                        StatusCodes.Status400BadRequest,
                        $"Next allowed username change at {user.LastUsernameChangeAt.AddDays(userSettings.Username.AllowedChangeFrequencyDays)}"
                        );
                }
                else
                {
                    user.LastUsernameChangeAt = DateTime.UtcNow;
                }
            }

            if (isUsernameChanged ||
                !user.Email.Equals(newEmail) ||
                !user.FirstName.Equals(newFirstName) ||
                !user.LastName.Equals(newLastName))
            {
                var previousUsername = user.Username;

                await ValidateAsync(
                    newUsername,
                    newEmail,
                    newFirstName,
                    newLastName,
                    !user.Username.ToLower().Equals(newUsername.ToLower()),
                    isEmailChanged);

                user.Username = newUsername;
                user.Email = newEmail;
                user.FirstName = newFirstName;
                user.LastName = newLastName;

                if (isEmailChanged)
                {
                    user.IsEmailVerified = false;
                    user.EmailVerificationCode = await GenerateEmailVerificationCodeAsync();
                    user.EmailVerificationCodeCreatedAt = DateTime.UtcNow;
                }

                var updatedUser = await userRepository.UpdateAsync(user);

                if (isEmailChanged)
                {
                    _ = mailService.SendEmailVerificationEmailAsync(updatedUser).ConfigureAwait(false);
                }

                jwtService.RevokeTokens(previousUsername);
                return jwtService.GenerateTokens(updatedUser);
            }
            else
            {
                return jwtService.GenerateTokens(user);
            }
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

        private async Task ValidateAsync(string username,
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

        private Task<User?> GetUserByUsernameIgnoreCaseAsync(string username)
        {
            return userRepository.GetByUsernameIgnoreCaseAsync(username);
        }

        private Task<User> SaveChangedPasswordAsync(User user, string newPassword)
        {
            passwordService.CheckPasswordComplexity(newPassword, user);

            byte[] passwordHash, passwordSalt;
            passwordService.CreatePasswordHash(newPassword, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.ResetPasswordCode = null;
            user.ResetPasswordCodeCreatedAt = null;

            return userRepository.UpdateAsync(user);
        }
    }
}
