using Application.Dtos.Ingoing;
using Application.Dtos.Outgoing;
using Application.Interfaces;
using Application.Settings;
using Domain.Entities;

namespace Application.Mappers
{
    public class UserMapper : IUserMapper
    {
        private IPasswordService passwordService;

        public UserMapper(IPasswordService passwordService)
        {
            this.passwordService = passwordService;
        }

        public User FromUserRegisterDtoToUser(UserRegisterDto userRegisterDto)
        {
            byte[] passwordSalt, passwordHash;
            passwordService.CreatePasswordHash(userRegisterDto.Password, out passwordHash, out passwordSalt);

            var user = new User();
            user.Username = userRegisterDto.Username;
            user.Email = userRegisterDto.Email;
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.FirstName = userRegisterDto.FirstName;
            user.LastName = userRegisterDto.LastName;

            return user;
        }

        public UserDataDto FromUserToUserDataDto(User user)
        {
            return new UserDataDto(user.Username,
                                   user.LastUsernameChangeAt,
                                   user.Email,
                                   user.IsEmailVerified,
                                   user.FirstName,
                                   user.LastName);
        }

        public UserSettingsDto FromUserSettingsToUserSettingsDto(UserSettings userSettings)
        {
            return new UserSettingsDto(userSettings.Username.MaxLength,
                                       userSettings.Username.AllowedChangeFrequencyDays,
                                       userSettings.FirstName.MaxLength,
                                       userSettings.LastName.MaxLength,
                                       userSettings.Password.MinLength,
                                       userSettings.Password.MaxLength,
                                       userSettings.Password.SmallLetterRequired,
                                       userSettings.Password.BigLetterRequired,
                                       userSettings.Password.DigitRequired,
                                       userSettings.Password.SpecialCharacterRequired,
                                       userSettings.Password.ForbidSameAsUsername,
                                       userSettings.Password.ForbidSameAsCurrent);
        }
    }
}
