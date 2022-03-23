﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class ActionErrorDto
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public string? TranslationKey { get; set; }
    }
}
