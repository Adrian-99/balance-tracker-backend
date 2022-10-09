using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Settings
{
    public class ValidationRulesSettings
    {
        public UserSettings User { get; set; }
        public EntrySettings Entry { get; set; }
        public TagSettings Tag { get; set; }

        public static ValidationRulesSettings Get(IConfiguration configuration)
        {
            var validationRulesSettings = new ValidationRulesSettings();

            validationRulesSettings.User = UserSettings.Get(configuration);
            validationRulesSettings.Entry = EntrySettings.Get(configuration);
            validationRulesSettings.Tag = TagSettings.Get(configuration);

            return validationRulesSettings;
        }
    }

    public class UserSettings
    {
        public UserUsernameSettings Username { get; set; }
        public UserFirstNameSettings FirstName { get; set; }
        public UserLastNameSettings LastName { get; set; }
        public UserPasswordSettings Password { get; set; }
        public UserEmailVerificationCodeSettings EmailVerificationCode { get; set; }
        public UserResetPasswordCodeSettings ResetPasswordCode { get; set; }

        public static UserSettings Get(IConfiguration configuration)
        {
            var userSettings = new UserSettings();

            userSettings.Username = UserUsernameSettings.Get(configuration);
            userSettings.FirstName = UserFirstNameSettings.Get(configuration);
            userSettings.LastName = UserLastNameSettings.Get(configuration);
            userSettings.Password = UserPasswordSettings.Get(configuration);
            userSettings.EmailVerificationCode = UserEmailVerificationCodeSettings.Get(configuration);
            userSettings.ResetPasswordCode = UserResetPasswordCodeSettings.Get(configuration);

            return userSettings;
        }
    }

    public class UserUsernameSettings
    {
        public int MaxLength { get; set; }
        public int AllowedChangeFrequencyDays { get; set; }

        public static UserUsernameSettings Get(IConfiguration configuration)
        {
            return configuration.GetSection("ValidationRules")
                .GetSection("User")
                .GetSection("Username")
                .Get<UserUsernameSettings>();
        }
    }

    public class UserFirstNameSettings
    {
        public int MaxLength { get; set; }

        public static UserFirstNameSettings Get(IConfiguration configuration)
        {
            return configuration.GetSection("ValidationRules")
                .GetSection("User")
                .GetSection("FirstName")
                .Get<UserFirstNameSettings>();
        }
    }

    public class UserLastNameSettings
    {
        public int MaxLength { get; set; }

        public static UserLastNameSettings Get(IConfiguration configuration)
        {
            return configuration.GetSection("ValidationRules")
                .GetSection("User")
                .GetSection("LastName")
                .Get<UserLastNameSettings>();
        }
    }

    public class UserPasswordSettings
    {
        public int MinLength { get; set; }
        public int MaxLength { get; set; }
        public bool SmallLetterRequired { get; set; }
        public bool BigLetterRequired { get; set; }
        public bool DigitRequired { get; set; }
        public bool SpecialCharacterRequired { get; set; }
        public bool ForbidSameAsUsername { get; set; }
        public bool ForbidSameAsCurrent { get; set; }

        public static UserPasswordSettings Get(IConfiguration configuration)
        {
            return configuration.GetSection("ValidationRules")
                .GetSection("User")
                .GetSection("Password")
                .Get<UserPasswordSettings>();
        }
    }

    public class UserEmailVerificationCodeSettings
    {
        public int Length { get; set; }
        public int ValidMinutes { get; set; }

        public static UserEmailVerificationCodeSettings Get(IConfiguration configuration)
        {
            return configuration.GetSection("ValidationRules")
                .GetSection("User")
                .GetSection("EmailVerificationCode")
                .Get<UserEmailVerificationCodeSettings>();
        }
    }

    public class UserResetPasswordCodeSettings
    {
        public int Length { get; set; }
        public int ValidMinutes { get; set; }

        public static UserResetPasswordCodeSettings Get(IConfiguration configuration)
        {
            return configuration.GetSection("ValidationRules")
                .GetSection("User")
                .GetSection("ResetPasswordCode")
                .Get<UserResetPasswordCodeSettings>();
        }
    }

    public class EntrySettings
    {
        public EntryNameSettings Name { get; set; }
        public EntryDescriptionSettings Description { get; set; }

        public static EntrySettings Get(IConfiguration configuration)
        {
            var entrySettings = new EntrySettings();

            entrySettings.Name = EntryNameSettings.Get(configuration);
            entrySettings.Description = EntryDescriptionSettings.Get(configuration);

            return entrySettings;
        }
    }

    public class EntryNameSettings
    {
        public int MaxLength { get; set; }

        public static EntryNameSettings Get(IConfiguration configuration)
        {
            return configuration.GetSection("ValidationRules")
                .GetSection("Entry")
                .GetSection("Name")
                .Get<EntryNameSettings>();
        }
    }

    public class EntryDescriptionSettings
    {
        public int MaxLength { get; set; }

        public static EntryDescriptionSettings Get(IConfiguration configuration)
        {
            return configuration.GetSection("ValidationRules")
                .GetSection("Entry")
                .GetSection("Description")
                .Get<EntryDescriptionSettings>();
        }
    }

    public class TagSettings
    {
        public TagNameSettings Name { get; set; }

        public static TagSettings Get(IConfiguration configuration)
        {
            var tagSettings = new TagSettings();

            tagSettings.Name = TagNameSettings.Get(configuration);

            return tagSettings;
        }
    }

    public class TagNameSettings
    {
        public int MaxLength { get; set; }

        public static TagNameSettings Get(IConfiguration configuration)
        {
            return configuration.GetSection("ValidationRules")
                .GetSection("Tag")
                .GetSection("Name")
                .Get<TagNameSettings>();
        }
    }
}
