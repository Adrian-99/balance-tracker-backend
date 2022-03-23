using Domain.Entities;

namespace Infrastructure.Data
{
    public static class DataSeeder
    {
        public static void Seed(DatabaseContext databaseContext)
        {
            databaseContext.Database.EnsureCreated();
            AddUsers(databaseContext);
        }

        private static void AddUsers(DatabaseContext context)
        {
            var user = context.Users.FirstOrDefault();

            if (user != null) return;

            //context.Users.Add(new User { Username = "User1", Email = "user1@gmail.com", PasswordHash = "somehash" });

            context.SaveChanges();
        }
    }
}
