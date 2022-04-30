using Application.Dtos.Ingoing;
using Application.Dtos.Outgoing;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserMapper
    {
        User FromUserRegisterDtoToUser(UserRegisterDto userRegisterDto);
        UserDataDto FromUserToUserDataDto(User user);
    }
}
