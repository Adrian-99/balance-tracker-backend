using Application.Dtos.Outgoing;
using Application.Settings;

namespace Application.Mappers
{
    public static class ValidationRuleMapper
    {
        public static ValidationRulesDto FromValidationRulesSettingsToValidationRulesDto(ValidationRulesSettings validationRulesSettings)
        {
            return new ValidationRulesDto(validationRulesSettings.User.Username.MaxLength,
                                          validationRulesSettings.User.Username.AllowedChangeFrequencyDays,
                                          validationRulesSettings.User.FirstName.MaxLength,
                                          validationRulesSettings.User.LastName.MaxLength,
                                          validationRulesSettings.User.Password.MinLength,
                                          validationRulesSettings.User.Password.MaxLength,
                                          validationRulesSettings.User.Password.SmallLetterRequired,
                                          validationRulesSettings.User.Password.BigLetterRequired,
                                          validationRulesSettings.User.Password.DigitRequired,
                                          validationRulesSettings.User.Password.SpecialCharacterRequired,
                                          validationRulesSettings.User.Password.ForbidSameAsUsername,
                                          validationRulesSettings.User.Password.ForbidSameAsCurrent,
                                          validationRulesSettings.Entry.Name.MaxLength,
                                          validationRulesSettings.Entry.Description.MaxLength,
                                          validationRulesSettings.Tag.Name.MaxLength);
        }
    }
}
