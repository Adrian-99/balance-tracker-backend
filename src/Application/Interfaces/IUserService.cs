using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserService
    {
        public Task ValidateUsernameAndEmail(string username, string email);
        public Task<User> Register(User user);
    }
}
