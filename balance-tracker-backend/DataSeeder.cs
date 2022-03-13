using balance_tracker_backend.Models;

namespace balance_tracker_backend
{
    public static class DataSeeder
    {
        public static void Seed(this IHost host)
        {
            using var scope = host.Services.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            context.Database.EnsureCreated();
            AddUsers(context);
        }

        private static void AddUsers(DatabaseContext context)
        {
            var user = context.Users.FirstOrDefault();

            if (user != null) return;

            context.Users.Add(new User { Username = "User1", Email = "user1@gmail.com", PasswordHash = "somehash" });

            context.SaveChanges();
        }
    }
}
