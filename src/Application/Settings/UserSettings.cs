using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Settings
{
    public class UserSettings
    {
        public UsernameSettings Username { get; set; }
        public FirstNameSettings FirstName { get; set; }
        public LastNameSettings LastName { get; set; }
        public PasswordSettings Password { get; set; }
        public EmailVerificationCodeSettings EmailVerificationCode { get; set; }
        public ResetPasswordCodeSettings ResetPasswordCode { get; set; }

        public static UserSettings Get(IConfiguration configuration)
        {
            var userSettings = new UserSettings();

            userSettings.Username = UsernameSettings.Get(configuration);
            userSettings.FirstName = FirstNameSettings.Get(configuration);
            userSettings.LastName = LastNameSettings.Get(configuration);
            userSettings.Password = PasswordSettings.Get(configuration);
            userSettings.EmailVerificationCode = EmailVerificationCodeSettings.Get(configuration);
            userSettings.ResetPasswordCode = ResetPasswordCodeSettings.Get(configuration);

            return userSettings;
        }
    }

    public class UsernameSettings
    {
        public int MaxLength { get; set; }
        public int AllowedChangeFrequencyDays { get; set; }

        public static UsernameSettings Get(IConfiguration configuration)
        {
            return configuration.GetSection("User")
                .GetSection("Username")
                .Get<UsernameSettings>();
        }
    }

    public class FirstNameSettings
    {
        public int MaxLength { get; set; }

        public static FirstNameSettings Get(IConfiguration configuration)
        {
            return configuration.GetSection("User")
                .GetSection("FirstName")
                .Get<FirstNameSettings>();
        }
    }

    public class LastNameSettings
    {
        public int MaxLength { get; set; }

        public static LastNameSettings Get(IConfiguration configuration)
        {
            return configuration.GetSection("User")
                .GetSection("LastName")
                .Get<LastNameSettings>();
        }
    }

    public class PasswordSettings
    {
        public int MinLength { get; set; }
        public int MaxLength { get; set; }
        public bool SmallLetterRequired { get; set; }
        public bool BigLetterRequired { get; set; }
        public bool DigitRequired { get; set; }
        public bool SpecialCharacterRequired { get; set; }
        public bool ForbidSameAsUsername { get; set; }
        public bool ForbidSameAsCurrent { get; set; }

        public static PasswordSettings Get(IConfiguration configuration)
        {
            return configuration.GetSection("User")
                .GetSection("Password")
                .Get<PasswordSettings>();
        }
    }

    public class EmailVerificationCodeSettings
    {
        public int Length { get; set; }
        public int ValidMinutes { get; set; }

        public static EmailVerificationCodeSettings Get(IConfiguration configuration)
        {
            return configuration.GetSection("User")
                .GetSection("EmailVerificationCode")
                .Get<EmailVerificationCodeSettings>();
        }
    }

    public class ResetPasswordCodeSettings
    {
        public int Length { get; set; }
        public int ValidMinutes { get; set; }

        public static ResetPasswordCodeSettings Get(IConfiguration configuration)
        {
            return configuration.GetSection("User")
                .GetSection("ResetPasswordCode")
                .Get<ResetPasswordCodeSettings>();
        }
    }
}
