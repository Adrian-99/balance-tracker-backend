﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Ingoing
{
    public class ResetPasswordDto
    {
        public string ResetPasswordCode { get; set; }
        public string NewPassword { get; set; }
    }
}
