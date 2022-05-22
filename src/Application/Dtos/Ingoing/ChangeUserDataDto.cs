using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Ingoing
{
    public class ChangeUserDataDto
    {
        [Required]
        public string Username { get; }

        [Required]
        public string Email { get; }

        public string? FirstName { get; }

        public string? LastName { get; }

        public ChangeUserDataDto(string username, string email, string? firstName, string? lastName)
        {
            Username = username;
            Email = email;
            FirstName = firstName;
            LastName = lastName;
        }
    }
}
