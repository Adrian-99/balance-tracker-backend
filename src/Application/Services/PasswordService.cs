using Application.Exceptions;
using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class PasswordService : IPasswordService
    {
        private const int PASSWORD_REQUIRED_LENGTH = 8;
        private const bool PASSWORD_REQUIRE_LOWER_CASE = true;
        private const bool PASSWORD_REQUIRE_UPPER_CASE = true;
        private const bool PASSWORD_REQUIRE_DIGIT = true;
        private const bool PASSWORD_REQUIRE_SPECIAL_CHAR = true;
        private const bool PASSWORD_FORBID_USERNAME_VALUE = true;

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
            if (PASSWORD_FORBID_USERNAME_VALUE && username != null && username.ToLower().Equals(password.ToLower()))
            {
                throw new DataValidationException(
                    "Password cannot be the same as username"
                    // TODO: Add translation key
                    );
            }
            else if (password.Length < PASSWORD_REQUIRED_LENGTH)
            {
                throw new DataValidationException(
                    $"Password must be at least {PASSWORD_REQUIRED_LENGTH} characters long"
                    // TODO: Add translation key
                    );
            }
            else if (PASSWORD_REQUIRE_LOWER_CASE && !password.Any(c => char.IsLower(c)))
            {
                throw new DataValidationException(
                    "Password must contain at least one small letter"
                    // TODO: Add translation key
                    );
            }
            else if (PASSWORD_REQUIRE_UPPER_CASE && !password.Any(c => char.IsUpper(c)))
            {
                throw new DataValidationException(
                    "Password must contain at least one big letter"
                    // TODO: Add translation key
                    );
            }
            else if (PASSWORD_REQUIRE_DIGIT && !password.Any(c => char.IsDigit(c)))
            {
                throw new DataValidationException(
                    "Password must contain at least one digit"
                    // TODO: Add translation key
                    );
            }
            else if (PASSWORD_REQUIRE_SPECIAL_CHAR && !password.Any(c => !char.IsLetterOrDigit(c)))
            {
                throw new DataValidationException(
                    "Password must contain at least one special character"
                    // TODO: Add translation key
                    );
            }
        }
    }
}
