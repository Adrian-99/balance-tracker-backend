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

                passwordService.CreatePasswordHash("User1", out passwordHash, out passwordSalt);

                databaseContext.Users.Add(new User { Username = "User1", Email = "user1@gmail.com", PasswordHash = passwordHash, PasswordSalt = passwordSalt });

                databaseContext.SaveChanges();
            }
        }
    }
}
