using Application.Services;
using Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Data
{
    public static class DataSeeder
    {
        public static void SeedAll(IConfiguration configuration, DatabaseContext databaseContext)
        {
            SeedUsers(configuration, databaseContext);
        }

        public static void SeedUsers(IConfiguration configuration, DatabaseContext databaseContext)
        {
            databaseContext.Database.EnsureCreated();
            if (databaseContext.Users.Count() == 0)
            {
                var passwordService = new PasswordService(configuration);

                byte[] passwordHash, passwordSalt;

                passwordService.CreatePasswordHash("User1!@#", out passwordHash, out passwordSalt);
                databaseContext.Users.Add(new User {
                    Username = "User1",
                    LastUsernameChangeAt = DateTime.UtcNow,
                    Email = "user1@gmail.com",
                    IsEmailVerified = true,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt
                });

                passwordService.CreatePasswordHash("J@n_Kowal$ki123", out passwordHash, out passwordSalt);
                databaseContext.Users.Add(new User {
                    Username = "jan_kowalski",
                    LastUsernameChangeAt = DateTime.UtcNow.AddDays(-14),
                    Email = "jankowalski@gmail.com",
                    IsEmailVerified = true,
                    FirstName = "Jan",
                    LastName = "Kowalski",
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    ResetPasswordCode = "erHj7QDYqnCeKebWKc0noYSnNhqwoO",
                    ResetPasswordCodeCreatedAt = DateTime.UtcNow
                });

                passwordService.CreatePasswordHash("Qwerty1@", out passwordHash, out passwordSalt);
                databaseContext.Users.Add(new User
                {
                    Username = "randomUser",
                    LastUsernameChangeAt = DateTime.UtcNow,
                    Email = "random@gmail.com",
                    IsEmailVerified = false,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    EmailVerificationCode = "iwFCfh9skMQSCikWjHzLbAojbIG_NT",
                    EmailVerificationCodeCreatedAt = DateTime.UtcNow
                });

                databaseContext.SaveChanges();
            }
        }
    }
}
