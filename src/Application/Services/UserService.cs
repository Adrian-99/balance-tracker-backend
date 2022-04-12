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

        public async Task<User> Register(User user)
        {
            var usedEmailVerificationCodes = userRepository.GetAll()
                .Where(user => user.EmailVerificationCode != null)
                .Select(user => user.EmailVerificationCode);
            string newEmailVerificationCode;
            do
            {
                newEmailVerificationCode = GenerateRandomString(30);
            }
            while (usedEmailVerificationCodes.Contains(newEmailVerificationCode));
            user.EmailVerificationCode = newEmailVerificationCode;
            var addedUser = await userRepository.AddAsync(user);

            _ = mailService.SendEmailVerificationEmailAsync(addedUser).ConfigureAwait(false);

            return addedUser;
        }

        public async Task<bool> VerifyEmail(string username, string emailVerificationCode)
        {
            var user = await userRepository.GetByUsernameAsync(username);
            
            if (user == null || !emailVerificationCode.Equals(user.EmailVerificationCode))
            {
                return false;
            }

            user.EmailVerificationCode = null;
            await userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<User?> Authenticate(string usernameOrEmail, string password)
        {
            User? user;
            if (usernameOrEmail.Contains('@'))
            {
                user = await userRepository.GetByEmailAsync(usernameOrEmail);
            }
            else
            {
                user = await userRepository.GetByUsernameAsync(usernameOrEmail);
            }

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
