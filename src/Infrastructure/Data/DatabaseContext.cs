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
        private IEncryptionProvider encryptionProvider;

        public DbSet<User> Users { get; set; }

        public DatabaseContext(DbContextOptions<DatabaseContext> options, IConfiguration configuration) :
            base(options)
        {
            var encryptionKey = Encoding.UTF8.GetBytes(configuration.GetSection("Encryption").GetSection("Key").Value);
            var encryptionIV = Encoding.UTF8.GetBytes(configuration.GetSection("Encryption").GetSection("IV").Value);

            encryptionProvider = new AesProvider(encryptionKey, encryptionIV);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseEncryption(encryptionProvider);
            modelBuilder.UseSerialColumns();
        }
    }
}
