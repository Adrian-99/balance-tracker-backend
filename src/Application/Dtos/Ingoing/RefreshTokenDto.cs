using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Ingoing
{
    public class RefreshTokenDto
    {
        [Required]
        public string RefreshToken { get; }

        public RefreshTokenDto(string refreshToken)
        {
            RefreshToken = refreshToken;
        }
    }
}
