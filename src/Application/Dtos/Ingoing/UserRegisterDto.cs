using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.Ingoing
{
    public class UserRegisterDto
    {
        [Required]
        public string Username { get; }

        [Required]
        public string Email { get; }

        public string? FirstName { get; }

        public string? LastName { get; }

        [Required]
        public string Password { get; }

        public UserRegisterDto(string username, string email, string? firstName, string? lastName, string password)
        {
            Username = username;
            Email = email;
            FirstName = firstName;
            LastName = lastName;
            Password = password;
        }
    }
}
