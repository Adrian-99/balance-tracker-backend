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
    public class UserRepository : GenericRepository<User, Guid>, IUserRepository
    {

        public UserRepository(DatabaseContext databaseContext) : base(databaseContext)
        { }

        public async Task<User?> GetByUsernameIgnoreCaseAsync(string username)
        {
            var users = await GetAllAsync();
            return users.Where(u => u.Username.ToLower().Equals(username.ToLower()))
                .FirstOrDefault();
        }

        public async Task<User?> GetByEmailIgnoreCaseAsync(string email)
        {
            var users = await GetAllAsync();
            return users.Where(u => u.Email.ToLower().Equals(email.ToLower()))
                .FirstOrDefault();
        }

        public async Task<User?> GetByResetPasswordCodeAsync(string resetPasswordCode)
        {
            var users = await GetAllAsync();
            return users.Where(u => u.ResetPasswordCode != null && u.ResetPasswordCode.Equals(resetPasswordCode))
                .FirstOrDefault();
        }

        public override Task<User> UpdateAsync(User oldEntity, User newEntity)
        {
            oldEntity.Username = newEntity.Username;
            oldEntity.LastUsernameChangeAt = newEntity.LastUsernameChangeAt;
            oldEntity.Email = newEntity.Email;
            oldEntity.IsEmailVerified = newEntity.IsEmailVerified;
            oldEntity.PasswordHash = newEntity.PasswordHash;
            oldEntity.PasswordSalt = newEntity.PasswordSalt;
            oldEntity.FirstName = newEntity.FirstName;
            oldEntity.LastName = newEntity.LastName;
            oldEntity.EmailVerificationCode = newEntity.EmailVerificationCode;
            oldEntity.EmailVerificationCodeCreatedAt = newEntity.EmailVerificationCodeCreatedAt;
            oldEntity.ResetPasswordCode = newEntity.ResetPasswordCode;
            oldEntity.ResetPasswordCodeCreatedAt = newEntity.ResetPasswordCodeCreatedAt;
            return UpdateAsync(oldEntity);
        }
    }
}
