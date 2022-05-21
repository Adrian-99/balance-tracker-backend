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
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [Encrypted]
        public string Username { get; set; }

        [Required]
        public DateTime LastUsernameChangeAt { get; set; }

        [Required]
        [Encrypted]
        public string Email { get; set; }

        [Required]
        public bool IsEmailVerified { get; set; }

        [Required]
        public byte[] PasswordHash { get; set; }

        [Required]
        public byte[] PasswordSalt { get; set; }

        [Encrypted]
        public string? FirstName { get; set; }

        [Encrypted]
        public string? LastName { get; set; }

        [Encrypted]
        public string? EmailVerificationCode { get; set; }

        public DateTime? EmailVerificationCodeCreatedAt { get; set; }

        [Encrypted]
        public string? ResetPasswordCode { get; set; }

        public DateTime? ResetPasswordCodeCreatedAt { get; set; }
    }
}
