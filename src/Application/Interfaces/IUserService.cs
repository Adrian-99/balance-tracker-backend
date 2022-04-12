﻿using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserService
    {
        Task ValidateUsernameAndEmail(string username, string email);
        Task<User> Register(User user);
        Task<bool> VerifyEmail(string username, string emailVerificationCode);
        Task<User?> Authenticate(string usernameOrEmail, string password);
        Task<User?> GetUserByUsernameIgnoreCaseAsync(string username);
    }
}
