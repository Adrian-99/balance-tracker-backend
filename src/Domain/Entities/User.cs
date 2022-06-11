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
        public string Username { get; set; }

        [Required]
        public DateTime LastUsernameChangeAt { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public bool IsEmailVerified { get; set; }

        [Required]
        public byte[] PasswordHash { get; set; }

        [Required]
        public byte[] PasswordSalt { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? EmailVerificationCode { get; set; }

        public DateTime? EmailVerificationCodeCreatedAt { get; set; }

        public string? ResetPasswordCode { get; set; }

        public DateTime? ResetPasswordCodeCreatedAt { get; set; }


        public virtual ICollection<Tag> Tags { get; set; }
        public virtual ICollection<Entry> Entries { get; set; }
    }
}
