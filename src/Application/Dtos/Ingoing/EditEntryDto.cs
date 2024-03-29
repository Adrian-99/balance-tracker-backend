﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Ingoing
{
    public class EditEntryDto
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
        public List<string> TagNames { get; }

        public EditEntryDto(DateTime date,
                            decimal value,
                            string name,
                            string? description,
                            string categoryKeyword,
                            List<string> tagNames)
        {
            Date = date;
            Value = value;
            Name = name;
            Description = description;
            CategoryKeyword = categoryKeyword;
            TagNames = tagNames;
        }
    }
}
