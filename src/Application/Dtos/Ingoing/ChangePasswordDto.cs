﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Ingoing
{
    public class ChangePasswordDto
    {
        [Required]
        public string NewPassword { get; set; }
    }
}