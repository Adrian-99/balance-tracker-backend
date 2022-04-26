using Application.Dtos.Ingoing;
using Application.Dtos.Outgoing;
using Application.Interfaces;
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
            user.EmailVerificationCode = null;
            user.ResetPasswordCode = null;

            return user;
        }

        public UserDataDto FromUserToUserDataDto(User user)
        {
            var userDataDto = new UserDataDto();
            userDataDto.Username = user.Username;
            userDataDto.Email = user.Email;
            userDataDto.IsEmailVerified = user.EmailVerificationCode == null;
            userDataDto.FirstName = user.FirstName;
            userDataDto.LastName = user.LastName;

            return userDataDto;
        }
    }
}
