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

        public async Task<User?> GetById(Guid id)
        {
            return await (from user in databaseContext.Users
                          where user.Id == id
                          select user).FirstOrDefaultAsync();
        }

        public async Task<User?> GetByUsernameIgnoreCase(string username)
        {
            return await (from user in databaseContext.Users
                          where user.Username.ToLower() == username.ToLower()
                          select user).FirstOrDefaultAsync();
        }

        public async Task<User?> GetByEmailVerificationCode(string emailVerificationCode)
        {
            return await (from user in databaseContext.Users
                          where user.EmailVerificationCode == emailVerificationCode
                          select user).FirstOrDefaultAsync();
        }

        public async Task<User> Add(User user)
        {
            var dbUser = await databaseContext.Users.AddAsync(user);
            await databaseContext.SaveChangesAsync();
            return dbUser.Entity;
        }

        public async Task<User> Update(User user)
        {
            var dbUser = databaseContext.Users.Update(user);
            await databaseContext.SaveChangesAsync();
            return dbUser.Entity;
        }

        public async Task Delete(Guid id)
        {
            var user = await GetById(id);
            databaseContext.Users.Remove(user);
            await databaseContext.SaveChangesAsync();
        }
    }
}
