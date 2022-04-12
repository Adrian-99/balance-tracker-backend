using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("Users")]
    [Index(nameof(Username), IsUnique = true, Name = "Index_Username")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [Encrypted]
        public string Email { get => _email; set => _email = value.ToLower(); }

        [Required]
        public byte[] PasswordHash { get; set; }

        [Required]
        public byte[] PasswordSalt { get; set; }

        [Encrypted]
        public string? FirstName { get; set; }

        [Encrypted]
        public string? LastName { get; set; }

        public string? EmailVerificationCode { get; set; }

        public string? ResetPasswordCode { get; set; }


        private string _email;
    }
}
