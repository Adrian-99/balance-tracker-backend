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
        private IMailService mailService;
        private IPasswordService passwordService;

        public UserService(IUserRepository userRepository, IMailService mailService, IPasswordService passwordService)
        {
            this.userRepository = userRepository;
            this.mailService = mailService;
            this.passwordService = passwordService;
        }

        public async Task ValidateUsernameAndEmail(string username, string email)
        {
            if (await userRepository.GetByUsernameIgnoreCase(username) != null)
            {
                throw new DataValidationException(
                    "Username is already taken",
                    "error.user.register.usernameTaken"
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

            _ = mailService.SendEmailVerificationEmail(addedUser).ConfigureAwait(false);

            return addedUser;
        }

        public async Task<bool> VerifyEmail(string username, string emailVerificationCode)
        {
            var user = await userRepository.GetByUsernameIgnoreCase(username);
            
            if (user == null || !emailVerificationCode.Equals(user.EmailVerificationCode))
            {
                return false;
            }

            user.EmailVerificationCode = null;
            await userRepository.Update(user);
            return true;
        }

        public async Task<User> Authenticate(string username, string password)
        {
            var user = await userRepository.GetByUsernameIgnoreCase(username);
            if (user != null && passwordService.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            {
                return user;
            }
            else
            {
                throw new UnauthorizedAccessException();
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
    }
}
