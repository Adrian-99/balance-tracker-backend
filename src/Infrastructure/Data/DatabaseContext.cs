using Application.Settings;
using Domain.Entities;
using Infrastructure.Data.Encryption;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DataEncryption;
using Microsoft.EntityFrameworkCore.DataEncryption.Providers;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data
{
    public class DatabaseContext : DbContext
    {
        //private static string CASE_INSENSITIVE_COLLATION = "case_insensitive_collation";

        private EncryptionSettings encryptionSettings;

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Entry> Entries { get; set; }
        public DbSet<EntryTag> EntryTags { get; set; }
        public DbSet<Tag> Tags { get; set; }

        public DatabaseContext(DbContextOptions<DatabaseContext> options, IConfiguration configuration) :
            base(options)
        {
            encryptionSettings = EncryptionSettings.Get(configuration);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var stringEncryptionConverter = new StringEncryptionConverter(encryptionSettings);
            var optionalStringEncryptionConverter = new OptionalStringEncryptionConverter(encryptionSettings);
            var dateTimeEndcryptionConverter = new DateTimeEncryptionConverter(encryptionSettings);
            var decimalEncryptionConverter = new DecimalEncryptionConverter(encryptionSettings);

            modelBuilder.UseSerialColumns();
            //modelBuilder.HasCollation(CASE_INSENSITIVE_COLLATION, locale: "en-u-ks-primary", provider: "icu", deterministic: false);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique(true);
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique(true);
            modelBuilder.Entity<User>()
                .HasIndex(u => u.EmailVerificationCode)
                .IsUnique(true);
            modelBuilder.Entity<User>()
                .HasIndex(u => u.ResetPasswordCode)
                .IsUnique(true);
            modelBuilder.Entity<User>()
                .Property(u => u.Username)
                .HasConversion(stringEncryptionConverter);
            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .HasConversion(stringEncryptionConverter);
            modelBuilder.Entity<User>()
                .Property(u => u.FirstName)
                .HasConversion(optionalStringEncryptionConverter);
            modelBuilder.Entity<User>()
                .Property(u => u.LastName)
                .HasConversion(optionalStringEncryptionConverter);
            modelBuilder.Entity<User>()
                .Property(u => u.EmailVerificationCode)
                .HasConversion(optionalStringEncryptionConverter);
            modelBuilder.Entity<User>()
                .Property(u => u.ResetPasswordCode)
                .HasConversion(optionalStringEncryptionConverter);

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Keyword)
                .IsUnique(true);
            modelBuilder.Entity<Category>()
                .HasIndex(c => c.OrderOnList)
                .IsUnique(true);

            modelBuilder.Entity<EntryTag>()
                .HasKey(et => new { et.TagId, et.EntryId })
                .HasName("PK_EntryTags");

            modelBuilder.Entity<Entry>()
                .HasOne(e => e.User)
                .WithMany(u => u.Entries);
            modelBuilder.Entity<Entry>()
                .HasOne(e => e.Category);
            modelBuilder.Entity<Entry>()
                .Property(e => e.Date)
                .HasConversion(dateTimeEndcryptionConverter);
            modelBuilder.Entity<Entry>()
                .Property(e => e.Value)
                .HasConversion(decimalEncryptionConverter);
            modelBuilder.Entity<Entry>()
                .Property(e => e.Name)
                .HasConversion(stringEncryptionConverter);
            modelBuilder.Entity<Entry>()
                .Property(e => e.DescriptionKey)
                .HasConversion(optionalStringEncryptionConverter);
            modelBuilder.Entity<Entry>()
                .Property(e => e.DescriptionIV)
                .HasConversion(optionalStringEncryptionConverter);

            modelBuilder.Entity<Tag>()
                .HasOne(t => t.User)
                .WithMany(u => u.Tags);
            modelBuilder.Entity<Tag>()
                .Property(t => t.Name)
                .HasConversion(stringEncryptionConverter);
            modelBuilder.Entity<Tag>()
                .HasIndex(t => new { t.UserId, t.Name })
                .IsUnique(true);

            modelBuilder.Entity<EntryTag>()
                .HasOne(et => et.Tag)
                .WithMany(t => t.EntryTags);
            modelBuilder.Entity<EntryTag>()
                .HasOne(et => et.Entry)
                .WithMany(e => e.EntryTags);
        }
    }
}
