using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.Ingoing
{
    public class UserRegisterDto
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Email { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
