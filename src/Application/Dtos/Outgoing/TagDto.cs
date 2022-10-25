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
        public Guid Id { get; }

        [Required]
        public string Name { get; }

        public int? EntriesCount { get; }

        public TagDto(Guid id, string name, int? entriesCount)
        {
            Id = id;
            Name = name;
            EntriesCount = entriesCount;
        }
    }
}
