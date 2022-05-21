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
        Task<ICollection<User>> GetAll();
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByUsernameIgnoreCaseAsync(string username);
        Task<User?> GetByEmailIgnoreCaseAsync(string email);
        Task<User?> GetByResetPasswordCodeAsync(string resetPasswordCode);
        Task<User> AddAsync(User user);
        Task<User> UpdateAsync(User user);
        Task DeleteAsync(Guid id);
    }
}
