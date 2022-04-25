using Application.Exceptions;
using Application.Interfaces;
using Application.Settings;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services
{
    public class PasswordService : IPasswordService
    {
        private readonly PasswordSettings passwordSettings;

        public PasswordService(IConfiguration configuration)
        {
            passwordSettings = PasswordSettings.Get(configuration);
        }

        public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        public void CheckPasswordComplexity(string password, string? username = null)
        {
            if (passwordSettings.ForbidSameAsUsername && username != null && username.ToLower().Equals(password.ToLower()))
            {
                throw new DataValidationException(
                    "Password cannot be the same as username",
                    "error.validation.passwordSameAsUsername"
                    );
            }
            else if (password.Length < passwordSettings.MinLength)
            {
                throw new DataValidationException(
                    $"Password must be at least {passwordSettings.MinLength} characters long"
                    // TODO: Add translation key
                    );
            }
            else if (password.Length > passwordSettings.MaxLength)
            {
                throw new DataValidationException(
                    $"Password must not be longer than {passwordSettings.MinLength} characters"
                    // TODO: Add translation key
                    );
            }
            else if (passwordSettings.SmallLetterRequired && !password.Any(c => char.IsLower(c)))
            {
                throw new DataValidationException(
                    "Password must contain at least one small letter"
                    // TODO: Add translation key
                    );
            }
            else if (passwordSettings.BigLetterRequired && !password.Any(c => char.IsUpper(c)))
            {
                throw new DataValidationException(
                    "Password must contain at least one big letter"
                    // TODO: Add translation key
                    );
            }
            else if (passwordSettings.DigitRequired && !password.Any(c => char.IsDigit(c)))
            {
                throw new DataValidationException(
                    "Password must contain at least one digit"
                    // TODO: Add translation key
                    );
            }
            else if (passwordSettings.SpecialCharacterRequired && !password.Any(c => !char.IsLetterOrDigit(c)))
            {
                throw new DataValidationException(
                    "Password must contain at least one special character"
                    // TODO: Add translation key
                    );
            }
        }

        public void CheckPasswordComplexity(string password, User user)
        {
            if (passwordSettings.ForbidSameAsCurrent && VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            {
                throw new DataValidationException(
                    "New password cannot be the same as current one",
                    "error.validation.passwordSameAsCurrentOne"
                    );
            }
            CheckPasswordComplexity(password, user.Username);
        }
    }
}
