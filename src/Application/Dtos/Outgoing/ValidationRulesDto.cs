using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Outgoing
{
    public class ValidationRulesDto
    {
        [Required]
        public int UserUsernameMaxLength { get; }

        [Required]
        public int UserUsernameAllowedChangeFrequencyDays { get; }

        [Required]
        public int UserFirstNameMaxLength { get; }

        [Required]
        public int UserLastNameMaxLength { get; }

        [Required]
        public int UserPasswordMinLength { get; }

        [Required]
        public int UserPasswordMaxLength { get; }

        [Required]
        public bool UserPasswordSmallLetterRequired { get; }

        [Required]
        public bool UserPasswordBigLetterRequired { get; }

        [Required]
        public bool UserPasswordDigitRequired { get; }

        [Required]
        public bool UserPasswordSpecialCharacterRequired { get; }

        [Required]
        public bool UserPasswordForbidSameAsUsername { get; }

        [Required]
        public bool UserPasswordForbidSameAsCurrent { get; }

        [Required]
        public int EntryNameMaxLength { get; }

        [Required]
        public int EntryDescriptionMaxLength { get; }

        [Required]
        public int TagNameMaxLength { get; }

        public ValidationRulesDto(int userUsernameMaxLength,
                                  int userUsernameAllowedChangeFrequencyDays,
                                  int userFirstNameMaxLength,
                                  int userLastNameMaxLength,
                                  int userPasswordMinLength,
                                  int userPasswordMaxLength,
                                  bool userPasswordSmallLetterRequired,
                                  bool userPasswordBigLetterRequired,
                                  bool userPasswordDigitRequired,
                                  bool userPasswordSpecialCharacterRequired,
                                  bool userPasswordForbidSameAsUsername,
                                  bool userPasswordForbidSameAsCurrent,
                                  int entryNameMaxLength,
                                  int entryDescriptionMaxLength,
                                  int tagNameMaxLength)
        {
            UserUsernameMaxLength = userUsernameMaxLength;
            UserUsernameAllowedChangeFrequencyDays = userUsernameAllowedChangeFrequencyDays;
            UserFirstNameMaxLength = userFirstNameMaxLength;
            UserLastNameMaxLength = userLastNameMaxLength;
            UserPasswordMinLength = userPasswordMinLength;
            UserPasswordMaxLength = userPasswordMaxLength;
            UserPasswordSmallLetterRequired = userPasswordSmallLetterRequired;
            UserPasswordBigLetterRequired = userPasswordBigLetterRequired;
            UserPasswordDigitRequired = userPasswordDigitRequired;
            UserPasswordSpecialCharacterRequired = userPasswordSpecialCharacterRequired;
            UserPasswordForbidSameAsUsername = userPasswordForbidSameAsUsername;
            UserPasswordForbidSameAsCurrent = userPasswordForbidSameAsCurrent;
            EntryNameMaxLength = entryNameMaxLength;
            EntryDescriptionMaxLength = entryDescriptionMaxLength;
            TagNameMaxLength = tagNameMaxLength;
        }
    }
}
