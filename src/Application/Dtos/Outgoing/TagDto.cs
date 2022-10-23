using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Outgoing
{
    public class TagDto
    {
        [Required]
        public string Name { get; }

        public int? EntriesCount { get; }

        public TagDto(string name, int? entriesCount)
        {
            Name = name;
            EntriesCount = entriesCount;
        }
    }
}
