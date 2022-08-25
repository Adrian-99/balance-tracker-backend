using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class TagDto
    {
        [Required]
        public string Name { get; }

        public TagDto(string name)
        {
            Name = name;
        }
    }
}
