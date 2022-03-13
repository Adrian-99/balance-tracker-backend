using System.ComponentModel.DataAnnotations;

namespace balance_tracker_backend.Models
{
    public class User
    {
        public long Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string Email { get; set; }
    }
}
