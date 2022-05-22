using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Outgoing
{
    public class UserDataDto
    {
        [Required]
        public string Username { get; }

        [Required]
        public DateTime LastUsernameChangeAt { get; }

        [Required]
        public string Email { get; }

        [Required]
        public bool IsEmailVerified { get; }

        public string? FirstName { get; }

        public string? LastName { get; }

        public UserDataDto(string username,
                           DateTime lastUsernameChangeAt,
                           string email,
                           bool isEmailVerified,
                           string? firstName,
                           string? lastName)
        {
            Username = username;
            LastUsernameChangeAt = lastUsernameChangeAt;
            Email = email;
            IsEmailVerified = isEmailVerified;
            FirstName = firstName;
            LastName = lastName;
        }
    }
}
