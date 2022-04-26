﻿using Application.Dtos.Ingoing;
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
        public User FromUserRegisterDtoToUser(UserRegisterDto userRegisterDto);
        public UserDataDto FromUserToUserDataDto(User user);
    }
}
