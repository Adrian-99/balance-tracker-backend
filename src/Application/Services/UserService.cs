using Application.Exceptions;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private const string ALLOWED_CHARS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890-_";

        private IUserRepository userRepository;

        public UserService(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        public async Task ValidateUsernameAndEmail(string username, string email)
        {
            if (await userRepository.GetByUsernameIgnoreCase(username) != null)
            {
                throw new DataValidationException(
                    "Username is already taken"
                    // TODO: Add translation key
                    );
            }
            else if (username.Any(c => !ALLOWED_CHARS.Contains(c)))
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
        }

        public async Task<User> Register(User user)
        {
            string emailVerificationCode;
            do
            {
                emailVerificationCode = GenerateRandomString(30);
            }
            while (await userRepository.GetByEmailVerificationCode(emailVerificationCode) != null);
            user.EmailVerificationCode = emailVerificationCode;
            var addedUser = await userRepository.Add(user);

            // TODO: Send activation email

            return addedUser;
        }

        public async Task<bool> VerifyEmail(string emailVerificationCode)
        {
            var user = await userRepository.GetByEmailVerificationCode(emailVerificationCode);
            
            if (user == null)
            {
                return false;
            }

            user.EmailVerificationCode = null;
            await userRepository.Update(user);
            return true;
        }

        private string GenerateRandomString(int length)
        {
            var randomBytes = RandomNumberGenerator.GetBytes(length);
            var randomStringBuilder = new StringBuilder();
            randomBytes.ToList()
                .ForEach(randomByte => randomStringBuilder.Append(ALLOWED_CHARS[randomByte % ALLOWED_CHARS.Length]));

            return randomStringBuilder.ToString();
        }
    }
}
