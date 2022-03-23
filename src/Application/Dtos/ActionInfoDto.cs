using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class ActionInfoDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? TranslationKey { get; set; }
    }
}
