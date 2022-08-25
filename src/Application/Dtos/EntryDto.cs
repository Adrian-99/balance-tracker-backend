using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class EntryDto
    {
        [Required]
        public DateTime Date { get; }

        [Required]
        public decimal Value { get; }

        [Required]
        public string Name { get; }

        public string? Description { get; }

        [Required]
        public string CategoryKeyword { get; }

        [Required]
        public List<TagDto> Tags { get; }

        public EntryDto(DateTime date, decimal value, string name, string? description, string categoryKeyword, List<TagDto> tags)
        {
            Date = date;
            Value = value;
            Name = name;
            Description = description;
            CategoryKeyword = categoryKeyword;
            Tags = tags;
        }
    }
}
