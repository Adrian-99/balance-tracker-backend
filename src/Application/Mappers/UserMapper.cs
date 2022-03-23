using Application.Dtos;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Mappers
{
    public class UserMapper : IUserMapper
    {
        private IUserService userService;

        public UserMapper(IUserService userService)
        {
            this.userService = userService;
        }

        public User FromUserRegisterDtoToUser(UserRegisterDto userRegisterDto)
        {
            byte[] passwordSalt, passwordHash;
            userService.CreatePasswordHash(userRegisterDto.Password, out passwordHash, out passwordSalt);

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
    }
}
