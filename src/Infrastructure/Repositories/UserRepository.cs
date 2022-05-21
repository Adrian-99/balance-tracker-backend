using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DatabaseContext databaseContext;

        public UserRepository(DatabaseContext databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        public async Task<ICollection<User>> GetAll()
        {
            return await databaseContext.Users.ToListAsync();
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await (from user in databaseContext.Users
                          where user.Id == id
                          select user).FirstOrDefaultAsync();
        }

        public async Task<User?> GetByUsernameIgnoreCaseAsync(string username)
        {
            var users = await GetAll();
            return users.Where(u => u.Username.ToLower().Equals(username.ToLower()))
                .FirstOrDefault();
        }

        public async Task<User?> GetByEmailIgnoreCaseAsync(string email)
        {
            var users = await GetAll();
            return users.Where(u => u.Email.ToLower().Equals(email.ToLower()))
                .FirstOrDefault();
        }

        public async Task<User?> GetByResetPasswordCodeAsync(string resetPasswordCode)
        {
            return await (from user in databaseContext.Users
                          where user.ResetPasswordCode == resetPasswordCode
                          select user).FirstOrDefaultAsync();
        }

        public async Task<User> AddAsync(User user)
        {
            var dbUser = await databaseContext.Users.AddAsync(user);
            await databaseContext.SaveChangesAsync();
            return dbUser.Entity;
        }

        public async Task<User> UpdateAsync(User user)
        {
            var dbUser = databaseContext.Users.Update(user);
            await databaseContext.SaveChangesAsync();
            return dbUser.Entity;
        }

        public async Task DeleteAsync(Guid id)
        {
            var user = await GetByIdAsync(id);
            databaseContext.Users.Remove(user);
            await databaseContext.SaveChangesAsync();
        }
    }
}
