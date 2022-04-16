using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Ingoing
{
    public class ResetPasswordRequestDto
    {
        [Required]
        public string UsernameOrEmail { get; set; }
    }
}
