using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Outgoing
{
    public class UserDataDto
    {
        public string Username { get; set; }
        public DateTime LastUsernameChangeAt { get; set; }
        public string Email { get; set; }
        public bool IsEmailVerified { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
