using Application.Services;
using Domain.Entities;

namespace Infrastructure.Data
{
    public static class DataSeeder
    {
        public static void SeedAll(DatabaseContext databaseContext)
        {
            SeedUsers(databaseContext);
        }

        public static void SeedUsers(DatabaseContext databaseContext)
        {
            databaseContext.Database.EnsureCreated();
            if (databaseContext.Users.Count() == 0)
            {
                var passwordService = new PasswordService();

                byte[] passwordHash, passwordSalt;

                passwordService.CreatePasswordHash("User1!@#", out passwordHash, out passwordSalt);
                databaseContext.Users.Add(new User {
                    Username = "User1",
                    Email = "user1@gmail.com",
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt
                });

                passwordService.CreatePasswordHash("J@n_Kowal$ki123", out passwordHash, out passwordSalt);
                databaseContext.Users.Add(new User {
                    Username = "jan_kowalski",
                    Email = "jankowalski@gmail.com",
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
                    Email = "random@gmail.com",
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
