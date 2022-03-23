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
        Task<User?> GetByActivationCode(string activationCode);
        Task<User> Add(User user);
        Task<User> Update(User user);
        Task Delete(Guid id);
    }
}
