using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Ingoing
{
    public class AuthenticateDto
    {
        [Required]
        public string UsernameOrEmail { get; }

        [Required]
        public string Password { get; }

        public AuthenticateDto(string usernameOrEmail, string password)
        {
            UsernameOrEmail = usernameOrEmail;
            Password = password;
        }
    }
}
