using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Ingoing
{
    public class VerifyEmailDto
    {
        [Required]
        public string EmailVerificationCode { get; }

        public VerifyEmailDto(string emailVerificationCode)
        {
            EmailVerificationCode = emailVerificationCode;
        }
    }
}
