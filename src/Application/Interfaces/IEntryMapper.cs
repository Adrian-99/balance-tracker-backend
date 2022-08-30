using Application.Dtos.Ingoing;
using Application.Dtos.Outgoing;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IEntryMapper
    {
        EntryDto FromEntryToEntryDto(Entry entry);
        Task<Entry> FromEditEntryDtoToEntryAsync(EditEntryDto editEntryDto, Guid userId);
    }
}
