using Application;
using Application.Services;
using Application.Utilities;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading;

namespace APITest.Tests
{
    public static class TestDataSeeder
    {
        public static void SeedAll(CategoriesLoader categoriesLoader,
                                   IConfiguration configuration,
                                   DatabaseContext databaseContext)
        {
            SeedUsers(configuration, databaseContext);
            SeedTags(databaseContext);
            SeedEntries(categoriesLoader, databaseContext);
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

        private static void SeedTags(DatabaseContext databaseContext)
        {
            if (databaseContext.Tags.Count() == 0)
            {
                var user1 = databaseContext.Users.Where(u => u.IsEmailVerified).First();
                var user2 = databaseContext.Users.Where(u => !u.IsEmailVerified).First();

                databaseContext.Tags.Add(new Tag
                {
                    Name = "tag1",
                    UserId = user1.Id
                });
                databaseContext.Tags.Add(new Tag
                {
                    Name = "secondTag",
                    UserId = user1.Id
                });
                databaseContext.Tags.Add(new Tag
                {
                    Name = "Tag number 3",
                    UserId = user1.Id
                });

                databaseContext.Tags.Add(new Tag
                {
                    Name = "tag of another user",
                    UserId = user2.Id
                });

                databaseContext.SaveChanges();
            }
        }

        private static void SeedEntries(CategoriesLoader categoriesLoader, DatabaseContext databaseContext)
        {
            if (databaseContext.Entries.Count() == 0)
            {
                categoriesLoader.WaitForLoadingToFinish();

                var categories = databaseContext.Categories.ToList();
                var user1 = databaseContext.Users.Where(u => u.IsEmailVerified).First();
                var user1Tags = databaseContext.Tags.Where(t => t.UserId.Equals(user1.Id)).ToList();
                var user2 = databaseContext.Users.Where(u => !u.IsEmailVerified).First();
                var user2Tags = databaseContext.Tags.Where(t => t.UserId.Equals(user2.Id)).ToList();

                string? encryptionKey = null, encryptionIV = null, encryptedDescription = null;

                encryptedDescription = EncryptionUtils.EncryptWithAES("Monthly bills", out encryptionKey, out encryptionIV);
                var entry1 = databaseContext.Entries.Add(new Entry
                {
                    Date = new DateTime(2022, 5, 12, 17, 21, 21, DateTimeKind.Utc),
                    Value = 17.65M,
                    Name = "Bills",
                    DescriptionContent = encryptedDescription,
                    DescriptionKey = encryptionKey,
                    DescriptionIV = encryptionIV,
                    UserId = user1.Id,
                    CategoryId = categories.ElementAt(4).Id
                });
                databaseContext.EntryTags.Add(new EntryTag
                {
                    TagId = user1Tags.ElementAt(0).Id,
                    EntryId = entry1.Entity.Id
                });
                databaseContext.EntryTags.Add(new EntryTag
                {
                    TagId = user1Tags.ElementAt(2).Id,
                    EntryId = entry1.Entity.Id
                });

                encryptedDescription = EncryptionUtils.EncryptWithAES("Product 1 has been bought", out encryptionKey, out encryptionIV);
                var entry2 = databaseContext.Entries.Add(new Entry
                {
                    Date = new DateTime(2022, 6, 2, 20, 15, 24, DateTimeKind.Utc),
                    Value = 60.45M,
                    Name = "Product 1",
                    DescriptionContent = encryptedDescription,
                    DescriptionKey = encryptionKey,
                    DescriptionIV = encryptionIV,
                    UserId = user1.Id,
                    CategoryId = categories.ElementAt(5).Id
                });
                databaseContext.EntryTags.Add(new EntryTag
                {
                    TagId = user1Tags.ElementAt(0).Id,
                    EntryId = entry2.Entity.Id
                });

                encryptedDescription = EncryptionUtils.EncryptWithAES("Salary for producing in 06/22", out encryptionKey, out encryptionIV);
                databaseContext.Entries.Add(new Entry
                {
                    Date = new DateTime(2022, 6, 10, 12, 0, 5, DateTimeKind.Utc),
                    Value = 3200.0M,
                    Name = "Salary",
                    DescriptionContent = encryptedDescription,
                    DescriptionKey = encryptionKey,
                    DescriptionIV = encryptionIV,
                    UserId = user1.Id,
                    CategoryId = categories.ElementAt(0).Id
                });

                var entry4 = databaseContext.Entries.Add(new Entry
                {
                    Date = new DateTime(2022, 6, 25, 6, 34, 54, DateTimeKind.Utc),
                    Value = 2.5M,
                    Name = "bread rolls",
                    UserId = user1.Id,
                    CategoryId = categories.ElementAt(4).Id
                });
                databaseContext.EntryTags.Add(new EntryTag
                {
                    TagId = user1Tags.ElementAt(1).Id,
                    EntryId = entry4.Entity.Id
                });

                encryptedDescription = EncryptionUtils.EncryptWithAES("Yay!", out encryptionKey, out encryptionIV);
                var entry5 = databaseContext.Entries.Add(new Entry
                {
                    Date = new DateTime(2022, 6, 12, 14, 5, 21, DateTimeKind.Utc),
                    Value = 100.0M,
                    Name = "found some money",
                    DescriptionContent = encryptedDescription,
                    DescriptionKey = encryptionKey,
                    DescriptionIV = encryptionIV,
                    UserId = user1.Id,
                    CategoryId = categories.ElementAt(2).Id
                });
                databaseContext.EntryTags.Add(new EntryTag
                {
                    TagId = user1Tags.ElementAt(2).Id,
                    EntryId = entry5.Entity.Id
                });

                encryptedDescription = EncryptionUtils.EncryptWithAES("Previous one broken down, bought again", out encryptionKey, out encryptionIV);
                var entry6 = databaseContext.Entries.Add(new Entry
                {
                    Date = new DateTime(2022, 7, 2, 15, 4, 3, DateTimeKind.Utc),
                    Value = 60.45M,
                    Name = "Product 1",
                    DescriptionContent = encryptedDescription,
                    DescriptionKey = encryptionKey,
                    DescriptionIV = encryptionIV,
                    UserId = user1.Id,
                    CategoryId = categories.ElementAt(5).Id
                });
                databaseContext.EntryTags.Add(new EntryTag
                {
                    TagId = user1Tags.ElementAt(0).Id,
                    EntryId = entry6.Entity.Id
                });

                var anotherUserEntry = databaseContext.Entries.Add(new Entry
                {
                    Date = new DateTime(2022, 6, 16, 17, 7, 43, DateTimeKind.Utc),
                    Value = 15.15M,
                    Name = "Food",
                    UserId = user2.Id,
                    CategoryId = categories.ElementAt(4).Id
                });
                databaseContext.EntryTags.Add(new EntryTag
                {
                    TagId = user2Tags.ElementAt(0).Id,
                    EntryId = anotherUserEntry.Entity.Id
                });

                databaseContext.SaveChanges();
            }
        }
    }
}
