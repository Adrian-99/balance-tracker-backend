﻿using Application.Settings;
using Domain.Entities;
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

        private IEncryptionProvider encryptionProvider;

        public DbSet<User> Users { get; set; }

        public DatabaseContext(DbContextOptions<DatabaseContext> options, IConfiguration configuration) :
            base(options)
        {
            var encryptionSettings = EncryptionSettings.Get(configuration);

            encryptionProvider = new AesProvider(encryptionSettings.KeyBytes, encryptionSettings.IVBytes);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseEncryption(encryptionProvider);
            modelBuilder.UseSerialColumns();
            //modelBuilder.HasCollation(CASE_INSENSITIVE_COLLATION, locale: "en-u-ks-primary", provider: "icu", deterministic: false);

            modelBuilder.Entity<User>().HasIndex(u => new { u.Username, u.Email, u.EmailVerificationCode, u.ResetPasswordCode }).IsUnique(true);
        }
    }
}
