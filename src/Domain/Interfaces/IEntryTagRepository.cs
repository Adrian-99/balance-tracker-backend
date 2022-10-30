using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IEntryTagRepository
    {
        Task<bool> CheckIfExistsAsync(Guid entryId, Guid tagId);
        Task<List<EntryTag>> GetAllByEntryIdAsync(Guid entryId);
        Task<List<EntryTag>> GetAllByTagIdAsync(Guid tagId);
        Task<EntryTag> AddAsync(EntryTag entryTag);
        Task DeleteAsync(params EntryTag[] entryTags);
        Task DeleteAllByEntryIdAsync(Guid entryId);
    }
}
