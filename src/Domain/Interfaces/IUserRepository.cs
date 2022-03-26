using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetById(Guid id);
        Task<User?> GetByUsernameIgnoreCase(string username);
        Task<User?> GetByEmailVerificationCode(string emailVerificationCode);
        Task<User> Add(User user);
        Task<User> Update(User user);
        Task Delete(Guid id);
    }
}
