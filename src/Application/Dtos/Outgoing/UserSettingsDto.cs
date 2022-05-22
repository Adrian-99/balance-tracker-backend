using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Outgoing
{
    public class UserSettingsDto
    {
        [Required]
        public int UsernameMaxLength { get; }

        [Required]
        public int UsernameAllowedChangeFrequencyDays { get; }

        [Required]
        public int FirstNameMaxLength { get; }

        [Required]
        public int LastNameMaxLength { get; }

        [Required]
        public int PasswordMinLength { get; }

        [Required]
        public int PasswordMaxLength { get; }

        [Required]
        public bool PasswordSmallLetterRequired { get; }

        [Required]
        public bool PasswordBigLetterRequired { get; }

        [Required]
        public bool PasswordDigitRequired { get; }

        [Required]
        public bool PasswordSpecialCharacterRequired { get; }

        [Required]
        public bool PasswordForbidSameAsUsername { get; }

        [Required]
        public bool PasswordForbidSameAsCurrent { get; }

        public UserSettingsDto(int usernameMaxLength,
                               int usernameAllowedChangeFrequencyDays,
                               int firstNameMaxLength,
                               int lastNameMaxLength,
                               int passwordMinLength,
                               int passwordMaxLength,
                               bool passwordSmallLetterRequired,
                               bool passwordBigLetterRequired,
                               bool passwordDigitRequired,
                               bool passwordSpecialCharacterRequired,
                               bool passwordForbidSameAsUsername,
                               bool passwordForbidSameAsCurrent)
        {
            UsernameMaxLength = usernameMaxLength;
            UsernameAllowedChangeFrequencyDays = usernameAllowedChangeFrequencyDays;
            FirstNameMaxLength = firstNameMaxLength;
            LastNameMaxLength = lastNameMaxLength;
            PasswordMinLength = passwordMinLength;
            PasswordMaxLength = passwordMaxLength;
            PasswordSmallLetterRequired = passwordSmallLetterRequired;
            PasswordBigLetterRequired = passwordBigLetterRequired;
            PasswordDigitRequired = passwordDigitRequired;
            PasswordSpecialCharacterRequired = passwordSpecialCharacterRequired;
            PasswordForbidSameAsUsername = passwordForbidSameAsUsername;
            PasswordForbidSameAsCurrent = passwordForbidSameAsCurrent;
        }
    }
}
