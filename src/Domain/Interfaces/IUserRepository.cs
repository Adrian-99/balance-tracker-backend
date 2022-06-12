using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IUserRepository : IGenericRepository<User, Guid>
    {
        Task<User?> GetByUsernameIgnoreCaseAsync(string username);
        Task<User?> GetByEmailIgnoreCaseAsync(string email);
        Task<User?> GetByResetPasswordCodeAsync(string resetPasswordCode);
    }
}
