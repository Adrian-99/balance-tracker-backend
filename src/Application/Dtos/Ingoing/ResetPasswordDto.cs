using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Ingoing
{
    public class ResetPasswordDto
    {
        [Required]
        public string ResetPasswordCode { get; }

        [Required]
        public string NewPassword { get; }

        public ResetPasswordDto(string resetPasswordCode, string newPassword)
        {
            ResetPasswordCode = resetPasswordCode;
            NewPassword = newPassword;
        }
    }
}
