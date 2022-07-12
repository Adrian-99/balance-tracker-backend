using Application.Dtos.Outgoing;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mappers
{
    public class EntryMapper
    {
        public static EntryDto FromEntryToEntryDto(Entry entry)
        {
            return new EntryDto(
                entry.Date,
                entry.Value,
                entry.Name,
                entry.Description,
                entry.Category.Keyword,
                entry.EntryTags.Select(et => et.Tag.Name).ToList()
                );
        }
    }
}
